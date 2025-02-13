using Fuwafuwa.Core.Data.SubjectData.Level2;
using Fuwafuwa.Core.Distributor.Interface;
using YourBot.AI.Interface;
using YourBot.Config.Implement.Level1.Service.Group;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.Distributor;

// ReSharper disable InconsistentNaming
public class AIReviewDistributor : ISimpleDistributor<MessageData, SubjectDataWithCommand, (IAI, AIReviewConfig)> {
    // ReSharper restore InconsistentNaming
    protected override int Distribute(int processorCount, MessageData serviceData, (IAI, AIReviewConfig) sharedData) {
        var uin = serviceData.MessageChain.FriendUin;
        return (int)(uin % processorCount);
    }
}