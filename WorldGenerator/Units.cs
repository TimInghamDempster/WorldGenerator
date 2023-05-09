namespace WorldGenerator
{
    public enum Magnitude
    {
        None,
        Deca,
        Centi,
        Kilo,
        Mega,
        Giga
    }

    public enum Dimension
    {
        Meter,
        Second,
        Newton
    }

    public record struct UnitPart(Magnitude Magnitude, Dimension Dimension, int Power);
    public record Unit(IReadOnlyList<UnitPart> Parts);
}
