using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using YourBot.Fuwafuwa.Application.Attribute.Executor.Friend;
using YourBot.Fuwafuwa.Application.Data.ExecutorData.Friend;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Executor.Friend;

public class SendMessage2FriendExecutor : IExecutorCore<SendMessage2FriendData, AsyncSharedDataWrapper<BotContext>, BotContext> {
    public static IServiceAttribute<SendMessage2FriendData> GetServiceAttribute() {
        return CanSendMessage2FriendAttribute.GetInstance();
    }
    public static AsyncSharedDataWrapper<BotContext> Init(BotContext initData) {
        return new AsyncSharedDataWrapper<BotContext>(initData);
    }
    public static void Final(AsyncSharedDataWrapper<BotContext> sharedData, Logger2Event? logger) { }
    public Task ExecuteTask(SendMessage2FriendData data, AsyncSharedDataWrapper<BotContext> sharedData, Logger2Event? logger) {
        var messageChain = data.MessageChain;
        return sharedData.ExecuteAsync(async botContext => {
            await botContext.Value.SendMessage(messageChain);
        });
    }
}