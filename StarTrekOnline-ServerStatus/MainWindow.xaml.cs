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
            
            Init();
        }

        public void Init()
        {
            Logger.SetLogLevel("Debug");
            CheckServer();
            UpdateNews();
            GetRecentNews();
            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            setWindow.isInitialized = true;
        }

        public async void GetRecentNews()
        {
            ICalendar calendar = new Calendar();
            var message = await calendar.GetUpcomingEvents();

            API.ChangeTextBlockContent(Recent_Events_Info, message);
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
            Logger.Log("Reloading...");
            CheckServer();
            UpdateNews();
            Logger.Log("Reload complete.");
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

        private void PlayAudioNot_Start()
        {
            if (!isPlayed_Start)
            {
                isPlayed_Start = true;
                Logger.Debug($"isPlayed_Start: {isPlayed_Start}");
                if (API.PlayAudioNotification(setWindow.selectedFileName).Result)
                {
                    Logger.Log("Start playing audio.");
                }
                else
                {
                    isPlayed_Start = false;
                    Logger.Error("Audio play failed.");
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
                    if (API.PlayAudioNotification(setWindow.selectedFileName).Result)
                    {
                        Logger.Log("Start playing audio.");
                    }
                    else
                    {
                        isPlayed_End = false;
                        playedTime = 0;
                    }
                    Logger.Error("Audio play failed.");
                }
            }
        }

        private async Task CheckServer()
        {
            IServerStatus serverStatus = new ServerStatus();
            
            (API.StatusCode, API.days, API.hours, API.minutes, API.seconds) = await serverStatus.CheckServer(setWindow.Debug_Mode);

            switch (API.StatusCode)
            {
                case 0:
                    API.UpdateServerStatus();
                    API.UpdateMaintenanceInfo(1);
                    PlayAudioNot_Start();
                    isPlayed_End = false;
                    playedTime = 0;
                    break;
                case 1:
                    API.UpdateServerStatus();
                    API.UpdateMaintenanceInfo(1);
                    break;
                case 2:
                    API.UpdateServerStatus();
                    API.UpdateMaintenanceInfo(3);
                    PlayAudioNot_End();
                    isPlayed_Start = false;
                    playedTime++;
                    break;
                case 3:
                    API.UpdateServerStatus();
                    API.UpdateMaintenanceInfo(2);
                    break;
                default:
                    break;
            }
        }
    }
}