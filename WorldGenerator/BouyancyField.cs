using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class BouyancyField : IField<Bouyancy>, IField<IVectorValued<MNPerKm3>>
    {
        public Bouyancy Value(Position position) =>
            position.Value.LengthSquared() < 1.0f ?
                new(Vector3.Normalize(position.Value)):
                new (Vector3.Zero);

        IVectorValued<MNPerKm3> IField<IVectorValued<MNPerKm3>>.Value(Position position) => Value(position);
    }
}
