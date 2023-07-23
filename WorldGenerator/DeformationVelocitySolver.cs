using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace WorldGenerator
{
    public class DeformationVelocitySolver : IField<MmPerKy, Vector3>, ITimeDependent
    {
        private readonly IField<TN, Vector3> _externalForces;

        public IManifold Manifold { get; }

        public DeformationVelocitySolver(IManifold manifold, IField<TN, Vector3> externalForces)
        {
            Manifold = manifold;
            _externalForces = externalForces;
            Values = new Vector3[Manifold.Values.Length];
        }

        public Vector3[] Values { get; private set; }

        public void ProgressTime(TimeKY timestep)
        {
            var newPositions = new List<Vector3>(Manifold.Values);
            var edgeLengths = CalcEdgeLengths(newPositions, Manifold);

            for(int i = 0; i < Manifold.Values.Length; i++)
            {
                newPositions[i] = Manifold.Values[i];
            }

            float maxForce;
            ApplyExternalForces(newPositions, timestep);
            do
            {
                maxForce = AdjustSprings(newPositions, Manifold.Values, timestep);
            } while(maxForce > 0.1f);

            var targetForceMagnitude = _externalForces.Values.Sum(v => v.Length());
            var currentForceMagnitude = 
                newPositions.
                Where((v,i) => _externalForces.Values[i].Length() > 0.01f).
                Select((v,i) => (v,i)).
                Sum(vi => (vi.v - Manifold.Values[vi.i]).Length());

            var forceRatio = targetForceMagnitude / currentForceMagnitude;

            var newLengths = CalcEdgeLengths(newPositions, Manifold);

            var threshold = 0.0005f;
            foreach (var l in newLengths)
            {
                var delta = newLengths[l.Key] - edgeLengths[l.Key];
                delta = delta * forceRatio;

                if (MathF.Abs(delta) > threshold)
                {
                    newLengths[l.Key] = edgeLengths[l.Key] + delta;
                }
                else
                {
                    newLengths[l.Key] = edgeLengths[l.Key];
                }
            }

            Values = SolveForEdgeLengthRelaxation(Manifold, newLengths);
        }

        public static Vector3[] SolveForEdgeLengthRelaxation(IManifold manifold, Dictionary<Edge, float> newLengths)
        {
            var newPositions = new List<Vector3>(manifold.Values);
            var springConst = 0.1f;
            var threshold = 0.001f;

            while (true)
            {
                var oldPositions = new List<Vector3>(newPositions);
                foreach (var edgeLength in newLengths)
                {
                    var edgeVector = newPositions[edgeLength.Key.Index1] - newPositions[edgeLength.Key.Index2];
                    var actualEdgeLength = edgeVector.Length();
                    var springDirection = edgeVector / actualEdgeLength;
                    var springForce = springDirection * (actualEdgeLength - edgeLength.Value) * springConst;
                    newPositions[edgeLength.Key.Index1] -= springForce;
                    newPositions[edgeLength.Key.Index2] += springForce;

                    if (actualEdgeLength == 0)
                    {
                        Debugger.Break();
                    }
                }

                var maxForce = newPositions.Select((p, i) => (p - oldPositions[i]).Length()).Max();
                if (maxForce < threshold) break;
            }

            return newPositions.Select((p,i) => p  - manifold.Values[i]).ToArray();
        }

        public static Dictionary<Edge, float> CalcEdgeLengths(List<Vector3> positions, IManifold manifold) =>
            manifold.Edges.
            Select(e => (e, (positions[e.Index1] - positions[e.Index2]).Length())).
            ToDictionary(kvp => kvp.e, kvp => kvp.Item2);

        private float AdjustSprings(List<Vector3> newPositions, IReadOnlyList<Vector3> originalPositions, TimeKY timestep)
        {
            var forces= new List<Vector3>(originalPositions.Count);
            for(int i = 0; i < originalPositions.Count; i++)
            {
                forces.Add(Vector3.Zero);
            }

            for(int i = 0; i < originalPositions.Count; i++)
            { 
                foreach(var neighbour in Manifold.Neighbours[i].Indices)
                {
                    if(i >= neighbour)
                    {
                        continue;
                    }

                    var spring = newPositions[i] - newPositions[neighbour];
                    var originalLength = (originalPositions[i] - originalPositions[neighbour]).Length();
                    var springLength = spring.Length();
                    var springDirection = spring / springLength;
                    var springForce = springDirection * (springLength - originalLength);
                    forces[i] -= springForce;
                    forces[neighbour] += springForce;

                    if(springLength == 0)
                    {
                        Debugger.Break();
                    }
                }
            }

            for(int i = 0; i < originalPositions.Count; i++)
            {
                newPositions[i] += forces[i] * 0.1f;
            }

            return forces.Max(f => f.Length());
        }

        private void ApplyExternalForces(List<Vector3> newPositions, TimeKY timestep)
        {
            for(int i = 0; i < newPositions.Count; i++)
            {
                newPositions[i] += _externalForces.Values[i] * timestep.Value;
            }
        }
    }
}
