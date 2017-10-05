using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CS.Wpf.Converters
{
    /// <summary>
    /// Converts a Thickness into a single double for use as a Stroke or other.
    /// </summary>
    [ValueConversion(typeof(Thickness), typeof(double))]
    public class BorderThicknessToStrokeThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Thickness thickness = (Thickness)value;
            var result = (thickness.Bottom + thickness.Left + thickness.Right + thickness.Top) / 4;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double? thick = (double?)value;
            double thickValue = thick ?? 0;
            return new Thickness(thickValue, thickValue, thickValue, thickValue);
        }
    }
}
