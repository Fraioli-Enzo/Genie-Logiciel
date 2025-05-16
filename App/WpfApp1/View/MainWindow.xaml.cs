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
            // Récupérer la ligne sélectionnée dans le DataGrid
            if (BackupDataGrid.SelectedItem is BackupWork selectedWork)
            {
                // Exécuter le backup pour l'ID sélectionné
                backupWorkManager.ExecuteWork(selectedWork.ID, "json");
                MessageBox.Show("Sauvegarde exécutée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un travail de sauvegarde à exécuter.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer la ligne sélectionnée dans le DataGrid
            if (BackupDataGrid.SelectedItem is BackupWork selectedWork)
            {
                // Supprimez le backup pour l'ID sélectionné
                backupWorkManager.RemoveWork(selectedWork.ID);
                MessageBox.Show("Sauvegarde supprimée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                BackupDataGrid.ItemsSource = null;
                BackupDataGrid.ItemsSource = backupWorkManager.Works;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un travail de sauvegarde à exécuter.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
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