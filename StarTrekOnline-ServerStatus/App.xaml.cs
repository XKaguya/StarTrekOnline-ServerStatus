using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StarTrekOnline_ServerStatus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow CurrentMainWindow { get; private set; }

        protected override void OnStartup(StartupEventArgs ev)
        {
            base.OnStartup(ev);
        
            // 创建一个新的 MainWindow 实例并赋值给静态属性
            CurrentMainWindow = new MainWindow();
            CurrentMainWindow.Show();
        }
    }
}