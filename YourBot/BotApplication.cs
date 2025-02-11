using System.Text;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Env;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.Log.LogEventArgs.Interface;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Event;
using Microsoft.Extensions.Logging;
using YourBot.AI.Implement;
using YourBot.AI.Interface;
using YourBot.Config;
using YourBot.Config.Implement;
using YourBot.Factory;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.InitData.Group;
using YourBot.Fuwafuwa.Application.Data.InitData.Group.Command;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Fuwafuwa.Application.ServiceCore.Executor;
using YourBot.Fuwafuwa.Application.ServiceCore.Input;
using YourBot.Fuwafuwa.Application.ServiceCore.Processor;
using YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group;
using YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command;
using YourBot.Logger;
using YourBot.Login;
using YourBot.ServiceManage;
using YourBot.Utils;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace YourBot;

public class BotApplication : IDisposable {
    private const int NameMaxWidth = 25; // > 3
    private const int ModuleMaxWidth = 12; // > 3
    private readonly AppConfig _appConfig;
    private readonly AppInfoAgentConfig _appInfo;
    private readonly AppLogger _appLogger;
    private readonly BotConfigAgentConfig _botConfig;

    private readonly BotContext _botContext;

    private readonly ConfigManager _configManager;


    private readonly BotDeviceInfoAgentConfig _deviceInfo;
    private readonly BotKeystoreAgentConfig _keystore;
    private readonly ILoginStrategy _loginStrategy;

    private ServiceManager? _serviceManager;

    public BotApplication(string mainConfigPath) {
        _configManager = new ConfigManager(mainConfigPath);

        _appConfig = _configManager.CreateConfig<AppConfig>();
        if (_appConfig.LoginType == LoginType.QrCode) {
            var task = LoginSigner.CreateAppInfoAsync(_appConfig.SignServerUrl,
                _appConfig.SignProxyUrl);
            task.Wait();
            _appInfo = new AppInfoAgentConfig(task.Result);
            _configManager.MainConfig.Configurations["AppInfoConfig"] = "AppInfoConfig.json";
        } else if (_appConfig.LoginType == LoginType.KeyStore) {
            _appInfo = _configManager.CreateConfig<AppInfoAgentConfig>();
        } else {
            throw new Exception("Login type not supported");
        }

        _deviceInfo = _configManager.CreateConfig<BotDeviceInfoAgentConfig>();
        _keystore = _configManager.CreateConfig<BotKeystoreAgentConfig>();
        _botConfig = _configManager.CreateConfig<BotConfigAgentConfig>();

        var appFactory = new AppFactory(_appConfig, _appInfo, _deviceInfo, _botConfig, _keystore,
            _configManager.MainConfig);
        _appLogger = appFactory.CreateAppLogger();
        _loginStrategy = appFactory.CreateLoginStrategy();
        _botContext = appFactory.CreateBotContext();
    }

    public void Dispose() {
        _serviceManager!.Env.Close();

        _configManager.WriteMainConfig();
        _configManager.WriteConfig(_appConfig);
        _configManager.WriteConfig(_deviceInfo);
        _configManager.WriteConfig(_keystore);
        _configManager.WriteConfig(_botConfig);
        _configManager.WriteConfig(_appInfo);
    }

    public async Task Run() {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        _botContext.Invoker.OnBotLogEvent += (sender, e) => {
            _appLogger.FromModule(LogSource.Lagrange).Log(Util.LogLevelConvert(e.Level), e.EventMessage);
        };

        if (!await _loginStrategy.Login(_deviceInfo, _keystore, _configManager.MainConfig, _botContext, _appLogger)) {
            return;
        }

        await RunServices();
    }

    private static void OutputHandler(LogLevel logLevel, object? sender, BaseLogEventArgs args, AppLogger appLogger) {
        var source = sender?.GetType().Name ?? "Unknown";
        var name = source.Length > NameMaxWidth ? "..." + source.Substring(source.Length - NameMaxWidth + 3) : source;
        var module = source.Substring(source.LastIndexOf(source.Last(char.IsUpper)));
        var res = module.Length > ModuleMaxWidth
            ? "..." + module.Substring(module.Length - ModuleMaxWidth + 3)
            : module;
        appLogger.FromModule($"{name}:\t[{res}]").Log(logLevel, args.Message);
    }

