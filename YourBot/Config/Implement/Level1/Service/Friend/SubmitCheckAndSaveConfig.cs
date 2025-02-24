using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Friend.Command;

public class SubmitCheckAndSaveConfig : IConfig {
    public int Priority { get; init; } = -1;
    
    public string FilePath { get; init; } = "Submits";
    
    public static string GetConfigName() {
        return "SubmitCheckAndSaveConfig";
    }
}