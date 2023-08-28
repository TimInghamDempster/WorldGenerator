using FluentAssertions;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class DeformationSolverTests
    {
        [TestMethod]
        public void SeparatesParts()
        {
            // Arrange
            var mesh = Mesh.Plane(2);
            var manifold = new PointCloudManifold(mesh.Vertices.ToArray(), mesh.Faces);

            var brokenEdges = manifold.Connectivity.Edges.Where(
                e => mesh.Vertices[e.Index1].Z != mesh.Vertices[e.Index2].Z);

            // Act
            var parts = DeformationSolver.SeparateParts(manifold.Values, manifold.Connectivity, brokenEdges);

            // Assert
            parts.Count().Should().Be(3);
            parts[0].Indices.Should().BeEquivalentTo(new[] { 0, 1, 2});
            parts[1].Indices.Should().BeEquivalentTo(new[] { 3, 4, 5});
            parts[2].Indices.Should().BeEquivalentTo(new[] { 6, 7, 8 });
        }
    }
}
