using ConsoleApp.ViewModel;
using System.Globalization;
using System.Resources;
using System.Text.Json;
using System.IO;

namespace ConsoleApp.View
{
    public class ConsoleView
    {
        public void Display()
        {
            // Read language from config.json
            string projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
            string configFilePath = Path.Combine(projectRootPath, "config.json");
            string configContent = File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(configContent);
            string language = config.ContainsKey("language") ? config["language"] : "en";

            // Set ResourceManager culture based on language
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            ResourceManager resourceManager = new ResourceManager("ConsoleApp.Resources.Messages", typeof(ConsoleView).Assembly);

            var viewModel = new EasySafeViewModel();

            // Main loop to allow returning to options
            while (true)
            {
                // ## Paramètres application     
                Console.WriteLine(resourceManager.GetString("WelcomeMessage"));
                string WelcomeChoice = Console.ReadLine();
                switch (WelcomeChoice)
                {
                    case "1":
                        Console.WriteLine(resourceManager.GetString("LanguageSelection"));
                        string languageChoice = Console.ReadLine();
                        viewModel.ChooseLanguage(languageChoice);

                        // Update language and ResourceManager
                        configContent = File.ReadAllText(configFilePath);
                        config = JsonSerializer.Deserialize<Dictionary<string, string>>(configContent);
                        language = config.ContainsKey("language") ? config["language"] : "fr";

                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
                        resourceManager = new ResourceManager("ConsoleApp.Resources.Messages", typeof(ConsoleView).Assembly);
                        break;

                    case "2":
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
                            string currentPathSource = null;
                            string currentPathTarget = null;
                            string name = null;
                            string type = null;
                            string typeChoice = null;

                            if (choice == "3" || choice == "4")
                            {
                                Console.WriteLine(responses["EnterFileName"]);
                                name = Console.ReadLine();
                            }
                            if (choice == "2")
                            {
                                Console.WriteLine(resourceManager.GetString("RunBackupSaveName"));
                                name = Console.ReadLine();

                                Console.WriteLine(resourceManager.GetString("RunBackupSaveType"));
                                typeChoice = Console.ReadLine();
                                switch (typeChoice)
                                {
                                    case "1":
                                        type = "FULL";
                                        break;
                                    case "2":
                                        type = "DIFFERENTIAL";
                                        break;
                                    default:
                                        Console.WriteLine(resourceManager.GetString("RunBackupDefaultError"));
                                        break;
                                }

                                // Demander à l'utilisateur de choisir un dossier source
                                Console.WriteLine(resourceManager.GetString("AddWorkDisk"));
                                string sourceDriveLetter = Console.ReadLine()?.ToUpper() + @":\";

                                if (Directory.Exists(sourceDriveLetter))
                                {
                                    currentPathSource = sourceDriveLetter;

                                    while (true)
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine($"Contenu du dossier source : {currentPathSource}");
                                        var directories = Directory.GetDirectories(currentPathSource);
                                        var files = Directory.GetFiles(currentPathSource);

                                        Console.WriteLine(resourceManager.GetString("AddWorkFolders"));
                                        for (int i = 0; i < directories.Length; i++)
                                        {
                                            Console.WriteLine($"{i + 1}. {Path.GetFileName(directories[i])}");
                                        }

                                        Console.WriteLine(resourceManager.GetString("AddWorkFiles"));
                                        for (int i = 0; i < files.Length; i++)
                                        {
                                            Console.WriteLine($"- {Path.GetFileName(files[i])}");
                                        }

                                        Console.WriteLine(resourceManager.GetString("AddWorkSelect"));
                                        string input = Console.ReadLine();

                                        if (input == "s")
                                        {
                                            Console.WriteLine($"Dossier source sélectionné : {currentPathSource}");
                                            break;
                                        }
                                        else if (input == "q")
                                        {
                                            Console.WriteLine(resourceManager.GetString("AddWorkCancel"));
                                            break;
                                        }
                                        else if (int.TryParse(input, out int index) && index > 0 && index <= directories.Length)
                                        {
                                            currentPathSource = directories[index - 1];
                                        }
                                        else
                                        {
                                            Console.WriteLine(resourceManager.GetString("RunBackupDefaultError"));
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(resourceManager.GetString("AddWorkDiskError"));
                                }

                                // Demander à l'utilisateur de choisir un dossier cible
                                Console.WriteLine(resourceManager.GetString("AddWorkDisk"));
                                string targetDriveLetter = Console.ReadLine()?.ToUpper() + @":\";

                                if (Directory.Exists(targetDriveLetter))
                                {
                                    currentPathTarget = targetDriveLetter;

                                    while (true)
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine($"Contenu du dossier cible : {currentPathTarget}");
                                        var directories = Directory.GetDirectories(currentPathTarget);
                                        var files = Directory.GetFiles(currentPathTarget);

                                        Console.WriteLine(resourceManager.GetString("AddWorkFolders"));
                                        for (int i = 0; i < directories.Length; i++)
                                        {
                                            Console.WriteLine($"{i + 1}. {Path.GetFileName(directories[i])}");
                                        }

                                        Console.WriteLine(resourceManager.GetString("AddWorkFiles"));
                                        for (int i = 0; i < files.Length; i++)
                                        {
                                            Console.WriteLine($"- {Path.GetFileName(files[i])}");
                                        }

                                        Console.WriteLine(resourceManager.GetString("AddWorkSelect"));
                                        string input = Console.ReadLine();

                                        if (input == "s")
                                        {
                                            Console.WriteLine($"Dossier cible sélectionné : {currentPathTarget}");
                                            break;
                                        }
                                        else if (input == "q")
                                        {
                                            Console.WriteLine(resourceManager.GetString("AddWorkCancel"));
                                            break;
                                        }
                                        else if (int.TryParse(input, out int index) && index > 0 && index <= directories.Length)
                                        {
                                            currentPathTarget = directories[index - 1];
                                        }
                                        else
                                        {
                                            Console.WriteLine(resourceManager.GetString("RunBackupDefaultError"));
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(resourceManager.GetString("AddWorkDiskError"));
                                }
                            }
                            
                            
                            string data = viewModel.RunBackup(choice, name, currentPathSource, currentPathTarget, type);

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
                        break;

                    case "3":
                        Console.WriteLine(resourceManager.GetString("RunBackupExit"));
                        Environment.Exit(0); // Quitte l'application
                        break;

                    default:
                        Console.WriteLine(resourceManager.GetString("RunBackupDefaultError"));
                        break;
                }
            }
        }
    }
}
