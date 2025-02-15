using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using MySql.Data.MySqlClient;

namespace YourBot.Utils;

public static partial class YourBotUtil {
    private const string InsertGroupMessageChainSql = "INSERT INTO group_message_chain (group_uin, sender_uin,sender_name,sender_avatar) VALUES (@groupUin, @senderUin,@senderName,@senderAvatar); SELECT LAST_INSERT_ID();";
    private const string InsertTextMessageSql =
        "INSERT INTO text_message (content, message_chain_id, sequence) VALUES (@content, @messageChainId, @sequence)";
    private const string InsertFaceMessageSql =
        "INSERT INTO face_message (face_id, is_large, message_chain_id, sequence) VALUES (@faceId, @isLarge, @messageChainId, @sequence)";
    private const string InsertImageMessageSql =
        "INSERT INTO image_message (path, message_chain_id, sequence) VALUES ( @path, @messageChainId, @sequence)";
    private const string InsertMultiMessageSql =
        "INSERT INTO multi_message (message_chain_id, sequence) VALUES (@messageChainId, @sequence); SELECT LAST_INSERT_ID();";
    private const string InsertMultiMessage2MessageChainSql =
        "INSERT INTO multi_message_2_message_chain (multi_message_id, message_chain_id,sequence) VALUES (@multiMessageId, @messageChainId, @sequence)";
    private static async Task SaveTextMessage(int messageChainId, int sequence, TextEntity textEntity,
        MySqlConnection connection, MySqlTransaction transaction) {
        var content = textEntity.Text;
        if (content.Length > 1024) {
            content = content[..1024];
        }

        await using var cmd = new MySqlCommand(InsertTextMessageSql, connection, transaction);
        cmd.Parameters.AddWithValue("@content", content);
        cmd.Parameters.AddWithValue("@messageChainId", messageChainId);
        cmd.Parameters.AddWithValue("@sequence", sequence);
        cmd.ExecuteNonQuery();
    }
    
    private static async Task SaveFaceMessage(int messageChainId, int sequence, FaceEntity faceEntity,
        MySqlConnection connection, MySqlTransaction transaction) {
        await using var cmd = new MySqlCommand(InsertFaceMessageSql, connection, transaction);
        cmd.Parameters.AddWithValue("@faceId", faceEntity.FaceId);
        cmd.Parameters.AddWithValue("@isLarge", faceEntity.IsLargeFace);
        cmd.Parameters.AddWithValue("@messageChainId", messageChainId);
        cmd.Parameters.AddWithValue("@sequence", sequence);
        cmd.ExecuteNonQuery();
    }
    
    private static async Task SaveImageMessage(int messageChainId, int sequence, ImageEntity imageEntity, string imageDir,
        MySqlConnection connection, MySqlTransaction transaction) {
        var name = YourBotUtil.GetUniqueTimeString() + ".jpg";
        var path = Path.Combine(imageDir, name);
        await YourBotUtil.SaveImage(imageEntity.ImageUrl, path);
        
        await using var cmd = new MySqlCommand(InsertImageMessageSql, connection, transaction);
        cmd.Parameters.AddWithValue("@path", path);
        cmd.Parameters.AddWithValue("@messageChainId", messageChainId);
        cmd.Parameters.AddWithValue("@sequence", sequence);
        cmd.ExecuteNonQuery();
    }
    
    private static async Task SaveMultipleMessage(int messageChainId, int sequence, MultiMsgEntity multiMsgEntity, string imageDir,
        MySqlConnection connection, MySqlTransaction transaction) {
        int multiMessageId;
        await using (var cmd = new MySqlCommand(InsertMultiMessageSql, connection, transaction)) {
            cmd.Parameters.AddWithValue("@messageChainId", messageChainId);
            cmd.Parameters.AddWithValue("@sequence", sequence);
            
            multiMessageId = Convert.ToInt32(cmd.ExecuteScalar());
        }
        

        for (var i = 0; i < multiMsgEntity.Chains.Count; i++) {
            var chain = multiMsgEntity.Chains[i];
            
            int newMessageChainId;
            await using (var cmd = new MySqlCommand(InsertGroupMessageChainSql, connection, transaction)) {
                cmd.Parameters.AddWithValue("@groupUin", chain.GroupUin);
                cmd.Parameters.AddWithValue("@senderUin", chain.GroupMemberInfo!.Uin);
                cmd.Parameters.AddWithValue("@senderName", chain.GroupMemberInfo!.MemberName);
                cmd.Parameters.AddWithValue("@senderAvatar", chain.GroupMemberInfo!.Avatar);
                
                newMessageChainId = Convert.ToInt32(cmd.ExecuteScalar());
            }
            
            await using (var cmd = new MySqlCommand(InsertMultiMessage2MessageChainSql, connection, transaction)) {
                cmd.Parameters.AddWithValue("@multiMessageId", multiMessageId);
                cmd.Parameters.AddWithValue("@messageChainId", newMessageChainId);
                cmd.Parameters.AddWithValue("@sequence", i);
                cmd.ExecuteNonQuery();
            }
            
            await SaveMessageChain(newMessageChainId, chain, connection, transaction, imageDir);
        }
    }
    
