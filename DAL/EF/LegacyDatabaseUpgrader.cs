using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace DAL;

/// <summary>
/// Upgrades databases that were created by migration files which existed only in
/// an earlier working tree. Such databases already contain the tables, but have
/// no history entry for the committed InitialCreate migration. EF would then try
/// to create the existing tables again. This upgrader brings the schema to the
/// committed InitialCreate shape and records the migration as applied.
/// </summary>
public static class LegacyDatabaseUpgrader
{
    public static void UpgradeIfNeeded(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var connection = context.Database.GetDbConnection();
        var wasClosed = connection.State != ConnectionState.Open;
        if (wasClosed)
        {
            connection.Open();
        }

        try
        {
            if (!TableExists(connection, "Configurations"))
            {
                return;
            }

            var initialMigrationId = context.Database.GetMigrations()
                .FirstOrDefault(id => id.EndsWith("_InitialCreate", StringComparison.Ordinal));
            if (initialMigrationId is null)
            {
                return;
            }

            if (TableExists(connection, "__EFMigrationsHistory") &&
                MigrationRecorded(connection, initialMigrationId))
            {
                return;
            }

            if (TableExists(connection, "GameStates"))
            {
                UpgradeGameStateColumns(connection);
            }

            EnsureHistoryTable(connection);
            ExecuteNonQuery(
                connection,
                """
                INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                VALUES ($id, $version)
                """,
                ("$id", initialMigrationId),
                ("$version", ProductVersion));

            try
            {
                ExecuteNonQuery(
                    connection,
                    """
                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_Configurations_Name"
                    ON "Configurations" ("Name")
                    """);
            }
            catch (DbException)
            {
                // Old data may contain duplicate configuration names. The
                // application still prevents creating new duplicates, and the
                // data is kept instead of deleting configurations.
            }
        }
        finally
        {
            if (wasClosed)
            {
                connection.Close();
            }
        }
    }

    private static void UpgradeGameStateColumns(DbConnection connection)
    {
        var columns = GetColumns(connection, "GameStates");

        if (!columns.Contains("MoveHistoryJson"))
        {
            AddColumn(connection, "GameStates", "MoveHistoryJson", "TEXT NOT NULL DEFAULT ''");
        }

        if (!columns.Contains("GameMode"))
        {
            AddColumn(connection, "GameStates", "GameMode", "INTEGER NOT NULL DEFAULT 0");
        }

        if (!columns.Contains("Player1Type"))
        {
            AddColumn(connection, "GameStates", "Player1Type", "INTEGER NOT NULL DEFAULT 0");
        }

        if (!columns.Contains("Player2Type"))
        {
            AddColumn(connection, "GameStates", "Player2Type", "INTEGER NOT NULL DEFAULT 0");
        }

        if (!columns.Contains("CreatedAt"))
        {
            AddColumn(
                connection,
                "GameStates",
                "CreatedAt",
                "TEXT NOT NULL DEFAULT '0001-01-01 00:00:00'");
        }
    }

    private static bool MigrationRecorded(DbConnection connection, string migrationId)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = $id
            """;
        AddParameter(command, "$id", migrationId);
        return command.ExecuteScalar() is not null;
    }

    private static void EnsureHistoryTable(DbConnection connection)
    {
        ExecuteNonQuery(
            connection,
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                "ProductVersion" TEXT NOT NULL
            )
            """);
    }

    private static void AddColumn(
        DbConnection connection,
        string table,
        string column,
        string definition)
    {
        ExecuteNonQuery(connection, $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition}");
    }

    private static bool TableExists(DbConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $name
            """;
        AddParameter(command, "$name", table);
        return command.ExecuteScalar() is not null;
    }

    private static HashSet<string> GetColumns(DbConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{table}\")";

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static void ExecuteNonQuery(
        DbConnection connection,
        string sql,
        params (string Name, object Value)[] parameters)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            AddParameter(command, name, value);
        }

        command.ExecuteNonQuery();
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static string ProductVersion =>
        typeof(DbContext).Assembly.GetName().Version?.ToString(3) ?? "10.0.0";
}
