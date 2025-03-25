using System.Text;
using System.Text.RegularExpressions;
using Fuwafuwa.Core.Container.Level3;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Env;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.Log.LogEventArgs.Interface;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Event;
using Lagrange.Core.Message.Entity;
using Microsoft.Extensions.Logging;
using YourBot.AI.Implement;
using YourBot.AI.Interface;
using YourBot.Config;
using YourBot.Config.Implement.Level1;
using YourBot.Config.Implement.Level1.Service;
using YourBot.Config.Implement.Level1.Service.Friend;
using YourBot.Config.Implement.Level1.Service.Friend.Command;
using YourBot.Config.Implement.Level1.Service.Group;
using YourBot.Config.Implement.Level1.Service.Group.Command;
using YourBot.Config.Implement.Level1.Service.Group.Command.Homework;
using YourBot.Factory;
using YourBot.Fuwafuwa.Application.Data;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.ExecutorData.Friend;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;
using YourBot.Fuwafuwa.Application.Distributor;
using YourBot.Fuwafuwa.Application.ServiceCore.Executor;
using YourBot.Fuwafuwa.Application.ServiceCore.Executor.Friend;
using YourBot.Fuwafuwa.Application.ServiceCore.Input;
using YourBot.Fuwafuwa.Application.ServiceCore.Processor;
using YourBot.Fuwafuwa.Application.ServiceCore.Processor.Friend;
using YourBot.Fuwafuwa.Application.ServiceCore.Processor.Friend.Command;
using YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group;
using YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command;
using YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command.Homework;
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
    private readonly AppLogger _appLogger;
    private readonly BotAppInfoAgentConfig _botAppInfo;
    private readonly BotConfigAgentConfig _botConfig;

    private readonly BotContext _botContext;

    private readonly ConfigManager _configManager;


    private readonly BotDeviceInfoAgentConfig _deviceInfo;
    private readonly BotKeyStoreAgentConfig _keystore;
    private readonly ILoginStrategy _loginStrategy;

    private ServiceManager? _serviceManager;

    public BotApplication(string mainConfigPath) {
        if (!CheckConfig(mainConfigPath)) {
            throw new Exception("Config file not found, default config generated.Please fill in the config file.");
        }

        _configManager = new ConfigManager(mainConfigPath);

        _appConfig = _configManager.ReadConfig<AppConfig>();
        _botAppInfo = _configManager.ReadConfig<BotAppInfoAgentConfig>();
        if (_appConfig.LoginType == LoginType.QrCode) {
            var task = LoginSigner.CreateAppInfoAsync(_appConfig.SignServerUrl,
                _appConfig.SignProxyUrl);
            task.Wait();
            _botAppInfo.BotAppInfo = task.Result;
        }

        _deviceInfo = _configManager.ReadConfig<BotDeviceInfoAgentConfig>();
        _keystore = _configManager.ReadConfig<BotKeyStoreAgentConfig>();
        _botConfig = _configManager.ReadConfig<BotConfigAgentConfig>();

        var appFactory = new AppFactory(_appConfig, _botAppInfo, _deviceInfo, _botConfig, _keystore);
        _appLogger = appFactory.CreateAppLogger();
        _loginStrategy = appFactory.CreateLoginStrategy();
        _botContext = appFactory.CreateBotContext();
    }

    public void Dispose() {
        _serviceManager?.Env.Close();

        _configManager.WriteConfig(_appConfig);
        _configManager.WriteConfig(_deviceInfo);
        _configManager.WriteConfig(_keystore);
        _configManager.WriteConfig(_botConfig);
        _configManager.WriteConfig(_botAppInfo);
    }

    private static bool CheckConfig(string mainConfigPath) {
        if (File.Exists(mainConfigPath)) {
            return true;
        }

        ConfigManager.SignDefaultConfig<AppConfig>();
        ConfigManager.SignDefaultConfig<BotAppInfoAgentConfig>();
        ConfigManager.SignDefaultConfig<BotConfigAgentConfig>();
        ConfigManager.SignDefaultConfig<BotDeviceInfoAgentConfig>();
        ConfigManager.SignDefaultConfig<BotKeyStoreAgentConfig>();
        ConfigManager.SignDefaultConfig<DatabaseConfig>();

        ConfigManager.SignDefaultConfig<ServiceManagerConfig>();

        ConfigManager.SignDefaultConfig<AIReviewConfig>();
        ConfigManager.SignDefaultConfig<AntiPlusOneConfig>();
        ConfigManager.SignDefaultConfig<GlobalGroupActiveConfig>();
        ConfigManager.SignDefaultConfig<GlobalFriendActiveConfig>();
        ConfigManager.SignDefaultConfig<GroupMessageLogConfig>();
        ConfigManager.SignDefaultConfig<AntiFloodConfig>();

        ConfigManager.SignDefaultConfig<MemeConfig>();
        ConfigManager.SignDefaultConfig<PingPongConfig>();
        ConfigManager.SignDefaultConfig<ServiceRunConfig>();
        ConfigManager.SignDefaultConfig<VersionConfig>();

        ConfigManager.SignDefaultConfig<ActorConfig>();
        ConfigManager.SignDefaultConfig<HomeworkDeadlineRemindInputConfig>();
        ConfigManager.SignDefaultConfig<SubmitCheckAndSaveConfig>();
        ConfigManager.SignDefaultConfig<SubmitCollectConfig>();
        ConfigManager.SignDefaultConfig<HomeworkConfig>();
        
        ConfigManager.SignDefaultConfig<JmConfig>();

        ConfigManager.GenerateDefaultConfigOnDisk(Path.Combine(".", "MainConfig.json"));
        return false;
    }

    public async Task Run() {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        _botContext.Invoker.OnBotLogEvent += (sender, e) => {
            _appLogger.FromModule(LogSource.Lagrange).Log(YourBotUtil.LogLevelConvert(e.Level), e.EventMessage);
        };

        if (!await _loginStrategy.Login(_deviceInfo, _keystore, _botContext, _appLogger)) {
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
        const int serviceCount = 5;

        var closeAi = new CloseAI();

        var logger2Event = new Logger2Event();
        logger2Event.ErrorLogGenerated += (sender, e) => { OutputHandler(LogLevel.Error, sender, e, _appLogger); };
        logger2Event.InfoLogGenerated += (sender, e) => { OutputHandler(LogLevel.Information, sender, e, _appLogger); };
        logger2Event.DebugLogGenerated += (sender, e) => { OutputHandler(LogLevel.Debug, sender, e, _appLogger); };


        _serviceManager = new ServiceManager(_configManager.ReadConfig<ServiceManagerConfig>(),
            new Env(serviceCount, logger2Event));

        var inputHandler =
            await _serviceManager.SignInput<EventInput, EventBase, NullSharedDataWrapper<object>, object>(
                ServiceName.EventInput, new object());

        await _serviceManager.SignPollingProcessor<EventRouterProcessor, EventData, NullSharedDataWrapper<object>,
            object>(ServiceName.EventRouterProcessor, new object());


        await _serviceManager.SignPollingProcessor<GroupEventProcessor, GroupEventData,
            SimpleSharedDataWrapper<GlobalGroupActiveConfig>, GlobalGroupActiveConfig>(ServiceName.GroupEventProcessor,
            _configManager
                .ReadConfig<GlobalGroupActiveConfig>());
        await _serviceManager
            .SignPollingProcessor<FriendEventProcessor, FriendEventData, NullSharedDataWrapper<GlobalFriendActiveConfig>
                , GlobalFriendActiveConfig>(ServiceName.FriendEventProcessor,
                _configManager.ReadConfig<GlobalFriendActiveConfig>());
        await _serviceManager.SignPollingProcessor<GroupMessageLogProcessor, MessageData,
            AsyncSharedDataWrapper<(AppLogger, BotContext, GroupMessageLogConfig,
                Dictionary<uint, (BotGroup, List<BotGroupMember>?)>)>, (AppLogger, BotContext,
            GroupMessageLogConfig)>(ServiceName.GroupMessageLogProcessor, (_appLogger, _botContext,
            _configManager.ReadConfig<GroupMessageLogConfig>()));
        await _serviceManager
            .SignProcessor<AIReviewProcessor, MessageData,
                QQUinDistributor<AsyncSharedDataWrapper<(IAI, AIReviewConfig)>>,
                AsyncSharedDataWrapper<(IAI, AIReviewConfig)>, (IAI, AIReviewConfig)>(ServiceName.AIReviewProcessor,
                (closeAi,
                    _configManager.ReadConfig<AIReviewConfig>()));
        await _serviceManager
            .SignProcessor<AntiPlusOneProcessor, MessageData, QQUinDistributor<NullSharedDataWrapper<AntiPlusOneConfig>>
                ,
                NullSharedDataWrapper<AntiPlusOneConfig>, AntiPlusOneConfig>(ServiceName.AntiPlusOneProcessor,
                _configManager.ReadConfig<AntiPlusOneConfig>());

        await _serviceManager
            .SignPollingProcessor<GroupCommandProcessor, MessageData, NullSharedDataWrapper<(string, uint)>,
                BotContext>(ServiceName.GroupCommandProcessor, _botContext);

        await _serviceManager
            .SignPollingProcessor<FriendCommandProcessor, MessageData, NullSharedDataWrapper<object>, object>(
                ServiceName.FriendCommandProcessor, new object());
        await _serviceManager.SignPollingProcessor<PingPongProcessor, GroupCommandData,
            NullSharedDataWrapper<PingPongConfig>, PingPongConfig>(ServiceName.PingPongProcessor,
            _configManager.ReadConfig<PingPongConfig>());
        await _serviceManager
            .SignPollingProcessor<VersionProcessor, GroupCommandData, NullSharedDataWrapper<(VersionConfig versionConfig
                    , DatabaseConfig databaseConfig)>
                , (VersionConfig versionConfig, DatabaseConfig databaseConfig)>(ServiceName.VersionProcessor,
                (_configManager.ReadConfig<VersionConfig>(), _configManager.ReadConfig<DatabaseConfig>()));
        await _serviceManager.SignPollingProcessor<MemeProcessor, GroupCommandData,
            AsyncSharedDataWrapper<(BotContext botContext, (MemeConfig memeConfig, DatabaseConfig databaseConfig)
                configs)>, (BotContext botContext, (MemeConfig memeConfig, DatabaseConfig databaseConfig)
            configs)>(ServiceName.MemeProcessor,
            (_botContext, (_configManager.ReadConfig<MemeConfig>(), _configManager.ReadConfig<DatabaseConfig>())));
        await _serviceManager
            .SignPollingProcessor<ServiceRunProcessor, GroupCommandData,
                AsyncSharedDataWrapper<(Func<List<(ServiceName, bool)>> getStatusFunc, Func<ServiceName, Task<bool>>
                    registerFunc, Func<ServiceName, Task<bool>> unregisterFunc, ServiceRunConfig)>, (
                Func<List<(ServiceName, bool)>> getStatusFunc, Func<ServiceName, Task<bool>> registerFunc,
                Func<ServiceName, Task<bool>> unregisterFunc, ServiceRunConfig)>(
                ServiceName.ServiceProcessor,
                (_serviceManager.GetServiceStatus, _serviceManager.RegisterService, _serviceManager.UnRegisterService,
                    _configManager.ReadConfig<ServiceRunConfig>()));
        await _serviceManager
            .SignProcessor<AntiFloodProcessor, MessageData, QQUinDistributor<NullSharedDataWrapper<AntiFloodConfig>>,
                NullSharedDataWrapper<AntiFloodConfig>, AntiFloodConfig>(ServiceName.AntiFloodProcessor,
                _configManager.ReadConfig<AntiFloodConfig>());
        await _serviceManager
            .SignPollingProcessor<ActorProcessor, FriendCommandData,
                AsyncSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext,
                    Dictionary<uint, string> userUinToName)>, (ActorConfig
                actorConfig, DatabaseConfig databaseConfig, BotContext botContext)>(ServiceName.ActorProcessor,
                (_configManager.ReadConfig<ActorConfig>(),
                    _configManager.ReadConfig<DatabaseConfig>(), _botContext));

        await _serviceManager.SignExecutor<SendGroupMessageExecutor, SendToGroupMessageData,
            AsyncSharedDataWrapper<BotContext>, BotContext>(ServiceName.SendGroupMessageExecutor, _botContext);
        await _serviceManager.SignExecutor<LogToConsoleExecutor, LogToConsoleData, SimpleSharedDataWrapper<AppLogger>
            , AppLogger>(ServiceName.LogToConsoleExecutor, _appLogger);
        await _serviceManager.SignExecutor<RevokeGroupMessageExecutor, RevokeGroupMessageData,
            AsyncSharedDataWrapper<BotContext>, BotContext>(ServiceName.RevokeGroupMessageExecutor, _botContext);
        await _serviceManager
            .SignExecutor<MuteGroupMemberExecutor, MuteGroupMemberData, AsyncSharedDataWrapper<BotContext>, BotContext>(
                ServiceName.MuteGroupMemberExecutor, _botContext);
        await _serviceManager
            .SignExecutor<SendMessage2FriendExecutor, SendMessage2FriendData, AsyncSharedDataWrapper<BotContext>,
                BotContext>(ServiceName.SendMessage2FriendExecutor, _botContext);

        var homeworkInput =
            await _serviceManager
                .SignInput<HomeworkDeadlineRemindInput, HomeworkDeadlineRemindInputData,
                    NullSharedDataWrapper<HomeworkDeadlineRemindInputConfig>, HomeworkDeadlineRemindInputConfig>(
                    ServiceName.HomeworkDeadlineRemindInput,
                    _configManager.ReadConfig<HomeworkDeadlineRemindInputConfig>());

        await _serviceManager.SignPollingProcessor<HomeworkDeadlineRemindProcessor, HomeworkData,
            SimpleSharedDataWrapper<(DatabaseConfig config, HomeworkDeadlineRemindInputConfig remindConfig,
                AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>> inputHandler,
                List<(Task task, CancellationTokenSource cancellationTokenSource, uint id)> initTasks)>, (DatabaseConfig
            , HomeworkDeadlineRemindInputConfig remindConfig,
            AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>>)>(
            ServiceName.HomeworkDeadlineRemindProcessor,
            (_configManager.ReadConfig<DatabaseConfig>(),
                _configManager.ReadConfig<HomeworkDeadlineRemindInputConfig>(),
                new AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>>(homeworkInput)));

        await _serviceManager
            .SignPollingProcessor<HomeworkProcessor, FriendCommandData,
                AsyncSharedDataWrapper<(HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig, BotContext
                    botContext)>, (HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig, BotContext botContext
                )>(ServiceName.HomeworkProcessor,
                (_configManager.ReadConfig<HomeworkConfig>(), _configManager.ReadConfig<DatabaseConfig>(),
                    _botContext));

        await _serviceManager
            .SignPollingProcessor<Message2CombineDataProcessor, MessageData, NullSharedDataWrapper<object>, object>(
                ServiceName.Message2CombineDataProcessor, new object());

        await _serviceManager
            .SignPollingProcessor<SubmitCheckAndSaveProcessor, SubmitData,
                NullSharedDataWrapper<(SubmitCheckAndSaveConfig submitConfig, DatabaseConfig databaseConfig)>, (
                SubmitCheckAndSaveConfig submitConfig, DatabaseConfig databaseConfig)>(
                ServiceName.SubmitCheckAndSaveProcessor,
                (_configManager.ReadConfig<SubmitCheckAndSaveConfig>(), _configManager.ReadConfig<DatabaseConfig>()));


        await _serviceManager.SignPollingProcessor<SubmitCollectProcessor, CombinedData,
            SimpleSharedDataWrapper<(Dictionary<uint, (List<FileEntity> submit, List<Regex> regexes, uint homeworkId)>
                dic, SubmitCollectConfig config)>, SubmitCollectConfig>(ServiceName.SubmitCollectProcessor,
            _configManager.ReadConfig<SubmitCollectConfig>());


        await _serviceManager
            .SignPollingProcessor<JmProcessor, GroupCommandData, NullSharedDataWrapper<JmConfig>, JmConfig>(
                ServiceName.JmProcessor, _configManager.ReadConfig<JmConfig>());


        _botContext.Invoker.OnGroupMessageReceived += (sender, e) => { inputHandler.Input(e); };
        _botContext.Invoker.OnFriendMessageReceived += (sender, e) => { inputHandler.Input(e); };
    }
}