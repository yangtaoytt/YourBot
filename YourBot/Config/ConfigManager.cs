using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using YourBot.Config.Interface;

namespace YourBot.Config;

public class ConfigManager {
    private static readonly JsonSerializerSettings Settings = new() {
        MissingMemberHandling = MissingMemberHandling.Error,
        Converters = new List<JsonConverter> { new StringEnumConverter() }
    };

    private static readonly Dictionary<string, JObject> ConfigRegistry = [];

    private readonly JObject _mainConfig;
    private readonly string _mainConfigPath;
    

    public ConfigManager(string mainConfigPath) {
        _mainConfigPath = mainConfigPath;
        _mainConfig = JObject.Parse(File.ReadAllText(mainConfigPath));
    }

    public TConfig ReadConfig<TConfig>() where TConfig : IConfig {
        var configName = TConfig.GetConfigName();
        var configToken = _mainConfig[configName] ??
                          throw new ArgumentException($"config {configName} not found in main config.");
        var configJObject = GetConfigJObject(configToken);

        return JsonConvert.DeserializeObject<TConfig>(configJObject.ToString(), Settings)!;
    }

    public void WriteConfig<TConfig>(TConfig config) where TConfig : IConfig {
        var configName = TConfig.GetConfigName();
        var configToken = _mainConfig[configName] ??
                          throw new ArgumentException($"config {configName} not found in main config.");

        if (configToken.Type == JTokenType.String) {
            var configPath = (string)configToken!;

            var json = JsonConvert.SerializeObject(config, Formatting.Indented, Settings);
            var directory = Path.GetDirectoryName(configPath)!;
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(configPath, json);
        } else {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented, Settings);
            var newConfig = JObject.Parse(json);
            _mainConfig[configName] = newConfig;

            File.WriteAllText(_mainConfigPath, _mainConfig.ToString(Formatting.Indented));
        }
    }
    
    public static void SignDefaultConfig<TConfig>() where TConfig : IConfig, new() {
        var config = new TConfig();
        var configName = TConfig.GetConfigName();
        var configJObject = JObject.Parse(JsonConvert.SerializeObject(config, Formatting.Indented, Settings));
        ConfigRegistry[configName] = configJObject;
    }
    
    public static void GenerateDefaultConfigOnDisk(string path) {
        var mainConfig = new JObject();
        foreach (var (configName, configJObject) in ConfigRegistry) {
            mainConfig[configName] = configJObject;
        }

        if (!string.IsNullOrEmpty(Path.GetDirectoryName(path)) && !Directory.Exists(Path.GetDirectoryName(path))) {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        }
        File.WriteAllText(path, mainConfig.ToString(Formatting.Indented));
    }

    private static JObject GetConfigJObject(JToken configJToken) {
        return configJToken.Type == JTokenType.String
            ? JObject.Parse(File.ReadAllText((string)configJToken!))
            : (JObject)configJToken;
    }
}