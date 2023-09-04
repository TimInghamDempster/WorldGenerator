using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class SmallForcesDontStretchPlate : IFunctionalTest
    {
        private readonly PointCloudManifold _manifold;
        private readonly Mesh _plane = Mesh.Plane(10);
        private readonly DeformationSolver _deformationSolver;
        private readonly Vector3[] _originalPositions;
        private readonly FuncField<TN, Vector3> _forces;
        private readonly FieldGroup _fieldGroup;
        private readonly ManifoldManipulator _manipulator;
        private int _frameCount;

        public SmallForcesDontStretchPlate()
        {
            _manifold = new PointCloudManifold(_plane.Vertices.ToArray(), _plane.Faces);
            var edgeIndices =
                _manifold.Values.
                Select((v, i) => (v, i)).
                Where(v => v.v.X < -4 || v.v.X > 4).
                Select(p => p.i).
                ToList();

            _originalPositions = _manifold.Values.ToArray();

            _forces = new FuncField<TN, Vector3>(
                _manifold,
                (i, v) => edgeIndices.Contains(i) ?
                new Vector3(v.X / MathF.Abs(v.X) * 0.0f, 0, 0) :
                Vector3.Zero);

            var tensileStrength = new SimpleField<TNPerMm2, float>(
                _manifold.Values.Select(_ => 1.0f).ToArray(), _manifold);

            _deformationSolver = new DeformationSolver(_manifold, _forces, tensileStrength);
            _manipulator = new ManifoldManipulator(_manifold, _deformationSolver);

            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _forces,
                _deformationSolver,
                _manipulator
            });

            Criteria = new TestCriteria(100, TimeoutResult.Completed, new List<ICondition>()
            {
                new ShouldNot(PlateStretched, "Plate Stretched Despite Insufficient Force"),
            });
        }

        private bool PlateStretched() =>
            _manifold.Values.Select((p, i) => (p, i)).Any(v => _originalPositions[v.i] != v.p);

        public IReadOnlyList<Face> Faces => _plane.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;
        public int FrameCount => _frameCount;

        public string Name => "Small Forces Don't Stretch Plate";

        public TestCriteria Criteria { get; }

        public void Update(GameTime gameTime)
        {
            _frameCount++;
            var time = new TimeKY(1);
            _fieldGroup.ProgressTime(time);
        }
    }
}
