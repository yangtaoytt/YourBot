using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Friend;

public class HomeworkDeadlineRemindInputConfig : IConfig {

    public int Priority { get; init; } = -1;
    public static string GetConfigName() {
        return "HomeworkDeadlineRemindInputConfig";
    }
}