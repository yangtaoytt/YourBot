using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using MySql.Data.MySqlClient;
using YourBot.Config.Implement.Level1;
using YourBot.Config.Implement.Level1.Service.Group.Command.Homework;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Attribute.Processor.Friend;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command.Homework;

public class ActorProcessor : IProcessorCore<FriendCommandData, NullSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig)>, (ActorConfig actorConfig, DatabaseConfig databaseConfig)> {
    public static IServiceAttribute<FriendCommandData> GetServiceAttribute() {
        return ReadFriendCommandAttribute.GetInstance();
    }
    public static NullSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig)> Init((ActorConfig actorConfig, DatabaseConfig databaseConfig) initData) {
        return new NullSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig)>(initData);
    }
    public static void Final(NullSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig)> sharedData, Logger2Event? logger) { }
    public async Task<List<Certificate>> ProcessData(FriendCommandData data, NullSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig)> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;
        var friendUin = data.FriendUin;
        var configs = sharedData.Execute(reference => reference.Value);
        if (!Utils.YourBotUtil.CheckFriendPermission(configs.actorConfig, friendUin)) {
            return [];
        }

        var command = data.Command;
        if (command != "actor") {
            return [];
        }
        var parameters = data.Parameters;
        if (parameters.Count == 0) {
            return [Utils.YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, "wrong parameters")];
        }
        var subCommand = parameters[0];
        if (subCommand == "manage") {
            if (parameters.Count < 2) {
                return [Utils.YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, "wrong parameters")];
            }
            var subSubCommand = parameters[1];
            return await ActorManage(parameters ,subSubCommand, friendUin, configs);
        }
        
        return [Utils.YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, "wrong parameters")];
    }

    private const string AddManageActorSql =
        "INSERT INTO actor (creator_uin, name) VALUES (@creator_uin, @name);SELECT LAST_INSERT_ID();";
    private const string RmManageActorSql = "DELETE FROM actor WHERE creator_uin = @creator_uin AND name = @name;";
    private const string LsManageActorSql = "SELECT * FROM actor WHERE creator_uin = @creator_uin;";
    
    private static async Task<List<Certificate>> ActorManage(List<string> parameters,string command, uint friendUin, (ActorConfig actorConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        var transaction = await connection.BeginTransactionAsync();
        
        switch (command) {
            case "add" when parameters.Count < 3:
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                        "wrong parameters")
                ];
            case "add": {
                var actorName = parameters[2];
                try {
                    await using var cmd = new MySqlCommand(AddManageActorSql, connection, transaction);
                    cmd.Parameters.AddWithValue("@creator_uin", friendUin);
                    cmd.Parameters.AddWithValue("@name", actorName);

                    await cmd.ExecuteScalarAsync();
                } catch (Exception) {
                    await transaction.RollbackAsync();
                    return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                        "add actor failed.Please ensure the actor does not exist")];
                }
                await transaction.CommitAsync();
                return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                    "add actor success")];
            }
            case "rm" when parameters.Count < 3:
                return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                    "wrong parameters")];
            case "rm": {
                var actorName = parameters[2];
                try {
                    await using var cmd = new MySqlCommand(RmManageActorSql, connection, transaction);
                    cmd.Parameters.AddWithValue("@creator_uin", friendUin);
                    cmd.Parameters.AddWithValue("@name", actorName);

                    await cmd.ExecuteNonQueryAsync();
                } catch (Exception) {
                    await transaction.RollbackAsync();
                    return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                        "remove actor failed.Please ensure the actor exists")];
                }
                await transaction.CommitAsync();
                return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, 
                    "remove actor success")];
            }
            case "ls":
                try {
                    var actorList = new List<string>();
                    await using (var cmd = new MySqlCommand(LsManageActorSql, connection, transaction)) {
                        cmd.Parameters.AddWithValue("@creator_uin", friendUin);
                        await using var reader = cmd.ExecuteReader();

                        while (await reader.ReadAsync()) {
                            actorList.Add(reader.GetString("name"));
                        }
                    }
                    await transaction.CommitAsync();
                    return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, 
                        "actor list: \n" + string.Join("\n ", actorList))];
                } catch (Exception) {
                    await transaction.RollbackAsync();
                    return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                        "list actor failed")];
                }

            default:
                return [Utils.YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, "wrong parameters")];
        }
    }
}