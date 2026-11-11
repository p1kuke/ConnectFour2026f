# Connect Four

## Student information

- Full name: KIRILL JANOVSKI
- Student code: 244451IADB
- School email: kijano@taltech.ee
- Uni-ID: kijano
- Assigned game: Connect Four

## Project structure

| Project | Purpose |
| --- | --- |
| `MenuSystem` | Reusable three-level console menu library. |
| `MenuDemo` | Demo application for `MenuSystem` with dynamic menu titles. |
| `Game` | Game engine, configuration presets, move providers, repository contracts. |
| `DAL.Json` | JSON implementations of the repository contracts. |
| `DAL` | EF Core/SQLite implementations, `AppDbContext`, and migrations. |
| `ConsoleApp` | Connect Four console application and composition root. |
| `Tests` | xUnit tests for the engine, AI, menu, and both repositories. |

The project targets .NET 10 and C# 14. Nullable reference types are enabled and
warnings are treated as errors.

## Implemented requirements

### A1 - Menu system

`MenuSystem` is a standalone class library. It supports main, sub-, and
sub-sub-menu levels; adds mandatory `E` (exit), `P` (previous menu), and `M`
(main menu) entries automatically; and keeps running at the correct level after
returning from a child menu. Menu items can use a static title or a
`TitleProvider` delegate for dynamic titles.

`MenuDemo` demonstrates all three levels and a dynamic
`Persistence: JSON/DB` item that can be toggled at every level:

```bash
dotnet run --project MenuDemo/MenuDemo.csproj
```

Menu construction is validated immediately: shortcuts and titles cannot be
empty, duplicates are rejected, and the reserved navigation shortcuts
`E`, `M`, and `P` cannot be assigned to regular items.

### A2 - Game engine

The engine supports arbitrary board dimensions, configurable winning length,
and rectangular or cylindrical boards. Horizontal, vertical, and both diagonal
directions are checked. Cylinder lines wrap across the left and right edges and
are counted as distinct cells, so a line cannot be counted twice by wrapping
around a narrow board.

Moves are validated in two ways: the target cell must be the lowest empty cell
of its column, and the move must be made by the player whose turn it is. Every
move is appended to the move history.

### A3 - Persistence

Both JSON and EF Core/SQLite repositories implement the same contracts
(`IConfigRepository`, `IGameRepository`) declared in `Game`. The EF
implementation includes an initial migration and applies it with
`Database.Migrate()` at startup. Configuration names have a unique database
index, and updating an existing saved game also updates its configuration
reference correctly.

### A4 - Console application

The console application provides:

- new games with a selected configuration and one of three modes:
  human vs human, human vs AI, or AI vs AI;
- loading saved games, with the configuration name, game mode, and save time
  shown in the list;
- deleting saved games;
- configuration management: list, create, edit, and delete;
- protection for built-in presets: presets cannot be edited, overwritten, or
  deleted, and missing presets are restored on startup.

During an AI-vs-AI game, press `Q` to stop and save. The game state is saved
with the player who would move next, so the game can be resumed correctly.
During a human turn, `S` stops the game and offers to save it.

### A5 - AI player

`RandomAiPlayer` chooses uniformly from the currently legal moves through the
`IMoveProvider` abstraction. The tests run it against every preset with many
random seeds, verify that AI-vs-AI games reach a terminal state, and check
deterministic behaviour and cancellation.

## Running

```bash
dotnet build
dotnet test
dotnet run --project ConsoleApp/ConsoleApp.csproj -- --json
dotnet run --project ConsoleApp/ConsoleApp.csproj -- --db
dotnet run --project MenuDemo/MenuDemo.csproj
```

`--db` uses EF Core/SQLite and is the default. `--json` uses JSON files. The
application stores data in `~/ConnectFourData`: the SQLite database is
`connectfour.db`, and JSON data is kept in the `configs` and `games`
subdirectories. Databases created before the migration was committed are
upgraded in place at startup: missing columns and the configuration-name index
are added and the initial migration is recorded, so existing saves are kept.

## Tests

The xUnit suite covers:

- win detection in all directions, including cylinder edge wrapping and narrow
  boards;
- move validation, turn validation, move history, and draw detection;
- AI legality for every preset and seed, determinism, cancellation, and
  AI-vs-AI termination;
- menu navigation (`P`, `M`, `E`) and menu construction validation;
- a shared repository contract for JSON and EF Core: save/get, update,
  delete, sorting, missing saves, and changing a saved game's configuration.

## AI usage log

AI assistance was used during development in the following areas:

- reviewing the repository against the A1-A5 and D1 requirements and listing
  missing contracts, build blockers, and behavioural bugs;
- analysing the menu navigation bug where `P` from the third level skipped to
  the main menu, and proposing the corrected return-value handling;
- suggesting additional test cases for cylinder line counting, wrong-player
  moves, AI legality, and repository contract parity;
- diagnosing the EF Core migration problem caused by a model snapshot without
  migration classes;
- reviewing the console flow for stopping and saving AI-vs-AI games;
- drafting and reviewing this README.

All AI suggestions were treated as proposals: they were checked against the
course requirements, compiled, and verified with `dotnet test`. Incomplete or
incorrect suggestions were corrected or rejected. The final implementation,
test results, and commit history were reviewed manually.
