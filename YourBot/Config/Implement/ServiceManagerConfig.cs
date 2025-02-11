using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;
using YourBot.ServiceManage;
using YourBot.Utils;

namespace YourBot.Config.Implement;

public class ServiceManagerConfig : IReadConfig<ServiceManagerConfig> {
    public readonly List<ServiceName> ActiveServices;

    private ServiceManagerConfig(List<ServiceName> activeServices) {
        ActiveServices = activeServices;
    }

    public static List<string> GetConfigName() {
        return ["ServiceManagerConfig"];
    }

    public static ServiceManagerConfig CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        var serviceManagerConfig = configurations["ServiceManagerConfig"];
        return new ServiceManagerConfig(Util.ReadJsonList<ServiceName>(serviceManagerConfig, "ActiveServices"));
    }
}