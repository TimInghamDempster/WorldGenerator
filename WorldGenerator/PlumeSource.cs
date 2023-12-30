using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public record Plume(Vector3 Location);

    public interface IPlumeSource
    {
        IReadOnlyList<Plume> Plumes { get; }
    }

    public class PlumeSource : IPlumeSource
    {
        public IReadOnlyList<Plume> Plumes => throw new NotImplementedException();
    }
}
