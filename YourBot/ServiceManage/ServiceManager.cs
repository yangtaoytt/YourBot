using Fuwafuwa.Core.Container.Level3;
using Fuwafuwa.Core.Data.ServiceData.Level1;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Data.SubjectData.Level2;
using Fuwafuwa.Core.Distributor.Implement;
using Fuwafuwa.Core.Distributor.Interface;
using Fuwafuwa.Core.Env;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.ServiceRegister;
using YourBot.Config.Implement;
using YourBot.Config.Implement.Level1.Service;

namespace YourBot.ServiceManage;

public enum ServiceName {
    EventInput,
    EventRouterProcessor,
    GroupEventProcessor,
    GroupMessageLogProcessor,

    // ReSharper disable InconsistentNaming
    AIReviewProcessor,
    // ReSharper restore InconsistentNaming
    AntiPlusOneProcessor,
    GroupCommandProcessor,
    PingPongProcessor,
    VersionProcessor,
    MemeProcessor,
    ServiceProcessor,
    AntiFloodProcessor,
    ActorProcessor,
    FriendEventProcessor,
    FriendCommandProcessor,
    
    SendGroupMessageExecutor,
    LogToConsoleExecutor,
    RevokeGroupMessageExecutor,
    MuteGroupMemberExecutor,
    SendMessage2FriendExecutor,
    
    
    HomeworkDeadlineRemindInput,
    HomeworkDeadlineRemindProcessor,
    HomeworkProcessor,
    Message2CombineDataProcessor,
    SubmitCheckAndSaveProcessor,
    SubmitCollectProcessor
}

public class ServiceManager {
    private readonly Dictionary<ServiceName, Type> _name2Type;
    private readonly ServiceManagerConfig _serviceManagerConfig;
    private readonly Dictionary<Type, ServiceName> _type2Name;
    public readonly Env Env;

    public ServiceManager(ServiceManagerConfig serviceManagerConfig, Env env) {
        _serviceManagerConfig = serviceManagerConfig;
        Env = env;
        _name2Type = [];
        _type2Name = [];
    }

    public async Task<InputHandler<TInputData>> SignInput<TInputCore, TInputData,
        TSharedData, TInitData>(ServiceName serviceName, TInitData initData)
        where TInputCore : IInputCore<TSharedData, TInitData>, new()
        where TInitData : new()
        where TSharedData : ISharedDataWrapper {
        Type serviceType;
        InputHandler<TInputData> inputHandler;
        if (_serviceManagerConfig.ActiveServices.Contains(serviceName)) {
            (serviceType, inputHandler) = await Env.CreateRunRegisterPollingInput<TInputCore, TInputData,
                TSharedData, TInitData>(initData);
        } else {
            (serviceType, inputHandler) = Env.CreateRunPollingInput<TInputCore, TInputData,
                TSharedData, TInitData>(initData);
        }

        _name2Type[serviceName] = serviceType;
        _type2Name[serviceType] = serviceName;

        return inputHandler;
    }

    public async Task SignPollingProcessor<TProcessorCore, TServiceData, TSharedData, TInitData>(ServiceName serviceName,
        TInitData initData)
        where TServiceData : IProcessorData
        where TProcessorCore : IProcessorCore<TServiceData, TSharedData, TInitData>, new()
        where TSharedData : ISharedDataWrapper {
        await SignProcessor<TProcessorCore, TServiceData,
            PollingDistributor<TServiceData, SubjectDataWithCommand, (SimpleSharedDataWrapper<Register>, TSharedData)>,
            TSharedData, TInitData>(serviceName, initData);
    }
    
    public async Task SignProcessor<TProcessorCore, TServiceData,TDistributor, TSharedData, TInitData>(ServiceName serviceName,
        TInitData initData)
        where TServiceData : IProcessorData
        where TProcessorCore : IProcessorCore<TServiceData, TSharedData, TInitData>, new()
        where TSharedData : ISharedDataWrapper
        where TDistributor : IDistributor<TServiceData, SubjectDataWithCommand, (SimpleSharedDataWrapper<Register>, TSharedData)>, new() {
        var serviceType =
            Env.CreateRunProcessor<TProcessorCore, TServiceData,TDistributor, TSharedData, TInitData>(
                initData);
        if (_serviceManagerConfig.ActiveServices.Contains(serviceName)) {
            await Env.Register(serviceType);
        }
        _name2Type[serviceName] = serviceType;
        _type2Name[serviceType] = serviceName;
    }

    public async Task SignExecutor<TExecutorCore, TServiceData, TSharedData, TInitData>(ServiceName serviceName,
        TInitData initData)
        where TServiceData : AExecutorData
        where TExecutorCore : IExecutorCore<TServiceData, TSharedData, TInitData>, new()
        where TSharedData : ISharedDataWrapper {
        Type serviceType;
        if (_serviceManagerConfig.ActiveServices.Contains(serviceName)) {
            serviceType =
                await Env.CreateRunRegisterPollingExecutor<TExecutorCore, TServiceData, TSharedData, TInitData>(
                    initData);
        } else {
            serviceType =
                Env.CreateRunPollingExecutor<TExecutorCore, TServiceData, TSharedData, TInitData>(
                    initData);
        }

        _name2Type[serviceName] = serviceType;
        _type2Name[serviceType] = serviceName;
    }

    public List<(ServiceName, bool)> GetServiceStatus() {
        return Env.GetServiceStatus().Select(tuple => (_type2Name[tuple.serviceType], tuple.isRegister)).ToList();
    }

    public async Task<bool> UnRegisterService(ServiceName serviceName) {
        if (!_name2Type.TryGetValue(serviceName, value: out var value)) {
            return false;
        }

        await Env.UnRegister(value);
        return true;

    }

    public async Task<bool> RegisterService(ServiceName serviceName) {
        if (!_name2Type.TryGetValue(serviceName, value: out var value)) {
            return false;
        }

        await Env.Register(value);
        return true;

    }
}