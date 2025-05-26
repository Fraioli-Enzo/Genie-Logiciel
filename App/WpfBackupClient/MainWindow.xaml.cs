using System.Windows;
using WpfBackupClient.Client;

namespace WpfBackupClient
{
    public partial class MainWindow : Window
    {
        private SocketBackupClient? _client;

        public MainWindow()
        {
            InitializeComponent();
            _client = new SocketBackupClient("127.0.0.1", 8080); // IP et port du serveur
            _client.MessageReceived += OnMessageReceived;
            Loaded += async (_, __) => await _client.ConnectAsync();
        }

        private void OnMessageReceived(string msg)
        {
            // Afficher le message dans l'UI, par exemple :
            Dispatcher.Invoke(() => MessageBox.Show($"Message du serveur : {msg}"));
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_client != null)
                await _client.SendMessageAsync("Hello serveur !");
        }

        protected override void OnClosed(EventArgs e)
        {
            _client?.Disconnect();
            base.OnClosed(e);
        }
    }
}
