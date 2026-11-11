using Game;
using MenuSystem;

namespace ConsoleApp;

public sealed class ConfigurationMenu
{
    private readonly IConfigRepository _repository;

    public ConfigurationMenu(IConfigRepository repository)
    {
        _repository = repository;
    }

    public string Run()
    {
        return new Menu("Configuration Management", EMenuLevel.SubMenu)
        {
            Items =
            [
                new MenuItem { Title = "List Configurations", Shortcut = "1", MethodToRun = List },
                new MenuItem { Title = "Create Configuration", Shortcut = "2", MethodToRun = Create },
                new MenuItem { Title = "Delete Configuration", Shortcut = "3", MethodToRun = Delete }
            ]
        }.Run();
    }

    public string? SelectConfiguration()
    {
        var names = _repository.List();
        if (names.Count == 0)
        {
            Console.WriteLine("No configurations available.");
            return null;
        }

        Console.WriteLine("Select configuration:");
        for (var index = 0; index < names.Count; index++)
            Console.WriteLine($"{index + 1}. {names[index]}");

        return int.TryParse(Console.ReadLine(), out var choice) &&
               choice >= 1 &&
               choice <= names.Count
            ? names[choice - 1]
            : null;
    }

    private string List()
    {
        foreach (var name in _repository.List())
            Console.WriteLine($"- {name}");
        return string.Empty;
    }

    private string Create()
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
        var cylinder = Console.ReadLine()?.Trim()
            .Equals("Y", StringComparison.OrdinalIgnoreCase) == true;
        var configuration = new GameConfiguration(name, width, height, win, cylinder);

        try
        {
            configuration.Validate();
            _repository.Save(name, configuration);
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine(exception.Message);
        }

        return string.Empty;
    }

    private string Delete()
    {
        var name = SelectConfiguration();
        if (name is null)
            return string.Empty;

        Console.Write($"Delete '{name}'? (Y/N): ");
        if (Console.ReadLine()?.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
            _repository.Delete(name);

        return string.Empty;
    }

    private static bool ReadPositiveInteger(string prompt, out int value)
    {
        Console.Write(prompt);
        return int.TryParse(Console.ReadLine(), out value) && value > 0;
    }
}
