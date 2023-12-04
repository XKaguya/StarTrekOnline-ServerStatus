using StarTrekOnline_ServerStatus.Utils.API;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StarTrekOnline_ServerStatus
{
    public interface INotification
    {
        Task<bool> AudioNotification(string path);
    }

    public class Notification : INotification
    {
        private static MediaPlayer player = new MediaPlayer();
        public async Task<bool> AudioNotification(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    Logger.Debug($"Trying playing {path} now...");
                    player.Open(new Uri(path));
                    player.Volume = 0.1;
                    player.Play();
                    return true;
                }
                else
                {
                    Logger.Error("Error. No such file or directory.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }
    }
}