using Fuwafuwa.Core.Data.ServiceData.Level1;
using Lagrange.Core.Event.EventArg;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;

public class FriendEventData : IProcessorData {
    public FriendEventData(FriendMessageEvent friendMessageEvent) {
        FriendMessageEvent = friendMessageEvent;
    }

    public FriendMessageEvent FriendMessageEvent { get; private init; }
}