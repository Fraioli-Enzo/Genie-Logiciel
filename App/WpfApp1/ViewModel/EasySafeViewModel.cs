using System.Globalization;
using System.Resources;
using WpfApp1.Model;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;

namespace WpfApp1.ViewModel
{
    public class EasySafeViewModel
    {
        private readonly BackupWorkManager workManager = new BackupWorkManager(); 

        public void ChooseLanguage(string language)
        {
            if (language != "fr" && language != "en")
            {
                throw new ArgumentException("Invalid language. Only 'fr' and 'en' are supported.");
            }

            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")); 
            string configFilePath = Path.Combine(projectRootPath, "config.json");

            // Désérialisation générique pour supporter les tableaux
            JsonObject configObj;
            if (File.Exists(configFilePath))
            {
                string configContent = File.ReadAllText(configFilePath);
                configObj = JsonNode.Parse(configContent)?.AsObject() ?? new JsonObject();
            }
            else
            {
                configObj = new JsonObject();
            }

            // Mise à jour de la langue
            configObj["language"] = language;

            // Sauvegarde du fichier config avec tous les types préservés
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(configFilePath, configObj.ToJsonString(options));
        }

        public void ChooseLogExtension(string logExtension)
        {
            if (logExtension != "json" && logExtension != "xml")
            {
                throw new ArgumentException("Invalid log extension. Only 'json' and 'xml' are supported.");
            }

            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")); 
            string configFilePath = Path.Combine(projectRootPath, "config.json");

            // Désérialisation générique pour supporter les tableaux
            JsonObject configObj;
            if (File.Exists(configFilePath))
            {
                string configContent = File.ReadAllText(configFilePath);
                configObj = JsonNode.Parse(configContent)?.AsObject() ?? new JsonObject();
            }
            else
            {
                configObj = new JsonObject();
            }

            // Mise à jour du log
            configObj["log"] = logExtension;

            // Sauvegarde du fichier config avec tous les types préservés
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(configFilePath, configObj.ToJsonString(options));
        }
    }
}
