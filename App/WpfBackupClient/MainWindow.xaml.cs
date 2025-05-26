using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using WpfBackupClient.Client;
using System.Diagnostics;

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
            Dispatcher.Invoke(() =>
            {
                try
                {
                    Debug.WriteLine($"[DEBUG] Message brut reçu : {msg}");
                    var doc = JsonDocument.Parse(msg);
                    var type = doc.RootElement.GetProperty("Type").GetString();
                    var worksJson = doc.RootElement.GetProperty("Works").GetRawText();
                    switch (type)
                    {
                        case "Update":
                            var travaux = JsonSerializer.Deserialize<List<BackupWork>>(worksJson);
                            BackupWorks.Clear();
                            foreach (var t in travaux)
                                BackupWorks.Add(t);
                            break;
                        case "Progress":
                            var travauxProgress = JsonSerializer.Deserialize<List<BackupWork>>(worksJson);
                            if (travauxProgress != null)
                            {
                                foreach (var travailMaj in travauxProgress)
                                {
                                    var travailExistant = BackupWorks.FirstOrDefault(w => w.ID == travailMaj.ID);
                                    if (travailExistant != null)
                                    {
                                        // Met à jour les propriétés pertinentes
                                        travailExistant.State = travailMaj.State;
                                        travailExistant.TotalFilesToCopy = travailMaj.TotalFilesToCopy;
                                        travailExistant.TotalFilesSize = travailMaj.TotalFilesSize;
                                        travailExistant.NbFilesLeftToDo = travailMaj.NbFilesLeftToDo;
                                        travailExistant.IsPaused = travailMaj.IsPaused;
                                        travailExistant.Progression = travailMaj.Progression;
                                        travailExistant.IsStopped = travailMaj.IsStopped;
                                    }
                                }
                            }
                            break;
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
