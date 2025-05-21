using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using WpfApp1.Model;
using System.IO;
using System.Resources;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        private BackupWorkManager backupWorkManager;
        private object resourceManager;
        private string logExtension;
        private string workingSoftware;
        private string[] extensions;
        private bool test = true;

        public MainWindow()
        {
            InitializeComponent();
            backupWorkManager = new BackupWorkManager();
            BackupDataGrid.ItemsSource = backupWorkManager.Works;

            LoadConfigAndUpdateUI(); 
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
            workingSoftware = config != null && config.ContainsKey("workingSoftware") ? config["workingSoftware"].GetString() ?? "" : "";

            // Définir la culture du ResourceManager en fonction de la langue
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            this.resourceManager = new ResourceManager("WpfApp1.Resources.Messages", typeof(MainWindow).Assembly);

            ButtonSettings.Content = ((ResourceManager)this.resourceManager).GetString("Setting");
            ButtonAdd.Content = ((ResourceManager)this.resourceManager).GetString("Add_Backup");
            //ButtonDelete.Content = ((ResourceManager)this.resourceManager).GetString("Delete_Backup");
            //ButtonExecute.Content = ((ResourceManager)this.resourceManager).GetString("Execute_Backup");
            ButtonLogger.Content = ((ResourceManager)this.resourceManager).GetString("Logger");
            BackupDataGrid.Columns[1].Header = ((ResourceManager)this.resourceManager).GetString("Name_Backup");
            BackupDataGrid.Columns[2].Header = ((ResourceManager)this.resourceManager).GetString("Source_Path");
            BackupDataGrid.Columns[3].Header = ((ResourceManager)this.resourceManager).GetString("Target_Path");
            BackupDataGrid.Columns[6].Header = ((ResourceManager)this.resourceManager).GetString("Progress");
            MenuLabel.Content = ((ResourceManager)this.resourceManager).GetString("MainMenu");
        }

        private void BackupDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddBackup addWorkWindow = new AddBackup();
            addWorkWindow.BackupAdded += AddBackupWindow_BackupAdded;
            addWorkWindow.ShowDialog();
        }

        private async void ButtonExecute_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BackupWork backup)
            {
                string id = backup.ID;
                string result = await backupWorkManager.ExecuteWorkAsync(id, logExtension, extensions, workingSoftware);

                switch (result)
                {
                    case "ExecuteWorkSuccess":
                        MessageBox.Show(((ResourceManager)this.resourceManager).GetString("ExecuteSuccess"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "ProcessAlreadyRunning":
                        MessageBox.Show(((ResourceManager)this.resourceManager).GetString("ProcessAlreadyRunning"), "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
            else
            {
                MessageBox.Show(((ResourceManager)this.resourceManager).GetString("SelectWorkToExecute"), "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BackupWork backup)
            {
                string id = backup.ID;
                backupWorkManager.RemoveWork(id);
                BackupDataGrid.ItemsSource = null;
                BackupDataGrid.ItemsSource = backupWorkManager.Works;

                MessageBox.Show(((ResourceManager)this.resourceManager).GetString("DeleteSuccess"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(((ResourceManager)this.resourceManager).GetString("SelectWorkToDelete"), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BackupWork backup)
            {
                string id = backup.ID;
                if (backup.IsPaused)
                {
                    await Task.Run(() => backupWorkManager.ResumeBackupAsync(id));
                }
                else
                {
                    await Task.Run(() => backupWorkManager.PauseBackupAsync(id));
                }
            }
            else
            {
                MessageBox.Show(((ResourceManager)this.resourceManager).GetString("SelectWorkToPause"), "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BackupWork backup)
            {
                string id = backup.ID;
                await backupWorkManager.StopBackupAsync(id);
                MessageBox.Show(((ResourceManager)this.resourceManager).GetString("StopSuccess") ?? "Backup stopped.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(((ResourceManager)this.resourceManager).GetString("SelectWorkToStop") ?? "Select a backup to stop.", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.ShowDialog();
            LoadConfigAndUpdateUI();
        }

        private void AddBackupWindow_BackupAdded(object sender, EventArgs e)
        {
            backupWorkManager = new BackupWorkManager();
            BackupDataGrid.ItemsSource = null;
            BackupDataGrid.ItemsSource = backupWorkManager.Works;
        }

        private void ButtonLogger_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}