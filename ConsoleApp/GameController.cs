using Game;

namespace ConsoleApp;

public class GameController
{
    private readonly IGameRepository _gameRepository;

    public GameController(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public string RunGame(
        GameBrain gameBrain,
        Guid gameId,
        string saveName,
        EGameMode? requestedMode = null)
    {
        ArgumentNullException.ThrowIfNull(gameBrain);
        var state = gameBrain.GetState();

        if (requestedMode.HasValue)
        {
            state.GameMode = requestedMode.Value;
            ApplyPlayerTypes(state, requestedMode.Value);
        }
        else if (state.GameMode != EGameMode.HumanVsHuman &&
                 state.Player1Type == EPlayerType.Human &&
                 state.Player2Type == EPlayerType.Human)
        {
            // Compatibility with saves created before player types were stored.
            ApplyPlayerTypes(state, state.GameMode);
        }

        var players = new IMoveProvider[]
        {
            CreateProvider(state.Player1Type, EGameCellState.Player1),
            CreateProvider(state.Player2Type, EGameCellState.Player2)
        };

        var playerIndex = state.CurrentPlayer == EGameCellState.Player2 ? 1 : 0;
        using var cancellation = new CancellationTokenSource();

        if (state.GameMode == EGameMode.AiVsAi)
        {
            Console.WriteLine("AI vs AI: press Q to stop and save the game.");
        }

        while (true)
        {
            GameUi.DrawGameBoard(gameBrain.GetBoard(), state.MoveHistory.LastOrDefault());

            var currentPlayer = players[playerIndex];
            Console.WriteLine($"{currentPlayer.Name}'s turn.");

            (int X, int Y) move;
            try
            {
                move = currentPlayer.GetMove(gameBrain, cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                return PromptSave(gameId, saveName, state, "Game stopped. Save before exiting?");
            }

            if (move == (-1, -1))
            {
                return PromptSave(gameId, saveName, state, "Save game before exiting?");
            }

            gameBrain.MakeMove(move.X, move.Y, currentPlayer.PlayerSymbol);
            Console.WriteLine($"{currentPlayer.Name} made a move at ({move.X + 1}, {move.Y + 1}).");

            if (gameBrain.IsWinningMove(move.X, move.Y))
            {
                GameUi.DrawGameBoard(gameBrain.GetBoard(), state.MoveHistory.LastOrDefault());
                Console.WriteLine($"{currentPlayer.Name} won!");
                return "game over";
            }

            if (gameBrain.IsBoardFull())
            {
                GameUi.DrawGameBoard(gameBrain.GetBoard(), state.MoveHistory.LastOrDefault());
                Console.WriteLine("Draw!");
                return "game over";
            }

            // Switch first: if the user stops during the pause, the saved state must
            // already contain the player who is going to move next.
            gameBrain.SwitchPlayer();
            playerIndex = (playerIndex + 1) % players.Length;

            if (state.GameMode == EGameMode.AiVsAi && WaitForAiTurnOrStop(cancellation))
            {
                return PromptSave(gameId, saveName, state, "AI vs AI stopped. Save before exiting?");
            }
        }
    }

    private static void ApplyPlayerTypes(GameState state, EGameMode mode)
    {
        state.Player1Type = mode == EGameMode.AiVsAi ? EPlayerType.RandomAi : EPlayerType.Human;
        state.Player2Type = mode == EGameMode.HumanVsHuman ? EPlayerType.Human : EPlayerType.RandomAi;
    }

    private static IMoveProvider CreateProvider(EPlayerType playerType, EGameCellState symbol) =>
        playerType switch
        {
            EPlayerType.Human => new HumanPlayer(
                symbol == EGameCellState.Player1 ? "Player 1" : "Player 2",
                symbol),
            EPlayerType.RandomAi => new RandomAiPlayer(symbol),
            _ => throw new ArgumentOutOfRangeException(
                nameof(playerType),
                playerType,
                "Unknown player type.")
        };

    private static bool WaitForAiTurnOrStop(CancellationTokenSource cancellation)
    {
        const int delayMilliseconds = 500;
        const int checkIntervalMilliseconds = 50;

        for (var elapsed = 0; elapsed < delayMilliseconds; elapsed += checkIntervalMilliseconds)
        {
            if (!Console.IsInputRedirected &&
                Console.KeyAvailable &&
                Console.ReadKey(intercept: true).Key == ConsoleKey.Q)
            {
                cancellation.Cancel();
                return true;
            }

            Thread.Sleep(checkIntervalMilliseconds);
        }

        return false;
    }

    private string PromptSave(
        Guid gameId,
        string saveName,
        GameState state,
        string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (Y/N): ");
            var choice = Console.ReadLine()?.Trim();

            if (string.Equals(choice, "Y", StringComparison.OrdinalIgnoreCase))
            {
                _gameRepository.Save(gameId, saveName, state);
                Console.WriteLine($"\nGame '{saveName}' saved successfully!");
                return "game saved and exited";
            }

            if (string.Equals(choice, "N", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("\nGame was not saved.");
                return "game exited";
            }

            if (choice is null)
            {
                Console.WriteLine();
                return "game exited";
            }

            Console.WriteLine("Please enter Y or N.");
        }
    }
}
