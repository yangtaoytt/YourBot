using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using MySql.Data.MySqlClient;
using YourBot.Config.Implement.Level1;
using YourBot.Config.Implement.Level1.Service.Friend.Command;
using YourBot.Fuwafuwa.Application.Attribute.Processor.Friend;
using YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Friend;

public class SubmitCheckAndSaveProcessor : IProcessorCore<SubmitData, NullSharedDataWrapper<(SubmitCheckAndSaveConfig submitConfig, DatabaseConfig databaseConfig)>, (SubmitCheckAndSaveConfig submitConfig, DatabaseConfig databaseConfig)> {
    public static IServiceAttribute<SubmitData> GetServiceAttribute() {
        return ReadSubmitDataAttribute.GetInstance();
    }
    public static NullSharedDataWrapper<(SubmitCheckAndSaveConfig submitConfig, DatabaseConfig databaseConfig)> Init((SubmitCheckAndSaveConfig submitConfig, DatabaseConfig databaseConfig) initData) {
        return new NullSharedDataWrapper<(SubmitCheckAndSaveConfig submitConfig, DatabaseConfig databaseConfig)>(
            initData);
    }
    public static void Final(NullSharedDataWrapper<(SubmitCheckAndSaveConfig submitConfig, DatabaseConfig databaseConfig)> sharedData, Logger2Event? logger) { }
    private const string SqlSelectDeadline = "SELECT deadline FROM homework WHERE id = @homeworkId;";

    
    public async Task<List<Certificate>> ProcessData(SubmitData data, NullSharedDataWrapper<(SubmitCheckAndSaveConfig submitConfig, DatabaseConfig databaseConfig)> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;
        var (config, databaseConfig) = sharedData.Execute(reference => reference.Value);
        var connectionStr = databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        try {
            await using var cmd = new MySqlCommand(SqlSelectDeadline, connection, transaction);
            cmd.Parameters.AddWithValue("@homeworkId", data.HomeworkId);
            var deadline = await cmd.ExecuteScalarAsync();
            if (deadline == null) {
                return [YourBotUtil.SendToFriendMessage(data.FriendUin, config.Priority, "homework not found")];
            }
            if (DateTime.Now > (DateTime)deadline) {
                return [YourBotUtil.SendToFriendMessage(data.FriendUin, config.Priority, "deadline passed")];
            }
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(data.FriendUin, config.Priority, "check deadline failed: " + e.Message)];
        }
        
        var homeworkId = data.HomeworkId;
        var friendUin = data.FriendUin;
        var fileEntities = data.FileEntities;
        var regexes = data.Regexes.Select(regex => (regex, false)).ToList();
        
        foreach (var fileEntity in fileEntities) {
            var fileName = fileEntity.FileName;
            var index = regexes.FindIndex((item) => item.regex.IsMatch(fileName) && !item.Item2);
            if (index == -1) {
                continue;
            }
            regexes[index] = (regexes[index].regex, true);
        }
        var uncompleted = regexes.Where(item => !item.Item2).Select(item => item.regex.ToString()).ToList();
        if (uncompleted.Count > 0) {
            return [YourBotUtil.SendToFriendMessage(friendUin, config.Priority, "the following files is not found:\n" + string.Join("\n", uncompleted))];
        }
        
        try {
            foreach (var file in fileEntities) {
                await YourBotUtil.SaveFileWithStream(file.FileUrl!,
                    Path.Combine(config.FilePath, homeworkId.ToString(), friendUin.ToString(), file.FileName));
            }
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, config.Priority, "submit failed: " + e.Message)];
        }
        

        try {         
            await using var command = new MySqlCommand(SqlInsertSubmit, connection, transaction);
            command.Parameters.AddWithValue("@homeworkId", homeworkId);
            command.Parameters.AddWithValue("@friendUin", friendUin);
            await command.ExecuteNonQueryAsync();
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, config.Priority, "submit status update failed: " + e.Message)];
        }
        await transaction.CommitAsync();
        
        
        return [YourBotUtil.SendToFriendMessage(friendUin, config.Priority, "submit success")];
    }
    
    private const string SqlInsertSubmit = "INSERT INTO homework_member (homework_id, user_uin) VALUES (@homeworkId, @friendUin)";
}