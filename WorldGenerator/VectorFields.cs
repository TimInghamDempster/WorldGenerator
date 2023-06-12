using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public record Neighbours(int[] Indices);

    public interface IManifold : IField<Mm, Vector3>
    {
        Dictionary<int, Neighbours> Neighbours { get; }
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