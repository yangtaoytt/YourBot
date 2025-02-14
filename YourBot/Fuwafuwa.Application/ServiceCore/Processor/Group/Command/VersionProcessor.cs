using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using MySql.Data.MySqlClient;
using YourBot.Config.Implement.Level1;
using YourBot.Config.Implement.Level1.Service.Group.Command;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command;

public class VersionProcessor : IProcessorCore<CommandData, NullSharedDataWrapper<(VersionConfig versionConfig, DatabaseConfig databaseConfig)>, (VersionConfig versionConfig, DatabaseConfig databaseConfig)> {
    public static IServiceAttribute<CommandData> GetServiceAttribute() {
        return ReadGroupCommandAttribute.GetInstance();
    }

    public static NullSharedDataWrapper<(VersionConfig versionConfig, DatabaseConfig databaseConfig)> Init((VersionConfig versionConfig, DatabaseConfig databaseConfig) initData) {
        return new NullSharedDataWrapper<(VersionConfig versionConfig, DatabaseConfig databaseConfig)>(initData);
    }

    public static void Final(NullSharedDataWrapper<(VersionConfig versionConfig, DatabaseConfig databaseConfig)> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(CommandData data,
        NullSharedDataWrapper<(VersionConfig versionConfig, DatabaseConfig databaseConfig)> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;

        var configs = sharedData.Execute(initData => initData.Value);

        var groupUin = data.GroupUin;
        if (!YourBotUtil.CheckSimpleGroupPermission(configs.versionConfig, groupUin)) {
            return [];
        }

        var command = data.Command;
        if (command != "version") {
            return [];
        }

        string reply;
        try {
            var queryResult = QueryLastVersion(configs.databaseConfig.ConnectionString);
            reply = "Version: " + queryResult.versionNumber + "\n" +
                        "Update Time: " + queryResult.updateTime + "\n" +
                        "Description: " + queryResult.versionDescrption;
        } catch (Exception) {
            reply = "No version data found";
        }
        

        var groupMessageChain = MessageBuilder.Group(data.MessageChain.GroupUin!.Value).Text(reply).Build();
        var sendGroupMessageData =
            new SendToGroupMessageData(new Priority(configs.versionConfig.Priority, PriorityStrategy.Share), groupMessageChain);

        return [
            CanSendGroupMessageAttribute.GetInstance().GetCertificate(sendGroupMessageData)
        ];
    }

    private static (string versionNumber, DateTime updateTime, string versionDescrption) QueryLastVersion(
        string connectionString) {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();
        const string query = "SELECT * FROM version ORDER BY update_time DESC LIMIT 1";
        using var cmd = new MySqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();
        if (reader.Read()) {
            return (reader.GetString("version_number"), reader.GetDateTime("update_time"),
                reader.GetString("version_description"));
        }

        throw new Exception("No version data found");
    }
}