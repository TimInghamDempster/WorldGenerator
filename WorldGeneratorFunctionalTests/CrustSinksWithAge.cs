using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class CrustSinksWithAge : IFunctionalTest
    {
        private Mesh _geodesic = Mesh.Plane(10);
        private BouyantVelocityField _velocityField;
        private CrustDensityField _densityField;
        private IManifold _manifold;
        private float xThreshold = float.MinValue;

        public CrustSinksWithAge()
        {
            _manifold = new PointCloudManifold(_geodesic.Vertices.ToArray());
            var densities =
                _manifold.Values.Select(p => Constants.OceanCrustDensityGTPerKm3 - 6.0f - p.X * 0.5f).ToArray();
            _densityField = new(_manifold, densities, new DensityChange(0.1f));
            _velocityField = new(_manifold, _densityField, new FuncField<Unitless, Vector3>(_manifold, p => -Vector3.UnitY));
        }

        public IReadOnlyList<Face> Faces => _geodesic.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public string Name => "Crust Sinks with Age";

        public State Update(GameTime gameTime)
        {
            var time = new Time(1);
            _densityField.ProgressTime(time);
            _manifold.ProgressTime(_velocityField, time);

            var sunkDepth = -1.0f;

            var sunk = _manifold.Values.Where(p => p.Y < sunkDepth);

            if (sunk.Any())
            {
                var oldThreshold = xThreshold;
                var sortedSunk = sunk.OrderBy(p => p.X);

                xThreshold = sortedSunk.First().X;

                if(oldThreshold > xThreshold)
                {
                    return new Failed(Name, $"Crust did not sink linearly by age");
                }
            }

            if (_manifold.Values.All(p => p.Y < sunkDepth))
            {
                return new Succeeded(Name);
            }

            return new Running();
        }
    }
}
