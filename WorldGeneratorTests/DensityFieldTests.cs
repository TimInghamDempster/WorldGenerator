using FluentAssertions;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class DensityFieldTests
    {
        // This might be too simple a behaviour, but it's
        // a start
        [TestMethod]
        public void DensityIncreasesLinearlyWithTime()
        {
            // Arrange
            var manifold = new PointCloudManifold(new Position[1]);
            var densityIncreaseRate = new DensityChange(0.1f);
            var densityFieid = new DensityField(
                manifold, new[] { new Density(0.0f) }, densityIncreaseRate);

            // Act
            densityFieid.ProgressTime(new Time(10.0f));

            // Assert
            densityFieid.Values[0].Value.Should().Be(1.0f);
        }
    }
}
