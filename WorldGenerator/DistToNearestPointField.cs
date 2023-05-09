using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class DistToNearestPointField : IField<Distance>
    {
        private readonly IManifold _manifold;

        public DistToNearestPointField(IManifold manifold)
        {
            _manifold = manifold;
        }

        public Distance Value(Position position)
        {
            var nearestPos = _manifold.NearestPoint(position);

            return new(Vector3.Distance(position.Value, nearestPos.Value));
        }
    }
}
