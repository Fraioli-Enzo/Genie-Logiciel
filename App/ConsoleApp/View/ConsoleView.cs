using ConsoleApp.ViewModel;
using System.Globalization;
using System.Resources;

namespace ConsoleApp.View
{
    public class ConsoleView
    {
        private readonly PersonViewModel _viewModel;

        public ConsoleView(PersonViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Run()
        {
            // Demander à l'utilisateur de choisir une langue  
            Console.WriteLine("Choisissez une langue (fr/en) :");
            var language = Console.ReadLine();

            // Définir la culture en fonction de l'entrée utilisateur  
            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            }
            catch (CultureNotFoundException)
            {
                Console.WriteLine("Langue non reconnue, utilisation de la langue par défaut (fr).");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr");
            }

            // Charger les ressources localisées  
            ResourceManager resourceManager = new ResourceManager("ConsoleApp.Resources.Messages", typeof(ConsoleView).Assembly);

            // Afficher un message localisé  
            Console.WriteLine(resourceManager.GetString("WelcomeMessage"));

            while (true)
            {
                Console.WriteLine(resourceManager.GetString("EnterName"));
                var input = Console.ReadLine();

                if (input?.ToLower() == "exit")
                {
                    Console.WriteLine(resourceManager.GetString("ExitMessage"));
                    break;
                }

                _viewModel.Name = input;
                Console.WriteLine($"{resourceManager.GetString("UpdatedName")} {_viewModel.Name}");
            }
        }
    }
}
