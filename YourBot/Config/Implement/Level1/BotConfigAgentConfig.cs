using Lagrange.Core.Common;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1;

public class BotConfigAgentConfig : IConfig {
    public BotConfig BotConfig { get; init; } = new BotConfig();
    public static string GetConfigName() {
        return "BotConfig";
    }
}