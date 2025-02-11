using System.Text.Json;
using Lagrange.Core.Common;
using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement;

public class BotKeystoreAgentConfig : IReadConfig<BotKeystoreAgentConfig>, IWriteConfig {
    private static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true
    };

    private BotKeystoreAgentConfig(BotKeystore? botKeystore) {
        BotKeystore = botKeystore;
    }

    public BotKeystore? BotKeystore { get; set; }

    public static List<string> GetConfigName() {
        return ["KeystoreConfig"];
    }

    public static BotKeystoreAgentConfig CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new BotKeystoreAgentConfig(configurations["KeystoreConfig"].Get<BotKeystore>() ?? null);
    }

    public void WriteToFile(string path) {
        if (BotKeystore == null) {
            return;
        }

        File.WriteAllText(path, JsonSerializer.Serialize(BotKeystore, Options));
    }

    public static string GetMainWriteConfigName() {
        return "KeystoreConfig";
    }
}