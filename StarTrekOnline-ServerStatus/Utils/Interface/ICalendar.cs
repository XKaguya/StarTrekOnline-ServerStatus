using StarTrekOnline_ServerStatus.Utils.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace StarTrekOnline_ServerStatus
{
    public interface ICalendar
    {
        Task<string> GetUpcomingEvents();
    }

    public class Calendar : ICalendar
    {
        public async Task<string> GetUpcomingEvents()
        {
            try
            {
                string[] scopes = { CalendarService.Scope.CalendarReadonly };
                string credPath = "credentials.json";

                UserCredential credential;

                using (var stream = new FileStream("client_secret.json", FileMode.Open,
                           FileAccess.Read))
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                }

                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "STO Server Checker"
                });

                EventsResource.ListRequest request =
                    service.Events.List("uhio1bvtudq50n2qhfeo98iduo@group.calendar.google.com");
                request.TimeMin = DateTime.UtcNow;
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.MaxResults = 10;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                Events events = request.Execute();

                if (events.Items == null || events.Items.Count == 0)
                {
                    Logger.Log("No upcoming events found.");
                    return null;
                }
                else
                {
                    string logs = "";
                    foreach (var eventItem in events.Items)
                    {
                        string start = "";
                        string end = "";
                        string timeTillEnd = "";
                        string timeTillStart = "";

                        if (eventItem.Start.DateTime != null)
                        {
                            DateTime startTime = eventItem.Start.DateTime.Value;
                            DateTime endTime1 = eventItem.End.DateTime.Value;
                            start = startTime.ToString("yyyy-MM-dd");
                            end = endTime1.ToString("yyyy-MM-dd");

                            if (DateTime.UtcNow < startTime)
                            {
                                TimeSpan timeFromStart = startTime - DateTime.UtcNow;
                                timeTillStart = $"{(int)timeFromStart.TotalDays} days.";
                            }
                            else
                            {
                                if (eventItem.End.DateTime != null)
                                {
                                    DateTime endTime = eventItem.End.DateTime.Value;
                                    end = endTime.ToString("yyyy-MM-dd");

                                    if (DateTime.UtcNow < endTime)
                                    {
                                        TimeSpan timeUntilEnd = endTime - DateTime.UtcNow;
                                        timeTillEnd = $"{(int)timeUntilEnd.TotalDays} days.";
                                    }
                                    else
                                    {
                                        timeTillEnd = "Event Ended";
                                    }
                                }
                            }
                        }
                        else
                        {
                            start = eventItem.Start.Date;
                            end = "All-Day Event";
                        }

                        logs += $"{start} - {end} - {eventItem.Summary}";
                        if (!string.IsNullOrEmpty(timeTillStart))
                        {
                            logs += $"- Event Start: {timeTillStart}";
                        }

                        if (!string.IsNullOrEmpty(timeTillEnd))
                        {
                            logs += $" - Event End: {timeTillEnd}\n";
                        }
                        else
                        {
                            logs += "\n";
                        }
                    }

                    return logs;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}, {ex.StackTrace}");
                GetUpcomingEvents();
                throw;
            }
        }
    }
}
