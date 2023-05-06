using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class PointCloudManifold : IManifold
    {
        private readonly List<Position> _positions;
        private readonly Dictionary<Int3, List<int>> _positionsHash;
        private const float _quantFactor = 100.0f;

        public PointCloudManifold(List<Position> positions)
        {
            _positions = positions;

            _positionsHash = new Dictionary<Int3, List<int>>();

            for(int i = 0; i < _positions.Count; i++)
            {
                var quantPos = new Int3(_positions[i].Value * _quantFactor);
                if (false == _positionsHash.ContainsKey(quantPos))
                {
                    _positionsHash.Add(quantPos, new List<int>());
                }
                _positionsHash[quantPos].Add(i);
            }
        }

        public Position NearestPoint(Position testLocation)
        {
            var quantPos = new Int3(testLocation.Value * _quantFactor);

            var candidates = new List<int>();
            for(int dz = -1; dz < 2; dz++)
            {
                for (int dy = -1; dy < 2; dy++)
                {
                    for (int dx = -1; dx < 2; dx++)
                    {
                        var offset = new Int3(dx, dy, dz);
                        var testBox = quantPos + offset;

                        if(_positionsHash.ContainsKey(testBox))
                        {
                            candidates.AddRange(_positionsHash[testBox]);
                        }
                    }
                }
            }

            if (false == candidates.Any()) return new Position(Vector3.Zero, Unit.None);

            return
                candidates.
                Select(c => _positions[c]).
                OrderBy(p => Vector3.Distance(p.Value, testLocation.Value)).
                First();
        }

        public IEnumerable<int> Neighbours(int origin)
        {
            throw new NotImplementedException();
        }
    }
}
