using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WpfBackupClient
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
        public bool IsPaused { get; set; }

        private string _progression;
        public string Progression
        {
            get => _progression;
            set
            {
                if (_progression != value)
                {
                    _progression = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progression)));
                }
            }
        }

        private bool _isStopped;
        public bool IsStopped
        {
            get => _isStopped;
            set
            {
                if (_isStopped != value)
                {
                    _isStopped = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStopped)));
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

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
            _progression = progression;
        }
    }
}