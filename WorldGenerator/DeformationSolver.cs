using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace WorldGenerator
{
    public class DeformationSolver : IField<Mm, Vector3>, ITimeDependent
    {
        private readonly IField<TN, Vector3> _externalForces;

        enum EdgeState
        {
            Intact,
            Broken
        }

        public IManifold Manifold { get; }

        public DeformationSolver(IManifold manifold, IField<TN, Vector3> externalForces)
        {
            Manifold = manifold;
            _externalForces = externalForces;
            Values = new Vector3[Manifold.Values.Length];
        }

        public Vector3[] Values { get; private set; }

        public void ProgressTime(TimeKY timestep)
        {
            var brokenEdges = FindBrokenEdges(timestep);

            var plates = SplitIntoPlates(Manifold, brokenEdges);

            Values = MovePlates(Manifold, _externalForces, plates.Plates, timestep);
        }

        private Vector3[] MovePlates(
            IManifold manifold,
            IField<TN, Vector3> externalForces, 
            IReadOnlyList<Plate> plates,
            TimeKY timestep)
        {
            var newPositions = new List<Vector3>(manifold.Values);

            var refinementSteps = 10;
            ApplyExternalForces(newPositions, externalForces, timestep);
            for(int i = 0; i < refinementSteps; i++)
            {
                AdjustSprings(newPositions, manifold.Values, timestep);
                ReconstructPlates(newPositions, plates);
            }

            return newPositions.ToArray();
        }

        private void ReconstructPlates(List<Vector3> newPositions, IReadOnlyList<Plate> plates)
        {
            foreach (var plate in plates)
            {
                var com = plate.Indices.Select(i => newPositions[i]).Aggregate((a, b) => a + b) / plate.Indices.Count;

                for (int i = 0; i < plate.Indices.Count; i++)
                {
                    newPositions[plate.Indices[i]] = com + plate.RelativePositions[i];
                }
            }
        }

        private record World(IReadOnlyList<Plate> Plates, IReadOnlyList<int> Deformable);
        private record Plate(IReadOnlyList<int> Indices, IReadOnlyList<Vector3> RelativePositions, Vector3 CoM);

        private World SplitIntoPlates(IManifold manifold, Dictionary<Edge, EdgeState> brokenEdges)
        {
            var plates = new List<Plate>();
            var notVisited = new HashSet<int>(Enumerable.Range(0, manifold.Values.Length));
            var notConnected = new HashSet<int>();

            while (notVisited.Any())
            {
                var indices = new List<int>();
                var toVisit = new Queue<int>();
                toVisit.Enqueue(notVisited.First());
                notVisited.Remove(toVisit.Peek());

                while (toVisit.Any())
                {
                    var current = toVisit.Dequeue();
                    indices.Add(current);

                    foreach (var neighbour in manifold.Neighbours[current].Indices)
                    {
                        if (notVisited.Contains(neighbour) && brokenEdges[new(current, neighbour)] == EdgeState.Intact)
                        {
                            toVisit.Enqueue(neighbour);
                            notVisited.Remove(neighbour);
                        }
                    }
                }

                var com = indices.Select(i => manifold.Values[i]).Aggregate((a, b) => a + b) / indices.Count;
                var relativePositions = indices.Select(i => manifold.Values[i] - com).ToList();

                if (indices.Count > 1)
                {
                    plates.Add(new(indices, relativePositions, com));
                }
                else
                {
                    notConnected.Add(indices[0]);
                }
            }
            return new(plates, notConnected.ToList());
        }

        private Dictionary<Edge, EdgeState> FindBrokenEdges(TimeKY timestep)
        {
            var newPositions = new List<Vector3>(Manifold.Values);
            var edgeLengths = CalcEdgeLengths(newPositions, Manifold);

            var brokenEdges = new Dictionary<Edge, EdgeState>();

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

            var threshold = 0.00001f;
            foreach (var l in newLengths)
            {
                var delta = newLengths[l.Key] - edgeLengths[l.Key];
                delta = delta * forceRatio;

                brokenEdges.Add(l.Key, MathF.Abs(delta) > threshold ? 
                    EdgeState.Broken : EdgeState.Intact);
            }

            Thread.Sleep(100);

            return brokenEdges;
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
