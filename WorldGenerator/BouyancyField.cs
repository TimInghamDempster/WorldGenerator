using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class BouyancyField : IField<Bouyancy>, IField<IVectorValued>
    {
        public Bouyancy Value(Position position) =>
            position.Value.LengthSquared() < 1.0f ?
                new(Vector3.Normalize(position.Value), new(new List<UnitPart>())):
                new (Vector3.Zero, new(new List<UnitPart>()));

        IVectorValued IField<IVectorValued>.Value(Position position) => Value(position);
    }
}
