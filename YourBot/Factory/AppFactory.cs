using Lagrange.Core.Common;
using YourBot.Config.Implement;
using YourBot.Config.Implement.Level1;
using YourBot.Login;

namespace YourBot.Factory;

public partial class AppFactory {
    private readonly AppConfig _appConfig;
    private readonly BotAppInfoAgentConfig _botAppInfo;
    private readonly BotConfigAgentConfig _botConfig;

    private readonly BotDeviceInfoAgentConfig _botDeviceInfo;
    private readonly BotKeyStoreAgentConfig _botKeystore;

    public AppFactory(AppConfig appConfig, BotAppInfoAgentConfig appInfoAgentConfig,
        BotDeviceInfoAgentConfig botDeviceInfoAgentConfig, BotConfigAgentConfig botConfigAgentConfig,
        BotKeyStoreAgentConfig botKeystore) {
        _appConfig = appConfig;

        _botDeviceInfo = botDeviceInfoAgentConfig;
        _botKeystore = botKeystore;
        _botAppInfo = appInfoAgentConfig;
        _botConfig = botConfigAgentConfig;
        
        var loginSigner = new LoginSigner(_appConfig.SignServerUrl, _appConfig.SignProxyUrl, _botAppInfo.BotAppInfo!.Os switch {
            "Windows" => "Windows",
            "Mac" => "MacOs",
            "Linux" => "Linux",
            _ => "Unknown"
        }, _botAppInfo.BotAppInfo!.CurrentVersion);

        _botConfig.BotConfig.CustomSignProvider = loginSigner;
    }
}