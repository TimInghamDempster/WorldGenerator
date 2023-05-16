using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class DensityField : IDiscreteField<Density>
    {
        private readonly IManifold _manifold;
        private readonly Density[] _values;
        private readonly DensityChange _densityIncreaseRate;

        public DensityField(IManifold manifold, Density[] values, DensityChange densityIncreaseRate)
        {
            _manifold = manifold;
            _values = values;
            _densityIncreaseRate = densityIncreaseRate;
        }

        public IManifold Manifold => _manifold;

        public int ValueCount => _values.Length;

        public void ProgressTime(Time time)
        {
            for(int i = 0; i < _values.Length; i++)
            {
                _values[i] = new Density(
                    _values[i].Value + _densityIncreaseRate.Value * time.Value);
            }
        }

        public Density Value(Position position) =>
            throw new NotImplementedException();

        public Density Values(int index) => _values[index];
    }
}
