using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MailClient.ViewModels
{
    internal class MessageVM :INotifyPropertyChanged
    {
        private bool isChecked;
        private bool isSeen;
        private bool flagget;
        private bool attachment;
        private bool important;
        private bool deleted;
        private bool spam;

        public MimeMessage Message { get; set; }

        public UniqueId UniqueId { get; set; }

        
        public bool Importance
        {
            get => important;
            set
            {
                important = value;
                OnPropertyChanged();
            }
        }

       

        public bool Attachment
        {
            get => attachment;
            set
            {
                attachment = value;
                OnPropertyChanged();
            }
        }

        public bool Flagget
        {
            get => flagget;
            set
            {
                flagget = value;
                OnPropertyChanged();
            }
        }

        public bool IsSeen
        {
            get => isSeen;
            set
            {
                isSeen = value;
                OnPropertyChanged();
            }
        }

        public bool IsChecked 
        {
            get => isChecked;
            set
            {
                isChecked = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
