﻿using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class PointCloudManifold : IManifold
    {
        public IManifold Manifold => this;

        public Vector3[] Values { get; }

        public Dictionary<int, Neighbours> Neighbours { get; }

        public PointCloudManifold(Vector3[] positions, IEnumerable<Face> faces)
        {
            Values = positions;
            Neighbours =
                positions.
                Select((_, i) => (i, new Neighbours(
                    faces.Where(f => f.Indices.Contains(i)).
                    SelectMany(f => f.Indices.Where(fi => fi > i)).ToArray()))).
                    ToDictionary(kvp => kvp.i, kvp => kvp.Item2);   
        }
    }
}
