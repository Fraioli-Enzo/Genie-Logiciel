using System.Windows;
using System.IO;
using System.Text.Json;
using System.Resources;
using System.Globalization;
using WpfApp1.ViewModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace WpfApp1
{
    public partial class Settings : Window
    {
        private object resourceManager;
        private readonly EasySafeViewModel viewModel = new EasySafeViewModel();

        public ObservableCollection<string> AvailableSoftwares { get; set; } = new ObservableCollection<string>
        {
            "CalculatorApp", "WINWORD", "EXCEL", "notepad", "chrome"
        };

        public ObservableCollection<string> SelectedSoftwares { get; set; } = new ObservableCollection<string>();

        public Settings()
        {
            InitializeComponent();
            DataContext = this;

            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string configFilePath = Path.Combine(projectRootPath, "config.json");
            string configContent = File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(configContent);

            string language = config != null && config.ContainsKey("language") ? config["language"].GetString() ?? "en" : "en";

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
            LabelDetector.Content = ((ResourceManager)this.resourceManager).GetString("Detect_Software");
            Extension_File_Encrypt.Content = ((ResourceManager)this.resourceManager).GetString("Extension_File_Encrypt");
            ButtonAddSoftware.Content = ((ResourceManager)this.resourceManager).GetString("Add");
            ButtonAdd_Ext.Content = ((ResourceManager)this.resourceManager).GetString("Add");
            FileSizeText.Content = ((ResourceManager)this.resourceManager).GetString("FileSizeText");

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

                    if (doc.RootElement.TryGetProperty("extensionsToCrypto", out var extProp) && extProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var extElem in extProp.EnumerateArray())
                        {
                            string? extension = extElem.GetString();
                            if (!string.IsNullOrWhiteSpace(extension))
                            {
                                // Vérifie si un type de fichier n'est pas déjà présent
                                bool alreadyExists = ExtensionCheckBoxPanel.Children
                                    .OfType<Border>()
                                    .Select(b => b.Child)
                                    .OfType<StackPanel>()
                                    .Select(sp => sp.Children[0] as TextBlock)
                                    .Any(tb => tb != null && tb.Text.Equals(extension, StringComparison.OrdinalIgnoreCase));

                                if (!alreadyExists)
                                {
                                    Border border = new Border
                                    {
                                        Style = (Style)FindResource("ExtensionTagStyle")
                                    };

                                    StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };

                                    // Remplacer la CheckBox par un TextBlock
                                    TextBlock tb = new TextBlock
                                    {
                                        Text = extension,
                                        FontSize = 14,
                                        Foreground = (System.Windows.Media.Brush)FindResource("PrimaryColor"),
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Margin = new Thickness(0)
                                    };

                                    Button btnDelete = new Button
                                    {
                                        Content = "✕",
                                        Style = (Style)FindResource("DeleteTagButtonStyle"),
                                        Tag = border
                                    };
                                    btnDelete.Click += BtnDelete_Click;

                                    sp.Children.Add(tb);
                                    sp.Children.Add(btnDelete);

                                    border.Child = sp;

                                    ExtensionCheckBoxPanel.Children.Add(border);
                                }
                            }
                        }
                    }

                    if (doc.RootElement.TryGetProperty("workingSoftware", out var wsProp) && wsProp.ValueKind == JsonValueKind.String)
                    {
                        string? sw = wsProp.GetString();
                        if (!string.IsNullOrWhiteSpace(sw) && !sw.Equals("null", StringComparison.OrdinalIgnoreCase) && !SelectedSoftwares.Contains(sw))
                        {
                            SelectedSoftwares.Add(sw);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement de la configuration : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Afficher les tags logiciels sélectionnés
            RefreshSoftwareTags();
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

            // Récupérer toutes les extensions (TextBlock) affichées
            var checkedExtensions = new List<string>();
            foreach (var border in ExtensionCheckBoxPanel.Children.OfType<Border>())
            {
                if (border.Child is StackPanel sp && sp.Children[0] is TextBlock tb)
                {
                    string ext = tb.Text;
                    // S'assurer que l'extension commence par un point
                    if (!string.IsNullOrWhiteSpace(ext))
                    {
                        if (!ext.StartsWith(".")) ext = "." + ext;
                        checkedExtensions.Add(ext.ToLowerInvariant());
                    }
                }
            }

            // Charger le fichier config.json
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")); 
            string configFilePath = Path.Combine(projectRootPath, "config.json");
            Dictionary<string, object> configDict;
            if (File.Exists(configFilePath))
            {
                string configContent = File.ReadAllText(configFilePath);
                configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(configContent) ?? new Dictionary<string, object>();
            }
            else
            {
                configDict = new Dictionary<string, object>();
            }

            configDict["extensionsToCrypto"] = checkedExtensions;
            configDict["workingSoftware"] = SelectedSoftwares.FirstOrDefault() ?? "null";

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(configFilePath, JsonSerializer.Serialize(configDict, options));

            this.Close();
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
                    .Select(sp => sp.Children[0] as TextBlock)
                    .Any(tb => tb != null && tb.Text.Equals(extension, StringComparison.OrdinalIgnoreCase));

                if (!alreadyExists)
                {
                    Border border = new Border
                    {
                        Style = (Style)FindResource("ExtensionTagStyle")
                    };

                    StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };

                    TextBlock tb = new TextBlock
                    {
                        Text = extension,
                        FontSize = 14,
                        Foreground = (System.Windows.Media.Brush)FindResource("PrimaryColor"),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0)
                    };

                    Button btnDelete = new Button
                    {
                        Content = "✕",
                        Style = (Style)FindResource("DeleteTagButtonStyle"),
                        Tag = border
                    };
                    btnDelete.Click += BtnDelete_Click;

                    sp.Children.Add(tb);
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

        private void AddSoftwareButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedSoftwares.Clear();
            RefreshSoftwareTags();

            if (SoftwareDetectComboBox.SelectedItem is string selectedSoftware && !string.IsNullOrWhiteSpace(selectedSoftware))
            {
                if (!SelectedSoftwares.Contains(selectedSoftware))
                {
                    SelectedSoftwares.Add(selectedSoftware);
                    RefreshSoftwareTags();
                }
                else
                {
                    MessageBox.Show("This software is already added.", "Duplicate", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void RefreshSoftwareTags()
        {
            SoftwareTagPanel.Children.Clear();
            foreach (var sw in SelectedSoftwares)
            {
                var border = new Border
                {
                    Style = (Style)FindResource("ExtensionTagStyle")
                };
                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                var tb = new TextBlock
                {
                    Text = sw,
                    FontSize = 14,
                    Foreground = (System.Windows.Media.Brush)FindResource("PrimaryColor"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0)
                };
                var btnDelete = new Button
                {
                    Content = "✕",
                    Style = (Style)FindResource("DeleteTagButtonStyle"),
                    Tag = sw
                };
                btnDelete.Click += RemoveSoftwareTag_Click;
                sp.Children.Add(tb);
                sp.Children.Add(btnDelete);
                border.Child = sp;
                SoftwareTagPanel.Children.Add(border);
            }
        }

        private void RemoveSoftwareTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string sw && SelectedSoftwares.Contains(sw))
            {
                SelectedSoftwares.Remove(sw);
                RefreshSoftwareTags();
            }
        }
    }
}
