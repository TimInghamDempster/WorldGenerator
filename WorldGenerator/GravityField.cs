namespace WorldGenerator
{
    public class GravityField : IContinousField<Velocity>
    {
        public float _g_mPerS { get; init; }

        public GravityField(float g_mPerS)
        {
            _g_mPerS = g_mPerS;
        }

        public Velocity Value(Position position)
        {
            var dir = position.Value;
            
            // point towards planet centre
            dir *= -1.0f;

            dir.Normalize();
            dir *= _g_mPerS;

            return new(dir);
        }
    }
}