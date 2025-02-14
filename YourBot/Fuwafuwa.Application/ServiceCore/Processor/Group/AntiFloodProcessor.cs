using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message.Entity;
using YourBot.Config.Implement.Level1.Service.Group;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group;

public class AntiFloodProcessor : IProcessorCore<MessageData, NullSharedDataWrapper<AntiFloodConfig>, AntiFloodConfig> {
    private Dictionary<uint, Dictionary<uint, (double, DateTime)>> _groupMemberMessageCount = new();
    
    public static IServiceAttribute<MessageData> GetServiceAttribute() {
        return ReadGroupMessageAttribute.GetInstance();
    }
    public static NullSharedDataWrapper<AntiFloodConfig> Init(AntiFloodConfig initData) {
        return new NullSharedDataWrapper<AntiFloodConfig>(initData);
    }
    public static void Final(NullSharedDataWrapper<AntiFloodConfig> sharedData, Logger2Event? logger) { }
    public async Task<List<Certificate>> ProcessData(MessageData data, NullSharedDataWrapper<AntiFloodConfig> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;
        var config = sharedData.Execute(reference => reference.Value);
        var messages = data.MessageChain;
        var groupUin = messages.GroupUin!.Value;
        var memberUin = messages.FriendUin;
        if (!Utils.YourBotUtil.CheckGroupMemberPermission(config, groupUin, memberUin)) {
            return [];
        }
        
        if (!_groupMemberMessageCount.ContainsKey(groupUin)) {
            _groupMemberMessageCount[groupUin] = [];
        }
        if (!_groupMemberMessageCount[groupUin].ContainsKey(memberUin)) {
            _groupMemberMessageCount[groupUin][memberUin] = (0, DateTime.Now);
        }
        var (sumMessageCount, lastTime) = _groupMemberMessageCount[groupUin][memberUin];
        var messageCount = CalculateMessageCount(data,config);
        var subMessageCount = (DateTime.Now - lastTime).TotalSeconds * config.RegenerateSpeed;
        var currentMessageCount = sumMessageCount > subMessageCount ? sumMessageCount - subMessageCount : 0;
        var resultMessageCount = currentMessageCount + messageCount;
        _groupMemberMessageCount[groupUin][memberUin] = (resultMessageCount, DateTime.Now);
        if (_groupMemberMessageCount[groupUin][memberUin].Item1 <= config.WarningLimit) {
            return [];
        }
        if (_groupMemberMessageCount[groupUin][memberUin].Item1 <= config.FloodLimit) {
            return [Utils.YourBotUtil.SendToGroupMessage(groupUin, config.Priority, "请注意发送消息过快可能影响群友阅读")];
        }

        return [
            Utils.YourBotUtil.SendToGroupMessage(groupUin, config.Priority, "请勿刷屏"),
            CanMuteGroupMemberAttribute.GetInstance().GetCertificate(
                new MuteGroupMemberData(new Priority(config.Priority, PriorityStrategy.Share), groupUin, memberUin, config.MuteTime))
        ];

    }

    private static double CalculateMessageCount(MessageData data, AntiFloodConfig config) {
        var messageChain = data.MessageChain;
        double sumMessageCount = 0;
        foreach (var message in messageChain) {
            if (message is TextEntity textEntity) {
                sumMessageCount += textEntity.Text.Length;
            } else if (message is FaceEntity || message is MentionEntity) {
                sumMessageCount += message.ToPreviewText().Length;
            } else {
                sumMessageCount += config.OtherMessageCount;
            }
        }

        return sumMessageCount;
    }
}