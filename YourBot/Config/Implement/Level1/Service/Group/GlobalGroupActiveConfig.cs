using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Group;

public class GlobalGroupActiveConfig : ASimpleGroupPermissionConfig<GlobalGroupActiveConfig>, IConfig {
    public static string GetConfigName() {
        return "GlobalGroupActiveConfig";
    }
}