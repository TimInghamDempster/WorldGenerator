using Microsoft.Xna.Framework;

namespace WorldGeneratorTests
{
    internal static class Misc
    {
        public static Vector3 RandomVector(Random random) =>
            new((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
    }
}
