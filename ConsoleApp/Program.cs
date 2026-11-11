using ConsoleApp;
using Microsoft.Extensions.DependencyInjection;

const bool useDatabase = true;

using var services = ApplicationCompositionRoot.Build(useDatabase);
using var scope = services.CreateScope();
scope.ServiceProvider.GetRequiredService<ConsoleApplication>().Run();
