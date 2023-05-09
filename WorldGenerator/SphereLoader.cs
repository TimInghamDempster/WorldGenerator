using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public static class SphereLoader
    {
        public static List<Position> LoadSphere()
        {
            var points = new List<Position>();
            using var reader = new BinaryReader(File.OpenRead("Content/Sphere10000.dat"));

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                points.Add(new(pos));
            }

            return points;
        }
    }
}
