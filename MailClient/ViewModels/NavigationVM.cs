using MailClient.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace MailClient.ViewModels
{
    internal class NavigationVM : ViewModelBase
    {
        private void Start()
        {
            UserControls.StartPageControl.LoadingStart += loadingStart;
            CurrentControl = UserControls.StartPageControl;
        }

        private async void loadingStart()
        {
            UserControls.MailClientControl.Service = UserControls.StartPageControl.SelectedService;
            UserControls.MailClientControl.EmailClient = UserControls.StartPageControl.MailClient;
            UserControls.MailClientControl.LogOut += LogOut;
            await UserControls.MailClientControl.SyncFolders();
            CurrentControl = UserControls.MailClientControl;
        }


        private void LogOut()
        {
            if( MessageBox.Show("Are you sure you want to log out?", "Log out" ,MessageBoxButton.YesNo) == MessageBoxResult.No ) return;
            ConfigUtility.UnlockSection("appSettings");
            ConfigUtility.AddUpdateAppSettings("Email", string.Empty);
            ConfigUtility.AddUpdateAppSettings("Password", string.Empty);
            ConfigUtility.AddUpdateAppSettings("CurrentService", Services.Google.ToString());
            ConfigUtility.LockSection("appSettings");
            UserControls.Reset();
            Start();
        }

        public NavigationVM()
        {
            Start();
        }
    }
}
