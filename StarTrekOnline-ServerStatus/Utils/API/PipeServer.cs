using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StarTrekOnline_ServerStatus.Utils.API
{
        public class NamedPipeServerHandler
    {
        private readonly string pipeName;

        public NamedPipeServerHandler(string pipeName)
        {
            this.pipeName = pipeName;
        }

        public async Task StartServerAsync()
        {
            while (true)
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName))
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

        private async Task ProcessClientMessageAsync(NamedPipeServerStream pipeServer)
        {
            byte[] buffer = new byte[256];
            int bytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length);
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Logger.Log($"Received message from client: {receivedMessage}");

            if (receivedMessage == "sS")
            {
                IServerStatus serverStatus = new ServerStatus();
                (API.StatusCode, API.days, API.hours, API.minutes, API.seconds) = await serverStatus.CheckServer(false);

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
}