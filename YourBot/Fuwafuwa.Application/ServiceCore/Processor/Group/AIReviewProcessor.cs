using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using YourBot.AI.Interface;
using YourBot.Config.Implement.Level1.Service.Group;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group;

// ReSharper disable InconsistentNaming
public class AIReviewProcessor : IProcessorCore<MessageData, AsyncSharedDataWrapper<(IAI, AIReviewConfig)>, (IAI,
    AIReviewConfig)> {
    // ReSharper restore InconsistentNaming

    private readonly Dictionary<uint, (List<IMessageEntity> messageEntities, List<int> onceMessageCountList)>
        _memberMessages = [];

    public static IServiceAttribute<MessageData> GetServiceAttribute() {
        return ReadGroupMessageAttribute.GetInstance();
    }

    public static AsyncSharedDataWrapper<(IAI, AIReviewConfig)> Init((IAI, AIReviewConfig) initData) {
        return new AsyncSharedDataWrapper<(IAI, AIReviewConfig)>(initData);
    }

    public static void Final(AsyncSharedDataWrapper<(IAI, AIReviewConfig)> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(MessageData data,
        AsyncSharedDataWrapper<(IAI, AIReviewConfig)> sharedData, Logger2Event? logger) {
        var messages = data.MessageChain;
        var groupUin = messages.GroupUin!.Value;
        var memberUin = messages.FriendUin;

        var config = await sharedData.ExecuteAsync(reference => Task.FromResult(reference.Value.Item2));
        if (!Utils.YourBotUtil.CheckGroupMemberPermission(config, groupUin, memberUin)) {
            return [];
        }

        if (!_memberMessages.ContainsKey(memberUin)) {
            _memberMessages[memberUin] = ([], []);
        }

        var sendingMessages = new List<IMessageEntity>();
        foreach (var messageEntity in messages) {
            if (messageEntity is MultiMsgEntity multiMsgEntity) {
                sendingMessages.AddRange(GetMessagesFromMultiMsgEntity(multiMsgEntity));
                continue;
            }

            sendingMessages.Add(messageEntity);
        }

        var (messageEntities, onceMessageCountList) = _memberMessages[memberUin];
        var readableMessages = sendingMessages.Where(msg => msg is not VideoEntity or FileEntity).ToList();
        messageEntities.AddRange(readableMessages);
        onceMessageCountList.Add(readableMessages.Count);

        var saveMessageCount =
            await sharedData.ExecuteAsync(reference => Task.FromResult(reference.Value.Item2.SaveMessageCount));
        if (onceMessageCountList.Count > saveMessageCount) {
            var removeCount = onceMessageCountList[0];
            onceMessageCountList.RemoveAt(0);

            for (var i = 0; i < removeCount; i++) {
                messageEntities.RemoveAt(0);
            }
        }

        var result = await sharedData.ExecuteAsync(async reference => {
            var (ai, aiReviewInitData) = reference.Value;
            return await ai.JudgeFriendly(messageEntities);
        });
        if (result.IsFriendly) {
            return [];
        }

        var groupMessageChain = MessageBuilder.Group(groupUin)
            .Mention(memberUin)
            .Text(" " + result.Suggestion)
            .Build();
        var (revokePriority, replyPriority) = await sharedData.ExecuteAsync(reference =>
            Task.FromResult((reference.Value.Item2.RevokePriority, reference.Value.Item2.ReplyPriority)));
        var revokeData = new RevokeGroupMessageData(new Priority(revokePriority, PriorityStrategy.Unique), groupUin,
            messages.Sequence);
        var replyData =
            new SendToGroupMessageData(new Priority(replyPriority, PriorityStrategy.Share), groupMessageChain);
        return [
            CanRevokeGroupMessageAttribute.GetInstance().GetCertificate(revokeData),
            CanSendGroupMessageAttribute.GetInstance().GetCertificate(replyData)
        ];
    }

    private static List<IMessageEntity> GetMessagesFromMultiMsgEntity(MultiMsgEntity multiMsgEntity) {
        List<IMessageEntity> messages = [];
        foreach (var messageEntity in multiMsgEntity.Chains.SelectMany(messageChain => messageChain)) {
            if (messageEntity is MultiMsgEntity entity) {
                messages.AddRange(GetMessagesFromMultiMsgEntity(entity));
            } else {
                messages.Add(messageEntity);
            }
        }

        return messages;
    }
}