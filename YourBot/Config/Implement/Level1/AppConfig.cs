using Microsoft.Extensions.Logging;
using YourBot.Config.Interface;
using YourBot.Login;

namespace YourBot.Config.Implement.Level1;

public class AppConfig : IConfig {

    public LogLevel MinLogLevel { get; init; } = LogLevel.Trace;

    public LoginType LoginType { get; init; } = LoginType.QrCode;
    public uint? Uin { get; init; } = 0;

    public string SignServerUrl { get; init; } = "";
    public string SignProxyUrl { get; init; } = "";
    
    public static string GetConfigName() {
        return "AppConfig";
    }
}