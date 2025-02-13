using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Microsoft.Extensions.Logging;
using QRCoder;
using YourBot.Config.Implement;
using YourBot.Config.Implement.Level1;
using YourBot.Logger;

namespace YourBot.Login;

public enum LoginType {
    QrCode,
    Keystore
}

public interface ILoginStrategy {
    Task<bool> Login(BotDeviceInfoAgentConfig deviceInfo, BotKeyStoreAgentConfig keystore,
        BotContext botContext, AppLogger appLogger);
}

public class QrCodeLogin : ILoginStrategy {
    public async Task<bool> Login(BotDeviceInfoAgentConfig deviceInfo, BotKeyStoreAgentConfig keystore,
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
        return true;
    }
}

public class KeyStoreLogin : ILoginStrategy {
    public async Task<bool> Login(BotDeviceInfoAgentConfig deviceInfo, BotKeyStoreAgentConfig keystore,
        BotContext botContext, AppLogger appLogger) {
        appLogger.FromModule(LogSource.YourBot).LogInformation("Begin to login with KeyStore...");

        if (deviceInfo.BotDeviceInfo == null || keystore.BotKeystore == null) {
            throw new ArgumentNullException(nameof(deviceInfo), "DeviceInfo or Keystore is null");
        }

        if (await botContext.LoginByPassword()) {
            appLogger.FromModule(LogSource.YourBot).LogInformation("Login success!");
            return true;
        }

        deviceInfo.BotDeviceInfo = null;
        keystore.BotKeystore = null;

        appLogger.FromModule(LogSource.YourBot)
            .LogCritical("Failed to login by current keystore and deviceInfo.");

        return false;
    }
}