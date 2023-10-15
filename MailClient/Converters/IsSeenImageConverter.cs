using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MailClient.Converters
{
    internal class IsSeenImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? Resource.email2 : Resource.mail;
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return false; }

    }
}
