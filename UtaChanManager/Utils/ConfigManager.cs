using System.IO;
using System.Text.Json;

namespace UtaChanManager.Utils;

public static class ConfigManager
{
    private const string ConfigPath = "user_config.json";

    public static void Save(List<string> priorities)
    {
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(priorities));
    }
    
    public static List<string> Load()
    {
        if (!File.Exists(ConfigPath)) return new();
        return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(ConfigPath)) ?? new List<string>();
    }
}