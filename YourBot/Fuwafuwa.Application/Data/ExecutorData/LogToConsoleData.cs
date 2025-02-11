using Fuwafuwa.Core.Data.ServiceData.Level1;
using Fuwafuwa.Core.Subjects;
using Microsoft.Extensions.Logging;
using YourBot.Fuwafuwa.Application.Attribute.Executor;

namespace YourBot.Fuwafuwa.Application.Data.ExecutorData;

public class LogToConsoleData : AExecutorData {
    public LogToConsoleData(Priority priority, string message, string source, LogLevel logLevel) : base(priority,
        typeof(CanLogToConsoleAttribute)) {
        Message = message;
        Source = source;
        LogLevel = logLevel;
    }

    public string Message { get; set; }

    public string Source { get; set; }

    public LogLevel LogLevel { get; set; }
}