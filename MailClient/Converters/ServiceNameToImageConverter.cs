using MailClient.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MailClient.Converters
{
    internal class ServiceNameToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object? image = null;
           
            switch ((Services)value)
            {
                case Services.Google:
                    image = Resource.gmail;
                    break;
                case Services.ICloud:
                    image = Resource.icloud;
                    break;
                case Services.UkrNet:
                    image = Resource.ukrnet;
                    break;
            }
           return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return false; }
    }
}
