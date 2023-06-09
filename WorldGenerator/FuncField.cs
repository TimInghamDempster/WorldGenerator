using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class FuncField<TUnit, TStorage> : IField<Unitless, TStorage>
    {
        private readonly Func<Vector3, TStorage> _func;

        public IManifold Manifold { get; }

        public FuncField(IManifold manifold, Func<Vector3, TStorage> func)
        {
            Manifold = manifold;
            _func = func;

            Values = new TStorage[Manifold.Values.Length];

            for(int i = 0; i < Manifold.Values.Length; i++)
            {
                Values[i] = _func(Manifold.Values[i]);
            }
        }

        public TStorage[] Values { get; }
    }
}
