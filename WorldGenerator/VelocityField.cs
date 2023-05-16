namespace WorldGenerator
{
    public class VelocityField : IDiscreteField<Velocity>
    {
        private readonly Velocity[] _values;

        public IManifold Manifold { get; init; }

        public int ValueCount => _values.Length;

        public VelocityField(IManifold manifold, Velocity[] values)
        {
            Manifold = manifold;
            _values = values;
        }

        public void ProgressTime(Time timestep, IDiscreteField<Force> forces)
        {
            if (forces.Manifold != Manifold)
            {
                throw new InvalidOperationException("Cannot integrate velocity from a force field" +
                    "with a different manifold");
            }

            for (int i = 0; i < _values.Length; i++)
            {
                _values[i] = new(_values[i].Value + forces.Values(i).Value * timestep.Value);
            }
        }

        public Velocity Values(int index) => _values[index];
    }
}
