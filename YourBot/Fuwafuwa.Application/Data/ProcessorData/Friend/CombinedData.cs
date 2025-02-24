using Fuwafuwa.Core.Data.ServiceData.Level1;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;

public class CombinedData :IProcessorData {
    public AddSubmitMissionData? AddSubmitMissionData;
    public MessageData? FriendMessageData;


    public CombinedData(AddSubmitMissionData? addSubmitMissionData, MessageData? friendCommandData) {
        AddSubmitMissionData = addSubmitMissionData;
        FriendMessageData = friendCommandData;
    }
}