using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using YourBot.Config.Implement.Level1.Service.Group;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group;

public class GroupEventProcessor : IProcessorCore<GroupEventData, SimpleSharedDataWrapper<GlobalGroupActiveConfig>,
    GlobalGroupActiveConfig> {
    public static IServiceAttribute<GroupEventData> GetServiceAttribute() {
        return ReadGroupQEventAttribute.GetInstance();
    }

    public static SimpleSharedDataWrapper<GlobalGroupActiveConfig> Init(GlobalGroupActiveConfig initData) {
        return new SimpleSharedDataWrapper<GlobalGroupActiveConfig>(initData);
    }

    public static void Final(SimpleSharedDataWrapper<GlobalGroupActiveConfig> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(GroupEventData data,
        SimpleSharedDataWrapper<GlobalGroupActiveConfig> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;

        var messageChain = data.GroupMessageEvent.Chain;
        var groupUin = messageChain.GroupUin!.Value;
        var config = sharedData.Execute(reference => reference.Value);
        if (!Utils.YourBotUtil.CheckSimpleGroupPermission(config, groupUin)) {
            return [];
        }

        var messageData = new MessageData(messageChain);
        return [
            ReadGroupMessageAttribute.GetInstance().GetCertificate(messageData)
        ];
    }
}