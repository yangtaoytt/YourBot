using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Lagrange.Core;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Utils;
using YourBot.Utils.Command;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group.Command;

public class GroupCommandProcessor : IProcessorCore<MessageData, NullSharedDataWrapper<(string, uint)>, BotContext> {
    public static IServiceAttribute<MessageData> GetServiceAttribute() {
        return ReadGroupMessageAttribute.GetInstance();
    }

    public static NullSharedDataWrapper<(string, uint)> Init(BotContext initData) {
        return new NullSharedDataWrapper<(string, uint)>((initData.BotName!, initData.BotUin));
    }

    public static void Final(NullSharedDataWrapper<(string, uint)> sharedData, Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(MessageData data, NullSharedDataWrapper<(string, uint)> sharedData,
        Logger2Event? logger) {
        await Task.CompletedTask;

        var messageChain = data.MessageChain;

        var (botName, botUin) = sharedData.Execute(reference => reference.Value);
        if (!CheckAt(messageChain, botName, botUin)) {
            return [];
        }

        var commandMessageIndex = YourBotUtil.CheckCommand(messageChain);
        if (commandMessageIndex == -1) {
            return [];
        }

        var textEntity = (messageChain[commandMessageIndex] as TextEntity)!;
        var index = textEntity.Text.IndexOf('/');
        var wordsOld = textEntity.Text[(index + 1)..].Split(' ');
        var wordsList = wordsOld.Select(item => item.Trim()).ToList();
        if (wordsList.Count == 0) {
            return [];
        }
        var commandHandler = new CommandHandler(wordsList);

        var command = wordsOld[0];
        var commandData = new GroupCommandData(command, messageChain, wordsOld.Length > 1 ? wordsOld[1..].ToList() : [],
            messageChain.GroupUin!.Value, commandHandler);

        return [
            ReadGroupCommandAttribute.GetInstance().GetCertificate(commandData)
        ];
    }

    private static bool CheckAt(MessageChain messageChain, string name, uint botUin) {
        foreach (var message in messageChain) {
            if (message is MentionEntity mentionEntity && mentionEntity.Uin == botUin) {
                return true;
            }

            if (message is TextEntity textEntity && textEntity.Text.Contains($"@{name}")) {
                return true;
            }
        }

        return false;
    }
}