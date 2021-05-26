﻿using System.Collections.Generic;
using System.IO;
using System.Numerics;

using fin.model;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace fin.math {
  public interface IBoneTransformManager {
    void CalculateMatrices(
        IBone rootBone,
        (IAnimation, float)? animationAndFrame);

    void ProjectVertex(
        IVertex vertex,
        IPosition outPosition,
        INormal? outNormal = null);
  }

  public class BoneTransformManager : IBoneTransformManager {
    private readonly SoftwareModelViewMatrixTransformer transformer_ =
        new();

    // TODO: This is going to be slow, can we put this somewhere else for O(1) access?
    private readonly IDictionary<IBone, Matrix<double>> bonesToMatrices_ =
        new Dictionary<IBone, Matrix<double>>();

    public void CalculateMatrices(
        IBone rootBone,
        (IAnimation, float)? animationAndFrame) {
      var animation = animationAndFrame?.Item1;
      var frame = animationAndFrame?.Item2;

      this.transformer_.Push();
      this.transformer_.Identity();

      // TODO: Use a pool of matrices to prevent unneeded instantiations.
      var rootMatrix = new DenseMatrix(4, 4);
      this.transformer_.Get(rootMatrix);

      var boneQueue = new Queue<(IBone, Matrix<double>)>();
      boneQueue.Enqueue((rootBone, rootMatrix));
      while (boneQueue.Count > 0) {
        var (bone, matrix) = boneQueue.Dequeue();

        this.transformer_.Set(matrix);

        var boneTracks = animation?.BoneTracks[bone];

        var localPosition = boneTracks?.Positions?.GetInterpolatedAtFrame(0) ??
                            bone.LocalPosition;
        this.transformer_.Translate(localPosition.X,
                                    localPosition.Y,
                                    localPosition.Z);

        var localRotation = boneTracks?.Rotations?.GetInterpolatedAtFrame(0) ??
                            (bone.LocalRotation != null
                                 ? QuaternionUtil.Create(bone.LocalRotation)
                                 : null);
        if (localRotation != null) {
          this.transformer_.Rotate(localRotation.Value);
        }

        var localScale = boneTracks?.Scales?.GetInterpolatedAtFrame(0) ??
                         bone.LocalScale;
        if (localScale != null) {
          this.transformer_.Scale(localScale.X,
                                  localScale.Y,
                                  localScale.Z);
        }

        this.transformer_.Get(matrix);

        this.bonesToMatrices_[bone] = matrix;

        foreach (var child in bone.Children) {
          // TODO: Use a pool of matrices to prevent unneeded instantiations.
          boneQueue.Enqueue((child, matrix.Clone()));
        }
      }

      this.transformer_.Pop();
    }

    public void ProjectVertex(
        IVertex vertex,
        IPosition outPosition,
        INormal? outNormal = null) {
      // TODO: Precompute these in a shared way somehow.
      var mergedMatrix = new DenseMatrix(4, 4);
      foreach (var weight in vertex.Weights) {
        var skinToBoneMatrix = weight.SkinToBone;
        var boneMatrix = this.bonesToMatrices_[weight.Bone];

        var skinToWorldMatrix =
            boneMatrix * skinToBoneMatrix * weight.Weight;

        for (var j = 0; j < 4; ++j) {
          for (var k = 0; k < 4; ++k) {
            mergedMatrix[j, k] += skinToWorldMatrix[j, k];
          }
        }
      }

      this.transformer_.Push();
      this.transformer_.Set(mergedMatrix);

      var localPosition = vertex.LocalPosition;
      double x = localPosition.X;
      double y = localPosition.Y;
      double z = localPosition.Z;
      this.transformer_.ProjectVertex(ref x, ref y, ref z);
      outPosition.X = (float) x;
      outPosition.Y = (float) y;
      outPosition.Z = (float) z;

      var localNormal = vertex.LocalNormal;
      if (outNormal != null && localNormal != null) {
        double nX = localNormal.X;
        double nY = localNormal.Y;
        double nZ = localNormal.Z;
        this.transformer_.ProjectNormal(ref nX, ref nY, ref nZ);
        outNormal.X = (float) nX;
        outNormal.Y = (float) nY;
        outNormal.Z = (float) nZ;
      }

      this.transformer_.Pop();
    }
  }
}