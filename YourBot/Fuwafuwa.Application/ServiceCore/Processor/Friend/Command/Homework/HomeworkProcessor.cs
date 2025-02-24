using System.Globalization;
using System.Text.RegularExpressions;
using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Lagrange.Core;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using MySql.Data.MySqlClient;
using YourBot.Config.Implement.Level1;
using YourBot.Config.Implement.Level1.Service.Group.Command.Homework;
using YourBot.Fuwafuwa.Application.Attribute.Processor.Friend;
using YourBot.Fuwafuwa.Application.Data;
using YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;
using YourBot.Utils;
using YourBot.Utils.Command;
using static System.Int32;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command.Homework;

public class HomeworkProcessor : IProcessorCore<FriendCommandData,AsyncSharedDataWrapper<(HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig, BotContext botContext)>,(HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig, BotContext botContext)> {
    public static IServiceAttribute<FriendCommandData> GetServiceAttribute() {
        return ReadFriendCommandAttribute.GetInstance();
    }

    public static AsyncSharedDataWrapper<(HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig, BotContext botContext)> Init(
        (HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig, BotContext botContext) initData) {
        return new AsyncSharedDataWrapper<(HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig, BotContext
            botContext)>(initData);
    }

    public static void Final(AsyncSharedDataWrapper<(HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig, BotContext botContext)> sharedData, Logger2Event? logger) { }
    public async Task<List<Certificate>> ProcessData(FriendCommandData data, AsyncSharedDataWrapper<(HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig, BotContext botContext)> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;
        var friendUin = data.FriendUin;
        var configs = await sharedData.ExecuteAsync(reference => Task.FromResult((reference.Value.homeworkConfig, reference.Value.databaseConfig)));
        if (!YourBotUtil.CheckFriendPermission(configs.homeworkConfig, friendUin)) {
            return [];
        }

        var commandHandler = data.CommandHandler;
        if (commandHandler.Command != "homework") {
            return [];
        }
        
        try {
            switch (commandHandler.Next().Command) {
                case "add":
                    return await AddHomework(commandHandler, friendUin, configs);
                case "release":
                    return await ReleaseHomework(commandHandler, friendUin, configs);
                case "rm":
                    return await RmHomework(commandHandler, friendUin, configs);
                case "add-submit-regex":
                    return await AddSubmitRegex(commandHandler, friendUin, configs, data.MessageChain);
                case "ls-finished-user":
                    return await LsHomeworkMember(commandHandler, friendUin, configs, true);
                case "ls-unfinished-user":
                    return await LsHomeworkMember(commandHandler, friendUin, configs, false);
                case "ls-detail":
                    return await LsHomeworkDetail(commandHandler, friendUin, configs);
                case "submit":
                    return await Submit(commandHandler, friendUin, configs);
                case "ls-mine":
                    return await LsMineHomework(commandHandler, friendUin, configs);
                default:
                    return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "wrong parameters")];
            }
        } catch (InvalidCommandException) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "wrong parameters")];
        }
    }
    
    private const string SqlSelectActorId = "SELECT id FROM actor WHERE creator_uin = @creatorUin AND name = @name;";
    private const string SqlInsertHomework = "INSERT INTO homework (actor_id, name, deadline, remind_time,is_release, introduction, create_time) VALUES (@actorId, @name, @deadline, @remindTime,@isRelease, @introduction, NOW());SELECT LAST_INSERT_ID();";
    
    private const string SqlSelectHomeworkIdByActorNameHomeworkNameCreator = "SELECT homework.id FROM homework JOIN actor ON homework.actor_id = actor.id WHERE actor.name = @actorName AND homework.name = @homeworkName;";
    private const string SqlReleaseHomework = "UPDATE homework SET is_release = True WHERE id = @id;";
    private const string SqlSelectHomeworkById = "SELECT * FROM homework WHERE id = @id;";
    private const string SqlLsHomeActorMember =
        "SELECT am.user_uin  FROM actor_member am JOIN homework on am.actor_id = homework.actor_id WHERE homework.id = @homeworkId;";
    
    
    private const string SqlRemoveHomework = "DELETE FROM homework WHERE id = @id;";
    
    private static async Task<List<Certificate>> AddHomework(CommandHandler commandHandler, uint friendUin,
        (HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig) configs) {
        
        var actorName = commandHandler.Next().Command;
        var homeworkName = commandHandler.Next().Command;
        DateTime deadline;
        try {
            deadline = DateTime.Parse(commandHandler.Next().Command +" "+ commandHandler.Next().Command);
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "wrong deadline format")];
        }

        if (deadline < DateTime.Now) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "deadline should be later than now")];
        }
        
        int remindTimeSpan;
        try {
            remindTimeSpan = Parse(commandHandler.Next().Command);
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "wrong remind time format")];
        }
        var remindTime = remindTimeSpan>0?deadline.Subtract(new TimeSpan(0, 0, remindTimeSpan, 0)): deadline;
        if (remindTime < DateTime.Now) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "remind time should be later than now")];
        }
        
        var introduction = commandHandler.TryNext()?.Command;
        
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        
        uint actorId;
        try {
            await using var cmd = new MySqlCommand(SqlSelectActorId, connection, transaction);
            cmd.Parameters.AddWithValue("@creatorUin", friendUin);
            cmd.Parameters.AddWithValue("@name", actorName);
            actorId = Convert.ToUInt32(await cmd.ExecuteScalarAsync());
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select actor failed:{e.Message}")];
        }
        
        
        try {
            await using (var cmd = new MySqlCommand(SqlInsertHomework, connection, transaction)) {
                cmd.Parameters.AddWithValue("@creatorUin", friendUin);
                cmd.Parameters.AddWithValue("@name", homeworkName);
                cmd.Parameters.AddWithValue("@deadline", deadline);
                cmd.Parameters.AddWithValue("@remindTime", remindTime);
                cmd.Parameters.AddWithValue("@isRelease", false);
                cmd.Parameters.AddWithValue("@introduction", introduction);
                cmd.Parameters.AddWithValue("@actorId", actorId);
                
                
                actorId = Convert.ToUInt32(await cmd.ExecuteScalarAsync());
            }
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"add homework failed:{e.Message}")];
        }

        await transaction.CommitAsync();
        
        return [
            YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "add homework success")
        ];
    }
    
    private const string SqlSelectRegexCount = "SELECT COUNT(*) FROM homework_regex WHERE homework_id = @homeworkId;";
    
    
    
    private static async Task<List<Certificate>> ReleaseHomework(CommandHandler commandHandler, uint friendUin,
        (HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        
        
        var actorName = commandHandler.Next().Command;
        var homeworkName = commandHandler.Next().Command;
        
        
        uint homeworkId;
        try {
            var homeworkIdResult = await GetHomeworkId(homeworkName, actorName, connection, transaction);
            if (homeworkIdResult == null) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "homework not found")
                ];
            }

            homeworkId = homeworkIdResult.Value;
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select homework failed:{e.Message}")];
        }

        try {
            await using var cmd = new MySqlCommand(SqlSelectRegexCount, connection, transaction);
            cmd.Parameters.AddWithValue("@homeworkId", homeworkId);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            if (count == 0) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "no submit regex found")
                ];
            }
        }
        catch (Exception e)
        {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select submit regex failed:{e.Message}")];
        }
        
        try {
            await using var cmd = new MySqlCommand(SqlSelectHomeworkById, connection, transaction);
            cmd.Parameters.AddWithValue("@id", homeworkId);

            await using var reader = cmd.ExecuteReader();
            if (!await reader.ReadAsync()) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "homework not found")
                ];
            }
            var isRelease = reader.GetBoolean("is_release");
            if (isRelease) {
                return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "homework already is released")];
            }
        }
        catch (Exception e)
        {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select status failed:{e.Message}")];
        }
        

        
        try {
            await using var cmd = new MySqlCommand(SqlReleaseHomework, connection, transaction);
            cmd.Parameters.AddWithValue("@id", homeworkId);
            await cmd.ExecuteNonQueryAsync();
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"release homework failed:{e.Message}")];
        }
        
        string? introduction;
        DateTime deadline;
        try {
            await using var cmd = new MySqlCommand(SqlSelectHomeworkById, connection, transaction);
            cmd.Parameters.AddWithValue("@id", homeworkId);

            await using var reader = cmd.ExecuteReader();
            if (!await reader.ReadAsync()) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "homework not found")
                ];
            }
            
            introduction = reader.IsDBNull(reader.GetOrdinal("introduction"))
                ? null
                : reader.GetString("introduction");
            deadline = reader.GetDateTime("deadline");
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select homework failed:{e.Message}")];
        }
        
        var memberList = new List<uint>();
        try {
            await using var cmd = new MySqlCommand(SqlLsHomeActorMember, connection, transaction);
            cmd.Parameters.AddWithValue("@homeworkId", homeworkId);

            await using var reader = cmd.ExecuteReader();
            while (await reader.ReadAsync()) {
                memberList.Add(reader.GetUInt32("user_uin"));
            }
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select homework actor member failed:{e.Message}")];
        }

        await transaction.CommitAsync();
        
        List<Certificate> result = [
            YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "release homework success"),
            ReadHomeworkDataAttribute.GetInstance()
                .GetCertificate(new HomeworkData(HomeworkDeadlineRemindDataType.Add, homeworkId,
                    friendUin))
        ];
        result.AddRange(memberList.Select(member => YourBotUtil.SendToFriendMessage(member, configs.homeworkConfig.Priority, $"Homework [{homeworkName}] released, deadline:{deadline} {introduction}")));
        
        return result;
    }

    private static async Task<List<Certificate>> RmHomework(CommandHandler commandHandler, uint friendUin,
        (HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        
        
        var actorName = commandHandler.Next().Command;
        var homeworkName = commandHandler.Next().Command;
        
        uint homeworkId;
        try {
            var result = await GetHomeworkId(homeworkName, actorName, connection, transaction);
            if (result == null) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "homework not found")
                ];
            }

            homeworkId = result.Value;
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select homework failed:{e.Message}")];
        }
        
        
        try {
            await using var cmd = new MySqlCommand(SqlRemoveHomework, connection, transaction);
            cmd.Parameters.AddWithValue("@id", homeworkId);
            await cmd.ExecuteNonQueryAsync();
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"remove homework failed:{e.Message}")];
        }
        
        await transaction.CommitAsync();
        
        return [
            YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "remove homework success"),
            ReadHomeworkDataAttribute.GetInstance()
                .GetCertificate(new HomeworkData(HomeworkDeadlineRemindDataType.Delete, homeworkId,
                    friendUin))
        ];
    }

    private static async Task<uint?> GetHomeworkId(string homeworkName, string actorName,
        MySqlConnection connection, MySqlTransaction transaction) {
        await using var cmd = new MySqlCommand(SqlSelectHomeworkIdByActorNameHomeworkNameCreator, connection,
            transaction);
        cmd.Parameters.AddWithValue("@actorName", actorName);
        cmd.Parameters.AddWithValue("@homeworkName", homeworkName);
        await using var reader = cmd.ExecuteReader();
        if (!await reader.ReadAsync()) {
            return null;
        }

        return reader.GetUInt32("id");
    }
    
    private const string SqlAddSubmitRegex = "INSERT INTO homework_regex (homework_id, regex) VALUES (@homeworkId, @regex);";

    private static async Task<List<Certificate>> AddSubmitRegex(CommandHandler commandHandler, uint friendUin,
        (HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig) configs, MessageChain messageChain) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        
        
        var actorName = commandHandler.Next().Command;
        var homeworkName = commandHandler.Next().Command;
        
        uint homeworkId;
        try {
            var result = await GetHomeworkId(homeworkName, actorName, connection, transaction);
            if (result == null) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "homework not found")
                ];
            }

            homeworkId = result.Value;
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select homework failed:{e.Message}")];
        }

        try {
            await using var cmd = new MySqlCommand(SqlSelectHomeworkById, connection, transaction);
            cmd.Parameters.AddWithValue("@id", homeworkId);
            
            await using var reader = cmd.ExecuteReader();
            if (!await reader.ReadAsync()) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "homework not found")
                ];
            }
            
            if (reader.GetBoolean("is_release")) {
                return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "homework already is released")];
            }
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"check homework status failed:{e.Message}")];
        }
        

        string? regexString = null;
        foreach (var message in messageChain) {
            if (message is TextEntity text) {
                var index = text.Text.IndexOf(homeworkName, StringComparison.Ordinal);
                if (index != -1 && index + homeworkName.Length +1 < text.Text.Length) {
                    regexString = text.Text[(index + homeworkName.Length + 1)..];
                }
            }
        }

        if (regexString == null) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "regex not found")];
        }

        try {
            new Regex(regexString);
        } catch (ArgumentException) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "wrong regex format")];
        }

        try {
            await using var cmd = new MySqlCommand(SqlAddSubmitRegex, connection, transaction);
            cmd.Parameters.AddWithValue("@homeworkId", homeworkId);
            cmd.Parameters.AddWithValue("@regex", regexString);

            await cmd.ExecuteNonQueryAsync();
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"add submit regex failed:{e.Message}")];
        }
        
        await transaction.CommitAsync();
        
        return [
            YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, "add submit regex success")
        ];
    }
    
    private const string SqlSelectUnfinishedMember = "SELECT user_uin FROM actor_member WHERE actor_id = (select  actor_id from homework where homework.id = @homeworkId) AND user_uin NOT IN (SELECT user_uin FROM homework_member WHERE homework_id = @homeworkId);";
    private const string SqlSelectFinishedMember = "SELECT user_uin FROM homework_member WHERE homework_id = @homeworkId;";

    private static async Task<List<Certificate>> LsHomeworkMember(CommandHandler commandHandler, uint friendUin,
        (HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig) configs,bool isFinished) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        
        
        var actorName = commandHandler.Next().Command;
        var homeworkName = commandHandler.Next().Command;
        
        uint homeworkId;
        try {
            var result = await GetHomeworkId(homeworkName, actorName, connection, transaction);
            if (result == null) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "homework not found")
                ];
            }

            homeworkId = result.Value;
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select homework failed:{e.Message}")];
        }
        
        List<uint> memberList;
        try {
            await using var cmd = new MySqlCommand(isFinished?SqlSelectFinishedMember:SqlSelectUnfinishedMember, connection, transaction);
            cmd.Parameters.AddWithValue("@homeworkId", homeworkId);
            await using var reader = cmd.ExecuteReader();
            memberList = new List<uint>();
            while (await reader.ReadAsync()) {
                memberList.Add(reader.GetUInt32("user_uin"));
            }
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select homework member failed:{e.Message}")];
        }
        
        await transaction.CommitAsync();
        return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"homework member list({isFinished}):{string.Join(",", memberList)}")];
        
    }
    
    private const string SqlSelectSubmitRegex = "SELECT regex FROM homework_regex WHERE homework_id = @homeworkId;";

    private static async Task<List<Certificate>> LsHomeworkDetail(CommandHandler commandHandler, uint friendUin,
        (HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();


        var actorName = commandHandler.Next().Command;
        var homeworkName = commandHandler.Next().Command;

        uint homeworkId;
        try {
            var result = await GetHomeworkId(homeworkName, actorName, connection, transaction);
            if (result == null) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "homework not found")
                ];
            }

            homeworkId = result.Value;
        } catch (Exception e) {
            return [
                YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                    $"select homework failed:{e.Message}")
            ];
        }

        string? introduction;
        DateTime deadline;
        bool isRelease;
        try {
            await using var cmd = new MySqlCommand(SqlSelectHomeworkById, connection, transaction);
            cmd.Parameters.AddWithValue("@id", homeworkId);


            await using var reader = cmd.ExecuteReader();
            if (!await reader.ReadAsync()) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "homework not found")
                ];
            }

            introduction = reader.IsDBNull(reader.GetOrdinal("introduction"))
                ? null
                : reader.GetString("introduction");
            deadline = reader.GetDateTime("deadline");
            isRelease = reader.GetBoolean("is_release");
        } catch (Exception e) {
            return [
                YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                    $"select homework failed:{e.Message}")
            ];
        }

        List<string> regexList;

        try {
            await using var cmd = new MySqlCommand(SqlSelectSubmitRegex, connection, transaction);
            cmd.Parameters.AddWithValue("@homeworkId", homeworkId);
            await using var reader = cmd.ExecuteReader();
            regexList = new List<string>();
            while (await reader.ReadAsync()) {
                regexList.Add(reader.GetString("regex"));
            }
        } catch (Exception e) {
            return [
                YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                    $"select homework regex failed:{e.Message}")
            ];
        }


        await transaction.CommitAsync();
        return [
            YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                $"homework detail: \nname: {homeworkName}\nactorName: {actorName}\ndeadline: {deadline}\nisRelease: {isRelease}\nregexList: {string.Join(",", regexList)} \nintroduction: {introduction} ")
        ];

    }

    private const string SqlSelectDeadline = "SELECT deadline FROM homework WHERE id = @homeworkId;";

    private static async Task<List<Certificate>> Submit(CommandHandler commandHandler, uint friendUin,
        (HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();


        var actorName = commandHandler.Next().Command;
        var homeworkName = commandHandler.Next().Command;

        uint homeworkId;
        try {
            var result = await GetHomeworkId(homeworkName, actorName, connection, transaction);
            if (result == null) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "homework not found")
                ];
            }

            homeworkId = result.Value;
        } catch (Exception e) {
            return [
                YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                    $"select homework failed:{e.Message}")
            ];
        }

        try {
            await using var cmd = new MySqlCommand(SqlSelectDeadline, connection, transaction);
            cmd.Parameters.AddWithValue("@homeworkId", homeworkId);
            var deadline = Convert.ToDateTime(await cmd.ExecuteScalarAsync());
            if (deadline < DateTime.Now) {
                return [
                    YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                        "deadline has passed")
                ];
            }
        } catch (Exception e) {
            return [
                YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                    $"select homework failed:{e.Message}")
            ];
        }
        
        List<string> regexStrList;

        try {
            await using var cmd = new MySqlCommand(SqlSelectSubmitRegex, connection, transaction);
            cmd.Parameters.AddWithValue("@homeworkId", homeworkId);
            await using var reader = cmd.ExecuteReader();
            regexStrList = new List<string>();
            while (await reader.ReadAsync()) {
                regexStrList.Add(reader.GetString("regex"));
            }
        } catch (Exception e) {
            return [
                YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                    $"select homework regex failed:{e.Message}")
            ];
        }
        
        var regexList = regexStrList.Select(regexStr => new Regex(regexStr)).ToList();
        
        
        await transaction.CommitAsync();
        return [
            YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                "start receiving submission.Please send the file"),
            ReadCombineData.GetInstance()
                .GetCertificate(
                    new CombinedData(new AddSubmitMissionData(regexList, homeworkId, friendUin), null))
        ];
    }

    private const string SqlSelectUserUnFinishedHomework = """
                                                           SELECT
                                                           	    homework.id AS homework_id,
                                                                   homework.name AS homework_name,
                                                                   actor.name AS actor_name
                                                               FROM
                                                           	    homework JOIN actor ON homework.actor_id = actor.id
                                                               WHERE
                                                           	    @userUin NOT IN ( SELECT user_uin FROM homework_member WHERE homework_member.homework_id = homework.id ) 
                                                           	AND @userUin IN ( SELECT user_uin FROM actor_member WHERE actor_member.actor_id = homework.actor_id )
                                                           	AND homework.create_time >= CURDATE() - INTERVAL @duration DAY;
                                                           """;

    private const string SqlSelectUserFinishedHomework = """
                                                         SELECT
                                                         	    homework.id AS homework_id,
                                                                 homework.name AS homework_name,
                                                                 actor.name AS actor_name
                                                             FROM
                                                         	    homework JOIN actor ON homework.actor_id = actor.id
                                                             WHERE
                                                         	    @userUin IN ( SELECT user_uin FROM homework_member WHERE homework_member.homework_id = homework.id ) 
                                                         	AND homework.create_time >= CURDATE() - INTERVAL @duration DAY;
                                                         """;
    
    private static async Task<List<Certificate>> LsMineHomework(CommandHandler commandHandler, uint friendUin,
        (HomeworkConfig homeworkConfig, DatabaseConfig databaseConfig) configs) {

        var isFinished = bool.Parse(commandHandler.Next().Command);
        bool result = false;
        int duration = 0;
        try
        {
            result  = TryParse(commandHandler.Next().Command, out duration);
        }
        catch (Exception e)
        {
            result = false;
        }
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        
        List<(uint homeworkId, string homeworkName, string actorName)> homeworkList;
        try {
            await using var cmd = new MySqlCommand(isFinished ? SqlSelectUserFinishedHomework : SqlSelectUserUnFinishedHomework, connection, transaction);
            cmd.Parameters.AddWithValue("@userUin", friendUin);
            cmd.Parameters.AddWithValue("@duration", result ? duration: 3650);
            await using var reader = cmd.ExecuteReader();
            homeworkList = new List<(uint homeworkId, string homeworkName, string actorName)>();
            while (await reader.ReadAsync()) {
                homeworkList.Add((reader.GetUInt32("homework_id"), reader.GetString("homework_name"), reader.GetString("actor_name")));
            }
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority, $"select homework failed:{e.Message}")];
        }
        
        await transaction.CommitAsync();
        var message = isFinished ? "finished" : "unfinished";
        return [
            YourBotUtil.SendToFriendMessage(friendUin, configs.homeworkConfig.Priority,
                $"{message} homework list:\n {string.Join("\n", homeworkList.Select(tuple => $"[{tuple.actorName}]:{tuple.homeworkName}"))}")
        ];
    }
}