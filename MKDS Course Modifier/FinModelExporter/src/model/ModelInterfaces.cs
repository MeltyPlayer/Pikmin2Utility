﻿using System.Collections.Generic;
using System.Numerics;

using MathNet.Numerics.LinearAlgebra;

namespace fin.model {
  public interface IModel {
    ISkeleton Skeleton { get; }
    ISkin Skin { get; }
    IMaterialManager MaterialManager { get; }
    IAnimationManager AnimationManager { get; }
  }

  public interface ISkeleton {
    IBone Root { get; }
  }

  public interface IBone {
    string Name { get; set; }

    IBone? Parent { get; }
    IReadOnlyList<IBone> Children { get; }
    IBone AddChild(float x, float y, float z);

    IPosition LocalPosition { get; }
    IRotation? LocalRotation { get; }
    IScale? LocalScale { get; }

    IBone SetLocalPosition(float x, float y, float z);
    IBone SetLocalRotationDegrees(float x, float y, float z);
    IBone SetLocalRotationRadians(float x, float y, float z);
    IBone SetLocalScale(float x, float y, float z);
  }


  public interface ISkin {
    IReadOnlyList<IVertex> Vertices { get; }
    IVertex AddVertex(float x, float y, float z);

    IReadOnlyList<IPrimitive> Primitives { get; }
    
    IPrimitive AddTriangles(params (IVertex, IVertex, IVertex)[] triangles);
    IPrimitive AddTriangles(params IVertex[] vertices);

    IPrimitive AddTriangleStrip(params IVertex[] vertices);

    IPrimitive AddQuads(params (IVertex, IVertex, IVertex, IVertex)[] quads);
    IPrimitive AddQuads(params IVertex[] vertices);
  }

  public record BoneWeight(
      IBone Bone,
      Matrix<double> SkinToBone,
      float Weight);

  public interface IVertex {
    IReadOnlyList<BoneWeight>? Weights { get; }
    IVertex SetBone(IBone bone);
    IVertex SetBones(params BoneWeight[] weights);

    IPosition LocalPosition { get; }
    IVertex SetLocalPosition(float x, float y, float z);

    INormal? LocalNormal { get; }
    IVertex SetLocalNormal(float x, float y, float z);
    // TODO: Setting colors.
    // TODO: Setting multiple texture UVs.
  }

  public enum PrimitiveType {
    TRIANGLES,
    TRIANGLE_STRIP,
    QUADS,
    // TODO: Other types.
  }

  public interface IPrimitive {
    PrimitiveType Type { get; }
    IReadOnlyList<IVertex> Vertices { get; }

    IPrimitive SetMaterial(IMaterial material);
  }


  public interface IMaterialManager {
    IReadOnlyList<IMaterial> All { get; }
    IMaterial AddMaterial();
  }

  public interface IMaterial {
    string Name { get; }

    // TODO: Setting texture layer(s).
    // TODO: Setting logic for combining texture layers.
  }
}