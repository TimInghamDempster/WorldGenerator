﻿using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class LargeForcesStretchPlate : IFunctionalTest
    {
        private static readonly float _planeSize = 10;
        private readonly PointCloudManifold _manifold;
        private readonly Mesh _plane = Mesh.Plane((int)_planeSize);
        private readonly DeformationVelocitySolver _deformationVelocitySolver;
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
                new Vector3(v.X / MathF.Abs(v.X)   / 5.0f, 0, 0) :
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

        public string Name => "Large Forces Stretch Plate";

        public State Update(GameTime gameTime)
        {
            _frameCount++;
            var time = new TimeKY(1);
            _fieldGroup.ProgressTime(time);

            var max = _manifold.Values[0].X;
            var min = _manifold.Values[0].X;

            if (_centralVerts.All(i => MathF.Abs(_manifold.Values[i].X) > 1.5f))
            {
                return new Succeeded(Name);
            }

            if(_frameCount > 1000)
            {
                  return new Failed(Name, $"Plate did not stretch in 1000 frames");
            }

            return new Running();
        }
    }
}
