using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1.Server
{
    public class SocketBackupServer
    {
        private readonly int _port;
        private Socket? _serverSocket;
        private bool _isRunning;

        public event Action<string>? MessageReceived;

        public SocketBackupServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _port);
            _serverSocket.Bind(endPoint);
            _serverSocket.Listen(10);
            _isRunning = true;

            Task.Run(() => AcceptClientsAsync());
        }

        public void Stop()
        {
            _isRunning = false;
            _serverSocket?.Close();
        }

        private async Task AcceptClientsAsync()
        {
            while (_isRunning && _serverSocket != null)
            {
                try
                {
                    var clientSocket = await _serverSocket.AcceptAsync();
                    _ = HandleClientAsync(clientSocket);
                }
                catch
                {
                    // Arrêt du serveur ou erreur de socket
                    break;
                }
            }
        }

        private async Task HandleClientAsync(Socket clientSocket)
        {
            using (clientSocket)
            {
                // Envoi d'un message de bienvenue ou d'état au client dès la connexion
                string welcomeMessage = "Connexion établie avec le serveur de sauvegarde.";
                var welcomeBytes = Encoding.UTF8.GetBytes(welcomeMessage);
                try
                {
                    await clientSocket.SendAsync(welcomeBytes, SocketFlags.None);
                }
                catch
                {
                    // Si l'envoi échoue, on arrête la gestion de ce client
                    return;
                }

                var buffer = new byte[4096];
                while (_isRunning && clientSocket.Connected)
                {
                    int bytesRead = 0;
                    try
                    {
                        bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
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
        }
    }
}
