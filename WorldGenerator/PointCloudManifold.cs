using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class PointCloudManifold : IManifold
    {
        private readonly List<Position> _positions;

        public PointCloudManifold(List<Position> positions)
        {
            _positions = positions;
        }

        public Position NearestPoint(Position testLocation)
        {
            return
                _positions.
                OrderBy(p => Vector3.Distance(p.Value, testLocation.Value)).
                First();
        }

        public IEnumerable<int> Neighbours(int origin)
        {
            throw new NotImplementedException();
        }
    }
}
