namespace Game;

public class AIPlayer : IPlayer
{
    public string Name { get; } = "AIPlayer";
    private readonly Random _random = new();
    public EGameCellState PlayerSymbol { get; }

    public AIPlayer(EGameCellState playerSymbol)
    {
        PlayerSymbol = playerSymbol;
    }
    
    public (int x, int y) GetMove(HashSet<(int x, int y)> legalMoves)
    {
        return legalMoves.ToArray()[_random.Next(legalMoves.Count)];
    }

}