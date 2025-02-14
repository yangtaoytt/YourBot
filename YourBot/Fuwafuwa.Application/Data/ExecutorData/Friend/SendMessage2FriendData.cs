using Fuwafuwa.Core.Data.ServiceData.Level1;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using YourBot.Fuwafuwa.Application.Attribute.Executor.Friend;

namespace YourBot.Fuwafuwa.Application.Data.ExecutorData.Friend;

public class SendMessage2FriendData : AExecutorData{
    public SendMessage2FriendData(Priority priority, MessageChain messageChain) : base(priority, typeof(CanSendMessage2FriendAttribute)) {
        MessageChain = messageChain;
    }
    public MessageChain MessageChain { get; set; }
}