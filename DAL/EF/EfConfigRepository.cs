using Game;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class EfConfigRepository : IConfigRepository
{
    private readonly AppDbContext _context;

    public EfConfigRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Save(string name, GameConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.Name = name;
        var existing = _context.Configurations.FirstOrDefault(c => c.Name == name);
        if (existing != null)
        {
            existing.Name = config.Name;
            existing.BoardWidth = config.BoardWidth;
            existing.BoardHeight = config.BoardHeight;
            existing.WinCondition = config.WinCondition;
            existing.IsCylinder = config.IsCylinder;
        }
        else
        {
            _context.Configurations.Add(config);
        }
        _context.SaveChanges();
    }

    public GameConfiguration Get(string name)
    {
        return _context.Configurations
                   .AsNoTracking()
                   .FirstOrDefault(c => c.Name == name)
               ?? throw new FileNotFoundException($"Configuration '{name}' not found in database.");
    }

    public List<string> List()
    {
        return _context.Configurations
            .AsNoTracking()
            .Select(c => c.Name)
            .OrderBy(name => name)
            .ToList();
    }

    public void Delete(string name)
    {
        var existing = _context.Configurations.FirstOrDefault(c => c.Name == name);
        if (existing != null)
        {
            _context.Configurations.Remove(existing);
            _context.SaveChanges();
        }
    }
}