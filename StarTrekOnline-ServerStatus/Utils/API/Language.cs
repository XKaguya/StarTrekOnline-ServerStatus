using System;
using System.Globalization;
using System.Resources;

namespace StarTrekOnline_ServerStatus.Utils.API
{
    public static class LanguageManager
    {
        private static ResourceManager resourceManager;

        static LanguageManager()
        {
            SetLanguage("en-US");
        }

        public static string CurrentLanguage()
        {
            return SetWindow.Language;
        }

        public static void SetLanguage(string language)
        {
            string resourceName = "StarTrekOnline_ServerStatus.Language";
            
            if (language == "en-US")
            {
                resourceName = "StarTrekOnline_ServerStatus.Language";
            }
            else if (language == "zh-CN")
            {
                resourceName = "StarTrekOnline_ServerStatus.Language-CN";
            }

            CultureInfo cultureInfo = new CultureInfo(language);

            resourceManager = new ResourceManager(resourceName, typeof(LanguageManager).Assembly);
            resourceManager.IgnoreCase = true;
        }


        public static string GetLocalizedString(string key)
        {
            if (resourceManager != null)
            {
                try
                {
                    return resourceManager.GetString(key);
                }
                catch (MissingManifestResourceException)
                {
                    return $"[{key}]";
                }
            }
            return $"[{key}]";
        }
    }

    
    public static class Language
    {
        private static LogWindow logWindow = LogWindow.Instance;
        private static SetWindow setWindow = SetWindow.Instance;
        private static MainWindow mainWindow = App.CurrentMainWindow;
        public static void SwitchLanguage(string language)
        {
            Logger.Log(LanguageManager.CurrentLanguage());

            API.ChangeTextBlockContent(mainWindow.ServerStatus, LanguageManager.GetLocalizedString("ServerStatus_Title") + LanguageManager.GetLocalizedString("ServerStatus_Default"));
            API.ChangeTextBlockContent(mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("Message_Content") + 0 + LanguageManager.GetLocalizedString("days") + 0 + LanguageManager.GetLocalizedString("hours") + 0 + LanguageManager.GetLocalizedString("minutes") + 0 + LanguageManager.GetLocalizedString("seconds"));    

            API.ChangeWindowTitle(mainWindow, LanguageManager.GetLocalizedString("Program_Title"));
            API.ChangeWindowTitle(logWindow, LanguageManager.GetLocalizedString("LogWindowTitle"));
            API.ChangeWindowTitle(setWindow, LanguageManager.GetLocalizedString("SetWindowTitle"));
            API.ChangeTextBlockContent(mainWindow.Message_Title, LanguageManager.GetLocalizedString("Message_Title"));
            API.ChangeButtonContent(mainWindow.Log, LanguageManager.GetLocalizedString("Log_Title"));
            API.ChangeTextBlockContent(mainWindow.Recent_Events, LanguageManager.GetLocalizedString("Recent_Events_Title"));
            API.ChangeButtonContent(mainWindow.Settings, LanguageManager.GetLocalizedString("Settings_Title"));
            API.ChangeButtonContent(mainWindow.Reload, LanguageManager.GetLocalizedString("Reload"));
            API.ChangeTextBlockContent(mainWindow.RecentNews, LanguageManager.GetLocalizedString("RecentNews"));

            API.ChangeTextBlockContent(setWindow.LanguageText, LanguageManager.GetLocalizedString("Language_Text"));
            API.ChangeTextBlockContent(setWindow.Audio_Text, LanguageManager.GetLocalizedString("Audio_Text"));
            API.ChangeTextBlockContent(setWindow.Audio_Notification_Text, LanguageManager.GetLocalizedString("Audio_Notification_Text"));

            API.ChangeButtonContent(setWindow.MusicButton, LanguageManager.GetLocalizedString("MusicButton"));
        }
    }
}