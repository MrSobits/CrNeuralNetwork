using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Lenium.NeuralNetwork.InputDatas.Interfaces;

namespace Lenium.NeuralNetwork.InputDatas.Normalizators
{
    public class BitmapDataNormalizator : IInputDataNormalizator<IList<Bitmap>>
    {
        /// <summary>
        /// 
        /// </summary>
        public IInputSettings InputSettings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSettings"></param>
        public BitmapDataNormalizator(IInputSettings inputSettings)
        {
            InputSettings = inputSettings;
        }

        /// <summary>
        /// нормализация по всем пиксеклям
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        //public IList<IInputData<double>> Normalize(IList<Bitmap> input)
        //{
        //    if (!ValidateBitmaps(input)) throw new Exception("Invalid inputs!");

        //    List<IInputData<double>> list = new List<IInputData<double>>();

        //    for (int i = 0; i < input.Count; i++)
        //    {
        //        double[] inputArray = new double[input[i].Width * input[i].Height];
        //        double[] outputArray = new double[input.Count];

        //        for (int j = 0, k = 0; j < input[i].Height; j++)
        //        {
        //            for (int u = 0; u < input[i].Width; u++)
        //            {
        //                double val = 0.3 * input[i].GetPixel(u, j).R + 0.59 * input[i].GetPixel(u, j).G + 0.11 * input[i].GetPixel(u, j).B;

        //                if (val > 127)
        //                {
        //                    inputArray[k++] = -0.5;
        //                }
        //                else
        //                {
        //                    inputArray[k++] = 0.5;
        //                }
        //            }
        //        }

        //        for (int j = 0; j < outputArray.Length; j++)
        //        {
        //            if (j == i)
        //                outputArray[j] = 0.99;
        //            else
        //                outputArray[j] = 0.01;
        //        }

        //        var inputData = new InputData<double>(inputArray, outputArray, InputSettings);

        //        list.Add(inputData);
        //    }

        //    return list;
        //}

        /// <summary>
        /// нормализация по графику
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IList<IInputData<double>> NormalizeXY(IList<Bitmap> input)
        {
            if (!ValidateBitmaps(input)) throw new Exception("Invalid inputs!");

            List<IInputData<double>> list = new List<IInputData<double>>();

            for (int i = 0; i < input.Count; i++)
            {
                double[] inputArray = new double[input[i].Width + input[i].Height];
                double[] outputArray = new double[input.Count];
                double[,] map = new double[input[i].Height, input[i].Width];

                for (int j = 0, k = 0; j < input[i].Height; j++)
                {
                    for (int u = 0; u < input[i].Width; u++)
                    {
                        double val = 0.3 * input[i].GetPixel(u, j).R + 0.59 * input[i].GetPixel(u, j).G + 0.11 * input[i].GetPixel(u, j).B;

                        if (val < 127)
                        {
                            map[j,u] = 1;
                        }
                        else
                        {
                            map[j, u] = 0;
                        }
                    }
                }
                var cc = map.GetLength(0);
                var cc2 = map.GetLength(1);
                int used = 0;
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    double sum = 0;
                    for (int y = 0; y < map.GetLength(1); y++)
                    {
                        sum += map[x,y];
                    }
                    inputArray[x] = sum;
                    used++;
                }
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    double sum = 0;
                    for (int x = 0; x < map.GetLength(0); x++)
                    {
                        sum += map[x, y];
                    }
                    inputArray[used +y] = sum;
                }

                for (int j = 0; j < outputArray.Length; j++)
                {
                    if (j == i)
                        outputArray[j] = 0.99;
                    else
                        outputArray[j] = 0.01;
                }

                var inputData = new InputData<double>(inputArray, outputArray, InputSettings);

                list.Add(inputData);
            }

