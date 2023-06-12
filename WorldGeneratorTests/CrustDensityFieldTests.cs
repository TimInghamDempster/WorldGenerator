using FluentAssertions;
using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class CrustDensityFieldTests
    {
        // This might be too simple a behaviour, but it's
        // a start
        [TestMethod]
        public void DensityIncreasesLinearlyWithTime()
        {
            // Arrange
            var manifold = new PointCloudManifold(new Vector3[1], new Face[0]);
            var densityIncreaseRate = new DensityChange(0.1f);
            var densityFieid = new CrustDensityField(
                manifold, new[] { 0.0f }, densityIncreaseRate);

            // Act
            densityFieid.ProgressTime(new TimeKY(10.0f));

            // Assert
            densityFieid.Value(0).Should().Be(1.0f);
        }
    }
}
