using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class DummyTest : IFunctionalTest
    {
        private Mesh _geodesic = Mesh.Geodesic(Constants.EarthRadiusMm, 100000);

        public IReadOnlyList<Face> Faces => _geodesic.Faces;

        public IEnumerable<Vector3> Vertices => _geodesic.Vertices;

        public string Name => "Dummy Test";

        public State Update(GameTime gameTime)
        {
            return new Running();
        }
    }
}
