using MailClient.Utility;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MailClient.ViewModels
{
    internal class AuthorizationControlVM : ViewModelBase
    {
        private string selectedService = string.Empty;

        private Services service;

        public event  Func<string,string, Services,Task> ButtonClick;

        public string SelectedService 
        {
            get => selectedService;
            set
            {
                selectedService = value;
                OnPropertyChanged();
                _ = Enum.TryParse(value, out service);
                ServiceChanget.Invoke(service);
            }
        }

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public event Action<Services> ServiceChanget;

        public string[] ServicesNames => Enum.GetNames(typeof(Services));

        public RelayCommand LogIn => new( async (o) => await ButtonClick(Email, Password , service), (o)=> !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password));
    }
}
