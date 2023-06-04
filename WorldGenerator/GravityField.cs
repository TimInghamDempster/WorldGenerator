using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class GravityField : IContinousField<TN, Vector3>, IDiscreteField<TN, Vector3>
    {
        public IManifold Manifold { get; init; }

        public int ValueCount => throw new NotImplementedException();

        public IEnumerable<Vector3> Values => throw new NotImplementedException();

        public GravityField(IManifold manifold)
        {
            Manifold = manifold;
        }

        public Vector3 Value(Vector3 position)
        {
            var dir = position;
            
            // point towards planet centre
            dir *= -1.0f;

            dir.Normalize();

            var submergedVolume = Constants.CellVolumeMm3(Manifold.ValueCount);
            var mass = Constants.CrustDensityGTPerMm3 * submergedVolume;
            var force = GravityField.Magnitude(mass);

            return dir * force;
        }

        public static float Magnitude(float massGT) =>
            Constants.GravitationalConstantTNMm2PerGT2 * Constants.EarthMassGT * massGT * Constants.EarthRadiusMm;

        public Vector3 Value(int index) => Value(Manifold.Value(index));
    }
}