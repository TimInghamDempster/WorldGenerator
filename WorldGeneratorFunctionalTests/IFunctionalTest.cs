using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public enum State
    {
        Running,
        Passed,
        Failed
    }

    public interface IFunctionalTest
    {
        IReadOnlyList<Face> Faces { get; }
        IEnumerable<Vector3> Vertices { get; }
        State Update(GameTime gameTime);
    }
}
