using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Group.Command.Homework;

public class ActorConfig : AFriendPermissionConfig<ActorConfig>, IConfig {
    
    public int Priority { get; init; } = -1;
    public static string GetConfigName() {
        return "ActorConfig";
    }
}