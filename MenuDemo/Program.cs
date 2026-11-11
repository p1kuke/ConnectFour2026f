using MenuSystem;

var state = new DemoState();

var level3 = new Menu("Level 3 - Deep menu", EMenuLevel.SubSubMenu)
    .AddItems(
        new MenuItem
        {
            TitleProvider = () => $"Persistence: {state.Persistence}",
            Shortcut = "1",
            MethodToRun = () =>
            {
                state.Toggle();
                return string.Empty;
            }
        },
        new MenuItem
        {
            Title = "Show current settings",
            Shortcut = "2",
            MethodToRun = () =>
            {
                Console.WriteLine($"Current persistence: {state.Persistence}");
                return string.Empty;
            }
        });

var level2 = new Menu("Level 2 - Settings", EMenuLevel.SubMenu)
    .AddItems(
        new MenuItem
        {
            Title = "Open level 3",
            Shortcut = "1",
            MethodToRun = () => level3.Run()
        },
        new MenuItem
        {
            TitleProvider = () => $"Persistence: {state.Persistence}",
            Shortcut = "2",
            MethodToRun = () =>
            {
                state.Toggle();
                return string.Empty;
            }
        });

var main = new Menu("MenuSystem demo - three levels")
    .AddItems(
        new MenuItem
        {
            Title = "Open level 2",
            Shortcut = "1",
            MethodToRun = () => level2.Run()
        },
        new MenuItem
        {
            TitleProvider = () => $"Persistence: {state.Persistence}",
            Shortcut = "2",
            MethodToRun = () =>
            {
                state.Toggle();
                return string.Empty;
            }
        });

main.Run();

internal sealed class DemoState
{
    public string Persistence { get; private set; } = "JSON";

    public void Toggle() => Persistence = Persistence == "JSON" ? "DB" : "JSON";
}
