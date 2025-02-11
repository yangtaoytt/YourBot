using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.Data.InitData.Group.Command;

public class VersionInitData : IReadConfig<VersionInitData> {
    private VersionInitData(string connectionString, Dictionary<uint, List<uint>> groupDic, int priority) {
        ConnectionString = connectionString;
        GroupDic = groupDic;
        Priority = priority;
    }

    public string ConnectionString { get; private init; }
    public Dictionary<uint, List<uint>> GroupDic { get; init; }

    public int Priority { get; init; }


    public static List<string> GetConfigName() {
        return ["DataBaseConfig", "VersionServiceConfig"];
    }

    public static VersionInitData CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new VersionInitData(configurations["DataBaseConfig"]["ConnectionString"]!,
            Util.ReadJsonGroupDic(configurations["VersionServiceConfig"], "GroupDic"),
            int.Parse(configurations["VersionServiceConfig"]["MessagePriority"]!));
    }
}