using Fuwafuwa.Core.Data.ServiceData.Level1;
using Lagrange.Core.Message;
using YourBot.Utils.Command;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData;

public class GroupCommandData : IProcessorData {
    public GroupCommandData(string command, MessageChain messageChain, List<string> parameters, uint groupUin, CommandHandler commandHandler) {
        Command = command;
        MessageChain = messageChain;
        Parameters = parameters;
        GroupUin = groupUin;
        CommandHandler = commandHandler;
    }

    public string Command { get; private init; }
    
    public CommandHandler CommandHandler { get; private init; }

    public List<string> Parameters { get; private init; }

    public MessageChain MessageChain { get; private init; }

    public uint GroupUin { get; private init; }
}