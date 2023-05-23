using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class GravityTest : IFunctionalTest
    {
        private VelocityField _velocity;
        private GravityField _gravity;
        private Mesh _geodesic;
        private IManifold _manifold;

        public GravityTest()
        {

            _geodesic = Mesh.Geodesic(1.0f, 100000);

            _manifold = new PointCloudManifold(_geodesic.Vertices.ToArray());
            _gravity = new(0.0001f, _manifold);
            _velocity = new(_manifold, new Vector3[_manifold.ValueCount]);
        }

        public IReadOnlyList<Face> Faces => _geodesic.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public State Update(GameTime gameTime)
        {
            var time = new Time(1);

            _velocity.ProgressTime(_gravity, time);
            _manifold.ProgressTime(_velocity, time);

            return State.Running;
        }
    }
}