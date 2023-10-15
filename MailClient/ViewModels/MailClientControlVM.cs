﻿using MailClient.Utility;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MailClient.ViewModels
{
    internal class MailClientControlVM : ViewModelBase, IDisposable
    {
        private Dictionary<IMailFolder, FolderVM> mails = new();
        private ImapClient emailClient;
        private FolderVM selectedFolder, prevFolder;
        private FolderVM? selectedParent;
        private bool disposedValue, isNewMailWindowOpen, groupCheckEnabled, isCreateNewFolderWindoOpen;
        private bool? gCheck = false;
        public static bool isWindowFull;
        private string newFolderName = string.Empty;

        private bool? groupCheck
        {
            get => gCheck;
            set 
            {
                gCheck = value;
                OnPropertyChanged(nameof(GroupCheck));
            }
        }

        private FolderVM SelectedFolder
        {
            get => selectedFolder;
            set
            {
                selectedFolder = value;
                selectedFolder.Selected = true;
                if (prevFolder != null) prevFolder.Selected = false;
                prevFolder = selectedFolder;

            }
        }

        private void exit()
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Application exit", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
            UserControls.Reset();
            Environment.Exit(0);
        }

        private void newMail()
        {
            IsNewMailWindowOpen = true;
        }

        private async void folderSubscribe(IMailFolder folder)
        {
            folder.CountChanged += OnCountChanged;

            folder.MessageExpunged += OnMessageExpunged;

            folder.MessageFlagsChanged += OnMessageFlagsChanged;

            folder.UnreadChanged += OnMessageUnreadChanged;
            await Task.Run(() =>
            {
                lock (folder.SyncRoot)
                {
                    folder.Subscribe();
                }
            });
        }



        private void folderUnsubscribe(IMailFolder folder)
        {
            folder.CountChanged -= OnCountChanged;

            folder.MessageExpunged -= OnMessageExpunged;

            folder.MessageFlagsChanged -= OnMessageFlagsChanged;

        }


        private void OnMessageUnreadChanged(object? sender, EventArgs e)
        {


            ////var message = mails[(IMailFolder)sender].FirstOrDefault(x => x.UniqueId == e.UniqueId);
            ////message.IsSeen = !message.IsSeen;
            //foreach (var item in Folders)
            //    item.UnreadApdateAsync();

        }

        async void OnMessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e)
        {
            //var message = mails[(IMailFolder)sender].FirstOrDefault(x => x.UniqueId == e.UniqueId);
            //switch (e.Flags)
            //{
            //    case MessageFlags.None:
            //    case MessageFlags.Seen:
            //        message.IsSeen = !message.IsSeen;
            //        foreach (var item in Folders)
            //            item.UnreadApdateAsync();
            //        break;

            //}


            //var folder = (ImapFolder)sender;

            //Console.WriteLine("{0}: flags have changed for message #{1} ({2}).", folder, e.Index, e.Flags);
        }

        void OnCountChanged(object sender, EventArgs e)
        {
            //var folder = (ImapFolder)sender;

            //// Note: because we are keeping track of the MessageExpunged event and updating our
            //// 'messages' list, we know that if we get a CountChanged event and folder.Count is
            //// larger than messages.Count, then it means that new messages have arrived.
            //if (folder.Count > messages.Count)
            //{
            //    int arrived = folder.Count - messages.Count;

            //    if (arrived > 1)
            //        Console.WriteLine("\t{0} new messages have arrived.", arrived);
            //    else
            //        Console.WriteLine("\t1 new message has arrived.");

            //    // Note: your first instinct may be to fetch these new messages now, but you cannot do
            //    // that in this event handler (the ImapFolder is not re-entrant).
            //    // 
            //    // Instead, cancel the `done` token and update our state so that we know new messages
            //    // have arrived. We'll fetch the summaries for these new messages later...
            //    messagesArrived = true;
            //    done?.Cancel();
            //}
        }

        void OnMessageExpunged(object sender, MessageEventArgs e)
        {
            //var folder = (ImapFolder)sender;

            //if (e.Index < messages.Count)
            //{
            //    var message = messages[e.Index];

            //    Console.WriteLine("{0}: message #{1} has been expunged: {2}", folder, e.Index, message.Envelope.Subject);

            //    // Note: If you are keeping a local cache of message information
            //    // (e.g. MessageSummary data) for the folder, then you'll need
            //    // to remove the message at e.Index.
            //    messages.RemoveAt(e.Index);
            //}
            //else
            //{
            //    Console.WriteLine("{0}: message #{1} has been expunged.", folder, e.Index);
            //}
        }

        public async Task getMessagesAsync(IMailFolder path, bool forceReload)
        {
            GroupCheck = GroupCheckEnabled = false;

            if (mails[path].Messages != null && !forceReload) return;
            mails[path].Messages = new();
           
            CurrentControl = UserControls.WaitLoadControl;
            var result = await Task.Run(() =>
            {
                lock (path.SyncRoot)
                {
                    path.Open(FolderAccess.ReadWrite);
                }
            }).ContinueWith((o) =>
            {
                lock (path.SyncRoot)
                {
                    return path.Search(SearchQuery.All);
                }
            });

            foreach (var id in result)
            {
                MessageVM mVM = new();
                var info = await Task.Run(() =>
                {
                    lock (path.SyncRoot)
                    {
                        var res = path.Fetch(new[] { id }, MessageSummaryItems.Flags | MessageSummaryItems.GMailLabels);
                        mVM.Message = path.GetMessage(id);
                        return res;
                    }
                });
                mVM.IsSeen = info[0]?.Flags.Value.HasFlag(MessageFlags.Seen) ?? false;
                mVM.Flagget = info[0]?.Flags.Value.HasFlag(MessageFlags.Flagged) ?? false;
                mVM.Importance = info[0].GMailLabels?.Contains("Important") ?? false;

                mVM.UniqueId = id;
                mVM.Attachment = mVM.Message.Attachments.Any();
                mails[path].Messages?.Add(mVM);
            }
           
            CurrentControl = UserControls.MailListControl;
            GroupCheckEnabled = true;
        }

        private async Task selectFolderAsync(object o)
        {
            SelectedFolder = o as FolderVM;
            await getMessagesAsync(SelectedFolder.Folder, false);
            UserControls.MailListControl.CurrentFolder = SelectedFolder;
        }

        public async Task<FolderVM[]?> SyncFolders()
        {
            var folders = await Task.Run(() =>
            {
                lock (emailClient.SyncRoot)
                {
                    var res = emailClient.GetFolders(emailClient.PersonalNamespaces[0]);
                    return res;
                }

            });

            var changetFolders = mails.Keys.Union(folders).Except(mails.Keys.Intersect(folders));
            List<FolderVM> added = new();
            foreach (var item in changetFolders)
            {
                if (!mails.Remove(item))
                {
                    if (item.Name == "[Gmail]") continue;
                    FolderVM temp = new(item, LocalFolder.User);
                    mails.Add(item, temp);
                    added.Add(temp);
                }
            }
            foreach (var item in added)
                folderSubscribe(item.Folder);
            await selectFolderAsync(mails.Values.First(x => x.LocalName == LocalFolder.Inbox));
            return added.Count == 0 ? null : added.ToArray();
        }

        public ImapClient EmailClient
        {
            set
            {
                emailClient = value;
                GetSpecialFolders(value);
            }
        }

        private void addFolder()
        {
            IsCreateNewFolderWindoOpen = true;
            NewFolderName = string.Empty;
            SelectedParent = null;
        }

        private void mailChecked()
        {
            int checkedMailCount = SelectedFolder.Messages.Where(x => x.IsChecked).Count();
            if (checkedMailCount == SelectedFolder.Messages.Count) groupCheck = true;
            else if (checkedMailCount == 0) groupCheck = false;
            else groupCheck = null;
        }


        private void deleteFolder(object o)
        {
            if (o is FolderVM folder)
            {
                folder.Folder.Delete();
                mails.Remove(folder.Folder);
                OnPropertyChanged(nameof(Folders));
            }
        }

        private async Task deleteMessages()
        {
            var messsges = SelectedFolder?.Messages?.Where(x => x.IsChecked).ToArray();
            IEnumerable<FolderVM>? folders = null;
            if (SelectedFolder.LocalName != LocalFolder.Trash)
            {
                var trashFolder = mails.Values.FirstOrDefault(x => x.LocalName == LocalFolder.Trash);
                foreach (var item in messsges)
                {
                    trashFolder?.Messages?.Add(item);
                    await SelectedFolder.Folder.MoveToAsync(item.UniqueId, trashFolder.Folder);
                   
                }
                folders = mails.Where(x => x.Value.LocalName != LocalFolder.Trash).Select(y => y.Value);
            }
            else
            {
                foreach (var item in messsges)
                {
                    if (!SelectedFolder.Folder.IsOpen) await SelectedFolder.Folder.OpenAsync(FolderAccess.ReadWrite);
                    await SelectedFolder.Folder.AddFlagsAsync(item.UniqueId, MessageFlags.Deleted, true);
                    await SelectedFolder.Folder.ExpungeAsync();
                    
                }
                folders = mails.Select(y => y.Value);
            }

            foreach (var folder in folders)
            {
                foreach (var item in messsges)
                {
                    var delMessage = folder?.Messages?.FirstOrDefault(x => x.Message.MessageId == item.Message.MessageId);
                    if (delMessage != null)
                        folder?.Messages?.Remove(delMessage);
                }
            }
            foreach (var item in mails.Select(y => y.Value))
                item.UnreadApdateAsync();
            mailChecked();
            OnPropertyChanged(nameof(GroupCheck));
        }
        


        private void newFolder()
        {
            IMailFolder folder = SelectedParent == null ? emailClient.GetFolder(emailClient.PersonalNamespaces[0]).Create(NewFolderName, true) :
                                 SelectedParent.Folder.Create(NewFolderName, true);
            mails.Add(folder, new(folder, LocalFolder.User));
            IsCreateNewFolderWindoOpen = false;
            OnPropertyChanged(nameof(Folders));
        }

        
        private void GetSpecialFolders(ImapClient value)
        {
            UserControls.MailListControl.HasEnableExt = value.Capabilities.HasFlag(ImapCapabilities.Enable);

            if ((emailClient.Capabilities & (ImapCapabilities.SpecialUse | ImapCapabilities.XList)) != 0)
            {
                IMailFolder folder = emailClient.GetFolder(SpecialFolder.All);
                if (folder != null)
                    mails.Add(folder, new(folder, LocalFolder.All) { Icon = Resource.all });
                
                folder = emailClient.Inbox;
                mails.Add(folder, new(folder, LocalFolder.Inbox) { Icon = Resource.inbox });


                folder = emailClient.GetFolder(SpecialFolder.Sent);
                if (folder != null)
                    mails.Add(folder, new(folder, LocalFolder.Send) { Icon = Resource.send });
                

                folder = emailClient.GetFolder(SpecialFolder.Trash);
                if (folder != null)
                    mails.Add(folder, new(folder, LocalFolder.Trash) { Icon = Resource.trash });
               

                folder = emailClient.GetFolder(SpecialFolder.Drafts);
                if (folder != null)
                   mails.Add(folder, new(folder, LocalFolder.Drafts) { Icon = Resource.drafts });
               
                folder = emailClient.GetFolder(SpecialFolder.Important);
                if (folder != null)
                   mails.Add(folder, new(folder, LocalFolder.Important) { Icon = Resource.important_folder });
                

                folder = emailClient.GetFolder(SpecialFolder.Flagged);
                if (folder != null)
                   mails.Add(folder, new(folder, LocalFolder.Flagged) { Icon = Resource.mark });
                

                folder = emailClient.GetFolder(SpecialFolder.Junk);
                if (folder != null)
                    mails.Add(folder, new(folder, LocalFolder.Junk) { Icon = Resource.spam });
                foreach (var item in Folders)
                    folderSubscribe(item.Folder);
            }
        }

        public Services Service { get; set; }

        public IEnumerable<FolderVM> Folders => mails.Values.ToArray(); 

        public event Action LogOut;


        

        public bool IsCreateNewFolderWindoOpen
        {
            get => isCreateNewFolderWindoOpen;
            set
            {
                isCreateNewFolderWindoOpen = value;
                OnPropertyChanged();
            }
        }

        public bool IsNewMailWindowOpen
        {
            get => isNewMailWindowOpen;
            set 
            {
                isNewMailWindowOpen = value;
                OnPropertyChanged();
            }
        }

        public bool? GroupCheck
        {
            get => groupCheck;
            set
            {
                if (SelectedFolder.Messages == null) return;
                groupCheck = value;
                if ( value == null)
                    groupCheck = false;
                if (groupCheck == false)
                {
                    foreach (var item in SelectedFolder.Messages)
                        item.IsChecked = false;
                }
                else
                {
                    groupCheck = true;
                    foreach (var item in SelectedFolder.Messages)
                        item.IsChecked = true;

                }
                OnPropertyChanged();

            }
        }


        public bool IsWindowFull
        {
            get => isWindowFull;
            set 
            {
                isWindowFull = value;
                OnPropertyChanged();
                if (value)
                {
                    Row = 1;
                    Col = 1;
                }
                else
                {
                    Row = 3;
                    Col = 2;
                }
                OnPropertyChanged(nameof(Row));
                OnPropertyChanged(nameof(Col));
            }
        }

        public int Row { get; set; } = 3;
        public int Col { get; set; } = 2;

        public bool GroupCheckEnabled
        {
            get => groupCheckEnabled;
            set 
            {
                groupCheckEnabled = value;
                OnPropertyChanged();
            }
        }


        public IEnumerable<FolderVM> ParentFolders => mails.Values.Where(x=>x.LocalName == LocalFolder.User);

        public FolderVM? SelectedParent
        {
            get => selectedParent;
            set
            {
                selectedParent = value;
                OnPropertyChanged();
            }
        }
        public string NewFolderName
        {
            get => newFolderName;
            set
            {
                newFolderName = value;
                OnPropertyChanged();
            }
        }




        public RelayCommand SelectFolder => new(async (o)=> await selectFolderAsync(o),(o)=> CurrentControl != UserControls.WaitLoadControl);
        public RelayCommand Logout => new( (o) => LogOut.Invoke());
        public RelayCommand Exit => new((o) => exit());
        public RelayCommand WindowResize => new((o) => IsWindowFull = !IsWindowFull);
        public RelayCommand CloseNewMailWindow => new((o) =>  IsNewMailWindowOpen = isWindowFull = false);
        public RelayCommand NewMail => new((o) => newMail());
        public RelayCommand ReloadFolder => new( async (o) => await getMessagesAsync(SelectedFolder.Folder,true), (o) => CurrentControl != UserControls.WaitLoadControl);
        public RelayCommand AddFolder => new((o) => addFolder());
        public RelayCommand CreateNewFolder => new((o) => newFolder(),(o) => !string.IsNullOrWhiteSpace(NewFolderName) && !mails.Keys.Any(x=>x.Name == NewFolderName));
        public RelayCommand Cancel => new((o) => IsCreateNewFolderWindoOpen = false);
        public RelayCommand DeleteFolder => new((o) => deleteFolder(o), (o) => o is FolderVM folder && folder != SelectedFolder && folder.LocalName == LocalFolder.User);
        public RelayCommand DeleteMessages => new(async (o) =>await deleteMessages(), (o) => GroupCheck != false);

        public MailClientControlVM()
        {
            UserControls.WaitLoadControl.LoadingText = "Loading messages...";
            UserControls.WaitLoadControl.TextColor = Brushes.White;
            UserControls.MailListControl.MailsBox = mails;
            UserControls.MailListControl.MailHasChecked += mailChecked;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (emailClient != null && emailClient.IsConnected)
                    {
                        emailClient.Disconnect(true);
                        emailClient.Dispose();
                        emailClient = null;
                    }
                    mails.Clear();
                    mails = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}