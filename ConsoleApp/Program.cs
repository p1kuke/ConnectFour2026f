using ConsoleApp;
using DAL;
using DAL.Json;
using Game;
using MenuSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

bool useDatabase = true;
var services = new ServiceCollection();

if (useDatabase)
{
    services.AddDbContext<AppDbContext>();
    services.AddScoped<IConfigRepository, EfConfigRepository>();
    services.AddScoped<IGameRepository, EfGameRepository>();
}
else
{
    var basePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "ConnectFourData");

    services.AddScoped<IConfigRepository>(_ =>
        new JsonConfigRepository(Path.Combine(basePath, "configs")));
    services.AddScoped<IGameRepository>(_ =>
        new JsonGameRepository(Path.Combine(basePath, "games")));
}

using var serviceProvider = services.BuildServiceProvider();

if (useDatabase)
{
    using var migrationScope = serviceProvider.CreateScope();
    migrationScope.ServiceProvider
        .GetRequiredService<AppDbContext>()
        .Database
        .Migrate();
}

using var appScope = serviceProvider.CreateScope();
var configRepository = appScope.ServiceProvider.GetRequiredService<IConfigRepository>();
var gameRepository = appScope.ServiceProvider.GetRequiredService<IGameRepository>();

EnsureDefaultPresets(configRepository);
BuildMainMenu(configRepository, gameRepository).Run();

static void EnsureDefaultPresets(IConfigRepository configRepository)
{
    if (configRepository.List().Count != 0)
    {
        return;
    }

    foreach (var preset in GameConfiguration.Presets)
    {
        configRepository.Save(preset.Name, preset);
    }
}

static Menu BuildMainMenu(IConfigRepository configRepository, IGameRepository gameRepository)
{
    return new Menu("Connect Four - Main Menu", EMenuLevel.MainMenu)
    {
        Items =
        [
            new MenuItem
            {
                Title = "New Game",
                Shortcut = "1",
                MethodToRun = () =>
                {
                    var configurationNames = configRepository.List();
                    if (configurationNames.Count == 0)
                    {
                        Console.WriteLine("No configurations available!");
                        return string.Empty;
                    }

                    var configurationName = SelectName(configurationNames, "Select Configuration:");
                    if (configurationName is null)
                    {
                        return string.Empty;
                    }

                    var controller = new GameController(gameRepository);
                    var mode = SelectGameMode();
                    controller.RunGame(
                        new GameBrain(configRepository.Get(configurationName)),
                        Guid.NewGuid(),
                        $"Game_{DateTime.Now:yyyyMMdd_HHmmss}",
                        mode);
                    return string.Empty;
                }
            },
            new MenuItem
            {
                Title = "Load Saved Game",
                Shortcut = "2",
                MethodToRun = () =>
                {
                    var savedGames = gameRepository.List();
                    if (savedGames.Count == 0)
                    {
                        Console.WriteLine("\nNo saved games found.");
                        return string.Empty;
                    }

                    Console.WriteLine("\nSaved Games:");
                    for (var index = 0; index < savedGames.Count; index++)
                    {
                        var game = savedGames[index];
                        Console.WriteLine(
                            $"{index + 1}. {game.Name} " +
                            $"(Saved: {game.SavedAt.ToLocalTime():yyyy-MM-dd HH:mm})");
                    }

                    Console.Write("Select game to load: ");
                    if (!TrySelectIndex(savedGames.Count, out var selectedIndex))
                    {
                        Console.WriteLine("Invalid choice.");
                        return string.Empty;
                    }

                    var selectedGame = savedGames[selectedIndex];
                    var (state, name, _) = gameRepository.Get(selectedGame.Id);
                    new GameController(gameRepository)
                        .RunGame(new GameBrain(state), selectedGame.Id, name);
                    return string.Empty;
                }
            },
            new MenuItem
            {
                Title = "Manage Configurations",
                Shortcut = "3",
                MethodToRun = () => BuildConfigurationMenu(configRepository).Run()
            },
            new MenuItem
            {
                Title = "Delete Saved Game",
                Shortcut = "4",
                MethodToRun = () =>
                {
                    DeleteSavedGame(gameRepository);
                    return string.Empty;
                }
            }
        ]
    };
}

