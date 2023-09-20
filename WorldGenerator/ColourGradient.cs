using Microsoft.Xna.Framework;

namespace WorldGenerator
{
    public record GradientPoint(float Position, Color Colour);

    public class ColourGradient
    {
        private readonly GradientPoint[] _gradientPoints;

        public ColourGradient(GradientPoint[] gradientPoints)
        {
            _gradientPoints = gradientPoints;
        }

        public Color ColourAt(float position)
        {
            if (position < _gradientPoints[0].Position)
            {
                return _gradientPoints[0].Colour;
            }

            if (position > _gradientPoints[^1].Position)
            {
                return _gradientPoints[^1].Colour;
            }

            if(_gradientPoints.Length == 1)
            {
                return _gradientPoints[0].Colour;
            }

            for (var i = 0; i < _gradientPoints.Length - 1; i++)
            {
                if (position >= _gradientPoints[i].Position && position <= _gradientPoints[i + 1].Position)
                {
                    var t = (position - _gradientPoints[i].Position) / (_gradientPoints[i + 1].Position - _gradientPoints[i].Position);
                    return Color.Lerp(_gradientPoints[i].Colour, _gradientPoints[i + 1].Colour, t);
                }
            }

            // Likely to show up as pink, which is a good indicator that something has gone wrong
            return Color.Pink;
        }
    }
}
