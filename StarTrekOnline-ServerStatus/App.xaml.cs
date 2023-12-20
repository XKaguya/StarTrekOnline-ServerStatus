using System;
using System.Windows;
using Newtonsoft.Json;
using StarTrekOnline_ServerStatus.Utils.API;

namespace StarTrekOnline_ServerStatus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow CurrentMainWindow { get; private set; }

        protected async override void OnStartup(StartupEventArgs ev)
        {
            base.OnStartup(ev);

            if (ev.Args.Length > 0)
            {
                string command = ev.Args[0];

                if (command == "--sS")
                {
                    IServerStatus serverStatus = new ServerStatus();
                    (API.StatusCode, API.days, API.hours, API.minutes, API.seconds) =
                        await serverStatus.CheckServer(false);

                    INewsProcessor newsProcessor = new NewsProcessor();
                    var newsContents = await newsProcessor.GetNewsContents();

                    var combinedData = new
                    {
                        StatusCode = API.StatusCode,
                        Days = API.days,
                        Hours = API.hours,
                        Minutes = API.minutes,
                        Seconds = API.seconds,
                        NewsContents = newsContents
                    };

                    string combined_json = JsonConvert.SerializeObject(combinedData);
                    Console.WriteLine(combined_json);

                    Environment.Exit(0);
                }
            }
            else
            {
                CurrentMainWindow = new MainWindow();
                CurrentMainWindow.Show();
            }
        }
    }
}