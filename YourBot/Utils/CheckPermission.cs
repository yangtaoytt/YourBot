using YourBot.Config.Implement.Level0;
using YourBot.Config.Interface;

namespace YourBot.Utils;

public static partial class Util {
    public static bool CheckSimpleGroupPermission<TConfig>(TConfig config, uint groupUin) where TConfig : ASimpleGroupPermissionConfig<TConfig>, IConfig {
        return config.GroupPermission.Contains(groupUin);
    }
    
    public static bool CheckGroupMemberPermission<TConfig>(TConfig config, uint groupUin, uint memberUin) where TConfig : AGroupMemberPermissionConfig<TConfig>, IConfig {
        if (!config.GroupPermission.TryGetValue(groupUin, out var value)) {
            return false;
        }
        var isWhiteList = value.IsWhiteList;
        return isWhiteList ? value.MemberList.Contains(memberUin) : !value.MemberList.Contains(memberUin);
    }
}