using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using log4net;

namespace OpenBus.Common.Configs
{
    /// <summary>
    /// Helper class for accessing the app.config file
    /// </summary>
    public static class ConfigHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConfigHelper));

        /// <summary>
        /// Gets a setting from the AppSettings section from app.config or web.config
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSettingsName"></param>
        /// <returns>The value as the given type argument</returns>
        public static T GetSettingFromConfig<T>(string appSettingsName)
        {
            string setting = ConfigurationManager.AppSettings[appSettingsName];

            if(String.IsNullOrEmpty(setting))
            {
                Logger.Error(String.Format("ConfigHelper: Could not configuration element '{0}' from app.config.", appSettingsName));
                return default(T);
            }

            try
            {
                return (T)Convert.ChangeType(setting, typeof(T));
            }
            catch (Exception ex)
            {
                Logger.Error("ConfigHelper: Could not convert type.", ex);
            }
            return default(T);
        }

        /// <summary>
        /// Gets a setting from app.config or web.config as a list.
        /// That is, if the setting is separated by , or ; or | it is returned as a list of strings
        /// </summary>
        /// <param name="appSettingsName"></param>
        /// <returns></returns>
        public static List<T> GetListFromConfig<T>(string appSettingsName)
        {
            if (String.IsNullOrEmpty(appSettingsName))
            {
                Logger.Error("ConfigHelper: appSettingsName is null or empty");
                return new List<T>();
            }

            string setting = ConfigurationManager.AppSettings[appSettingsName];

            if (String.IsNullOrEmpty(setting))
            {
                Logger.Error(String.Format("ConfigHelper: Could not configuration element '{0}' from app.config.", appSettingsName));
                return new List<T>();
            }

            // Make the array and ensure no empty entries
            string[] itemNamesArr = setting.Split(new [] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);

            // Trim each setting name
            try
            {
                return itemNamesArr.Select(s => (T) Convert.ChangeType(s.Trim(), typeof (T))).ToList();
            }
            catch(Exception ex)
            {
                Logger.Error("ConfigHelper: Could not convert type.", ex);
            }
            return new List<T>();
        }
    }
}
