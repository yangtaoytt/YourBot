using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;

namespace YourBot.Utils;

public static partial class YourBotUtil {
    public static int CheckCommand(MessageChain messageChain) {
        for (var i = 0; i < messageChain.Count; ++i) {
            var message = messageChain[i];
            if (message is TextEntity textEntity && textEntity.Text.Contains('/')) {
                return i;
            }
        }

        return -1;
    }
}