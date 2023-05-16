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
            var points = new Position[1];
            var manifold = new PointCloudManifold(points);
            var manifold2 = new PointCloudManifold(points);

            var forces = new SimpleField<Force>(
                new Force[1] { new(Vector3.Zero) },
                manifold);

            var velocities = new VelocityField(manifold2, new Velocity[1] { new(Vector3.Zero) });

            // Act
            velocities.ProgressTime(new Time(1), forces);
        }

        [TestMethod]
        public void FEqualsMa()
        {
            // Arrange
            var random = new Random();

            var startVel = RandomVector(random);
            var timestepCount = random.Next(1000);
            var timestep = (float)random.NextDouble();

            var force = RandomVector(random);

            // S = ut + 1/2at^2, with a = 0
            var endVel= startVel + force * timestepCount * timestep;

            var manifold = new PointCloudManifold(new Position[1] {new Position()});

            var forces = new SimpleField<Force>(
                new Force[1] { new(force) },
                manifold);

            var velocities = new VelocityField(manifold, new Velocity[1] { new(startVel) });

            // Act
            for (int i = 0; i < timestepCount; i++)
            {
                velocities.ProgressTime(new Time(timestep), forces);
            }

            // Assert
            var delta = (velocities.Values(0).Value - endVel);
            delta.Length().Should().BeLessThan(endVel.Length() / 100.0f);
        }
    }
}
