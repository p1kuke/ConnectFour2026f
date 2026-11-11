using DAL;
using DAL.Json;
using Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp;

public static class ApplicationCompositionRoot
{
    public static ServiceProvider Build(bool useDatabase)
    {
        var services = new ServiceCollection();
        services.AddScoped<ConsoleApplication>();

        if (useDatabase)
        {
            services.AddDbContext<AppDbContext>();
            services.AddScoped<IConfigRepository, EfConfigRepository>();
            services.AddScoped<IGameRepository, EfGameRepository>();
        }
        else
        {
            var dataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "ConnectFourData");
            services.AddScoped<IConfigRepository>(_ =>
                new JsonConfigRepository(Path.Combine(dataPath, "configs")));
            services.AddScoped<IGameRepository>(_ =>
                new JsonGameRepository(Path.Combine(dataPath, "games")));
        }

        var provider = services.BuildServiceProvider();
        if (useDatabase)
        {
            using var migrationScope = provider.CreateScope();
            var context = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();
            LegacyDatabaseUpgrader.UpgradeIfNeeded(context);
            context.Database.Migrate();
        }

        return provider;
    }
}
