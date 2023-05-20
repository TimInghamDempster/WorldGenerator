﻿using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class DensityField : IDiscreteField<GTPerKm3, float>
    {
        private readonly IManifold _manifold;
        private readonly float[] _values;
        private readonly DensityChange _densityIncreaseRate;

        public DensityField(IManifold manifold, float[] values, DensityChange densityIncreaseRate)
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
                _values[i] = _values[i] + _densityIncreaseRate.Value * time.Value;
            }
        }

        public float Value(Vector3 position) =>
            throw new NotImplementedException();

        public float Values(int index) => _values[index];
    }
}
