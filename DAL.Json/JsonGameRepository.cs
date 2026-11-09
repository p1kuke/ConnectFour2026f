using System.Text.Json;
using Game;

namespace DAL.Json;

public class JsonGameRepository : IGameRepository
{
    private readonly string _folderPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonGameRepository(string folderPath)
    {
        _folderPath = folderPath;
        if (!Directory.Exists(_folderPath))
        {
            Directory.CreateDirectory(_folderPath);
        }
    }

    private string GetFilePath(Guid id) => Path.Combine(_folderPath, $"{id}.save.json");

    public void Save(Guid id, string name, GameState gameState)
    {
        gameState.Id = id;
        gameState.Name = name;
        gameState.SavedAt = DateTime.UtcNow;

        var filePath = GetFilePath(id);
        var jsonString = JsonSerializer.Serialize(gameState, _jsonOptions);
        File.WriteAllText(filePath, jsonString);
    }

    public void Save(Guid id, string name, Game.GameState gameState)
    {
        throw new NotImplementedException();
    }

    public (GameState State, string Name, DateTime SavedAt) Get(Guid id)
    {
        var filePath = GetFilePath(id);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Save file not found: {filePath}");
        }

        var jsonString = File.ReadAllText(filePath);
        var state = JsonSerializer.Deserialize<GameState>(jsonString, _jsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize game state.");

        return (state, state.Name, state.SavedAt);
    }

    public List<(Guid Id, string Name, DateTime SavedAt)> List()
    {
        if (!Directory.Exists(_folderPath)) 
        {
            return new List<(Guid Id, string Name, DateTime SavedAt)>();
        }

        return Directory.GetFiles(_folderPath, "*.save.json")
            .Select(filePath => 
            {
                var jsonString = File.ReadAllText(filePath);
                var state = JsonSerializer.Deserialize<GameState>(jsonString, _jsonOptions);
                return (state!.Id, state.Name, state.SavedAt);
            })
            .ToList();
    }

    public void Delete(Guid id)
    {
        var filePath = GetFilePath(id);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}