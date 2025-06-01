using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using WpfApp1.Model;
using System.IO;
using System.Resources;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using WpfApp1.Server;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private SocketBackupServer? _server;
        private BackupWorkManager backupWorkManager;

        private object resourceManager;
        private string logExtension;
        private string workingSoftware;
        private string[] extensions;
        private string[] extensionsPrio;
        private string maxKo;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfigAndUpdateUI();

            backupWorkManager = new BackupWorkManager();
            BackupDataGrid.ItemsSource = backupWorkManager.Works;

            backupWorkManager.ProgressChanged += BackupWorkManager_ProgressChanged;

            // Pass the same instance of BackupWorkManager to the server
            _server = new SocketBackupServer(8080, backupWorkManager);
            _server.MessageReceived += OnMessageReceived;
            _server.Start();
        }

        private async void OnMessageReceived(string msg)
        {
            try
            {
                var message = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(msg);
                if (message != null && message.ContainsKey("Type") && message.ContainsKey("WorkId"))
                {
                    string type = message["Type"].GetString() ?? "";
                    string workId = message["WorkId"].GetString() ?? "";
                    System.Diagnostics.Debug.WriteLine($"Type reçu: {type}, WorkId: {workId}");

                    switch (type)
                    {
                        case "Execute":
                             string result = await backupWorkManager.ExecuteWorkAsync(workId, logExtension, extensions, workingSoftware, maxKo, extensionsPrio);
                            if (result == "ExecuteWorkSuccess")
                            {
                                _server?.NotifyClientsSucess("ExecuteSucess");
                            }

                            break;
                        case "Pause":
                            backupWorkManager.PauseWork(workId);
                            break;
                        case "Stop":
                             backupWorkManager.StopWork(workId);
                            _server?.NotifyClientsSucess("StopSucess");
                            break;
                    }
                }
                else
                {
                    //MessageBox.Show("Message du serveur invalide.");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Erreur lors du traitement du message serveur : {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _server?.Stop();
            base.OnClosed(e);
        }

        private void BackupWorkManager_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            string workId = e.WorkId;
            _server?.NotifyClientsProgress(workId);
        }

        private void LoadConfigAndUpdateUI()
        {
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string configFilePath = Path.Combine(projectRootPath, "config.json");
            string configContent = File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(configContent);

            string language = config != null && config.ContainsKey("language") ? config["language"].GetString() ?? "en" : "en";
            logExtension = config != null && config.ContainsKey("log") ? config["log"].GetString() ?? "json" : "json";
            extensions = config != null && config.ContainsKey("extensionsToCrypto") ? config["extensionsToCrypto"].EnumerateArray().Select(e => e.GetString()).ToArray() : Array.Empty<string>();
            extensionsPrio = config != null && config.ContainsKey("extensionsToPrio") ? config["extensionsToPrio"].EnumerateArray().Select(e => e.GetString()).ToArray() : Array.Empty<string>();
            workingSoftware = config != null && config.ContainsKey("workingSoftware") ? config["workingSoftware"].GetString() ?? "" : "";
            maxKo = config != null && config.ContainsKey("MaxFileSize") ? config["MaxFileSize"].GetString() ?? "1024" : "1024";
            // Set the culture of the ResourceManager based on the language
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            this.resourceManager = new ResourceManager("WpfApp1.Resources.Messages", typeof(MainWindow).Assembly);

            ButtonSettings.Content = ((ResourceManager)this.resourceManager).GetString("Setting");
            ButtonAdd.Content = ((ResourceManager)this.resourceManager).GetString("Add_Backup");
            ButtonLogger.Content = ((ResourceManager)this.resourceManager).GetString("Logger");
            BackupDataGrid.Columns[1].Header = ((ResourceManager)this.resourceManager).GetString("Name_Backup");
            BackupDataGrid.Columns[2].Header = ((ResourceManager)this.resourceManager).GetString("Source_Path");
            BackupDataGrid.Columns[3].Header = ((ResourceManager)this.resourceManager).GetString("Target_Path");
            BackupDataGrid.Columns[6].Header = ((ResourceManager)this.resourceManager).GetString("Progress");
            MenuLabel.Content = ((ResourceManager)this.resourceManager).GetString("MainMenu");
        }

        public async void ShowBanner(string message, bool isSuccess)
        {
            BannerText.Text = message;
            Banner.Background = isSuccess ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            Banner.Visibility = Visibility.Visible;

            await Task.Delay(3000);

            Banner.Visibility = Visibility.Collapsed;
        }
        //------------------------------------TO DO--------------------------------------
        private void ButtonLogger_Click(object sender, RoutedEventArgs e)
        {

        }

        //---------------------------------ACTIONS-------------------------------------

        private async void ButtonExecute_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BackupWork backup)
            {
                if (backup.State == "INACTIVE")
                {
                    string id = backup.ID;
                    string result = await backupWorkManager.ExecuteWorkAsync(id, logExtension, extensions, workingSoftware, maxKo, extensionsPrio);

                    switch (result)
                    {
                        case "ExecuteWorkSuccess":
                            //MessageBox.Show(((ResourceManager)this.resourceManager).GetString("ExecuteSuccess"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            ShowBanner(((ResourceManager)this.resourceManager).GetString("ExecuteSuccess"), true);
                            break;
                        case "ProcessRunning":
                            ShowBanner(((ResourceManager)this.resourceManager).GetString("ProcessRunning"), false);
                            //MessageBox.Show(((ResourceManager)this.resourceManager).GetString("ProcessRunning"), "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        case "ExecuteWorkError":
                            ShowBanner(((ResourceManager)this.resourceManager).GetString("ExecuteWorkError"), false);
                            //MessageBox.Show(((ResourceManager)this.resourceManager).GetString("ExecuteWorkError"), "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
            }
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BackupWork backup)
            {
                if (backup.State == "ACTIVE")
                {
                    string id = backup.ID;
                    string result = backupWorkManager.PauseWork(id);
                    switch (result)
                    {
                        case "PauseWorkSuccess":
                            ShowBanner(((ResourceManager)this.resourceManager).GetString("PauseWorkSuccess"), true);
                            //MessageBox.Show(((ResourceManager)this.resourceManager).GetString("PauseWorkSuccess"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case "PauseWorkError":
                            ShowBanner(((ResourceManager)this.resourceManager).GetString("PauseWorkError"), false);
                            //MessageBox.Show(((ResourceManager)this.resourceManager).GetString("PauseWorkError"), "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BackupWork backup)
            {
                if (backup.State == "ACTIVE")
                {
                    string id = backup.ID;
                    string result = backupWorkManager.StopWork(id);
                    switch (result)
                    {
                        case "StopWorkSuccess":
                            ShowBanner(((ResourceManager)this.resourceManager).GetString("StopWorkSuccess"), true);
                            //MessageBox.Show(((ResourceManager)this.resourceManager).GetString("StopWorkSuccess"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case "StopWorkError":
                            ShowBanner(((ResourceManager)this.resourceManager).GetString("StopWorkError"), false);
                            //MessageBox.Show(((ResourceManager)this.resourceManager).GetString("StopWorkError"), "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {

            if (sender is Button button && button.DataContext is BackupWork backup)
            {
                if (backup.State == "INACTIVE")
                {
                    string id = backup.ID;
                    backupWorkManager.RemoveWork(id);
                    BackupDataGrid.ItemsSource = null;
                    BackupDataGrid.ItemsSource = backupWorkManager.Works;
                    _server.NotifyClients();

                    //MessageBox.Show(((ResourceManager)this.resourceManager).GetString("DeleteSuccess"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    ShowBanner(((ResourceManager)this.resourceManager).GetString("DeleteSuccess"), true);
                }
            }
            else
            {
                ShowBanner(((ResourceManager)this.resourceManager).GetString("SelectWorkToDelete"), false);
                //MessageBox.Show(((ResourceManager)this.resourceManager).GetString("SelectWorkToDelete"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //---------------------------------OPEN WINDOWS-------------------------------------

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddBackup addWorkWindow = new AddBackup();
            addWorkWindow.BackupAdded += AddBackupWindow_BackupAdded;
            addWorkWindow.ShowDialog();
            _server.NotifyClients();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.ShowDialog();
            LoadConfigAndUpdateUI();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BackupWork backup)
            {
                if (backup.State == "INACTIVE")
                {
                    EditBackup editBackupWindow = new EditBackup(backup);
                    editBackupWindow.BackupEdited += EditBackupWindow_BackupEdited;
                    editBackupWindow.ShowDialog();
                    _server.NotifyClients();
                }
            }
            else
            {
                ShowBanner(((ResourceManager)this.resourceManager).GetString("SelectWorkToEdit"), false);
                //MessageBox.Show(((ResourceManager)this.resourceManager).GetString("SelectWorkToEdit"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddBackupWindow_BackupAdded(object sender, EventArgs e)
        {
            backupWorkManager = new BackupWorkManager();
            BackupDataGrid.ItemsSource = null;
            BackupDataGrid.ItemsSource = backupWorkManager.Works;
            
        }

        private void EditBackupWindow_BackupEdited(object sender, EventArgs e)
        {
            backupWorkManager = new BackupWorkManager();
            BackupDataGrid.ItemsSource = null;
            BackupDataGrid.ItemsSource = backupWorkManager.Works;
        }
    }
}