using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class FuncField<TUnit, TStorage> : IDiscreteField<Unitless, TStorage>
    {
        private readonly Func<Vector3, TStorage> _func;

        public int ValueCount => throw new NotImplementedException();

        public IManifold Manifold { get; }

        public FuncField(IManifold manifold, Func<Vector3, TStorage> func)
        {
            Manifold = manifold;
            _func = func;
        }

        public IEnumerable<TStorage> Values => throw new NotImplementedException();

        public TStorage Value(int index) => _func(Manifold.Value(index));
    }
}
