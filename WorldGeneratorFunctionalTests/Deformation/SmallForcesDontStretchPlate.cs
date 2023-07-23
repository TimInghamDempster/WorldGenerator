using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class SmallForcesDontStretchPlate : IFunctionalTest
    {
        private readonly PointCloudManifold _manifold;
        private readonly Mesh _plane = Mesh.Plane(10);
        private readonly DeformationVelocitySolver _deformationVelocitySolver;
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
                Select(p => p.i);

            _originalPositions = _manifold.Values.ToArray();

            _forces = new FuncField<TN, Vector3>(
                _manifold,
                (i, v) => edgeIndices.Contains(i) ?
                new Vector3(v.X / (MathF.Abs(v.X) * 20.0f), 0, 0) :
                Vector3.Zero);
                

            _deformationVelocitySolver = new DeformationVelocitySolver(_manifold, _forces);
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

        public string Name => "Small Forces Don't Stretch Plate";

        public State Update(GameTime gameTime)
        {
            _frameCount++;
            var time = new TimeKY(1);
            _fieldGroup.ProgressTime(time);

            if (_manifold.Values.Select((p, i) => (p, i)).Any(v => _originalPositions[v.i] != v.p))
            {
                return new Failed(Name, "Plate Stretched Despite Insufficient Force");
            }

            if(_frameCount > 1000)
            {
                return new Succeeded(Name);
            }

            return new Running();
        }
    }
}
