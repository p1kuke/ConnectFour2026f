using Game;

namespace ConsoleApp;

public static class GameUi
{
    public static void DrawGameBoard(EGameCellState[][] board, GameMove? lastMove = null)
    {
        if (board.Length == 0)
        {
            return;
        }

        var width = board.Length;
        var height = board[0].Length;
        var cellWidth = Math.Max(3, width.ToString().Length + 2);
        var separator = "+" + string.Join("+", Enumerable.Repeat(new string('-', cellWidth), width)) + "+";

        for (var y = 0; y < height; y++)
        {
            Console.Write("|");
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
                var isLastMove = lastMove is not null &&
                                 lastMove.X == x &&
                                 lastMove.Y == y;
                var leftPadding = (cellWidth - 1) / 2;
                var rightPadding = cellWidth - leftPadding - 1;

                Console.Write(new string(' ', leftPadding));
                if (isLastMove)
                {
                    var previousBackground = Console.BackgroundColor;
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.Write(displayChar);
                    Console.BackgroundColor = previousBackground;
                }
                else
                {
                    Console.Write(displayChar);
                }

                Console.Write(new string(' ', rightPadding));
                Console.Write("|");
            }

            Console.WriteLine();
        }

        Console.WriteLine(separator);
        Console.Write("|");
        for (var x = 0; x < width; x++)
        {
            var columnNumber = (x + 1).ToString();
            var numberLeftPadding = (cellWidth - columnNumber.Length) / 2;
            var numberRightPadding = cellWidth - numberLeftPadding - columnNumber.Length;
            Console.Write(
                $"{new string(' ', numberLeftPadding)}{columnNumber}{new string(' ', numberRightPadding)}|");
        }

        Console.WriteLine();
    }
}