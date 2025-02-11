using Microsoft.Extensions.Logging;
using YourBot.Logger;

namespace YourBot.Factory;

public partial class AppFactory {
    public AppLogger CreateAppLogger() {
        return _appConfig.MinLogLevel switch {
            LogLevel.Trace => new FromTraceAppLogger(),
            LogLevel.Debug => new FromDebugAppLogger(),
            LogLevel.Information => new FromInfoAppLogger(),
            LogLevel.Warning => new FromWarnAppLogger(),
            LogLevel.Error => new FromErrorAppLogger(),
            LogLevel.Critical => new FromCriticalAppLogger(),
            LogLevel.None => new FromNoneAppLogger(),
            _ => throw new ArgumentOutOfRangeException(nameof(_appConfig.MinLogLevel), _appConfig.MinLogLevel,
                "Invalid log level")
        };
    }
}