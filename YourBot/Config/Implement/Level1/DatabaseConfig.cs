using YourBot.Config.Interface;

namespace YourBot.Config.Implement.Level1;

public class DatabaseConfig: IConfig {
    public string ConnectionString { get; init; } = "";
    
    public static string GetConfigName() {
        return "DatabaseConfig";
    }
}