namespace Game;

public class RandomAiPlayer : IMoveProvider
{
    private readonly Random _random;

    public RandomAiPlayer(EGameCellState playerSymbol, Random? random = null)
    {
        PlayerSymbol = playerSymbol;
        _random = random ?? new Random();
    }

    public string Name => $"Random AI ({PlayerSymbol})";
    public EGameCellState PlayerSymbol { get; }

    public (int X, int Y) GetMove(GameBrain game, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);
        cancellationToken.ThrowIfCancellationRequested();

        var legalMoves = game.GetEmptyCellCoordinates();
        if (legalMoves.Count == 0)
        {
            throw new InvalidOperationException("Cannot choose a move when no legal moves are available.");
        }

        return legalMoves.ElementAt(_random.Next(legalMoves.Count));
    }
}
