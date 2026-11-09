namespace Game;

public record GameConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Classic 7x6";
    
    public int BoardWidth { get; set; } = 7;
    public int BoardHeight { get; set; } = 6;
    public int WinCondition { get; set; } = 4;
    public bool IsCylinder { get; set; } = false;

    // Constructor for deserialization
    public GameConfiguration() { }

    public GameConfiguration(string name, int width, int height, int winCondition, bool isCylinder = false)
    {
        Name = name;
        BoardWidth = width;
        BoardHeight = height;
        WinCondition = winCondition;
        IsCylinder = isCylinder;
    }
    
    // Presets for quick access to common configurations
    public static List<GameConfiguration> Presets => new()
    {
        new GameConfiguration("Classic 7x6 / 4", 7, 6, 4, false),
        new GameConfiguration("Connect3 5x4 / 3", 5, 4, 3, false),
        new GameConfiguration("Connect5 9x7 / 5", 9, 7, 5, false),
        new GameConfiguration("Cylinder 7x6 / 4", 7, 6, 4, true)
    };
}