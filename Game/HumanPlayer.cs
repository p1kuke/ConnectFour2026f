namespace Game;

public class HumanPlayer : IPlayer
{
    private readonly string _name;
    private readonly EGameCellState _playerSymbol;

    public HumanPlayer(string name, EGameCellState playerSymbol)
    {
        _name = name;
        _playerSymbol = playerSymbol;
    }

    public string Name => _name;
    public EGameCellState PlayerSymbol => _playerSymbol;

    public (int x, int y) GetMove(HashSet<(int x, int y)> legalMoves)
    {
        while (true)
        {
            Console.Write($"{Name}, enter column number (1-based) or 'S' to save: ");
            var input = Console.ReadLine()?.Trim();

            if (string.Equals(input, "S", StringComparison.OrdinalIgnoreCase))
            {
                return (-1, -1);
            }

            if (!int.TryParse(input, out var column))
            {
                Console.WriteLine("Please enter a valid number or 'S' to save.");
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