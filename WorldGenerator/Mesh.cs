using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public record Face(int Index1, int Index2, int Index3)
    {
        public IEnumerable<int> Indices
        {
            get
            {
                yield return Index1;
                yield return Index2;
                yield return Index3;
            }
        }
    }

    public record Mesh(IReadOnlyList<Face> Faces, IReadOnlyList<Vector3> Vertices)
    {
        internal static Mesh Cube(float size)
        {
            var verts = new List<Vector3>();
            var faces = new List<Face>();

            verts.Add(new Vector3(-size, -size, size));
            verts.Add(new Vector3(size, -size, size));
            verts.Add(new Vector3(-size, size, size));
            verts.Add(new Vector3(size, size, size));


            verts.Add(new Vector3(-size, -size, -size));
            verts.Add(new Vector3(size, -size, -size));
            verts.Add(new Vector3(-size, size, -size));
            verts.Add(new Vector3(size, size, -size));

            faces.Add(new(2, 1, 0));
            faces.Add(new(1, 2, 3));
            faces.Add(new(4, 5, 6));
            faces.Add(new(7, 6, 5));

            faces.Add(new(0, 1, 4));
            faces.Add(new(4, 1, 5));
            faces.Add(new(6, 3, 2));
            faces.Add(new(3, 6, 7));

            faces.Add(new(4, 2, 0));
            faces.Add(new(2, 4, 6));
            faces.Add(new(1, 3, 5));
            faces.Add(new(7, 5, 3));

            return new(faces, verts);
        }

        enum FaceType
        {
            SideFace,
            EndCap
        }

        internal static Mesh FromPointSphere(int resolution, Func<Vector3, float> altitude)
        {
            var verts = new List<Vector3>();
            var faces = new List<Face>();

            var halfRes = resolution / 2;

            for (int i = 0; i < 4; i++)
            {
                var rot = Matrix.CreateFromAxisAngle(Vector3.UnitY, MathF.PI * i / 2.0f);
                BuildCubeFaceVerts(verts, halfRes, rot);
            }
            var rot2 = Matrix.Identity;
            BuildEndCapVerts(verts, halfRes, rot2);
            rot2 = Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI);
            BuildEndCapVerts(verts, halfRes, rot2);

            for (int i = 0; i < 6; i++)
            {
                var faceOffset = i * (resolution + 1) * (resolution + 1);
                for (int y = 0; y < resolution; y++)
                {
                    for (int x = 0; x < resolution; x++)
                    {
                        var v1 = (x + 0) + (y + 0) * (resolution + 1) + faceOffset;
                        var v2 = (x + 1) + (y + 0) * (resolution + 1) + faceOffset;
                        var v3 = (x + 0) + (y + 1) * (resolution + 1) + faceOffset;
                        var v4 = (x + 1) + (y + 1) * (resolution + 1) + faceOffset;

                        faces.Add(new(v1, v2, v3));
                        faces.Add(new(v2, v4, v3));
                    }
                }
            }

            return new(faces, verts);
        }

        private static void BuildCubeFaceVerts(List<Vector3> verts, int halfRes, Matrix rot)
        {
            for (int y = -halfRes; y <= halfRes; y++)
            {
                for (int x = -halfRes; x <= halfRes; x++)
                {
                    var theta = (float)x / (float)halfRes * MathF.PI / 4.0f;
                    var phi = (float)y / (float)halfRes * MathF.PI / 4.0f;
                    var unrot = new Vector3(MathF.Cos(theta) * MathF.Cos(phi), MathF.Sin(phi), MathF.Sin(theta) * MathF.Cos(phi));
                    var rv = Vector3.Transform(unrot, rot);
                    verts.Add(rv);
                }
            }
        }
        private record Int2(int X, int Y);
        private static void BuildEndCapVerts(List<Vector3> verts, int halfRes, Matrix rot)
        {
            for (int y = -halfRes; y <= halfRes; y++)
            {
                for (int x = -halfRes; x <= halfRes; x++)
                {
                    const float p2 = 1.5707963267948966f; // Pi / 2
                    var gridTheta = MathF.Atan2(x, y) - p2 / 2.0f;

                    int offset = gridTheta switch
                    {
                        <= p2 * -2.0f => 3,
                        <= p2 * -1.0f => 0,
                        <= p2 * 0.0f => 1,
                        <= p2 * 1.0f => 2,
                        <= p2 * 2.0f => 3,
                        _ => throw new NotImplementedException()
                    };

                    Int2 angularId = offset switch
                    {
                        0 => new(y, Math.Abs(x)),
                        1 => new(x, Math.Abs(y)),
                        2 => new(-y, Math.Abs(x)),
                        3 => new(-x, Math.Abs(y)),
                        _ => throw new NotImplementedException()
                    };

                    var theta = (offset * (MathF.PI / 2.0f)) + ((float)angularId.X / ((float)angularId.Y) * (MathF.PI / 4.0f));
                    var phi = (float)angularId.Y / (float)halfRes * MathF.PI / 4.0f;

                    if (x == 0 && y == 0) { theta = 0.0f; }

                    var unrot = new Vector3(MathF.Sin(theta) * MathF.Sin(phi), MathF.Cos(phi), MathF.Cos(theta) * MathF.Sin(phi));
                    var rv = Vector3.Transform(unrot, rot);
                    verts.Add(rv);
                }
            }
        }

        public static Mesh Geodesic(float radius, int minFaces)
        {
            var icosahedron = Icosahedron(radius);

            var verts = new List<Vector3>(icosahedron.Vertices);
            var faces = new List<Face>(icosahedron.Faces);

            faces = Subdivide(faces, verts, minFaces, radius);

            return new (faces, verts);
        }

        private static List<Face> Subdivide(List<Face> faces, List<Vector3> verts, int minFaces, float radius)
        {
            while (faces.Count < minFaces)
            {
                var newFaces = new List<Face>();
                foreach (var face in faces)
                {
                    var i0 = face.Index1;
                    var i1 = face.Index2;
                    var i2 = face.Index3;
                    var v0 = verts[i0];
                    var v1 = verts[i1];
                    var v2 = verts[i2];

                    var v20 = (v2 + v0) / 2.0f;
                    var v01 = (v0 + v1) / 2.0f;
                    var v12 = (v1 + v2) / 2.0f;
                    var i20 = verts.Count + 0;
                    var i01 = verts.Count + 1;
                    var i12 = verts.Count + 2;
                    verts.Add(v20);
                    verts.Add(v01);
                    verts.Add(v12);

                    newFaces.Add(new(i0, i01, i20));
                    newFaces.Add(new(i1, i12, i01));
                    newFaces.Add(new(i2, i20, i12));
                    newFaces.Add(new(i2, i20, i12));
                    newFaces.Add(new(i12, i20, i01));
                }

                for (int i = 0; i < verts.Count; i++)
                {
                    verts[i] = Vector3.Normalize(verts[i]) * radius;
                }

                faces = newFaces;
            }

            return faces;
        }

        private static Mesh Icosahedron(float radius)
        {
            float phi = (1.0f + MathF.Sqrt(5.0f)) * 0.5f; // golden ratio
            float a = 1.0f;
            float b = 1.0f / phi;

            // add vertices
            var verts = new List<Vector3>();
            verts.Add(new(0, b, -a));
            verts.Add(new(b, a, 0));
            verts.Add(new(-b, a, 0));
            verts.Add(new(0, b, a));
            verts.Add(new(0, -b, a));
            verts.Add(new(-a, 0, b));
            verts.Add(new(0, -b, -a));
            verts.Add(new(a, 0, -b));
            verts.Add(new(a, 0, b));
            verts.Add(new(-a, 0, -b));
            verts.Add(new(b, -a, 0));
            verts.Add(new(-b, -a, 0));

            for (int i = 0; i < verts.Count; i++)
            {
                verts[i] = Vector3.Normalize(verts[i]) * radius;
            }

            // add triangles
            var faces = new List<Face>();
            faces.Add(new(2, 1, 0));
            faces.Add(new(1, 2, 3));
            faces.Add(new(5, 4, 3));
            faces.Add(new(4, 8, 3));
            faces.Add(new(7, 6, 0));
            faces.Add(new(6, 9, 0));
            faces.Add(new(11, 10, 4));
            faces.Add(new(10, 11, 6));
            faces.Add(new(9, 5, 2));
            faces.Add(new(5, 9, 11));
            faces.Add(new(8, 7, 1));
            faces.Add(new(7, 8, 10));
            faces.Add(new(2, 5, 3));
            faces.Add(new(8, 1, 3));
            faces.Add(new(9, 2, 0));
            faces.Add(new(1, 7, 0));
            faces.Add(new(11, 9, 6));
            faces.Add(new(7, 10, 6));
            faces.Add(new(5, 11, 4));
            faces.Add(new(10, 8, 4));

            return new (faces, verts);
        }

        public static Mesh Plane(int subdivisons)
        {
            var verts = new List<Vector3>();
            var faces = new List<Face>();

            for (int y = 0; y <= subdivisons; y++)
            {
                for (int x = 0; x <= subdivisons; x++)
                {
                    verts.Add(new Vector3(x - subdivisons / 2.0f, 0, y - subdivisons / 2.0f));
                }
            }

            for (int y = 0; y < subdivisons; y++)
            {
                for (int x = 0; x < subdivisons; x++)
                {
                    var i0 = x + y * (subdivisons + 1);
                    var i1 = x + 1 + y * (subdivisons + 1);
                    var i2 = x + (y + 1) * (subdivisons + 1);
                    var i3 = x + 1 + (y + 1) * (subdivisons + 1);

                    if(x % 2 == 0)
                    {
                        faces.Add(new(i0, i2, i1));
                        faces.Add(new(i1, i2, i3));
                    }
                    else
                    {
                        faces.Add(new(i0, i3, i1));
                        faces.Add(new(i0, i2, i3));
                    }
                }
            }

            return new (faces, verts);
        }
    }
}
