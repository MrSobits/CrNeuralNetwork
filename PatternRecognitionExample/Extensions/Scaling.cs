using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PatternRecognitionExample.Extensions
{
    /// <summary>
    /// Масштабирование изображения
    /// </summary>
    public static class Scaling
    {
        /// <summary>
        /// Масштабировать и обрезать изображение
        /// </summary>
        /// <param name="image">Входное изображение</param>
        /// <param name="width">Требуемая ширина</param>
        /// <param name="height">Требуемая высота</param>
        /// <returns>Изначальное изображение, но без лишнего белого пространства и указанного размера</returns>
        public static Bitmap ScaleAndCrop(Bitmap image, int width, int height)
        {
            int maxX = 0;
            int maxY = 0;
            int minX = image.Width;
            int minY = image.Height;

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    if (pixelColor.R != 255 && pixelColor.G != 255 && pixelColor.B != 255)
                    {
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }

            Console.WriteLine($"({minX};{minY})");
            Console.WriteLine($"({maxX};{maxY})");

            int letterWidth = maxX - minX;
            int letterHeight = maxY - minY;

            Bitmap resized = Scale(image.Clone(new Rectangle(minX, minY, letterWidth, letterHeight), PixelFormat.Format32bppRgb), width, height);
            return resized;
        }


        /// <summary>
        /// Изменить размер изображения с помощью перерисовки
        /// </summary>
        /// <param name="image">Входное изображение</param>
        /// <param name="width">Требуемая ширина</param>
        /// <param name="height">Требуемая высота</param>
        /// <returns>Изначальное изображение, но указанного размера</returns>
        public static Bitmap Scale(Bitmap image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
