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

        public BackupWorkManager()
        {
            InitializeWorksFromState();
        }

        private void InitializeWorksFromState()
        {
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string jsonFilePath = Path.Combine(projectRootPath, "state.json");

            if (File.Exists(jsonFilePath))
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                Works = JsonSerializer.Deserialize<List<BackupWork>>(jsonContent) ?? new List<BackupWork>();
            }
        }

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
            // Vérifier si le fichier state.json existe
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string jsonFilePath = Path.Combine(projectRootPath, "state.json");

            List<BackupWork> existingWorks = new List<BackupWork>();

            if (File.Exists(jsonFilePath))
            {
                // Charger les travaux existants depuis le fichier JSON  
                string existingJsonContent = File.ReadAllText(jsonFilePath);
                existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();
            }

            // Ajouter le nouveau travail à la liste existante  
            existingWorks.Add(newWork);

            // Créer le contenu JSON mis à jour  
            string updatedJsonContent = JsonSerializer.Serialize(existingWorks, new JsonSerializerOptions { WriteIndented = true });

            // Sauvegarder le fichier JSON mis à jour  
            File.WriteAllText(jsonFilePath, updatedJsonContent);

            Works.Add(newWork);
            return "AddWorkSuccess";
        }

        public string RemoveWork(string workName)
        {
            // Find the work in the in-memory list
            var workToRemove = Works.FirstOrDefault(w => w.Name == workName);

            if (workToRemove != null)
            {
                // Remove from the in-memory list
                Works.Remove(workToRemove);

                // Update state.json
                string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")); 
                string jsonFilePath = Path.Combine(projectRootPath, "state.json");

                if (File.Exists(jsonFilePath))
                {
                    // Load existing works from state.json
                    string existingJsonContent = File.ReadAllText(jsonFilePath);
                    var existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();

                    // Remove the work from the JSON list
                    existingWorks = existingWorks.Where(w => w.Name != workName).ToList();

                    // Save the updated list back to state.json
                    string updatedJsonContent = JsonSerializer.Serialize(existingWorks, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(jsonFilePath, updatedJsonContent);
                }

                return "RemoveWorkSuccess";
            }

            // Return error if the work was not found
            return "RemoveWorkError";
        }

        public string DisplayWorks()
        {
            // Déterminer le chemin du fichier state.json
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string jsonFilePath = Path.Combine(projectRootPath, "state.json");

            // Vérifier si le fichier existe
            if (!File.Exists(jsonFilePath))
            {
                return "DisplayWorksError";
            }

            // Charger et désérialiser le contenu du fichier JSON
            string jsonContent = File.ReadAllText(jsonFilePath);
            var worksFromFile = JsonSerializer.Deserialize<List<BackupWork>>(jsonContent) ?? new List<BackupWork>();

            // Vérifier si des travaux existent
            if (worksFromFile.Count == 0)
            {
                return "DisplayWorksError";
            }

            // Retourner les travaux sous forme de chaîne formatée
            return string.Join(Environment.NewLine, worksFromFile.Select((w, index) => $"{index + 1}. {w.Name} ({w.SourcePath} -> {w.TargetPath})"));
        }

        public string ExecuteWork(string workName)
        {
            // Find the work in the in-memory list
            var workToExecute = Works.FirstOrDefault(w => w.Name == workName);
            if (workToExecute != null)
            {
                string sourcePath = workToExecute.SourcePath;
                string targetPath = workToExecute.TargetPath;

                // Check if source directory exists
                if (!Directory.Exists(sourcePath))
                {
                    return "SourceDirectoryNotFound";
                }

                // Ensure target directory exists
                Directory.CreateDirectory(targetPath);

                try
                {
                    // Get all files from the source directory
                    var allFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

                    int totalFiles = allFiles.Length;
                    int filesCopied = 0;

                    foreach (var file in allFiles)
                    {
                        // Determine the relative path and target file path
                        string relativePath = Path.GetRelativePath(sourcePath, file);
                        string targetFilePath = Path.Combine(targetPath, relativePath);

                        // Ensure the target directory exists
                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);

                        // Copy the file
                        File.Copy(file, targetFilePath, overwrite: true);
                        filesCopied++;

                        // Update progress
                        workToExecute.NbFilesLeftToDo = (totalFiles - filesCopied).ToString();
                        workToExecute.Progression = ((filesCopied * 100) / totalFiles).ToString();
                    }

                    // Update state.json
                    string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
                    string jsonFilePath = Path.Combine(projectRootPath, "state.json");

                    if (File.Exists(jsonFilePath))
                    {
                        string existingJsonContent = File.ReadAllText(jsonFilePath);
                        var existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();

                        var workIndex = existingWorks.FindIndex(w => w.Name == workName);
                        if (workIndex != -1)
                        {
                            existingWorks[workIndex].NbFilesLeftToDo = workToExecute.NbFilesLeftToDo;
                            existingWorks[workIndex].Progression = workToExecute.Progression;
                        }

                        string updatedJsonContent = JsonSerializer.Serialize(existingWorks, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(jsonFilePath, updatedJsonContent);
                    }

                    return "ExecuteWorkSuccess";
                }
                catch (Exception ex)
                {
                    // Handle any errors during file copy
                    return $"ExecuteWorkError: {ex.Message}";
                }
            }

            return "ExecuteWorkError";
        }
    }
}
