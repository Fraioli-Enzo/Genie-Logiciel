using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Model
{
    public class BackupWorkManager
    {
        private const int MaxWorks = 5;
        public List<string> Works { get; set; } = new List<string>();

        public string AddWork(string work)
        {
            if (Works.Count >= MaxWorks)
            {
                return "AddWorkError";
            }

            Works.Add(work);
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
