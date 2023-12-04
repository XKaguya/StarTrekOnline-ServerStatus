using System;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StarTrekOnline_ServerStatus.Utils.API
{
    public static class API
    {
        private static MainWindow mainWindow = App.CurrentMainWindow;

        public static int StatusCode { get; set; } = 0;

        public static int days { get; set; }

        public static int hours { get; set; }

        public static int minutes { get; set; }

        public static int seconds { get; set; }

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

        public static async Task<bool> PlayAudioNotification(string path)
        {
            INotification notification = new Notification();
            if (path != null)
            {
                bool flag = await notification.AudioNotification(path);
                return flag;
            }
            return false;
        }

        public static bool ChangeTextBlockContent(TextBlock textBlock, string content)
        {
            try
            {
                textBlock.Text = content;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}, {ex.StackTrace}");
            }
            return false;
        }

        public static bool ChangeWindowTitle(Window window, string content)
        {
            try
            {
                window.Title = content;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}, {ex.StackTrace}");
            }
            return false;
        }
        
        public static bool ChangeButtonContent(Button button, string content)
        {
            try
            {
                button.Content = content;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}, {ex.StackTrace}");
            }
            return false;
        }

        public static void UpdateServerStatus()
        {
            if (StatusCode == 0)
            {
                ChangeTextBlockContent(mainWindow.ServerStatus, LanguageManager.GetLocalizedString("ServerStatus_Title") + LanguageManager.GetLocalizedString("ServerStatus_Offline"));
            }
            else
            {
                ChangeTextBlockContent(mainWindow.ServerStatus, LanguageManager.GetLocalizedString("ServerStatus_Title") + LanguageManager.GetLocalizedString("ServerStatus_Online"));
            }
        }

        public static void UpdateMaintenanceInfo(int type)
        {
            if (type == 1)
            {
                ChangeTextBlockContent(mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("Message_Content") + days + "days" + hours + "hours" + minutes + "minutes" + seconds + "seconds.");
            }
            else if (type == 2)
            {
                ChangeTextBlockContent(mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("No_Message"));
            }
            else
            {
                ChangeTextBlockContent(mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("Maintenance_Ended"));
            }
        }
    }
}