static Menu BuildConfigurationMenu(IConfigRepository configRepository)
{
    return new Menu("Configuration Management", EMenuLevel.SubMenu)
    {
        Items =
        [
            new MenuItem
            {
                Title = "List Configurations",
                Shortcut = "1",
                MethodToRun = () =>
                {
                    ListConfigurations(configRepository);
                    return string.Empty;
                }
            },
            new MenuItem
            {
                Title = "Create Configuration",
                Shortcut = "2",
                MethodToRun = () =>
                {
                    CreateConfiguration(configRepository);
                    return string.Empty;
                }
            },
            new MenuItem
            {
                Title = "Delete Configuration",
                Shortcut = "3",
                MethodToRun = () =>
                {
                    DeleteConfiguration(configRepository);
                    return string.Empty;
                }
            }
        ]
    };
}

static void ListConfigurations(IConfigRepository configRepository)
{
    var configurations = configRepository.List();
    Console.WriteLine("\nConfigurations:");
    if (configurations.Count == 0)
    {
        Console.WriteLine("No configurations found.");
        return;
    }

    foreach (var configuration in configurations)
    {
        Console.WriteLine($"- {configuration}");
    }
}

static void CreateConfiguration(IConfigRepository configRepository)
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
    var isCylinder = Console.ReadLine()?.Trim()
        .Equals("Y", StringComparison.OrdinalIgnoreCase) == true;
    configRepository.Save(name, new GameConfiguration(name, width, height, winCondition, isCylinder));
    Console.WriteLine($"Configuration '{name}' created.");
}

static void DeleteConfiguration(IConfigRepository configRepository)
{
    var configurations = configRepository.List();
    if (configurations.Count == 0)
    {
        Console.WriteLine("\nNo configurations found.");
        return;
    }

    var name = SelectName(configurations, "Select configuration to delete:");
    if (name is null)
    {
        return;
    }

    Console.Write($"Delete '{name}'? (Y/N): ");
    if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
    {
        configRepository.Delete(name);
        Console.WriteLine($"Configuration '{name}' deleted.");
    }
}

static void DeleteSavedGame(IGameRepository gameRepository)
{
    var savedGames = gameRepository.List();
    if (savedGames.Count == 0)
    {
        Console.WriteLine("\nNo saved games found.");
        return;
    }

    Console.WriteLine("\nSelect saved game to delete:");
    for (var index = 0; index < savedGames.Count; index++)
    {
        var game = savedGames[index];
        Console.WriteLine(
            $"{index + 1}. {game.Name} " +
            $"(Saved: {game.SavedAt.ToLocalTime():yyyy-MM-dd HH:mm})");
    }

    Console.Write("Choice: ");
    if (!TrySelectIndex(savedGames.Count, out var selectedIndex))
    {
        Console.WriteLine("Invalid choice.");
        return;
    }

    var selectedGame = savedGames[selectedIndex];
    Console.Write($"Delete '{selectedGame.Name}'? (Y/N): ");
    if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
    {
        gameRepository.Delete(selectedGame.Id);
        Console.WriteLine($"Saved game '{selectedGame.Name}' deleted.");
    }
}

static string? SelectName(IReadOnlyList<string> names, string title)
{
    Console.WriteLine($"\n{title}");
    for (var index = 0; index < names.Count; index++)
    {
        Console.WriteLine($"{index + 1}. {names[index]}");
    }

    Console.Write("Choice: ");
    return TrySelectIndex(names.Count, out var selectedIndex)
        ? names[selectedIndex]
        : null;
}

static GameMode SelectGameMode()
{
    while (true)
    {
        Console.WriteLine("Select game mode: 1) Human vs human 2) Human vs AI 3) AI vs AI");
        switch (Console.ReadLine()?.Trim())
        {
            case "1": return GameMode.HumanVsHuman;
            case "2": return GameMode.HumanVsAi;
            case "3": return GameMode.AiVsAi;
            default:
                Console.WriteLine("Unknown mode.");
                break;
        }
    }
}

static bool TrySelectIndex(int itemCount, out int index)
{
    if (int.TryParse(Console.ReadLine(), out var choice) &&
        choice >= 1 &&
        choice <= itemCount)
    {
        index = choice - 1;
        return true;
    }

    index = -1;
    return false;
}

static bool ReadPositiveInteger(string prompt, out int value)
{
    Console.Write(prompt);
    return int.TryParse(Console.ReadLine(), out value) && value > 0;
}
