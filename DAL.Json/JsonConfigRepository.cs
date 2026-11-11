using System.Text.Json;
using Game;

namespace DAL.Json;

public class JsonConfigRepository : IConfigRepository
{
    private readonly string _folderPath;
    private readonly JsonSerializerOptions _jsonOptions = GameJson.Options;

    public JsonConfigRepository(string folderPath)
    {
        _folderPath = folderPath;
        if (!Directory.Exists(_folderPath))
        {
            Directory.CreateDirectory(_folderPath);
        }
    }

    private string GetFilePath(string name)
    {
        // The file name is sanitized for the file system, but the configuration's
        // real name is kept inside the JSON. List() reads it back from there.
        var safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_folderPath, $"{safeName}.config.json");
    }

    public void Save(string name, GameConfiguration config)
    {
        var filePath = GetFilePath(name);
        var savedConfig = config with { Name = name };
        if (File.Exists(filePath))
        {
            var existing = JsonSerializer.Deserialize<GameConfiguration>(
                File.ReadAllText(filePath), _jsonOptions);
            if (existing is not null)
            {
                savedConfig = savedConfig with { Id = existing.Id };
            }
        }

        var jsonString = JsonSerializer.Serialize(savedConfig, _jsonOptions);
        File.WriteAllText(filePath, jsonString);
    }

    public GameConfiguration Get(string name)
    {
        var filePath = GetFilePath(name);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file standard not found: {filePath}");
        }

        var jsonString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<GameConfiguration>(jsonString, _jsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize configuration.");
    }

    public List<string> List()
    {
        if (!Directory.Exists(_folderPath)) return new List<string>();

        return Directory.GetFiles(_folderPath, "*.config.json")
            .Select(ReadName)
            .OfType<string>()
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name)
            .ToList();
    }

    private string? ReadName(string filePath)
    {
        try
        {
            var configuration = JsonSerializer.Deserialize<GameConfiguration>(
                File.ReadAllText(filePath), _jsonOptions);
            if (!string.IsNullOrWhiteSpace(configuration?.Name))
            {
                return configuration.Name;
            }
        }
        catch (JsonException)
        {
            // Fall back to the file name for a damaged or manually edited file.
        }

        return Path.GetFileName(filePath).Replace(".config.json", "");
    }

    public void Delete(string name)
    {
        var filePath = GetFilePath(name);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
