using Game;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class EfGameRepository : IGameRepository
{
    private readonly AppDbContext _context;

    public EfGameRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Save(Guid id, string name, GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(gameState.Configuration);
        gameState.Id = id;
        gameState.Name = name;
        gameState.SavedAt = DateTime.UtcNow;

        var configuration = ResolveConfiguration(gameState.Configuration);
        var existing = _context.GameStates.FirstOrDefault(g => g.Id == id);
        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(gameState);
            _context.Entry(existing)
                .Property("ConfigurationId")
                .CurrentValue = configuration.Id;
        }
        else
        {
            gameState.Configuration = configuration;
            _context.GameStates.Add(gameState);
        }
        _context.SaveChanges();
    }

    private GameConfiguration ResolveConfiguration(GameConfiguration configuration)
    {
        var tracked = _context.Configurations.Local
            .FirstOrDefault(existing => existing.Id == configuration.Id);
        if (tracked is not null)
        {
            return tracked;
        }

        var stored = _context.Configurations
            .FirstOrDefault(existing => existing.Id == configuration.Id);
        if (stored is not null)
        {
            return stored;
        }

        _context.Configurations.Add(configuration);
        return configuration;
    }

    public (GameState State, string Name, DateTime SavedAt) Get(Guid id)
    {
        var state = _context.GameStates
                        .AsNoTracking()
                        .Include(g => g.Configuration)
                        .FirstOrDefault(g => g.Id == id)
                    ?? throw new FileNotFoundException($"Game save '{id}' not found.");

        return (state, state.Name, state.SavedAt);
    }
    public List<(Guid Id, string Name, DateTime SavedAt)> List()
    {
        return _context.GameStates
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new ValueTuple<Guid, string, DateTime>(g.Id, g.Name, g.SavedAt))
            .ToList();
    }

    public void Delete(Guid id)
    {
        var existing = _context.GameStates.FirstOrDefault(g => g.Id == id);
        if (existing != null)
        {
            _context.GameStates.Remove(existing);
            _context.SaveChanges();
        }
    }
}