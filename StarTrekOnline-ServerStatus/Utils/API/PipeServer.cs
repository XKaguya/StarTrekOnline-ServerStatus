using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StarTrekOnline_ServerStatus.Utils.API
{
    public class NamedPipeServerHandler
    {

        private readonly string _pipeName;
        public NamedPipeServerHandler(string pipeName)
        {
            _pipeName = pipeName;
        }

        public async Task StartServerAsync()
        {
            while (true)
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(_pipeName))
                {
                    Logger.Log("Waiting for connection...");

                    await pipeServer.WaitForConnectionAsync();

                    Logger.Log("Pipe connected.");

                    try
                    {
                        await ProcessClientMessageAsync(pipeServer);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error occurred: {ex.Message}");
                    }

                    pipeServer.Disconnect();
                    Logger.Log("Disconnected.");
                }
            }
        }

        private static async Task ProcessClientMessageAsync(PipeStream pipeServer)
        {
            byte[] buffer = new byte[256];
            int bytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length);
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Logger.Log($"Received message from client: {receivedMessage}");

            if (receivedMessage == "cL")
            {
                await ClientCheckServerAlive(pipeServer);
            }
            else if (receivedMessage == "sS")
            {
                await ClientAskForData(pipeServer);
            }
        }

        private static async Task ClientCheckServerAlive(PipeStream pipeServer)
        {
            if (pipeServer.IsConnected && pipeServer.CanWrite)
            {
                Logger.Log("Client awaiting for response. Sending now...");

                byte[] messageBytes = Encoding.UTF8.GetBytes("Success");
                await pipeServer.WriteAsync(messageBytes);
            }
            else
            {
                Logger.Error("Client cant write.");
            }
        }
        private static async Task ClientAskForData(PipeStream pipeServer)
        {
            IServerStatus serverStatus = new ServerStatus();
            API.MaintenanceInfo maintenanceInfo = new();

            maintenanceInfo = await serverStatus.CheckServerAsync(SetWindow.Instance.Debug_Mode);

            INewsProcessor newsProcessor = new NewsProcessor();
            var newsContents = await newsProcessor.GetNewsContents();

            ICalendar calendar = new Calendar();
            var recentNews = await calendar.GetUpcomingEvents();

            var combinedData = new
            {
                maintenanceInfo.ShardStatus,
                maintenanceInfo.Days,
                maintenanceInfo.Hours,
                maintenanceInfo.Minutes,
                maintenanceInfo.Seconds,
                NewsContents = newsContents,
                RecentNews = recentNews
            };

            string combinedJson = JsonConvert.SerializeObject(combinedData);
            byte[] messageBytes = Encoding.UTF8.GetBytes(combinedJson);

            if (pipeServer.IsConnected && pipeServer.CanWrite)
            {
                Logger.Log("Client is connected and writeable. Trying to write data though pipe.");
                await pipeServer.WriteAsync(messageBytes);
            }
            else
            {
                Logger.Error("Client cant write.");
            }
        }
    }
}