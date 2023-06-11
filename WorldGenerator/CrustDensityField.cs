using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class CrustDensityField : IField<GTPerKm3, float>, ITimeDependent
    {
        private readonly IManifold _manifold;
        private readonly float[] _values;
        private readonly DensityChange _densityIncreaseRate;

        public CrustDensityField(IManifold manifold, float[] values, DensityChange densityIncreaseRate)
        {
            _manifold = manifold;
            _values = values;
            _densityIncreaseRate = densityIncreaseRate;
        }

        public IManifold Manifold => _manifold;


        public float[] Values => _values;

        public void ProgressTime(TimeKY time)
        {
            for(int i = 0; i < _values.Length; i++)
            {
                _values[i] = _values[i] + _densityIncreaseRate.Value * time.Value;
            }
        }

        public float Value(int index) => _values[index];
    }
}
