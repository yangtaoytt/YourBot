using Fuwafuwa.Core.Data.ServiceData.Level1;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using YourBot.Fuwafuwa.Application.Attribute.Executor;

namespace YourBot.Fuwafuwa.Application.Data.ExecutorData;

public class SendToGroupMessageData : AExecutorData {
    public SendToGroupMessageData(Priority priority, MessageChain messageChain) : base(priority,
        typeof(CanSendGroupMessageAttribute)) {
        MessageChain = messageChain;
    }

    public MessageChain MessageChain { get; set; }
}