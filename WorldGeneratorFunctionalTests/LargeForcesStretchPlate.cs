using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class LargeForcesStretchPlate : IFunctionalTest
    {
        private readonly PointCloudManifold _manifold;
        private readonly Mesh _plane = Mesh.Plane(10);
        private readonly DeformationVelocitySolver _deformationVelocitySolver;
        private readonly FuncField<TN, Vector3> _forces;

        public LargeForcesStretchPlate()
        {
            _manifold = new PointCloudManifold(_plane.Vertices.ToArray());
            _forces = new FuncField<TN, Vector3>(
                _manifold,
                v => v.X switch
                {
                    < -4 => new Vector3(-1.0f, 0, 0),
                    > 4 => new Vector3(1.0f, 0, 0),
                    _ => Vector3.Zero
                });

            _deformationVelocitySolver = new DeformationVelocitySolver(_manifold, _forces);
        }
        public IReadOnlyList<Face> Faces => _plane.Faces;

        public IEnumerable<Vector3> Vertices => _manifold.Values;

        public string Name => "Forces Stretch Plate";

        public State Update(GameTime gameTime)
        {
            var time = new Time(1);
            _manifold.ProgressTime(_deformationVelocitySolver, time);

            var max = _manifold.Values[0].X;
            var min = _manifold.Values[0].X;

            foreach (var v in _manifold.Values)
            {
                if (v.X > max) max = v.X;
                if (v.X < min) min = v.X;
            }

            if (max - min > 20.0f)
            {
                return new Succeeded(Name);
            }

            return new Running();
        }
    }
}
