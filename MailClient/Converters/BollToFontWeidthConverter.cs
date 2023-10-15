using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MailClient.Converters
{
    internal class BollToFontWeidthConverter : IValueConverter
    {
         public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? FontWeights.Normal : FontWeights.Medium ;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return false; }
    }
}
