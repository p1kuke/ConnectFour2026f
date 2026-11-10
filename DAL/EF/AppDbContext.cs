using Game;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class AppDbContext : DbContext
{
    public DbSet<GameConfiguration> Configurations { get; set; } = default!;
    public DbSet<GameState> GameStates { get; set; } = default!;

    private readonly string _dbPath;

    public AppDbContext()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            "ConnectFourData"
        );
        Directory.CreateDirectory(folder);
        _dbPath = Path.Combine(folder, "connectfour.db");
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        _dbPath = string.Empty;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GameConfiguration>().HasKey(c => c.Id);
        modelBuilder.Entity<GameState>().HasKey(g => g.Id);
        modelBuilder.Entity<GameState>()
            .HasOne(g => g.Configuration)
            .WithMany()
            .HasForeignKey("ConfigurationId")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Ignore the jagged Board array because EF Core will persist it through BoardJson
        modelBuilder.Entity<GameState>().Ignore(g => g.Board);
    }
}