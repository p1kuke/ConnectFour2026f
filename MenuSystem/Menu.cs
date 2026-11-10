namespace MenuSystem;

public class Menu
{
    private readonly string _title;
    public Func<string>? TitleProvider { get; set; }

    private List<MenuItem> _items = new();

    public List<MenuItem> Items
    {
        get => _items;
        set
        {
            _items = value ?? throw new ArgumentNullException(nameof(value));
            ValidateItems();
        }
    }

    private readonly EMenuLevel _menuLevel;

    public Menu(string title, EMenuLevel menuLevel = EMenuLevel.MainMenu)
    {
        _title = title;
        _menuLevel = menuLevel;
    }

    public string Run()
    {
        ValidateItems();

        var menuDone = false;
        var actionResult = "";
        do
        {
            PrintMenu();
            var userChoice = Console.ReadLine();
            // validate user choice
            if (ValidateUserChoice(userChoice))
            {
                // act on user choice
                actionResult = PerformUserChoice(userChoice!);
                if (_menuLevel == EMenuLevel.MainMenu && actionResult == "E")
                {
                    menuDone = true;
                }
                else if (_menuLevel != EMenuLevel.MainMenu &&
                         (actionResult == "E" || actionResult == "M" || actionResult == "P"))
                {
                    menuDone = true;
                }
                
            }
            else
            {
                Console.WriteLine("Invalid choice. Please try again.");
            }
        } while (!menuDone);

        return actionResult;
    }

    private string PerformUserChoice(string userChoice)
    {
        foreach (var menuItem in Items)
        {
            if (string.Equals(menuItem.Shortcut, userChoice, StringComparison.OrdinalIgnoreCase))
                return menuItem.MethodToRun != null ? menuItem.MethodToRun() : "";
        }

        if (IsMandatoryShortcut(userChoice))
            return userChoice.ToUpperInvariant();

        return "";
    }

    private bool ValidateUserChoice(string? userChoice)
    {
        if (userChoice == null)
        {
            return false;
        }

        foreach (var menuItem in Items)
        {
            if (string.Equals(menuItem.Shortcut, userChoice, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return IsMandatoryShortcut(userChoice);
    }

    private readonly MenuItem _exitMenuItem = new MenuItem()
    {
        Title = "Exit",
        Shortcut = "E",
    };

    private readonly MenuItem _returnToMainMenuItem = new MenuItem()
    {
        Title = "Return to main menu",
        Shortcut = "M",
    };

    private readonly MenuItem _returnToPreviousMenuItem = new MenuItem()
    {
        Title = "Return to previous menu",
        Shortcut = "P",
    };

    private void PrintMenu()
    {
        Console.WriteLine(GetTitle());
        Console.WriteLine("---------------------");
        foreach (var menuItem in Items)
        {
            Console.WriteLine(menuItem);
        }

        switch (_menuLevel)
        {
            case EMenuLevel.MainMenu:
                Console.WriteLine(_exitMenuItem);
                break;
            case EMenuLevel.SubMenu:
                Console.WriteLine(_exitMenuItem);
                Console.WriteLine(_returnToPreviousMenuItem);
                break;
            case EMenuLevel.SubSubMenu:
                Console.WriteLine(_exitMenuItem);
                Console.WriteLine(_returnToPreviousMenuItem);
                Console.WriteLine(_returnToMainMenuItem);
                break;
        }

        Console.WriteLine("---------------------");
        Console.Write("Please select an option: ");
    }

    private string GetTitle() =>
        TitleProvider != null ? TitleProvider() : _title;

    private bool IsMandatoryShortcut(string shortcut) =>
        shortcut.ToUpperInvariant() switch
        {
            "E" when _menuLevel is EMenuLevel.MainMenu or EMenuLevel.SubMenu or EMenuLevel.SubSubMenu => true,
            "M" when _menuLevel is EMenuLevel.SubMenu or EMenuLevel.SubSubMenu => true,
            "P" when _menuLevel == EMenuLevel.SubSubMenu => true,
            _ => false
        };

    private void ValidateItems()
    {
        var shortcuts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in Items)
        {
            if (item is null)
                throw new ArgumentException("Menu items cannot be null.", nameof(Items));

            if (string.IsNullOrWhiteSpace(item.Shortcut))
                throw new ArgumentException("Menu item shortcuts cannot be empty.", nameof(Items));

            if (item.Shortcut.Length == 1 && item.Shortcut.ToUpperInvariant() is "E" or "M" or "P")
            {
                throw new ArgumentException(
                    $"Shortcut '{item.Shortcut}' is reserved for menu navigation.",
                    nameof(Items));
            }

            if (!shortcuts.Add(item.Shortcut))
                throw new ArgumentException(
                    $"Duplicate menu shortcut '{item.Shortcut}'.",
                    nameof(Items));
        }
    }
}
