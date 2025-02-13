using YourBot.Config.Interface;
using YourBot.ServiceManage;

namespace YourBot.Config.Implement.Level1.Service;

public class ServiceManagerConfig : IConfig {
    public List<ServiceName> ActiveServices { get; init; } = [];
    
    public static string GetConfigName() {
        return "ServiceManagerConfig";
    }
}