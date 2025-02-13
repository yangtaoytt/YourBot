using Lagrange.Core.Common;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1;

public class BotKeyStoreAgentConfig : IConfig {
    public BotKeystore? BotKeystore { get; set; } = null;
    public static string GetConfigName() {
        return "BotKeystore";
    }
}