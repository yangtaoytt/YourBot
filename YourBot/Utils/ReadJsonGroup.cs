using Microsoft.Extensions.Configuration;

namespace YourBot.Utils;

public static partial class Util {
    public static Dictionary<uint, List<uint>> ReadJsonGroupDic(IConfigurationRoot configuration, string key) {
        return configuration
            .GetSection(key)
            .Get<Dictionary<string, List<uint>>>()
            ?
            .ToDictionary(k => uint.Parse(k.Key), v => v.Value)!;
    }

    public static List<uint> ReadJsonGroupList(IConfigurationRoot configuration, string key) {
        return configuration
            .GetSection(key)
            .Get<List<uint>>()!;
    }

    public static List<T> ReadJsonList<T>(IConfigurationRoot configuration, string key) {
        return configuration
            .GetSection(key)
            .Get<List<T>>()!;
    }
}