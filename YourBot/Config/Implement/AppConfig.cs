using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YourBot.Config.Interface;
using YourBot.Login;

namespace YourBot.Config.Implement;

public class AppConfig : IReadConfig<AppConfig>, IWriteConfig {
    public AppConfig(LogLevel minLogLevel, LoginType loginType, uint? uin, string signServerUrl, string signProxyUrl) {
        MinLogLevel = minLogLevel;
        LoginType = loginType;
        Uin = uin;
        SignServerUrl = signServerUrl;
        SignProxyUrl = signProxyUrl;
    }

    public LogLevel MinLogLevel { get; set; }

    public LoginType LoginType { get; set; }
    public uint? Uin { get; set; }

    public string SignServerUrl { get; set; }
    public string SignProxyUrl { get; set; }

    public static List<string> GetConfigName() {
        return ["AppConfig"];
    }

    public static AppConfig CreateConfig(Dictionary<string, IConfigurationRoot> configurations) {
        return new AppConfig(
            Enum.Parse<LogLevel>(configurations["AppConfig"]["MinLoggingLevel"]!),
            Enum.Parse<LoginType>(configurations["AppConfig"]["Login:LoginType"]!),
            uint.Parse(configurations["AppConfig"]["Login:Uin"]!),
            configurations["AppConfig"]["Sign:SignServerUrl"]!,
            configurations["AppConfig"]["Sign:SignProxyUrl"]!
        );
    }

    public void WriteToFile(string path) {
        var jsonMainConfig = new {
            MinLoggingLevel = MinLogLevel.ToString(),
            Login = new {
                LoginType = LoginType.ToString(),
                Uin = Uin?.ToString() ?? ""
            },
            Sign = new {
                SignServerUrl, SignProxyUrl
            }
        };

        File.WriteAllText(path, JsonSerializer.Serialize(jsonMainConfig, ConfigManager.Options));
    }

    public static string GetMainWriteConfigName() {
        return "AppConfig";
    }
}