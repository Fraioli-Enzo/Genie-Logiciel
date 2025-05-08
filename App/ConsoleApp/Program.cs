using ConsoleApp.View;
using ConsoleApp.ViewModel;

namespace ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var viewModel = new PersonViewModel();
            var view = new ConsoleView(viewModel);
            view.Run();
        }
    }
}