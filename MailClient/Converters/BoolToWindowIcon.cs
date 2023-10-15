using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MailClient.Converters
{
    internal class BoolToWindowIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? Resource.window_restore : Resource.window_full;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return false; }
    }
}
