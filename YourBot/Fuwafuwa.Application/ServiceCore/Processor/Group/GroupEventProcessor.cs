using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.InitData.Group;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group;

public class GroupEventProcessor : IProcessorCore<GroupEventData, SimpleSharedDataWrapper<GroupEventInitData>,
    GroupEventInitData> {
    public static IServiceAttribute<GroupEventData> GetServiceAttribute() {
        return ReadGroupQEventAttribute.GetInstance();
    }

    public static SimpleSharedDataWrapper<GroupEventInitData> Init(GroupEventInitData initData) {
        return new SimpleSharedDataWrapper<GroupEventInitData>(initData);
    }

    public static void Final(SimpleSharedDataWrapper<GroupEventInitData> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(GroupEventData data,
        SimpleSharedDataWrapper<GroupEventInitData> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;

        var messageChain = data.GroupMessageEvent.Chain;
        var groupUin = messageChain.GroupUin!.Value;
        var groupList = sharedData.Execute(initData => initData.Value.GroupList);
        if (!groupList.Contains(groupUin)) {
            return [];
        }

        var messageData = new MessageData(messageChain);
        return [
            ReadGroupQMessageAttribute.GetInstance().GetCertificate(messageData)
        ];
    }
}