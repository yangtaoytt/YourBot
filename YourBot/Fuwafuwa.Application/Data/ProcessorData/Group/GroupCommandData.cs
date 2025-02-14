using Fuwafuwa.Core.Data.ServiceData.Level1;
using Lagrange.Core.Message;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData;

public class GroupCommandData : IProcessorData {
    public GroupCommandData(string command, MessageChain messageChain, List<string> parameters, uint groupUin) {
        Command = command;
        MessageChain = messageChain;
        Parameters = parameters;
        GroupUin = groupUin;
    }

    public string Command { get; private init; }

    public List<string> Parameters { get; private init; }

    public MessageChain MessageChain { get; private init; }

    public uint GroupUin { get; private init; }
}