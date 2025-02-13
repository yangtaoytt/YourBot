using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level0;

public abstract class AFriendPermissionConfig<TConfig> where TConfig : AFriendPermissionConfig<TConfig>, IConfig {
    public List<uint> FriendPermission { get; init; } = [];
}