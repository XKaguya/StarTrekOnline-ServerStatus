﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarTrekOnline_ServerStatus.Utils.API;

namespace StarTrekOnline_ServerStatus
{
    public interface IServerStatusRemastered
    {
        Task<API.MaintenanceInfo> CheckServerAsync();
    }
    
    public class ServerStatusRemastered : IServerStatusRemastered
    {
        private readonly HttpClient _client = new();

        private bool NullCheck(DateTime? date)
        {
            return date == null;
        }
        
        private bool NullCheck(TimeSpan? time)
        {
            return time == null;
        }

        public async Task<API.MaintenanceInfo> CheckServerAsync()
        {
            try
            {
                string? message = null;
                message = await GetMaintenanceTimeFromLauncherAsync();
                if (SetWindow.Instance.Debug_Mode)
                {
                    message = API.GetDebugMessage();
                }
                
                Enums.ShardStatus serverStatus = Enums.ShardStatus.None;
                
                serverStatus = ExtractServerStatus(message);
                
                if (message == null)
                {
                    Logger.Debug("Statement Null.");
                    
                    API.MaintenanceInfo maintenanceInfoA = new API.MaintenanceInfo
                    {
                        ShardStatus = Enums.MaintenanceTimeType.None,
                    };
                    
                    maintenanceInfoA.Days = 0;
                    maintenanceInfoA.Hours = 0;
                    maintenanceInfoA.Minutes = 0;
                    maintenanceInfoA.Seconds = 0;
                }

                (DateTime? date, (TimeSpan? startTime, TimeSpan? endTime)) = await ExtractMaintenanceTime(message);
                
                Logger.Debug($"CheckServerAsync: {NullCheck(date)}, {NullCheck(startTime)}, {NullCheck(endTime)}");

                var (startEventTime, endEventTime) = TimeUntilMaintenance(date, startTime, endTime);
                DateTime currentTime = DateTime.Now;
                
                Logger.Debug($"TEST!!!! {serverStatus}, {currentTime}, {startEventTime}, {endEventTime}");

                var maintenanceType = GetMaintenanceTimeType(currentTime, startEventTime, endEventTime, serverStatus);

                API.MaintenanceInfo maintenanceInfo = new API.MaintenanceInfo
                {
                    ShardStatus = maintenanceType,
                };

                if (maintenanceType == Enums.MaintenanceTimeType.WaitingForMaintenance)
                {
                    Logger.Debug("Statement 1");
                    
                    maintenanceInfo.Days = startEventTime.Value.Subtract(currentTime).Days;
                    maintenanceInfo.Hours = startEventTime.Value.Subtract(currentTime).Hours;
                    maintenanceInfo.Minutes = startEventTime.Value.Subtract(currentTime).Minutes;
                    maintenanceInfo.Seconds = startEventTime.Value.Subtract(currentTime).Seconds;
                    
                    Logger.Debug($"{maintenanceInfo}");
                }
                if (maintenanceType == Enums.MaintenanceTimeType.Maintenance)
                {
                    Logger.Debug("Statement 2");
                    
                    maintenanceInfo.Days = endEventTime.Value.Subtract(currentTime).Days;
                    maintenanceInfo.Hours = endEventTime.Value.Subtract(currentTime).Hours;
                    maintenanceInfo.Minutes = endEventTime.Value.Subtract(currentTime).Minutes;
                    maintenanceInfo.Seconds = endEventTime.Value.Subtract(currentTime).Seconds;
                    
                    Logger.Debug($"{maintenanceInfo}");
                }
                else if (maintenanceType == Enums.MaintenanceTimeType.MaintenanceEnded)
                {
                    Logger.Debug("Statement 3");
                    
                    maintenanceInfo.Days = 0;
                    maintenanceInfo.Hours = 0;
                    maintenanceInfo.Minutes = 0;
                    maintenanceInfo.Seconds = 0;
                    
                    Logger.Debug($"{maintenanceInfo}");
                }
                else if (maintenanceType == Enums.MaintenanceTimeType.SpecialMaintenance)
                {
                    Logger.Debug("Statement 4");
                    
                    maintenanceInfo.Days = -1;
                    maintenanceInfo.Hours = -1;
                    maintenanceInfo.Minutes = -1;
                    maintenanceInfo.Seconds = -1;
                    
                    Logger.Debug($"{maintenanceInfo}");
                }
                else if (maintenanceType == Enums.MaintenanceTimeType.None)
                {
                    Logger.Debug("Statement 5");
                    
                    maintenanceInfo.Days = 0;
                    maintenanceInfo.Hours = 0;
                    maintenanceInfo.Minutes = 0;
                    maintenanceInfo.Seconds = 0;
                    
                    Logger.Debug($"{maintenanceInfo}");
                }
                
                return maintenanceInfo;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message + ex.StackTrace);
                throw;
            }
        }

