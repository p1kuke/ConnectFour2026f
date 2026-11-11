using ConsoleApp;
using Microsoft.Extensions.DependencyInjection;

var useDatabase = true;
foreach (var argument in args)
{
    switch (argument.ToLowerInvariant())
    {
        case "--db":
            useDatabase = true;
            break;
        case "--json":
            useDatabase = false;
            break;
        case "--help" or "-h":
            Console.WriteLine("Usage: dotnet run --project ConsoleApp/ConsoleApp.csproj [--db | --json]");
            Console.WriteLine("  --db    use EF Core/SQLite persistence (default)");
            Console.WriteLine("  --json  use JSON file persistence");
            return;
        default:
            Console.WriteLine($"Unknown argument '{argument}'. Use --help for usage.");
            return;
    }
}

using var services = ApplicationCompositionRoot.Build(useDatabase);
using var scope = services.CreateScope();
scope.ServiceProvider.GetRequiredService<ConsoleApplication>().Run();
