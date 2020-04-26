using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ophelia
{
    public static class ConfigurationManager
    {
        public static T GetParameter<T>(string key, T defaultValue = default(T)) where T : struct
        {
            try
            {
                object value = GetParameter(key);
                return value as T? ?? defaultValue;
            }
            catch { return defaultValue; }
        }
        public static string GetParameter(string key, string defaultValue = "")
        {
            try
            {
                string returnValue = defaultValue;
                if (!string.IsNullOrEmpty(key) && System.Configuration.ConfigurationManager.AppSettings[key] != null)
                    returnValue = System.Configuration.ConfigurationManager.AppSettings[key];
                return returnValue;
            }
            catch { return defaultValue; }
        }
        public static void SetParameter(string key, string value)
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                    if (config.AppSettings.Settings[key] == null)
                        config.AppSettings.Settings.Add(key, value);
                    else
                        config.AppSettings.Settings[key].Value = value;
                    config.Save(System.Configuration.ConfigurationSaveMode.Modified);
                    System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                }
            }
            catch {
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            }
        }
        public static void AddKey(string key, string value)
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                    if (config.AppSettings.Settings[key] == null)
                        config.AppSettings.Settings.Add(key, value);
                    else
                        config.AppSettings.Settings[key].Value = value;
                    config.Save(System.Configuration.ConfigurationSaveMode.Modified);
                    System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                }
            }
            catch (Exception ex)
            {
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            }
        }
        public static void RemoveKey(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                    if (config.AppSettings.Settings[key] != null)
                    {
                        config.AppSettings.Settings.Remove(key);
                        config.Save(System.Configuration.ConfigurationSaveMode.Modified);
                        System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                    }
                }
            }
            catch {
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            }
        }
    }
}
