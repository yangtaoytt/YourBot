using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.Data.InitData.Group;

public class GroupMessageLogInitData : IReadConfig<GroupMessageLogInitData> {
    private GroupMessageLogInitData(List<uint> groupList, int priority) {
        GroupList = groupList;
        Priority = priority;
    }

    public List<uint> GroupList { get; init; }

    public int Priority { get; init; }

    public static List<string> GetConfigName() {
        return ["GroupLogConfig"];
    }

    public static GroupMessageLogInitData CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new GroupMessageLogInitData(
            Util.ReadJsonGroupList(configurations["GroupLogConfig"], "GroupList"),
            int.Parse(configurations["GroupLogConfig"]["MessagePriority"]!));
    }
}