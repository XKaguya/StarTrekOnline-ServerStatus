using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StarTrekOnline_ServerStatus.Utils.API;

namespace StarTrekOnline_ServerStatus
{
    public class ServerStatusViewModel : INotifyPropertyChanged
    {
        private string _serverStatus = "Offline";

        public string ServerStatus
        {
            get { return _serverStatus; }
            set
            {
                if ((value != "Online" && value != "Offline") || (value != "在线" && value != "离线"))
                {
                    Logger.Error("ServerStatus must be either 'Online' or 'Offline'.\"");
                    throw new ArgumentException("ServerStatus must be either 'Online' or 'Offline'.");
                }

                if (_serverStatus != value)
                {
                    _serverStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
