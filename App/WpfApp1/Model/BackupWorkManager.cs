using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LoggingLibrary;


namespace WpfApp1.Model
{
    public class BackupWorkManager
    {

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

            // Calculer le nombre total de fichiers et leur taille totale
            var allFiles = Directory.GetFiles(pathSource, "*", SearchOption.AllDirectories);
            int totalFilesToCopy = allFiles.Length;
            long totalFilesSize = allFiles.Sum(file => new FileInfo(file).Length);
            string id = (Works.Count + 1).ToString();

            var newWork = new BackupWork(
                id: id,
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

        public string RemoveWork(string ids)
        {
            // Parse les IDs à supprimer
            var idsToRemove = ParseIds(ids);

            if (idsToRemove.Count == 0)
                return "RemoveWorkError";

            // Supprime de la liste en mémoire
            Works.RemoveAll(w => idsToRemove.Contains(w.ID));

            // Met à jour state.json
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")); 
            string jsonFilePath = Path.Combine(projectRootPath, "state.json");

            if (File.Exists(jsonFilePath))
            {
                string existingJsonContent = File.ReadAllText(jsonFilePath);
                var existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();

                // Supprime les travaux correspondants
                existingWorks = existingWorks.Where(w => !idsToRemove.Contains(w.ID)).ToList();

                string updatedJsonContent = JsonSerializer.Serialize(existingWorks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonFilePath, updatedJsonContent);
            }

            // Vérifie si au moins un travail a été supprimé
            return "RemoveWorkSuccess";
        }

        public string ExecuteWork(string ids, string log)
        {
            var idsToExecute = ParseIds(ids);
            if (idsToExecute.Count == 0)
                return "ExecuteWorkError";

            var results = new List<string>();
            foreach (var id in idsToExecute)
            {
                var result = ExecuteSingleWork(id, log);
                results.Add(result);
            }

            if (results.All(r => r == "ExecuteWorkSuccess"))
                return "ExecuteWorkSuccess";
            if (results.Any(r => r == "SourceDirectoryNotFound"))
                return "SourceDirectoryNotFound";
            // Retourne le premier message d'erreur rencontré sinon
            var firstError = results.FirstOrDefault(r => r != "ExecuteWorkSuccess");
            return firstError ?? "ExecuteWorkError";
        }

        // Ancienne logique d'exécution d'un seul travail, rendue privée
        private string ExecuteSingleWork(string id, string log)
        {
            var workToExecute = Works.FirstOrDefault(w => w.ID == id);
            if (workToExecute != null)
            {
                string sourcePath = workToExecute.SourcePath;
                string targetPath = workToExecute.TargetPath;

                if (!Directory.Exists(sourcePath))
                {
                    return "SourceDirectoryNotFound";
                }

                var allFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

                // Ajout : filtrage selon le type de sauvegarde
                List<string> filesToCopy;
                if (workToExecute.Type == "FULL")
                {
                    filesToCopy = allFiles.ToList();
                }
                else if (workToExecute.Type == "DIFFERENTIAL")
                {
                    filesToCopy = new List<string>();
                    foreach (var file in allFiles)
                    {
                        string relativePath = Path.GetRelativePath(sourcePath, file);
                        string targetFilePath = Path.Combine(targetPath, relativePath);
                        if (!File.Exists(targetFilePath))
                        {
                            filesToCopy.Add(file);
                        }
                    }
                }
                else
                {
                    // Type inconnu, ne rien faire
                    return "ExecuteWorkError";
                }

                int totalFiles = filesToCopy.Count;
                long totalSize = filesToCopy.Sum(file => new FileInfo(file).Length);

                workToExecute.TotalFilesToCopy = totalFiles.ToString();
                workToExecute.TotalFilesSize = totalSize.ToString();
                workToExecute.NbFilesLeftToDo = totalFiles.ToString();
                workToExecute.Progression = "0";

                string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")); 
                string jsonFilePath = Path.Combine(projectRootPath, "state.json");

                if (File.Exists(jsonFilePath))
                {
                    string existingJsonContent = File.ReadAllText(jsonFilePath);
                    var existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();

                    var workIndex = existingWorks.FindIndex(w => w.ID == id);
                    if (workIndex != -1)
                    {
                        existingWorks[workIndex].TotalFilesToCopy = workToExecute.TotalFilesToCopy;
                        existingWorks[workIndex].TotalFilesSize = workToExecute.TotalFilesSize;
                        existingWorks[workIndex].NbFilesLeftToDo = workToExecute.NbFilesLeftToDo;
                        existingWorks[workIndex].Progression = workToExecute.Progression;
                    }

                    string updatedJsonContent = JsonSerializer.Serialize(existingWorks, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(jsonFilePath, updatedJsonContent);
                }

                Directory.CreateDirectory(targetPath);

                try
                {
                    int filesCopied = 0;

                    foreach (var file in filesToCopy)
                    {
                        string relativePath = Path.GetRelativePath(sourcePath, file);
                        string targetFilePath = Path.Combine(targetPath, relativePath);

                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);

                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        File.Copy(file, targetFilePath, overwrite: true);

                        stopwatch.Stop();
                        double fileTransferTime = stopwatch.Elapsed.TotalMilliseconds;

                        long fileSize = new FileInfo(file).Length;
                        LoggingLibrary.Logger.Log(id, file, targetFilePath, fileSize, fileTransferTime, log);

                        filesCopied++;

                        workToExecute.NbFilesLeftToDo = (totalFiles - filesCopied).ToString();
                        workToExecute.Progression = (totalFiles == 0 ? "100" : ((filesCopied * 100) / totalFiles).ToString());
                    }

                    if (File.Exists(jsonFilePath))
                    {
                        string existingJsonContent = File.ReadAllText(jsonFilePath);
                        var existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();

                        var workIndex = existingWorks.FindIndex(w => w.ID == id);
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
                    return $"ExecuteWorkError: {ex.Message}";
                }
            }

            return "ExecuteWorkError";
        }

        private static HashSet<string> ParseIds(string ids)
        {
            var result = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(ids))
                return result;

            foreach (var part in ids.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (part.Contains('-'))
                {
                    var range = part.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end) && start <= end)
                    {
                        for (int i = start; i <= end; i++)
                            result.Add(i.ToString());
                    }
                }
                else if (int.TryParse(part, out int singleId))
                {
                    result.Add(singleId.ToString());
                }
            }
            return result;
        }
    }
}
