using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Executor;

public class
    SendGroupMessageExecutor : IExecutorCore<SendToGroupMessageData, AsyncSharedDataWrapper<BotContext>, BotContext> {
    public static IServiceAttribute<SendToGroupMessageData> GetServiceAttribute() {
        return CanSendGroupMessageAttribute.GetInstance();
    }

    public static AsyncSharedDataWrapper<BotContext> Init(BotContext initData) {
        return new AsyncSharedDataWrapper<BotContext>(initData);
    }

    public static void Final(AsyncSharedDataWrapper<BotContext> sharedData, Logger2Event? logger) { }

    public async Task ExecuteTask(SendToGroupMessageData data, AsyncSharedDataWrapper<BotContext> sharedData,
        Logger2Event? logger) {
        var messageChain = data.MessageChain;
        await sharedData.ExecuteAsync(async botContext => { await botContext.Value.SendMessage(messageChain); });
    }
}