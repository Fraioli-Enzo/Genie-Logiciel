using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
