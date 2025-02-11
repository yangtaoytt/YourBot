using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.InitData.Group;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group;

public class
    AntiPlusOneProcessor : IProcessorCore<MessageData, NullSharedDataWrapper<AntiPlusOneInitData>,
    AntiPlusOneInitData> {
    private readonly Dictionary<uint, MessageChain> _groupMessages = [];

    public static IServiceAttribute<MessageData> GetServiceAttribute() {
        return ReadGroupQMessageAttribute.GetInstance();
    }

    public static NullSharedDataWrapper<AntiPlusOneInitData> Init(AntiPlusOneInitData initData) {
        return new NullSharedDataWrapper<AntiPlusOneInitData>(initData);
    }

    public static void Final(NullSharedDataWrapper<AntiPlusOneInitData> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(MessageData data,
        NullSharedDataWrapper<AntiPlusOneInitData> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;

        var groupUin = data.MessageChain.GroupUin!.Value;

        var initData = sharedData.Execute(initData => initData.Value);
        var groupList = initData.GroupList;
        if (!groupList.Contains(groupUin)) {
            return [];
        }

        var message = data.MessageChain;
        if (_groupMessages.TryAdd(groupUin, message)) {
            return [];
        }

        var lastMessage = _groupMessages[groupUin];
        _groupMessages[groupUin] = message;

        if (lastMessage.Count != message.Count) {
            return [];
        }

        for (var i = 0; i < lastMessage.Count; ++i) {
            var lastMessageElement = lastMessage[i];
            var messageElement = message[i];
            if (lastMessageElement.ToPreviewString() != messageElement.ToPreviewString()) {
                return [];
            }
        }

        var randomRange = sharedData.Execute(initData => initData.Value.RandomRange);
        var randomCount = new Random().Next(0, randomRange);
        var randomString = "";
        while (randomCount-- != 0) {
            randomString += "~";
        }

        var imageBytes = await File.ReadAllBytesAsync(initData.ReplyImagePath);

        var groupMessageChain = MessageBuilder.Group(groupUin)
            .Image(imageBytes)
            .Text("+1被路过的小猫偷走了！~" + randomString)
            .Build();

        var sendToGroupMessageData =
            new SendToGroupMessageData(
                new Priority(sharedData.Execute(initData => initData.Value.ReplyPriority), PriorityStrategy.Share),
                groupMessageChain);

        return [
            CanSendGroupMessageAttribute.GetInstance().GetCertificate(sendToGroupMessageData)
        ];
    }
}