        private Enums.MaintenanceTimeType GetMaintenanceTimeType(DateTime? currentTime, DateTime? startTime, DateTime? endTime, Enums.ShardStatus serverStatus)
        {
            if (currentTime < startTime && serverStatus == Enums.ShardStatus.Up)
            {
                return Enums.MaintenanceTimeType.WaitingForMaintenance;
            }
            else if (currentTime >= startTime && currentTime <= endTime && serverStatus == Enums.ShardStatus.Maintenance)
            {
                return Enums.MaintenanceTimeType.Maintenance;
            }
            else if (currentTime > endTime && serverStatus == Enums.ShardStatus.Up)
            {
                return Enums.MaintenanceTimeType.MaintenanceEnded;
            }
            else if (currentTime > endTime && serverStatus == Enums.ShardStatus.Maintenance)
            {
                return Enums.MaintenanceTimeType.SpecialMaintenance;
            }

            return Enums.MaintenanceTimeType.None;
        }
        
        private Enums.ShardStatus ExtractServerStatus(string? message)
        {
            if (message == null)
            {
                return Enums.ShardStatus.None;
            }
            
            string json = message.Trim('\"').Replace("\\\"", "\"");
            
            try
            {
                JObject jObject = JObject.Parse(json);
                string server_status = (string)jObject["server_status"];
                if (server_status == "up")
                {
                    return Enums.ShardStatus.Up;
                }
                else if (server_status == "down")
                {
                    return Enums.ShardStatus.Maintenance;
                }
            }
            catch (JsonReaderException ex)
            {
                Logger.Error($"Failed to parse JSON: {ex.Message}");
            }

            return Enums.ShardStatus.None;
        }
        
        private async Task<string?> GetMaintenanceTimeFromLauncherAsync()
        {
            string url = "http://launcher.startrekonline.com/launcher_server_status";
            HttpResponseMessage response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                
                return data;
            }