    private static async Task SaveMessageChain(int messageGroupId, MessageChain messageChain,
        MySqlConnection connection, MySqlTransaction transaction, string imageDir) {
        for (var i = 0; i < messageChain.Count; ++i) {
            var message = messageChain[i];
            switch (message) {
                case TextEntity textEntity:
                    await SaveTextMessage(messageGroupId, i, textEntity, connection, transaction);
                    break;
                case FaceEntity faceEntity:
                    await SaveFaceMessage(messageGroupId, i, faceEntity, connection, transaction);
                    break;
                case ImageEntity imageEntity:
                    await SaveImageMessage(messageGroupId, i, imageEntity, imageDir, connection, transaction);
                    break;
                case MultiMsgEntity multiMsgEntity:
                    await SaveMultipleMessage(messageGroupId, i, multiMsgEntity, imageDir, connection, transaction);
                    break;
            }
        }
    }

    public static async Task<int> SaveMessageChain(MessageChain messageChain,string connectionString, string imageDir) {
        await using var connection = new MySqlConnection(connectionString);
        connection.Open();
        var transaction = await connection.BeginTransactionAsync();
        int messageGroupId;
        try {
            await using (var cmd = new MySqlCommand(InsertGroupMessageChainSql, connection, transaction)) {
                cmd.Parameters.AddWithValue("@groupUin", messageChain.GroupUin);
                cmd.Parameters.AddWithValue("@senderUin", messageChain.GroupMemberInfo!.Uin);
                cmd.Parameters.AddWithValue("@senderName", messageChain.GroupMemberInfo!.MemberName);
                cmd.Parameters.AddWithValue("@senderAvatar", messageChain.GroupMemberInfo!.Avatar);
                
                messageGroupId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            await SaveMessageChain(messageGroupId, messageChain, connection, transaction, imageDir);
        } catch (Exception) {
            await transaction.RollbackAsync();
            throw;
        }
        
        await transaction.CommitAsync();
        return messageGroupId;
    }
    
    private const string GetGroupMessageChainSql = "SELECT id, group_uin, sender_uin,sender_name,sender_avatar FROM group_message_chain WHERE id = @messageChainId";
    private const string GetTextMessagesSql =
        "SELECT id, content, message_chain_id, sequence FROM text_message WHERE message_chain_id = @messageChainId";
    private const string GetImageMessagesSql =
        "SELECT id, path, message_chain_id, sequence FROM image_message WHERE message_chain_id = @messageChainId";
    private const string GetFaceMessagesSql = 
        "SELECT id, face_id, is_large, message_chain_id, sequence FROM face_message WHERE message_chain_id = @messageChainId";
    private const string GetMultiMessagesSql = 
        "SELECT id, message_chain_id,sequence FROM multi_message WHERE message_chain_id = @messageChainId";
    private const string GetMultiMessage2MessageChainSql = 
        "SELECT multi_message_id, message_chain_id, sequence FROM multi_message_2_message_chain WHERE multi_message_id = @multiMessageId";

    private static List<(uint sequence, TextEntity textEntity)> GetTextMessage(uint messageChainId, MySqlConnection connection, MySqlTransaction transaction) {
        var result = new List<(uint sequence, TextEntity textEntity)>();
        using var cmd = new MySqlCommand(GetTextMessagesSql, connection, transaction);
        cmd.Parameters.AddWithValue("@messageChainId", messageChainId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            var sequence = reader.GetUInt32("sequence");
            var content = reader.GetString("content");
            result.Add((sequence, new TextEntity(content)));
        }

        return result;
    }
    
    private static async Task<List<(uint sequence, ImageEntity imageEntity)>> GetImageMessage(uint messageChainId, MySqlConnection connection, MySqlTransaction transaction) {
        var result = new List<(uint sequence, ImageEntity imageEntity)>();
        await using var cmd = new MySqlCommand(GetImageMessagesSql, connection, transaction);
        cmd.Parameters.AddWithValue("@messageChainId", messageChainId);
        await using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            var sequence = reader.GetUInt32("sequence");
            var path = reader.GetString("path");
            result.Add((sequence, new ImageEntity(path)));
        }

        return result;
    }
    