    private async Task RunServices() {
        const int serviceCount = 1;

        var closeAi = new CloseAI();

        var logger2Event = new Logger2Event();
        logger2Event.ErrorLogGenerated += (sender, e) => { OutputHandler(LogLevel.Error, sender, e, _appLogger); };
        logger2Event.InfoLogGenerated += (sender, e) => { OutputHandler(LogLevel.Information, sender, e, _appLogger); };
        logger2Event.DebugLogGenerated += (sender, e) => { OutputHandler(LogLevel.Debug, sender, e, _appLogger); };


        _serviceManager = new ServiceManager(_configManager.CreateConfig<ServiceManagerConfig>(),
            new Env(serviceCount, logger2Event));

        var inputHandler =
            await _serviceManager.SignInput<EventInput, EventBase, NullSharedDataWrapper<object>, object>(
                ServiceName.EventInput, new object());

        await _serviceManager.SignProcessor<EventRouterProcessor, EventData, NullSharedDataWrapper<object>,
            object>(ServiceName.EventRouterProcessor, new object());


        await _serviceManager.SignProcessor<GroupEventProcessor, GroupEventData,
            SimpleSharedDataWrapper<GroupEventInitData>, GroupEventInitData>(ServiceName.GroupEventProcessor,
            _configManager
                .CreateConfig<GroupEventInitData>());
        await _serviceManager.SignProcessor<GroupMessageLogProcessor, MessageData,
            AsyncSharedDataWrapper<(AppLogger, BotContext, GroupMessageLogInitData,
                Dictionary<uint, (BotGroup, List<BotGroupMember>?)>)>, (AppLogger, BotContext,
            GroupMessageLogInitData)>(ServiceName.GroupMessageLogProcessor, (_appLogger, _botContext,
            _configManager.CreateConfig<GroupMessageLogInitData>()));
        await _serviceManager.SignProcessor<AIReviewProcessor, MessageData,
            AsyncSharedDataWrapper<(IAI, AIReviewInitData)>, (IAI, AIReviewInitData)>(ServiceName.AIReviewProcessor,
            (closeAi,
                _configManager.CreateConfig<AIReviewInitData>()));
        await _serviceManager.SignProcessor<AntiPlusOneProcessor, MessageData,
            NullSharedDataWrapper<AntiPlusOneInitData>, AntiPlusOneInitData>(ServiceName.AntiPlusOneProcessor,
            _configManager.CreateConfig<AntiPlusOneInitData>());

        await _serviceManager.SignProcessor<GroupCommandProcessor, MessageData, NullSharedDataWrapper<(string, uint)>,
            BotContext>(ServiceName.GroupCommandProcessor, _botContext);

        await _serviceManager.SignProcessor<PingPongProcessor, CommandData,
            NullSharedDataWrapper<PingPongInitData>, PingPongInitData>(ServiceName.PingPongProcessor,
            _configManager.CreateConfig<PingPongInitData>());
        await _serviceManager.SignProcessor<VersionProcessor, CommandData, NullSharedDataWrapper<VersionInitData>
            , VersionInitData>(ServiceName.VersionProcessor, _configManager.CreateConfig<VersionInitData>());
        await _serviceManager.SignProcessor<MemeProcessor, CommandData,
            AsyncSharedDataWrapper<(BotContext botContext, MemeInitData memeInitData)>, (BotContext botContext,
            MemeInitData memeInitData)>(ServiceName.MemeProcessor,
            (_botContext, _configManager.CreateConfig<MemeInitData>()));
        await _serviceManager
            .SignProcessor<ServiceProcessor, CommandData,
                AsyncSharedDataWrapper<(Func<List<(ServiceName, bool)>> getStatusFunc, Func<ServiceName, Task<bool>>
                    registerFunc, Func<ServiceName, Task<bool>> unregisterFunc, ServiceInitData)>, (
                Func<List<(ServiceName, bool)>> getStatusFunc, Func<ServiceName, Task<bool>> registerFunc,
                Func<ServiceName, Task<bool>> unregisterFunc, ServiceInitData)>(
                ServiceName.ServiceProcessor,
                (_serviceManager.GetServiceStatus, _serviceManager.RegisterService, _serviceManager.UnRegisterService,
                    _configManager.CreateConfig<ServiceInitData>()));


        await _serviceManager.SignExecutor<SendGroupMessageExecutor, SendToGroupMessageData,
            AsyncSharedDataWrapper<BotContext>, BotContext>(ServiceName.SendGroupMessageExecutor, _botContext);
        await _serviceManager.SignExecutor<LogToConsoleExecutor, LogToConsoleData, SimpleSharedDataWrapper<AppLogger>
            , AppLogger>(ServiceName.LogToConsoleExecutor, _appLogger);
        await _serviceManager.SignExecutor<RevokeGroupMessageExecutor, RevokeGroupMessageData,
            AsyncSharedDataWrapper<BotContext>, BotContext>(ServiceName.RevokeGroupMessageExecutor, _botContext);


        _botContext.Invoker.OnGroupMessageReceived += (sender, e) => { inputHandler.Input(e); };
    }
}