using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lenium.NeuralNetwork;
using Lenium.NeuralNetwork.Education.Backpropagation;
using Lenium.NeuralNetwork.Enums;
using Lenium.NeuralNetwork.InputDatas;
using Lenium.NeuralNetwork.InputDatas.Interfaces;
using Lenium.NeuralNetwork.InputDatas.Normalizators;
using Lenium.NeuralNetwork.Logging;
using Lenium.NeuralNetwork.Logging.Args;
using Lenium.NeuralNetwork.SavingAndLoading.ByteArraySavingAndLoading;
using Lenium.NeuralNetwork.TransferFunctions;
using Microsoft.Win32;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;
using PatternRecognitionExample.Extensions;

namespace PatternRecognitionExample
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string[] _symbolsArray = {"А1", "а2", "Б1", "б2", "В", "Г"};

        private IList<IInputData<double>> _inputData;
        private InputSettings _inputSettings;
        private bool _isPaint;

        private MultiLayerSegment _segment;

        private WriteableBitmap _writeableBitmap = new WriteableBitmap(250, 250, 96, 96,
            PixelFormats.Pbgra32, null);

        /// <summary>
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            image1.IsEnabled = false;
            recognizeButton.IsEnabled = false;
            saveButton.IsEnabled = false;

            Logger.Current.MessageReceived += InstanceOnMessageReceived;

            image1.MouseLeftButtonDown += image1_MouseLeftButtonDown;
            image1.MouseMove += image1_MouseMove;
            image1.MouseLeftButtonUp += image1_MouseLeftButtonUp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            image1.CaptureMouse();
            _isPaint = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPaint)
            {
                _writeableBitmap.FillEllipse((int) e.GetPosition(image1).X - 10, (int) e.GetPosition(image1).Y - 10,
                    (int) e.GetPosition(image1).X + 10, (int) e.GetPosition(image1).Y + 10, Colors.Black);

                image1.Source = _writeableBitmap;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPaint = false;
            image1.ReleaseMouseCapture();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Bitmap GetBitmap(BitmapSource source)
        {
            var bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
                new Rectangle(Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);
            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height*data.Stride,
                data.Stride);
            bmp.UnlockBits(data);

            var normBmp = Scaling.ScaleAndCrop(PixelBinarization.ConvertToIdeal(bmp, "#808080"), 250, 250);
            image2.Source = BitmapToImageSource(normBmp);

            return normBmp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _writeableBitmap.FillRectangle(0, 0, 250, 250, Colors.White);

            image1.Source = new WriteableBitmap(_writeableBitmap);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LearnButton_OnClick(object sender, RoutedEventArgs e)
        {
            learnButton.IsEnabled = false;

            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (o, args) =>
            {
                var hiddenLayersDescription = new List<LayerDescription>
                {
                   // new LayerDescription(500, new SigmoidTransferFunction()),
                   // new LayerDescription(500, new SigmoidTransferFunction()),
                    //new LayerDescription(300, new SigmoidTransferFunction()),
                 //   new LayerDescription(256, new SigmoidTransferFunction()),                    
                   // new LayerDescription(128, new SigmoidTransferFunction()),
                    new LayerDescription(60, new SigmoidTransferFunction()),
                     new LayerDescription(18, new SigmoidTransferFunction()),
                  //  new LayerDescription(32, new SigmoidTransferFunction()),
                    new LayerDescription(6, new SigmoidTransferFunction())
                };

                _inputSettings = new InputSettings(LayersCreationType.Manual, hiddenLayersDescription,
                    new LinearTransferFunction());

                _inputData = new BitmapDataNormalizator(_inputSettings).Normalize(CreateSymbolImages(true));

                var segment = new MultiLayerSegment {Title = "Segment1"};

                var educationStrategy =
                    new BackpropagationEducationStrategy(new BackpropagationSettings
                    {
                        EducationSpeed = 0.01,
                        LogMessageFrequency = 1,
                        MaxEpoches = 15000,
                        Momentum = 0.01,
                        NormalError = 0.00001
                    });

                educationStrategy.Train(segment, _inputData);

                args.Result = segment;
            };

            backgroundWorker.RunWorkerCompleted += (o, args) =>
            {
                image1.IsEnabled = true;
                recognizeButton.IsEnabled = true;
                saveButton.IsEnabled = true;
                _segment = (MultiLayerSegment) args.Result;
            };

            backgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="logMessageEventArgs"></param>
        private void InstanceOnMessageReceived(object sender, LogMessageEventArgs logMessageEventArgs)
        {
            Dispatcher.BeginInvoke(
                new Action<string>(p => { statusTextBlock.Text = string.Format("Logger message: {0}", p); }),
                logMessageEventArgs.Message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recreate"></param>
        /// <returns></returns>
        private List<Bitmap> CreateSymbolImages(bool recreate = false)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Symbols"));
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var bitmaps = new List<Bitmap>();

            foreach (string s in _symbolsArray)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Symbols", s + ".png");

                Bitmap bitmap = GetBitmap(s.Substring(0, 1));

                bitmaps.Add(bitmap);

                if (!recreate)
                    if (File.Exists(path)) continue;

                Dispatcher.BeginInvoke(
                    new Action<string>(p => { statusTextBlock.Text = string.Format("Создание изображения {0}", p); }),
                    path);

                bitmap.Save(path);
            }

            return bitmaps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Bitmap GetBitmap(string text)
        {
            var bitmap = new Bitmap(250, 250);
            using (Graphics gr = Graphics.FromImage(bitmap))
            {
                const float xText = 0;
                float yText = 0;
                const float fontSize = 150f;

                gr.FillRectangle(new SolidBrush(Color.FromArgb(160, 255, 255, 255)), 0, 0, 250, 250);
                gr.DrawString(text, new Font("Verdana", fontSize, System.Drawing.FontStyle.Bold), new SolidBrush(Color.Black), xText, yText);
                bitmap.Tag = text;

                return Scaling.ScaleAndCrop(PixelBinarization.ConvertToIdeal(bitmap, "#808080"), 250, 250);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _writeableBitmap = new WriteableBitmap(250, 250, 96, 96,
                PixelFormats.Pbgra32, null);
            _writeableBitmap.FillRectangle(0, 0, 250, 250, Colors.White);

            image1.Source = _writeableBitmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            IInputData<double> input =
                new BitmapDataNormalizator(_inputSettings).Normalize(new List<Bitmap> {GetBitmap(_writeableBitmap)})[0];

            IInputData<double> output = _segment.CalculateOutput(input);

            int index = output.Output.ToList().IndexOf(output.Output.Max());

            statusTextBlock.Text = string.Format("Эскиз похож на букву: {0}", _symbolsArray[index]);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_segment != null)
            {
                var savingStrategy = new ByteArraySavingStrategy();

                var dialog = new SaveFileDialog();

                dialog.RestoreDirectory = true;

                dialog.Filter = "Bin Files .bin | *.bin";

                if (dialog.ShowDialog() == true)
                {
                    savingStrategy.SaveToStream(_segment, dialog.OpenFile());
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_segment == null)
                _segment = new MultiLayerSegment();

            var loadingStrategy = new ByteArrayLoadingStrategy(null);

            var dialog = new OpenFileDialog();

            dialog.RestoreDirectory = true;

            dialog.Filter = "Bin Files .bin | *.bin";

            if (dialog.ShowDialog() == true)
            {
                loadingStrategy.LoadFromStream(_segment, dialog.OpenFile());

                image1.IsEnabled = true;
                recognizeButton.IsEnabled = true;
            }
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}