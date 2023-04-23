﻿using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public interface IVisualiser
    {
        Color GetColour(Position pos, IFloatField field);
    }

    public interface IManifold
    {
        IEnumerable<int> Neighbours(int origin);
        Position NearestPoint(Position testLocation);
    }

    public class EuclideanManifold1d : IManifold
    {
        private readonly int _length;

        public EuclideanManifold1d(int length)
        {
            _length = length;
        }

        public Position NearestPoint(Position testLocation)
        {
            throw new NotImplementedException();
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

    public record struct Position(Vector3 Value, Unit Unit);
    public record struct Velocity(Vector3 Value, Unit Unit);
    public record struct Distance(float Value, Unit Unit);

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