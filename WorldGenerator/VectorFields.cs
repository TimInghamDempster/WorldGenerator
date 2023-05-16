using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public interface IVisualiser<TInput>
    {
        Color GetColour(Position pos, IContinousField<TInput> field);
    }

    public class DistToGrayscaleVisualiser : IVisualiser<Distance>
    {
        public Color GetColour(Position pos, IContinousField<Distance> field)
        {
            var dist = field.Value(pos);
            var col = 1.0f - dist.Value * 200.0f;
            return new Color(col, col, col);
        }
    }

    public class Vector3Visualiser<TUnit> : IVisualiser<IVectorValued<TUnit>>
        where TUnit : IUnit
    {
        public Color GetColour(Position pos, IContinousField<IVectorValued<TUnit>> field)
        {
            return new Color(field.Value(pos).Value);
        }
    }

    public interface IManifold : IDiscreteField<Position>
    {
        IEnumerable<int> Neighbours(int origin);
        Position NearestPoint(Position testLocation);
        void ProgressTime(IDiscreteField<Velocity> velocityField, Time timestep);
    }

    public class TempManifold : IManifold
    {
        public IManifold Manifold => throw new NotImplementedException();

        public int ValueCount => throw new NotImplementedException();

        public Position NearestPoint(Position testLocation)
        {
            return new(Vector3.UnitX);
        }

        public IEnumerable<int> Neighbours(int origin)
        {
            throw new NotImplementedException();
        }

        public void ProgressTime(IDiscreteField<Velocity> velocityField, Time timestep)
        {
            throw new NotImplementedException();
        }

        public Position Values(int index)
        {
            throw new NotImplementedException();
        }
    }

    public class EuclideanManifold1d : IManifold
    {
        private readonly int _length;

        public EuclideanManifold1d(int length)
        {
            _length = length;
        }

        public IManifold Manifold => throw new NotImplementedException();

        public int ValueCount => throw new NotImplementedException();

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

        public void ProgressTime(IDiscreteField<Velocity> velocityField, Time timestep)
        {
            throw new NotImplementedException();
        }

        public Position Values(int index)
        {
            throw new NotImplementedException();
        }
    }

    public interface IDiscreteField<out TValue>
    {
        TValue Values(int index);
        int ValueCount { get; }
        IManifold Manifold { get; }
    }
    public interface IContinousField<out TValue>
    {
        TValue Value(Position position);
    }
    public record SimpleField<TValue>(TValue[] Values, IManifold Manifold) : IDiscreteField<TValue>
    {
        public int ValueCount => Values.Length;

        TValue IDiscreteField<TValue>.Values(int index) => Values[index];
    }

    public static class FieldOperators
    {
        public static SimpleField<float> DiffuseSimple(IDiscreteField<float> initialField)
        {
            var newVals = new float[initialField.ValueCount];

            for(int i = 0; i < newVals.Count(); i++)
            {
                var neighbours = initialField.Manifold.Neighbours(i);
                var distributedToNeighbours = 0.1f * neighbours.Count();

                newVals[i] =
                    initialField.Values(i) * (1.0f - distributedToNeighbours) +
                    neighbours.Select(n => initialField.Values(n)).Sum() * 0.1f;
            }

            return new(newVals, initialField.Manifold);
        }
    }
}