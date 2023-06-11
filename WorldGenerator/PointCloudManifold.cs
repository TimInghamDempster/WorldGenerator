using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class PointCloudManifold : IManifold
    {
        public IManifold Manifold => this;

        public int ValueCount => Values.Length;

        public Vector3[] Values { get; }

        public PointCloudManifold(Vector3[] positions)
        {
            Values = positions;
        }
    }
}
