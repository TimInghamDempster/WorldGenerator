using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class ManifoldManipulator : ITimeDependent
    {
        private readonly IManifold _manifold;
        private readonly IField<MmPerKy, Vector3> _velocities;

        public ManifoldManipulator(IManifold manifold, IField<MmPerKy, Vector3> velocities)
        {
            _manifold = manifold;
            _velocities = velocities;

            if(_manifold != _velocities.Manifold)
            {
                throw new ArgumentException("Manifolds must be the same");
            }
        }
        public void ProgressTime(TimeKY timestep)
        {
            for(int i = 0; i < _manifold.Values.Length; i++)
            {
                _manifold.Values[i] += _velocities.Values[i] * timestep.Value;
            }
        }
    }
}
