using Game;

namespace ConsoleApp;

public class GameController
{
    public string RunGame(GameBrain gameBrain)
    {
        var players = new IPlayer[]
        {
            new HumanPlayer("Player 1", EGameCellState.O),
            new AIPlayer(EGameCellState.X),
            //new AIPlayer(EGameCellState.O)
        };

        // TODO: this must be in state machine, not here
        var playerIndex = 0;

        while (true)
        {
            GameUi.DrawGameBoard(gameBrain.GetBoard());

            var currentPlayer = players[playerIndex];
            var move = currentPlayer.GetMove(gameBrain.GetEmptyCellCoordinates());
            gameBrain.MakeMove(move.x, move.y, currentPlayer.PlayerSymbol);
            Console.WriteLine($"{currentPlayer.Name} made a move at ({move.x}, {move.y})");

            if (gameBrain.IsWinningMove(move.x, move.y))
            {
                Console.WriteLine($"{currentPlayer.Name} won!");
                break;
            }

            if (gameBrain.IsBoardFull())
            {
                Console.WriteLine("Draw!");
                break;
            }

            playerIndex++;
            if (playerIndex >= players.Length)
            {
                playerIndex = 0;
            }
        }

        GameUi.DrawGameBoard(gameBrain.GetBoard());

        return "game over";
    }
}