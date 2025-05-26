using System.Collections.ObjectModel;
using System.Windows;
using WpfBackupClient.Client;

namespace WpfBackupClient
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<BackupWork> BackupWorks { get; set; } = new();
        private SocketBackupClient? _client;

        public MainWindow()
        {
            InitializeComponent();
            BackupDataGrid.ItemsSource = BackupWorks;

            _client = new SocketBackupClient("127.0.0.1", 8080); // IP et port du serveur
            _client.MessageReceived += OnMessageReceived;
            Loaded += async (_, __) => await _client.ConnectAsync();
        }

        private void OnMessageReceived(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                MessageBox.Show("Message reçu vide ou null.");
                return;
            }

            Dispatcher.Invoke(() =>
            {
                try
                {
                    var travaux = System.Text.Json.JsonSerializer.Deserialize<List<BackupWork>>(msg);
                    BackupWorks.Clear();
                    if (travaux != null)
                    {
                        foreach (var t in travaux)
                        {
                            BackupWorks.Add(t);
                        }


                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur de désérialisation : " + ex.Message);
                }
            });
        }


        protected override void OnClosed(EventArgs e)
        {
            _client?.Disconnect();
            base.OnClosed(e);
        }
    }
}
