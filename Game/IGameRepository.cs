namespace Game;

public interface IGameRepository
{
    void Save(Guid id, string name, GameState gameState);
    
    (global::GameState State, string Name, DateTime SavedAt) Get(Guid id);
    
    List<(Guid Id, string Name, DateTime SavedAt)> List();
    
    void Delete(Guid id);
}