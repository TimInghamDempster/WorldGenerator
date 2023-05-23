using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class BouyancyField : IContinousField<TN, Vector3>, IDiscreteField<TN, Vector3>
    {
        public int ValueCount => throw new NotImplementedException();

        public IManifold Manifold => throw new NotImplementedException();

        public IEnumerable<Vector3> Values => throw new NotImplementedException();

        public Vector3 Value(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public Vector3 Value(int index)
        {
            throw new NotImplementedException();
        }
    }
}
