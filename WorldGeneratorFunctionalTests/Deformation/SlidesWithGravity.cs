using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class SlidesWithGravity : FunctionalTest
    {
        private readonly DeformationSolver _deformationSolver;
        private readonly Vector3[] _originalPositions;
        //private readonly FuncField<Mm, Vector3> _forces;
        private readonly ManifoldManipulator _manipulator;

        public SlidesWithGravity()
        {
            _mesh = Mesh.Plane(10);
            _manifold = new PointCloudManifold(_mesh.Vertices.ToArray(), _mesh.Faces);
            var edgeIndices =
                _manifold.Values.
                Select((v, i) => (v, i)).
                Where(v => v.v.X < -4 || v.v.X > 4).
                Select(p => p.i).
                ToList();

            _originalPositions = _manifold.Values.ToArray();

            var constraints = 
                new Func<int, Vector3, Vector3>((i, v) => v);
                
            var tensileStrength = new SimpleField<TNPerMm2, float>(
                _manifold.Values.Select(_ => 1.0f).ToArray(), _manifold);

            _deformationSolver = new DeformationSolver(_manifold, constraints, tensileStrength);
            _manipulator = new ManifoldManipulator(_manifold, _deformationSolver);

            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _deformationSolver,
                _manipulator
            });

            _criteria = new TestCriteria(100, TimeoutResult.Completed, new List<ICondition>()
            {
                new ShouldNot(PlateStretched, "Plate Stretched Despite Insufficient Force"),
            });
        }

        private bool PlateStretched() =>
            _manifold?.Values.Select((p, i) => (p, i)).Any(v => _originalPositions[v.i] != v.p) ??
            false;
    }
}
