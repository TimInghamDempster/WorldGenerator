using Microsoft.Xna.Framework;
using static WorldGenerator.DeformationSolver;

namespace WorldGenerator
{
    public class PointCloudManifold : IManifold
    {
        public IManifold Manifold => this;

        public Vector3[] Values { get; }

        public Dictionary<int, Neighbours> Neighbours { get; }

        public Connectivity Connectivity { get; }

        public Dictionary<int, FaceGroup> Faces { get;}

        public PointCloudManifold(Vector3[] positions, IEnumerable<Face> faces)
        {
            Values = positions;
            Neighbours =
                positions.
                Select((_, i) => (i, new Neighbours(
                    faces.Where(f => f.Indices.Contains(i)).
                    SelectMany(f => f.Indices.Where(fi => fi != i)).Distinct().ToArray()))).
                    ToDictionary(kvp => kvp.i, kvp => kvp.Item2);

            Connectivity = new(new HashSet<Edge>(
                faces.
                SelectMany(f => 
                    f.Indices.
                    SelectMany(i => 
                        f.Indices.
                        Where(j => j > i).
                        Select(j => new Edge(i, j))))));

            Faces = new();
            for(int i = 0; i < Values.Length; i++)
            {
                var faceGroup = faces.Where(f => f.Indices.Contains(i)).ToList();
                Faces.Add(i, new FaceGroup(faceGroup));
            }    
        }
    }
}
