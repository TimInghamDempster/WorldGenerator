using FluentAssertions;
using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class VelocityFieldTests
    {
        private Vector3 RandomVector(Random random) =>
            new((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IntegrateRejectsOtherManifold()
        {
            // Arrange
            var points = new Vector3[1];
            var manifold = new PointCloudManifold(points);
            var manifold2 = new PointCloudManifold(points);

            var forces = new SimpleField<TN, Vector3>(
                new Vector3[1] { Vector3.Zero },
                manifold);

            var velocities = new VelocityField(manifold2, new Vector3[1] { Vector3.Zero });

            // Act
            velocities.ProgressTime(forces, new Time(1));
        }

        [TestMethod]
        public void VEqualsUPlusAT()
        {
            // Arrange
            var random = new Random();

            var startVel = RandomVector(random);
            var timestepCount = random.Next(1000);
            var timestep = (float)random.NextDouble();

            var force = RandomVector(random);

            // a = f / m, v = u + at
            var endVel= startVel + force * timestepCount * timestep;

            var manifold = new PointCloudManifold(new Vector3[1] {Vector3.Zero});

            var forces = new SimpleField<TN, Vector3>(
                new Vector3[1] { force },
                manifold);

            var velocities = new VelocityField(manifold, new Vector3[1] { startVel });

            // Act
            for (int i = 0; i < timestepCount; i++)
            {
                velocities.ProgressTime(forces, new Time(timestep));
            }

            // Assert
            var delta = (velocities.Value(0) - endVel);
            delta.Length().Should().BeLessThan(endVel.Length() / 100.0f);
        }
    }
}
