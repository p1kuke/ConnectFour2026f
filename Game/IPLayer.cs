namespace Game;

public interface IPlayer
{
    string Name { get; }
    EGameCellState PlayerSymbol { get;}
    (int x, int y) GetMove(HashSet<(int x, int y)> allowedMoves);
    
}