    private static List<(uint sequence, FaceEntity faceEntity)> GetFaceMessage(uint messageChainId, MySqlConnection connection, MySqlTransaction transaction) {
        var result = new List<(uint sequence, FaceEntity faceEntity)>();
        using var cmd = new MySqlCommand(GetFaceMessagesSql, connection, transaction);
        cmd.Parameters.AddWithValue("@messageChainId", messageChainId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            var sequence = reader.GetUInt32("sequence");
            var faceId = reader.GetUInt32("face_id");
            var isLarge = reader.GetBoolean("is_large");
            result.Add((sequence, new FaceEntity((ushort)faceId, isLarge)));
        }

        return result;
    }
    private static async Task<List<(uint sequence, MultiMsgEntity multiMsgEntity)>> GetMultiMessage(uint messageChainId, MySqlConnection connection, MySqlTransaction transaction) {
        var result = new List<(uint sequence, MultiMsgEntity multiMsgEntity)>();
        List<(uint sequence, uint multiMessageId)> multiMessages = [];
        
        await using (var cmd = new MySqlCommand(GetMultiMessagesSql, connection, transaction)) {
            cmd.Parameters.AddWithValue("@messageChainId", messageChainId);
            await using var reader = cmd.ExecuteReader();
        

            while (reader.Read()) {
                var sequence = reader.GetUInt32("sequence");
                var multiMessageId = reader.GetUInt32("id");
                multiMessages.Add((sequence, multiMessageId));
            }
        }

        foreach (var (sequence, multiMessageId) in multiMessages) {
            var chains = new List<MessageChain>();
            List<uint> messageChainIds = [];
            await using (var cmd2 = new MySqlCommand(GetMultiMessage2MessageChainSql, connection, transaction)) {
                cmd2.Parameters.AddWithValue("@multiMessageId", multiMessageId);
                await using var reader2 = cmd2.ExecuteReader();
                while (reader2.Read()) {
                    var newMessageChainId = reader2.GetUInt32("message_chain_id");
                    messageChainIds.Add(newMessageChainId);
                }
            }

            foreach (var chainId in messageChainIds) {
                chains.Add(await GetMessageChain(chainId, connection, transaction));
            }
            
            result.Add((sequence, new MultiMsgEntity(chains)));
        }

        return result;
    }
    
    private static async Task<MessageChain> GetMessageChain(uint messageChainId, MySqlConnection connection, MySqlTransaction transaction) {
        uint groupUin;
        uint? senderUin;
        string senderName;
        string senderAvatar;
        await using (var cmd = new MySqlCommand(GetGroupMessageChainSql, connection, transaction)) {
            cmd.Parameters.AddWithValue("@messageChainId", messageChainId);
            await using var reader = cmd.ExecuteReader();
            
            if (!reader.Read()) {
                throw new Exception("Message chain not found");
            }
            groupUin = reader.GetUInt32("group_uin");
            senderUin = reader.IsDBNull(reader.GetOrdinal("sender_uin")) ? null : reader.GetUInt32("sender_uin");
            senderName = reader.GetString("sender_name");
            senderAvatar = reader.GetString("sender_avatar");
        }
        
        var textMessages = GetTextMessage(messageChainId, connection, transaction);
        var imageMessages = await GetImageMessage(messageChainId, connection, transaction);
        var faceMessages = GetFaceMessage(messageChainId, connection, transaction);
        var multiMessages = await GetMultiMessage(messageChainId, connection, transaction);
        var messageChain = MessageBuilder.Group((uint)groupUin).FriendName(senderName).FriendAvatar(senderAvatar).Build();
        List<(uint sequence, IMessageEntity messageEntity)> allMessages = [];
        allMessages.AddRange(textMessages.Select(x => (x.sequence, (IMessageEntity)x.textEntity)));
        allMessages.AddRange(imageMessages.Select(x => (x.sequence, (IMessageEntity)x.imageEntity)));
        allMessages.AddRange(faceMessages.Select(x => (x.sequence, (IMessageEntity)x.faceEntity)));
        allMessages.AddRange(multiMessages.Select(x => (x.sequence, (IMessageEntity)x.multiMsgEntity)));
        allMessages = allMessages.OrderBy(x => x.sequence).ToList();
        foreach (var (_, message) in allMessages) {
            messageChain.Add(message);
        }

        return messageChain;
    }
    
    
    public static async Task<MessageChain> GetMessageChain(uint messageChainId, string connectionString) {
        await using var connection = new MySqlConnection(connectionString);
        connection.Open();
        await using var transaction = await connection.BeginTransactionAsync();
        MessageChain messageChain;
        try {
            messageChain = await GetMessageChain(messageChainId, connection, transaction);
        } catch (Exception) {
            await transaction.RollbackAsync();
            throw;
        }
        await transaction.CommitAsync();

        return messageChain;
    }
}