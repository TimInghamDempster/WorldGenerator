using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class PlateStretchesAtWeakPoint : IFunctionalTest
    {
        private readonly PointCloudManifold _manifold;
        private readonly Mesh _plane = Mesh.Plane(10);
        private readonly DeformationSolver _deformationVelocitySolver;
        private readonly IEnumerable<int> _centralEdges;
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

            _centralEdges =
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
                new Vector3(v.X / MathF.Abs(v.X), 0, 0) :
                Vector3.Zero);
                

            _deformationVelocitySolver = new DeformationSolver(_manifold, _forces);
            _manipulator = new ManifoldManipulator(_manifold, _deformationVelocitySolver);

            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _forces,
                _deformationVelocitySolver,
                _manipulator
            });
        }
        public IReadOnlyList<Face> Faces => _plane.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public string Name => "Stretches At Weak Point";

        public State Update(GameTime gameTime)
        {
            _frameCount++;
            var time = new TimeKY(1);
            _fieldGroup.ProgressTime(time);

            var max = _manifold.Values[0].X;
            var min = _manifold.Values[0].X;

            var edgeLengths = new List<float>();


            if (_centralEdges.All(i => MathF.Abs(_manifold.Values[i].X) > 1.5f))
            {
                
            }

            if(_frameCount > 100)
            {
                return new Succeeded(Name);
            }

            return new Running();
        }
    }
}
