using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.Data.InitData.Group.Command;

public class PingPongInitData : IReadConfig<PingPongInitData> {
    private PingPongInitData(Dictionary<uint, List<uint>> groupDic, int priority) {
        GroupDic = groupDic;
        Priority = priority;
    }

    public Dictionary<uint, List<uint>> GroupDic { get; init; }

    public int Priority { get; init; }


    public static List<string> GetConfigName() {
        return ["PingPongConfig"];
    }

    public static PingPongInitData CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new PingPongInitData(
            Util.ReadJsonGroupDic(configurations["PingPongConfig"], "GroupDic"),
            int.Parse(configurations["PingPongConfig"]["MessagePriority"]!));
    }
}