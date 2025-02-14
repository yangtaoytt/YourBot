using Fuwafuwa.Core.Attributes.ServiceAttribute.Level1;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;

namespace YourBot.Fuwafuwa.Application.Attribute.Executor;

public class CanSendGroupMessageAttribute : IExecutorAttribute<CanSendGroupMessageAttribute, SendToGroupMessageData>;