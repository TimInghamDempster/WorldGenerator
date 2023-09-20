using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorTests
{
    [TestClass]
    public class ColourGradientTests
    {
        private Color RandomColor()
        {
            var random = new Random();
            return new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
        }

        [TestMethod]
        public void BelowStartOfGradientIsFirstColour()
        {
            var firstColour = RandomColor();
            var secondColour = RandomColor();
            var gradient = new ColourGradient(new GradientPoint[]
            {
                new GradientPoint(0.5f, firstColour),
                new GradientPoint(1.0f, secondColour)
            });

            Assert.AreEqual(firstColour, gradient.ColourAt(0.0f));
        }

        [TestMethod]
        public void AboveEndOfGradientIsLastColour()
        {
            var firstColour = RandomColor();
            var secondColour = RandomColor();
            var gradient = new ColourGradient(new GradientPoint[]
            {
                new GradientPoint(0.5f, firstColour),
                new GradientPoint(1.0f, secondColour)
            });

            Assert.AreEqual(secondColour, gradient.ColourAt(2.0f));
        }

        [TestMethod]
        public void MiddleOfGradientIsInterpolatedColour()
        {
            var firstColour = RandomColor();
            var secondColour = RandomColor();
            var gradient = new ColourGradient(new GradientPoint[]
            {
                new GradientPoint(0.5f, firstColour),
                new GradientPoint(1.0f, secondColour)
            });

            var expectedColour = Color.Lerp(firstColour, secondColour, 0.5f);
            Assert.AreEqual(expectedColour, gradient.ColourAt(0.75f));
        }

        [TestMethod]
        public void GradientWithOnePointIsThatPoint()
        {
            var firstColour = RandomColor();
            var gradient = new ColourGradient(new GradientPoint[]
            {
                new GradientPoint(0.5f, firstColour)
            });

            Assert.AreEqual(firstColour, gradient.ColourAt(0.5f));
        }

        [TestMethod]
        public void GradientWithTwoPointsIsInterpolatedBetweenThosePoints()
        {
            var firstColour = RandomColor();
            var secondColour = RandomColor();
            var gradient = new ColourGradient(new GradientPoint[]
            {
                new GradientPoint(0.5f, firstColour),
                new GradientPoint(1.0f, secondColour)
            });

            var expectedColour = Color.Lerp(firstColour, secondColour, 0.5f);
            Assert.AreEqual(expectedColour, gradient.ColourAt(0.75f));
        }

        [TestMethod]
        public void GradientWithThreePointsIsInterpolatedBetweenThosePoints()
        {
            var firstColour = RandomColor();
            var secondColour = RandomColor();
            var thirdColour = RandomColor();
            var gradient = new ColourGradient(new GradientPoint[]
            {
                new GradientPoint(0.25f, firstColour),
                new GradientPoint(0.5f, secondColour),
                new GradientPoint(1.0f, thirdColour)
            });

            var expectedColour = Color.Lerp(secondColour, thirdColour, 0.5f);
            Assert.AreEqual(expectedColour, gradient.ColourAt(0.75f));

            expectedColour = Color.Lerp(firstColour, secondColour, 0.5f);
            Assert.AreEqual(expectedColour, gradient.ColourAt(0.375f));
        }
    }
}
