# Connect Four

## Student information

- Full name: KIRILL JANOVSKI
- Student code: 244451IADB
- School email: kijano@taltech.ee
- Uni-ID: kijano
- Assigned game: Connect Four

## Requirements and implemented extensions

The project targets .NET 10 and C# 14. It contains a reusable `MenuSystem`, a
game engine, JSON and EF Core/SQLite persistence, and automated tests. The
Connect Four engine supports configurable board sizes, configurable winning
length, and the cylindrical-board extension. Human-vs-human hot-seat play and
AI play are available from the console application.

## Running

```bash
dotnet build
dotnet test
dotnet run --project ConsoleApp/ConsoleApp.csproj
```

The application uses SQLite persistence by default (`useDatabase = true` in
`ConsoleApp/Program.cs`). Both stores use `~/ConnectFourData`; the SQLite
database is `connectfour.db`, while JSON data is stored in the `configs` and
`games` subdirectories. Set `useDatabase` to `false` to use JSON instead.

When database persistence is selected, the application calls
`Database.Migrate()` at startup. A fresh clone therefore needs no manual
database setup or `EnsureCreated()` call.

## AI usage log

AI was used to review the repository structure, suggest test cases, and identify
EF Core and repository-contract issues. Suggestions were checked against the
course requirements and the code was adapted manually, including persistence
paths, migrations, repository behaviour, and console flow. Incorrect or
incomplete suggestions were not accepted without compilation and test
verification.
