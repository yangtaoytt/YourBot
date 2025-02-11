using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using MySql.Data.MySqlClient;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.InitData.Group.Command;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command;

public class MemeProcessor : IProcessorCore<CommandData,
    AsyncSharedDataWrapper<(BotContext botContext, MemeInitData memeInitData)>, (BotContext botContext, MemeInitData
    memeInitData)> {
    public static IServiceAttribute<CommandData> GetServiceAttribute() {
        return ReadGroupCommandAttribute.GetInstance();
    }

    public static AsyncSharedDataWrapper<(BotContext botContext, MemeInitData memeInitData)> Init(
        (BotContext botContext, MemeInitData memeInitData) initData) {
        return new AsyncSharedDataWrapper<(BotContext botContext, MemeInitData memeInitData)>(initData);
    }

    public static void Final(AsyncSharedDataWrapper<(BotContext botContext, MemeInitData memeInitData)> sharedData,
        Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(CommandData data,
        AsyncSharedDataWrapper<(BotContext botContext, MemeInitData memeInitData)> sharedData, Logger2Event? logger) {
        var groupUin = data.GroupUin;
        var memberUin = data.MessageChain.FriendUin;

        var inRage = await sharedData.ExecuteAsync(reference => {
            var value = reference.Value;
            return Task.FromResult(value.memeInitData.GroupDic.ContainsKey(groupUin) &&
                                   value.memeInitData.GroupDic[groupUin].Contains(memberUin));
        });
        if (!inRage) {
            return [];
        }

        var command = data.Command;
        if (command != "meme") {
            return [];
        }

        var parameters = data.Parameters;
        var initData = await sharedData.ExecuteAsync(reference => Task.FromResult(reference.Value.memeInitData));
        if (parameters.Count == 0) {
            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(Util.BuildSendToGroupMessageData(
                        data.GroupUin, initData.Priority, "wrong parameters"))
            ];
        }

        var parameter = parameters[0];
        if (parameters[0] == "save") {
            return await SaveMeme(data, sharedData, initData);
        }

        if (parameters[0] == "get") {
            return await GetMeme(data, sharedData, initData);
        }

        return [
            CanSendGroupMessageAttribute.GetInstance()
                .GetCertificate(Util.BuildSendToGroupMessageData(
                    data.GroupUin, initData.Priority, "wrong parameters"))
        ];
    }

    private static async Task<List<Certificate>> GetMeme(CommandData data,
        AsyncSharedDataWrapper<(BotContext botContext, MemeInitData memeInitData)> sharedData, MemeInitData initData) {
        try {
            await using var conn = new MySqlConnection(initData.ConnectionString);
            await conn.OpenAsync();

            // Get random meme
            int memeId;
            await using (var cmd = new MySqlCommand("SELECT id FROM meme ORDER BY RAND() LIMIT 1", conn)) {
                var result = await cmd.ExecuteScalarAsync();
                if (result == null) {
                    return [
                        CanSendGroupMessageAttribute.GetInstance()
                            .GetCertificate(
                                Util.BuildSendToGroupMessageData(data.GroupUin, initData.Priority, "No memes found"))
                    ];
                }

                memeId = Convert.ToInt32(result);
            }

            // Get message components
            var messageBuilder = MessageBuilder.Group(data.GroupUin);
            var entities = new List<(int, object)>();

            // Process faces
            await using (var cmd = new MySqlCommand(
                             "SELECT face_id, is_large,sequence FROM meme_face_message WHERE meme_id = @id ORDER BY sequence",
                             conn)) {
                cmd.Parameters.AddWithValue("@id", memeId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    entities.Add((reader.GetInt32(reader.GetOrdinal("sequence")), new FaceEntity(
                        (ushort)reader.GetInt32(reader.GetOrdinal("face_id")),
                        reader.GetBoolean(reader.GetOrdinal("is_large"))
                    )));
                }
            }

            // Process images
            await using (var cmd = new MySqlCommand(
                             "SELECT path, sequence FROM meme_image_message WHERE meme_id = @id ORDER BY sequence",
                             conn)) {
                cmd.Parameters.AddWithValue("@id", memeId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    var path = reader.GetString(reader.GetOrdinal("path"));
                    var imageBytes = await File.ReadAllBytesAsync(path);
                    entities.Add((reader.GetInt32(reader.GetOrdinal("sequence")), imageBytes));
                }
            }

            // Process texts
            await using (var cmd = new MySqlCommand(
                             "SELECT content, sequence FROM meme_text_message WHERE meme_id = @id ORDER BY sequence",
                             conn)) {
                cmd.Parameters.AddWithValue("@id", memeId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    entities.Add((reader.GetInt32(reader.GetOrdinal("sequence")),
                        new TextEntity(reader.GetString(reader.GetOrdinal("content")))));
                }
            }

            // Build message chain
            entities.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            foreach (var (_, entity) in entities) {
                if (entity is FaceEntity faceEntity) {
                    messageBuilder.Face(faceEntity.FaceId, faceEntity.IsLargeFace);
                } else if (entity is byte[] imageBytes) {
                    messageBuilder.Image(imageBytes);
                } else if (entity is TextEntity textEntity) {
                    messageBuilder.Text(textEntity.Text);
                }
            }

            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(
                        new SendToGroupMessageData(
                            new Priority(initData.Priority, PriorityStrategy.Share),
                            messageBuilder.Build()
                        )
                    )
            ];
        } catch (Exception ex) {
            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(
                        Util.BuildSendToGroupMessageData(data.GroupUin, initData.Priority, $"Error: {ex.Message}")
                    )
            ];
        }
    }

    private static async Task<List<Certificate>> SaveMeme(CommandData data,
        AsyncSharedDataWrapper<(BotContext botContext, MemeInitData memeInitData)> sharedData, MemeInitData initData) {
        var messageChain = data.MessageChain;

        if (messageChain[0] is not ForwardEntity forwardEntity) {
            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(Util.BuildSendToGroupMessageData(
                        data.GroupUin, initData.Priority, "wrong parameters type"))
            ];
        }

        var targetMessageChain = (await sharedData.ExecuteAsync(reference => reference.Value.botContext.GetGroupMessage(
            data.GroupUin,
            forwardEntity.Sequence, forwardEntity.Sequence)))![0];

        await SaveMeme(targetMessageChain, initData.ConnectionString, initData.ImageDir);

        return [
            CanSendGroupMessageAttribute.GetInstance()
                .GetCertificate(Util.BuildSendToGroupMessageData(
                    data.GroupUin, initData.Priority, "save successfully"))
        ];
    }


    private static async Task SaveMeme(MessageChain messageChain, string connectionString, string imageDir) {
        // Save meme to database
        await using var conn = new MySqlConnection(connectionString);
        conn.Open();
        const string queryMemeHash = "SELECT COUNT(*) FROM meme WHERE hash_code = @hash_code";
        const string insertMeme = "INSERT INTO meme (sender_uin, hash_code) VALUES (@sender_uin, @hash_code)";
        const string queryMemeId = "SELECT id FROM meme WHERE hash_code = @hash_code";

        const string queryImageName = "SELECT COUNT(path) FROM meme_image_message";
        const string insertFace =
            "INSERT INTO meme_face_message (face_id, is_large, meme_id, sequence) VALUES (@face_id, @is_large, @meme_id, @sequence)";
        const string insertImage =
            "INSERT INTO meme_image_message (size, path, meme_id, sequence) VALUES (@size, @path, @meme_id, @sequence)";
        const string insertText =
            "INSERT INTO meme_text_message (content, meme_id, sequence) VALUES (@content, @meme_id, @sequence)";

        var transaction = await conn.BeginTransactionAsync();

        try {
            var hash = messageChain.GetHashCode();
            await using (var cmd = new MySqlCommand(queryMemeHash, conn, transaction)) {
                cmd.Parameters.AddWithValue("@hash_code", hash);
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0) {
                    throw new Exception("Meme already exists");
                }
            }

            await using (var cmd = new MySqlCommand(insertMeme, conn, transaction)) {
                cmd.Parameters.AddWithValue("@sender_uin", messageChain.FriendUin);
                cmd.Parameters.AddWithValue("@hash_code", hash);
                cmd.ExecuteNonQuery();
            }

            int memeId;
            await using (var cmd = new MySqlCommand(queryMemeId, conn, transaction)) {
                cmd.Parameters.AddWithValue("@hash_code", hash);
                memeId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            for (var i = 0; i < messageChain.Count; i++) {
                var message = messageChain[i];
                if (message is FaceEntity faceEntity) {
                    await using var cmd = new MySqlCommand(insertFace, conn, transaction);
                    cmd.Parameters.AddWithValue("@face_id", faceEntity.FaceId);
                    cmd.Parameters.AddWithValue("@is_large", faceEntity.IsLargeFace ? 1 : 0);
                    cmd.Parameters.AddWithValue("@meme_id", memeId);
                    cmd.Parameters.AddWithValue("@sequence", i);
                } else if (message is ImageEntity imageEntity) {
                    string name;
                    await using (var cmd = new MySqlCommand(queryImageName, conn, transaction)) {
                        var id = Convert.ToInt32(cmd.ExecuteScalar());
                        name = id + ".jpg";
                    }

                    var imagePath = Path.Combine(imageDir, name);
                    await Util.SaveImage(imageEntity.ImageUrl, imagePath);

                    await using (var cmd = new MySqlCommand(insertImage, conn, transaction)) {
                        cmd.Parameters.AddWithValue("@size", imageEntity.ImageSize);
                        cmd.Parameters.AddWithValue("@path", imagePath);
                        cmd.Parameters.AddWithValue("@meme_id", memeId);
                        cmd.Parameters.AddWithValue("@sequence", i);
                        cmd.ExecuteNonQuery();
                    }
                } else if (message is TextEntity textEntity) {
                    await using var cmd = new MySqlCommand(insertText, conn, transaction);

                    cmd.Parameters.AddWithValue("@content",
                        textEntity.Text.Length > 250 ? textEntity.Text.Substring(0, 250) : textEntity.Text);
                    cmd.Parameters.AddWithValue("@meme_id", memeId);
                    cmd.Parameters.AddWithValue("@sequence", i);
                    cmd.ExecuteNonQuery();
                }
            }

            await transaction.CommitAsync();
        } catch (Exception e) {
            await transaction.RollbackAsync();
            throw;
        }
    }
}