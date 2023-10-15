using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;

namespace MailClient.Converters
{
    internal class BoolToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? Brushes.Gray : Brushes.White;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return false; }
    }
}
