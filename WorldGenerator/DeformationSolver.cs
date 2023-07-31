using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace WorldGenerator
{
    public class DeformationSolver : IField<Mm, Vector3>, ITimeDependent
    {
        private readonly IField<TN, Vector3> _externalForces;

        public IManifold Manifold { get; }

        public DeformationSolver(IManifold manifold, IField<TN, Vector3> externalForces)
        {
            Manifold = manifold;
            _externalForces = externalForces;
            Values = Manifold.Values.ToArray();
        }

        public Vector3[] Values { get; private set; }

        public void ProgressTime(TimeKY timestep)
        {
            var edgeLengths = ApplySpringForces(timestep);

            ApplyEdgeLengths(edgeLengths);
        }

        private Dictionary<Edge, float> ApplySpringForces(TimeKY timestep)
        {
            var newPositions = new List<Vector3>(Manifold.Values);
            var originalLengths = CalcEdgeLengths(newPositions, Manifold);

            float maxForce;
            ApplyExternalForces(newPositions, _externalForces, timestep);
            do
            {
                maxForce = AdjustSprings(newPositions, Manifold.Values, timestep);
            } while (maxForce > 0.1f);

            var targetForceMagnitude = _externalForces.Values.Sum(v => v.Length());
            var currentForceMagnitude =
                newPositions.
                Where((v, i) => _externalForces.Values[i].Length() > 0.01f).
                Select((v, i) => (v, i)).
                Sum(vi => (vi.v - Manifold.Values[vi.i]).Length());

            var forceRatio = targetForceMagnitude / currentForceMagnitude;

            var newLengths = CalcEdgeLengths(newPositions, Manifold);

            var threshold = 0.001f;
            foreach (var l in newLengths)
            {
                var delta = newLengths[l.Key] - originalLengths[l.Key];
                delta = delta * forceRatio;

                newLengths[l.Key] = 
                    delta > threshold ? originalLengths[l.Key] + delta : 
                    originalLengths[l.Key];
            }
            return newLengths;
        }

        private void ApplyEdgeLengths(Dictionary<Edge, float> edges)
        {
            var iterations = 100;
            for(int i = 0; i < iterations; i++)
            {
                foreach(var edge in edges.Keys)
                {
                    var dir = Values[edge.Index1] - Values[edge.Index2];
                    var delta = edges[edge] - dir.Length();
                    dir.Normalize();
                    var springForce = delta * 0.5f;
                    Values[edge.Index1] += dir * springForce;
                    Values[edge.Index2] -= dir * springForce;
                }
            }    
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

        private static void ApplyExternalForces(
            List<Vector3> newPositions,
            IField<TN, Vector3> externalForces, 
            TimeKY timestep)
        {
            for(int i = 0; i < newPositions.Count; i++)
            {
                newPositions[i] += externalForces.Values[i] * timestep.Value;
            }
        }
    }
}
