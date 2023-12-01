using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace StarTrekOnline_ServerStatus.Utils.API
{
    public static class API
    {
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
            {
                Logger.Log($"Parent found. {parent}");
                return parent;
            }
            else
            {
                Logger.Log($"Parent not found. Executing again.");
                return FindParent<T>(parentObject);
            }
        }
    }

    public static class Logger
    {
        private static RichTextBox logRichTextBox;

        static Logger()
        {
            logRichTextBox = new RichTextBox();
        }

        public static void SetLogTarget(RichTextBox richTextBox)
        {
            logRichTextBox = richTextBox;
        }

        public static void Log(string message)
        {
            if (logRichTextBox != null)
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [INFO]: {message}";
                LogAddLine(logMessage, Brushes.Aquamarine);
            }
        }

        public static void Error(string message)
        {
            if (logRichTextBox != null)
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR]: {message}";
                LogAddLine(logMessage, Brushes.Red);
            }
        }
        
        public static void Debug(string message)
        {
            SetWindow setWindow = SetWindow.Instance;
            if (logRichTextBox != null)
            {
                if (setWindow.Debug_Mode)
                {
                    string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [DEBUG]: {message}";
                    LogAddLine(logMessage, Brushes.White);
                }
            }
        }

        private static void LogAddLine(string message, SolidColorBrush color)
        {
            Paragraph paragraph = new Paragraph(new Run(message));
            paragraph.Foreground = color;

            logRichTextBox.Document.Blocks.Add(paragraph);

            logRichTextBox.ScrollToEnd();
        }
    }

    public static class Notification
    {
        static SetWindow setWindow = SetWindow.Instance;
        private static MediaPlayer player = new MediaPlayer();

        public static bool PlayAudioNotification()
        {
            try
            {
                string audioPath = setWindow.selectedFileName;
                if (System.IO.File.Exists(audioPath))
                {
                    Logger.Debug($"Trying playing {audioPath} now...");
                    player.Open(new Uri(audioPath));
                    player.Volume = 0.1;
                    player.Play();
                    return true;
                }
                else
                {
                    Logger.Error("Error. No such file or directory.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }
    }
}