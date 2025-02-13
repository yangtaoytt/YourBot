using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace YourBot.Config.Interface;

public interface IConfig {
    public static abstract string GetConfigName();
}