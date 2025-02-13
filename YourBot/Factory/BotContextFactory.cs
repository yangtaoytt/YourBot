using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using YourBot.Login;

namespace YourBot.Factory;

public partial class AppFactory {
    public BotContext CreateBotContext() {
        BotContext? res;
        switch (_appConfig.LoginType) {
            case LoginType.QrCode:
                if (_appConfig.Uin == null) {
                    throw new ArgumentNullException(nameof(_appConfig.Uin), "the Uin is required for QrCode login");
                }

                res = BotFactory.Create(_botConfig.BotConfig, _appConfig.Uin.Value, "password",
                    _botAppInfo.BotAppInfo!, out var deviceInfo);
                _botDeviceInfo.BotDeviceInfo = deviceInfo;
                _botConfig.BotConfig.CustomSignProvider = null;
                break;
            case LoginType.Keystore:
                if (_botDeviceInfo.BotDeviceInfo == null) {
                    throw new ArgumentNullException(nameof(_botDeviceInfo.BotDeviceInfo),
                        "the  DeviceInfo  is required for KeyStore login");
                }

                if (_botKeystore.BotKeystore == null) {
                    throw new ArgumentNullException(nameof(_botKeystore.BotKeystore),
                        "the Keystore is required for KeyStore login");
                }

                res = BotFactory.Create(_botConfig.BotConfig, _botDeviceInfo.BotDeviceInfo, _botKeystore.BotKeystore,
                    _botAppInfo.BotAppInfo!);
               break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _botConfig.BotConfig.CustomSignProvider = null;
        return res;
    }
}