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
        var existing = _configRepository.List();
        foreach (var preset in GameConfiguration.Presets)
        {
            if (!existing.Any(name =>
                    string.Equals(name, preset.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _configRepository.Save(preset.Name, preset);
            }
        }
    }

    private Menu BuildMainMenu() => new Menu("Connect Four - Main Menu")
        .AddItems(
            new MenuItem { Title = "New Game", Shortcut = "1", MethodToRun = StartNewGame },
            new MenuItem { Title = "Load Saved Game", Shortcut = "2", MethodToRun = LoadGame },
            new MenuItem { Title = "Manage Configurations", Shortcut = "3", MethodToRun = ManageConfigurations },
            new MenuItem { Title = "Delete Saved Game", Shortcut = "4", MethodToRun = DeleteGame });

    private string StartNewGame()
    {
        var names = _configRepository.List();
        if (names.Count == 0)
        {
            Console.WriteLine("No configurations available.");
            return string.Empty;
        }

        var name = SelectName(names, "Select configuration:");
        if (name is null)
        {
            return string.Empty;
        }

        var mode = SelectGameMode();
        new GameController(_gameRepository).RunGame(
            new GameBrain(_configRepository.Get(name)),
            Guid.NewGuid(),
            $"Game_{DateTime.Now:yyyyMMdd_HHmmss}",
            mode);
        return string.Empty;
    }

    private string LoadGame()
    {
        var games = _gameRepository.List();
        if (games.Count == 0)
        {
            Console.WriteLine("No saved games found.");
            return string.Empty;
        }

        PrintSavedGames(games);
        if (!TrySelectIndex(games.Count, out var index))
        {
            return string.Empty;
        }

        var game = games[index];
        var (state, name, _) = _gameRepository.Get(game.Id);
        new GameController(_gameRepository).RunGame(new GameBrain(state), game.Id, name);
        return string.Empty;
    }

    private string ManageConfigurations() => new Menu("Configuration Management", EMenuLevel.SubMenu)
        .AddItems(
            new MenuItem { Title = "List Configurations", Shortcut = "1", MethodToRun = ListConfigurations },
            new MenuItem { Title = "Create Configuration", Shortcut = "2", MethodToRun = CreateConfiguration },
            new MenuItem { Title = "Edit Configuration", Shortcut = "3", MethodToRun = EditConfiguration },
            new MenuItem { Title = "Delete Configuration", Shortcut = "4", MethodToRun = DeleteConfiguration })
        .Run();

    private string ListConfigurations()
    {
        var names = _configRepository.List();
        if (names.Count == 0)
        {
            Console.WriteLine("No configurations available.");
            return string.Empty;
        }

        foreach (var name in names)
        {
            PrintConfiguration(_configRepository.Get(name));
        }

        return string.Empty;
    }

    private string CreateConfiguration()
    {
        Console.Write("Name: ");
        var name = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Configuration name cannot be empty.");
            return string.Empty;
        }

        if (GameConfiguration.IsPresetName(name))
        {
            Console.WriteLine("Preset configurations cannot be overwritten. Choose another name.");
            return string.Empty;
        }

        if (_configRepository.List().Any(existing =>
                string.Equals(existing, name, StringComparison.OrdinalIgnoreCase)))
        {
            Console.Write($"Configuration '{name}' already exists. Overwrite it? (Y/N): ");
            if (!ReadYesNoAnswer())
            {
                return string.Empty;
            }
        }

        if (!ReadPositiveInteger("Width: ", out var width) ||
            !ReadPositiveInteger("Height: ", out var height) ||
            !ReadPositiveInteger("Win length: ", out var win))
        {
            Console.WriteLine("Invalid configuration.");
            return string.Empty;
        }

        var cylinder = ReadYesNo("Cylinder board? (Y/N): ", false);
        var configuration = new GameConfiguration(name, width, height, win, cylinder);

        try
        {
            configuration.Validate();
            _configRepository.Save(name, configuration);
            Console.WriteLine($"Configuration '{name}' saved.");
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine(exception.Message);
        }

        return string.Empty;
    }

    private string EditConfiguration()
    {
        var name = SelectName(_configRepository.List(), "Select configuration to edit:");
        if (name is null)
        {
            return string.Empty;
        }

        if (GameConfiguration.IsPresetName(name))
        {
            Console.WriteLine("Preset configurations cannot be edited. Create a custom configuration.");
            return string.Empty;
        }

        var current = _configRepository.Get(name);
        Console.WriteLine("Press Enter to keep the current value.");
        if (!ReadPositiveInteger("Width: ", current.BoardWidth, out var width) ||
            !ReadPositiveInteger("Height: ", current.BoardHeight, out var height) ||
            !ReadPositiveInteger("Win length: ", current.WinCondition, out var win))
        {
            Console.WriteLine("Invalid configuration.");
            return string.Empty;
        }

        var cylinder = ReadYesNo("Cylinder board? (Y/N): ", current.IsCylinder);
        var updated = new GameConfiguration(name, width, height, win, cylinder);

        try
        {
            updated.Validate();
            _configRepository.Save(name, updated);
            Console.WriteLine($"Configuration '{name}' updated.");
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine(exception.Message);
        }

        return string.Empty;
    }

    private string DeleteConfiguration()
    {
        var name = SelectName(_configRepository.List(), "Select configuration:");
        if (name is null)
        {
            return string.Empty;
        }

        if (GameConfiguration.IsPresetName(name))
        {
            Console.WriteLine("Preset configurations cannot be deleted.");
            return string.Empty;
        }

        Console.Write($"Delete '{name}'? (Y/N): ");
        if (ReadYesNoAnswer())
        {
            _configRepository.Delete(name);
            Console.WriteLine($"Configuration '{name}' deleted.");
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
        PrintSavedGames(games);

        Console.Write("Select a game to delete: ");
        if (!TrySelectIndex(games.Count, out var index))
        {
            Console.WriteLine("Invalid choice.");
            return string.Empty;
        }

        Console.Write($"Delete '{games[index].Name}'? (Y/N): ");
        if (ReadYesNoAnswer())
        {
            _gameRepository.Delete(games[index].Id);
            Console.WriteLine($"Saved game '{games[index].Name}' deleted.");
        }

        return string.Empty;
    }

    private void PrintSavedGames(IReadOnlyList<(Guid Id, string Name, DateTime SavedAt)> games)
    {
        for (var i = 0; i < games.Count; i++)
        {
            var (state, _, _) = _gameRepository.Get(games[i].Id);
            var configurationName = state.Configuration?.Name ?? "unknown";
            Console.WriteLine(
                $"{i + 1}. {games[i].Name} | {configurationName} | {state.GameMode} | " +
                $"{games[i].SavedAt.ToLocalTime():yyyy-MM-dd HH:mm}");
        }
    }

    private static void PrintConfiguration(GameConfiguration configuration)
    {
        Console.WriteLine(
            $"- {configuration.Name}: {configuration.BoardWidth}x{configuration.BoardHeight}, " +
            $"win {configuration.WinCondition}, " +
            $"{(configuration.IsCylinder ? "cylinder" : "rectangle")}");
    }

    private static string? SelectName(IReadOnlyList<string> names, string title)
    {
        if (names.Count == 0)
        {
            Console.WriteLine("No configurations available.");
            return null;
        }

        Console.WriteLine(title);
        for (var i = 0; i < names.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {names[i]}");
        }

        return TrySelectIndex(names.Count, out var index) ? names[index] : null;
    }

    private static EGameMode SelectGameMode()
    {
        Console.WriteLine("1) Human vs human");
        Console.WriteLine("2) Human vs AI");
        Console.WriteLine("3) AI vs AI");
        Console.Write("Select play mode: ");
        return Console.ReadLine()?.Trim() switch
        {
            "2" => EGameMode.HumanVsAi,
            "3" => EGameMode.AiVsAi,
            _ => EGameMode.HumanVsHuman
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

    private static bool ReadPositiveInteger(string prompt, int current, out int value)
    {
        while (true)
        {
            Console.Write($"{prompt}[{current}]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                value = current;
                return true;
            }

            if (int.TryParse(input, out value) && value > 0)
            {
                return true;
            }

            Console.WriteLine("Enter a positive integer, or press Enter to keep the current value.");
        }
    }

    private static bool ReadYesNo(string prompt, bool current)
    {
        while (true)
        {
            Console.Write($"{prompt} [{(current ? "Y" : "N")}]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return current;
            }

            if (string.Equals(input, "Y", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(input, "N", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.WriteLine("Please enter Y or N.");
        }
    }

    private static bool ReadYesNoAnswer()
    {
        while (true)
        {
            var input = Console.ReadLine()?.Trim();
            if (string.Equals(input, "Y", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(input, "N", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.WriteLine("Please enter Y or N.");
        }
    }
}
