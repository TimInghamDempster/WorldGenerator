using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class VelocityField : IDiscreteField<MmPerKy, Vector3>
    {
        private readonly Vector3[] _values;

        public IManifold Manifold { get; init; }

        public int ValueCount => _values.Length;

        public VelocityField(IManifold manifold, Vector3[] values)
        {
            Manifold = manifold;
            _values = values;
        }

        public void ProgressTime(Time timestep, IDiscreteField<TN, Vector3> forces)
        {
            if (forces.Manifold != Manifold)
            {
                throw new InvalidOperationException("Cannot integrate velocity from a force field" +
                    "with a different manifold");
            }

            for (int i = 0; i < _values.Length; i++)
            {
                _values[i] = _values[i] + forces.Values(i) * timestep.Value;
            }
        }

        public Vector3 Values(int index) => _values[index];
    }
}
