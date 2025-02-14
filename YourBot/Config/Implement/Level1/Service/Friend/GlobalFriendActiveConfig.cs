using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Friend;

public class GlobalFriendActiveConfig : AFriendPermissionConfig<GlobalFriendActiveConfig>, IConfig {
    public static string GetConfigName() {
        return "GlobalFriendActiveConfig";
    }
}