            return list;
        }


        /// <summary>
        /// нормализация по графику
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IList<IInputData<double>> NormalizeCNNXY(IList<Bitmap> input)
        {
            if (!ValidateBitmaps(input)) throw new Exception("Invalid inputs!");

            List<IInputData<double>> list = new List<IInputData<double>>();
            int boxsize = 5;

            for (int i = 0; i < input.Count; i++)
            {
                int arraysize = input[i].Width * input[i].Height / (int)Math.Pow(boxsize, 2);
                int xysize = input[i].Width + input[i].Height;
                double[] inputArray = new double[arraysize+xysize];
                double[] outputArray = new double[input.Count];
                double[,] map = new double[input[i].Height, input[i].Width];

                for (int j = 0, k = 0; j < input[i].Height; j++)
                {
                    for (int u = 0; u < input[i].Width; u++)
                    {
                        double val = 0.3 * input[i].GetPixel(u, j).R + 0.59 * input[i].GetPixel(u, j).G + 0.11 * input[i].GetPixel(u, j).B;

                        if (val < 127)
                        {
                            map[j, u] = 1;
                        }
                        else
                        {
                            map[j, u] = 0;
                        }
                    }
                }
                var cc = map.GetLength(0);
                var cc2 = map.GetLength(1);
                int used = 0;
                for (int x = 0; x < map.GetLength(0); x += boxsize)
                {

                    for (int y = 0; y < map.GetLength(1); y += boxsize)
                    {
                        double sum = 0;
                        for (int subX = x; subX < x + boxsize; subX++)
                        {
                            for (int subY = y; subY < y + boxsize; subY++)
                            {
                                sum += map[subX, subY];
                            }
                        }
                        inputArray[used] = sum;
                        used++;
                    }

                }                
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    double sum = 0;
                    for (int y = 0; y < map.GetLength(1); y++)
                    {
                        sum += map[x, y];
                    }
                    inputArray[used + x] = sum;
                    used++;
                }
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    double sum = 0;
                    for (int x = 0; x < map.GetLength(0); x++)
                    {
                        sum += map[x, y];
                    }
                    inputArray[used + y] = sum;
                }


                for (int j = 0; j < outputArray.Length; j++)
                {
                    if (j == i)
                        outputArray[j] = 0.99;
                    else
                        outputArray[j] = 0.01;
                }

                var inputData = new InputData<double>(inputArray, outputArray, InputSettings);

                list.Add(inputData);
            }

            return list;
        }


        /// <summary>
        /// нормализация по графику
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IList<IInputData<double>> Normalize(IList<Bitmap> input)
        {
            if (!ValidateBitmaps(input)) throw new Exception("Invalid inputs!");

            List<IInputData<double>> list = new List<IInputData<double>>();
            int boxsize = 5;
            
            for (int i = 0; i < input.Count; i++)
            {
                int arraysize = input[i].Width * input[i].Height / (int)Math.Pow(boxsize, 2);
                double[] inputArray = new double[arraysize];
                double[] outputArray = new double[input.Count];
                double[,] map = new double[input[i].Height, input[i].Width];

                for (int j = 0, k = 0; j < input[i].Height; j++)
                {
                    for (int u = 0; u < input[i].Width; u++)
                    {
                        double val = 0.3 * input[i].GetPixel(u, j).R + 0.59 * input[i].GetPixel(u, j).G + 0.11 * input[i].GetPixel(u, j).B;

                        if (val < 127)
                        {
                            map[j, u] = 0.1;
                        }
                        else
                        {
                            map[j, u] = 0;
                        }
                    }
                }
                var cc = map.GetLength(0);
                var cc2 = map.GetLength(1);
                int used = 0;
                for (int x = 0; x < map.GetLength(0); x+= boxsize)
                {
                    
                    for (int y = 0; y < map.GetLength(1); y+= boxsize)
                    {
                        double sum = 0;
                        for (int subX = x; subX < x+boxsize; subX++)
                        {
                            for (int subY = y; subY < y + boxsize; subY++)
                            {
                                sum += map[subX, subY];
                            }
                        }
                        inputArray[used] = sum;
                        used++;
                    }
                   
                }
              

                for (int j = 0; j < outputArray.Length; j++)
                {
                    if (j == i)
                        outputArray[j] = 0.99;
                    else
                        outputArray[j] = 0.01;
                }

                var inputData = new InputData<double>(inputArray, outputArray, InputSettings);

                list.Add(inputData);
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmaps"></param>
        /// <returns></returns>
        private bool ValidateBitmaps(IList<Bitmap> bitmaps)
        {
            if (bitmaps == null || bitmaps.Count == 0)
                throw new ArgumentNullException("bitmaps");

            double width, height;
            width = bitmaps.First().Width;
            height = bitmaps.First().Height;

            foreach (var bitmap in bitmaps.Skip(1))
            {
                if (bitmap.Width != width || bitmap.Height != height || width == 0.0d || height == 0.0d)
                    return false;
            }

            return true;
        }
    }
}
