using Fuwafuwa.Core.Data.ServiceData.Level1;
using Lagrange.Core.Message;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;

public class FriendCommandData : IProcessorData {
    public FriendCommandData(string command, List<string> parameters, MessageChain messageChain, uint friendUin) {
        Command = command;
        Parameters = parameters;
        MessageChain = messageChain;
        FriendUin = friendUin;
    }
    public string Command { get; private init; }

    public List<string> Parameters { get; private init; }

    public MessageChain MessageChain { get; private init; }
    
    public uint FriendUin { get; private init; }
}