using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using StarTrekOnline_ServerStatus.Utils.API;

namespace StarTrekOnline_ServerStatus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static LogWindow logWindow = LogWindow.Instance;
        static SetWindow setWindow = SetWindow.Instance;
        private DispatcherTimer timer;
        private bool isPlayed_Start { get; set; }
        private bool isPlayed_End { get; set; }
        
        private int playedTime { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            logWindow.Hide();
            setWindow.Hide();
            
            CheckServer();
            UpdateNews();
            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            setWindow.isInitialized = true;
        }
        
        private async void OnNewsImageClick(object sender, MouseButtonEventArgs ev)
        {
            if (sender is Image image)
            {
                StackPanel stackPanel = API.FindParent<StackPanel>(image);
                if (stackPanel != null)
                {
                    string newsUrl = await GetNewsUrlFromStackPanel(stackPanel);
                    Logger.Log($"Trying open {newsUrl}");
                    OpenLinkInBrowser(newsUrl);
                }
                else
                {
                    Logger.Error($"{stackPanel} is null !");
                }
            }
        }

        private async void OnNewsTitleClick(object sender, MouseButtonEventArgs ev)
        {
            if (sender is TextBlock textBlock)
            {
                StackPanel stackPanel = API.FindParent<StackPanel>(textBlock);
                if (stackPanel != null)
                {
                    string newsUrl = await GetNewsUrlFromStackPanel(stackPanel);
                    Logger.Log($"Trying open {newsUrl}");
                    OpenLinkInBrowser(newsUrl);
                }
                else
                {
                    Logger.Error($"{stackPanel} is null !");
                }
            }
        }
        
        private void OnLogClick(object sender, MouseButtonEventArgs ev)
        {
            if (logWindow.IsVisible)
            {
                logWindow.Hide();
            }
            else
            {
                logWindow.Show();
            }
        }
        
        protected override void OnClosing(CancelEventArgs ev)
        {
            base.OnClosing(ev);
            Application.Current.Shutdown();
        }
        
        private void OnSetClick(object sender, MouseButtonEventArgs ev)
        {
            if (setWindow.IsVisible)
            {
                setWindow.Hide();
            }
            else
            {
                setWindow.Show();
            }
        }
        
        private void OnReloadClick(object sender, MouseButtonEventArgs ev)
        {
            CheckServer();
            UpdateNews();
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            CheckServer();
        }
        
        private async Task<string> GetNewsUrlFromStackPanel(StackPanel stackPanel)
        {
            INewsProcessor newsProcessor = new NewsProcessor();
            var newsContents = await newsProcessor.GetNewsContents();
            
            int index = Grid.GetColumn(stackPanel) + (Grid.GetRow(stackPanel) * 3);

            if (index >= 0 && index < newsContents.Count)
            {
                Logger.Log($"Method GetNewsUrlFromStackPanel returing: {newsContents[index].NewsLink}");
                return newsContents[index].NewsLink;
            }

            Logger.Error("News is null ! Please check network connection.");
            return null;
        }


        private void OpenLinkInBrowser(string link)
        {
            if (!string.IsNullOrEmpty(link))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(link) { UseShellExecute = true });
            }
        }

        private async void UpdateNews()
        {
            INewsProcessor newsProcessor = new NewsProcessor();
            var newsContents = await newsProcessor.GetNewsContents();

            int index = 0;
            try
            {
                foreach (var child in NewsGrid.Children)
                {
                    if (child is StackPanel stackPanel)
                    {
                        if (index >= newsContents.Count)
                            return;

                        Image? image = stackPanel.Children.OfType<Image>().FirstOrDefault();
                        TextBlock? textBlock = stackPanel.Children.OfType<TextBlock>().FirstOrDefault();

                        if (image != null && textBlock != null)
                        {
                            image.Source = new BitmapImage(new Uri(newsContents[index].ImageUrl, UriKind.Absolute));
                            textBlock.Text = System.Net.WebUtility.HtmlDecode(newsContents[index].Title);

                            Logger.Log($"News {System.Net.WebUtility.HtmlDecode(newsContents[index].Title)} has been successfully loaded.");
                            index++;

                        }
                    }
                }
            }
            catch (Exception e)
            {
                var error = $"Failed on update news. {e}";
                Logger.Error(error);
                throw;
            }
            finally
            {
                Logger.Log("All news has been successfully loaded.");
            }
        }
        
        private void UpdateMaintenanceInfo(string info)
        {
            MaintenanceInfo.Text = info;
        }
        
        private void UpdateServerStatus(string status)
        {
            if (LanguageManager.CurrentLanguage() == "zh-CN")
            {
                ServerStatus.Text = $"服务器状态: {status}";
            }
            else if (LanguageManager.CurrentLanguage() == "en-US")
            {
                ServerStatus.Text = $"Server Status: {status}";
            }
        }

        private void PlayAudioNot_Start()
        {
            if (!isPlayed_Start)
            {
                isPlayed_Start = true;
                Logger.Debug($"isPlayed_Start: {isPlayed_Start}");
                if (Notification.PlayAudioNotification())
                {
                    Logger.Log("Start playing audio.");
                }
                else
                {
                    isPlayed_Start = false;
                }
            }
        }
        
        private void PlayAudioNot_End()
        {
            if (!isPlayed_End)
            {
                if (playedTime == 0)
                {
                    isPlayed_End = true;
                    Logger.Debug($"isPlayed_Start: {isPlayed_Start}");
                    if (Notification.PlayAudioNotification())
                    {
                        Logger.Log("Start playing audio.");
                    }
                    else
                    {
                        isPlayed_End = false;
                        playedTime = 0;
                    }
                    Logger.Log("Start playing audio.");
                }
            }
        }

        private async Task CheckServer()
        {
            IServerStatus serverStatus = new ServerStatus();
    
            (int statusCode, int days, int hours, int minutes, int seconds) = await serverStatus.CheckServer(setWindow.Debug_Mode);

            if (LanguageManager.CurrentLanguage() == "en-US")
            {
                switch (statusCode)
                {
                    case 0:
                        UpdateServerStatus("Offline");
                        UpdateMaintenanceInfo($"The event is currently ongoing and will end in {days} days, {hours} hours, {minutes} minutes, and {seconds} seconds.");
                        PlayAudioNot_Start();
                        isPlayed_End = false;
                        playedTime = 0;
                        break;
                    case 1:
                        UpdateServerStatus("Online");
                        UpdateMaintenanceInfo($"The event will start in {days} days, {hours} hours, {minutes} minutes, and {seconds} seconds.");
                        break;
                    case 2:
                        UpdateServerStatus("Online");
                        UpdateMaintenanceInfo("The event has ended.");
                        PlayAudioNot_End();
                        isPlayed_Start = false;
                        playedTime++;
                        break;
                    case 3:
                        UpdateServerStatus("Online");
                        UpdateMaintenanceInfo("There's no events currently.");
                        break;
                    default:
                        break;
                }
            }
            else if (LanguageManager.CurrentLanguage() == "zh-CN")
            {
                switch (statusCode)
                {
                    case 0:
                        UpdateServerStatus("离线");
                        UpdateMaintenanceInfo($"服务器正在维护，预计 {days} 天 {hours} 小时 {minutes} 分钟 {seconds} 秒后结束维护。");
                        PlayAudioNot_Start();
                        isPlayed_End = false;
                        playedTime = 0;
                        break;
                    case 1:
                        UpdateServerStatus("在线");
                        UpdateMaintenanceInfo($"服务器将在 {days} 天 {hours} 小时 {minutes} 分钟 {seconds} 秒后开始维护。");
                        break;
                    case 2:
                        UpdateServerStatus("在线");
                        UpdateMaintenanceInfo("服务器维护已结束。");
                        PlayAudioNot_End();
                        isPlayed_Start = false;
                        playedTime++;
                        break;
                    case 3:
                        UpdateServerStatus("在线");
                        UpdateMaintenanceInfo("目前没有任何事件。");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}