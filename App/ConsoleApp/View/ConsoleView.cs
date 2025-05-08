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
            // Dictionnaire pour afficher les messages en fonction de la langue et de l'action
            var responses = new Dictionary<string, string>
            {
                { "RunBackupExit", resourceManager.GetString("RunBackupExit") },
                { "RunBackupDefaultError", resourceManager.GetString("RunBackupDefaultError") },
                { "AddWorkError", resourceManager.GetString("AddWorkError") },
                { "AddWorkSuccess", resourceManager.GetString("AddWorkSuccess") },
                { "RemoveWorkSuccess", resourceManager.GetString("RemoveWorkSuccess") },
                { "RemoveWorkError", resourceManager.GetString("RemoveWorkError") },
                { "DisplayWorksError", resourceManager.GetString("DisplayWorksError") },
                { "EnterFileName", resourceManager.GetString("EnterFileName") },
            };

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine(resourceManager.GetString("RunBackupWelcomeMessage"));
                string choice = Console.ReadLine();
                if (choice == "2" || choice == "3")
                {
                    Console.WriteLine(responses["EnterFileName"]);
                }
                string data = viewModel.RunBackup(choice);

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
