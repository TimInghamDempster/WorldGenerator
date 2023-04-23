using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public interface IVisualiser
    {
        Color GetColour(Position pos, IFloatField field);
    }

    public interface IManifold
    {
        IEnumerable<int> Neighbours(int origin);
    }

    public class EuclideanManifold1d : IManifold
    {
        private readonly int _length;

        public EuclideanManifold1d(int length)
        {
            _length = length;
        }

        public IEnumerable<int> Neighbours(int origin) =>
            origin switch
            {
                0 => new int[] { 1 },
                int i when i == _length - 1  => new int[] { _length - 2 },
                _ => new int[] { origin - 1, origin + 1 }
            };

    }

    public interface IFloatField 
    {
        float[] Values { get; } 
        IManifold Manifold { get; }
    }

    public interface IFloatField<TValue> : IFloatField, IField<TValue> { }
    public interface IField<TValue>
    {
        TValue Value(Position position);
    }
    public record SimpleField(float[] Values, IManifold Manifold) : IFloatField;

    public enum Unit
    {
        None,
        Deca,
        Centi,
        Kilo,
        Mega,
        Giga
    }

    public record Position(Vector3 Value, Unit Unit);
    public record Velocity(Vector3 Value, Unit Unit);

    public class Gravity : IField<Velocity>
    {
        public float _g_mPerS { get; init; }

        public Gravity(float g_mPerS)
        {
            _g_mPerS = g_mPerS;
        }

        public Velocity Value(Position position)
        {
            var dir = position.Value;
            
            // point towards planet centre
            dir *= -1.0f;

            dir.Normalize();
            dir *= _g_mPerS;

            return new(dir, position.Unit);
        }
    }

    public static class FieldOperators
    {
        public static SimpleField DiffuseSimple(IFloatField initialField)
        {
            var newVals = new float[initialField.Values.Count()];

            for(int i = 0; i < newVals.Count(); i++)
            {
                var neighbours = initialField.Manifold.Neighbours(i);
                var distributedToNeighbours = 0.1f * neighbours.Count();

                newVals[i] =
                    initialField.Values[i] * (1.0f - distributedToNeighbours) +
                    neighbours.Select(n => initialField.Values[n]).Sum() * 0.1f;
            }

            return new(newVals, initialField.Manifold);
        }
    }
}