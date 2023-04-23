using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public interface IVisualiser
    {
        Color GetColour(float[] pos, Field field);
    }

    public interface IManifold
    {
        IEnumerable<int> Neighbours(int origin);
    }

    public class EuclideanManifold1d : IManifold
    {
        private readonly int _length;

        public EuclideanManifold1d(int length)
        {
            _length = length;
        }

        public IEnumerable<int> Neighbours(int origin) =>
            origin switch
            {
                0 => new int[] { 1 },
                int i when i == _length - 1  => new int[] { _length - 2 },
                _ => new int[] { origin - 1, origin + 1 }
            };

    }

    public record Field(IManifold Manifold, IReadOnlyList<float> Values)
    {
        public static Field DiffuseSimple(Field initialField)
        {
            var newVals = new float[initialField.Values.Count].ToList();

            for(int i = 0; i < newVals.Count(); i++)
            {
                var neighbours = initialField.Manifold.Neighbours(i);
                var distributedToNeighbours = 0.1f * neighbours.Count();

                newVals[i] =
                    initialField.Values[i] * (1.0f - distributedToNeighbours) +
                    neighbours.Select(n => initialField.Values[n]).Sum() * 0.1f;
            }

            return initialField with { Values = newVals };
        }
    }
}