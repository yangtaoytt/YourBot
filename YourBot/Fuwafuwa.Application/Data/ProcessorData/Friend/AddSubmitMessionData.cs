using System.Text.RegularExpressions;
using Fuwafuwa.Core.Data.ServiceData.Level1;
using Lagrange.Core.Message;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;

public class AddSubmitMissionData : IProcessorData {
    public List<Regex> Regexes;
    public uint HomeworkId;
    public uint FriendUin;
    
    public AddSubmitMissionData(List<Regex> regexes, uint homeworkId, uint friendUin) {
        Regexes = regexes;
        HomeworkId = homeworkId;
        FriendUin = friendUin;
    }

}