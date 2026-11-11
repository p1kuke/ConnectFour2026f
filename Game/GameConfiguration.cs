namespace Game;

public record GameConfiguration
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = "Classic 7x6";
    
    public int BoardWidth { get; init; } = 7;
    public int BoardHeight { get; init; } = 6;
    public int WinCondition { get; init; } = 4;
    public bool IsCylinder { get; init; } = false;

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

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Configuration name cannot be empty.", nameof(Name));
        if (BoardWidth <= 0 || BoardHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(BoardWidth), "Board dimensions must be positive.");
        if (WinCondition <= 0 || WinCondition > Math.Max(BoardWidth, BoardHeight))
            throw new ArgumentOutOfRangeException(nameof(WinCondition), "Win condition is outside the board dimensions.");
    }

    public static bool IsPresetName(string name) =>
        Presets.Any(preset => string.Equals(preset.Name, name, StringComparison.OrdinalIgnoreCase));

    // Presets for quick access to common configurations
    public static List<GameConfiguration> Presets => new()
    {
        new GameConfiguration("Classic 7x6 / 4", 7, 6, 4, false),
        new GameConfiguration("Connect3 5x4 / 3", 5, 4, 3, false),
        new GameConfiguration("Connect5 9x7 / 5", 9, 7, 5, false),
        new GameConfiguration("Cylinder 7x6 / 4", 7, 6, 4, true)
    };
}