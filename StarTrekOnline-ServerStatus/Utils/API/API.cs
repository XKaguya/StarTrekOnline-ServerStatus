using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StarTrekOnline_ServerStatus.Utils.API
{
    public class API
    {
        private static MainWindow _mainWindow = App.CurrentMainWindow;

        public class MaintenanceInfo
        {
            public Enums.ShardStatus ShardStatus { get; set; }

            public int Days { get; set; }

            public int Hours { get; set; }

            public int Minutes { get; set; }

            public int Seconds { get; set; }
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
        
        public static void UpdateServerStatus(Enums.ShardStatus shardStatus)
        {
            if (shardStatus == Enums.ShardStatus.Maintenance)
            {
                ChangeTextBlockContent(_mainWindow.ServerStatus, LanguageManager.GetLocalizedString("ServerStatus_Title") + LanguageManager.GetLocalizedString("ServerStatus_Offline"));
            }
            else
            {
                ChangeTextBlockContent(_mainWindow.ServerStatus, LanguageManager.GetLocalizedString("ServerStatus_Title") + LanguageManager.GetLocalizedString("ServerStatus_Online"));
            }
        }
        
        public static void UpdateMaintenanceInfo(MaintenanceInfo maintenanceInfo)
        {
            switch (maintenanceInfo.ShardStatus)
            {
                case Enums.ShardStatus.Maintenance:
                    ChangeTextBlockContent(_mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("Message_Content_Ongoing") + '\n' + maintenanceInfo.Days + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Days") + " " + maintenanceInfo.Hours + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Hours")  + " " + maintenanceInfo.Minutes + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Minutes") + " " + maintenanceInfo.Seconds + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Seconds"));
                    break;
                    
                case Enums.ShardStatus.WaitingForMaintenance:
                    ChangeTextBlockContent(_mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("Message_Content") + '\n' + maintenanceInfo.Days + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Days") + " " + maintenanceInfo.Hours + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Hours")  + " " + maintenanceInfo.Minutes + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Minutes") + " " + maintenanceInfo.Seconds + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Seconds"));
                    break;
                
                case Enums.ShardStatus.MaintenanceEnded:
                    ChangeTextBlockContent(_mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("Maintenance_Ended"));
                    break;
                
                case Enums.ShardStatus.Up:
                    ChangeTextBlockContent(_mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("No_Message"));
                    break;
            }
        }
        
        public static async Task<string> GetFormattedUpcomingEvents()
        {
            ICalendar calendar = new Calendar();
            var eventInfos = await calendar.GetUpcomingEvents();

            StringBuilder formattedMessage = new StringBuilder();

            if (eventInfos != null && eventInfos.Any())
            {
                foreach (var eventInfo in eventInfos)
                {
                    formattedMessage.AppendLine($"Event: {eventInfo.Summary} \nStart Date: {eventInfo.StartDate}  End Date: {eventInfo.EndDate}");

                    if (!string.IsNullOrEmpty(eventInfo.TimeTillStart))
                    {
                        formattedMessage.AppendLine($"Event Start: {eventInfo.TimeTillStart}");
                    }

                    if (!string.IsNullOrEmpty(eventInfo.TimeTillEnd))
                    {
                        formattedMessage.AppendLine($"Event End: {eventInfo.TimeTillEnd}");
                    }

                    formattedMessage.AppendLine();
                }
            }
            else
            {
                formattedMessage.AppendLine("No upcoming events found.");
            }

            return formattedMessage.ToString();
        }
    }
}