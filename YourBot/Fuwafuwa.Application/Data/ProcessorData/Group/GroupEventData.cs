using Fuwafuwa.Core.Data.ServiceData.Level1;
using Lagrange.Core.Event.EventArg;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData;

public class GroupEventData : IProcessorData {
    public GroupEventData(GroupMessageEvent groupMessageEvent) {
        GroupMessageEvent = groupMessageEvent;
    }

    public GroupMessageEvent GroupMessageEvent { get; private init; }
}