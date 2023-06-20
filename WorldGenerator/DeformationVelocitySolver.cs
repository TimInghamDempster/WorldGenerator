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

        public Vector3[] Values {get; }

        public void ProgressTime(TimeKY timestep)
        {
            var newPositions = new List<Vector3>(Manifold.Values);

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

            var threshold = 0.005f;
            for(int i = 0; i < Manifold.Values.Length; i++)
            {
                var force = newPositions[i] - Manifold.Values[i];
                if(force.Length() * forceRatio < threshold)
                {
                    newPositions[i] = Manifold.Values[i];
                }
                else
                {
                    newPositions[i] = Manifold.Values[i] + force * forceRatio * timestep.Value;
                }
            }

            for (int i = 0; i < Manifold.Values.Length; i++)
            {
                Manifold.Values[i] = newPositions[i];
            }
        }

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
