using Fuwafuwa.Core.Data.ServiceData.Level1;

namespace YourBot.Fuwafuwa.Application.Data;
public enum HomeworkDeadlineRemindDataType {
    Add,
    Delete,
}
    
public class HomeworkData : IProcessorData {
    public HomeworkDeadlineRemindDataType Type { get; private init; }
    public uint HomeworkId { get; private init; }
    
    public uint FriendUin { get; private init; }
    
    public HomeworkData(HomeworkDeadlineRemindDataType type, uint homeworkId, uint friendUin) {
        Type = type;
        HomeworkId = homeworkId;
    }

}