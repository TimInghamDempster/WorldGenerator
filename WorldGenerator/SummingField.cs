using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class DiscreteSummingField<TUnit> : IDiscreteField<TUnit, Vector3>
    {
        private readonly IEnumerable<IDiscreteField<TUnit, Vector3>> _fields;

        public int ValueCount => _fields.First().ValueCount;

        public IManifold Manifold => _fields.First().Manifold;

        public IEnumerable<Vector3> Values
        {
            get
            {
                for (int i = 0; i < _fields.First().ValueCount; i++)
                {
                    yield return Value(i);
                };
            }
        }

        public Vector3 Value(int index) =>
            _fields.Select(f => f.Value(index)).Aggregate((a,b) => a + b);

        public DiscreteSummingField(IEnumerable<IDiscreteField<TUnit, Vector3>> fields)
        {
            _fields = fields;

            foreach (var field in _fields)
            {
                if (field.Manifold != fields.First().Manifold)
                {
                    throw new ArgumentException("All fields must have the same manifold");
                }
            }
        }
    }
}
