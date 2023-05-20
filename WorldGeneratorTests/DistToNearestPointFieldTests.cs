﻿using FluentAssertions;
using Microsoft.Xna.Framework;
using Moq;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class DistToNearestPointFieldTests
    {
        [TestMethod]
        public void ReturnsCorrectDistance()
        {
            // Arrange
            var testPoint = new Vector3(1, 1, 1);
            var nearestPoint = new Vector3(2, 2, 2);
            var manifold = new Mock<IManifold>();
            manifold.Setup(m => m.NearestPoint(It.IsAny<Vector3>())).Returns(nearestPoint);

            var distField = new DistToNearestPointField(manifold.Object);

            // Act
            var dist = distField.Value(testPoint);

            // Assert
            dist.Should().Be(MathF.Sqrt(3));
        }
    }
}
