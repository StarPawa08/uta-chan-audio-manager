using System.IO;
using System.Text.Json;

namespace UtaChanManager.Utils;

public class ConfigManager
{
    private static string configPath = "user_config.json";

    public static void Save(List<string> priorities)
    {
        File.WriteAllText(configPath, JsonSerializer.Serialize(priorities));
    }
    
    public static List<string> Load()
    {
        if (!File.Exists(configPath)) return new();
        return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(configPath)) ?? new List<string>();
    }
}