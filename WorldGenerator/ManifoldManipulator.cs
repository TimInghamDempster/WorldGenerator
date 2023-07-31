using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class ManifoldManipulator : ITimeDependent
    {
        private readonly IManifold _manifold;
        private readonly IField<Mm, Vector3> _newPositions;

        public ManifoldManipulator(IManifold manifold, IField<Mm, Vector3> newPositions)
        {
            _manifold = manifold;
            _newPositions = newPositions;

            if(_manifold != _newPositions.Manifold)
            {
                throw new ArgumentException("Manifolds must be the same");
            }
        }
        public void ProgressTime(TimeKY timestep)
        {
            for(int i = 0; i < _manifold.Values.Length; i++)
            {
                _manifold.Values[i] = _newPositions.Values[i];
            }
        }
    }
}
