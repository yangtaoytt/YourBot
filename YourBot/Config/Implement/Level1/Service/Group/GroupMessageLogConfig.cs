using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Group;

public class GroupMessageLogConfig : ASimpleGroupPermissionConfig<GroupMessageLogConfig>, IConfig {
    public int Priority { get; init; } = -1;
    public static string GetConfigName() {
        return "GroupMessageLogConfig";
    }
}