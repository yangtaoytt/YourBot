using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Executor.Friend;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.ExecutorData.Friend;

namespace YourBot.Utils;

public static partial class YourBotUtil {
    public static SendToGroupMessageData BuildSendToGroupMessageData(uint groupUin, int priority, string message) {
        var groupMessageChain = MessageBuilder.Group(groupUin).Text(message).Build();
        return new SendToGroupMessageData(new Priority(priority, PriorityStrategy.Share), groupMessageChain);
    }

    public static Certificate SendToGroupMessage(uint groupUin, int priority, string message) {
        return CanSendGroupMessageAttribute.GetInstance()
            .GetCertificate(BuildSendToGroupMessageData(
                groupUin, priority, message));
    }
    
    public static SendMessage2FriendData BuildSendToFriendMessageData(uint friendUin, int priority, string message) {
        var friendMessageChain = MessageBuilder.Friend(friendUin).Text(message).Build();
        return new SendMessage2FriendData(new Priority(priority, PriorityStrategy.Share), friendMessageChain);
    }
    
    public static Certificate SendToFriendMessage(uint friendUin, int priority, string message) {
        return CanSendMessage2FriendAttribute.GetInstance()
            .GetCertificate(BuildSendToFriendMessageData(friendUin, priority, message));
    }
}