using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class LargeForcesStretchPlate : FunctionalTest
    {
        private static readonly float _planeSize = 10;
        private readonly DeformationSolver _deformationSolver;
        private readonly IReadOnlyList<int> _centralVerts;
        private readonly ManifoldManipulator _manipulator;

        public LargeForcesStretchPlate()
        {
            _mesh = Mesh.Plane((int)_planeSize);
            _manifold = new PointCloudManifold(_mesh.Vertices.ToArray(), _mesh.Faces);
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

            var originalEdgePositions = edgeIndices.ToDictionary(i => i, i => _manifold.Values[i]);
            var stretchVec = new Vector3(5, 1, 1);

            var runLength = 100;

            var constraints = new Func<int, Vector3, Vector3>(
                (i, v) => edgeIndices.Contains(i) ?
                originalEdgePositions[i] * stretchVec : v);

            var tensileStrength = new SimpleField<TNPerMm2, float>(
                _manifold.Values.Select(_ => 1f).ToArray(), _manifold);

            _deformationSolver = new DeformationSolver(_manifold, constraints, tensileStrength,
                new FuncField<TN, Vector3>(_manifold, (_, _) => Vector3.Zero));
            _manipulator = new ManifoldManipulator(_manifold, _deformationSolver);

            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _deformationSolver,
                _manipulator
            });

            _criteria = new TestCriteria(
                runLength, TimeoutResult.TimedOut,
                new List<ICondition>()
                {
                    new Should(PlateStretched, "Plate Stretched"),
                });
        }

        private bool PlateStretched()
        {
            if (_manifold is null) return false;

            var max = _manifold.Values[0].X;
            var min = _manifold.Values[0].X;

            return _centralVerts.All(i => MathF.Abs(_manifold.Values[i].X) > 2.5f);
        }
    }
}
