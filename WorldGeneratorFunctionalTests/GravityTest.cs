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

        private Running _running = new();
        private int _frameCount;

        public GravityTest()
        {

            _geodesic = Mesh.Geodesic(Constants.EarthRadiusMm, 100000);

            _manifold = new PointCloudManifold(_geodesic.Vertices.ToArray());
            _gravity = new(_manifold);
            _velocity = new(_manifold, new Vector3[_manifold.ValueCount]);
        }

        public IReadOnlyList<Face> Faces => _geodesic.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public string Name => "Gravity Test";

        public State Update(GameTime gameTime)
        {
            if(_frameCount > 1000) return new Failed(Name, "Mesh did not collapse in time");

            foreach(var point in _manifold.Values)
            {
                if(point.Length() > 0.1f) continue;

                return new Succeeded("Gravity Test");
            }

            var time = new Time(1);

            _velocity.ProgressTime(_gravity, time);
            _manifold.ProgressTime(_velocity, time);

            _frameCount++;

            return _running;
        }
    }
}