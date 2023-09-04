using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class LargeForcesStretchPlate : IFunctionalTest
    {
        private static readonly float _planeSize = 10;
        private readonly PointCloudManifold _manifold;
        private readonly Mesh _plane = Mesh.Plane((int)_planeSize);
        private readonly DeformationSolver _deformationSolver;
        private readonly IEnumerable<int> _centralVerts;
        private readonly FuncField<TN, Vector3> _forces;
        private readonly FieldGroup _fieldGroup;
        private readonly ManifoldManipulator _manipulator;
        private int _frameCount;

        public LargeForcesStretchPlate()
        {
            _manifold = new PointCloudManifold(_plane.Vertices.ToArray(), _plane.Faces);
            var edgeIndexPos = (_planeSize / 2.0f) - 0.1f;
            var edgeIndices =
                _manifold.Values.
                Select((v, i) => (v, i)).
                Where(v => v.v.X < -edgeIndexPos || v.v.X > edgeIndexPos).
                Select(p => p.i).
                ToList();

            _centralVerts =
                _manifold.Values.
                Select((v, i) => (v, i)).
                Where(v => 
                (v.v.X > -1.1 && v.v.X < -0.9) || 
                (v.v.X > 0.9 && v.v.X < 1.1)).
                Select(p => p.i).
                ToList();

            _forces = new FuncField<TN, Vector3>(
                _manifold,
                (i, v) => edgeIndices.Contains(i) ?
                new Vector3(v.X / (MathF.Abs(v.X) / 20.0f), 0, 0) :
                Vector3.Zero);

            var tensileStrength = new SimpleField<TNPerMm2, float>(
                _manifold.Values.Select(_ => 1f).ToArray(), _manifold);

            _deformationSolver = new DeformationSolver(_manifold, _forces, tensileStrength);
            _manipulator = new ManifoldManipulator(_manifold, _deformationSolver);

            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _forces,
                _deformationSolver,
                _manipulator
            });

            Criteria = new TestCriteria(
                100, TimeoutResult.TimedOut,
                new List<ICondition>()
                {
                    new Should(PlateStretched, "Plate Stretched"),
                });
        }

        public int FrameCount => _frameCount;
        private bool PlateStretched()
        {
            var max = _manifold.Values[0].X;
            var min = _manifold.Values[0].X;

            return _centralVerts.All(i => MathF.Abs(_manifold.Values[i].X) > 2.5f);
        }

        public IReadOnlyList<Face> Faces => _plane.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public string Name => "Large Forces Stretch Plate";

        public TestCriteria Criteria { get; }

        public void Update(GameTime gameTime)
        {
            _frameCount++;
            var time = new TimeKY(1);
            _fieldGroup.ProgressTime(time);
        }
    }
}
