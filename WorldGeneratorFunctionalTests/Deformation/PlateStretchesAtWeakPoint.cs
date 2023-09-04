using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class PlateStretchesAtWeakPoint : IFunctionalTest
    {
        private readonly PointCloudManifold _manifold;
        private readonly Mesh _plane = Mesh.Plane(10);
        private readonly DeformationSolver _deformationVelocitySolver;
        private readonly List<int> _weakPoints;
        private readonly FuncField<TN, Vector3> _forces;
        private readonly FieldGroup _fieldGroup;
        private readonly ManifoldManipulator _manipulator;
        private int _frameCount;

        public PlateStretchesAtWeakPoint()
        {
            _manifold = new PointCloudManifold(_plane.Vertices.ToArray(), _plane.Faces);
            var edgeIndices =
                _manifold.Values.
                Select((v, i) => (v, i)).
                Where(v => v.v.X < -4 || v.v.X > 4).
                Select(p => p.i).
                ToList();

            _weakPoints =
                _manifold.Values.
                Select((v, i) => (v, i)).
                Where(v => 
                (v.v.X > 0.9 && v.v.X < 2.1)).
                Select(p => p.i).
                ToList();

            _forces = new FuncField<TN, Vector3>(
                _manifold,
                (i, v) => edgeIndices.Contains(i) ?
                new Vector3(v.X * 2.0f / MathF.Abs(v.X), 0, 0) :
                Vector3.Zero);


            var tensileStrength = new SimpleField<TNPerMm2, float>(
                _manifold.Values.Select(
                    (v, i) => _weakPoints.Contains(i) ? 0.1f : 1.0f).ToArray(), _manifold);


            _deformationVelocitySolver = new DeformationSolver(_manifold, _forces, tensileStrength);
            _manipulator = new ManifoldManipulator(_manifold, _deformationVelocitySolver);

            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _forces,
                _deformationVelocitySolver,
                _manipulator
            });

            Criteria = new(1000, TimeoutResult.TimedOut, new List<ICondition>
            {
                  new Should(StretchedAtWeakPoint, "Stretched at weak point"),
                  new ShouldNot(StretchedAtNormalPoint, "Did not stretch at normal point")
            });
        }
        public int FrameCount => _frameCount;

        private bool StretchedAtNormalPoint()
        {
            var stretchedEdges =
                DeformationSolver.CalcEdgeLengths(_manifold.Values.ToList(), _manifold).
                Where(l => l.Value > 4.0f);

            return stretchedEdges.Any(e =>
            !_weakPoints.Contains(e.Key.Index1) &&
            !_weakPoints.Contains(e.Key.Index2));
        }

        private bool StretchedAtWeakPoint()
        {
            var stretchedEdges =
                DeformationSolver.CalcEdgeLengths(_manifold.Values.ToList(), _manifold).
                Where(l => l.Value > 4.0f);

            return _weakPoints.All(i =>
            stretchedEdges.Any(
                e => e.Key.Index1 == i || e.Key.Index2 == i));
        }

        public IReadOnlyList<Face> Faces => _plane.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public string Name => "Stretches At Weak Point";

        public TestCriteria Criteria { get; }

        public void Update(GameTime gameTime)
        {
            _frameCount++;
            var time = new TimeKY(1);
            _fieldGroup.ProgressTime(time);
        }
    }
}
