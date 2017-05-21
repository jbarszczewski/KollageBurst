namespace KollageBurst_WP8.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;

    public static class ColorExtensions
    {
        public static Color FindNearestColorMatch(this Color sourceColor, IEnumerable<Color> colorCollection)
        {
            var selectedColor = colorCollection.OrderBy(x => x.GetColorsDistance(sourceColor)).First<Color>();
            return selectedColor;
        }

        public static Color FindNearestColorMatch(this Color sourceColor, IEnumerable<string> colorCodesCollection)
        {
            var selectedColor = colorCodesCollection.Select(y => HexColor(y)).OrderBy(x => x.GetColorsDistance(sourceColor)).First<Color>();
            return selectedColor;
        }

        public static double GetColorsDistance(this Color c1, Color c2)
        {
            double r = Math.Abs(c1.R - c2.R) / 255d;
            double g = Math.Abs(c1.G - c2.G) / 255d;
            double b = Math.Abs(c1.B - c2.B) / 255d;
            return r + g + b;
        }

        private static Color HexColor(String hex)
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }
    }
}
