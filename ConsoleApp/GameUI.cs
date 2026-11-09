using Game;

namespace ConsoleApp;

public static class GameUi
{
    public static void DrawGameBoard(EGameCellState[][] board)
    {
        for (int x = 0; x < board.Length; x++)
        {
            for (int y = 0; y < board[x].Length; y++)
            {
                var cellState = board[x][y];
                char displayChar = cellState switch
                {
                    EGameCellState.Empty => '.',
                    EGameCellState.X => 'X',
                    EGameCellState.O => 'O',
                    _ => throw new ArgumentOutOfRangeException()
                };
                Console.Write(displayChar + " ");
            }
            Console.WriteLine();
        }
    }
}