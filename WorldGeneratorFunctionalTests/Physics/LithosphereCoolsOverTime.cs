using Microsoft.Xna.Framework;
using WorldGenerator;
using WorldGeneratorFunctionalTests.Physics;

namespace WorldGeneratorFunctionalTests
{
    public class LithosphereCoolsOverTime : FunctionalTest
    {
        private readonly LithosphereTemperatureField _temperatureField;
        private readonly int _timestepKY = 1000;

        public LithosphereCoolsOverTime()
        {
            _mesh = Mesh.Plane(10);
            _manifold = new PointCloudManifold(_mesh.Vertices.ToArray(), _mesh.Faces);

            _temperatureField = new(_manifold);

            var baseTemp = _temperatureField.Values.Average();
            _seriesData.Add(baseTemp);

            var halfLife = 15;

            var timesteps = 100;

            _criteria = new TestCriteria(timesteps, TimeoutResult.Completed, new List<ICondition>()
            {
                new ShouldNot(() => AboveTempAfterTime(baseTemp / 2, halfLife + 1), "First half life cool"),
                new ShouldNot(() => BelowTempBeforeTime(baseTemp / 2, halfLife), "First half life hot"),
                new ShouldNot(() => AboveTempAfterTime(baseTemp / 4, halfLife * 2 + 2), "Second half life cool"),
                new ShouldNot(() => BelowTempBeforeTime(baseTemp / 4, halfLife * 2), "Second half life hot"),
            });

            _colors = TemperatureGradient(_temperatureField, _manifold);

            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _temperatureField,
                (ITimeDependent)_colors
            });
        }

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

        private bool BelowTempBeforeTime(float temp, int time) =>
            _seriesData.Select((t, i) => (t, i)).Any(v => v.t < temp && v.i < time);
        private bool AboveTempAfterTime(float temp, int time) =>
            _seriesData.Select((t, i) => (t, i)).Any(v => v.t > temp && v.i > time);

        public override void PostUpdate() =>
            _seriesData.Add(_temperatureField.Values.Average());

        private bool CooledToCriticalTemperatureTooFast() =>
            CooledToCriticalTemperature() && 
            FrameCount * _timestepKY < 
            Constants.OceanLithosphereMimimumLifespanKY;

        private bool CooledToCriticalTemperature() =>
            _temperatureField.Values.All(t => t < Constants.LithospereCriticalTemperatureC);
    }
}
