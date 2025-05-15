using System;
using System.IO;
using System.Text.Json;

namespace LoggingLibrary
{
    public class Logger
    {
        public static void Log(string id, string fileSource, string fileTarget, long fileSize, double fileTransferTime)
        {
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string logsDirectoryPath = Path.Combine(projectRootPath, "Logs");
            Directory.CreateDirectory(logsDirectoryPath);

            string logFilePath = Path.Combine(logsDirectoryPath, $"{id}_log.json");

            var logEntry = new
            {
                Name = id,
                FileSource = fileSource,
                FileTarget = fileTarget,
                FileSize = fileSize,
                FileTransferTime = fileTransferTime,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            string logEntryJson = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });
            File.AppendAllText(logFilePath, logEntryJson + Environment.NewLine);
        }
    }
}