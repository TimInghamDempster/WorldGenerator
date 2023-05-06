using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public record struct Int3(int X, int Y, int Z)
    {
        public Int3(Vector3 position) : 
            this((int)position.X, (int)position.Y, (int)position.Z) { }
        public static Int3 operator +(Int3 a, Int3 b)
            => new Int3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public record struct UInt3(uint X, uint Y, uint Z)
    {
        public static UInt3 operator +(UInt3 a, UInt3 b)
            => new UInt3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
}
