using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class GravityField : IContinousField<TN, Vector3>, IDiscreteField<TN, Vector3>
    {
        public float _TN { get; init; }


        public IManifold Manifold { get; init; }

        public int ValueCount => throw new NotImplementedException();

        public IEnumerable<Vector3> Values => throw new NotImplementedException();

        public GravityField(float TN, IManifold manifold)
        {
            _TN = TN;
            Manifold = manifold;
        }

        public Vector3 Value(Vector3 position)
        {
            var dir = position;
            
            // point towards planet centre
            dir *= -1.0f;

            dir.Normalize();
            dir *= _TN;

            return dir;
        }

        public Vector3 Value(int index) => Value(Manifold.Value(index));
    }
}