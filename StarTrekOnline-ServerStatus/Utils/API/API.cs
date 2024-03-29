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
            public Enums.MaintenanceTimeType ShardStatus { get; set; }

            public int Days { get; set; }

            public int Hours { get; set; }

            public int Minutes { get; set; }

            public int Seconds { get; set; }

            public override string ToString()
            {
                string shardStatusString = ShardStatus.ToString();

                string timeString = $"{Days} days, {Hours} hours, {Minutes} minutes, {Seconds} seconds";

                return $"Shard Status: {shardStatusString}, Maintenance Time: {timeString}";
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
        
        public static void UpdateServerStatus(Enums.MaintenanceTimeType shardStatus)
        {
            try
            {
                if (shardStatus == Enums.MaintenanceTimeType.Maintenance)
                {
                    ChangeTextBlockContent(_mainWindow.ServerStatus, LanguageManager.GetLocalizedString("ServerStatus_Title") + LanguageManager.GetLocalizedString("ServerStatus_Offline"));
                }
                else if (shardStatus == Enums.MaintenanceTimeType.SpecialMaintenance)
                {
                    ChangeTextBlockContent(_mainWindow.ServerStatus, LanguageManager.GetLocalizedString("ServerStatus_Title") + LanguageManager.GetLocalizedString("ServerStatus_Offline"));
                }
                else
                {
                    ChangeTextBlockContent(_mainWindow.ServerStatus, LanguageManager.GetLocalizedString("ServerStatus_Title") + LanguageManager.GetLocalizedString("ServerStatus_Online"));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message + ex.StackTrace);
                throw;
            }
        }
        
        public static void UpdateMaintenanceInfo(MaintenanceInfo maintenanceInfo)
        {
            switch (maintenanceInfo.ShardStatus)
            {
                case Enums.MaintenanceTimeType.Maintenance:
                    ChangeTextBlockContent(_mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("Message_Content_Ongoing") + '\n' + maintenanceInfo.Days + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Days") + " " + maintenanceInfo.Hours + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Hours")  + " " + maintenanceInfo.Minutes + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Minutes") + " " + maintenanceInfo.Seconds + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Seconds"));
                    break;
                    
                case Enums.MaintenanceTimeType.WaitingForMaintenance:
                    ChangeTextBlockContent(_mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("Message_Content") + '\n' + maintenanceInfo.Days + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Days") + " " + maintenanceInfo.Hours + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Hours")  + " " + maintenanceInfo.Minutes + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Minutes") + " " + maintenanceInfo.Seconds + " " + LanguageManager.GetLocalizedString("maintenanceInfo.Seconds"));
                    break;
                
                case Enums.MaintenanceTimeType.MaintenanceEnded:
                    ChangeTextBlockContent(_mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("Maintenance_Ended"));
                    break;
                
                case Enums.MaintenanceTimeType.SpecialMaintenance:
                    ChangeTextBlockContent(_mainWindow.MaintenanceInfo, LanguageManager.GetLocalizedString("Maintenance_Ended"));
                    break;
                
                case Enums.MaintenanceTimeType.None:
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