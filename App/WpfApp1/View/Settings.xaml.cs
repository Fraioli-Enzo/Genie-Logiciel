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
using System.IO;
using System.Text.Json;

namespace WpfApp1
{
    /// <summary>
    /// Logique d'interaction pour Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            // Charger la langue et les logs depuis config.json
            try
            {
                string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
                string configPath = Path.Combine(projectRootPath, "config.json");
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("language", out var langProp))
                    {
                        string? lang = langProp.GetString();
                        if (lang == "fr")
                        {
                            RadioButtonFR.IsChecked = true;
                            RadioButtonEN.IsChecked = false;
                        }
                        else if (lang == "en")
                        {
                            RadioButtonFR.IsChecked = false;
                            RadioButtonEN.IsChecked = true;
                        }
                    }
                    if (doc.RootElement.TryGetProperty("log", out var logProp))
                    {
                        string? log = logProp.GetString();
                        if (log == "json")
                        {
                            RadioButtonJSON.IsChecked = true;
                            RadioButtonXML.IsChecked = false;
                        }
                        else if (log == "xml")
                        {
                            RadioButtonXML.IsChecked = true;
                            RadioButtonJSON.IsChecked = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement de la configuration : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonFR_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonEN_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonXML_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonJSON_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
