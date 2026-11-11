namespace Game;

public interface IConfigRepository
{
    void Save(string name, GameConfiguration config);

    GameConfiguration Get(string name);

    List<string> List();

    void Delete(string name);
}
