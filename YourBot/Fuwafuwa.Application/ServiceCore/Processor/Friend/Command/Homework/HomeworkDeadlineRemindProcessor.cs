using System.Reflection.Emit;
using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Container.Level3;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.Log.LogEventArgs.Interface;
using Fuwafuwa.Core.ServiceCore.Level1;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core.Message;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cms;
using YourBot.Config.Implement.Level1;
using YourBot.Config.Implement.Level1.Service.Friend;
using YourBot.Fuwafuwa.Application.Attribute.Processor.Friend;
using YourBot.Fuwafuwa.Application.Data;
using YourBot.Fuwafuwa.Application.Data.ExecutorData.Friend;
using YourBot.Fuwafuwa.Application.ServiceCore.Input;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command.Homework;

public class HomeworkDeadlineRemindProcessor : IProcessorCore<HomeworkData,
    SimpleSharedDataWrapper<(DatabaseConfig config, HomeworkDeadlineRemindInputConfig remindConfig,
        AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>> inputHandler,
        List<(Task task, CancellationTokenSource cancellationTokenSource, uint id)> initTasks)>, (DatabaseConfig,HomeworkDeadlineRemindInputConfig remindConfig,
    AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>>)> {
    public static IServiceAttribute<HomeworkData> GetServiceAttribute() {
        return ReadHomeworkDataAttribute.GetInstance();
    }
    private const string SqlGetHomework = "SELECT * FROM homework WHERE deadline > CURRENT_TIMESTAMP";

    private const string SqlGetUnfinishedUser =
        "SELECT am.user_uin FROM actor_member am LEFT JOIN homework_member hm ON am.user_uin = hm.user_uin AND hm.homework_id = @homeworkId WHERE hm.id IS NULL; ";
    
    
    private static async Task WaitAndSend(DatabaseConfig config, uint homeworkId, uint actorId, string name,
        DateTime deadline, DateTime remindTime, string instruction,
        AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>> inputHandler,
        CancellationToken cancellationToken) {
        var interval = remindTime - DateTime.Now;
        var connectionStr = config.ConnectionString;
        if (interval.TotalMilliseconds < 0) {
            await using var connection = new MySqlConnection(connectionStr);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await using var cmd = new MySqlCommand(SqlGetUnfinishedUser, connection, transaction);
            cmd.Parameters.AddWithValue("@homeworkId", homeworkId);
            await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync(cancellationToken);
            var friends = new List<uint>();
            while (await reader.ReadAsync(cancellationToken)) {
                friends.Add(reader.GetUInt32("user_uin"));
            }
            await inputHandler.ExecuteAsync(reference =>
                Task.FromResult(reference.Value.Input(new HomeworkDeadlineRemindInputData(homeworkId, actorId, name,
                    deadline, remindTime, instruction, friends))));
        }
        await Task.Delay(interval, cancellationToken).ContinueWith(async task => {
            if (task.IsCanceled) {
                return;
            }
            await using var connection = new MySqlConnection(connectionStr);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await using var cmd = new MySqlCommand(SqlGetUnfinishedUser, connection, transaction);
            cmd.Parameters.AddWithValue("@homeworkId", homeworkId);
            await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync(cancellationToken);
            var friends = new List<uint>();
            while (await reader.ReadAsync(cancellationToken)) {
                friends.Add(reader.GetUInt32("user_uin"));
            }
            
            await inputHandler.ExecuteAsync(reference =>
                Task.FromResult(reference.Value.Input(new HomeworkDeadlineRemindInputData(homeworkId, actorId, name,
                    deadline, remindTime, instruction, friends))));
        }, cancellationToken);
    }
    
    public static
        SimpleSharedDataWrapper<(DatabaseConfig config, HomeworkDeadlineRemindInputConfig remindConfig,
            AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>> inputHandler,
            List<(Task task, CancellationTokenSource cancellationTokenSource, uint id)> initTasks)> Init(
            (DatabaseConfig,HomeworkDeadlineRemindInputConfig remindConfig, AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>>) initData) {
        var config = initData.Item1;
        var remindConfig = initData.Item2;
        
        var inputHandler = initData.Item3;
        
        var connectionStr = config.ConnectionString;
        using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        using var cmd = new MySqlCommand(SqlGetHomework, connection, transaction);
        using var reader = cmd.ExecuteReader();
        
        var initTasks = new List<(Task, CancellationTokenSource, uint)>();
        while ( reader.Read() ) {
            var homeworkId = reader.GetUInt32("id");
            var actorId = reader.GetUInt32("actor_id");
            var name = reader.GetString("name");
            var deadline = reader.GetDateTime("deadline");
            var remindTime = reader.GetDateTime("remind_time");
            var instruction = reader.GetString("introduction") ?? "";

            if (remindTime == deadline) {
                continue;
            }
            var cancellationTokenSource = new CancellationTokenSource();
            var task = WaitAndSend(config, homeworkId, actorId, name, deadline,
                remindTime, instruction, inputHandler,cancellationTokenSource.Token);
            initTasks.Add((task, cancellationTokenSource, homeworkId));
        }
        
        return new SimpleSharedDataWrapper<(DatabaseConfig config, HomeworkDeadlineRemindInputConfig remindConfig,
            AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>> inputHandler,
            List<(Task task, CancellationTokenSource cancellationTokenSource, uint id)>initTasks)>((config, remindConfig,
            inputHandler, initTasks));
    }
    public static void Final(
        SimpleSharedDataWrapper<(DatabaseConfig config, HomeworkDeadlineRemindInputConfig remindConfig,
            AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>> inputHandler,
            List<(Task task, CancellationTokenSource cancellationTokenSource, uint id)> initTasks)> sharedData,
        Logger2Event? logger) {
        var tuples = sharedData.Execute(reference => reference.Value.initTasks);
        
        foreach (var (task, cancellationTokenSource, _) in tuples) {
            try {
                cancellationTokenSource.Cancel();
                task.Wait();
                
            } catch (AggregateException e) {
                logger?.Error(new object(),"Error when cancel task from HomeworkDeadlineRemindProcessor:" + e.Message);
            }
        }
    }
    
    private const string SqlGetHomeworkById = "SELECT * FROM homework WHERE id = @id";

    public async Task<List<Certificate>> ProcessData(HomeworkData data,
        SimpleSharedDataWrapper<(DatabaseConfig config, HomeworkDeadlineRemindInputConfig remindConfig,
            AsyncSharedDataWrapper<InputHandler<HomeworkDeadlineRemindInputData>> inputHandler,
            List<(Task task, CancellationTokenSource cancellationTokenSource, uint id)> initTasks)> sharedData,
        Logger2Event? logger) {
        await Task.CompletedTask;
        var config = sharedData.Execute(reference => reference.Value.config);
        var remindConfig = sharedData.Execute(reference => reference.Value.remindConfig);
        var inputHandler = sharedData.Execute(reference => reference.Value.inputHandler);
        
        if (data.Type == HomeworkDeadlineRemindDataType.Add) {
            var connectionStr = config.ConnectionString;
            await using var connection = new MySqlConnection(connectionStr);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try {
                await using (var cmd = new MySqlCommand(SqlGetHomeworkById, connection, transaction)) {
                    cmd.Parameters.AddWithValue("@id", data.HomeworkId);
                    await using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                    if (reader.Read()) {
                        var homeworkId = reader.GetUInt32("id");
                        var actorId = reader.GetUInt32("actor_id");
                        var name = reader.GetString("name");
                        var deadline = reader.GetDateTime("deadline");
                        var remindTime = reader.GetDateTime("remind_time");
                        var instruction = reader.GetString("introduction") ?? "";

                        if (remindTime == deadline) {
                            return [];
                        }

                        var cancellationTokenSource = new CancellationTokenSource();
                        var task = WaitAndSend(config, homeworkId, actorId, name, deadline,
                            remindTime, instruction, inputHandler, cancellationTokenSource.Token);
                        sharedData.Execute(reference =>
                            reference.Value.initTasks.Add((task, cancellationTokenSource, homeworkId)));
                    }
                }
            } catch (Exception e) {
                return [YourBotUtil.SendToFriendMessage(data.FriendUin, remindConfig.Priority, $"Error when add remind task: {e.Message}")];
            }

            await transaction.CommitAsync();
            return [YourBotUtil.SendToFriendMessage(data.FriendUin, remindConfig.Priority, "Add remind task successfully")];
        }

        if (data.Type == HomeworkDeadlineRemindDataType.Delete) {
            sharedData.Execute(reference => {
                
                reference.Value.initTasks.ForEach(tuple => {
                    if (tuple.task.IsCompleted) {
                        return;
                    }
                    if (tuple.task.IsCanceled) {
                        return;
                    }
                    if (tuple.id != data.HomeworkId) {
                        return;
                    }
                    try {
                        tuple.cancellationTokenSource.Cancel();
                        tuple.task.Wait();
                    } catch (AggregateException e) { }
                });
                
                reference.Value.initTasks.RemoveAll(tuple => tuple.id == data.HomeworkId);
            });

            return [YourBotUtil.SendToFriendMessage(data.FriendUin, remindConfig.Priority, "Delete remind task successfully")];
        }
        throw new Exception("Unknown type of HomeworkDeadlineRemindData");
    }
}