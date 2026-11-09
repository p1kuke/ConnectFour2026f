using Game;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

public class GameState : BaseEntity
{
    public GameConfiguration Configuration { get; set; } = default!;
    public DateTime SavedAt { get; set; }
}