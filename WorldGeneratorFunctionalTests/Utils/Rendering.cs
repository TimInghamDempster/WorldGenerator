using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests.Utils
{
    internal static class Rendering
    {
        public static IField<Unitless, Color> TemperatureGradient(IField<Celsius, float> temperatureField, IManifold manifold)
        {
            var firstPoint = new GradientPoint(Constants.SurfaceTemperatureC, Color.DarkBlue);
            var lastPoint = new GradientPoint(Constants.AesthenosphereTemperatureC, Color.DarkRed);
            var midPoint = new GradientPoint(
                (Constants.SurfaceTemperatureC + Constants.AesthenosphereTemperatureC) * 0.5f,
                Color.White);
            var earlyPoint = new GradientPoint(
                Constants.SurfaceTemperatureC * 0.75f + Constants.AesthenosphereTemperatureC * 0.25f,
                Color.Blue);
            var latePoint = new GradientPoint(
                Constants.SurfaceTemperatureC * 0.25f + Constants.AesthenosphereTemperatureC * 0.75f,
                Color.Red);

            var colors =
                new ColourField<Celsius>(manifold, temperatureField,
                new(new[]
                {
                    firstPoint,
                    earlyPoint,
                    midPoint,
                    latePoint,
                    lastPoint
                }));

            return colors;
        }
    }
}
