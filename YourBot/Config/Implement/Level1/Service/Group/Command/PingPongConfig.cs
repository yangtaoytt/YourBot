using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Group.Command;

public class PingPongConfig : AGroupMemberPermissionConfig<PingPongConfig>, IConfig {
    public int Priority { get; init; } = -1;
    public static string GetConfigName() {
        return "PingPongConfig";
    }
}