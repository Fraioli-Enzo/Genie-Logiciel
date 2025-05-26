using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WpfBackupClient.Client
{
    public class SocketBackupClient
    {
        private readonly string _serverIp;
        private readonly int _serverPort;
        private TcpClient? _client;
        private NetworkStream? _stream;

        public event Action<string>? MessageReceived;

        public SocketBackupClient(string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }

        public async Task ConnectAsync()
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_serverIp, _serverPort);
            _stream = _client.GetStream();
            _ = ReceiveMessagesAsync();
        }

        public async Task SendMessageAsync(string message)
        {
            if (_stream == null) return;
            var data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[4096];
            while (_client != null && _client.Connected)
            {
                int bytesRead = 0;
                try
                {
                    bytesRead = await _stream!.ReadAsync(buffer, 0, buffer.Length);
                }
                catch
                {
                    break;
                }
                if (bytesRead == 0)
                    break;

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                MessageReceived?.Invoke(message);
            }
        }



        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
        }
    }
}
