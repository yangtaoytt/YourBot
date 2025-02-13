using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using YourBot.Config.Implement.Level1.Service.Group.Command;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.ServiceManage;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command;

public class ServiceRunProcessor : IProcessorCore<CommandData,
    AsyncSharedDataWrapper<(Func<List<(ServiceName, bool)>> getStatusFunc, Func<ServiceName, Task<bool>> registerFunc,
        Func<ServiceName, Task<bool>> unregisterFunc, ServiceRunConfig)>, (Func<List<(ServiceName, bool)>> getStatusFunc,
    Func<ServiceName, Task<bool>> registerFunc, Func<ServiceName, Task<bool>> unregisterFunc, ServiceRunConfig)> {
    public static IServiceAttribute<CommandData> GetServiceAttribute() {
        return ReadGroupCommandAttribute.GetInstance();
    }

    public static AsyncSharedDataWrapper<(Func<List<(ServiceName, bool)>> getStatusFunc, Func<ServiceName, Task<bool>>
        registerFunc, Func<ServiceName, Task<bool>> unregisterFunc, ServiceRunConfig)> Init(
        (Func<List<(ServiceName, bool)>> getStatusFunc, Func<ServiceName, Task<bool>> registerFunc,
            Func<ServiceName, Task<bool>> unregisterFunc, ServiceRunConfig) initData) {
        return new AsyncSharedDataWrapper<(Func<List<(ServiceName, bool)>> getStatusFunc, Func<ServiceName, Task<bool>>
            registerFunc, Func<ServiceName, Task<bool>> unregisterFunc, ServiceRunConfig)>(initData);
    }

    public static void Final(
        AsyncSharedDataWrapper<(Func<List<(ServiceName, bool)>> getStatusFunc, Func<ServiceName, Task<bool>>
            registerFunc, Func<ServiceName, Task<bool>> unregisterFunc, ServiceRunConfig)> sharedData,
        Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(CommandData data,
        AsyncSharedDataWrapper<(Func<List<(ServiceName, bool)>> getStatusFunc, Func<ServiceName, Task<bool>>
            registerFunc, Func<ServiceName, Task<bool>> unregisterFunc, ServiceRunConfig)> sharedData,
        Logger2Event? logger) {
        var config = await sharedData.ExecuteAsync(initData => Task.FromResult(initData.Value.Item4));

        var groupUin = data.GroupUin;
        var memberUin = data.MessageChain.FriendUin;
        if (!Utils.Util.CheckGroupMemberPermission(config, groupUin, memberUin)) {
            return [];
        }

        var command = data.Command;
        if (command != "service") {
            return [];
        }

        var parameters = data.Parameters;
        if (parameters.Count == 0) {
            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(Util.BuildSendToGroupMessageData(
                        data.GroupUin, config.Priority, "wrong parameters"))
            ];
        }

        if (parameters.Count < 2) {
            return [Util.SendToGroupMessage(data.GroupUin, config.Priority, "wrong parameters")];
        }

        var commandType = parameters[0];

        if (commandType == "ls") {
            var subCommand = bool.Parse(parameters[1]);

            var status = await sharedData.ExecuteAsync(reference => Task.FromResult(reference.Value.getStatusFunc()));
            var message = status.Where(tuple => tuple.Item2 == subCommand)
                .Select(s => $"{s.Item1} : {s.Item2}")
                .Aggregate((s1, s2) => $"{s1}\n{s2}");
            return [
                Util.SendToGroupMessage(data.GroupUin, config.Priority, message)
            ];
        }

        var service = Enum.Parse<ServiceName>(parameters[1]);
        if (commandType == "add") {
            var registerResult =
                await sharedData.ExecuteAsync(async reference => await reference.Value.registerFunc(service));
            return [
                Util.SendToGroupMessage(data.GroupUin, config.Priority, registerResult ? "add success" : "add failed")
            ];
        }

        if (commandType == "rm") {
            var unregisterResult =
                await sharedData.ExecuteAsync(async reference => await reference.Value.unregisterFunc(service));
            return [
                Util.SendToGroupMessage(data.GroupUin, config.Priority,
                    unregisterResult ? "remove success" : "remove failed")
            ];
        }

        return [Util.SendToGroupMessage(data.GroupUin, config.Priority, "wrong command")];
    }
}