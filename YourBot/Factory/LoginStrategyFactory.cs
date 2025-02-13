using YourBot.Login;

namespace YourBot.Factory;

public partial class AppFactory {
    public ILoginStrategy CreateLoginStrategy() {
        return _appConfig.LoginType switch {
            LoginType.QrCode => new QrCodeLogin(),
            LoginType.Keystore => new KeyStoreLogin(),
            _ => throw new ArgumentOutOfRangeException(nameof(_appConfig.LoginType), _appConfig.LoginType,
                "Invalid login type")
        };
    }
}