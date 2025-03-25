using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Group.Command;

public class JmConfig : ASimpleGroupPermissionConfig<JmConfig>, IConfig {
    public static string GetConfigName() {
        return "JMConfig";
    }
    
    public int Priority { get; init; } = -1;
    
    public string ImageDir { get; init; } = "";

    public string OptionFilePath { get; init; } = "";
}