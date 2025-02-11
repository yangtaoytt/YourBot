namespace YourBot.Config.Interface;

public interface IWriteConfig {
    public void WriteToFile(string path);

    public static abstract string GetMainWriteConfigName();
}