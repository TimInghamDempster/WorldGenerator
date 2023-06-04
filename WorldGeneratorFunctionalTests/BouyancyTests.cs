using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class BouyancyTestsSinks : IFunctionalTest
    {
        private VelocityField _velocity;
        private readonly BouyancyField _bouyancy;
        private GravityField _gravity;
        private readonly DiscreteSummingField<TN> _bodyForces;
        private Mesh _geodesic;
        private IManifold _manifold;

        private Running _running = new();
        private int _frameCount;

        public BouyancyTestsSinks()
        {
            _geodesic = Mesh.Geodesic(Constants.EarthRadiusMm, 100000);

            _manifold = new PointCloudManifold(_geodesic.Vertices.ToArray());
            _gravity = new(_manifold);
            _velocity = new(_manifold, new Vector3[_manifold.ValueCount]);
            _bouyancy = new(_manifold, Constants.MantleDensityGTPerMm3 * 0.99f);
            _bodyForces = new(new IDiscreteField<TN, Vector3>[] { _gravity, _bouyancy });
        }
        public IReadOnlyList<Face> Faces => _geodesic.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public string Name => "Bouyancy Sink Test";

        public State Update(GameTime gameTime)
        {
            if (_frameCount > 1000) return new Failed(Name, "Mesh did not collapse in time");

            foreach (var point in _manifold.Values)
            {
                if (point.Length() > 0.1f) continue;

                return new Succeeded(Name);
            }

            var time = new Time(1);

            _velocity.ProgressTime(_bodyForces, time);
            _manifold.ProgressTime(_velocity, time);

            _frameCount++;

            return _running;
        }
    }

    public class BouyancyTestsFloats : IFunctionalTest
    {
        private readonly VelocityField _velocity;
        private readonly BouyancyField _bouyancy;
        private readonly GravityField _gravity;
        private readonly Mesh _geodesic;
        private readonly DiscreteSummingField<TN> _bodyForces;
        private IManifold _manifold;

        private Running _running = new();
        private int _frameCount;

        public BouyancyTestsFloats()
        {
            _geodesic = Mesh.Geodesic(Constants.EarthRadiusMm, 100000);

            _manifold = new PointCloudManifold(_geodesic.Vertices.ToArray());
            _gravity = new(_manifold);
            _velocity = new(_manifold, new Vector3[_manifold.ValueCount]);
            _bouyancy = new(_manifold, Constants.MantleDensityGTPerMm3 * 1.001f);
            _bodyForces = new(new IDiscreteField<TN, Vector3>[] { _gravity, _bouyancy });
        }
        public IReadOnlyList<Face> Faces => _geodesic.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public string Name => "Bouyancy Float Test";

        public State Update(GameTime gameTime)
        {
            if (_frameCount > 1000) return new Succeeded(Name);

            foreach (var point in _manifold.Values)
            {
                if (point.Length() > Constants.EarthRadiusMm * 0.9f) continue;

                return new Failed(Name, "Planet collapsed");
            }

            var time = new Time(1);

            _velocity.ProgressTime(_bodyForces, time);
            _manifold.ProgressTime(_velocity, time);

            _frameCount++;

            return _running;
        }
    }
}
