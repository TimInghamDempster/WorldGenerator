using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldGenerator
{
    public struct PositionColorTex3Vertex : IVertexType
    {
        public Vector3 Positon;
        public Color Color;
        public Vector3 TexCoord;

        static readonly VertexDeclaration MyVertexDeclaration
            = new VertexDeclaration(new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
            });

        public PositionColorTex3Vertex(Vector3 position, Color col, Vector3 centrePos)
        {
            Positon = position;
            Color = col;
            TexCoord = centrePos;
        }

        public VertexDeclaration VertexDeclaration
        {
            get { return MyVertexDeclaration; }
        }

    }
}
