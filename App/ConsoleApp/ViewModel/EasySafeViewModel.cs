using ConsoleApp.View;
using System.Globalization;
using System.Resources;
using ConsoleApp.Model;


namespace ConsoleApp.ViewModel
{
    public class EasySafeViewModel
    {
        public void ChooseLanguage()
        {
            var language = Console.ReadLine();

            // Définir la langue en fonction de l'entrée utilisateur / si erreur utiliser français par défaut 
            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            }
            catch (CultureNotFoundException)
            {
                Console.WriteLine("Langue non reconnue, utilisation de la langue par défaut (fr).");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr");
            }
        }

        public string RunBackup()
        {
            var workManager = new BackupWorkManager();

            var choice = Console.ReadLine();
            string workName;

            switch (choice)
            {
                case "1":
                    string displayWork = workManager.DisplayWorks();
                    return displayWork;

                case "2":
                    workName = Console.ReadLine();
                    string addWork = workManager.AddWork(workName);
                    return addWork;

                case "3":
                    workName = Console.ReadLine();
                    string removeWork = workManager.RemoveWork(workName);
                    return removeWork;

                case "4":
                    return "RunBackupExit";

                default:
                    return "RunBackupDefaultError";
            }
            
        }
    }
}
