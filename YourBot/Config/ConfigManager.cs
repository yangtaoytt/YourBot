using System.Text.Json;
using Microsoft.Extensions.Configuration;
using YourBot.Config.Implement;
using YourBot.Config.Interface;

namespace YourBot.Config;

public class ConfigManager {
    public static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true
    };

    private readonly string _mainConfigPath;

    public ConfigManager(string mainConfigPath) {
        _mainConfigPath = mainConfigPath;
        var dic = new Dictionary<string, IConfigurationRoot> { { "MainConfig", Init() } };
        MainConfig = MainConfig.CreateConfig(dic);
    }

    public MainConfig MainConfig { get; }

    private IConfigurationRoot Init() {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(_mainConfigPath, false, false)
            .Build();
    }

    public TConfig CreateConfig<TConfig>() where TConfig : IReadConfig<TConfig> {
        var configName = TConfig.GetConfigName();
        var configurations = new Dictionary<string, IConfigurationRoot>();
        foreach (var name in configName) {
            var configurationPath = MainConfig.Configurations[name];
            if (configurationPath != "") {
                configurations[name] = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(configurationPath, false, false)
                    .Build();
            } else {
                configurations[name] = new ConfigurationBuilder().Build();
            }
        }

        return TConfig.CreateConfig(configurations);
    }

    public void WriteConfig<TConfig>(TConfig config) where TConfig : IWriteConfig {
        var configName = TConfig.GetMainWriteConfigName();
        var configurationPath = MainConfig.Configurations[configName];
        config.WriteToFile(configurationPath);
    }

    public void WriteMainConfig() {
        MainConfig.WriteToFile(_mainConfigPath);
    }
}