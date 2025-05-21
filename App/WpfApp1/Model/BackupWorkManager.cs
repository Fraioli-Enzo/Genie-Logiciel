using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LoggingLibrary;
using System.IO.Packaging;
using System.Diagnostics;
using System.Windows;

namespace WpfApp1.Model
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public string WorkId { get; }
        public int Progression { get; }

        public ProgressChangedEventArgs(string workId, int progression)
        {
            WorkId = workId;
            Progression = progression;
        }
    }

    public class BackupWorkManager
    {

        public List<BackupWork> Works { get; set; } = new List<BackupWork>();
        public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
        private readonly ManualResetEventSlim _pauseEvent = new(true);

        // Ajoutez ces méthodes pour gérer la pause/reprise par ID
        public async Task PauseBackupAsync(string id)
        {
            await Task.Yield();
            var backup = Works.FirstOrDefault(w => w.ID == id);
            if (backup != null)
            {
                if (backup.ID == Works.FirstOrDefault(w => !w.IsPaused)?.ID)
                {
                    _pauseEvent.Reset();
                }
                backup.IsPaused = true;
            }
        }

        public async Task ResumeBackupAsync(string id)
        {
            await Task.Yield();
            var backup = Works.FirstOrDefault(w => w.ID == id);
            if (backup != null)
            {
                if (backup.ID == Works.FirstOrDefault(w => w.IsPaused)?.ID)
                {
                    _pauseEvent.Set();
                }
                backup.IsPaused = false;
            }
        }

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

        public static bool IsProcessRunning(string processName)
            {
                // Ne pas inclure l'extension .exe dans le nom du processus
                var processes = Process.GetProcessesByName(processName);
                return processes.Length > 0;
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

            // Vérifier si le fichier state.json existe
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string jsonFilePath = Path.Combine(projectRootPath, "state.json");

            List<BackupWork> existingWorks = new List<BackupWork>();

            if (File.Exists(jsonFilePath))
            { 
                string existingJsonContent = File.ReadAllText(jsonFilePath);
                existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();
            }
 
            existingWorks.Add(newWork);
            string updatedJsonContent = JsonSerializer.Serialize(existingWorks, new JsonSerializerOptions { WriteIndented = true }); 
            File.WriteAllText(jsonFilePath, updatedJsonContent);

            Works.Add(newWork);
            return "AddWorkSuccess";
        }

        public string RemoveWork(string ids)
        {
            var idsToRemove = ParseIds(ids);

            if (idsToRemove.Count == 0)
                return "RemoveWorkError";

            Works.RemoveAll(w => idsToRemove.Contains(w.ID));

            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")); 
            string jsonFilePath = Path.Combine(projectRootPath, "state.json");

            if (File.Exists(jsonFilePath))
            {
                string existingJsonContent = File.ReadAllText(jsonFilePath);
                var existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();

                existingWorks = existingWorks.Where(w => !idsToRemove.Contains(w.ID)).ToList();

                string updatedJsonContent = JsonSerializer.Serialize(existingWorks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonFilePath, updatedJsonContent);
            }

            return "RemoveWorkSuccess";
        }

        public async Task<string> ExecuteWorkAsync(string ids, string log, string[] extensions, string workingSoftware)
        {
            if (IsProcessRunning(workingSoftware) && workingSoftware != "null")
            {
                var workToExecute = Works.FirstOrDefault(w => w.ID == ids);
                string nameBackup = workToExecute?.Name;
                string sourcePath = workToExecute.SourcePath;
                string targetPath = workToExecute.TargetPath;
                LoggingLibrary.Logger.Log(nameBackup, "Error", "Error", 0, 0, log, 0);
                return "ProcessAlreadyRunning";
            }

            var idsToExecute = ParseIds(ids);
            if (idsToExecute.Count == 0)
                return "ExecuteWorkError";

            var results = new List<string>();
            foreach (var id in idsToExecute)
            {
                var result = await ExecuteSingleWorkAsync(id, log, extensions);
                results.Add(result);
            }

            if (results.All(r => r == "ExecuteWorkSuccess"))
                return "ExecuteWorkSuccess";
            if (results.Any(r => r == "SourceDirectoryNotFound"))
                return "SourceDirectoryNotFound";
            var firstError = results.FirstOrDefault(r => r != "ExecuteWorkSuccess");
            return firstError ?? "ExecuteWorkError";
        }

        private async Task<string> ExecuteSingleWorkAsync(string id, string log, string[] extensions)
        {
            var workToExecute = Works.FirstOrDefault(w => w.ID == id);
            string nameBackup = workToExecute?.Name;
            if (workToExecute != null)
            {
                string sourcePath = workToExecute.SourcePath;
                string targetPath = workToExecute.TargetPath;

                if (!Directory.Exists(sourcePath))
                {
                    return "SourceDirectoryNotFound";
                }

                var allFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

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
                        await Task.Run(() => _pauseEvent.Wait());
                        await Task.Delay(300); // Simule un délai sans bloquer l'UI
                        string relativePath = Path.GetRelativePath(sourcePath, file);
                        string targetFilePath = Path.Combine(targetPath, relativePath);

                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);

                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        File.Copy(file, targetFilePath, overwrite: true);

                        // Chiffrement du fichier si l'extension correspond
                        int encryptionTime = 0;
                        string fileExtension = Path.GetExtension(targetFilePath);
                        if (extensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                        {
                            encryptionTime = CryptoSoft.RunEncryption(targetFilePath, "ProtectedKey");
                            if (encryptionTime == -99)
                            {
                                return "EncryptionError";
                            }
                        }

                        stopwatch.Stop();
                        double fileTransferTime = stopwatch.Elapsed.TotalMilliseconds;

                        long fileSize = new FileInfo(file).Length;
                        LoggingLibrary.Logger.Log(nameBackup, file, targetFilePath, fileSize, fileTransferTime, log, encryptionTime);

                        filesCopied++;

                        workToExecute.NbFilesLeftToDo = (totalFiles - filesCopied).ToString();
                        workToExecute.Progression = (totalFiles == 0 ? "100" : ((filesCopied * 100) / totalFiles).ToString());
                        ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(workToExecute.ID, int.Parse(workToExecute.Progression)));
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
