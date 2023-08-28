using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace WorldGenerator
{
    public class DeformationSolver : IField<Mm, Vector3>, ITimeDependent
    {
        private readonly IField<TN, Vector3> _externalForces;
        private readonly IField<TNPerMm2, float> _tensileStrength;
        private readonly Mesh _mesh;

        public IManifold Manifold { get; }

        public DeformationSolver(
            IManifold manifold,
            IField<TN, Vector3> externalForces,
            IField<TNPerMm2, float> tensileStrength, 
            Mesh mesh)
        {
            Manifold = manifold;
            _externalForces = externalForces;
            _tensileStrength = tensileStrength;
            _mesh = mesh;
            Values = Manifold.Values.ToArray();
        }

        public Vector3[] Values { get; private set; }

        public void ProgressTime(TimeKY timestep)
        {
            var currentPositions = new List<Vector3>(Manifold.Values);
            var edgeLengths = ApplySpringForces(timestep);
            
            var iterations = 10;
            for (int i = 0; i < iterations; i++)
            {
                ApplyEdgeLengths(edgeLengths.NewLengths);
                ApplyAngleConstraint(currentPositions, _mesh.Faces, edgeLengths.Broken);
            }
        }

        private record EdgeLengths(Dictionary<Edge, float> Lengths);
        public record IndicesWithBrokenEdges(HashSet<int> Indices);

        private (IndicesWithBrokenEdges Broken, EdgeLengths NewLengths) ApplySpringForces(TimeKY timestep)
        {
            var newPositions = new List<Vector3>(Manifold.Values);
            var originalLengths = CalcEdgeLengths(newPositions, Manifold);
            var brokenIndices = new HashSet<int>();

            float maxForce;
            ApplyExternalForces(newPositions, _externalForces, timestep);
            do
            {
                maxForce = AdjustSprings(newPositions, Manifold.Values, timestep);
            } while (maxForce > 0.05f);

            var targetForceMagnitude = _externalForces.Values.Sum(v => v.Length());
            var currentForceMagnitude =
                newPositions.
                Where((v, i) => _externalForces.Values[i].Length() > 0.01f).
                Select((v, i) => (v, i)).
                Sum(vi => (vi.v - Manifold.Values[vi.i]).Length());

            var forceRatio = targetForceMagnitude / currentForceMagnitude;

            var newLengths = CalcEdgeLengths(newPositions, Manifold);

            var threshold = 0.01f;
            foreach (var l in newLengths)
            {
                var delta = newLengths[l.Key] - originalLengths[l.Key];
                delta = delta * forceRatio;

                if (MathF.Abs(delta) > threshold)
                {
                    newLengths[l.Key] = originalLengths[l.Key] + delta;
                    brokenIndices.Add(l.Key.Index1);
                    brokenIndices.Add(l.Key.Index2);
                }
                else
                {
                    newLengths[l.Key] = originalLengths[l.Key];
                }
            }
            return (new(brokenIndices), new(newLengths));
        }

        private void ApplyEdgeLengths(EdgeLengths edges)
        {
            foreach(var edge in edges.Lengths.Keys)
            {
                var dir = Values[edge.Index1] - Values[edge.Index2];
                var delta = edges.Lengths[edge] - dir.Length();
                dir.Normalize();
                var springForce = delta * 0.5f;
                Values[edge.Index1] += dir * springForce;
                Values[edge.Index2] -= dir * springForce;
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

        public static int[][] VertexCombinations { get; } =
            new int[][]
            {
                new int[] { 0, 1, 2 },
                new int[] { 1, 2, 0 },
                new int[] { 2, 0, 1 }
            };

        public void ApplyAngleConstraint(
            IReadOnlyList<Vector3> originalVertices, 
            IReadOnlyList<Face> faces,
            IndicesWithBrokenEdges broken)
        {
            foreach (var face in faces)
            {
                if(face.Indices.Any(i => broken.Indices.Contains(i)))
                {
                    continue;
                }
                foreach (var combo in VertexCombinations)
                {
                    var v0 = Values[face.Indices[combo[0]]];
                    var v1 = Values[face.Indices[combo[1]]];
                    var v2 = Values[face.Indices[combo[2]]];

                    var o0 = originalVertices[face.Indices[combo[0]]];
                    var o1 = originalVertices[face.Indices[combo[1]]];
                    var o2 = originalVertices[face.Indices[combo[2]]];


                    var a = v1 - v0;
                    var b = v2 - v0;
                    var h = Vector3.Normalize(a + b);
                    var originalNorm = Vector3.Cross(a, b);

                    var oa = o1 - o0;
                    var ob = o2 - o0;

                    var oaAngle = CalcAngle(originalVertices,
                        face.Indices[combo[0]], face.Indices[combo[1]], face.Indices[combo[2]]);

                    var axis = Vector3.Normalize(Vector3.Cross(a, h));

                    var rotation = Matrix.CreateFromAxisAngle(axis, oaAngle * 0.5f);
                    var rotation2 = Matrix.CreateFromAxisAngle(axis, oaAngle * -0.5f);

                    var newV1 = Vector3.Transform(h * oa.Length(), rotation2) + v0;
                    var newV2 = Vector3.Transform(h * ob.Length(), rotation) + v0;

                    Values[face.Indices[combo[1]]] = newV1;
                    Values[face.Indices[combo[2]]] = newV2;
                    var newNorm = Vector3.Cross(newV1 - v0, newV2 - v0);

                    /*if(Vector3.Dot(newNorm, originalNorm) < 0)
                    {
                        Values[face.Indices[combo[1]]] = newV2;
                        Values[face.Indices[combo[2]]] = newV1;
                    }*/
                }
            }
        }

        public static float CalcAngle(IReadOnlyList<Vector3> vertices,
            int centerIndex, int endOne, int endTwo)
        {
            var v0 = vertices[centerIndex];
            var v1 = vertices[endOne];
            var v2 = vertices[endTwo];

            var a = v1 - v0;
            var b = v2 - v0;

            var angle = Vector3.Dot(a, b) / (a.Length() * b.Length());

            return MathF.Acos(angle);
        }
    }
}
