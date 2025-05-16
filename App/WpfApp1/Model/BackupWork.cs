using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Model
{
    public class BackupWork
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public string TotalFilesToCopy { get; set; }
        public string TotalFilesSize { get; set; }
        public string NbFilesLeftToDo { get; set; }
        public string Progression { get; set; }

        public BackupWork(string id, string name, string sourcePath, string targetPath, string type, string totalFilesToCopy, string totalFilesSize, string nbFilesLeftToDo, string state = "INACTIVE", string progression = "0")
        {
            ID = id;
            Name = name;
            SourcePath = sourcePath;
            TargetPath = targetPath;
            Type = type;
            State = state;
            TotalFilesToCopy = totalFilesToCopy;
            TotalFilesSize = totalFilesSize;
            NbFilesLeftToDo = nbFilesLeftToDo;
            Progression = progression;
        }
    }
}