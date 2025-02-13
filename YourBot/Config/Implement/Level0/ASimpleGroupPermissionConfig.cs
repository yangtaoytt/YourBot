using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level0;

public abstract class ASimpleGroupPermissionConfig<TConfig> where TConfig : ASimpleGroupPermissionConfig<TConfig>, IConfig {
    public List<uint> GroupPermission { get; init; } = [];
}