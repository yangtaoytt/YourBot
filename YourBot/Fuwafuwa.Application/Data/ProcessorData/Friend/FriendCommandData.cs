using Fuwafuwa.Core.Data.ServiceData.Level1;
using Lagrange.Core.Message;
using YourBot.Utils.Command;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;

public class FriendCommandData : IProcessorData {
    public FriendCommandData(CommandHandler commandHandler, MessageChain messageChain, uint friendUin) {
        CommandHandler = commandHandler;
        MessageChain = messageChain;
        FriendUin = friendUin;
    }
    public CommandHandler CommandHandler { get; private init; }

    public MessageChain MessageChain { get; private init; }
    
    public uint FriendUin { get; private init; }
}