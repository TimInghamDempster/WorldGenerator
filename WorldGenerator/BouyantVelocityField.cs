using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class BouyantVelocityField : IDiscreteField<MmPerKy, Vector3>
    {
        private readonly IManifold _manifold;
        private readonly IDiscreteField<GTPerKm3, float> _density;
        private readonly IDiscreteField<Unitless, Vector3> _gravityDir;

        // TODO: Calibrate this
        private float _sinkRate = 0.1f;

        public BouyantVelocityField(
            IManifold manifold, 
            IDiscreteField<GTPerKm3, float> density,
            IDiscreteField<Unitless, Vector3> gravityDir)
        {
            _manifold = manifold;
            _density = density;
            _gravityDir = gravityDir;
        }

        public int ValueCount => _manifold.ValueCount;

        public IManifold Manifold => _manifold;

        public IEnumerable<Vector3> Values => throw new NotImplementedException();

       public Vector3 Value(int index) =>
            _density.Value(index) < Constants.MantleDensityGTPerKm3 ? 
            new Vector3(0.0f, 0.0f, 0.0f) :
            _gravityDir.Value(index) * _sinkRate;
    }
}
