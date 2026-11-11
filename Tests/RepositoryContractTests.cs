using DAL;
using DAL.Json;
using Game;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests;

public sealed class RepositoryContractTests
{
    public static TheoryData<Func<IConfigRepository>> ConfigRepositoryFactories => new()
    {
        CreateJsonConfigRepository,
        CreateEfConfigRepository
    };

    public static TheoryData<Func<IGameRepository>> GameRepositoryFactories => new()
    {
        CreateJsonGameRepository,
        CreateEfGameRepository
    };

    [Theory]
    [MemberData(nameof(ConfigRepositoryFactories))]
    public void ConfigRepository_SaveAndGet_PreservesConfiguration(
        Func<IConfigRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());
        var configuration = new GameConfiguration("Test configuration", 8, 7, 4, true);

        repositoryScope.Config.Save(configuration.Name, configuration);

        var result = repositoryScope.Config.Get(configuration.Name);

        Assert.Equal(configuration.Id, result.Id);
        Assert.Equal(configuration.BoardWidth, result.BoardWidth);
        Assert.Equal(configuration.BoardHeight, result.BoardHeight);
        Assert.Equal(configuration.WinCondition, result.WinCondition);
        Assert.Equal(configuration.IsCylinder, result.IsCylinder);
    }

    [Theory]
    [MemberData(nameof(ConfigRepositoryFactories))]
    public void ConfigRepository_SaveWithSameName_UpdatesExistingConfiguration(
        Func<IConfigRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());
        var original = new GameConfiguration("Test configuration", 7, 6, 4);
        var updated = new GameConfiguration("Test configuration", 9, 8, 5, true);

        repositoryScope.Config.Save(original.Name, original);
        repositoryScope.Config.Save(updated.Name, updated);

        var result = repositoryScope.Config.Get(original.Name);

        Assert.Equal(1, repositoryScope.Config.List().Count(name => name == original.Name));
        Assert.Equal(original.Id, result.Id);
        Assert.Equal(updated.BoardWidth, result.BoardWidth);
        Assert.Equal(updated.BoardHeight, result.BoardHeight);
        Assert.Equal(updated.WinCondition, result.WinCondition);
        Assert.Equal(updated.IsCylinder, result.IsCylinder);
    }

    [Theory]
    [MemberData(nameof(ConfigRepositoryFactories))]
    public void ConfigRepository_GetMissing_ThrowsFileNotFoundException(
        Func<IConfigRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());

        Assert.Throws<FileNotFoundException>(
            () => repositoryScope.Config.Get("Missing configuration"));
    }

    [Theory]
    [MemberData(nameof(ConfigRepositoryFactories))]
    public void ConfigRepository_Delete_RemovesConfiguration(
        Func<IConfigRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());
        var configuration = new GameConfiguration("Test configuration", 7, 6, 4);

        repositoryScope.Config.Save(configuration.Name, configuration);
        repositoryScope.Config.Delete(configuration.Name);

        Assert.DoesNotContain(configuration.Name, repositoryScope.Config.List());
    }

    [Theory]
    [MemberData(nameof(ConfigRepositoryFactories))]
    public void ConfigRepository_List_IsSortedByName(
        Func<IConfigRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());
        repositoryScope.Config.Save("Zulu", new GameConfiguration("Zulu", 7, 6, 4));
        repositoryScope.Config.Save("Alpha", new GameConfiguration("Alpha", 7, 6, 4));

        Assert.Equal(["Alpha", "Zulu"], repositoryScope.Config.List());
    }

    [Theory]
    [MemberData(nameof(ConfigRepositoryFactories))]
    public void ConfigRepository_PreservesNamesWithPathSeparators(
        Func<IConfigRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());
        var configuration = new GameConfiguration("Custom / config", 8, 7, 4);

        repositoryScope.Config.Save(configuration.Name, configuration);

        Assert.Contains(configuration.Name, repositoryScope.Config.List());
        Assert.Equal(configuration.Name, repositoryScope.Config.Get(configuration.Name).Name);
    }

    [Theory]
    [MemberData(nameof(GameRepositoryFactories))]
    public void GameRepository_SaveAndGet_PreservesStateAndStableId(
        Func<IGameRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());
        var id = Guid.NewGuid();
        var configuration = new GameConfiguration("Test configuration", 7, 6, 4);
        var state = new GameState(configuration.BoardWidth, configuration.BoardHeight)
        {
            Configuration = configuration,
            CurrentPlayer = EGameCellState.Player2,
            GameMode = EGameMode.HumanVsAi,
            Player1Type = EPlayerType.Human,
            Player2Type = EPlayerType.RandomAi
        };
        state.MakeMove(2, 0, EGameCellState.Player1);

        repositoryScope.Game.Save(id, "Test save", state);

        var result = repositoryScope.Game.Get(id);

        Assert.Equal(id, result.State.Id);
        Assert.Equal(configuration.Id, result.State.Configuration.Id);
        Assert.Equal("Test save", result.Name);
        Assert.Equal(EGameCellState.Player2, result.State.CurrentPlayer);
        Assert.Equal(EGameCellState.Player1, result.State.GetCellState(2, 0));
        Assert.Equal(EGameMode.HumanVsAi, result.State.GameMode);
        Assert.Equal(EPlayerType.Human, result.State.Player1Type);
        Assert.Equal(EPlayerType.RandomAi, result.State.Player2Type);
    }

    [Theory]
    [MemberData(nameof(GameRepositoryFactories))]
    public void GameRepository_SaveWithSameId_UpdatesExistingGame(
        Func<IGameRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());
        var id = Guid.NewGuid();
        var configuration = new GameConfiguration("Test configuration", 7, 6, 4);
        var original = new GameState(configuration.BoardWidth, configuration.BoardHeight)
        {
            Configuration = configuration
        };
        var updated = new GameState(configuration.BoardWidth, configuration.BoardHeight)
        {
            Configuration = configuration,
            CurrentPlayer = EGameCellState.Player2
        };

        repositoryScope.Game.Save(id, "Original name", original);
        repositoryScope.Game.Save(id, "Updated name", updated);

        var result = repositoryScope.Game.Get(id);

        Assert.Single(repositoryScope.Game.List());
        Assert.Equal("Updated name", result.Name);
        Assert.Equal(EGameCellState.Player2, result.State.CurrentPlayer);
    }

    [Theory]
    [MemberData(nameof(GameRepositoryFactories))]
    public void GameRepository_SaveWithSameId_UpdatesConfiguration(
        Func<IGameRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());
        var id = Guid.NewGuid();
        var first = new GameConfiguration("First configuration", 7, 6, 4);
        var second = new GameConfiguration("Second configuration", 9, 8, 5, true);
        var original = new GameState(first.BoardWidth, first.BoardHeight)
        {
            Configuration = first
        };
        var updated = new GameState(second.BoardWidth, second.BoardHeight)
        {
            Configuration = second
        };

        repositoryScope.Game.Save(id, "Test save", original);
        repositoryScope.Game.Save(id, "Test save", updated);

        var result = repositoryScope.Game.Get(id);

        Assert.Equal(second.Id, result.State.Configuration.Id);
        Assert.Equal(second.Name, result.State.Configuration.Name);
        Assert.Equal(second.BoardWidth, result.State.Configuration.BoardWidth);
        Assert.Equal(second.BoardHeight, result.State.Configuration.BoardHeight);
        Assert.Equal(second.IsCylinder, result.State.Configuration.IsCylinder);
    }

    [Theory]
    [MemberData(nameof(GameRepositoryFactories))]
    public void GameRepository_GetMissing_ThrowsFileNotFoundException(
        Func<IGameRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());

        Assert.Throws<FileNotFoundException>(
            () => repositoryScope.Game.Get(Guid.NewGuid()));
    }

    [Theory]
    [MemberData(nameof(GameRepositoryFactories))]
    public void GameRepository_Delete_RemovesGame(
        Func<IGameRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());
        var id = Guid.NewGuid();
        var configuration = new GameConfiguration("Test configuration", 7, 6, 4);
        var state = new GameState
        {
            Configuration = configuration
        };

        repositoryScope.Game.Save(id, "Test save", state);
        repositoryScope.Game.Delete(id);

        Assert.Empty(repositoryScope.Game.List());
    }

    [Theory]
    [MemberData(nameof(GameRepositoryFactories))]
    public void GameRepository_List_IsSortedByName(
        Func<IGameRepository> createRepository)
    {
        var repositoryScope = new RepositoryScope(createRepository());
        var configuration = new GameConfiguration("Test configuration", 7, 6, 4);

        repositoryScope.Game.Save(
            Guid.NewGuid(),
            "Zulu",
            new GameState { Configuration = configuration });
        repositoryScope.Game.Save(
            Guid.NewGuid(),
            "Alpha",
            new GameState { Configuration = configuration });

        Assert.Equal(["Alpha", "Zulu"], repositoryScope.Game.List().Select(game => game.Name));
    }

    private static IConfigRepository CreateJsonConfigRepository()
    {
        return new JsonConfigRepository(CreateTemporaryDirectory());
    }

    private static IConfigRepository CreateEfConfigRepository()
    {
        return new EfConfigRepository(CreateEfContext());
    }

    private static IGameRepository CreateJsonGameRepository()
    {
        return new JsonGameRepository(CreateTemporaryDirectory());
    }

    private static IGameRepository CreateEfGameRepository()
    {
        return new EfGameRepository(CreateEfContext());
    }

    private static AppDbContext CreateEfContext()
    {
        var databasePath = Path.Combine(CreateTemporaryDirectory(), "test.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;
        var context = new AppDbContext(options);
        context.Database.Migrate();
        return context;
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ConnectFourTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class RepositoryScope
    {
        public RepositoryScope(IConfigRepository repository)
        {
            Config = repository;
        }

        public RepositoryScope(IGameRepository repository)
        {
            Game = repository;
        }

        public IConfigRepository Config { get; } = default!;
        public IGameRepository Game { get; } = default!;
    }
}
