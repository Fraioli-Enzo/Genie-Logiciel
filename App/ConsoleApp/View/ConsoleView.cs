using ConsoleApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            while (true)
            {
                Console.WriteLine("Entrez un nom (ou tapez 'exit' pour quitter) :");
                var input = Console.ReadLine();

                if (input?.ToLower() == "exit")
                    break;

                _viewModel.Name = input;
                Console.WriteLine($"Nom mis à jour : {_viewModel.Name}");
            }
        }
    }
}
