using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public class GravitationalAcceleartionField : IField<TN, Vector3>, ITimeDependent
    {
        private readonly IField<Unitless, Vector3> _gravityDir;

        public IManifold Manifold { get; }

        public Vector3[] Values { get; }

        public GravitationalAcceleartionField(IManifold manifold, IField<Unitless, Vector3> gravityDir)
        {
            Manifold = manifold;
            _gravityDir = gravityDir;
            Values = new Vector3[manifold.Values.Length];
        }

        public void ProgressTime(TimeKY timestep)
        {
            for(int i = 0; i < Values.Length; i++)
            {
                var normal = Vector3.Zero;

                var neighbourFaces = Manifold.Faces[i];
                var pos = Manifold.Values[i];
                var gravityDir = _gravityDir.Values[i];

                if(i == 10) { int a = 0; a++; }

                foreach(var face in neighbourFaces.Faces)
                {
                    normal += IManifold.FaceNormal(Manifold, face);
                }

                normal = Vector3.Normalize(normal);

                // Can't slide downhill if the ground is level/flat
                var angle = MathF.Acos(Vector3.Dot(normal, gravityDir));
                var oneDegreeInRads = MathF.PI / 180;
                if(angle < oneDegreeInRads ||
                    angle > MathF.PI - oneDegreeInRads)
                {
                    continue;
                }

                var overallTangent = Vector3.Cross(normal, gravityDir);
                var slideDir = Vector3.Normalize(Vector3.Cross(overallTangent, normal));

                if(float.IsNaN(slideDir.X) || float.IsNaN(slideDir.Y) || float.IsNaN(slideDir.Z))
                {
                    continue;
                }

                Values[i] = slideDir * MathF.Sin(angle) *  0.01f;
            }
        }
    }
}
