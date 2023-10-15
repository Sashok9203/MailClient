using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailClient.Utility
{
    internal static class ConfigUtility
    {
        private static readonly Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public static bool UnlockSection(string sectionName)
        {
            
            ConfigurationSection section = config.GetSection(sectionName);
            if (section != null && section.SectionInformation.IsProtected)
            {
                section.SectionInformation.UnprotectSection();
                return true;
            }
            return false;
        }

        public static bool LockSection(string sectionName)
        {
            ConfigurationSection section = config.GetSection(sectionName);
            if (section != null && !section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                config.Save();
                return true;
            }
            return false;
        }

        public static bool AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var settings = config.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                return true;
            }
            catch (ConfigurationErrorsException)
            {
                return false;

            }
        }
    }
}
