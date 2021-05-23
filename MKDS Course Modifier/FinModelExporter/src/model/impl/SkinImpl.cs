﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace fin.model.impl {
  public partial class ModelImpl {
    public ISkin Skin { get; } = new SkinImpl();

    private class SkinImpl : ISkin {
      private readonly IList<IVertex> vertices_ = new List<IVertex>();
      private readonly IList<IPrimitive> primitives_ = new List<IPrimitive>();

      public SkinImpl() {
        this.Vertices = new ReadOnlyCollection<IVertex>(this.vertices_);
        this.Primitives = new ReadOnlyCollection<IPrimitive>(this.primitives_);
      }

      public IReadOnlyList<IVertex> Vertices { get; }

      public IVertex AddVertex(float x, float y, float z) {
        var vertex = new VertexImpl(x, y, z);
        this.vertices_.Add(vertex);
        return vertex;
      }

      public IReadOnlyList<IPrimitive> Primitives { get; }

      public IPrimitive AddTriangles(
          params (IVertex, IVertex, IVertex)[] triangles) {
        var vertices = new IVertex[3 * triangles.Length];
        for (var i = 0; i < triangles.Length; ++i) {
          var triangle = triangles[i];
          vertices[3 * i] = triangle.Item1;
          vertices[3 * i + 1] = triangle.Item2;
          vertices[3 * i + 2] = triangle.Item3;
        }
        return this.AddTriangles(vertices);
      }

      public IPrimitive AddTriangles(params IVertex[] vertices) {
        Debug.Assert(vertices.Length % 3 == 0);
        var primitive = new PrimitiveImpl(PrimitiveType.TRIANGLES, vertices);
        this.primitives_.Add(primitive);
        return primitive;
      }

      public IPrimitive AddTriangleStrip(params IVertex[] vertices) {
        var primitive =
            new PrimitiveImpl(PrimitiveType.TRIANGLE_STRIP, vertices);
        this.primitives_.Add(primitive);
        return primitive;
      }

      public IPrimitive AddQuads(
          params (IVertex, IVertex, IVertex, IVertex)[] quads) {
        var vertices = new IVertex[4 * quads.Length];
        for (var i = 0; i < quads.Length; ++i) {
          var quad = quads[i];
          vertices[4 * i] = quad.Item1;
          vertices[4 * i + 1] = quad.Item2;
          vertices[4 * i + 2] = quad.Item3;
          vertices[4 * i + 3] = quad.Item4;
        }
        return this.AddQuads(vertices);
      }

      public IPrimitive AddQuads(params IVertex[] vertices) {
        Debug.Assert(vertices.Length % 4 == 0);
        var primitive = new PrimitiveImpl(PrimitiveType.QUADS, vertices);
        this.primitives_.Add(primitive);
        return primitive;
      }

      private class VertexImpl : IVertex {
        public VertexImpl(float x, float y, float z)
          => this.SetGlobalPosition(x, y, z);


        public IReadOnlyList<(IBone, float)>? Weights { get; private set; }

        public IVertex SetBone(IBone bone) => this.SetBones((bone, 1));

        public IVertex SetBones(params (IBone, float)[] weights) {
          this.Weights = new ReadOnlyCollection<(IBone, float)>(weights);
          return this;
        }


        public IPosition GlobalPosition { get; } = new PositionImpl();

        public IVertex SetGlobalPosition(float x, float y, float z) {
          this.GlobalPosition.X = x;
          this.GlobalPosition.Y = y;
          this.GlobalPosition.Z = z;
          return this;
        }


        public INormal? GlobalNormal { get; private set; }

        public IVertex SetGlobalNormal(float x, float y, float z) {
          this.GlobalNormal ??= new NormalImpl();
          this.GlobalNormal.X = x;
          this.GlobalNormal.Y = y;
          this.GlobalNormal.Z = z;
          return this;
        }
      }


      private class PrimitiveImpl : IPrimitive {
        public PrimitiveImpl(PrimitiveType type, params IVertex[] vertices) {
          this.Type = type;
          this.Vertices = new ReadOnlyCollection<IVertex>(vertices);
        }

        public PrimitiveType Type { get; }
        public IReadOnlyList<IVertex> Vertices { get; }

        public IPrimitive SetMaterial(IMaterial material) {
          throw new System.NotImplementedException();
        }
      }
    }
  }
}