using Lagrange.Core.Common;
using YourBot.Config.Implement;
using YourBot.Login;

namespace YourBot.Factory;

public partial class AppFactory {
    private readonly AppConfig _appConfig;
    private readonly BotAppInfo _botAppInfo;
    private readonly BotConfig _botConfig;

    private readonly BotDeviceInfoAgentConfig _botDeviceInfo;
    private readonly BotKeystoreAgentConfig _botKeystore;

    private readonly MainConfig _mainConfig;

    public AppFactory(AppConfig appConfig, AppInfoAgentConfig appInfoAgentConfig,
        BotDeviceInfoAgentConfig botDeviceInfoAgentConfig, BotConfigAgentConfig botConfigAgentConfig,
        BotKeystoreAgentConfig botKeystore, MainConfig mainConfig) {
        _appConfig = appConfig;

        _botDeviceInfo = botDeviceInfoAgentConfig;
        _botKeystore = botKeystore;
        _mainConfig = mainConfig;
        _botAppInfo = appInfoAgentConfig.BotAppInfo;
        var loginSigner = new LoginSigner(_appConfig.SignServerUrl, _appConfig.SignProxyUrl, _botAppInfo.Os switch {
            "Windows" => "Windows",
            "Mac" => "MacOs",
            "Linux" => "Linux",
            _ => "Unknown"
        }, _botAppInfo.CurrentVersion);

        _botConfig = botConfigAgentConfig.GetBotConfig(loginSigner);
    }
}