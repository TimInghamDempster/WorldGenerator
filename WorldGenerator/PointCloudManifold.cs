using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class PointCloudManifold : IManifold
    {
        private readonly Dictionary<Int3, List<int>> _positionsHash;
        private const float _quantFactor = 100.0f;

        public IManifold Manifold => this;

        public int ValueCount => Values.Length;

        public Vector3[] Values { get; }

        public PointCloudManifold(Vector3[] positions)
        {
            Values = positions;

            _positionsHash = new ();
            BuildHash();
        }

        private void BuildHash()
        {
            _positionsHash.Clear();
            for (int i = 0; i < Values.Count(); i++)
            {
                var quantPos = new Int3(Values[i] * _quantFactor);
                if (false == _positionsHash.ContainsKey(quantPos))
                {
                    _positionsHash.Add(quantPos, new List<int>());
                }
                _positionsHash[quantPos].Add(i);
            }
        }

        public void ProgressTime(IField<MmPerKy, Vector3> velocityField, Time timestep)
        {
            if(velocityField.Manifold != this)
            {
                throw new InvalidOperationException("Cannot integrate position from a velocity field" +
                    "with a different manifold");
            }

            for(int i = 0; i < Values.Length; i++)
            {
                var vel = velocityField.Values[i];
                Values[i] = Values[i] + vel * timestep.Value;
            }
        }

        public Vector3 NearestPoint(Vector3 testLocation)
        {
            var quantPos = new Int3(testLocation * _quantFactor);

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

            if (false == candidates.Any()) return Vector3.Zero;

            return
                candidates.
                Select(c => Values[c]).
                OrderBy(p => Vector3.Distance(p, testLocation)).
                First();
        }

        public IEnumerable<int> Neighbours(int origin)
        {
            throw new NotImplementedException();
        }

        public Vector3 Value(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public Vector3 Value(int index) => Values[index];
    }
}
