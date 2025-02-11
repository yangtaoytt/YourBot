using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.Data.InitData.Group;

public class AntiPlusOneInitData : IReadConfig<AntiPlusOneInitData> {
    private AntiPlusOneInitData(List<uint> groupList, int replyPriority, int randomRange, string replyImagePath) {
        GroupList = groupList;
        ReplyPriority = replyPriority;
        RandomRange = randomRange;
        ReplyImagePath = replyImagePath;
    }

    public List<uint> GroupList { get; private init; }

    public int ReplyPriority { get; private init; }

    public int RandomRange { get; private init; }

    public string ReplyImagePath { get; private init; }

    public static List<string> GetConfigName() {
        return ["AntiPlusOneConfig"];
    }

    public static AntiPlusOneInitData CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new AntiPlusOneInitData(
            Util.ReadJsonGroupList(configurations["AntiPlusOneConfig"], "GroupList"),
            int.Parse(configurations["AntiPlusOneConfig"]["ReplyPriority"]!),
            int.Parse(configurations["AntiPlusOneConfig"]["RandomRange"]!),
            configurations["AntiPlusOneConfig"]["ReplyImagePath"]!
        );
    }
}