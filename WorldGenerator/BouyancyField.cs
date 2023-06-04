using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class BouyancyField : IContinousField<TN, Vector3>, IDiscreteField<TN, Vector3>
    {
        private readonly float _mantleDensityGTPerMm3;

        public int ValueCount => Manifold.ValueCount;

        public IManifold Manifold { get; init; }

        public BouyancyField(IManifold manifold, float mantleDensityGTPerMm3)
        {
            Manifold = manifold;
            _mantleDensityGTPerMm3 = mantleDensityGTPerMm3;
        }

        public IEnumerable<Vector3> Values => throw new NotImplementedException();

        public Vector3 Value(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public Vector3 Value(int index)
        {
            var x = Manifold.Value(index);
            var r = x.Length();

            var submergedVolume = Constants.CellVolumeMm3(Manifold.ValueCount);
            var mass = _mantleDensityGTPerMm3 * submergedVolume;
            var force = GravityField.Magnitude(mass);

            return r < Constants.EarthRadiusMm ? Vector3.Normalize(x) * force : Vector3.Zero;
        }
    }
}
