using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldGenerator
{

    internal class RenderMesh
    {
        internal VertexBuffer VertexBuffer { get; }
        internal IndexBuffer IndexBuffer { get; }
        public int FacesCount { get; }

        internal RenderMesh(Mesh mesh, GraphicsDevice graphicsDevice)
        {
            var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(PositionColorTex3Vertex), mesh.Vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(mesh.Vertices.Select(
                v => new PositionColorTex3Vertex(v, Color.White, new Vector3(0.0f, 0.0f, 0.0f))).ToArray());
            VertexBuffer = vertexBuffer;

            var indexBuffer = new IndexBuffer(graphicsDevice, typeof(int), mesh.Faces.Count * 3, BufferUsage.WriteOnly);
            indexBuffer.SetData(mesh.Faces.SelectMany(f => f.Indices).ToArray());
            IndexBuffer = indexBuffer;

            FacesCount = mesh.Faces.Count;
        }

    }
}
