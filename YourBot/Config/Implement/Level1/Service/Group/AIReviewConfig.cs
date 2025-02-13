using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Group;

// ReSharper disable InconsistentNaming
public class AIReviewConfig : AGroupMemberPermissionConfig<AIReviewConfig>, IConfig {
    // ReSharper restore InconsistentNaming

    public  int RevokePriority { get; init; } = -1;

    public  int ReplyPriority { get; init; } = -1;

    public  int SaveMessageCount { get; init; } = -1;
    public static string GetConfigName() {
        return "AIReviewConfig";
    }
}