using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class BouyantVelocityField : IDiscreteField<MmPerKy, Vector3>
    {
        IManifold _manifold;
        IDiscreteField<GTPerKm3, float> _density;

        // TODO: Calibrate this
        private float _sinkRate = 0.1f;

        public BouyantVelocityField(IManifold   manifold, IDiscreteField<GTPerKm3, float> density)
        {
            _manifold = manifold;
            _density = density;
        }

        public int ValueCount => _manifold.ValueCount;

        public IManifold Manifold => _manifold;

        public IEnumerable<Vector3> Values => throw new NotImplementedException();

       public Vector3 Value(int index) =>
            _density.Value(index) < Constants.MantleDensityGTPerKm3 ? 
            new Vector3(0.0f, 0.0f, 0.0f) :
            Vector3.Normalize(_manifold.Value(index)) * -_sinkRate;
    }
}
