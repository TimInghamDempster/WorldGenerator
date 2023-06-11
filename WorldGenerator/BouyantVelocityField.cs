using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class BouyantVelocityField : IField<MmPerKy, Vector3>, ITimeDependent
    {
        private readonly IManifold _manifold;
        private readonly IField<GTPerKm3, float> _density;
        private readonly IField<Unitless, Vector3> _gravityDir;

        // TODO: Calibrate this
        private float _sinkRate = 0.1f;

        public BouyantVelocityField(
            IManifold manifold, 
            IField<GTPerKm3, float> density,
            IField<Unitless, Vector3> gravityDir)
        {
            _manifold = manifold;
            _density = density;
            _gravityDir = gravityDir;
            Values = new Vector3[Manifold.Values.Length];
            ProgressTime(new(0));
        }

        public IManifold Manifold => _manifold;

        public Vector3[] Values { get; }

        public void ProgressTime(TimeKY time)
        {
            for(int i = 0; i < Values.Length; i++)
            {
                Values[i] = Value(i);
            }
        }

       private Vector3 Value(int index) =>
            _density.Values[index] < Constants.MantleDensityGTPerKm3 ? 
            new Vector3(0.0f, 0.0f, 0.0f) :
            _gravityDir.Values[index] * _sinkRate;
    }
}
