using Microsoft.Extensions.Logging;

namespace YourBot.Utils;

public static partial class YourBotUtil {
    public static LogLevel LogLevelConvert(Lagrange.Core.Event.EventArg.LogLevel logLevel) {
        return logLevel switch {
            Lagrange.Core.Event.EventArg.LogLevel.Debug => LogLevel.Debug,
            Lagrange.Core.Event.EventArg.LogLevel.Verbose => LogLevel.Trace,
            Lagrange.Core.Event.EventArg.LogLevel.Information => LogLevel.Information,
            Lagrange.Core.Event.EventArg.LogLevel.Warning => LogLevel.Warning,
            Lagrange.Core.Event.EventArg.LogLevel.Exception => LogLevel.Error,
            Lagrange.Core.Event.EventArg.LogLevel.Fatal => LogLevel.Critical,
            _ => LogLevel.Trace
        };
    }
}