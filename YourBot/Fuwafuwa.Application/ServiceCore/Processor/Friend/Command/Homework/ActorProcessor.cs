using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using MySql.Data.MySqlClient;
using YourBot.Config.Implement.Level1;
using YourBot.Config.Implement.Level1.Service.Group.Command.Homework;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Attribute.Processor.Friend;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;
using YourBot.Utils;
using YourBot.Utils.Command;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command.Homework;

public class ActorProcessor : IProcessorCore<FriendCommandData,
    AsyncSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext, Dictionary<uint, string> userUinToName)>, (ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext)> {
    
    public static IServiceAttribute<FriendCommandData> GetServiceAttribute() {
        return ReadFriendCommandAttribute.GetInstance();
    }
    public static AsyncSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext, Dictionary<uint, string> userUinToName)> Init((ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext) initData) {
        return new AsyncSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext, Dictionary<uint, string> userUinToName)>((initData.actorConfig, initData.databaseConfig, initData.botContext, []));
    }
    public static void Final(AsyncSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext, Dictionary<uint, string> userUinToName)> sharedData, Logger2Event? logger) { }
    public async Task<List<Certificate>> ProcessData(FriendCommandData data, AsyncSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext, Dictionary<uint, string> userUinToName)> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;
        var friendUin = data.FriendUin;
        var configs = await sharedData.ExecuteAsync(reference => Task.FromResult((reference.Value.actorConfig, reference.Value.databaseConfig)));
        if (!YourBotUtil.CheckFriendPermission(configs.actorConfig, friendUin)) {
            return [];
        }

        var commandHandler = data.CommandHandler;
        if (commandHandler.Command != "actor") {
            return [];
        }
        
        try {
            switch (commandHandler.Next().Command) {
                case "add":
                    return await AddActor(commandHandler, friendUin, configs);
                case "rm":
                    return await RmActor(commandHandler, friendUin, configs);
                case "ls":
                    return await LsActor(friendUin, configs);
                case "ls-mine":
                    return await LsMineActor(friendUin, configs);
                case "ls-member":
                    return await LsMember(commandHandler, friendUin, configs, sharedData);
                case "rm-member":
                    return await RmMember(commandHandler, friendUin, configs);
                case "invite-member":
                    return await InviteMember(commandHandler, friendUin, configs, sharedData);
                case "accept-invite":
                    return await AcceptInviteMember(commandHandler, friendUin, configs);
                default:
                    return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, "wrong parameters")];
            }
        } catch (InvalidCommandException) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, "wrong parameters")];
        }
    }

    private const string SqlAddActor =
        "INSERT INTO actor (creator_uin, name) VALUES (@creatorUin, @name);SELECT LAST_INSERT_ID();";
    private const string SqlSelectActorId = "SELECT id FROM actor WHERE creator_uin = @creatorUin AND name = @name;";
    private const string SqlRmActor = "DELETE FROM actor WHERE id = @id;";
    private const string SqlLsActor = "SELECT * FROM actor WHERE creator_uin = @creatorUin;";
    private static async Task<List<Certificate>> AddActor(CommandHandler commandHandler, uint friendUin,
        (ActorConfig actorConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        
        var actorName = commandHandler.Next().Command;
        try {
            await using var cmd = new MySqlCommand(SqlAddActor, connection, transaction);
            cmd.Parameters.AddWithValue("@creatorUin", friendUin);
            cmd.Parameters.AddWithValue("@name", actorName);

            await cmd.ExecuteScalarAsync();
        } catch (Exception) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                "Add actor failed.Please ensure the actor does not exist.")];
        }
        await transaction.CommitAsync();
        return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
            "Add actor success.")];
    }
    private static async Task<List<Certificate>> RmActor(CommandHandler commandHandler, uint friendUin,
        (ActorConfig actorConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        
        var actorName = commandHandler.Next().Command;
        try {
            List<uint> actorIdList = new();
            await using (var cmd = new MySqlCommand(SqlSelectActorId, connection, transaction)) {
                cmd.Parameters.AddWithValue("@creatorUin", friendUin);
                cmd.Parameters.AddWithValue("@name", actorName);
                
                await using ( var reader = cmd.ExecuteReader()) {
                    while (await reader.ReadAsync()) {
                        actorIdList.Add(reader.GetUInt32("id"));
                    }
                }
            }
            if (actorIdList.Count != 1) {
                return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                    "Remove actor failed.Please ensure the actor exists.")];
            }

            await using (var cmd = new MySqlCommand(SqlRmActor, connection, transaction)) {
                cmd.Parameters.AddWithValue("@id", actorIdList[0]);
                await cmd.ExecuteNonQueryAsync();
            }
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                $"Remove actor failed.({e.Message})")];
        }
        await transaction.CommitAsync();
        return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, 
            "Remove actor success.")];
    }
    private static async Task<List<Certificate>> LsActor(uint friendUin,
        (ActorConfig actorConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        List<string> actorList;
        try {
            actorList = new List<string>();
            await using (var cmd = new MySqlCommand(SqlLsActor, connection, transaction)) {
                cmd.Parameters.AddWithValue("@creatorUin", friendUin);
                await using var reader = cmd.ExecuteReader();

                while (await reader.ReadAsync()) {
                    actorList.Add(reader.GetString("name"));
                }
            }

        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                $"List actor failed.({e.Message})")];
        }
        await transaction.CommitAsync();
        return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, 
            "Actors: \n" + string.Join("\n ", actorList.Select((actor, index) => $"{index + 1}. {actor}")))];
    }
    
    private const string SqlLsMember =
        "SELECT am.user_uin  FROM actor_member am WHERE am.actor_id = @actorId;";

    private const string SqlLsAllActorMember =
        "SELECT a.name AS actor_name, am.user_uin FROM actor a JOIN actor_member am ON a.id = am.actor_id WHERE a.creator_uin = @creatorUin;";

    private const string SqlRmMember =
        "DELETE FROM actor_member WHERE actor_id = @actorId AND user_uin = @memberId;";

    private const string SqlCheckActorMember =
        "SELECT COUNT(*) FROM actor_member WHERE actor_id = @actorId AND user_uin = @userId;";
    private const string SqlAddMember =
        "INSERT INTO actor_member (actor_id, user_uin) VALUES (@actorId, @memberId);";
    private const string SqlAddInviteMember =
        "INSERT INTO actor_member_invitation (actor_id, user_uin) VALUES (@actorId, @memberId);SELECT LAST_INSERT_ID();";
    private const string SqlSelectInviteMemberById =
        "SELECT actor_id, user_uin FROM actor_member_invitation WHERE id = @invitationId;";
    private const string SqlSelectInviteMemberByActorIdUin =
        "SELECT id FROM actor_member_invitation WHERE actor_id = @actorId AND user_uin = @UserUin;";
    private const string SqlRmInviteMember =
        "DELETE FROM actor_member_invitation WHERE id = @invitationId;";
    private static async Task<List<Certificate>> LsMember(CommandHandler commandHandler, uint friendUin,
        (ActorConfig actorConfig, DatabaseConfig databaseConfig) configs, AsyncSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext, Dictionary<uint, string> userUinToName)> sharedData) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        if (commandHandler.TryNext() != null) {
            var actorName = commandHandler.Command;
            
            var actorIdList = new List<uint>();
            var memberUinList = new List<uint>();
            try {
                await using (var cmd = new MySqlCommand(SqlSelectActorId, connection, transaction)) {
                    cmd.Parameters.AddWithValue("@name", actorName);
                    cmd.Parameters.AddWithValue("@creatorUin", friendUin);
                    await using var reader = cmd.ExecuteReader();

                    while (await reader.ReadAsync()) {
                        actorIdList.Add(reader.GetUInt32("id"));
                    }
                }
                
                if (actorIdList.Count != 1) {
                    return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                        "Remove actor failed.Please ensure the actor exists.")];
                }
                
                await using (var cmd = new MySqlCommand(SqlLsMember, connection, transaction)) {
                    cmd.Parameters.AddWithValue("@actorId", actorIdList[0]);
                    await using var reader = cmd.ExecuteReader();
                    
                    while (await reader.ReadAsync()) {
                        memberUinList.Add(reader.GetUInt32("user_uin"));
                    }
                }
                
            } catch (Exception e) {
                return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                    $"List member failed.({e.Message})")];
            }
            await transaction.CommitAsync();

            var memberUinNameList = memberUinList.Select(memberUin => (memberUin,GetUserName(memberUin, sharedData).Result)).ToList();
            
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, 
                $"Member list of {actorName}: \n" + string.Join("\n ", memberUinNameList.Select((member, index) => $"{index + 1}. {member.memberUin}({member.Result})")))];
        }
        
        var actorMemberDict = new Dictionary<string, List<uint>>();
        
        try {
            await using (var cmd = new MySqlCommand(SqlLsAllActorMember, connection, transaction)) {
                cmd.Parameters.AddWithValue("@creatorUin", friendUin);
                await using var reader = cmd.ExecuteReader();

                while (await reader.ReadAsync()) {
                    var actorName = reader.GetString("actor_name");
                    var userUin = reader.GetUInt32("user_uin");
                    if (!actorMemberDict.ContainsKey(actorName)) {
                        actorMemberDict[actorName] = new List<uint>();
                    }
                    actorMemberDict[actorName].Add(userUin);
                }
            }
            await transaction.CommitAsync();
            var message = "All actor member list: \n";
            foreach (var (actorName, memberList) in actorMemberDict) {
                message += $"Actor of {actorName}: " + string.Join(", ", memberList) + "\n";
            }
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, message)];
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                $"List all actor member failed.({e.Message})")];
        }
        
    }

    private static async Task<List<Certificate>> RmMember(CommandHandler commandHandler, uint friendUin,
        (ActorConfig actorConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        
        var actorName = commandHandler.Next().Command;
        uint memberId;
        try {
            memberId  = uint.Parse(commandHandler.Next().Command);
        } catch (Exception) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                "Remove member failed.Please ensure the member exists.")];
        }
        
        try {
            var actorIdList = new List<uint>();
            await using (var cmd = new MySqlCommand(SqlSelectActorId, connection, transaction)) {
                cmd.Parameters.AddWithValue("@name", actorName);
                cmd.Parameters.AddWithValue("@creatorUin", friendUin);

                await using var reader = cmd.ExecuteReader();

                while (await reader.ReadAsync()) {
                    actorIdList.Add(reader.GetUInt32("id"));
                }
            }
            if (actorIdList.Count != 1) {
                return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                    "Remove member failed.Please ensure the actor exists.")];
            }
            await using(var cmd = new MySqlCommand(SqlRmMember, connection, transaction)) {
                cmd.Parameters.AddWithValue("@actorId", actorIdList[0]);
                cmd.Parameters.AddWithValue("@memberId", memberId);
                await cmd.ExecuteNonQueryAsync();
            }

        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                $"Remove member failed.({e.Message})")];
        }
        
        await transaction.CommitAsync();
        return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, 
            "Remove member success.")];
    }

    private static async Task<List<Certificate>> InviteMember(CommandHandler commandHandler, uint friendUin,
        (ActorConfig actorConfig, DatabaseConfig databaseConfig) configs,
        AsyncSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext, Dictionary<uint, string> userUinToName)>
            sharedData) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync() ;

        var actorName = commandHandler.Next().Command;
        uint memberId;
        try {
            memberId  = uint.Parse(commandHandler.Next().Command);
        } catch (Exception) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                "Invite member failed.Please ensure the qq exists.")];
        }
        var friendList = await sharedData.ExecuteAsync(async reference => await reference.Value.botContext.FetchFriends());
        if (friendList.Find(friend => friend.Uin == memberId) == null) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                "Invite member failed.Please ensure the qq is friend with the bot.")];
        }

        string invitationId;
        try {
            var actorIdList = new List<uint>();
            await using (var cmd = new MySqlCommand(SqlSelectActorId, connection, transaction)) {
                cmd.Parameters.AddWithValue("@name", actorName);
                cmd.Parameters.AddWithValue("@creatorUin", friendUin);

                await using var reader = cmd.ExecuteReader();

                while (await reader.ReadAsync()) {
                    actorIdList.Add(reader.GetUInt32("id"));
                }
            }
            if (actorIdList.Count != 1) {
                return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                    "Invite member failed.Please ensure the actor exists.")];
            }
            var actorId = actorIdList[0];
            await using (var cmd = new MySqlCommand(SqlCheckActorMember, connection, transaction)) {
                cmd.Parameters.AddWithValue("@actorId", actorId);
                cmd.Parameters.AddWithValue("@userId", memberId);
                var result = await cmd.ExecuteScalarAsync();
                var count = Convert.ToInt32(result);
                if (count != 0) {
                    return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                        "Invite member failed.Please ensure the member does not already exist.")];
                }
            }

            await using (var cmd = new MySqlCommand(SqlSelectInviteMemberByActorIdUin, connection, transaction)) {
                cmd.Parameters.AddWithValue("@actorId", actorId);
                cmd.Parameters.AddWithValue("@UserUin", memberId);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null) {
                    var id = Convert.ToInt32(result);
                    invitationId = YourBotUtil.Encrypt(id);
                    return [
                        YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                            $"Invite member failed.The invitation already exists:[ {invitationId} ]."),
                    ];
                }
            }


            await using (var cmd = new MySqlCommand(SqlAddInviteMember, connection, transaction)) {
                cmd.Parameters.AddWithValue("@actorId", actorId);
                cmd.Parameters.AddWithValue("@memberId", memberId);
                invitationId = YourBotUtil.Encrypt(Convert.ToInt32(cmd.ExecuteScalar()));
            }
            
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                $"Invite member failed.({e.Message})")];
        }
        
        await transaction.CommitAsync();
        return [
            YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, 
                "Invite member success.Wait for the member to accept."),
            YourBotUtil.SendToFriendMessage(memberId, configs.actorConfig.Priority - 1, 
                $"You have been invited to join the actor {actorName} from {friendUin}.\nYou can accept the invitation by [ {invitationId} ].")
        ];
    }

    private static async Task<List<Certificate>> AcceptInviteMember(CommandHandler commandHandler, uint friendUin,
        (ActorConfig actorConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        
        var invitationId = YourBotUtil.Decrypt(commandHandler.Next().Command);
        try {
            uint actorId;
            uint userUin;
            await using (var cmd = new MySqlCommand(SqlSelectInviteMemberById, connection, transaction)) {
                cmd.Parameters.AddWithValue("@invitationId", invitationId);
                
                await using var reader = cmd.ExecuteReader();

                if (await reader.ReadAsync()) {
                    actorId = reader.GetUInt32("actor_id");
                    userUin = reader.GetUInt32("user_uin");
                    
                } else {
                    return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                        "Accept failed.Please ensure the invitation exists.")];
                }
            }
            if (userUin != friendUin) {
                return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                    "Accept failed.Please ensure the invitation is for you.")];
            }
            await using (var cmd = new MySqlCommand(SqlRmInviteMember, connection, transaction)) {
                cmd.Parameters.AddWithValue("@invitationId", invitationId);
                await cmd.ExecuteNonQueryAsync();
            }
            await using (var cmd = new MySqlCommand(SqlAddMember, connection, transaction)) {
                cmd.Parameters.AddWithValue("@actorId", actorId);
                cmd.Parameters.AddWithValue("@memberId", friendUin);
                await cmd.ExecuteNonQueryAsync();
            }
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                $"Accept failed.({e.Message})")];
        }
        await transaction.CommitAsync();
        return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority, 
            "Accept success")];
    }


    private static async Task<string> GetUserName(uint userUin,
        AsyncSharedDataWrapper<(ActorConfig actorConfig, DatabaseConfig databaseConfig, BotContext botContext, Dictionary<uint, string> userUinToName)>
            sharedData) {
        var isBuffered = await sharedData.ExecuteAsync(reference => {
            var flag = reference.Value.userUinToName.ContainsKey(userUin);
            if (!flag) {
                reference.Value.userUinToName[userUin] = "Unknown";
            }
            return Task.FromResult(flag);
        });
        if (!isBuffered) {
            await sharedData.ExecuteAsync(async reference => {
                var friendList = await reference.Value.botContext.FetchFriends();
                var friend = friendList.Find(friend => friend.Uin == userUin);
                reference.Value.userUinToName[userUin] = friend?.Nickname ?? "Unknown";
            });
        }
        return await sharedData.ExecuteAsync(reference => Task.FromResult(reference.Value.userUinToName[userUin]));
    }
    
    private const string SqlLsMineActor =
        "SELECT a.name  FROM actor a  JOIN actor_member am ON a.id = am.actor_id  WHERE am.user_uin = @userUin;";

    private static async Task<List<Certificate>> LsMineActor(uint friendUin,
        (ActorConfig actorConfig, DatabaseConfig databaseConfig) configs) {
        var connectionStr = configs.databaseConfig.ConnectionString;
        await using var connection = new MySqlConnection(connectionStr);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        List<string> actorList;
        try {
            await using (var cmd = new MySqlCommand(SqlLsMineActor, connection, transaction)) {
                cmd.Parameters.AddWithValue("@userUin", friendUin);
                actorList = new List<string>();
                await using var reader = cmd.ExecuteReader();

                while (await reader.ReadAsync()) {
                    actorList.Add(reader.GetString("name"));
                }
            }
        } catch (Exception e) {
            return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
                $"lsMine failed.({e.Message})")];
        }
        
        await transaction.CommitAsync();
        
        return [YourBotUtil.SendToFriendMessage(friendUin, configs.actorConfig.Priority,
            "Your Actors: \n" + string.Join("\n ", actorList.Select((actor, index) => $"{index + 1}. {actor}")))];
    }
    
}