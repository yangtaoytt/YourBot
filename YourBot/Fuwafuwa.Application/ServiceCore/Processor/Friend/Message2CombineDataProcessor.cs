using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using YourBot.Fuwafuwa.Application.Attribute.Processor.Friend;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Friend;

public class Message2CombineDataProcessor : IProcessorCore<MessageData, NullSharedDataWrapper<object>, object> {
    public static IServiceAttribute<MessageData> GetServiceAttribute() {
        return ReadFriendMessageAttribute.GetInstance();
    }
    public static NullSharedDataWrapper<object> Init(object initData) {
        return new NullSharedDataWrapper<object>(initData);
    }
    public static void Final(NullSharedDataWrapper<object> sharedData, Logger2Event? logger) { }
    public Task<List<Certificate>> ProcessData(MessageData data, NullSharedDataWrapper<object> sharedData, Logger2Event? logger) {
        return Task.FromResult<List<Certificate>>([
            ReadCombineData.GetInstance().GetCertificate(new CombinedData(null, data))
        ]);
    }
}