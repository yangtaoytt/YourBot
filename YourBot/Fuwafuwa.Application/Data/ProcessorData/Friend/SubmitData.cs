using System.Text.RegularExpressions;
using Fuwafuwa.Core.Data.ServiceData.Level1;
using Lagrange.Core.Message.Entity;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;

public class SubmitData : IProcessorData {
    public uint HomeworkId { get; private init; }
    
    public uint FriendUin { get; private init; }

    public List<FileEntity> FileEntities;

    public List<Regex> Regexes;
    
    public SubmitData(uint homeworkId, uint friendUin, List<FileEntity> fileEntities, List<Regex> regexes) {
        HomeworkId = homeworkId;
        FriendUin = friendUin;
        FileEntities = fileEntities;
        Regexes = regexes;
    }
}