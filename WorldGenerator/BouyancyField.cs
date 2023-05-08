using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class BouyancyField : IField<Bouyancy>, IField<IVectorValued>
    {
        public Bouyancy Value(Position position) =>
            position.Value.LengthSquared() < 1.0f ?
                new(Vector3.Normalize(position.Value), Unit.None):
                new (Vector3.Zero, Unit.None);

        IVectorValued IField<IVectorValued>.Value(Position position) => Value(position);
    }
}
