using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.Data.InitData.Group.Command;

public class MemeInitData : IReadConfig<MemeInitData> {
    private MemeInitData(Dictionary<uint, List<uint>> groupDic, int priority, string connectionString,
        string imageDir) {
        GroupDic = groupDic;
        Priority = priority;
        ConnectionString = connectionString;
        ImageDir = imageDir;
    }

    public string ConnectionString { get; private init; }

    public Dictionary<uint, List<uint>> GroupDic { get; private init; }

    public int Priority { get; private init; }

    public string ImageDir { get; init; }

    public static List<string> GetConfigName() {
        return ["DataBaseConfig", "MemeConfig"];
    }

    public static MemeInitData CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new MemeInitData(
            Util.ReadJsonGroupDic(configurations["MemeConfig"], "GroupDic"),
            int.Parse(configurations["MemeConfig"]["MessagePriority"]!),
            configurations["DataBaseConfig"]["ConnectionString"]!,
            configurations["MemeConfig"]["ImageDir"]!);
    }
}