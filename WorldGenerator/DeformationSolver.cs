using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace WorldGenerator
{
    public class DeformationSolver : IField<Mm, Vector3>, ITimeDependent
    {
        private readonly IField<TN, Vector3> _externalForces;
        private readonly IField<TNPerMm2, float> _tensileStrength;

        public IManifold Manifold { get; }

        public DeformationSolver(
            IManifold manifold,
            IField<TN, Vector3> externalForces,
            IField<TNPerMm2, float> tensileStrength)
        {
            Manifold = manifold;
            _externalForces = externalForces;
            _tensileStrength = tensileStrength;
            Values = Manifold.Values.ToArray();
        }

        public Vector3[] Values { get; private set; }

        public void ProgressTime(TimeKY timestep)
        {
            var edgeLengths = ApplySpringForces(timestep);
            
            var softBodyPositions = ApplyEdgeLengths(edgeLengths.NewLengths);
            var parts = SeparateParts(softBodyPositions, Manifold.Connectivity, edgeLengths.Broken.Edges);

            Values = MoveByParts(
                Manifold.Values,
                softBodyPositions,
                parts);
        }

        private Vector3[] MoveByParts(Vector3[] originalValues, Vector3[] softBodyPositions, IReadOnlyList<MeshPart> parts)
        {
            var newPositions = new List<Vector3>(originalValues);
            foreach(var part in parts)
            {
                var partPositions = part.Indices.Select(i => softBodyPositions[i]).ToArray();
                var partOriginalPositions = part.Indices.Select(i => originalValues[i]).ToArray();
                var partCenter = partPositions.Aggregate((a, b) => a + b) / partPositions.Length;
                var partOriginalCenter = partOriginalPositions.Aggregate((a, b) => a + b) / partOriginalPositions.Length;
                var partDelta = partCenter - partOriginalCenter;
                for (var i = 0; i < part.Indices.Count(); i++)
                {
                    newPositions[part.Indices[i]] += partDelta;
                }
            }

            return newPositions.ToArray();
        }

        private record EdgeLengths(Dictionary<Edge, float> Lengths);
        public record BrokenEdges(HashSet<Edge> Edges);

        private (BrokenEdges Broken, EdgeLengths NewLengths) ApplySpringForces(TimeKY timestep)
        {
            var newPositions = new List<Vector3>(Manifold.Values);
            var originalLengths = CalcEdgeLengths(newPositions, Manifold);
            var brokenEdges = new HashSet<Edge>();

            float maxForce;
            ApplyExternalForces(newPositions, _externalForces, timestep);
            int iterations = 100;
            for(int iteration = 0; iteration < iterations; iteration++)
            {
                maxForce = AdjustSprings(newPositions, Manifold.Values, timestep);
            }

            var targetForceMagnitude = _externalForces.Values.Sum(v => v.Length());
            var currentForceMagnitude =
                newPositions.
                Where((v, i) => _externalForces.Values[i].Length() > 0.01f).
                Select((v, i) => (v, i)).
                Sum(vi => (vi.v - Manifold.Values[vi.i]).Length());

            var forceRatio = 1.0f / 2.0f;

            var newLengths = CalcEdgeLengths(newPositions, Manifold);

            var threshold = 0.01f;
            foreach (var l in newLengths)
            {
                var delta = newLengths[l.Key] - originalLengths[l.Key];
                delta = delta * forceRatio;

                if (MathF.Abs(delta) > threshold)
                {
                    newLengths[l.Key] = originalLengths[l.Key] + delta;
                    brokenEdges.Add(l.Key);
                }
                else
                {
                    newLengths[l.Key] = originalLengths[l.Key];
                }
            }
            return (new(brokenEdges), new(newLengths));
        }

        private Vector3[] ApplyEdgeLengths(EdgeLengths edges)
        {
            var currentPositions = Manifold.Values.ToArray();

            var iterations = 100;
            for (int i = 0; i < iterations; i++)
            {
                foreach (var edge in edges.Lengths.Keys)
                {
                    var dir = currentPositions[edge.Index1] - currentPositions[edge.Index2];
                    var delta = edges.Lengths[edge] - dir.Length();
                    dir.Normalize();
                    var springForce = delta * 0.5f;
                    currentPositions[edge.Index1] += dir * springForce;
                    currentPositions[edge.Index2] -= dir * springForce;
                }
            }

            return currentPositions;
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
        public record MeshPart(IReadOnlyList<int> Indices);
        public record Connectivity(HashSet<Edge> Edges);

        public static IReadOnlyList<MeshPart> SeparateParts(Vector3[] positions, Connectivity connectivity, IEnumerable<Edge> brokenEdges)
        {
            var parts = positions.Select(
                (v,i) => new MeshPart(new List<int>() { i })).
                ToList();

            var partsByIndex = new Dictionary<int, MeshPart>();
            foreach(var part in parts)
            {
                foreach(var index in part.Indices)
                {
                    partsByIndex[index] = part;
                }
            }

            foreach(var edge in connectivity.Edges)
            {
                if(brokenEdges.Contains(edge))
                {
                    continue;
                }

                var part1 = partsByIndex[edge.Index1];
                var part2 = partsByIndex[edge.Index2];
                if(part1 == part2)
                {
                    continue;
                }

                var newPart = new MeshPart(part1.Indices.Concat(part2.Indices).ToList());
                foreach(var index in newPart.Indices)
                {
                    partsByIndex[index] = newPart;
                }

                parts.Remove(part1);
                parts.Remove(part2);
                parts.Add(newPart);
            }

            return parts;
        }
    }
}
