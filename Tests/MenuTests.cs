using MenuSystem;
using Xunit;

namespace Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ConsoleCollection
{
    public const string Name = "Console";
}

[Collection(ConsoleCollection.Name)]
public sealed class MenuTests
{
    [Fact]
    public void P_FromSubSubMenu_ReturnsToSubMenu_NotToMainMenu()
    {
        var level3 = new Menu("Level 3", EMenuLevel.SubSubMenu)
            .AddItem(new MenuItem { Title = "No-op", Shortcut = "1" });
        var level2 = new Menu("Level 2", EMenuLevel.SubMenu)
            .AddItem(new MenuItem { Title = "Open level 3", Shortcut = "1", MethodToRun = level3.Run });
        var main = new Menu("Main")
            .AddItem(new MenuItem { Title = "Open level 2", Shortcut = "1", MethodToRun = level2.Run });

        // Main -> level 2 -> level 3 -> P (back to level 2) -> P (back to main) -> E.
        var (result, output) = RunWithInput(main, "1", "1", "P", "P", "E");

        Assert.Equal("E", result);
        Assert.Equal(1, CountOccurrences(output, "Level 3"));
        Assert.Equal(2, CountOccurrences(output, "Level 2"));
        Assert.Equal(2, CountOccurrences(output, "Main"));
    }

    [Fact]
    public void M_FromSubSubMenu_ReturnsToMainMenu()
    {
        var level3 = new Menu("Level 3", EMenuLevel.SubSubMenu)
            .AddItem(new MenuItem { Title = "No-op", Shortcut = "1" });
        var level2 = new Menu("Level 2", EMenuLevel.SubMenu)
            .AddItem(new MenuItem { Title = "Open level 3", Shortcut = "1", MethodToRun = level3.Run });
        var main = new Menu("Main")
            .AddItem(new MenuItem { Title = "Open level 2", Shortcut = "1", MethodToRun = level2.Run });

        var (result, output) = RunWithInput(main, "1", "1", "M", "E");

        Assert.Equal("E", result);
        Assert.Equal(1, CountOccurrences(output, "Level 3"));
        Assert.Equal(1, CountOccurrences(output, "Level 2"));
        Assert.Equal(2, CountOccurrences(output, "Main"));
    }

    [Fact]
    public void E_ExitsFromAnyDepth()
    {
        var level3 = new Menu("Level 3", EMenuLevel.SubSubMenu)
            .AddItem(new MenuItem { Title = "No-op", Shortcut = "1" });
        var level2 = new Menu("Level 2", EMenuLevel.SubMenu)
            .AddItem(new MenuItem { Title = "Open level 3", Shortcut = "1", MethodToRun = level3.Run });
        var main = new Menu("Main")
            .AddItem(new MenuItem { Title = "Open level 2", Shortcut = "1", MethodToRun = level2.Run });

        var (result, output) = RunWithInput(main, "1", "1", "E");

        Assert.Equal("E", result);
        Assert.Equal(1, CountOccurrences(output, "Main"));
    }

    [Fact]
    public void DuplicateShortcuts_AreRejected_AndMenuStaysUnchanged()
    {
        var menu = new Menu("Main")
            .AddItem(new MenuItem { Title = "First", Shortcut = "1" });

        Assert.Throws<ArgumentException>(() =>
            menu.AddItem(new MenuItem { Title = "Second", Shortcut = "1" }));
        Assert.Single(menu.Items);
    }

    [Theory]
    [InlineData("E")]
    [InlineData("M")]
    [InlineData("P")]
    [InlineData("e")]
    [InlineData("m")]
    [InlineData("p")]
    public void ReservedNavigationShortcuts_AreRejected(string shortcut)
    {
        var menu = new Menu("Main");

        Assert.Throws<ArgumentException>(() =>
            menu.AddItem(new MenuItem { Title = "Reserved", Shortcut = shortcut }));
        Assert.Empty(menu.Items);
    }

    [Fact]
    public void AddItems_WithOneInvalidItem_AddsNothing()
    {
        var menu = new Menu("Main");

        Assert.Throws<ArgumentException>(() => menu.AddItems(
            new MenuItem { Title = "Valid", Shortcut = "1" },
            new MenuItem { Title = "Empty shortcut", Shortcut = " " }));
        Assert.Empty(menu.Items);
    }

    [Fact]
    public void EmptyTitle_IsRejected_UnlessTitleProviderIsSet()
    {
        var menu = new Menu("Main");

        Assert.Throws<ArgumentException>(() =>
            menu.AddItem(new MenuItem { Title = " ", Shortcut = "1" }));

        menu.AddItem(new MenuItem
        {
            Title = "Ignored",
            TitleProvider = () => "Dynamic title",
            Shortcut = "1"
        });

        Assert.Single(menu.Items);
    }

    private static (string Result, string Output) RunWithInput(Menu menu, params string[] input)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        var output = new StringWriter();

        try
        {
            Console.SetIn(new StringReader(string.Join(Environment.NewLine, input) + Environment.NewLine));
            Console.SetOut(output);
            var result = menu.Run();
            return (result, output.ToString());
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
