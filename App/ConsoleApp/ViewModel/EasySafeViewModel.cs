using ConsoleApp.View;
using System.Globalization;
using System.Resources;
using ConsoleApp.Model;
using System.Xml.Serialization;
using System.Text.Json;
using System.IO;

namespace ConsoleApp.ViewModel
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

            // Define the path to the config.json file
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")); 
            string configFilePath = Path.Combine(projectRootPath, "config.json");

            // Read the existing config file
            var config = new Dictionary<string, string>();
            if (File.Exists(configFilePath))
            {
                string configContent = File.ReadAllText(configFilePath);
                config = JsonSerializer.Deserialize<Dictionary<string, string>>(configContent) ?? new Dictionary<string, string>();
            }

            // Update the language setting
            config["language"] = language;

            // Write the updated config back to the file
            string updatedConfigContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configFilePath, updatedConfigContent);
        }

        public string RunBackup(string choice, string name, string pathSource, string pathTarget, string type, string id)
        {

            switch (choice)
            {
                case "1":
                    string displayWork = workManager.DisplayWorks();
                    return displayWork;

                case "2":
                    string addWork = workManager.AddWork(name, pathSource, pathTarget, type);
                    return addWork;

                case "3":
                    string removeWork = workManager.RemoveWork(id);
                    return removeWork;

                case "4":
                    string executeWork = workManager.ExecuteWork(id);
                    return executeWork;

                case "5":
                    return "RunBackupExit";

                default:
                    return "RunBackupDefaultError";
            }
            
        }
    }
}
