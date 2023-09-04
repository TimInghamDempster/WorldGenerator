using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class SmallForcesDontStretchPlate : FunctionalTest
    {
        private readonly DeformationSolver _deformationSolver;
        private readonly Vector3[] _originalPositions;
        private readonly FuncField<TN, Vector3> _forces;
        private readonly ManifoldManipulator _manipulator;

        public SmallForcesDontStretchPlate()
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
