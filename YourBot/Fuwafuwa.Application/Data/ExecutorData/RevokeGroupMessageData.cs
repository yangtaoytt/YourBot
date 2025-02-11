using Fuwafuwa.Core.Data.ServiceData.Level1;
using Fuwafuwa.Core.Subjects;
using YourBot.Fuwafuwa.Application.Attribute.Executor;

namespace YourBot.Fuwafuwa.Application.Data.ExecutorData;

public class RevokeGroupMessageData : AExecutorData {
    public RevokeGroupMessageData(Priority priority, uint groupUin, uint messageSeq) : base(priority,
        typeof(CanRevokeGroupMessageAttribute)) {
        GroupUin = groupUin;
        MessageSeq = messageSeq;
    }

    public uint GroupUin { get; init; }
    public uint MessageSeq { get; init; }
}