using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;

namespace YourBot.Utils;

public static partial class Util {
    public static SendToGroupMessageData BuildSendToGroupMessageData(uint groupUin, int priority, string message) {
        var groupMessageChain = MessageBuilder.Group(groupUin).Text(message).Build();
        return new SendToGroupMessageData(new Priority(priority, PriorityStrategy.Share), groupMessageChain);
    }

    public static Certificate SendToGroupMessage(uint groupUin, int priority, string message) {
        return CanSendGroupMessageAttribute.GetInstance()
            .GetCertificate(BuildSendToGroupMessageData(
                groupUin, priority, message));
    }
}