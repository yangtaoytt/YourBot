using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Group;

public class AntiPlusOneConfig : ASimpleGroupPermissionConfig<AntiPlusOneConfig>, IConfig {

    public  int ReplyPriority { get; init; } = -1;

    public  int RandomRange { get; init; } = -1;

    public string ReplyImagePath { get; init; } = "";
    
    public static string GetConfigName() {
        return "AntiPlusOneConfig";
    }
}