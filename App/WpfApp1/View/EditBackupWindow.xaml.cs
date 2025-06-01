using Microsoft.Win32;
using System.Globalization;
using System.Resources;
using System.Text.Json;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Model;

namespace WpfApp1
{
    public partial class EditBackup : Window
    {
        private BackupWorkManager backupWorkManager;
        public event EventHandler BackupEdited;
        private object resourceManager;

        private string name;
        private string pathSource;
        private string pathTarget;
        private string type;
        private string id;

        public EditBackup(BackupWork work)
        {
            InitializeComponent();
            backupWorkManager = new BackupWorkManager();

            // Initialize the fields with default values
            TextBoxName.Text = work.Name;
            TextBoxSource.Text = work.SourcePath;
            TextBoxTarget.Text = work.TargetPath;
            id = work.ID;
            if (work.Type == "FULL")
            {
                RadioButtonFull.IsChecked = true;
                type = "FULL";
            }
            else if (work.Type == "DIFFERENTIAL")
            {
                RadioButtonDifferential.IsChecked = true;
                type = "DIFFERENTIAL";
            }

            LoadConfigAndUpdateUI();
            TextLanguageConfiguration();
        }

        private void LoadConfigAndUpdateUI()
        {
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string configFilePath = Path.Combine(projectRootPath, "config.json");
            string configContent = File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(configContent);
            string language = config != null && config.ContainsKey("language") ? config["language"].GetString() ?? "en" : "en";

            // Définir la culture du ResourceManager en fonction de la langue
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            this.resourceManager = new ResourceManager("WpfApp1.Resources.Messages", typeof(MainWindow).Assembly);

        }
        private void TextLanguageConfiguration()
        {
            // Set the content of the buttons and labels using the resource manager
            ButtonEdit.Content = ((ResourceManager)this.resourceManager).GetString("Edit");
            ButtonCancel.Content = ((ResourceManager)this.resourceManager).GetString("Cancel");
            LabelSource.Content = ((ResourceManager)this.resourceManager).GetString("Source_Path");
            LabelTarget.Content = ((ResourceManager)this.resourceManager).GetString("Target_Path");
            LabelName.Content = ((ResourceManager)this.resourceManager).GetString("Name_Backup");
            RadioButtonFull.Content = ((ResourceManager)this.resourceManager).GetString("Complet");
            RadioButtonDifferential.Content = ((ResourceManager)this.resourceManager).GetString("Differential");
            HeaderText.Text = ((ResourceManager)this.resourceManager).GetString("Edit_Backup");
            Type_Save.Content = ((ResourceManager)this.resourceManager).GetString("Type_Save");
        }
        private void ButtonSource_Click(object sender, RoutedEventArgs e)
        {
            var dialogSource = new OpenFolderDialog();
            if (dialogSource.ShowDialog() == true)
            {
                TextBoxSource.Text = dialogSource.FolderName;
            }
        }

        private void ButtonTarget_Click(object sender, RoutedEventArgs e)
        {
            var dialogTarget = new OpenFolderDialog();
            if (dialogTarget.ShowDialog() == true)
            {
                TextBoxTarget.Text = dialogTarget.FolderName;
            }
        }

        private void RadioButtonFull_Checked(object sender, RoutedEventArgs e)
        {
            type = "FULL";
        }

        private void RadioButtonDifferential_Checked(object sender, RoutedEventArgs e)
        {
            type = "DIFFERENTIAL";
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            name = TextBoxName.Text;
            pathSource = TextBoxSource.Text;
            pathTarget = TextBoxTarget.Text;
            backupWorkManager.EditWork(id, name, pathSource, pathTarget, type);
            BackupEdited?.Invoke(this, EventArgs.Empty); 
            this.Close();
        }
    }
}
