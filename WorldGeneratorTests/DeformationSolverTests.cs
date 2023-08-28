using FluentAssertions;
using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class DeformationSolverTests
    {
        [TestMethod]
        public void AngleConstraint()
        {
            var mesh = new Mesh(new Face[]
            {
                new Face(0, 1, 2)
            }, new Vector3[]
            {
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0)
            });

            var initialAngles = DeformationSolver.VertexCombinations.Select(
                c => DeformationSolver.CalcAngle(mesh.Vertices, c[0], c[1], c[2])).
                ToList();

            var manifold = new PointCloudManifold(mesh.Vertices.ToArray(), mesh.Faces);

            var defSolver = new DeformationSolver(manifold, )

            var initialNorm = Vector3.Cross(
                manifold.Values[1] - manifold.Values[0],
                manifold.Values[2] - manifold.Values[0]);

            manifold.Values[0] = new Vector3(0.3f, 0, 1);

            var midAngles = DeformationSolver.VertexCombinations.Select(
                c => DeformationSolver.CalcAngle(manifold.Values, c[0], c[1], c[2])).
                ToList();

            DeformationSolver.ApplyAngleConstraint(mesh.Vertices, mesh.Faces, new(new()));

            var endAngles = DeformationSolver.VertexCombinations.Select(
                c => DeformationSolver.CalcAngle(manifold.Values, c[0], c[1], c[2])).
                ToList();

            var finalNorm = Vector3.Cross(
                manifold.Values[1] - manifold.Values[0],
                manifold.Values[2] - manifold.Values[0]);

            for (int i = 0; i < initialAngles.Count(); i++)
            {
                var diff = MathF.Abs(endAngles[i] - initialAngles[i]);
                diff.Should().BeLessThan(0.001f);
            }

            Vector3.Dot(initialNorm, finalNorm).Should().BeGreaterThan(0.0f);
        }
    }
}
