using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class DoesntSlideOnFlat : FunctionalTest
    {
        private readonly DeformationSolver _deformationSolver;
        private readonly ManifoldManipulator _manipulator;
        private readonly GravitationalAcceleartionField _gravityField;

        public DoesntSlideOnFlat()
        {
            _mesh = Mesh.Plane(10);
            _manifold = new PointCloudManifold(_mesh.Vertices.ToArray(), _mesh.Faces);
            
            var constraints = 
                new Func<int, Vector3, Vector3>((i, v) => v);
                
            var tensileStrength = new SimpleField<TNPerMm2, float>(
                _manifold.Values.Select(_ => 1.0f).ToArray(), _manifold);

            _gravityField = new GravitationalAcceleartionField(_manifold, 
                new FuncField<Unitless, Vector3>(_manifold, (_,_) => -Vector3.UnitY));

            _deformationSolver = new DeformationSolver(_manifold, constraints, tensileStrength, _gravityField);
            _manipulator = new ManifoldManipulator(_manifold, _deformationSolver);


            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _deformationSolver,
                _manipulator,
                _gravityField
            });

            _criteria = new TestCriteria(100, TimeoutResult.Completed, new List<ICondition>()
            {
                new ShouldNot(PlateSlid, "Plate Moved due to Gravity"),
            });
        }

        private bool PlateSlid() =>
            _manifold?.Values.All(v => v.X < -1) ?? false;
    }
}
