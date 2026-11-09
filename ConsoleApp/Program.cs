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
                MethodToRun = () => 
                {
                    var configs = configRepo.List();
                    Console.WriteLine("\nConfigurations:");
                    foreach (var cfg in configs)
                    {
                        Console.WriteLine($"- {cfg}");
                    }
                    return "";
                }
            }
        }
    };
}