using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Group.Command;

public class MemeConfig : ASimpleGroupPermissionConfig<MemeConfig>, IConfig {
    public static string GetConfigName() {
        return "GroupMeme";
    }

    public int Priority { get; init; } = -1;

    public string ImageDir { get; init; } = "";
}