using DAL;
using Game;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests;

public sealed class LegacyDatabaseUpgraderTests
{
    [Fact]
    public void LegacyDatabase_IsUpgraded_AndExistingDataIsPreserved()
    {
        var databasePath = Path.Combine(CreateTemporaryDirectory(), "legacy.db");
        CreateLegacyDatabase(databasePath);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;
        using var context = new AppDbContext(options);

        LegacyDatabaseUpgrader.UpgradeIfNeeded(context);
        context.Database.Migrate();

        var configRepository = new EfConfigRepository(context);
        var gameRepository = new EfGameRepository(context);

        var configuration = configRepository.Get("Legacy configuration");
        var game = gameRepository.Get(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        Assert.Equal(8, configuration.BoardWidth);
        Assert.Equal("Legacy game", game.Name);
        Assert.Equal(configuration.Id, game.State.Configuration.Id);
        Assert.Equal(EGameCellState.Player1, game.State.GetCellState(0, 1));

        var newConfiguration = new GameConfiguration("New configuration", 9, 8, 5, true);
        configRepository.Save(newConfiguration.Name, newConfiguration);
        Assert.Equal(newConfiguration.Id, configRepository.Get(newConfiguration.Name).Id);
    }

    private static void CreateLegacyDatabase(string databasePath)
    {
        using var connection = new SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE "Configurations" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Configurations" PRIMARY KEY,
                "Name" TEXT NOT NULL,
                "BoardWidth" INTEGER NOT NULL,
                "BoardHeight" INTEGER NOT NULL,
                "WinCondition" INTEGER NOT NULL,
                "IsCylinder" INTEGER NOT NULL
            );

            CREATE TABLE "GameStates" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_GameStates" PRIMARY KEY,
                "WinningLength" INTEGER NOT NULL,
                "CurrentPlayer" INTEGER NOT NULL,
                "ConfigurationId" TEXT NOT NULL,
                "SavedAt" TEXT NOT NULL,
                "BoardJson" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                CONSTRAINT "FK_GameStates_Configurations_ConfigurationId"
                    FOREIGN KEY ("ConfigurationId") REFERENCES "Configurations" ("Id")
                    ON DELETE CASCADE
            );

            CREATE TABLE "__EFMigrationsHistory" (
                "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                "ProductVersion" TEXT NOT NULL
            );

            INSERT INTO "__EFMigrationsHistory" VALUES
                ('20261110174211_InitialCreate', '10.0.15');

            INSERT INTO "Configurations" VALUES
                ('11111111-1111-1111-1111-111111111111',
                 'Legacy configuration', 8, 7, 4, 0);

            INSERT INTO "GameStates" VALUES
                ('22222222-2222-2222-2222-222222222222',
                 4, 2, '11111111-1111-1111-1111-111111111111',
                 '2026-11-11 23:48:23',
                 '[[0,1,0,0,0,0],[0,0,0,0,0,0],[0,0,0,0,0,0],[0,0,0,0,0,0],[0,0,0,0,0,0],[0,0,0,0,0,0],[0,0,0,0,0,0]]',
                 'Legacy game');
            """;
        command.ExecuteNonQuery();
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ConnectFourTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }
}
