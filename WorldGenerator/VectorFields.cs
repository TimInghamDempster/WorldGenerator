using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public interface IManifold : IField<Mm, Vector3>
    {
    }

    public class TempManifold : IManifold
    {
        public IManifold Manifold => throw new NotImplementedException();

        public int ValueCount => throw new NotImplementedException();

        public Vector3[] Values => throw new NotImplementedException();

        public Vector3 NearestPoint(Vector3 testLocation)
        {
            return Vector3.UnitX;
        }

        public IEnumerable<int> Neighbours(int origin)
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

        public Vector3[] Values => throw new NotImplementedException();

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
    }

    public interface ITimeDependent
    {
        void ProgressTime(TimeKY timestep);
    }

    public interface IField<TUnit, TStorage>
    {
        IManifold Manifold { get; }
        TStorage[] Values { get; }
    }

    public class FieldGroup : ITimeDependent
    {
        private readonly IEnumerable<ITimeDependent> _fields;

        public FieldGroup(IEnumerable<ITimeDependent> fields)
        {
            _fields = fields;
        }

        public void ProgressTime(TimeKY timestep)
        {
            foreach(var field in _fields)
            {
                field.ProgressTime(timestep);
            }
        }
    }

    public record SimpleField<TUnit, TStorage>(TStorage[] Values, IManifold Manifold) : IField<TUnit, TStorage>
    {
    }
}