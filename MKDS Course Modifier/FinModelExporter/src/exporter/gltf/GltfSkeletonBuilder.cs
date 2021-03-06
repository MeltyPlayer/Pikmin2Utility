﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.math;
using fin.math.matrix;
using fin.model;

using SharpGLTF.Schema2;

namespace fin.exporter.gltf {
  using GltfNode = Node;
  using GltfSkin = Skin;

  public class GltfSkeletonBuilder {
    public (GltfNode, IBone)[] BuildAndBindSkeleton(
        GltfNode rootNode,
        GltfSkin skin,
        ISkeleton skeleton) {
      var rootBone = skeleton.Root;

      var boneQueue = new Queue<(GltfNode, IBone)>();
      boneQueue.Enqueue((rootNode, rootBone));

      var skinNodesAndBones = new List<(GltfNode, IBone)>();
      while (boneQueue.Count > 0) {
        var (node, bone) = boneQueue.Dequeue();

        this.ApplyBoneOrientationToNode_(node, bone);

        if (bone != rootBone) {
          skinNodesAndBones.Add((node, bone));
        }

        foreach (var child in bone.Children) {
          boneQueue.Enqueue((node.CreateNode(child.Name), child));
        }
      }

      skin.BindJoints(skinNodesAndBones
                      .Select(skinNodesAndBone => skinNodesAndBone.Item1)
                      .ToArray());

      return skinNodesAndBones.ToArray();
    }

    private void ApplyBoneOrientationToNode_(GltfNode node, IBone bone) {
      var systemMatrix = node.LocalMatrix;

      MatrixConversionUtil.CopyFinIntoSystem(
          MatrixTransformUtil.FromTrs(bone.LocalPosition,
                                      bone.LocalRotation,
                                      bone.LocalScale),
          ref systemMatrix);

      node.LocalMatrix = systemMatrix;
    }
  }
}