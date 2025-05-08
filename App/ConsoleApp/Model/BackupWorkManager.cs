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
        public List<string> Works { get; set; } = new List<string>();

        public string AddWork(string pathSource, string pathTarget)
        {
            if (Works.Count >= MaxWorks)
            {
                return "AddWorkError";
            }

            // Create JSON content
            var workData = new
            {
                Name = $"work_{Works.Count + 1}",
                SourceFilePath = pathSource,
                TargetFilePath = pathTarget,
                State = "ACTIVE",
                TotalFilesToCopy = 3300,
                TotalFilesSize = 1240312777,
                NbFilesLeftToDo = 3274,
                Progression = 0
            };

            string jsonContent = JsonSerializer.Serialize(workData, new JsonSerializerOptions { WriteIndented = true });

            // Save JSON file
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string jsonFilePath = Path.Combine(projectRootPath, "state.json");

            File.WriteAllText(jsonFilePath, jsonContent);

            Works.Add($"work_{Works.Count}");
            return "AddWorkSuccess";
        }

        public string RemoveWork(string work)
        {
            if (Works.Remove(work))
            {
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
                return string.Join(", ", Works);
            }
        }
    }
}
