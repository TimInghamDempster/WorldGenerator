using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public interface State { }
    public record Running : State { }
    public record Succeeded : State { }
    public record Failed(string Error) : State { }

    public interface IFunctionalTest
    {
        IReadOnlyList<Face> Faces { get; }
        IEnumerable<Vector3> Vertices { get; }
        State Update(GameTime gameTime);
    }
}
