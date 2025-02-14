using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using YourBot.Config.Implement.Level1.Service.Group;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group;

public class
    AntiPlusOneProcessor : IProcessorCore<MessageData, NullSharedDataWrapper<AntiPlusOneConfig>,
    AntiPlusOneConfig> {
    private readonly Dictionary<uint, MessageChain> _groupMessages = [];

    public static IServiceAttribute<MessageData> GetServiceAttribute() {
        return ReadGroupMessageAttribute.GetInstance();
    }

    public static NullSharedDataWrapper<AntiPlusOneConfig> Init(AntiPlusOneConfig initData) {
        return new NullSharedDataWrapper<AntiPlusOneConfig>(initData);
    }

    public static void Final(NullSharedDataWrapper<AntiPlusOneConfig> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(MessageData data,
        NullSharedDataWrapper<AntiPlusOneConfig> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;

        var groupUin = data.MessageChain.GroupUin!.Value;

        var config = sharedData.Execute(initData => initData.Value);
        if (!Utils.YourBotUtil.CheckSimpleGroupPermission(config, groupUin)) {
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

        var imageBytes = await File.ReadAllBytesAsync(config.ReplyImagePath);

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