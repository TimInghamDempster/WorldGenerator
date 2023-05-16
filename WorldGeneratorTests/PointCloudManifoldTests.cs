using FluentAssertions;
using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class PointCloudManifoldTests
    {
        [TestMethod]
        public void FindsClosestPoint()
        {
            // Arrange
            var random = new Random();

            var points = new Position[100];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Position(RandomVector(random));
            }
            var manifold = new PointCloudManifold(points);

            var closestPoint = points[random.Next(points.Count())];
            var testPoint = new Position(closestPoint.Value + new Vector3(0.003f, 0.0f, 0.0f));

            // Act
            var returnedPoint = manifold.NearestPoint(testPoint);
            
            // Assert
            returnedPoint.Should().Be(closestPoint);
        }

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

            var velocities = new SimpleField<Velocity>(new Velocity[1], manifold2);

            // Act
            manifold.ProgressTime(velocities, new Time(1));
        }

        [TestMethod]
        public void MomentumExists()
        {
            // Arrange
            var random = new Random();

            var startPos = RandomVector(random);
            var points = new Position[1] { new(startPos) };

            var vel = RandomVector(random);
            var timestepCount = random.Next(1000);
            var timestep = (float)random.NextDouble();

            // S = ut + 1/2at^2, with a = 0
            var endPos = startPos + vel * timestepCount * timestep;

            var manifold = new PointCloudManifold(points);

            var velocities = new VelocityField(
                manifold,
                new Velocity[1] { new(vel) });

            // Act
            for (int i = 0; i < timestepCount; i++)
            {
                manifold.ProgressTime(velocities, new Time(timestep));
            }

            // Assert
            var delta = (manifold.Values(0).Value - endPos);
            delta.Length().Should().BeLessThan(endPos.Length() / 100.0f);
        }
    }
}
