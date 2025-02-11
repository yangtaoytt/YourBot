using Fuwafuwa.Core.Data.ServiceData.Level1;
using Lagrange.Core.Message;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData;

public class MessageData : IProcessorData {
    public MessageData(MessageChain messageChain) {
        MessageChain = messageChain;
    }

    public MessageChain MessageChain { get; set; }
}