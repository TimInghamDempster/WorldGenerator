using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class DistToNearestPointField : IContinousField<Mm, float>
    {
        private readonly IManifold _manifold;

        public DistToNearestPointField(IManifold manifold)
        {
            _manifold = manifold;
        }

        public float Value(Vector3 position)
        {
            var nearestPos = _manifold.NearestPoint(position);

            return Vector3.Distance(position, nearestPos);
        }
    }
}
