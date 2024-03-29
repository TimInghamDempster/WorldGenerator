﻿using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public class PlateStretchesAtWeakPoint : FunctionalTest
    {
        private readonly DeformationSolver _deformationVelocitySolver;
        private readonly List<int> _weakPoints;
        private readonly ManifoldManipulator _manipulator;

        public PlateStretchesAtWeakPoint()
        {
            _mesh = Mesh.Plane(10);
            _manifold = new PointCloudManifold(_mesh.Vertices.ToArray(), _mesh.Faces);
            var edgeIndices =
                _manifold.Values.
                Select((v, i) => (v, i)).
                Where(v => v.v.X < -4 || v.v.X > 4).
                Select(p => p.i).
                ToList();

            _weakPoints =
                _manifold.Values.
                Select((v, i) => (v, i)).
                Where(v => 
                (v.v.X > 0.9 && v.v.X < 2.1)).
                Select(p => p.i).
                ToList();

            var originalEdgePositions = edgeIndices.ToDictionary(i => i, i => _manifold.Values[i]);
            var stretchVec = new Vector3(5, 1, 1);

            var constraints = new Func<int, Vector3, Vector3>(
                (i, v) => edgeIndices.Contains(i) ?
                originalEdgePositions[i] * stretchVec : v);


            var tensileStrength = new SimpleField<TNPerMm2, float>(
                _manifold.Values.Select(
                    (v, i) => _weakPoints.Contains(i) ? 0.1f : 1.0f).ToArray(), _manifold);


            _deformationVelocitySolver = new DeformationSolver(_manifold, constraints, tensileStrength, 
                new FuncField<TN, Vector3> (_manifold, (_,_) => Vector3.Zero));
            _manipulator = new ManifoldManipulator(_manifold, _deformationVelocitySolver);

            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _deformationVelocitySolver,
                _manipulator
            });

            _criteria = new(1000, TimeoutResult.TimedOut, new List<ICondition>
            {
                  new Should(StretchedAtWeakPoint, "Stretched at weak point"),
                  new ShouldNot(StretchedAtNormalPoint, "Did not stretch at normal point")
            });
        }
        
        private bool StretchedAtNormalPoint()
        {
            if(_manifold == null) return false;
            
            var stretchedEdges =
                DeformationSolver.CalcEdgeLengths(_manifold.Values.ToList(), _manifold).
                Where(l => l.Value > 4.0f);

            return stretchedEdges.Any(e =>
            !_weakPoints.Contains(e.Key.Index1) &&
            !_weakPoints.Contains(e.Key.Index2));
        }

        private bool StretchedAtWeakPoint()
        {
            if (_manifold == null) return false;

            var stretchedEdges =
                DeformationSolver.CalcEdgeLengths(_manifold.Values.ToList(), _manifold).
                Where(l => l.Value > 4.0f);

            return _weakPoints.All(i =>
            stretchedEdges.Any(
                e => e.Key.Index1 == i || e.Key.Index2 == i));
        }
    }
}
