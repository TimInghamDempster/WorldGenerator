using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class SlidesFastOnSteepSlope : FunctionalTest
    {
        private readonly DeformationSolver _deformationSolver;
        private readonly ManifoldManipulator _manipulator;
        private readonly GravitationalAcceleartionField _gravityField;

        public SlidesFastOnSteepSlope()
        {
            _mesh = Mesh.Plane(10);
            _manifold = new PointCloudManifold(_mesh.Vertices.ToArray(), _mesh.Faces);
            
            for(int i = 0; i < _manifold.Values.Length; i++)
            {
                _manifold.Values[i].Y = _manifold.Values[i].X / 2.0f;
            }

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

            _criteria = new TestCriteria(20, TimeoutResult.TimedOut, new List<ICondition>()
            {
                new Should(PlateSlid, "Plate Moved due to Gravity"),
            });
        }

        private bool PlateSlid() =>
            _manifold?.Values.All(v => v.X < -1) ?? false;
    }
}
