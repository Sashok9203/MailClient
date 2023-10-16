using MailClient.Utility;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MailClient.ViewModels
{
    internal class MailListControlVM : ViewModelBase
    {
        public FolderVM currentFolder;


        private void selectMail(object o)
        {
            if (o is MessageVM message)
            {
                CurrentFolder.Folder.AddFlags(message.UniqueId,  MessageFlags.Seen,false);
                message.IsSeen = true;
                foreach (var item in MailsBox.Values)
                    item.UnreadApdateAsync();
                SetFlagsAllMessage(MessageFlags.Seen, message.Message.MessageId, message.IsSeen);
                MailSelected.Invoke(message);
            }
        }

        private async  void seenInvert(object o)
        {
            if (o is MessageVM message)
            {
                await changeFlag(message, MessageFlags.Seen);
                message.IsSeen = !message.IsSeen;
                foreach (var item in MailsBox.Values)
                    item.UnreadApdateAsync();
                SetFlagsAllMessage(MessageFlags.Seen, message.Message.MessageId, message.IsSeen);
            }
        }

        private void SetFlagsAllMessage(MessageFlags flag,string messageId, bool condition)
        {
            var messages = MailsBox.Values.SelectMany(x => Messages.Where(x => x.Message.MessageId == messageId));
            foreach (var item in messages)
            {
                switch (flag)
                {
                    case MessageFlags.Seen:
                        item.IsSeen = condition;
                        break;
                    case MessageFlags.Flagged:
                        item.Flagget = condition;
                        break;
                }
            }
        }

        private void deleteMailFromFolders(IEnumerable<FolderVM> folders, string messageId)
        {
            foreach (var item in folders)
            {
                var delMessage = item?.Messages?.FirstOrDefault(x => x.Message.MessageId == messageId);
                if (delMessage != null)
                    item?.Messages?.Remove(delMessage);
            }
            foreach (var item in MailsBox.Select(y => y.Value))
                item.UnreadApdateAsync();
        }

        private void mailToSpam(object o)
        {
            if (o is MessageVM message)
            {
                var spamFolder = MailsBox.Values.FirstOrDefault(x => x.LocalName == LocalFolder.Junk);
                spamFolder?.Messages?.Add(message);
                CurrentFolder.Folder.MoveTo(message.UniqueId, spamFolder.Folder);
                var  folders = MailsBox.Where(x => x.Value.LocalName != LocalFolder.Junk).Select(y => y.Value);
                deleteMailFromFolders(folders, message.Message.MessageId);
            }
        }


        private async Task deleteMail(object o)
        {
            if (o is MessageVM message)
            {
                IEnumerable<FolderVM> folders;
                if (CurrentFolder.LocalName != LocalFolder.Trash)
                {
                    var trashFolder = MailsBox.Values.FirstOrDefault(x => x.LocalName == LocalFolder.Trash);
                    trashFolder?.Messages?.Add(message);
                    CurrentFolder.Folder.MoveTo(message.UniqueId, trashFolder.Folder);
                    folders = MailsBox.Where(x => x.Value.LocalName != LocalFolder.Trash).Select(y => y.Value);
                }
                else
                {
                    if (!CurrentFolder.Folder.IsOpen) await CurrentFolder.Folder.OpenAsync( FolderAccess.ReadWrite);
                    CurrentFolder.Folder.AddFlags(message.UniqueId,  MessageFlags.Deleted, true);
                    CurrentFolder.Folder.Expunge();
                    folders = MailsBox.Select(y => y.Value);
                }
                deleteMailFromFolders(folders, message.Message.MessageId);
                MailHasChecked.Invoke();
            }
        }

        private  void importanceInvert(object o)
        {
            if (o is MessageVM message)
            {
                if (message.Importance)
                {
                    message.Message.Priority =   MessagePriority.Normal;

                }
                else
                {

                    message.Message.Priority = MessagePriority.Urgent;
                }
                message.Importance = !message.Importance;
                var messages = MailsBox.Values.SelectMany(x => Messages.Where(x => x.Message.MessageId == message.Message.MessageId));
                foreach (var item in messages)
                    item.Importance = message.Importance;
             }
        }

        private async void flaggetInvert(object o)
        {
            if (o is MessageVM message)
            {
                await changeFlag(message, MessageFlags.Flagged);
                message.Flagget = !message.Flagget;
                FolderVM? flaggetFolder = MailsBox.Values.FirstOrDefault(x => x.LocalName == LocalFolder.Flagged);
                if (flaggetFolder != null)
                {
                    if (message.Flagget)
                    {
                        flaggetFolder.Messages?.Add(message);
                        flaggetFolder.UnreadApdateAsync();
                    }
                    else
                    {
                        flaggetFolder.Messages?.Remove(flaggetFolder.Messages?.First(x => x.Message.MessageId == message.Message.MessageId));
                        flaggetFolder.UnreadApdateAsync();
                    }
                    
                }
                SetFlagsAllMessage(MessageFlags.Flagged, message.Message.MessageId, message.Flagget);

            }
        }

        
        private async Task changeFlag(MessageVM message, MessageFlags flag)
        {
           await Task.Run(() =>
            {
                bool condition = flag switch
                {
                    MessageFlags.Seen => message.IsSeen,
                    MessageFlags.Flagged => message.Flagget,
                    MessageFlags.None => throw new NotImplementedException(),
                    MessageFlags.Answered => throw new NotImplementedException(),
                    MessageFlags.Deleted => throw new NotImplementedException(),
                    MessageFlags.Draft => throw new NotImplementedException(),
                    MessageFlags.Recent => throw new NotImplementedException(),
                    MessageFlags.UserDefined => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };
               
                lock (CurrentFolder.Folder.SyncRoot)
                {
                    if (!CurrentFolder.Folder.IsOpen) CurrentFolder.Folder.Open( FolderAccess.ReadWrite);
                        if (condition)
                        CurrentFolder.Folder.RemoveFlags(message.UniqueId, flag, HasEnableExt);
                    else
                        CurrentFolder.Folder.AddFlags(message.UniqueId, flag, HasEnableExt);
                }
            });
        }

        

        public FolderVM CurrentFolder 
        {
           
            private get => currentFolder;
            set
            {
                currentFolder = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        public bool HasEnableExt { set; private get; }

        public Dictionary<IMailFolder, FolderVM> MailsBox { private get; set; }

        public ObservableCollection<MessageVM>? Messages => CurrentFolder.Messages;

        public event Action MailHasChecked;

        public event Action<MessageVM> MailSelected;


        public RelayCommand SelectMail => new((o) => selectMail(o));
        public RelayCommand SeenInvert => new( (o) =>  seenInvert(o));
        public RelayCommand FlaggetInvert => new((o) => flaggetInvert(o));
        public RelayCommand ImportanceInvert => new((o) => importanceInvert(o));
        public RelayCommand DeleteMail => new((o) => deleteMail(o));
        public RelayCommand MailToSpam => new((o) => mailToSpam(o));
        public RelayCommand MailChecked => new((o) => MailHasChecked.Invoke());

    }
}
