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
    
    public float Possibility { get; init; } = 0.01f;
    
    public int Guarantee { get; init; } = -1;
    
    public int SmallGuarantee { get; init; } = -1;
    
    public float AutoDensity { get; init; } = 0.3f;
    
    public float LessAutoDensity { get; init; } = 0.1f;
}