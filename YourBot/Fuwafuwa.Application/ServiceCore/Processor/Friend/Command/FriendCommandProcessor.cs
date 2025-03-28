using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Lagrange.Core.Message.Entity;
using YourBot.Fuwafuwa.Application.Attribute.Processor.Friend;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;
using YourBot.Utils;
using YourBot.Utils.Command;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Friend.Command;

public class FriendCommandProcessor : IProcessorCore<MessageData,NullSharedDataWrapper<object>,object> {
    public static IServiceAttribute<MessageData> GetServiceAttribute() {
        return ReadFriendMessageAttribute.GetInstance();
    }
    public static NullSharedDataWrapper<object> Init(object initData) {
        return new NullSharedDataWrapper<object>(initData);
    }
    public static void Final(NullSharedDataWrapper<object> sharedData, Logger2Event? logger) { }
    
    public async Task<List<Certificate>> ProcessData(MessageData data, NullSharedDataWrapper<object> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;
        
        var messageChain = data.MessageChain;
        
        var commandMessageIndex = YourBotUtil.CheckCommand(messageChain);
        if (commandMessageIndex == -1) {
            return [];
        }
        var textEntity = (messageChain[commandMessageIndex] as TextEntity)!;
        var index = textEntity.Text.IndexOf('/');
        var words = textEntity.Text[(index + 1)..].Split(' ').Select(item => item.Trim()).ToList();
        if (words.Count == 0) {
            return [];
        }
        var commandHandler = new CommandHandler(words);
        var commandData = new FriendCommandData(commandHandler, messageChain, messageChain.FriendUin);
        return [ReadFriendCommandAttribute.GetInstance().GetCertificate(commandData)];
    }
}