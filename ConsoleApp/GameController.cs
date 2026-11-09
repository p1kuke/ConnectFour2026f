using Game;

namespace ConsoleApp;

public class GameController
{
    private readonly IGameRepository _gameRepository;

    public GameController(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public string RunGame(GameBrain gameBrain, Guid gameId, string saveName)
    {
        var players = new IPlayer[]
        {
            new HumanPlayer("Player 1", EGameCellState.Player1),
            new AIPlayer(EGameCellState.Player2),
        };

        var playerIndex = Array.FindIndex(players, p => p.PlayerSymbol == gameBrain.CurrentPlayer);
        if (playerIndex == -1) playerIndex = 0;

        while (true)
        {
            GameUi.DrawGameBoard(gameBrain.GetBoard());

            var currentPlayer = players[playerIndex];
            var move = currentPlayer.GetMove(gameBrain.GetEmptyCellCoordinates());

            // Intercept the save key 'S'
            if (move == (-1, -1))
            {
                _gameRepository.Save(gameId, saveName, gameBrain.GetState());
                Console.WriteLine($"\nGame '{saveName}' saved successfully!");

                Console.Write("Continue playing? (Y/N): ");
                var choice = Console.ReadLine()?.Trim().ToUpper();
    
                if (choice == "N")
                {
                    return "game saved and exited";
                }

                continue; 
            }

            // Execute a valid move
            gameBrain.MakeMove(move.x, move.y, currentPlayer.PlayerSymbol);
            Console.WriteLine($"{currentPlayer.Name} made a move at ({move.x + 1}, {move.y + 1})");

            if (gameBrain.IsWinningMove(move.x, move.y))
            {
                GameUi.DrawGameBoard(gameBrain.GetBoard());
                Console.WriteLine($"{currentPlayer.Name} won!");
                break;
            }

            if (gameBrain.IsBoardFull())
            {
                GameUi.DrawGameBoard(gameBrain.GetBoard());
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