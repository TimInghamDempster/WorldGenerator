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
    }
}
