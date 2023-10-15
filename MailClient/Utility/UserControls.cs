using MailClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailClient.Utility
{
    internal static class UserControls
    {
        private static AuthorizationControlVM? authorizationControlVM = null;
        public static AuthorizationControlVM AuthorizationControl => authorizationControlVM ??= new();

        private static MailClientControlVM? mailClientControlVM = null;
        public static MailClientControlVM MailClientControl => mailClientControlVM ??= new();
       
        private static MailListControlVM? mailListControlVM = null;
        public static MailListControlVM MailListControl => mailListControlVM ??= new();
        
        private static StartPageControlVM? startPageControlVM = null;
        public static StartPageControlVM StartPageControl => startPageControlVM ??= new();
        
        private static WaitLoadControlVM? waitLoadControlVM = null;
        public static WaitLoadControlVM WaitLoadControl => waitLoadControlVM ??= new();
        public static WaitLoadControlVM WaitLoadControlNew => new();

        public static void Reset()
        {
            
            authorizationControlVM = null;
            mailClientControlVM?.Dispose();
            mailClientControlVM = null;
            mailListControlVM = null;
            startPageControlVM?.Dispose();
            startPageControlVM = null;
            waitLoadControlVM = null;
        }

        public static void Reset(object type)
        {
            switch (type) 
            {
                case AuthorizationControlVM:
                    authorizationControlVM = null;
                    break;
                case MailClientControlVM:
                    mailClientControlVM?.Dispose();
                    mailClientControlVM = null;
                    break;
                case MailListControlVM:
                    mailListControlVM = null;
                    break;
                case StartPageControlVM:
                    startPageControlVM?.Dispose();
                    startPageControlVM = null;
                    break;
                case WaitLoadControlVM:
                    waitLoadControlVM = null;
                    break;
                default: throw new NotImplementedException();
            }
        }
    }
}
