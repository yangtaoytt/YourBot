using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.InitData.Group.Command;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command;

public class
    PingPongProcessor : IProcessorCore<CommandData, NullSharedDataWrapper<PingPongInitData>, PingPongInitData> {
    public static IServiceAttribute<CommandData> GetServiceAttribute() {
        return ReadGroupCommandAttribute.GetInstance();
    }

    public static NullSharedDataWrapper<PingPongInitData> Init(PingPongInitData initData) {
        return new NullSharedDataWrapper<PingPongInitData>(initData);
    }

    public static void Final(NullSharedDataWrapper<PingPongInitData> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(CommandData data,
        NullSharedDataWrapper<PingPongInitData> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;

        var initData = sharedData.Execute(initData => initData.Value);

        var groupUin = data.GroupUin;
        var memberUin = data.MessageChain.FriendUin;
        var groupDic = initData.GroupDic;
        if (!groupDic.TryGetValue(groupUin, out var value) || !value.Contains(memberUin)) {
            return [];
        }


        var command = data.Command;
        if (command[..4] != "ping") {
            return [];
        }

        var groupMessageChain = MessageBuilder.Group(groupUin).Text("pong").Build();
        var sendGroupMessageData =
            new SendToGroupMessageData(new Priority(initData.Priority, PriorityStrategy.Share), groupMessageChain);

        return [
            CanSendGroupMessageAttribute.GetInstance().GetCertificate(sendGroupMessageData)
        ];
    }
}