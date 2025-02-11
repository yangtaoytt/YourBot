using System.Text.Json;
using Lagrange.Core.Common;
using Lagrange.Core.Utility.Sign;
using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement;

public class BotConfigAgentConfig : IReadConfig<BotConfigAgentConfig>, IWriteConfig {
    private readonly BotConfig _botConfig;

    public BotConfigAgentConfig(BotConfig botConfig) {
        _botConfig = botConfig;
    }

    public static List<string> GetConfigName() {
        return ["BotConfig"];
    }

    public static BotConfigAgentConfig CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new BotConfigAgentConfig(new BotConfig {
            Protocol = Enum.Parse<Protocols>(configurations["BotConfig"]["Protocol"]!),
            AutoReconnect = bool.Parse(configurations["BotConfig"]["AutoReconnect"]!),
            UseIPv6Network = bool.Parse(configurations["BotConfig"]["UseIPv6Network"]!),
            GetOptimumServer = bool.Parse(configurations["BotConfig"]["GetOptimumServer"]!),
            HighwayChunkSize = uint.Parse(configurations["BotConfig"]["HighwayChunkSize"]!),
            HighwayConcurrent = uint.Parse(configurations["BotConfig"]["HighwayConcurrent"]!),
            AutoReLogin = bool.Parse(configurations["BotConfig"]["AutoReLogin"]!)
        });
    }

    public void WriteToFile(string path) {
        var jsonMainConfig = new {
            Protocol = _botConfig.Protocol.ToString(),
            _botConfig.AutoReconnect,
            _botConfig.UseIPv6Network,
            _botConfig.GetOptimumServer,
            _botConfig.HighwayChunkSize,
            _botConfig.HighwayConcurrent,
            _botConfig.AutoReLogin
        };

        File.WriteAllText(path, JsonSerializer.Serialize(jsonMainConfig, ConfigManager.Options));
    }

    public static string GetMainWriteConfigName() {
        return "BotConfig";
    }


    public BotConfig GetBotConfig(SignProvider signProvider) {
        _botConfig.CustomSignProvider = signProvider;
        return _botConfig;
    }
}