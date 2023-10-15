using MailClient.Utility;
using MailKit;
using MailKit.Search;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MailClient.ViewModels
{
    internal class FolderVM : INotifyPropertyChanged
    {
        private bool selected;

        private int unread;

        public IMailFolder Folder { get;private set; }

        public byte[]? Icon { get; set; } = Resource.cloudfolder;

        public FolderVM(IMailFolder folder, LocalFolder lokalName)
        {
            Folder = folder;
            UnreadApdateAsync();
            LocalName = lokalName;
        }

        public int Unread
        {
            get => unread;
            set 
            {
                unread = value;
                OnPropertyChanged();
            }
        }

        public  void UnreadApdateAsync()
        {
            if (Messages == null)
            {
                Task.Run(() =>
                {
                    lock (Folder.SyncRoot)
                    {
                        Folder.Status(StatusItems.Unread);
                    }
                    Unread = Folder.Unread;
                });
            }
            else Unread = Messages.Where(x => !x.IsSeen).Count();
        }

        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }
        
        public LocalFolder LocalName { get; private set; }

        public ObservableCollection<MessageVM>? Messages { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
