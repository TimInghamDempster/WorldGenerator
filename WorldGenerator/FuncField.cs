using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class FuncField<TUnit, TStorage> : IField<TUnit, TStorage>, ITimeDependent
    {
        private readonly Func<int, Vector3, TStorage> _func;
        private TimeKY _time;

        public IManifold Manifold { get; }

        public FuncField(IManifold manifold, Func<int, Vector3, TStorage> func)
        {
            Manifold = manifold;
            _func = func;

            Values = new TStorage[Manifold.Values.Length];

            for(int i = 0; i < Manifold.Values.Length; i++)
            {
                Values[i] = _func(i, Manifold.Values[i]);
            }
        }

        public TStorage[] Values { get; }

        public void ProgressTime(TimeKY timestep)
        {
            _time.Value += timestep.Value;
        }
    }
}
