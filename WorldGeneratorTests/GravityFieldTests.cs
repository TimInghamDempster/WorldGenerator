using FluentAssertions;
using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorTests
{
    // These tests are largely trivial but at least prove
    // that the basics are working
    [TestClass]
    public class GravityFieldTests
    {
        [TestMethod]
        [DynamicData(nameof(GravityExamples))]
        public void GravityWorks(Vector3 position, Vector3 expected, float strength)
        {
            // Arrange
            var gField = new GravityField(strength, new TempManifold());

            // Act
            var result = gField.Value(position);

            // Assert
            result.Should().Be(expected);
        }

        public static IEnumerable<object[]> GravityExamples
        {
            get
            {
                var strength = 9.81f;
                return new[]
                {
                    new object[] { Vector3.UnitX, Vector3.UnitX * -strength, strength},
                    new object[] { Vector3.UnitY / 2.0f, Vector3.UnitY * -strength, strength},
                };
            }
        }
    }
}
