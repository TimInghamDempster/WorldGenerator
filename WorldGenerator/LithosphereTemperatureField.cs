using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests.Physics
{
    public class LithosphereTemperatureField : IField<Celsius, float>, ITimeDependent
    {
        private readonly IPlumeSource _plumes;

        public IManifold Manifold { get; }

        public LithosphereTemperatureField(IManifold manifold, IPlumeSource plumes)
        {
            Manifold = manifold;
            _plumes = plumes;
            Values = new float[Manifold.Values.Length];

            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = Constants.AesthenosphereTemperatureC;
            }
        }

        public float[] Values { get; }

        public void ProgressTime(TimeKY timestep)
        {
            var aesthenosphereTemp = Constants.AesthenosphereTemperatureC;
            var sufraceTemp = Constants.SurfaceTemperatureC;

            var aesthenosphereThermalConductivity = 0.1f;
            var surfaceThermalConductivity = 0.9f;

            var temperatureFlowSpeed = 0.055f;

            for(int i = 0; i < Values.Length; i++)
            {
                var cell = Manifold.Values[i];

                var temperature = Values[i];

                var aesthenosphereGradient = (aesthenosphereTemp - temperature) * aesthenosphereThermalConductivity;
                var surfaceGradient = (sufraceTemp - temperature) * surfaceThermalConductivity;

                var gradient = aesthenosphereGradient + surfaceGradient;

                var flow = gradient * temperatureFlowSpeed;

                Values[i] = temperature + flow;

                foreach(var plume in _plumes.Plumes)
                {
                    var distance = Vector3.Distance(cell, plume.Location);

                    if(distance < Constants.PlumeRadiusKm)
                    {
                        Values[i] = aesthenosphereTemp;
                    }
                }
            }
        }
    }
}
