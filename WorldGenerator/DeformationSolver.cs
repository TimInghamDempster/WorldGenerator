using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace WorldGenerator
{
    public class DeformationSolver : IField<Mm, Vector3>, ITimeDependent
    {
        private readonly Func<int, Vector3, Vector3> _externalConstraints;
        private readonly IField<TNPerMm2, float> _tensileStrength;

        public IManifold Manifold { get; }

        public DeformationSolver(
            IManifold manifold,
            Func<int, Vector3, Vector3> externalConstraints,
            IField<TNPerMm2, float> tensileStrength)
        {
            Manifold = manifold;
            _externalConstraints = externalConstraints;
            _tensileStrength = tensileStrength;
            Values = Manifold.Values.ToArray();
        }

        public Vector3[] Values { get; private set; }

        public void ProgressTime(TimeKY timestep)
        {
            Values = ApplySpringForces(timestep);
        }

        private Vector3[] ApplySpringForces(TimeKY timestep)
        {
            var newPositions = new List<Vector3>(Manifold.Values);

            float maxForce;
            int iterations = 100;
            for(int iteration = 0; iteration < iterations; iteration++)
            {
                ApplyExternalConstraints(newPositions, _externalConstraints, timestep);
                maxForce = AdjustSprings(newPositions, Manifold.Values, timestep);
            }
            return newPositions.ToArray();
        }

        public static Dictionary<Edge, float> CalcEdgeLengths(List<Vector3> positions, IManifold manifold) =>
            manifold.Connectivity.Edges.
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

                    var edgeStrength = (_tensileStrength.Values[i] + _tensileStrength.Values[neighbour]) * 0.5f;

                    var spring = newPositions[i] - newPositions[neighbour];
                    var originalLength = (originalPositions[i] - originalPositions[neighbour]).Length();
                    var springLength = spring.Length();
                    var springDirection = spring / springLength;
                    var springForce = springDirection * (springLength - originalLength) * edgeStrength;
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

        private static void ApplyExternalConstraints(
            List<Vector3> newPositions,
            Func<int, Vector3, Vector3> externalConstraints, 
            TimeKY timestep)
        {
            for(int i = 0; i < newPositions.Count; i++)
            {
                newPositions[i] = externalConstraints(i, newPositions[i]);
            }
        }
    }
}
