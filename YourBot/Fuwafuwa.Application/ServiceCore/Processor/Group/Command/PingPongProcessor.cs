using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using YourBot.Config.Implement.Level1.Service.Group.Command;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command;

public class
    PingPongProcessor : IProcessorCore<CommandData, NullSharedDataWrapper<PingPongConfig>, PingPongConfig> {
    public static IServiceAttribute<CommandData> GetServiceAttribute() {
        return ReadGroupCommandAttribute.GetInstance();
    }

    public static NullSharedDataWrapper<PingPongConfig> Init(PingPongConfig initData) {
        return new NullSharedDataWrapper<PingPongConfig>(initData);
    }

    public static void Final(NullSharedDataWrapper<PingPongConfig> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(CommandData data,
        NullSharedDataWrapper<PingPongConfig> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;

        var config = sharedData.Execute(initData => initData.Value);

        var groupUin = data.GroupUin;
        var memberUin = data.MessageChain.FriendUin;
        if (!Utils.Util.CheckGroupMemberPermission(config, groupUin, memberUin)) {
            return [];
        }


        var command = data.Command;
        if (command[..4] != "ping") {
            return [];
        }

        var groupMessageChain = MessageBuilder.Group(groupUin).Text("pong").Build();
        var sendGroupMessageData =
            new SendToGroupMessageData(new Priority(config.Priority, PriorityStrategy.Share), groupMessageChain);

        return [
            CanSendGroupMessageAttribute.GetInstance().GetCertificate(sendGroupMessageData)
        ];
    }
}