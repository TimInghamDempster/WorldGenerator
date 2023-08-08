using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public interface IUnit { }
    public interface Mm : IUnit { }
    public interface MmPerKy : IUnit { }
    public interface GTPerKm3 : IUnit { }
    public interface GTPerKm3PerKy : IUnit { }
    public interface Unitless : IUnit { }
    public interface IKy : IUnit { }
    public interface TNPerKm3 : IUnit { }
    public interface TN : IUnit { }
    public interface TNPerMm2 : IUnit { }
    public interface IVectorValued<TUnit> where TUnit : IUnit { Vector3 Value { get; } }
    public interface IFloatValued<TUnit> where TUnit : IUnit { float Value { get; } }
    public record struct TimeKY(float Value) : IFloatValued<IKy>;
    public record struct DensityChange(float Value) : IFloatValued<GTPerKm3PerKy>;
    }
