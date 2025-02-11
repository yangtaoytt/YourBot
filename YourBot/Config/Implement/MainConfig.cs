using System.Text.Json;
using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement;

public class MainConfig : IReadConfig<MainConfig>, IWriteConfig {
    public readonly Dictionary<string, string> Configurations;

    private MainConfig(Dictionary<string, string> configurations) {
        Configurations = configurations;
    }

    public static List<string> GetConfigName() {
        return [];
    }

    public static MainConfig CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        var mainConfig = configurations["MainConfig"];
        return new MainConfig(
            mainConfig.AsEnumerable().ToDictionary(x => x.Key, x => x.Value!));
    }

    public void WriteToFile(string path) {
        File.WriteAllText(path, JsonSerializer.Serialize(Configurations, ConfigManager.Options));
    }

    public static string GetMainWriteConfigName() {
        return "";
    }
}