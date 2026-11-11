using Game;
using MenuSystem;

namespace ConsoleApp;

public sealed class ConsoleApplication
{
    private readonly IConfigRepository _configRepository;
    private readonly IGameRepository _gameRepository;

    public ConsoleApplication(IConfigRepository configRepository, IGameRepository gameRepository)
    {
        _configRepository = configRepository;
        _gameRepository = gameRepository;
    }

    public void Run()
    {
        EnsureDefaultPresets();
        BuildMainMenu().Run();
    }

    private void EnsureDefaultPresets()
    {
        if (_configRepository.List().Count != 0) return;
        foreach (var preset in GameConfiguration.Presets)
            _configRepository.Save(preset.Name, preset);
    }

    private Menu BuildMainMenu() => new("Connect Four - Main Menu")
    {
        Items =
        [
            new MenuItem { Title = "New Game", Shortcut = "1", MethodToRun = StartNewGame },
            new MenuItem { Title = "Load Saved Game", Shortcut = "2", MethodToRun = LoadGame },
            new MenuItem { Title = "Manage Configurations", Shortcut = "3", MethodToRun = ManageConfigurations },
            new MenuItem { Title = "Delete Saved Game", Shortcut = "4", MethodToRun = DeleteGame }
        ]
    };

    private string StartNewGame()
    {
        var names = _configRepository.List();
        if (names.Count == 0) { Console.WriteLine("No configurations available."); return string.Empty; }
        var name = SelectName(names, "Select configuration:");
        if (name is null) return string.Empty;
        var mode = SelectGameMode();
        new GameController(_gameRepository).RunGame(
            new GameBrain(_configRepository.Get(name)), Guid.NewGuid(),
            $"Game_{DateTime.Now:yyyyMMdd_HHmmss}", mode);
        return string.Empty;
    }

    private string LoadGame()
    {
        var games = _gameRepository.List();
        if (games.Count == 0) { Console.WriteLine("No saved games found."); return string.Empty; }
        for (var i = 0; i < games.Count; i++)
            Console.WriteLine($"{i + 1}. {games[i].Name} ({games[i].SavedAt.ToLocalTime():yyyy-MM-dd HH:mm})");
        if (!TrySelectIndex(games.Count, out var index)) return string.Empty;
        var game = games[index];
        var (state, name, _) = _gameRepository.Get(game.Id);
        new GameController(_gameRepository).RunGame(new GameBrain(state), game.Id, name);
        return string.Empty;
    }

    private string ManageConfigurations() => new Menu("Configuration Management", EMenuLevel.SubMenu)
    {
        Items =
        [
            new MenuItem { Title = "List Configurations", Shortcut = "1", MethodToRun = ListConfigurations },
            new MenuItem { Title = "Create Configuration", Shortcut = "2", MethodToRun = CreateConfiguration },
            new MenuItem { Title = "Delete Configuration", Shortcut = "3", MethodToRun = DeleteConfiguration }
        ]
    }.Run();

    private string ListConfigurations()
    {
        foreach (var name in _configRepository.List()) Console.WriteLine($"- {name}");
        return string.Empty;
    }

    private string CreateConfiguration()
    {
        Console.Write("Name: ");
        var name = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(name) ||
            !ReadPositiveInteger("Width: ", out var width) ||
            !ReadPositiveInteger("Height: ", out var height) ||
            !ReadPositiveInteger("Win length: ", out var win))
        {
            Console.WriteLine("Invalid configuration.");
            return string.Empty;
        }
        Console.Write("Cylinder board? (Y/N): ");
        var cylinder = Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true;
        var config = new GameConfiguration(name, width, height, win, cylinder);
        try { config.Validate(); _configRepository.Save(name, config); }
        catch (ArgumentException exception) { Console.WriteLine(exception.Message); }
        return string.Empty;
    }

    private string DeleteConfiguration()
    {
        var name = SelectName(_configRepository.List(), "Select configuration:");
        if (name is not null)
        {
            Console.Write($"Delete '{name}'? (Y/N): ");
            if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
                _configRepository.Delete(name);
        }
        return string.Empty;
    }

    private string DeleteGame()
    {
        var games = _gameRepository.List();
        if (games.Count == 0)
        {
            Console.WriteLine("No saved games found.");
            return string.Empty;
        }

        Console.WriteLine("\nSaved games:");
        for (var i = 0; i < games.Count; i++)
            Console.WriteLine($"{i + 1}. {games[i].Name} ({games[i].SavedAt.ToLocalTime():yyyy-MM-dd HH:mm})");

        Console.Write("Select a game to delete: ");
        if (TrySelectIndex(games.Count, out var index))
        {
            Console.Write($"Delete '{games[index].Name}'? (Y/N): ");
            if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
            {
                _gameRepository.Delete(games[index].Id);
                Console.WriteLine($"Saved game '{games[index].Name}' deleted.");
            }
        }
        else
        {
            Console.WriteLine("Invalid choice.");
        }
        return string.Empty;
    }

    private static string? SelectName(IReadOnlyList<string> names, string title)
    {
        Console.WriteLine(title);
        for (var i = 0; i < names.Count; i++) Console.WriteLine($"{i + 1}. {names[i]}");
        return TrySelectIndex(names.Count, out var index) ? names[index] : null;
    }

    private static GameMode SelectGameMode()
    {
        Console.WriteLine("1) Human vs human 2) Human vs AI 3) AI vs AI");
        return Console.ReadLine()?.Trim() switch
        {
            "2" => GameMode.HumanVsAi,
            "3" => GameMode.AiVsAi,
            _ => GameMode.HumanVsHuman
        };
    }

    private static bool TrySelectIndex(int count, out int index)
    {
        var valid = int.TryParse(Console.ReadLine(), out var choice) && choice > 0 && choice <= count;
        index = valid ? choice - 1 : -1;
        return valid;
    }

    private static bool ReadPositiveInteger(string prompt, out int value)
    {
        Console.Write(prompt);
        return int.TryParse(Console.ReadLine(), out value) && value > 0;
    }
}
