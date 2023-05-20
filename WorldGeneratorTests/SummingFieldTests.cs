using FluentAssertions;
using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class SummingFieldTests
    {
        [TestMethod]
        public void SummingFieldSumsFields()
        {
              // Arrange
            var random = new Random();

            var manifold = new PointCloudManifold(new Vector3[1] { Vector3.Zero });

            var field1 = new SimpleField<TN, Vector3>(
                new Vector3[1] { Misc.RandomVector(random) }, manifold);

            var field2 = new SimpleField<TN, Vector3>(
                new Vector3[1] { Misc.RandomVector(random)}, manifold);

            var summingField = new DiscreteSummingField<TN>(new IDiscreteField<TN, Vector3>[] { field1, field2 });

            // Act
            var sum = summingField.Value(0);
            var sum2 = summingField.Values.First();

            // Assert
            sum.Should().Be(field1.Value(0) + field2.Value(0));
            sum2.Should().Be(field1.Value(0) + field2.Value(0));
        }

        [TestMethod]
        public void SumsWhenFieldsChange()
        {
              // Arrange
            var random = new Random();

            var manifold = new PointCloudManifold(new Vector3[1] { Vector3.Zero });

            var field1 = new SimpleField<TN, Vector3>(
                new Vector3[1] { Misc.RandomVector(random) }, manifold);

            var field2 = new SimpleField<TN, Vector3>(
                new Vector3[1] { Misc.RandomVector(random) }, manifold);

            var summingField = new DiscreteSummingField<TN>(new IDiscreteField<TN, Vector3>[] { field1, field2 });

            // Act
            field1.SetValue(0, Misc.RandomVector(random));
            field2.SetValue(0, Misc.RandomVector(random));

            // Assert
            summingField.Value(0).Should().Be(field1.Value(0) + field2.Value(0));
            summingField.Values.First().Should().Be(field1.Value(0) + field2.Value(0));
        }

        [TestMethod]
        public void SumsFieldsWithMultipleValues()
        {
            // Arrange
            var random = new Random();

            var manifold = new PointCloudManifold(new Vector3[2] { Vector3.Zero, Vector3.UnitX });

            var field1 = new SimpleField<TN, Vector3>(
                               new Vector3[2] { Misc.RandomVector(random), Misc.RandomVector(random) }, manifold);

            var field2 = new SimpleField<TN, Vector3>(
                               new Vector3[2] { Misc.RandomVector(random), Misc.RandomVector(random) }, manifold);

            var summingField = new DiscreteSummingField<TN>(new IDiscreteField<TN, Vector3>[] { field1, field2 });

            // Act
            var sum = summingField.Value(0);
            var sum2 = summingField.Value(1);

            // Assert
            sum.Should().Be(field1.Value(0) + field2.Value(0));
            sum2.Should().Be(field1.Value(1) + field2.Value(1));
        }

        [TestMethod]
        public void RejectsFieldsWithDifferingManifolds()
        {
            // Arrange
            var random = new Random();

            var manifold1 = new PointCloudManifold(new Vector3[1] { Vector3.Zero });
            var manifold2 = new PointCloudManifold(new Vector3[1] { Vector3.Zero });

            var field1 = new SimpleField<TN, Vector3>(
                               new Vector3[1] { Misc.RandomVector(random) }, manifold1);

            var field2 = new SimpleField<TN, Vector3>(
                               new Vector3[1] { Misc.RandomVector(random) }, manifold2);

            // Act
            Action act = () => new DiscreteSummingField<TN>(new IDiscreteField<TN, Vector3>[] { field1, field2 });

            // Assert
            act.Should().Throw<ArgumentException>();    
        }

        [TestMethod]
        public void ReturnsUnderlyingManifold()
        {
            // Arrange
            var random = new Random();

            var manifold = new PointCloudManifold(new Vector3[1] { Vector3.Zero });

            var field1 = new SimpleField<TN, Vector3>(
                               new Vector3[1] { Misc.RandomVector(random) }, manifold);

            var field2 = new SimpleField<TN, Vector3>(
                               new Vector3[1] { Misc.RandomVector(random) }, manifold);

            var summingField = new DiscreteSummingField<TN>(new IDiscreteField<TN, Vector3>[] { field1, field2 });

            // Act
            var summingFieldManifold = summingField.Manifold;

            // Assert
            summingFieldManifold.Should().Be(manifold);
        }

        [TestMethod]
        public void HasCorrectNumberOfPoints()
        {
            // Arrange
            var random = new Random();

            var manifold = new PointCloudManifold(new Vector3[2] { Vector3.Zero, Vector3.Zero });

            var field1 = new SimpleField<TN, Vector3>(new Vector3[2] 
            { 
                Misc.RandomVector(random), 
                Misc.RandomVector(random) 
            },
                manifold);

            var field2 = new SimpleField<TN, Vector3>(new Vector3[2] 
            { 
                Misc.RandomVector(random), 
                Misc.RandomVector(random) 
            }, manifold);

            // Act
            var summingField = new DiscreteSummingField<TN>(
                new IDiscreteField<TN, Vector3>[] { field1, field2 });

            // Assert
            summingField.Values.Count().Should().Be(2);
            summingField.ValueCount.Should().Be(2);
        }
    }
}
