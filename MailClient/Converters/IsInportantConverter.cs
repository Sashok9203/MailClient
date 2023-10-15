using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MailClient.Converters
{
    internal class IsInportantConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? Resource.important_marked1 : Resource.important_1;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return false; }
    }
}
