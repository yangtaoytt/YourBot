using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Microsoft.Extensions.Logging;
using QRCoder;
using YourBot.Config.Implement;
using YourBot.Logger;

namespace YourBot.Login;

public enum LoginType {
    QrCode,
    KeyStore
}

public interface ILoginStrategy {
    Task<bool> Login(BotDeviceInfoAgentConfig deviceInfo, BotKeystoreAgentConfig keystore, MainConfig mainConfig,
        BotContext botContext, AppLogger appLogger);
}

public class QrCodeLogin : ILoginStrategy {
    public async Task<bool> Login(BotDeviceInfoAgentConfig deviceInfo, BotKeystoreAgentConfig keystore,
        MainConfig mainConfig,
        BotContext botContext, AppLogger appLogger) {
        appLogger.FromModule(LogSource.YourBot).LogInformation("Begin to login with QRCode...");
        var qrCodeInfo = await botContext.FetchQrCode();
        if (qrCodeInfo == null) {
            appLogger.FromModule(LogSource.YourBot).LogCritical("Failed to fetch QR code.");
            return false;
        }

        using (QRCodeGenerator qrGenerator = new())
        using (var qrCodeData = qrGenerator.CreateQrCode(qrCodeInfo.Value.Url, QRCodeGenerator.ECCLevel.Q))
        using (AsciiQRCode asciiQrCode = new(qrCodeData)) {
            appLogger.FromModule(LogSource.YourBot)
                .LogInformation("Please scan the QR code to login\n{}\n",
                    asciiQrCode.GetGraphicSmall());
        }

        await botContext!.LoginByQrCode();
        appLogger.FromModule(LogSource.YourBot).LogInformation("Login success!");

        keystore.BotKeystore = botContext.UpdateKeystore();
        mainConfig.Configurations["KeystoreConfig"] = "keystore.json";
        return true;
    }
}

public class KeyStoreLogin : ILoginStrategy {
    public async Task<bool> Login(BotDeviceInfoAgentConfig deviceInfo, BotKeystoreAgentConfig keystore,
        MainConfig mainConfig,
        BotContext botContext, AppLogger appLogger) {
        appLogger.FromModule(LogSource.YourBot).LogInformation("Begin to login with KeyStore...");

        if (deviceInfo.DeviceInfo == null || keystore.BotKeystore == null) {
            throw new ArgumentNullException(nameof(deviceInfo), "DeviceInfo or Keystore is null");
        }

        if (await botContext.LoginByPassword()) {
            appLogger.FromModule(LogSource.YourBot).LogInformation("Login success!");
            return true;
        }

        keystore.BotKeystore = null;
        deviceInfo.DeviceInfo = null;
        try {
            File.Delete(mainConfig.Configurations["KeystoreConfig"]);
            File.Delete(mainConfig.Configurations["DeviceInfoConfig"]);
            mainConfig.Configurations["KeystoreConfig"] = "";
            mainConfig.Configurations["DeviceInfoConfig"] = "";
        } catch {
            appLogger.FromModule(LogSource.YourBot)
                .LogCritical("Failed to delete keystore or deviceInfo, Please delete it manually.");
        }

        return false;
    }
}