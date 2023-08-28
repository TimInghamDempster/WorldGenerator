using FluentAssertions;
using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class ManifoldManipulatorTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IntegrateRejectsOtherManifold()
        {
            // Arrange
            var points = new Vector3[1];
            var manifold = new PointCloudManifold(points, new Face[0]);
            var manifold2 = new PointCloudManifold(points, new Face[0]);
            var manipulator = new ManifoldManipulator(manifold, 
                new FuncField<Mm, Vector3>(manifold2, (_,_) => Vector3.Zero));
            
            var velocities = new SimpleField<MmPerKy, Vector3>(new Vector3[1], manifold2);

            // Act
            manipulator.ProgressTime(new TimeKY(1));
        }

        [TestMethod]
        public void AppliesVelocity()
        {
            // Arrange
            var random = new Random();

            var startPos = Misc.RandomVector(random);
            var points = new Vector3[1] { startPos };

            var vel = Misc.RandomVector(random);
            var timestepCount = random.Next(1000);
            var timestep = (float)random.NextDouble();

            // S = ut + 1/2at^2, with a = 0
            var endPos = startPos + vel * timestepCount * timestep;

            var manifold = new PointCloudManifold(points, new Face[0]);

            var velocities = new FuncField<Mm, Vector3>(
                manifold,
                (_,p) => p + vel * timestep);
            var manipulator = new ManifoldManipulator(manifold, velocities);


            // Act
            for (int i = 0; i < timestepCount; i++)
            {
                manipulator.ProgressTime(new TimeKY(timestep));
            }

            // Assert
            var delta = (manifold.Values[0] - endPos);
            delta.Length().Should().BeLessThan(endPos.Length() / 100.0f);
        }
    }
}
