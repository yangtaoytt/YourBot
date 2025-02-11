using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.Data.InitData.Group.Command;

public class ServiceInitData : IReadConfig<ServiceInitData> {
    public ServiceInitData(List<uint> operatorId, int priority) {
        OperatorId = operatorId;
        Priority = priority;
    }

    public List<uint> OperatorId { get; private init; }

    public int Priority { get; init; }

    public static List<string> GetConfigName() {
        return ["ServiceConfig"];
    }

    public static ServiceInitData CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new ServiceInitData(Util.ReadJsonList<uint>(configurations["ServiceConfig"], "OperatorId"),
            int.Parse(configurations["ServiceConfig"]["Priority"]!));
    }
}