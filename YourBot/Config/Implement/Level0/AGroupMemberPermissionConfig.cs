using System.Text.Json;
using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level0;

public abstract class AGroupMemberPermissionConfig<TConfig> where TConfig : AGroupMemberPermissionConfig<TConfig>, IConfig {
    public class GroupPermissionEntity {
        public bool IsWhiteList { get; init; } = true;
        public List<uint> MemberList { get; init; } = [];
    }

    public Dictionary<uint, GroupPermissionEntity> GroupPermission { get; init; } = [];
}