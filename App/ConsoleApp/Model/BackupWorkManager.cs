using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp.Model
{
    public class BackupWorkManager
    {
        private const int MaxWorks = 5;

        public List<BackupWork> Works { get; set; } = new List<BackupWork>();

        public string AddWork(string name, string pathSource, string pathTarget, string type)
        {
            if (Works.Count >= MaxWorks)
            {
                return "AddWorkError";
            }

            // Calculer le nombre total de fichiers et leur taille totale
            var allFiles = Directory.GetFiles(pathSource, "*", SearchOption.AllDirectories);
            int totalFilesToCopy = allFiles.Length;
            long totalFilesSize = allFiles.Sum(file => new FileInfo(file).Length);


            var newWork = new BackupWork(
                name: name,
                sourcePath: pathSource,
                targetPath: pathTarget,
                type: type,
                totalFilesToCopy: totalFilesToCopy.ToString(),
                totalFilesSize: totalFilesSize.ToString(),
                nbFilesLeftToDo: totalFilesToCopy.ToString()
            );

            // Create JSON content
            string jsonContent = JsonSerializer.Serialize(newWork, new JsonSerializerOptions { WriteIndented = true });

            // Save JSON file
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string jsonFilePath = Path.Combine(projectRootPath, "state.json");

            File.WriteAllText(jsonFilePath, jsonContent);

            Works.Add(newWork);
            return "AddWorkSuccess";
        }

        public string RemoveWork(string workName)
        {
            var workToRemove = Works.FirstOrDefault(w => w.Name == workName);
            if (workToRemove != null)
            {
                Works.Remove(workToRemove);
                return "RemoveWorkSuccess";
            }
            return "RemoveWorkError";
        }

        public string DisplayWorks()
        {
            if (Works.Count == 0)
            {
                return "DisplayWorksError";
            }
            else
            {
                return string.Join(", ", Works.Select(w => $"{w.Name} ({w.SourcePath} -> {w.TargetPath})"));
            }
        }
    }
}
