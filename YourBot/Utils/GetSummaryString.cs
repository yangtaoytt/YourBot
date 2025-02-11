using Lagrange.Core.Message;

namespace YourBot.Utils;

public static partial class Util {
    public static string GetSummaryStringInLine(MessageChain messageChain) {
        return string.Join(" ", messageChain.Select(item => item.ToPreviewString()));
    }
}