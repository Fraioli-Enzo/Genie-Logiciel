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
    /// <summary>
    /// Logique d'interaction pour AddBackup.xaml
    /// </summary>
    public partial class AddBackup : Window
    {
        private BackupWorkManager backupWorkManager;
        public event EventHandler BackupAdded;
        private object resourceManager;

        // Declare the fields as instance variables to fix the CS0103 errors
        private string name;
        private string pathSource;
        private string pathTarget;
        private string type;

        public AddBackup()
        {
            InitializeComponent();
            backupWorkManager = new BackupWorkManager();

            // Initialize the fields with default values
            name = string.Empty;
            pathSource = string.Empty;
            pathTarget = string.Empty;
            type = string.Empty;

            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string configFilePath = Path.Combine(projectRootPath, "config.json");
            string configContent = File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(configContent);
            string language = config.ContainsKey("language") ? config["language"] : "en";

            // Définir la culture du ResourceManager en fonction de la langue
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            this.resourceManager = new ResourceManager("WpfApp1.Resources.Messages", typeof(MainWindow).Assembly);

            // Set the content of the buttons and labels using the resource manager
            ButtonAdd.Content = ((ResourceManager)this.resourceManager).GetString("Add");
            ButtonCancel.Content = ((ResourceManager)this.resourceManager).GetString("Cancel");
            LabelSource.Content = ((ResourceManager)this.resourceManager).GetString("Source_Path");
            LabelTarget.Content = ((ResourceManager)this.resourceManager).GetString("Target_Path");
            LabelName.Content = ((ResourceManager)this.resourceManager).GetString("Name_Backup");
            RadioButtonFull.Content = ((ResourceManager)this.resourceManager).GetString("Complet");
            RadioButtonDifferential.Content = ((ResourceManager)this.resourceManager).GetString("Differential");
            HeaderText.Text = ((ResourceManager)this.resourceManager).GetString("Add_Backup");
            Type_Save.Content = ((ResourceManager)this.resourceManager).GetString("Type_Save");
        }

        private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBoxSource_TextChanged(object sender, TextChangedEventArgs e)
        {
 
        }

        private void TextBoxTarget_TextChanged(object sender, TextChangedEventArgs e)
        {

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
            // Update the 'type' field when the full backup radio button is checked
            type = "FULL";
        }

        private void RadioButtonDifferential_Checked(object sender, RoutedEventArgs e)
        {
            // Update the 'type' field when the differential backup radio button is checked
            type = "DIFFERENTIAL";
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            name = TextBoxName.Text;
            pathSource = TextBoxSource.Text;
            pathTarget = TextBoxTarget.Text;
            backupWorkManager.AddWork(name, pathSource, pathTarget, type);
            MessageBox.Show("Backup added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            BackupAdded?.Invoke(this, EventArgs.Empty); 
            this.Close();
        }
    }
}
