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

            var timesteps = (int)((Constants.OceanLithosphereMimimumLifespanKY / _timestepKY) * 1.05);

            _criteria = new TestCriteria(timesteps, TimeoutResult.TimedOut, new List<ICondition>()
            {
                new Should(CooledToCriticalTemperature, "Plate Cooled Over Time"),
                new ShouldNot(CooledToCriticalTemperatureTooFast, "Plate Cooled Too Fast"),
            });
        }

        private bool CooledToCriticalTemperatureTooFast() =>
            CooledToCriticalTemperature() && 
            FrameCount * _timestepKY < 
            Constants.OceanLithosphereMimimumLifespanKY;

        private bool CooledToCriticalTemperature() =>
            _temperatureField.Values.All(t => t < Constants.LithospereCriticalTemperatureC);
    }
}
