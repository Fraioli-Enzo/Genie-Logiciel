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
using System.Collections.ObjectModel;

namespace WpfApp1
{
    /// <summary>
    /// Logique d'interaction pour Settings.xaml
    /// </summary>
    public partial class Menu : Page
    {
        private BackupWorkManager backupWorkManager;

        public Menu()
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

        }

        private void ButtonExecute_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonLogger_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
