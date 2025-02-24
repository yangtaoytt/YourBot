using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Attributes.ServiceAttribute.Level1;
using Fuwafuwa.Core.Data.ServiceData.Level1;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using YourBot.Config.Implement.Level1.Service.Friend;
using YourBot.Fuwafuwa.Application.Attribute.Executor.Friend;
using YourBot.Fuwafuwa.Application.Data.ExecutorData.Friend;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Input;

public class HomeworkDeadlineRemindInputData {
    public HomeworkDeadlineRemindInputData(uint homeworkId, uint actorId, string name, DateTime deadline,DateTime remindTime, string instruction, List<uint> friends) {
        HomeworkId = homeworkId;
        ActorId = actorId;
        Name = name;
        Deadline = deadline;
        RemindTime = remindTime;
        Instruction = instruction;
        Friends = friends;
    }
    public uint HomeworkId { get; set; }
    public uint ActorId { get; set; }
    public string Name { get; set; }
    public DateTime Deadline { get; set; }
    
    public DateTime RemindTime { get; set; }
    public string Instruction { get; set; }
    
    public List<uint> Friends { get; set; }
}
public class HomeworkDeadlineRemindInput : IInputCore<NullSharedDataWrapper<HomeworkDeadlineRemindInputConfig>, HomeworkDeadlineRemindInputConfig> {
    public async Task<List<Certificate>> ProcessData(InputPackagedData data, NullSharedDataWrapper<HomeworkDeadlineRemindInputConfig> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;

        var priority = sharedData.Execute(reference => reference.Value.Priority);
        
        var homeworkDeadlineRemindInputData = (HomeworkDeadlineRemindInputData)data.PackagedObject!;
        var reminder = $"作业 {homeworkDeadlineRemindInputData.Name} 的截止日期为 {homeworkDeadlineRemindInputData.Deadline}，即将结束提交，请尽快完成。";
        return homeworkDeadlineRemindInputData.Friends.Select(friend =>
                CanSendMessage2FriendAttribute.GetInstance()
                    .GetCertificate(new SendMessage2FriendData(new Priority(priority, PriorityStrategy.Share),
                        MessageBuilder.Friend(friend).Text(reminder).Build())))
            .ToList();
    }

    public static IServiceAttribute<InputPackagedData> GetServiceAttribute() {
        return IInputAttribute.GetInstance();
    }
    public static NullSharedDataWrapper<HomeworkDeadlineRemindInputConfig> Init(HomeworkDeadlineRemindInputConfig initData) {
        return new NullSharedDataWrapper<HomeworkDeadlineRemindInputConfig>(initData);
    }
    public static void Final(NullSharedDataWrapper<HomeworkDeadlineRemindInputConfig> sharedData, Logger2Event? logger) { }
}