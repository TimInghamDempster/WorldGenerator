﻿using FluentAssertions;
using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class BouyantVelocityFieldTests
    {
        [TestMethod]
        public void HeavyCrustSinks()
        {
            // Arrange
            var points = new Vector3[1] { Vector3.UnitX };
            var manifold = new PointCloudManifold(points);
            var density = new SimpleField<GTPerKm3, float>(new[] { Constants.OceanCrustDensityGTPerKm3 * 1.3f }, manifold);

            // Act
            var velocities = new BouyantVelocityField(manifold, density);

            // Assert
            Vector3.Dot(
                Vector3.Normalize(velocities.Value(0)),
                Vector3.Normalize(points[0]))
                .Should().Be(-1.0f);
        }

        [TestMethod]
        public void LightCrustFloats()
        {
            // Arrange
            var points = new Vector3[1];
            var manifold = new PointCloudManifold(points);
            var density = new SimpleField<GTPerKm3, float>(new[] { Constants.OceanCrustDensityGTPerKm3 * 0.9f }, manifold);

            // Act
            var velocities = new BouyantVelocityField(manifold, density);

            // Assert
            velocities.Value(0).Should().Be(Vector3.Zero);
        }
    }
}
