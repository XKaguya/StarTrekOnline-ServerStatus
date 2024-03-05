﻿using System;
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
                    
                    API.MaintenanceInfo maintenanceInfo = new();
                
                    maintenanceInfo = await serverStatus.CheckServerAsync(SetWindow.Instance.Debug_Mode);

                    INewsProcessor newsProcessor = new NewsProcessor();
                    var newsContents = await newsProcessor.GetNewsContents();

                    var combinedData = new
                    {
                        maintenanceInfo.ShardStatus,
                        maintenanceInfo.Days,
                        maintenanceInfo.Hours,
                        maintenanceInfo.Minutes,
                        maintenanceInfo.Seconds,
                        NewsContents = newsContents,
                    };

                    string combinedjson = JsonConvert.SerializeObject(combinedData);
                    Console.WriteLine(combinedjson);

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