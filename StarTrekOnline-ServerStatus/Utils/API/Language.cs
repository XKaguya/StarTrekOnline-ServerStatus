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

            // 根据 language 参数设置不同的资源文件名称
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
            
            logWindow.Title = ((LanguageManager.GetLocalizedString("LogWindowTitle")));
            
            setWindow.Title = ((LanguageManager.GetLocalizedString("SetWindowTitle")));
            setWindow.LanguageText.Text = ((LanguageManager.GetLocalizedString("Language_Text")));
            setWindow.Audio_Text.Text = ((LanguageManager.GetLocalizedString("Audio_Text")));
            setWindow.MusicButton.Content = ((LanguageManager.GetLocalizedString("MusicButton")));
            setWindow.Audio_Notification_Text.Text = ((LanguageManager.GetLocalizedString("Audio_Notification_Text")));

            mainWindow.Title = ((LanguageManager.GetLocalizedString("Program_Title")));
            mainWindow.Message_Title.Text = ((LanguageManager.GetLocalizedString("Message_Title")));
            mainWindow.Log_Title.Text = ((LanguageManager.GetLocalizedString("Log_Title")));
            mainWindow.Settings_Title.Text = ((LanguageManager.GetLocalizedString("Settings_Title")));
            mainWindow.ServerStatus.Text = ((LanguageManager.GetLocalizedString("ServerStatus_Title")));
        }
    }
}