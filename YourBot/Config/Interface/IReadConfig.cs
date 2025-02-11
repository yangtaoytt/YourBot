using Microsoft.Extensions.Configuration;

namespace YourBot.Config.Interface;

public interface IReadConfig<out TConfig> where TConfig : IReadConfig<TConfig> {
    public static abstract List<string> GetConfigName();

    public static abstract TConfig CreateConfig(Dictionary<string, IConfigurationRoot> configurations);
}