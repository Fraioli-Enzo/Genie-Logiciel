using System.IO;
using System.Text.Json;
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
        //----------------------------GENERAL----------------------------------------
        public List<BackupWork> Works { get; set; } = new List<BackupWork>();
        public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
        private static readonly SemaphoreSlim LargeFileCopyLock = new(1, 1);
        private static readonly SemaphoreSlim EncryptionLock = new(1, 1);


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

        //--------------------------ACTIONS FUNCTIONS------------------------------------------
        public string AddWork(string name, string pathSource, string pathTarget, string type)
        {

            // Calculer le nombre total de fichiers et leur taille totale
            var allFiles = Directory.GetFiles(pathSource, "*", SearchOption.AllDirectories);
            int totalFilesToCopy = allFiles.Length;
            long totalFilesSize = allFiles.Sum(file => new FileInfo(file).Length);

            string id;
            if (Works.Count == 0)
            {
                id = "1";
            }
            else
            {
                var existingIds = Works.Select(w => int.TryParse(w.ID, out var n) ? n : 0).Where(n => n > 0).ToList();
                int newId = 1;
                while (existingIds.Contains(newId))
                {
                    newId++;
                }
                id = newId.ToString();
            }

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

        public string EditWork(string id, string name, string pathSource, string pathTarget, string type)
        {
            var workToEdit = Works.FirstOrDefault(w => w.ID == id);
            if (workToEdit == null)
                return "EditWorkError";
            // Mettre à jour les propriétés de l'objet BackupWork
            workToEdit.Name = name;
            workToEdit.SourcePath = pathSource;
            workToEdit.TargetPath = pathTarget;
            workToEdit.Type = type;

            // Recalculer le nombre total de fichiers et leur taille totale
            var allFiles = Directory.GetFiles(pathSource, "*", SearchOption.AllDirectories);
            int totalFilesToCopy = allFiles.Length;
            long totalFilesSize = allFiles.Sum(file => new FileInfo(file).Length);
            workToEdit.TotalFilesToCopy = totalFilesToCopy.ToString();
            workToEdit.TotalFilesSize = totalFilesSize.ToString();
            workToEdit.NbFilesLeftToDo = totalFilesToCopy.ToString();
            workToEdit.Progression = "0";
            // Mettre à jour le fichier state.json
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string jsonFilePath = Path.Combine(projectRootPath, "state.json");
            if (File.Exists(jsonFilePath))
            {
                string existingJsonContent = File.ReadAllText(jsonFilePath);
                var existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();
                var workIndex = existingWorks.FindIndex(w => w.ID == id);
                if (workIndex != -1)
                {
                    existingWorks[workIndex] = workToEdit;
                }
                string updatedJsonContent = JsonSerializer.Serialize(existingWorks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonFilePath, updatedJsonContent);
            }
            return "EditWorkSuccess";
        }
        public string RemoveWork(string id)
        {
            Works.RemoveAll(w => id.Contains(w.ID));

            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string jsonFilePath = Path.Combine(projectRootPath, "state.json");

            if (File.Exists(jsonFilePath))
            {
                string existingJsonContent = File.ReadAllText(jsonFilePath);
                var existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();

                existingWorks = existingWorks.Where(w => !id.Contains(w.ID)).ToList();

                string updatedJsonContent = JsonSerializer.Serialize(existingWorks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonFilePath, updatedJsonContent);
            }

            return "RemoveWorkSuccess";
        }

        public string PauseWork(string id)
        {
            var workToPause = Works.FirstOrDefault(w => w.ID == id);
            if (workToPause != null)
            {
                workToPause.IsPaused = !(workToPause.IsPaused);
                return "PauseWorkSuccess";
            }
            return "PauseWorkError";
        }
        public string StopWork(string id)
        {
            var workToStop = Works.FirstOrDefault(w => w.ID == id);
            if (workToStop != null)
            {
                workToStop.IsStopped = true;
                workToStop.IsPaused = false;
                return "StopWorkSuccess";
            }
            return "StopWorkError";
        }

        public async Task<string> ExecuteWorkAsync(string id, string log, string[] extensions, string workingSoftware, string maxKo, string[] extensionsPrio)
        {
            var workToExecute = Works.FirstOrDefault(w => w.ID == id);
            workToExecute.State = "ACTIVE";
            if (_IsProcessRunning(workingSoftware) && workingSoftware != "null")
            {
                LoggingLibrary.Logger.Log(workToExecute.Name, workToExecute.SourcePath, workToExecute.TargetPath, 0, 0, log, 0, "PROCESS_RUNNING");
                return "ProcessRunning";
            }

            if (workToExecute == null)
                return "ExecuteWorkError";

            if (!Directory.Exists(workToExecute.SourcePath))
                return "ExecuteWorkError";

            var filesToCopy = _GetFilesToCopy(workToExecute);
            if (filesToCopy == null)
                return "ExecuteWorkError";

            filesToCopy = _OrderFilesByExtensionPriority(filesToCopy, extensionsPrio);

            _UpdateWorkFileStats(workToExecute, filesToCopy);

            _UpdateWorkStateInJson(workToExecute);

            Directory.CreateDirectory(workToExecute.TargetPath);

            // Ajout de la surveillance du logiciel métier pendant la sauvegarde
            CancellationTokenSource cts = new();
            Task monitoringTask = Task.CompletedTask;
            if (!string.IsNullOrEmpty(workingSoftware) && workingSoftware != "null")
            {
                monitoringTask = MonitorBusinessSoftwareAsync(workToExecute, workingSoftware, cts.Token);
            }

            try
            {
                var result = await _CopyAndEncryptFilesAsync(workToExecute, filesToCopy, log, extensions, maxKo, cts.Token);
                _UpdateWorkStateInJson(workToExecute);
                workToExecute.State = "INACTIVE";
                cts.Cancel();
                await monitoringTask;
                return result;
            }
            catch (Exception)
            {
                workToExecute.State = "INACTIVE";
                cts.Cancel();
                await monitoringTask;
                return "ExecuteWorkError";
            }

            List<string> _OrderFilesByExtensionPriority(List<string> files, string[] extensionsPrio)
            {
                return files
                    .OrderBy(f =>
                    {
                        var ext = Path.GetExtension(f);
                        int idx = Array.FindIndex(extensionsPrio, e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase));
                        return idx == -1 ? int.MaxValue : idx;
                    })
                    .ToList();
            }

            // Surveillance du logiciel métier pendant la sauvegarde
            async Task MonitorBusinessSoftwareAsync(BackupWork work, string processName, CancellationToken token)
            {
                try
                {
                    bool wasRunning = false;
                    while (!token.IsCancellationRequested && !work.IsStopped)
                    {
                        // Si le logiciel est lancé, on met la sauvegarde en pause
                        bool isRunning = _IsProcessRunning(processName);
                        if (isRunning && !work.IsPaused)
                        {
                            work.IsPaused = true;
                            wasRunning = true;
                        }
                        // Si le logiciel est arrêté, on reprend la sauvegarde
                        else if (!isRunning && work.IsPaused && wasRunning)
                        {
                            work.IsPaused = false;
                            wasRunning = false;
                        }
                        await Task.Delay(1000, token);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Annulation attendue, on ne fait rien
                }
            }

            bool _IsProcessRunning(string processName)
            {
                if (string.IsNullOrEmpty(processName) || processName == "null")
                    return false;
                var processes = Process.GetProcessesByName(processName);
                return processes.Length > 0;
            }

            List<string>? _GetFilesToCopy(BackupWork work)
            {
                var allFiles = Directory.GetFiles(work.SourcePath, "*", SearchOption.AllDirectories);

                if (work.Type == "FULL")
                {
                    return allFiles.ToList();
                }
                else if (work.Type == "DIFFERENTIAL")
                {
                    var filesToCopy = new List<string>();
                    foreach (var file in allFiles)
                    {
                        string relativePath = Path.GetRelativePath(work.SourcePath, file);
                        string targetFilePath = Path.Combine(work.TargetPath, relativePath);
                        if (!File.Exists(targetFilePath))
                        {
                            filesToCopy.Add(file);
                        }
                    }
                    return filesToCopy;
                }
                return null;
            }

            void _UpdateWorkFileStats(BackupWork work, List<string> filesToCopy)
            {
                int totalFiles = filesToCopy.Count;
                long totalSize = filesToCopy.Sum(file => new FileInfo(file).Length);
                work.TotalFilesToCopy = totalFiles.ToString();
                work.TotalFilesSize = totalSize.ToString();
                work.NbFilesLeftToDo = totalFiles.ToString();
                work.Progression = "0";
            }

            void _UpdateWorkStateInJson(BackupWork work)
            {
                string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
                string jsonFilePath = Path.Combine(projectRootPath, "state.json");

                if (File.Exists(jsonFilePath))
                {
                    string existingJsonContent = File.ReadAllText(jsonFilePath);
                    var existingWorks = JsonSerializer.Deserialize<List<BackupWork>>(existingJsonContent) ?? new List<BackupWork>();

                    var workIndex = existingWorks.FindIndex(w => w.ID == work.ID);
                    if (workIndex != -1)
                    {
                        existingWorks[workIndex].TotalFilesToCopy = work.TotalFilesToCopy;
                        existingWorks[workIndex].TotalFilesSize = work.TotalFilesSize;
                        existingWorks[workIndex].NbFilesLeftToDo = work.NbFilesLeftToDo;
                        existingWorks[workIndex].Progression = work.Progression;
                    }

                    string updatedJsonContent = JsonSerializer.Serialize(existingWorks, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(jsonFilePath, updatedJsonContent);
                }
            }

            async Task<string> _CopyAndEncryptFilesAsync(BackupWork work, List<string> filesToCopy, string log, string[] extensions, string maxKo, CancellationToken token)
            {
                int totalFiles = filesToCopy.Count;
                int filesCopied = 0;

                for (int i = 0; i < filesToCopy.Count; i++)
                {
                    // Pause manuelle ou automatique
                    while (work.IsPaused)
                    {
                        await Task.Delay(200, token);
                    }

                    // Stop
                    if (work.IsStopped)
                    {
                        __DeleteFilesInTargetPath(work);
                        LoggingLibrary.Logger.Log(work.Name, work.SourcePath, work.TargetPath, 0, 0, log, 0, "STOP");
                        return "ExecuteWorkStopped";
                    }

                    var file = filesToCopy[i];
                    long fileSize = new FileInfo(file).Length;
                    await Task.Delay(300, token); // Simule un délai sans bloquer l'UI
                    string relativePath = Path.GetRelativePath(work.SourcePath, file);
                    string targetFilePath = Path.Combine(work.TargetPath, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);

                    var stopwatch = Stopwatch.StartNew();

                    if (Math.Ceiling(fileSize / 1024.0) >= long.Parse(maxKo))
                    {
                        LoggingLibrary.Logger.Log(work.Name, file, targetFilePath, fileSize, 0, log, 0, "WAIT_LOCK_SIZE");
                        await LargeFileCopyLock.WaitAsync(token);
                        try
                        {
                            LoggingLibrary.Logger.Log(work.Name, file, targetFilePath, fileSize, 0, log, 0, "ENTER_LOCK_SIZE");
                            File.Copy(file, targetFilePath, overwrite: true);
                        }
                        finally
                        {
                            LoggingLibrary.Logger.Log(work.Name, file, targetFilePath, fileSize, 0, log, 0, "RELEASE_LOCK_SIZE");
                            LargeFileCopyLock.Release();
                        }
                    }
                    else
                    {
                        File.Copy(file, targetFilePath, overwrite: true);
                    }

                    // Chiffrement du fichier si l'extension correspond
                    int encryptionTime = 0;
                    string fileExtension = Path.GetExtension(targetFilePath);
                    if (extensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                    {
                        LoggingLibrary.Logger.Log(work.Name, file, targetFilePath, fileSize, 0, log, 0, "WAIT_LOCK_ENCRYPTION");
                        await EncryptionLock.WaitAsync(token);
                        try
                        {
                            LoggingLibrary.Logger.Log(work.Name, file, targetFilePath, fileSize, 0, log, 0, "ENTER_LOCK_ENCRYPTION");
                            encryptionTime = CryptoSoft.RunEncryption(targetFilePath, "ProtectedKey");
                            if (encryptionTime == -99)
                            {
                                return "ExecuteWorkEncryptionError";
                            }
                        }
                        finally
                        {
                            LoggingLibrary.Logger.Log(work.Name, file, targetFilePath, fileSize, 0, log, 0, "RELEASE_LOCK_ENCRYPTION");
                            EncryptionLock.Release();
                        }
                    }

                    stopwatch.Stop();
                    double fileTransferTime = stopwatch.Elapsed.TotalMilliseconds;

                    LoggingLibrary.Logger.Log(work.Name, file, targetFilePath, fileSize, fileTransferTime, log, encryptionTime, "_");

                    filesCopied++;

                    work.NbFilesLeftToDo = (totalFiles - filesCopied).ToString();
                    work.Progression = (totalFiles == 0 ? "100" : ((filesCopied * 100) / totalFiles).ToString());
                    ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(work.ID, int.Parse(work.Progression)));
                }

                return "ExecuteWorkSuccess";

                // Inner function to delete files in the target path
                void __DeleteFilesInTargetPath(BackupWork work)
                {
                    if (Directory.Exists(work.TargetPath))
                    {
                        var files = Directory.GetFiles(work.TargetPath, "*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            File.Delete(file);
                        }
                    }
                    work.IsStopped = false;
                    work.Progression = "0";
                    ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(work.ID, int.Parse(work.Progression)));
                    work.NbFilesLeftToDo = work.TotalFilesToCopy;
                }
            }
        }
    }
}