            return null;
        }
        
        private static (DateTime?, DateTime?) TimeUntilMaintenance(DateTime? date, TimeSpan? startTime, TimeSpan? endTime)
        {
            if (date == null || startTime == null || endTime == null)
            {
                return (null, null);
            }

            DateTime startDateTime = date.Value.Date.Add(startTime.Value);
            DateTime endDateTime = date.Value.Date.Add(endTime.Value);
            
            if (endTime < startTime)
            {
                endDateTime = endDateTime.AddDays(1);
            }

            return (startDateTime, endDateTime);
        }

        private async Task<(DateTime?, (TimeSpan?, TimeSpan?))> ExtractMaintenanceTime(string message)
        {
            DateTime? date = null;
            TimeSpan? startTime, endTime = null;
            TimeSpan? utcStartTime, utcEndTime = null;
            TimeSpan? finalStartTime, finalEndTime = null;

            date = TryParseDate(message, @"(?i)(January|February|March|April|May|June|July|August|September|October|November|December) \d+");
            (startTime, endTime) = TryParseTimeSpan(message, @"(\d+-\d+:\d+)");
            (utcStartTime, utcEndTime) = TryParseTimeSpan(message, @"(\d+:\d+-\d+:\d+ UTC)");
            
            // parsedDate, startTime, endTime, utcStartTime, utcEndTime, finalStartTime, finalEndTime;

            if (date != null && startTime != null && endTime != null && utcStartTime != null && utcEndTime != null)
            {
                Logger.Debug($"ExtractMaintenanceTime: {date}, {startTime}, {endTime}, {utcStartTime}, {utcEndTime}");

                if (CompareTimeRanges(startTime, endTime, utcStartTime, utcEndTime))
                {
                    finalStartTime = utcStartTime;
                    finalEndTime = utcEndTime;
                }
                else
                {
                    Logger.Debug($"The time range isnt same. It should be Kael's problem.");

                    finalStartTime = utcStartTime;
                    finalEndTime = utcEndTime;
                }

                (finalStartTime, finalEndTime) = ConvertToLocalTime(finalStartTime, finalEndTime);
                Logger.Debug($"Converting to local time: {finalStartTime}, {finalEndTime}");

                return (date, (finalStartTime, finalEndTime));

            }

            return ((null), (null, null));
        }
        
        private static bool CompareTimeRanges(TimeSpan? timeSpan0, TimeSpan? timeSpan1, TimeSpan? timeSpan2, TimeSpan? timeSpan3)
        {
            try
            {
                DateTime utcTimeSpan0 = DateTime.UtcNow.Date.Add(timeSpan0 ?? TimeSpan.Zero);
                DateTime utcTimeSpan1 = DateTime.UtcNow.Date.Add(timeSpan1 ?? TimeSpan.Zero);
                
                if (utcTimeSpan0 < utcTimeSpan1)
                {
                    return true;
                }
                else if (utcTimeSpan0 > utcTimeSpan1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message + ex.StackTrace);
                throw;
            }
        }
        
        private static (TimeSpan?, TimeSpan?) ConvertToLocalTime(TimeSpan? timeSpan0, TimeSpan? timeSpan1)
        {
            try
            {
                TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

                if (timeSpan0 != null && timeSpan1 != null)
                {
                    DateTime utcDateTime0 = DateTime.UtcNow.Date.Add(timeSpan0.Value);
                    DateTime utcDateTime1 = DateTime.UtcNow.Date.Add(timeSpan1.Value);
                    utcDateTime0 = DateTime.SpecifyKind(utcDateTime0, DateTimeKind.Utc);
                    utcDateTime1 = DateTime.SpecifyKind(utcDateTime1, DateTimeKind.Utc);
                    
                    DateTime localDateTime0 = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime0, localTimeZone);
                    DateTime localDateTime1 = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime1, localTimeZone);
                    
                    TimeSpan? localTime0 = localDateTime0.TimeOfDay;
                    TimeSpan? localTime1 = localDateTime1.TimeOfDay;

                    return (localTime0, localTime1);
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message + ex.StackTrace);
                throw;
            }
        }
        
        private DateTime? TryParseDate(string? input, string pattern)
        {
            if (input == null)
            {
                return null;
            }
            
            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                DateTime temp;
                if (DateTime.TryParse(match.Value, out temp))
                {
                    return temp;
                }
            }

            return null;
        }
        
        /// <summary>
        ///     Extracts the maintenance time from the message.
        ///     Returns a tuple of the date and the time range.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns>TimeSpan or null if failed.</returns>
        private (TimeSpan?, TimeSpan?) TryParseTimeSpan(string? input, string pattern)
        {
            if (input == null)
            {
                return (null, null);
            }
            
            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                var temp0 = "";
                var temp1 = "";
                TimeSpan parsedTime0;
                TimeSpan parsedTime1;
                
                temp0 = match.Value.Split('-')[0].Trim();
                temp1 = match.Value.Split('-')[1].Trim();

                if (temp1.Contains("UTC"))
                {
                    temp1 = temp1.Replace(" UTC", "");
                }

                if (temp0.Length == 1)
                {
                    var format = "%h";
                    if (TimeSpan.TryParseExact(temp0, format, null, out parsedTime0))
                    {
                        // Logger.Debug($"Success parsing time span: {parsedTime0}");
                    }
                    else
                    {
                        Logger.Debug($"Failed to parse time span: {temp0}");
                    }
                }
                else
                {
                    var format = "h\\:mm";
                    if (TimeSpan.TryParseExact(temp0, format, null, out parsedTime0))
                    {
                        // Logger.Debug($"Success parsing time span: {parsedTime0}");
                    }
                    else
                    {
                        Logger.Debug($"Failed to parse time span: {temp0}");
                    }
                }

                if (TimeSpan.TryParseExact(temp1, "h\\:mm", null, out parsedTime1))
                {
                    // Logger.Debug($"Success parsing time span: {parsedTime1}");
                }
                else
                {
                    Logger.Debug($"Failed to parse time span: {temp1}");
                }
                

                return (parsedTime0, parsedTime1);
            }
            else
            {
                return (null, null);
            }
        }
    }
}