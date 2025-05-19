using System.Windows;
using System.IO;
using System.Text.Json;
using System.Resources;
using System.Globalization;
using WpfApp1.ViewModel;
using System.Collections.Generic;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// Logique d'interaction pour Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private object resourceManager;
        private readonly EasySafeViewModel viewModel = new EasySafeViewModel(); // Ajout du ViewModel

        public Settings()
        {
            InitializeComponent();

            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string configFilePath = Path.Combine(projectRootPath, "config.json");
            string configContent = File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(configContent);
            string language = config.ContainsKey("language") ? config["language"] : "en";

            // Définir la culture du ResourceManager en fonction de la langue
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            this.resourceManager = new ResourceManager("WpfApp1.Resources.Messages", typeof(MainWindow).Assembly);

            // Set the content of the buttons and labels using the resource manager
            ButtonSave.Content = ((ResourceManager)this.resourceManager).GetString("Save");
            ButtonCancel.Content = ((ResourceManager)this.resourceManager).GetString("Cancel");
            LabelLanguage.Content = ((ResourceManager)this.resourceManager).GetString("Language");
            LabelLog.Content = ((ResourceManager)this.resourceManager).GetString("Log_File_Type");
            RadioButtonFR.Content = ((ResourceManager)this.resourceManager).GetString("French");
            RadioButtonEN.Content = ((ResourceManager)this.resourceManager).GetString("English");
            HeaderText.Text = ((ResourceManager)this.resourceManager).GetString("Setting");

            // Charger la langue et les logs depuis config.json
            try
            {
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
            string selectedLanguage = RadioButtonFR.IsChecked == true ? "fr" : "en";
            string selectedLogExtension = RadioButtonJSON.IsChecked == true ? "json" : "xml";
            viewModel.ChooseLanguage(selectedLanguage);
            viewModel.ChooseLogExtension(selectedLogExtension);

            this.Close();

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

        private void TextBoxTarget_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void AddExtensionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            string extension = Type_File.Text.Trim();

            // Vérifie si le champ de saisi n'est pas vide
            if (!string.IsNullOrEmpty(extension))
            {
                // Vérifie si un type de fichier n'est pas dejà présent
                bool alreadyExists = ExtensionCheckBoxPanel.Children
                    .OfType<Border>()
                    .Select(b => b.Child)
                    .OfType<StackPanel>()
                    .Select(sp => sp.Children[0] as CheckBox)
                    .Any(cb => cb != null && cb.Content.ToString().Equals(extension, StringComparison.OrdinalIgnoreCase));

                if (!alreadyExists)
                {
                    Border border = new Border
                    {
                        Style = (Style)FindResource("ExtensionTagStyle")
                    };

                    // StackPanel horizontal pour CheckBox + bouton supprimer
                    StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };

                    // CheckBox avec le nom de l'extension
                    CheckBox cb = new CheckBox
                    {
                        Content = extension,
                        Style = (Style)FindResource("TagCheckBoxStyle"),
                        IsChecked = true
                    };

                    // Bouton supprimer à coté de la CheckBox
                    Button btnDelete = new Button
                    {
                        Content = "✕",
                        Style = (Style)FindResource("DeleteTagButtonStyle"),
                        Tag = border // On met le Border dans Tag pour pouvoir le retrouver au clic
                    };
                    btnDelete.Click += BtnDelete_Click;

                    sp.Children.Add(cb);
                    sp.Children.Add(btnDelete);

                    border.Child = sp;

                    ExtensionCheckBoxPanel.Children.Add(border);

                    Type_File.Text = "";
                }
                else
                {
                    MessageBox.Show("This extension is already added.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Border border)
            {
                ExtensionCheckBoxPanel.Children.Remove(border);
            }
        }
    }
}
