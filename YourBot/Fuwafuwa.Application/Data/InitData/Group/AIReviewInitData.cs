using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.Data.InitData.Group;

// ReSharper disable InconsistentNaming
public class AIReviewInitData : IReadConfig<AIReviewInitData> {
    // ReSharper restore InconsistentNaming

    private AIReviewInitData(Dictionary<uint, List<uint>> groupDic, int revokePriority, int replyPriority,
        int saveMessageCount) {
        GroupDic = groupDic;
        RevokePriority = revokePriority;
        SaveMessageCount = saveMessageCount;
        ReplyPriority = replyPriority;
    }

    public Dictionary<uint, List<uint>> GroupDic { get; private init; }

    public int RevokePriority { get; private init; }

    public int ReplyPriority { get; private init; }

    public int SaveMessageCount { get; private init; }

    public static List<string> GetConfigName() {
        return ["AIReviewConfig"];
    }

    public static AIReviewInitData CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new AIReviewInitData(
            Util.ReadJsonGroupDic(configurations["AIReviewConfig"], "GroupDic"),
            int.Parse(configurations["AIReviewConfig"]["RevokePriority"]!),
            int.Parse(configurations["AIReviewConfig"]["ReplyPriority"]!),
            int.Parse(configurations["AIReviewConfig"]["SaveMessageCount"]!));
    }
}