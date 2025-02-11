using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Microsoft.Extensions.Logging;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Logger;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Executor;

public class LogToConsoleExecutor : IExecutorCore<LogToConsoleData, SimpleSharedDataWrapper<AppLogger>, AppLogger> {
    public static IServiceAttribute<LogToConsoleData> GetServiceAttribute() {
        return CanLogToConsoleAttribute.GetInstance();
    }

    public static SimpleSharedDataWrapper<AppLogger> Init(AppLogger initData) {
        return new SimpleSharedDataWrapper<AppLogger>(initData);
    }

    public static void Final(SimpleSharedDataWrapper<AppLogger> sharedData, Logger2Event? logger) { }

    public Task ExecuteTask(LogToConsoleData data, SimpleSharedDataWrapper<AppLogger> sharedData,
        Logger2Event? logger) {
        sharedData.Execute(appLogger => {
            appLogger.Value.FromModule(data.Source).Log(data.LogLevel, "{}", data.Message);
        });
        return Task.CompletedTask;
    }
}