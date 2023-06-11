using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class DeformationVelocitySolver : IField<MmPerKy, Vector3>, ITimeDependent
    {
        private readonly IField<TN, Vector3> _externalForces;

        public IManifold Manifold { get; }

        public DeformationVelocitySolver(IManifold manifold, IField<TN, Vector3> externalForces)
        {
            Manifold = manifold;
            _externalForces = externalForces;
        }

        public Vector3[] Values => _externalForces.Values;

        public void ProgressTime(TimeKY timestep)
        {
            // NoP
        }
    }
}
