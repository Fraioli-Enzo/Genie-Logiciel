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
        }

        public static void Log(string id, string fileSource, string fileTarget, long fileSize, double fileTransferTime, string log)
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
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            if (log.ToLower() == "xml")
            {
                var xmlSerializer = new XmlSerializer(typeof(LogEntry));
                var xmlSettings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true, // No XML declaration
                    Indent = true,
                    Encoding = Encoding.UTF8
                };

                // Serialize the log entry without a root element
                string logEntryXml;
                using (var stringWriter = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(stringWriter, xmlSettings))
                {
                    xmlSerializer.Serialize(xmlWriter, logEntry);
                    logEntryXml = stringWriter.ToString();
                }

                // Remove the root element tags from the serialized XML
                int startIndex = logEntryXml.IndexOf(">") + 1;
                int endIndex = logEntryXml.LastIndexOf("</");
                logEntryXml = logEntryXml.Substring(startIndex, endIndex - startIndex).Trim();

                // Append the log entry XML to the file
                File.AppendAllText(logFilePath, logEntryXml + Environment.NewLine, Encoding.UTF8);
            }
            else // Par défaut JSON
            {
                string logEntryJson = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });
                File.AppendAllText(logFilePath, logEntryJson + Environment.NewLine);
            }
        }
    }
}