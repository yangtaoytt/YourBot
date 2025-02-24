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
using YourBot.Config.Implement.Level1;
using YourBot.Config.Implement.Level1.Service.Group.Command;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command;

public class MemeProcessor : IProcessorCore<GroupCommandData,
    AsyncSharedDataWrapper<(BotContext botContext, (MemeConfig memeConfig, DatabaseConfig databaseConfig)
        configs)>, (BotContext botContext, (MemeConfig memeConfig, DatabaseConfig databaseConfig)
    configs)> {
    public static IServiceAttribute<GroupCommandData> GetServiceAttribute() {
        return ReadGroupCommandAttribute.GetInstance();
    }

    public static AsyncSharedDataWrapper<(BotContext botContext, (MemeConfig memeConfig, DatabaseConfig databaseConfig) configs)> Init(
        (BotContext botContext, (MemeConfig memeConfig, DatabaseConfig databaseConfig) configs) initData) {
        return new AsyncSharedDataWrapper<(BotContext botContext, (MemeConfig memeConfig, DatabaseConfig
            databaseConfig) configs)>(initData);
    }

    public static void Final(AsyncSharedDataWrapper<(BotContext botContext, (MemeConfig memeConfig, DatabaseConfig databaseConfig) configs)> sharedData, Logger2Event? logger) { }
    public async Task<List<Certificate>> ProcessData(GroupCommandData data, AsyncSharedDataWrapper<(BotContext botContext, (MemeConfig memeConfig, DatabaseConfig databaseConfig) configs)> sharedData, Logger2Event? logger) {
        var groupUin = data.GroupUin;
        
        var configs = await sharedData.ExecuteAsync(reference => Task.FromResult(reference.Value.configs));

        var hasPermission = Utils.YourBotUtil.CheckSimpleGroupPermission(configs.memeConfig, groupUin);
        if (!hasPermission) {
            return [];
        }

        var command = data.Command;
        if (command != "meme") {
            return [];
        }

        var parameters = data.Parameters;
        
        if (parameters.Count == 0) {
            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(YourBotUtil.BuildSendToGroupMessageData(
                        data.GroupUin, configs.memeConfig.Priority, "wrong parameters"))
            ];
        }

        var parameter = parameters[0];
        if (parameter == "save") {
            return await SaveMeme(data, sharedData, configs);
        }

        if (parameter == "get") {
            return await GetMeme(data, sharedData, configs);
        }

        return [
            CanSendGroupMessageAttribute.GetInstance()
                .GetCertificate(YourBotUtil.BuildSendToGroupMessageData(
                    data.GroupUin, configs.memeConfig.Priority, "wrong parameters"))
        ];
    }
    
    private static async Task<List<Certificate>> SaveMeme(GroupCommandData data,
        AsyncSharedDataWrapper<(BotContext botContext, (MemeConfig memeConfig, DatabaseConfig databaseConfig)
            configs)> sharedData, (MemeConfig memeConfig, DatabaseConfig databaseConfig) configs) {
        var messageChain = data.MessageChain;

        if (messageChain[0] is not ForwardEntity forwardEntity) {
            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(YourBotUtil.BuildSendToGroupMessageData(
                        data.GroupUin, configs.memeConfig.Priority, "wrong parameters type"))
            ];
        }

        var targetMessageChain = (await sharedData.ExecuteAsync(reference => reference.Value.botContext.GetGroupMessage(
            data.GroupUin,
            forwardEntity.Sequence, forwardEntity.Sequence)))![0];

        try {
            var savedMessageChainId = await Utils.YourBotUtil.SaveMessageChain(targetMessageChain,
                configs.databaseConfig.ConnectionString, configs.memeConfig.ImageDir);
            
            await using var connection = new MySqlConnection( configs.databaseConfig.ConnectionString);
            connection.Open();
            await using var cmd = new MySqlCommand("INSERT INTO meme (message_chain_id) VALUES (@messageChainId)", connection);
            cmd.Parameters.AddWithValue("@messageChainId", savedMessageChainId);
            await cmd.ExecuteNonQueryAsync();

            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(YourBotUtil.BuildSendToGroupMessageData(
                        data.GroupUin, configs.memeConfig.Priority, "save successfully"))
            ];
        } catch (Exception e) {
            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(YourBotUtil.BuildSendToGroupMessageData(
                        data.GroupUin, configs.memeConfig.Priority, $"Error: {e.Message}"))
            ];
        }

    }
    
    private static async Task<List<Certificate>> GetMeme(GroupCommandData data, 
        AsyncSharedDataWrapper<(BotContext botContext, (MemeConfig memeConfig, DatabaseConfig databaseConfig)
            configs)> sharedData, (MemeConfig memeConfig, DatabaseConfig databaseConfig) configs) {
        try {
            await using var conn = new MySqlConnection(configs.databaseConfig.ConnectionString);
            await conn.OpenAsync();

            // Get random meme
            uint messageChainId;
            await using (var cmd = new MySqlCommand("SELECT message_chain_id FROM meme ORDER BY RAND() LIMIT 1", conn)) {
                var result = await cmd.ExecuteScalarAsync();
                if (result == null) {
                    return [
                        CanSendGroupMessageAttribute.GetInstance()
                            .GetCertificate(
                                YourBotUtil.BuildSendToGroupMessageData(data.GroupUin, configs.memeConfig.Priority, "No memes found"))
                    ];
                }

                messageChainId = Convert.ToUInt32(result);
            }
            var resultMessageChain = await Utils.YourBotUtil.GetMessageChain(messageChainId, configs.databaseConfig.ConnectionString, data.MessageChain.GroupUin);

            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(
                        new SendToGroupMessageData(
                            new Priority(configs.memeConfig.Priority, PriorityStrategy.Share),
                            resultMessageChain
                        )
                    )
            ];
        } catch (Exception ex) {
            return [
                CanSendGroupMessageAttribute.GetInstance()
                    .GetCertificate(
                        YourBotUtil.BuildSendToGroupMessageData(data.GroupUin, configs.memeConfig.Priority, $"Error: {ex.Message}")
                    )
            ];
        }
    }
}