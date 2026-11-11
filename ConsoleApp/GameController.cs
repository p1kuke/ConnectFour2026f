using Game;

namespace ConsoleApp;

public enum GameMode
{
    HumanVsHuman,
    HumanVsAi,
    AiVsAi
}

public class GameController
{
    private readonly IGameRepository _gameRepository;

    public GameController(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public string RunGame(GameBrain gameBrain, Guid gameId, string saveName,
        GameMode mode = GameMode.HumanVsHuman)
    {
        var players = mode switch
        {
            GameMode.HumanVsHuman => new IPlayer[]
            {
                new HumanPlayer("Player 1", EGameCellState.Player1),
                new HumanPlayer("Player 2", EGameCellState.Player2)
            },
            GameMode.AiVsAi => new IPlayer[]
            {
                new AIPlayer(EGameCellState.Player1),
                new AIPlayer(EGameCellState.Player2)
            },
            _ => new IPlayer[]
            {
                new HumanPlayer("Player 1", EGameCellState.Player1),
                new AIPlayer(EGameCellState.Player2)
            }
        };

        var playerIndex = Array.FindIndex(players, p => p.PlayerSymbol == gameBrain.CurrentPlayer);
        if (playerIndex == -1) playerIndex = 0;

        while (true)
        {
            GameUi.DrawGameBoard(gameBrain.GetBoard(), gameBrain.GetState().MoveHistory.LastOrDefault());

            var currentPlayer = players[playerIndex];
            var move = currentPlayer.GetMove(gameBrain.GetEmptyCellCoordinates());

            // Intercept the exit key.
            if (move == (-1, -1))
            {
                while (true)
                {
                    Console.Write("Save game before exiting? (Y/N): ");
                    var choice = Console.ReadLine()?.Trim();

                    if (string.Equals(choice, "Y", StringComparison.OrdinalIgnoreCase))
                    {
                        _gameRepository.Save(gameId, saveName, gameBrain.GetState());
                        Console.WriteLine($"\nGame '{saveName}' saved successfully!");
                        return "game saved and exited";
                    }

                    if (string.Equals(choice, "N", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("\nGame was not saved.");
                        return "game exited";
                    }

                    Console.WriteLine("Please enter Y or N.");
                }
            }

            // Execute a valid move
            gameBrain.MakeMove(move.x, move.y, currentPlayer.PlayerSymbol);
            Console.WriteLine($"{currentPlayer.Name} made a move at ({move.x + 1}, {move.y + 1})");

            if (gameBrain.IsWinningMove(move.x, move.y))
            {
                GameUi.DrawGameBoard(gameBrain.GetBoard(), gameBrain.GetState().MoveHistory.LastOrDefault());
                Console.WriteLine($"{currentPlayer.Name} won!");
                break;
            }

            if (gameBrain.IsBoardFull())
            {
                GameUi.DrawGameBoard(gameBrain.GetBoard(), gameBrain.GetState().MoveHistory.LastOrDefault());
                Console.WriteLine("Draw!");
                break;
            }

            // Switch player
            gameBrain.SwitchPlayer();
            playerIndex = (playerIndex + 1) % players.Length;
        }

        return "game over";
    }
}