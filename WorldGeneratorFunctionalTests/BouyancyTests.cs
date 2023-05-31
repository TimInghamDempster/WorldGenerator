using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class BouyancyTestsSinks : IFunctionalTest
    {
        private VelocityField _velocity;
        private GravityField _gravity;
        private Mesh _geodesic;
        private IManifold _manifold;

        private Running _running = new();

        public BouyancyTestsSinks()
        {

            _geodesic = Mesh.Geodesic(Constants.EarthRadiusMm, 100000);

            _manifold = new PointCloudManifold(_geodesic.Vertices.ToArray());
            _gravity = new(0.0001f, _manifold);
            _velocity = new(_manifold, new Vector3[_manifold.ValueCount]);
        }
        public IReadOnlyList<Face> Faces => _geodesic.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public State Update(GameTime gameTime)
        {
            return new Failed("Bouyancy Sink Test","Not implemented");
        }
    }

    public class BouyancyTestsFloats : IFunctionalTest
    {
        private VelocityField _velocity;
        private GravityField _gravity;
        private Mesh _geodesic;
        private IManifold _manifold;

        private Running _running = new();

        public BouyancyTestsFloats()
        {

            _geodesic = Mesh.Geodesic(Constants.EarthRadiusMm, 100000);

            _manifold = new PointCloudManifold(_geodesic.Vertices.ToArray());
            _gravity = new(0.0001f, _manifold);
            _velocity = new(_manifold, new Vector3[_manifold.ValueCount]);
        }
        public IReadOnlyList<Face> Faces => _geodesic.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public State Update(GameTime gameTime)
        {
            return new Failed("Bouyancy Float Test", "Not implemented");
        }
    }
}
