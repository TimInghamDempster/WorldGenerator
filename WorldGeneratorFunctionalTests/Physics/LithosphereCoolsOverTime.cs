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

            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _temperatureField
            });

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
