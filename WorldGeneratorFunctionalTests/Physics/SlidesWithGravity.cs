using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class SlidesWithGravity : FunctionalTest
    {
        private readonly DeformationSolver _deformationSolver;
        private readonly Vector3[] _originalPositions;
        private readonly ManifoldManipulator _manipulator;

        public SlidesWithGravity()
        {
            _mesh = Mesh.Plane(10);
            _manifold = new PointCloudManifold(_mesh.Vertices.ToArray(), _mesh.Faces);
            
            for(int i = 0; i < _manifold.Values.Length; i++)
            {
                _manifold.Values[i].Y = _manifold.Values[i].X / 10.0f;
            }

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
                new Should(PlateSlid, "Plate Moved due to Gravity"),
            });
        }

        private bool PlateSlid() =>
            _manifold?.Values.All(v => v.X > 5) ?? false;
    }
}
