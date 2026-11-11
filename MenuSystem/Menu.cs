namespace MenuSystem;

public class Menu
{
    private readonly string _title;
    private readonly EMenuLevel _menuLevel;
    private readonly List<MenuItem> _items = new();

    public Menu(string title, EMenuLevel menuLevel = EMenuLevel.MainMenu)
    {
        _title = title;
        _menuLevel = menuLevel;
    }

    public Func<string>? TitleProvider { get; set; }

    public IReadOnlyList<MenuItem> Items => _items;

    public Menu AddItem(MenuItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var candidate = _items.Append(item).ToList();
        ValidateItems(candidate);
        _items.Add(item);
        return this;
    }

    public Menu AddItems(params MenuItem[] items) => AddItems((IEnumerable<MenuItem>)items);

    public Menu AddItems(IEnumerable<MenuItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var additions = items.ToList();
        var candidate = _items.Concat(additions).ToList();
        ValidateItems(candidate);
        _items.AddRange(additions);
        return this;
    }

    public string Run()
    {
        ValidateItems(_items);

        var menuDone = false;
        var actionResult = "";
        do
        {
            PrintMenu();
            var userChoice = Console.ReadLine();
            if (userChoice is null)
            {
                Console.WriteLine();
                return actionResult;
            }

            if (!ValidateUserChoice(userChoice))
            {
                Console.WriteLine("Invalid choice. Please try again.");
                continue;
            }

            var isMandatory = IsMandatoryShortcut(userChoice);
            actionResult = PerformUserChoice(userChoice);

            if (isMandatory)
            {
                if (actionResult == "E")
                {
                    menuDone = true;
                }
                else if (_menuLevel != EMenuLevel.MainMenu &&
                         actionResult is "P" or "M")
                {
                    menuDone = true;
                }

                continue;
            }

            // E exits from any depth. M unwinds to the main menu. A child's P means it
            // already returned to this menu, so this menu must keep running.
            if (actionResult == "E")
            {
                menuDone = true;
            }
            else if (actionResult == "M" && _menuLevel != EMenuLevel.MainMenu)
            {
                menuDone = true;
            }
        } while (!menuDone);

        return actionResult;
    }

    private string PerformUserChoice(string userChoice)
    {
        foreach (var menuItem in _items)
        {
            if (string.Equals(menuItem.Shortcut, userChoice, StringComparison.OrdinalIgnoreCase))
            {
                return menuItem.MethodToRun != null ? menuItem.MethodToRun() : "";
            }
        }

        if (IsMandatoryShortcut(userChoice))
        {
            return userChoice.ToUpperInvariant();
        }

        return "";
    }

    private bool ValidateUserChoice(string userChoice)
    {
        foreach (var menuItem in _items)
        {
            if (string.Equals(menuItem.Shortcut, userChoice, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return IsMandatoryShortcut(userChoice);
    }

    private readonly MenuItem _exitMenuItem = new()
    {
        Title = "Exit",
        Shortcut = "E",
    };

    private readonly MenuItem _returnToMainMenuItem = new()
    {
        Title = "Return to main menu",
        Shortcut = "M",
    };

    private readonly MenuItem _returnToPreviousMenuItem = new()
    {
        Title = "Return to previous menu",
        Shortcut = "P",
    };

    private void PrintMenu()
    {
        Console.WriteLine(GetTitle());
        Console.WriteLine("---------------------");
        foreach (var menuItem in _items)
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

    private string GetTitle() => TitleProvider != null ? TitleProvider() : _title;

    private bool IsMandatoryShortcut(string shortcut) =>
        shortcut.ToUpperInvariant() switch
        {
            "E" => true,
            "P" when _menuLevel is EMenuLevel.SubMenu or EMenuLevel.SubSubMenu => true,
            "M" when _menuLevel == EMenuLevel.SubSubMenu => true,
            _ => false
        };

    private static void ValidateItems(IReadOnlyCollection<MenuItem> items)
    {
        var shortcuts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Shortcut))
            {
                throw new ArgumentException("Menu item shortcuts cannot be empty.", nameof(items));
            }

            if (string.IsNullOrWhiteSpace(item.Title) && item.TitleProvider is null)
            {
                throw new ArgumentException("Menu item titles cannot be empty.", nameof(items));
            }

            if (item.Shortcut.Length == 1 &&
                item.Shortcut.ToUpperInvariant() is "E" or "M" or "P")
            {
                throw new ArgumentException(
                    $"Shortcut '{item.Shortcut}' is reserved for menu navigation.",
                    nameof(items));
            }

            if (!shortcuts.Add(item.Shortcut))
            {
                throw new ArgumentException(
                    $"Duplicate menu shortcut '{item.Shortcut}'.",
                    nameof(items));
            }
        }
    }
}
