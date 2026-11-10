using Game;

namespace ConsoleApp;

public static class GameUi
{
    public static void DrawGameBoard(EGameCellState[][] board)
    {
        if (board.Length == 0)
        {
            return;
        }

        var width = board.Length;
        var height = board[0].Length;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var cellState = board[x][y];
                char displayChar = cellState switch
                {
                    EGameCellState.Empty => '.',
                    EGameCellState.Player1 => 'X',
                    EGameCellState.Player2 => 'O',
                    _ => throw new ArgumentOutOfRangeException()
                };
                Console.Write(displayChar + " ");
            }

            Console.WriteLine();
        }
    }
}