using FluentAssertions;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class FieldTests
    {
        [TestMethod]
        public void SimpleDiffusionDiffusesSimply()
        {
            // Arrange
            var initialData = new float[10];
            var random = new Random();
            var testVal = (float)random.NextDouble() * 100000.0f;
            initialData[0] = testVal;
            var initialField = new SimpleField<Mm ,float>(
                initialData,
                new EuclideanManifold1d(initialData.Length));

            // Act
            for(int i = 0; i < 1000; i++) 
            {
                initialField = FieldOperators.DiffuseSimple(initialField);
            }

            // Assert
            foreach (var val in initialField.Values)
            {
                val.Should().BeGreaterThan(testVal / 10.1f);
                val.Should().BeLessThan(testVal / 9.9f);
            }
        }
    }
}