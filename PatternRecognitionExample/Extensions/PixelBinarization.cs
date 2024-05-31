using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PatternRecognitionExample.Extensions
{
    /// <summary>
    /// Бинаризация изображения побитовым сдвигом цвета пикселей
    /// </summary>
    public static class PixelBinarization
    {
        /// <summary>
        /// Получить набор оттенков HEX из картинки
        /// </summary>
        /// <param name="input">Изображение</param>
        /// <returns>List, содержащий все оттенки входного изображения</returns>
        public static List<string> GetHEXCollection(Bitmap input)
        {
            var output = ConvertToBW(input);

            var colorList = new List<string>(input.Width * input.Height);

            for (int j = 0; j < input.Height; j++)
            {
                for (int i = 0; i < input.Width; i++)
                {
                    var color = output.GetPixel(i, j);

                    colorList.Add("#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2"));
                }
            }

            var colorCountDict = colorList.GroupBy(x => x)
            .OrderByDescending(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Count());

            List<string> hexCollection = new List<string>();

            for (var i = 0; i < colorCountDict.Count(); i++)
            {
                hexCollection.Add(colorCountDict.Keys.ElementAt(i));
            }

            return hexCollection;
        }

        /// <summary>
        /// Метод конвертации в ЧБ-изображение побитовым сдвигом
        /// </summary>
        /// <param name="input">Изображение</param>
        /// <returns>ЧБ вариант входного изображение</returns>
        public static Bitmap ConvertToBW(Bitmap input)
        {
            Bitmap output = new Bitmap(input.Width, input.Height);

            for (int j = 0; j < input.Height; j++)
            {
                for (int i = 0; i < input.Width; i++)
                {
                    uint pixel = (uint)(input.GetPixel(i, j).ToArgb());

                    float R = (float)((pixel & 0x00FF0000) >> 16);
                    float G = (float)((pixel & 0x0000FF00) >> 8);
                    float B = (float)(pixel & 0x000000FF);

                    R = G = B = (R + G + B) / 3.0f;

                    uint newPixel = 0xFF000000 | ((uint)R << 16) | ((uint)G << 8) | ((uint)B);

                    output.SetPixel(i, j, Color.FromArgb((int)newPixel));
                }
            }

            return output;
        }

        /// <summary>
        /// Конвертирует изображение в идеальное ЧБ (только черный и белый цвета)
        /// </summary>
        /// <param name="input">Изображение</param>
        /// <param name="middleColor">Цвет-маркер границы черного и белого</param>
        /// <returns>ЧБ-изображение, содержащее только #000000 и #FFFFFF цвета</returns>
        public static Bitmap ConvertToIdeal(Bitmap input, string middleColor)
        {
            Bitmap output = new Bitmap(input.Width, input.Height);
            middleColor = middleColor.Replace("#", "0xFF");
            uint midColorUint = Convert.ToUInt32(middleColor, 16);

            for (int j = 0; j < input.Height; j++)
            {
                for (int i = 0; i < input.Width; i++)
                {
                    uint pixel = (uint)(input.GetPixel(i, j).ToArgb());

                    float R = (float)((pixel & 0x00FF0000) >> 16);
                    float G = (float)((pixel & 0x0000FF00) >> 8);
                    float B = (float)(pixel & 0x000000FF);

                    R = G = B = (R + G + B) / 3.0f;

                    uint newPixel = 0xFF000000 | ((uint)R << 16) | ((uint)G << 8) | ((uint)B);

                    if (newPixel > midColorUint)
                    {
                        output.SetPixel(i, j, Color.White);
                    }
                    else
                    {
                        output.SetPixel(i, j, Color.Black);
                    }
                }
            }

            return output;
        }
    }
}
