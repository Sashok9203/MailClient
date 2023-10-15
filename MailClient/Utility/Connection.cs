using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MailClient.Utility
{
    public enum Services
    {
        Google,
        UkrNet,
        ICloud
    }

    public struct ConnectionParams
    {
        public int Port;
        public string Server;
    }


    internal static  class Connection
    {

        static public ConnectionParams GetIMAPConnectionParams(Services service)
        {
            return service switch
            {
                Services.ICloud => new() { Port = 993, Server = "imap.mail.me.com" },
                Services.Google => new() { Port = 993, Server = "imap.gmail.com" },
                Services.UkrNet => new() { Port = 993, Server = "imap.ukr.net" },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        static public ConnectionParams GetSMTPConnectionParams(Services service)
        {
            return service switch
            {
                Services.ICloud => new() { Port = 25, Server = "smtp.mail.me.com" },
                Services.Google => new() { Port = 465, Server = "smtp.gmail.com" },
                Services.UkrNet => new() { Port = 465, Server = "smtp.ukr.net" },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

    }
}
