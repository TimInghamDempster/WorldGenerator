using FluentAssertions;
using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class PointManifoldTests
    {
        [TestMethod]
        public void PointCloudManifoldHasNoNeighbours()
        {
            // Arrange
            var points = new Vector3[1];
            var manifold = new PointCloudManifold(points, new Face[0]);

            // Act
            var neighbours = manifold.Neighbours[0];

            // Assert
            neighbours.Indices.Should().BeEmpty();
        }

        [TestMethod]
        public void PointCloudManifoldHasNoEdges()
        {
            // Arrange
            var points = new Vector3[1];
            var manifold = new PointCloudManifold(points, new Face[0]);

            // Act
            var edges = manifold.Edges;

            // Assert
            edges.Should().BeEmpty();
        }

        [TestMethod]
        public void GeneratesCorrectEdges()
        {
            // Arrange

            var p1 = new Vector3(0.0f, 0.0f, 0.0f);
            var p2 = new Vector3(1.0f, 0.0f, 0.0f);
            var p3 = new Vector3(0.0f, 1.0f, 0.0f);
            var points = new Vector3[3] { p1, p2, p3 };
            var manifold = new PointCloudManifold(points, new Face[1] {new Face(0,1,2)});

            // Act
            var edges = manifold.Edges;

            // Assert
            edges.Should().Contain(p2 - p1);
            edges.Should().Contain(p3 - p2);
            edges.Should().Contain(p3 - p1);
            edges.Count().Should().Be(3);
        }
    }
}
