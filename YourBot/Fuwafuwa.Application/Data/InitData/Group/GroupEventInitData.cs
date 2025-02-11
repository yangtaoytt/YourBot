using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.Data.InitData.Group;

public class GroupEventInitData : IReadConfig<GroupEventInitData> {
    public GroupEventInitData(List<uint> groupList) {
        GroupList = groupList;
    }

    public List<uint> GroupList { get; private init; }

    public static List<string> GetConfigName() {
        return ["QGroupEventConfig"];
    }

    public static GroupEventInitData CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new GroupEventInitData(Util.ReadJsonGroupList(configurations["QGroupEventConfig"], "GroupList"));
    }
}