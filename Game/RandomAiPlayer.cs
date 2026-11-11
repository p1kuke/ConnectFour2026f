namespace Game;

public class RandomAiPlayer : IPlayer
{
    private readonly Random _random;

    public RandomAiPlayer(EGameCellState playerSymbol, Random? random = null)
    {
        PlayerSymbol = playerSymbol;
        _random = random ?? new Random();
    }

    public string Name => "Random AI";
    public EGameCellState PlayerSymbol { get; }

    public (int x, int y) GetMove(HashSet<(int x, int y)> legalMoves)
    {
        ArgumentNullException.ThrowIfNull(legalMoves);
        if (legalMoves.Count == 0)
            throw new InvalidOperationException("Cannot choose a move when no legal moves are available.");

        return legalMoves.ElementAt(_random.Next(legalMoves.Count));
    }
}
