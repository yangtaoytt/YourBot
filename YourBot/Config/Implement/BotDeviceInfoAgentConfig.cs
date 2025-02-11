using System.Text.Json;
using Lagrange.Core.Common;
using Microsoft.Extensions.Configuration;
using YourBot.Config.Interface;

namespace YourBot.Config.Implement;

public class BotDeviceInfoAgentConfig : IReadConfig<BotDeviceInfoAgentConfig>, IWriteConfig {
    public BotDeviceInfoAgentConfig(BotDeviceInfo? deviceInfo) {
        DeviceInfo = deviceInfo;
    }

    public BotDeviceInfo? DeviceInfo { get; set; }


    public static List<string> GetConfigName() {
        return ["DeviceInfoConfig"];
    }

    public static BotDeviceInfoAgentConfig CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new BotDeviceInfoAgentConfig(configurations["DeviceInfoConfig"].Get<BotDeviceInfo>() ?? null);
    }

    public void WriteToFile(string path) {
        if (DeviceInfo == null) {
            return;
        }

        File.WriteAllText(path, JsonSerializer.Serialize(DeviceInfo, ConfigManager.Options));
    }

    public static string GetMainWriteConfigName() {
        return "DeviceInfoConfig";
    }
}