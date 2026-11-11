using Game;

namespace ConsoleApp;

public sealed class HumanPlayer : IMoveProvider
{
    private readonly string _name;

    public HumanPlayer(string name, EGameCellState playerSymbol)
    {
        _name = name;
        PlayerSymbol = playerSymbol;
    }

    public string Name => _name;
    public EGameCellState PlayerSymbol { get; }

    public (int X, int Y) GetMove(GameBrain game, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);
        var legalMoves = game.GetEmptyCellCoordinates();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Console.Write($"{Name}, enter column number (1-based) or 'S' to exit: ");
            var input = Console.ReadLine()?.Trim();

            if (input is null)
            {
                Console.WriteLine();
                Console.WriteLine("Input was closed.");
                return (-1, -1);
            }

            if (string.Equals(input, "S", StringComparison.OrdinalIgnoreCase))
            {
                return (-1, -1);
            }

            if (!int.TryParse(input, out var column))
            {
                Console.WriteLine("Please enter a valid number or 'S' to exit.");
                continue;
            }

            var x = column - 1;

            foreach (var move in legalMoves)
            {
                if (move.x == x)
                {
                    return move;
                }
            }

            Console.WriteLine("Invalid column or the column is full. Try again.");
        }
    }
}
