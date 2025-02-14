using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Lagrange.Core.Event.EventArg;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Attribute.Processor.Friend;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor;

public class EventRouterProcessor : IProcessorCore<EventData, NullSharedDataWrapper<object>, object> {
    public static IServiceAttribute<EventData> GetServiceAttribute() {
        return ReadQEventAttribute.GetInstance();
    }

    public static NullSharedDataWrapper<object> Init(object initData) {
        return new NullSharedDataWrapper<object>(initData);
    }

    public static void Final(NullSharedDataWrapper<object> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(EventData data, NullSharedDataWrapper<object> sharedData,
        Logger2Event? logger) {
        await Task.CompletedTask;

        if (data.Event is GroupMessageEvent groupMessageEvent) {
            var groupMessageData = new GroupEventData(groupMessageEvent);
            return [
                ReadGroupEventAttribute.GetInstance().GetCertificate(groupMessageData)
            ];
        } 
        if (data.Event is FriendMessageEvent friendMessageEvent) {
            var friendMessageData = new FriendEventData(friendMessageEvent);
            return [
                ReadFriendEventAttribute.GetInstance().GetCertificate(friendMessageData)
            ];
        }

        return [];
    }
}