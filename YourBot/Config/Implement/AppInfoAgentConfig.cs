using System.Diagnostics;
using System.Text.Json;
using Lagrange.Core.Common;
using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement;

public class AppInfoAgentConfig : IReadConfig<AppInfoAgentConfig>, IWriteConfig {
    public AppInfoAgentConfig(BotAppInfo botAppInfo) {
        BotAppInfo = botAppInfo;
    }

    public BotAppInfo BotAppInfo { get; set; }

    public static List<string> GetConfigName() {
        return ["AppInfoConfig"];
    }

    public static AppInfoAgentConfig CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new AppInfoAgentConfig(configurations["AppInfoConfig"].Get<BotAppInfo>()!);
    }

    public void WriteToFile(string path) {
        Debug.Assert(BotAppInfo != null);

        File.WriteAllText(path, JsonSerializer.Serialize(BotAppInfo, ConfigManager.Options));
    }

    public static string GetMainWriteConfigName() {
        return "AppInfoConfig";
    }
}