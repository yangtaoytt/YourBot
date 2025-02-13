using Lagrange.Core.Common;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1;

public class BotDeviceInfoAgentConfig : IConfig {
    public BotDeviceInfo? BotDeviceInfo { get; set; } = null;
    public static string GetConfigName() {
        return "BotDeviceInfo";
    }
}