namespace WorldGenerator
{
    public class GravityField : IContinousField<Force>, IDiscreteField<Force>
    {
        public float _TN { get; init; }


        public IManifold Manifold { get; init; }

        public int ValueCount => throw new NotImplementedException();

        public GravityField(float TN, IManifold manifold)
        {
            _TN = TN;
            Manifold = manifold;
        }

        public Force Value(Position position)
        {
            var dir = position.Value;
            
            // point towards planet centre
            dir *= -1.0f;

            dir.Normalize();
            dir *= _TN;

            return new(dir);
        }

        public Force Values(int index) => Value(Manifold.Values(index));
    }
}