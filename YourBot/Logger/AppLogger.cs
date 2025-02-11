using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace YourBot.Logger;

public enum LogSource {
    YourBot,
    Lagrange
}

public abstract class AppLogger {
    private static readonly ConcurrentDictionary<string, ILogger> Loggers = new();

    protected abstract LogLevel MinLogLevel { get; }

    public static ILogger DefaultLogger => LoggerFactory.Create(builder => {
            builder.AddSimpleConsole(options => { }).SetMinimumLevel(LogLevel.Trace);
        })
        .CreateLogger("default");

    private static ILogger CreateLogger(LogLevel minLogLevel, string tag) {
        var logger = LoggerFactory.Create(builder => {
                builder.AddSimpleConsole(options => {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "[HH:mm:ss] ";
                        options.UseUtcTimestamp = false;
                        options.ColorBehavior = LoggerColorBehavior.Enabled;
                    })
                    .SetMinimumLevel(minLogLevel);
            })
            .CreateLogger(tag);
        return logger;
    }

    public ILogger FromModule(string tag) {
        if (Loggers.TryGetValue(tag, out var logger)) {
            return logger;
        }

        var newLogger = CreateLogger(MinLogLevel, tag);
        Loggers.TryAdd(tag, newLogger);
        return newLogger;
    }

    public ILogger FromModule(LogSource source) {
        return FromModule(source.ToString());
    }
}

public class FromTraceAppLogger : AppLogger {
    protected override LogLevel MinLogLevel => LogLevel.Trace;
}

public class FromDebugAppLogger : AppLogger {
    protected override LogLevel MinLogLevel => LogLevel.Debug;
}

public class FromInfoAppLogger : AppLogger {
    protected override LogLevel MinLogLevel => LogLevel.Information;
}

public class FromWarnAppLogger : AppLogger {
    protected override LogLevel MinLogLevel => LogLevel.Warning;
}

public class FromErrorAppLogger : AppLogger {
    protected override LogLevel MinLogLevel => LogLevel.Error;
}

public class FromCriticalAppLogger : AppLogger {
    protected override LogLevel MinLogLevel => LogLevel.Critical;
}

public class FromNoneAppLogger : AppLogger {
    protected override LogLevel MinLogLevel => LogLevel.None;
}