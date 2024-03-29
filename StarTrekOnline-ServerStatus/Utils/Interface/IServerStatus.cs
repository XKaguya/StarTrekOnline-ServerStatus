/*
using System;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HandyControl.Tools.Extension;
using StarTrekOnline_ServerStatus.Utils.API;
using static StarTrekOnline_ServerStatus.Utils.API.Enums;

namespace StarTrekOnline_ServerStatus
{
    public interface IServerStatus
    {
        Task<API.MaintenanceInfo> CheckServerAsync(bool debug);
    }

    public class ServerStatus : IServerStatus
    {
        public async Task<API.MaintenanceInfo> CheckServerAsync(bool debug)
        {
            try
            {
                string message = await GetMaintenanceTimeAsync(debug);
                if (!string.IsNullOrEmpty(message) && message.Contains("maintenance"))
                {
                    var times = ExtractTimes(message);
                    Enums.ShardStatus shardStatus;
                    if (times.Item1 == null && times.Item2 == null && times.Item3 == null)
                    {
                        shardStatus = ExtractServerStatus(message, false);
                    }
                    else
                    {
                        shardStatus = ExtractServerStatus(message, true);
                    }
                    
                    var (startEventTime, endEventTime) = TimeUntilEvent(times);
                    DateTime currentTime = DateTime.UtcNow;

                    if (startEventTime <= currentTime && currentTime <= endEventTime)
                    {
                        TimeSpan remainingTime = endEventTime.Value - currentTime;
                        return new API.MaintenanceInfo
                        {
                            ShardStatus = shardStatus,
                            Days = remainingTime.Days,
                            Hours = remainingTime.Hours,
                            Minutes = remainingTime.Minutes,
                            Seconds = remainingTime.Seconds
                        };
                    }
                    else if (currentTime < startEventTime)
                    {
                        TimeSpan remainingTime = startEventTime.Value - currentTime;
                        return new API.MaintenanceInfo
                        {
                            ShardStatus = shardStatus,
                            Days = remainingTime.Days,
                            Hours = remainingTime.Hours,
                            Minutes = remainingTime.Minutes,
                            Seconds = remainingTime.Seconds
                        };
                    }
                    else
                    {
                        return new API.MaintenanceInfo
                        {
                            ShardStatus = shardStatus,
                            Days = 0,
                            Hours = 0,
                            Minutes = 0,
                            Seconds = 0
                        };
                    }
                }
                else
                {
                    return new API.MaintenanceInfo
                    {
                        ShardStatus = Enums.ShardStatus.Up,
                        Days = 0,
                        Hours = 0,
                        Minutes = 0,
                        Seconds = 0
                    };
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while checking server status. {e.Message + e.StackTrace}");
                throw;
            }
        }
        
        private async Task<string?> GetMaintenanceTimeAsync(bool debug)
        {
            if (debug)
            {
                return "{\"result\":\"success\",\"news\":[{\"newsID\":11555873,\"title\":\"PC Patch Notes for 10\\/5\\/23\",\"postDate\":1696489200,\"url\":\"https:\\/\\/www.arcgames.com\\/en\\/games\\/star-trek-online\\/news\\/detail\\/11555873\",\"date\":\"2023-10-05\"},{\"newsID\":11555803,\"title\":\"Command the Dauntless II!\",\"postDate\":1696431600,\"url\":\"https:\\/\\/www.arcgames.com\\/en\\/games\\/star-trek-online\\/news\\/detail\\/11555803\",\"date\":\"2023-10-04\"}],\"banners\":[{\"newsID\":\"11555803\",\"imageLoc\":\"https:\\/\\/pwimages-a.akamaihd.net\\/arc\\/60\\/05\\/6005b6427b307db75446f74743f4d0db1696377153.png\",\"clientImageLoc\":\"\",\"schedule\":\"true\"},{\"newsID\":\"11554993\",\"imageLoc\":\"https:\\/\\/pwimages-a.akamaihd.net\\/arc\\/f4\\/39\\/f4397cd9f15fed16bafd3e87db0a70731695874491.png\",\"clientImageLoc\":\"\",\"schedule\":\"true\"},{\"newsID\":\"11553903\",\"imageLoc\":\"https:\\/\\/pwimages-a.akamaihd.net\\/arc\\/cb\\/28\\/cb2857f30572a5783fe56f0acfb51e1b1694533798.png\",\"clientImageLoc\":\"\",\"schedule\":\"true\"},{\"newsID\":\"11553753\",\"imageLoc\":\"https:\\/\\/pwimages-a.akamaihd.net\\/arc\\/21\\/c3\\/21c3dcc75dc481a9c953dae4c090541b1694268654.png\",\"clientImageLoc\":\"\",\"schedule\":\"true\"},{\"newsID\":\"11553353\",\"imageLoc\":\"https:\\/\\/pwimages-a.akamaihd.net\\/arc\\/9f\\/c6\\/9fc64701663148ae2956da9f7b21a9a41694543132.png\",\"clientImageLoc\":\"\",\"schedule\":\"true\"}],\"left_ad\":{\"newsID\":\"11552853\",\"imageLoc\":\"https:\\/\\/pwimages-a.akamaihd.net\\/arc\\/a4\\/a9\\/a4a9e3209fdc7e3b1effe2174b9ced251693280101.png\",\"clientImageLoc\":\"\",\"schedule\":\"true\"},\"message\":\"We will be performing shard maintenance on January 5 from 7-9:30 AM Pacific (02:34-09:00 UTC). Please check the forums.\",\"baseurl\":\"https:\\/\\/www.arcgames.com\\/en\\/games\\/star-trek-online\\/news\\/detail\\/\",\"server_status\":\"down\"}";
            }
            else
            {
                {
                    string url = "http://launcher.startrekonline.com/launcher_server_status";
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            string data = await response.Content.ReadAsStringAsync();
                            return data;
                        }
                    }
                    return null;
                }
            }
        }

        private static (string?, string?, string?) ExtractTimes(string message)
        {
            string datePattern = @"(January|February|March|April|May|June|July|August|September|October|November|December)\s+(\d{1,2})";
            Match dateMatch = Regex.Match(message, datePattern);
            if (dateMatch.Success)
            {
                string month = dateMatch.Groups[1].Value;
                string day = dateMatch.Groups[2].Value;

                string timePattern = @"(\d{1,2}:\d{2}-\d{1,2}:\d{2})";
                Match timeMatch = Regex.Match(message, timePattern);
                if (timeMatch.Success)
                {
                    string utcTimeRange = timeMatch.Groups[1].Value;
                    return (month, day, utcTimeRange);
                }
            }
            
            return (null, null, null);
        }

        private (DateTime?, DateTime?) TimeUntilEvent((string?, string?, string?) times)
        {
            if (times.Item3 == null || times.Item2 == null || times.Item1 == null)
            {
                return (null, null);
            }
            
            string[] timeRange = times.Item3.Split('-');

            DateTime startTime = DateTime.ParseExact(timeRange[0], "H:mm", CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.ParseExact(timeRange[1], "H:mm", CultureInfo.InvariantCulture);
            
            return (startTime, endTime);
        }

        private Enums.ShardStatus ExtractServerStatus(string message, bool isMaintenance)
        {
            string pattern = "\"server_status\"\\s*:\\s*\"(\\w+)\"";
            Match match = Regex.Match(message, pattern);
            if (match.Success)
            {
                if (match.Groups[1].Value == "up" && isMaintenance)
                {
                    return Enums.ShardStatus.MaintenanceEnded;
                }
                else if (match.Groups[1].Value == "up")
                {
                    return Enums.ShardStatus.Up;
                }
                else
                {
                    return Enums.ShardStatus.Maintenance;
                }
            }

            return Enums.ShardStatus.None;
        }
    }
}
*/
