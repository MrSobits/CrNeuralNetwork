using System;
using System.Drawing;
using Color = System.Drawing.Color;

namespace PatternRecognitionExample.Extensions
{
    /// <summary>
    /// Бинаризация изображения методом Саувола
    /// </summary>
    public static class SauvolBinarization
    {
        /// <summary>
        /// Метод конвертации по Сауволу
        /// </summary>
        /// <param name="img">Начальное изображение</param>
        /// <param name="k">Пороговый коэффициент</param>
        /// <param name="localSquareSize">Сторона квадрата для интегральной суммы</param>
        /// <returns>Конвертированное изображение</returns>
        public static Bitmap ConvertBySauvol(Bitmap img, double k = 0.1, int localSquareSize = 15)
        {
            if (localSquareSize % 2 != 1)
            {
                throw new ArgumentException("Values in the kernel array must be odd numbers. Please check if kernel[0] or kernel[1] is odd!");
            }

            var integral = Integral(img);
            int[,] integralSum = integral.Item1;
            int[,] integralSqrtSum = integral.Item2;
            int rows = img.Height;
            int cols = img.Width;
            double[,] diff = new double[rows, cols];
            double[,] sqrtDiff = new double[rows, cols];
            double[,] mean = new double[rows, cols];
            double[,] threshold = new double[rows, cols];
            double[,] std = new double[rows, cols];

            int whalf = localSquareSize >> 1;

            for (int row = 0; row < rows; row++)
            {
                Console.WriteLine($"Processing row {row}...");
                for (int col = 0; col < cols; col++)
                {
                    int xmin = Math.Max(0, row - whalf);
                    int ymin = Math.Max(0, col - whalf);
                    int xmax = Math.Min(rows - 1, row + whalf);
                    int ymax = Math.Min(cols - 1, col + whalf);

                    int area = (xmax - xmin + 1) * (ymax - ymin + 1);
                    if (area <= 0)
                    {
                        Environment.Exit(1);
                    }

                    if (xmin == 0 && ymin == 0)
                    {
                        diff[row, col] = integralSum[xmax, ymax];
                        sqrtDiff[row, col] = integralSqrtSum[xmax, ymax];
                    }
                    else if (xmin > 0 && ymin == 0)
                    {
                        diff[row, col] = integralSum[xmax, ymax] - integralSum[xmin - 1, ymax];
                        sqrtDiff[row, col] = integralSqrtSum[xmax, ymax] - integralSqrtSum[xmin - 1, ymax];
                    }
                    else if (xmin == 0 && ymin > 0)
                    {
                        diff[row, col] = integralSum[xmax, ymax] - integralSum[xmax, ymax - 1];
                        sqrtDiff[row, col] = integralSqrtSum[xmax, ymax] - integralSqrtSum[xmax, ymax - 1];
                    }
                    else
                    {
                        int diagSum = integralSum[xmax, ymax] + integralSum[xmin - 1, ymin - 1];
                        int idiagSum = integralSum[xmax, ymin - 1] + integralSum[xmin - 1, ymax];
                        diff[row, col] = diagSum - idiagSum;

                        double sqDiagSum = integralSqrtSum[xmax, ymax] + integralSqrtSum[xmin - 1, ymin - 1];
                        double sqIdiagSum = integralSqrtSum[xmax, ymin - 1] + integralSqrtSum[xmin - 1, ymax];
                        sqrtDiff[row, col] = sqDiagSum - sqIdiagSum;
                    }

                    mean[row, col] = diff[row, col] / area;
                    std[row, col] = Math.Sqrt((sqrtDiff[row, col] - Math.Sqrt(diff[row, col]) / area) / (area - 1));
                    threshold[row, col] = mean[row, col] * (1 + k * ((std[row, col] / 128) - 1));

                    Color pixel = img.GetPixel(col, row);
                    int grayValue = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    if (grayValue < threshold[row, col])
                    {
                        img.SetPixel(col, row, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        img.SetPixel(col, row, Color.FromArgb(255, 255, 255));
                    }
                }
            }

            return img;
        }

        private static Tuple<int[,], int[,]> Integral(Bitmap img)
        {
            int rows = img.Height;
            int cols = img.Width;
            int[,] integralSum = new int[rows, cols];
            int[,] integralSqrtSum = new int[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                int sum = 0;
                int sqrtSum = 0;
                for (int c = 0; c < cols; c++)
                {
                    Color pixel = img.GetPixel(c, r);
                    int grayValue = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    sum += grayValue;
                    sqrtSum += (int)Math.Sqrt(grayValue);

                    if (r == 0)
                    {
                        integralSum[r, c] = sum;
                        integralSqrtSum[r, c] = sqrtSum;
                    }
                    else
                    {
                        integralSum[r, c] = sum + integralSum[r - 1, c];
                        integralSqrtSum[r, c] = sqrtSum + integralSqrtSum[r - 1, c];
                    }
                }
            }

            return new Tuple<int[,], int[,]>(integralSum, integralSqrtSum);
        }
    }

}
