using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class PhysicsVelocitySolver : IField<MmPerKy, Vector3>
    {
        public IManifold Manifold { get; }

        public PhysicsVelocitySolver(IManifold manifold)
        {
            Manifold = manifold;
        }

        public Vector3[] Values => throw new NotImplementedException();
    }
}
