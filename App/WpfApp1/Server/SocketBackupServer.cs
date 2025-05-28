using System;
using System.IO.Packaging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using WpfApp1.Model;


namespace WpfApp1.Server
{
    public class SocketBackupServer
    {
        private readonly List<Socket> _clients = new();
        private readonly BackupWorkManager backupWorkManager; // Now readonly and injected
        private readonly int _port;
        private Socket? _serverSocket;
        private bool _isRunning;

        public event Action<string>? MessageReceived;

        // Accept BackupWorkManager as a parameter
        public SocketBackupServer(int port, BackupWorkManager backupWorkManager)
        {
            _port = port;
            this.backupWorkManager = backupWorkManager;
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
                    lock (_clients) { _clients.Add(clientSocket); }
                    _ = HandleClientAsync(clientSocket);
                }
                catch
                {
                    break;
                }
            }
        }

        public void NotifyClients()
        {
            // Do not recreate backupWorkManager
            // backupWorkManager = new BackupWorkManager();

            var travaux = backupWorkManager.Works;
            var updateMsg = new { Type = "Update", Works = travaux };
            string json = JsonSerializer.Serialize(updateMsg);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            lock (_clients)
            {
                foreach (var client in _clients.ToList())
                {
                    try { client.Send(jsonBytes); }
                    catch { _clients.Remove(client); }
                }
            }
        }

        public void NotifyClientsProgress(string id)
        {
            // Do not recreate backupWorkManager
            // backupWorkManager = new BackupWorkManager();

            var travail = backupWorkManager.Works.Where(w => w.ID == id).ToList();
            var updateMsg = new { Type = "Progress", Works = travail };
            string json = JsonSerializer.Serialize(updateMsg);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            lock (_clients)
            {
                foreach (var client in _clients.ToList())
                {
                    try { client.Send(jsonBytes); }
                    catch { _clients.Remove(client); }
                }
            }
        }

        private async Task HandleClientAsync(Socket clientSocket)
        {
            using (clientSocket)
            {
                // Do not recreate backupWorkManager
                // backupWorkManager = new BackupWorkManager();

                var travaux = backupWorkManager.Works;
                var updateMsg = new { Type = "Update", Works = travaux };
                string json = JsonSerializer.Serialize(updateMsg);
                var jsonBytes = Encoding.UTF8.GetBytes(json);

                try
                {
                    await clientSocket.SendAsync(jsonBytes, SocketFlags.None);
                }
                catch
                {
                    MessageBox.Show("Erreur lors de l'envoi des travaux au client.");
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
