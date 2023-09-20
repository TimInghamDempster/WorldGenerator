using Microsoft.Xna.Framework;
using static WorldGenerator.DeformationSolver;

namespace WorldGenerator
{
    public record Neighbours(int[] Indices);

    public record Edge(int Index1, int Index2);

    public interface IManifold : IField<Mm, Vector3>
    {
        Dictionary<int, Neighbours> Neighbours { get; }
        Dictionary<int, FaceGroup> Faces { get; }
        Connectivity Connectivity { get; }

        public static Vector3 FaceNormal(IManifold manifold, Face face)
        {
            var v0 = manifold.Values[face.Indices[0]];
            var v1 = manifold.Values[face.Indices[1]];
            var v2 = manifold.Values[face.Indices[2]];
            var normal = Vector3.Cross(v1 - v0, v2 - v0);

            return Vector3.Normalize(normal);
        }
    }
    public record Connectivity(HashSet<Edge> Edges);
    public record FaceGroup(IEnumerable<Face> Faces);

    public interface ITimeDependentField<TUnit, TStorage> : IField<TUnit, TStorage>, ITimeDependent { }

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

    public class ColourField<TUnit> : IField<Unitless, Color>, ITimeDependent
    {
        private readonly IField<TUnit, float> _inputField;
        private readonly ColourGradient _colourGradient;

        public IManifold Manifold { get; }

        public Color[] Values { get; }

        public ColourField(IManifold manifold, IField<TUnit, float> inputField, ColourGradient colourGradient)
        {
            Manifold = manifold;
            _inputField = inputField;
            _colourGradient = colourGradient;
            Values = inputField.Values.Select(
                v => colourGradient.ColourAt(v)).ToArray();
        }

        public void ProgressTime(TimeKY timestep)
        {
            for(int i = 0; i < Values.Length; i++)
            {
                Values[i] = _colourGradient.ColourAt(_inputField.Values[i]);
            }
        }
    }

    public record SimpleField<TUnit, TStorage>(TStorage[] Values, IManifold Manifold) : IField<TUnit, TStorage>
    {
    }
}