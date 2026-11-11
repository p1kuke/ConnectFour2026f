namespace Game;

/// <summary>
/// Source of moves for one player. Implementations must not mutate the supplied game.
/// </summary>
public interface IMoveProvider
{
    string Name { get; }

    EGameCellState PlayerSymbol { get; }

    (int X, int Y) GetMove(GameBrain game, CancellationToken cancellationToken = default);
}
