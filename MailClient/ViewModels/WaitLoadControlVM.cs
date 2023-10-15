using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MailClient.ViewModels
{
    internal class WaitLoadControlVM : ViewModelBase
    {
        private string loadingText = string.Empty;
        private int width = 60;
        private int height = 60;
        private int textSize = 13;
        private Brush textColor = Brushes.Black;


        public Brush TextColor
        {
            get => textColor;
            set
            {
                textColor = value;
                OnPropertyChanged();
            }
        }

        public int TextSize
        {
            get => textSize;
            set
            {
                textSize = value;
                OnPropertyChanged();
            }
        }

        public int Width
        {
            get => width;
            set
            {
                width = value;
                OnPropertyChanged();
            }
        }

        public int Height
        {
            get => height;
            set
            {
                height = value;
                OnPropertyChanged();
            }
        }

        public string LoadingText
        {
            get => loadingText;
            set 
            {
                loadingText = value;
                OnPropertyChanged();
            }
        }


    }
}
