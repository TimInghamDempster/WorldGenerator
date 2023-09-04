using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class SpreadingCreatesRidges : IFunctionalTest
    {

        private readonly Mesh _plane = Mesh.Plane(10);

        public IReadOnlyList<Face> Faces => _plane.Faces;

        public IEnumerable<Vector3> Vertices => _plane.Vertices;

        public string Name => "Globe";

        public TestCriteria Criteria => throw new NotImplementedException();
        public int FrameCount => throw new NotImplementedException();

        public void Update(GameTime gameTime)
        {

        }
    }
}
