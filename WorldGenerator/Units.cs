using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public interface IUnit { }
    public interface Mm : IUnit { }
    public interface MmPerKy : IUnit { }
    public interface GTPerKm3 : IUnit { }
    public interface GTPerKm3PerKy : IUnit { }
    public interface IKy : IUnit { }
    public interface TNPerKm3 : IUnit { }
    public interface TN : IUnit { }
    public interface IVectorValued<TUnit> where TUnit : IUnit { Vector3 Value { get; } }
    public interface IFloatValued<TUnit> where TUnit : IUnit { float Value { get; } }
    public record struct Position(Vector3 Value) : IVectorValued<Mm>;
    public record struct Velocity(Vector3 Value) : IVectorValued<MmPerKy>;
    public record struct Distance(float Value) : IFloatValued<Mm>;
    public record struct Bouyancy(float Value) : IFloatValued<TNPerKm3>;
    public record struct Density(float Value) : IFloatValued<GTPerKm3>;
    public record struct DensityChange(float Value) : IFloatValued<GTPerKm3PerKy>;
    public record struct Time(float Value) : IFloatValued<IKy>;
    public record struct Force(Vector3 Value) : IVectorValued<TN>;
}
