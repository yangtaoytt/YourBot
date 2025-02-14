using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using YourBot.Config.Implement.Level1.Service.Friend;
using YourBot.Fuwafuwa.Application.Attribute.Processor.Friend;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Friend;

public class FriendEventProcessor : IProcessorCore<FriendEventData, NullSharedDataWrapper<GlobalFriendActiveConfig>, GlobalFriendActiveConfig> {
    public static IServiceAttribute<FriendEventData> GetServiceAttribute() {
        return ReadFriendEventAttribute.GetInstance();
    }
    public static NullSharedDataWrapper<GlobalFriendActiveConfig> Init(GlobalFriendActiveConfig initData) {
        return new NullSharedDataWrapper<GlobalFriendActiveConfig>(initData);
    }
    public static void Final(NullSharedDataWrapper<GlobalFriendActiveConfig> sharedData, Logger2Event? logger) { }
    public async Task<List<Certificate>> ProcessData(FriendEventData data, NullSharedDataWrapper<GlobalFriendActiveConfig> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;
        
        var config = sharedData.Execute(reference => reference.Value);
        
        var messageChain = data.FriendMessageEvent.Chain;
        var friendUin = messageChain.FriendUin;
        if (!Utils.YourBotUtil.CheckFriendPermission(config, friendUin)) {
            return [];
        }
        
        var messageData = new MessageData(messageChain);
        return [
            ReadFriendMessageAttribute.GetInstance().GetCertificate(messageData)
        ];
    }
}