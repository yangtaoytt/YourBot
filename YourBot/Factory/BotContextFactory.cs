using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using YourBot.Login;

namespace YourBot.Factory;

public partial class AppFactory {
    public BotContext CreateBotContext() {
        switch (_appConfig.LoginType) {
            case LoginType.QrCode:
                if (_appConfig.Uin == null) {
                    throw new ArgumentNullException(nameof(_appConfig.Uin), "the Uin is required for QrCode login");
                }

                var res = BotFactory.Create(_botConfig, _appConfig.Uin.Value, "password",
                    _botAppInfo, out var deviceInfo);
                _botDeviceInfo.DeviceInfo = deviceInfo;
                _mainConfig.Configurations["DeviceInfoConfig"] = "deviceinfo.json";
                return res;
            case LoginType.KeyStore:
                if (_botDeviceInfo.DeviceInfo == null) {
                    throw new ArgumentNullException(nameof(_botDeviceInfo.DeviceInfo),
                        "the  DeviceInfo  is required for KeyStore login");
                }

                if (_botKeystore.BotKeystore == null) {
                    throw new ArgumentNullException(nameof(_botKeystore.BotKeystore),
                        "the Keystore is required for KeyStore login");
                }

                return BotFactory.Create(_botConfig, _botDeviceInfo.DeviceInfo, _botKeystore.BotKeystore,
                    _botAppInfo);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}