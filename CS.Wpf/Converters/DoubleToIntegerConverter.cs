using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace CS.Wpf.Converters
{
    /// <summary>
    /// Double to integer converter.
    /// </summary>
    [ValueConversion(typeof(double), typeof(int))]
    public class DoubleToIntegerConverter : IValueConverter
    {
        int min = Int32.MinValue;
        int max = Int32.MaxValue;

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            int result = (int)((double)value + 0.5);

            if ( result < min )
            {
                result = min;
            }

            if ( result > max )
            {
                result = max;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
