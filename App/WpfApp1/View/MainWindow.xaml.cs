using System.Text;
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
using System.Collections.ObjectModel;
using System.Linq;
using WpfApp1; // Pour accéder à AddBackup

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        private BackupWorkManager backupWorkManager;

        public MainWindow()
        {
            InitializeComponent();
            backupWorkManager = new BackupWorkManager();
            BackupDataGrid.ItemsSource = backupWorkManager.Works;
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

        private void ButtonExecute_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer les lignes sélectionnées dans le DataGrid
            var selectedItems = BackupDataGrid.SelectedItems;
            if (selectedItems != null && selectedItems.Count > 0)
            {
                var selectedWorks = selectedItems.Cast<BackupWork>().ToList();
                var ids = string.Join(";", selectedWorks.Select(w => w.ID));
                backupWorkManager.ExecuteWork(ids, "json");
                MessageBox.Show("Sauvegarde(s) exécutée(s) avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un ou plusieurs travaux de sauvegarde à exécuter.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer les lignes sélectionnées dans le DataGrid
            var selectedItems = BackupDataGrid.SelectedItems;
            if (selectedItems != null && selectedItems.Count > 0)
            {
                var selectedWorks = selectedItems.Cast<BackupWork>().ToList();
                var ids = string.Join(";", selectedWorks.Select(w => w.ID));
                backupWorkManager.RemoveWork(ids);
                BackupDataGrid.ItemsSource = null;
                BackupDataGrid.ItemsSource = backupWorkManager.Works;
                MessageBox.Show("Sauvegarde(s) supprimée(s) avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un ou plusieurs travaux de sauvegarde à supprimer.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonLogger_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.ShowDialog();
        }

        private void AddBackupWindow_BackupAdded(object sender, EventArgs e)
        {
            // Rafraîchir la liste des sauvegardes affichées
            backupWorkManager = new BackupWorkManager();
            BackupDataGrid.ItemsSource = null;
            BackupDataGrid.ItemsSource = backupWorkManager.Works;
        }
    }
}