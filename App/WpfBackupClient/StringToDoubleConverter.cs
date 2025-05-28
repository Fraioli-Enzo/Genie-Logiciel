using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfBackupClient
{
    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && double.TryParse(s, out double d))
                return d;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return d.ToString();
            return "0";
        }
    }
}
