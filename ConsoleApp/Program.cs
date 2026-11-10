using ConsoleApp;
using DAL.Json;
using Game;
using MenuSystem;

var basePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
    "ConnectFourData"
);
var configFolder = Path.Combine(basePath, "configs");
var gamesFolder = Path.Combine(basePath, "games");
IConfigRepository configRepository = new JsonConfigRepository(configFolder);
IGameRepository gameRepository = new JsonGameRepository(gamesFolder);

EnsureDefaultPresets(configRepository);
var mainMenu = BuildMainMenu(configRepository, gameRepository);

mainMenu.Run();
static void EnsureDefaultPresets(IConfigRepository configRepo)
{
    var existingConfigs = configRepo.List();
    if (existingConfigs.Count == 0)
    {
        foreach (var preset in GameConfiguration.Presets)
        {
            configRepo.Save(preset.Name, preset);
        }
    }
}

static Menu BuildMainMenu(IConfigRepository configRepo, IGameRepository gameRepo)
{
    return new Menu("Connect Four - Main Menu", EMenuLevel.MainMenu)
    {
        Items = new List<MenuItem>
        {
            new MenuItem
            {
                Title = "New Game",
                Shortcut = "1",
                MethodToRun = () => 
                {
                    var configNames = configRepo.List();
                    if (configNames.Count == 0)
                    {
                        Console.WriteLine("No configurations available!");
                        return "";
                    }

                    Console.WriteLine("\nSelect Configuration:");
                    for (int i = 0; i < configNames.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {configNames[i]}");
                    }

                    Console.Write("Choice: ");
                    if (int.TryParse(Console.ReadLine(), out var choice) && choice >= 1 && choice <= configNames.Count)
                    {
                        var selectedConfigName = configNames[choice - 1];
                        var config = configRepo.Get(selectedConfigName);

                        var gameBrain = new GameBrain(config);
                        var controller = new GameController(gameRepo);

                        controller.RunGame(gameBrain, Guid.NewGuid(), $"Game_{DateTime.Now:yyyyMMdd_HHmmss}");
                    }
                    return "";
                }
            },
            new MenuItem
            {
                Title = "Load Saved Game",
                Shortcut = "2",
                MethodToRun = () => 
                {
                    var savedGames = gameRepo.List();
                    if (savedGames.Count == 0)
                    {
                        Console.WriteLine("\nNo saved games found.");
                        return "";
                    }

                    Console.WriteLine("\nSaved Games:");
                    for (int i = 0; i < savedGames.Count; i++)
                    {
                        var g = savedGames[i];
                        Console.WriteLine($"{i + 1}. {g.Name} (Saved: {g.SavedAt.ToLocalTime():yyyy-MM-dd HH:mm})");
                    }

                    Console.Write("Select game to load: ");
                    if (int.TryParse(Console.ReadLine(), out var choice) && choice >= 1 && choice <= savedGames.Count)
                    {
                        var selectedSave = savedGames[choice - 1];
                        var (state, name, _) = gameRepo.Get(selectedSave.Id);

                        var gameBrain = new GameBrain(state);
                        var controller = new GameController(gameRepo);

                        controller.RunGame(gameBrain, selectedSave.Id, name);
                    }
                    return "";
                }
            },
            new MenuItem
            {
                Title = "Manage Configurations",
                Shortcut = "3",
                MethodToRun = () => BuildConfigurationMenu(configRepo).Run()
            },
            new MenuItem
            {
                Title = "Delete Saved Game",
                Shortcut = "4",
                MethodToRun = () =>
                {
                    DeleteSavedGame(gameRepo);
                    return "";
                }
            }
        }
    };
}

static Menu BuildConfigurationMenu(IConfigRepository configRepo)
{
    return new Menu("Configuration Management", EMenuLevel.SubMenu)
    {
        Items = new List<MenuItem>
        {
            new MenuItem
            {
                Title = "List Configurations",
                Shortcut = "1",
                MethodToRun = () =>
                {
                    ListConfigurations(configRepo);
                    return "";
                }
            },
            new MenuItem
            {
                Title = "Create Configuration",
                Shortcut = "2",
                MethodToRun = () =>
                {
                    CreateConfiguration(configRepo);
                    return "";
                }
            },
            new MenuItem
            {
                Title = "Delete Configuration",
                Shortcut = "3",
                MethodToRun = () =>
                {
                    DeleteConfiguration(configRepo);
                    return "";
                }
            }
        }
    };
}

static void ListConfigurations(IConfigRepository configRepo)
{
    var configs = configRepo.List();
    Console.WriteLine("\nConfigurations:");
    if (configs.Count == 0)
    {
        Console.WriteLine("No configurations found.");
        return;
    }

    foreach (var config in configs)
    {
        Console.WriteLine($"- {config}");
    }
}

static void CreateConfiguration(IConfigRepository configRepo)
{
    Console.Write("\nConfiguration name: ");
    var name = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Configuration name cannot be empty.");
        return;
    }

    if (!ReadPositiveInteger("Board width: ", out var width) ||
        !ReadPositiveInteger("Board height: ", out var height) ||
        !ReadPositiveInteger("Win condition: ", out var winCondition))
    {
        Console.WriteLine("Board dimensions and win condition must be positive integers.");
        return;
    }

    if (winCondition > Math.Max(width, height))
    {
        Console.WriteLine("Win condition cannot exceed the board dimensions.");
        return;
    }

    Console.Write("Cylinder board? (Y/N): ");
    var isCylinder = Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true;
    configRepo.Save(name, new GameConfiguration(name, width, height, winCondition, isCylinder));
    Console.WriteLine($"Configuration '{name}' created.");
}

static void DeleteConfiguration(IConfigRepository configRepo)
{
    var configs = configRepo.List();
    if (configs.Count == 0)
    {
        Console.WriteLine("\nNo configurations found.");
        return;
    }

    Console.WriteLine("\nSelect configuration to delete:");
    for (var i = 0; i < configs.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {configs[i]}");
    }

    Console.Write("Choice: ");
    if (!int.TryParse(Console.ReadLine(), out var choice) || choice < 1 || choice > configs.Count)
    {
        Console.WriteLine("Invalid choice.");
        return;
    }

    var name = configs[choice - 1];
    Console.Write($"Delete '{name}'? (Y/N): ");
    if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
    {
        configRepo.Delete(name);
        Console.WriteLine($"Configuration '{name}' deleted.");
    }
}

static void DeleteSavedGame(IGameRepository gameRepo)
{
    var savedGames = gameRepo.List();
    if (savedGames.Count == 0)
    {
        Console.WriteLine("\nNo saved games found.");
        return;
    }

    Console.WriteLine("\nSelect saved game to delete:");
    for (var i = 0; i < savedGames.Count; i++)
    {
        var game = savedGames[i];
        Console.WriteLine($"{i + 1}. {game.Name} (Saved: {game.SavedAt.ToLocalTime():yyyy-MM-dd HH:mm})");
    }

    Console.Write("Choice: ");
    if (!int.TryParse(Console.ReadLine(), out var choice) || choice < 1 || choice > savedGames.Count)
    {
        Console.WriteLine("Invalid choice.");
        return;
    }

    var selectedGame = savedGames[choice - 1];
    Console.Write($"Delete '{selectedGame.Name}'? (Y/N): ");
    if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
    {
        gameRepo.Delete(selectedGame.Id);
        Console.WriteLine($"Saved game '{selectedGame.Name}' deleted.");
    }
}

static bool ReadPositiveInteger(string prompt, out int value)
{
    Console.Write(prompt);
    return int.TryParse(Console.ReadLine(), out value) && value > 0;
}