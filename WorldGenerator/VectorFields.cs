using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public interface IManifold : IDiscreteField<Mm, Vector3>
    {
        IEnumerable<int> Neighbours(int origin);
        Vector3 NearestPoint(Vector3 testLocation);
        void ProgressTime(IDiscreteField<MmPerKy, Vector3> velocityField, Time timestep);
    }

    public class TempManifold : IManifold
    {
        public IManifold Manifold => throw new NotImplementedException();

        public int ValueCount => throw new NotImplementedException();

        public IEnumerable<Vector3> Values => throw new NotImplementedException();

        public Vector3 NearestPoint(Vector3 testLocation)
        {
            return Vector3.UnitX;
        }

        public IEnumerable<int> Neighbours(int origin)
        {
            throw new NotImplementedException();
        }

        public void ProgressTime(IDiscreteField<MmPerKy, Vector3> velocityField, Time timestep)
        {
            throw new NotImplementedException();
        }

        public Vector3 Value(int index)
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

        public IEnumerable<Vector3> Values => throw new NotImplementedException();

        public Vector3 NearestPoint(Vector3 testLocation)
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

        public void ProgressTime(IDiscreteField<MmPerKy, Vector3> velocityField, Time timestep)
        {
            throw new NotImplementedException();
        }

        public Vector3 Value(int index)
        {
            throw new NotImplementedException();
        }
    }

    public interface IDiscreteField<TUnit, TStorage>
    {
        TStorage Value(int index);
        int ValueCount { get; }
        IManifold Manifold { get; }
        IEnumerable<TStorage> Values { get; }
    }

    public record SimpleField<TUnit, TStorage>(TStorage[] ValuesArray, IManifold Manifold) : IDiscreteField<TUnit, TStorage>
    {
        public int ValueCount => ValuesArray.Length;

        public IEnumerable<TStorage> Values => ValuesArray;

        public void SetValue(int index, TStorage newValue)
        {
            ValuesArray[index] = newValue;
        }

        public TStorage Value(int index) => ValuesArray[index];
    }

    public static class FieldOperators
    {
        public static SimpleField<TUnit, float> DiffuseSimple<TUnit>(IDiscreteField<TUnit, float> initialField)
        {
            var newVals = new float[initialField.ValueCount];

            for(int i = 0; i < newVals.Count(); i++)
            {
                var neighbours = initialField.Manifold.Neighbours(i);
                var distributedToNeighbours = 0.1f * neighbours.Count();

                newVals[i] =
                    initialField.Value(i) * (1.0f - distributedToNeighbours) +
                    neighbours.Select(n => initialField.Value(n)).Sum() * 0.1f;
            }

            return new(newVals, initialField.Manifold);
        }
    }
}