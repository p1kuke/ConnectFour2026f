namespace MenuSystem;

public class Menu
{
    private readonly string _title;
    public Func<string>? TitleProvider { get; set; }

    public List<MenuItem> Items { get; set; } = new List<MenuItem>();

    private readonly EMenuLevel _menuLevel;

    public Menu(string title, EMenuLevel menuLevel = EMenuLevel.MainMenu)
    {
        _title = title;
        _menuLevel = menuLevel;
    }

    public string Run()
    {
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
                if (_menuLevel != EMenuLevel.MainMenu && actionResult == "M")
                {
                    menuDone = true;
                }
                if (_menuLevel == EMenuLevel.SubSubMenu && actionResult == "P")
                {
                    menuDone = true;
                    actionResult = "Prev";
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
            if (menuItem.Shortcut == userChoice)
                return menuItem.MethodToRun != null ? menuItem.MethodToRun() : "";
        }

        if (userChoice == "E" ||
            userChoice == "M" ||
            userChoice == "P")
            return userChoice;

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
            if (menuItem.Shortcut == userChoice) return true;
        }

        if (_menuLevel == EMenuLevel.MainMenu && userChoice == "E")
            return true;

        if (_menuLevel == EMenuLevel.SubMenu && userChoice == "M")
            return true;

        if (_menuLevel == EMenuLevel.SubSubMenu && (userChoice == "M" || userChoice == "P"))
            return true;

        return false;
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
                Console.WriteLine(_returnToMainMenuItem);
                break;
            case EMenuLevel.SubSubMenu:
                Console.WriteLine(_returnToPreviousMenuItem);
                Console.WriteLine(_returnToMainMenuItem);
                break;
        }

        Console.WriteLine("---------------------");
        Console.Write("Please select an option: ");
    }

    private string GetTitle() =>
        TitleProvider != null ? TitleProvider() : _title;
}
