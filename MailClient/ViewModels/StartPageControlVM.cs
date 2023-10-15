using MailClient.Controls;
using MailClient.Utility;
using MailKit.Net.Imap;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MailClient.ViewModels
{
    internal class StartPageControlVM :ViewModelBase,IDisposable
    {
        private Services selectedService;
        private bool disposedValue;

        private async Task logIn( string email,string password,Services service)
        {
            UserControls.WaitLoadControl.LoadingText = "Authorization ...";
            CurrentControl = UserControls.WaitLoadControl;
            try
            {
                MailClient = new();
                ConnectionParams cParams = Connection.GetIMAPConnectionParams(service);
                await MailClient.ConnectAsync(cParams.Server, cParams.Port, SecureSocketOptions.SslOnConnect);
                MailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                await MailClient.AuthenticateAsync(email, password);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
                MailClient?.Dispose();
                MailClient = null;
                return;
            }
            finally
            {
                if (MailClient == null)
                    CurrentControl = UserControls.AuthorizationControl;
                else
                {
                    CurrentControl = UserControls.WaitLoadControlNew;
                    (CurrentControl as WaitLoadControlVM).LoadingText = "Loading ...";
                    ConfigUtility.AddUpdateAppSettings("Email", email);
                    ConfigUtility.AddUpdateAppSettings("Password", password);
                    ConfigUtility.AddUpdateAppSettings("CurrentService", service.ToString());
                    ConfigUtility.LockSection("appSettings");
                    LoadingStart?.Invoke();
                }
            }
        }

        private void changeImage(Services service) => SelectedService = service;
        
        public Services SelectedService
        {
            get => selectedService;
            set 
            {
                selectedService = value;
                OnPropertyChanged();
            }
        }

        private async void authorization()
        {
            
            ConfigUtility.UnlockSection("appSettings");
            string serviseStr = ConfigurationManager.AppSettings["CurrentService"] ?? string.Empty;
            _ = Enum.TryParse(serviseStr, out Services service);
            SelectedService = service;
            UserControls.AuthorizationControl.SelectedService = serviseStr;
            string Password;
            string Email = ConfigurationManager.AppSettings["Email"] ?? string.Empty;
            if (!string.IsNullOrEmpty(Email))
            {
                Password = ConfigurationManager.AppSettings["Password"] ?? string.Empty;
                await logIn(Email, Password, SelectedService);
                return;
            }
             UserControls.AuthorizationControl.SelectedService = Services.Google.ToString();

            CurrentControl = UserControls.AuthorizationControl;
        }

        public ImapClient? MailClient { get; private set; }

        public event Action? LoadingStart;

        public StartPageControlVM()
        {
            UserControls.AuthorizationControl.ServiceChanget += changeImage;
            UserControls.AuthorizationControl.ButtonClick += logIn;
            authorization();
        }

        protected virtual async void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (MailClient != null && MailClient.IsConnected)
                    {
                        MailClient.Disconnect(true);
                        MailClient.Dispose();
                        MailClient = null;
                    }
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
