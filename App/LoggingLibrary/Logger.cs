using System;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using System.Linq;
using System.Xml;
using System.Text;

namespace LoggingLibrary
{
    public class Logger
    {
        // Classe interne pour la sérialisation
        public class LogEntry
        {
            public string Name { get; set; }
            public string FileSource { get; set; }
            public string FileTarget { get; set; }
            public long FileSize { get; set; }
            public double FileTransferTime { get; set; }
            public string Time { get; set; }
            public int EncryptionTime { get; set; }
            public string Message { get; set; }
        }

        public static void Log(string id, string fileSource, string fileTarget, long fileSize, double fileTransferTime, string log, int encryptionTime, string message)
        {
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\")); 
            string logsDirectoryPath = Path.Combine(projectRootPath, "Logs");
            Directory.CreateDirectory(logsDirectoryPath);

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string extension = log.ToLower() == "xml" ? "xml" : "json";
            string logFilePath = Path.Combine(logsDirectoryPath, $"{today}.{extension}");

            var logEntry = new LogEntry
            {
                Name = id,
                FileSource = fileSource,
                FileTarget = fileTarget,
                FileSize = fileSize,
                FileTransferTime = fileTransferTime,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                EncryptionTime = encryptionTime,
                Message = message
            };

            if (log.ToLower() == "xml")
            {
                // Prepare the entry XML
                var entrySb = new StringBuilder();
                entrySb.AppendLine("  <Entry>");
                entrySb.AppendLine($"    <Name>{System.Security.SecurityElement.Escape(logEntry.Name)}</Name>");
                entrySb.AppendLine($"    <FileSource>{System.Security.SecurityElement.Escape(logEntry.FileSource)}</FileSource>");
                entrySb.AppendLine($"    <FileTarget>{System.Security.SecurityElement.Escape(logEntry.FileTarget)}</FileTarget>");
                entrySb.AppendLine($"    <FileSize>{logEntry.FileSize}</FileSize>");
                entrySb.AppendLine($"    <FileTransferTime>{logEntry.FileTransferTime}</FileTransferTime>");
                entrySb.AppendLine($"    <Time>{System.Security.SecurityElement.Escape(logEntry.Time)}</Time>");
                entrySb.AppendLine($"    <EncryptionTime>{logEntry.EncryptionTime}</EncryptionTime>");
                entrySb.AppendLine($"    <Message>{logEntry.Message}</Message>");
                entrySb.AppendLine("  </Entry>");
                string entryXml = entrySb.ToString();

                if (!File.Exists(logFilePath))
                {
                    // Create new XML file with declaration, root, and first entry
                    var sb = new StringBuilder();
                    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sb.AppendLine("<LogEntry>");
                    sb.Append(entryXml);
                    sb.AppendLine("</LogEntry>");
                    File.WriteAllText(logFilePath, sb.ToString(), Encoding.UTF8);
                }
                else
                {
                    // Append new entry before the closing </LogEntry>
                    string xml = File.ReadAllText(logFilePath, Encoding.UTF8);
                    int insertPos = xml.LastIndexOf("</LogEntry>");
                    if (insertPos != -1)
                    {
                        xml = xml.Insert(insertPos, entryXml);
                        File.WriteAllText(logFilePath, xml, Encoding.UTF8);
                    }
                    // else: fallback, do nothing or recreate file (optional)
                }
            }
            else // Par défaut JSON
            {
                // Lire le contenu existant ou initialiser une liste
                List<LogEntry> entries;
                if (File.Exists(logFilePath))
                {
                    string existing = File.ReadAllText(logFilePath);
                    if (!string.IsNullOrWhiteSpace(existing))
                    {
                        try
                        {
                            entries = JsonSerializer.Deserialize<List<LogEntry>>(existing) ?? new List<LogEntry>();
                        }
                        catch
                        {
                            // Si le fichier n'est pas un tableau JSON, on réinitialise
                            entries = new List<LogEntry>();
                        }
                    }
                    else
                    {
                        entries = new List<LogEntry>();
                    }
                }
                else
                {
                    entries = new List<LogEntry>();
                }

                // Ajouter la nouvelle entrée
                entries.Add(logEntry);

                // Sérialiser et écraser le fichier
                string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(logFilePath, json);
            }
        }
    }
}