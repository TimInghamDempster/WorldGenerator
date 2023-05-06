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

            var points = new List<Position>();
            for (int i = 0; i < 100; i++)
            {
                var vec = new Position(RandomVector(random), Unit.None);
                points.Add(vec);
            }
            var manifold = new PointCloudManifold(points);

            var closestPoint = points[random.Next(points.Count)];
            var testPoint = new Position(closestPoint.Value + new Vector3(0.003f, 0.0f, 0.0f), Unit.None);

            // Act
            var returnedPoint = manifold.NearestPoint(testPoint);
            
            // Assert
            returnedPoint.Should().Be(closestPoint);
        }

        private Vector3 RandomVector(Random random) =>
            new((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
    }
}
