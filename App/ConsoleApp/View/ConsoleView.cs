using ConsoleApp.ViewModel;
using System.Globalization;
using System.Resources;

namespace ConsoleApp.View
{
    public class ConsoleView
    {
        public void Display()
        {
            var viewModel = new EasySafeViewModel();

            // ## Choisir la langue
            Console.WriteLine("Choisissez une langue (fr/en) :");
            viewModel.ChooseLanguage();

            // Charger les différents langages 
            ResourceManager resourceManager = new ResourceManager("ConsoleApp.Resources.Messages", typeof(ConsoleView).Assembly);
            Console.WriteLine(resourceManager.GetString("WelcomeMessage"));


            // ## Début de la sauvegarde
            var responses = new Dictionary<string, string>
            {
                { "RunBackupExit", resourceManager.GetString("RunBackupExit") },
                { "RunBackupDefaultError", resourceManager.GetString("RunBackupDefaultError") },
                { "AddWorkError", resourceManager.GetString("AddWorkError") },
                { "AddWorkSucess", resourceManager.GetString("AddWorkSucess") },
                { "RemoveWorkSucess", resourceManager.GetString("RemoveWorkSucess") },
                { "DisplayWorksError", resourceManager.GetString("DisplayWorksError") },
            };

            while (true)
            {
                Console.WriteLine(resourceManager.GetString("RunBackupWelcomeMessage"));
                string data = viewModel.RunBackup();

                if (responses.ContainsKey(data))
                {
                    Console.WriteLine(responses[data]);
                    if (data == "RunBackupExit") break;
                }
                else
                {
                    Console.WriteLine(data);
                }
            }
        }
    }
}
