using Microsoft.Xna.Framework;
using Moq;
using WorldGenerator;
using WorldGeneratorFunctionalTests.Physics;
using WorldGeneratorFunctionalTests.Utils;

namespace WorldGeneratorFunctionalTests
{
    public class MantlePlumeWarmsLithosphere : FunctionalTest
    {
        private readonly LithosphereTemperatureField _temperatureField;
        const int _meshSize = 100;

        public MantlePlumeWarmsLithosphere()
        {
            _mesh = Mesh.Plane(_meshSize, 10);
            _manifold = new PointCloudManifold(_mesh.Vertices.ToArray(), _mesh.Faces);

            var plumeSource = new Mock<IPlumeSource>();
            plumeSource.SetupGet(x => x.Plumes).Returns(new List<Plume>()
            {
                new Plume(new Vector3(0, 0, 0))
            });

            _temperatureField = new(_manifold, plumeSource.Object);

            var baseTemp = _temperatureField.Values.Average();
            _seriesData.Add(baseTemp);

            var timesteps = 100;

            _criteria = new TestCriteria(timesteps, TimeoutResult.TimedOut, new List<ICondition>()
            {
               new Should(BeHotAtCentre, "Should be hot at centre"),
               new Should(BeColdAtEdge, "Should be cold at edge"),
            });

            _colors = Rendering.TemperatureGradient(_temperatureField, _manifold);

            _fieldGroup = new FieldGroup(new List<ITimeDependent>
            {
                _temperatureField,
                (ITimeDependent)_colors
            });
        }

        public override float ZoomModifier => 100;

        private bool BeColdAtEdge() =>
            _temperatureField.Values[0] < Constants.AesthenosphereTemperatureC * 0.25f;

        private bool BeHotAtCentre()
        {
            var size = _meshSize - 1;
            var index = size * _meshSize / 2 + _meshSize / 2;

            return _temperatureField.Values[index] > Constants.AesthenosphereTemperatureC * 0.75f;
        }
    }
}
