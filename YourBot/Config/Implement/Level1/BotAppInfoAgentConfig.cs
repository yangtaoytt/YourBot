using Lagrange.Core.Common;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1;

public class BotAppInfoAgentConfig : IConfig {
    public BotAppInfo? BotAppInfo { get; set; } = null;
    public static string GetConfigName() {
        return "BotAppInfo";
    }
}