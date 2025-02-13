using Fuwafuwa.Core.Data.ServiceData.Level1;
using Fuwafuwa.Core.Subjects;
using YourBot.Fuwafuwa.Application.Attribute.Executor;

namespace YourBot.Fuwafuwa.Application.Data.ExecutorData;

public class MuteGroupMemberData : AExecutorData {
    public MuteGroupMemberData(Priority priority, uint groupUin, uint memberUin, uint duration) : base(priority, typeof(CanMuteGroupMemberAttribute)) {
        GroupUin = groupUin;
        MemberUin = memberUin;
        Duration = duration;
    }
    public uint GroupUin { get; init; }
    public uint MemberUin { get; init; }
    
    public uint Duration { get; init; }
    
}