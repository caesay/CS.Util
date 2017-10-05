using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CS.Wpf.Brushes
{
    [MarkupExtensionReturnType(typeof(Brush))]
    public class BilinearGradientBrushExtension : MarkupExtension, INotifyPropertyChanged
    {
        public List<Color> ColorMatrix
        {
            get { return colorMatrix; }
            set
            {
                colorMatrix = value;
                OnPropertyChanged("ColorMatrix");
            }
        }
        public int CountY { get; set; }
        public int CountX { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private List<Color> colorMatrix = new List<Color>();

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var i = ColorMatrix.Count;

            var pixelValues = GeneratePixelValues();

            var bmp = new WriteableBitmap(CountX, CountY, 96, 96, PixelFormats.Pbgra32, null);
            bmp.WritePixels(new Int32Rect(0, 0, CountX, CountY), pixelValues, pixelValues.Length, 0);

            var brush = new ImageBrush(bmp);

            return brush;
        }
        private int[] GeneratePixelValues()
        {
            var pixelCount = CountX * CountY;

            var pixelValues = new int[pixelCount];
            for (var i = 0; i < CountY; i++)
            {
                for (var j = 0; j < CountX; j++)
                {
                    var c = ColorMatrix[i * CountY + j];
                    var a = c.A;
                    var r = c.R;
                    var g = c.G;
                    var b = c.B;

                    pixelValues[i * CountY + j] = (a << 24) + (r << 16) + (g << 8) + b;
                }
            }
            return pixelValues;
        }
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}