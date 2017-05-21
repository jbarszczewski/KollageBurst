namespace KollageBurst_WP8.Extensions
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public static class ImageProcessingExtensions
    {
        public static Color GetAverageColor(this WriteableBitmap source, int top = 0, int left = 0, int height = 0, int width = 0)
        {
            if (width == 0)
            {
                width = (int)source.PixelWidth;
            }

            if (height == 0)
            {
                height = (int)source.PixelHeight;
            }

            int right = width + left;
            int bottom = height + top;
            
            int numberOfPixels = (int)(height * width);
            int r = 0;
            int g = 0;
            int b = 0;
            for (int i = left; i < right; i++)
            {
                for (int j = top; j < bottom; j++)
                {
                    Color pixel = source.GetPixel(i, j);
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                }
            }


            var averageColor = new System.Windows.Media.Color
            {
                A = 0xFF,
                R = (byte)(r / numberOfPixels),
                G = (byte)(g / numberOfPixels),
                B = (byte)(b / numberOfPixels)
            };

            return averageColor;
        }
    }
}
