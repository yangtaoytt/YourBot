using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Data.SubjectData.Level2;
using Fuwafuwa.Core.Distributor.Interface;
using Fuwafuwa.Core.ServiceRegister;
using YourBot.AI.Interface;
using YourBot.Config.Implement.Level1.Service.Group;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.Distributor;

// ReSharper disable InconsistentNaming
public class QQUinDistributor<TSharedData> :ISimpleDistributor<MessageData, SubjectDataWithCommand, (SimpleSharedDataWrapper<Register>, TSharedData)> {
    // ReSharper restore InconsistentNaming
    protected override int Distribute(int processorCount, MessageData serviceData, (SimpleSharedDataWrapper<Register>, TSharedData) sharedData) {
        var uin = serviceData.MessageChain.FriendUin;
        return (int)(uin % processorCount);
    }
}