using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1.Service.Group;

public class AntiFloodConfig : AGroupMemberPermissionConfig<AntiFloodConfig>, IConfig {
    
    public double FloodLimit { get; init; } = -1;
    
    public double WarningLimit { get; init; } = -1;
    
    public double RegenerateSpeed { get; init; } = -1;
    
    public double OtherMessageCount { get; init; } = -1;
    public uint MuteTime { get; init; } = 1;
    
    public int Priority { get; init; } = -1;
    public static string GetConfigName() {
        return "AntiFloodConfig";
    }
}