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
    RevokeGroupMessageExecutor : IExecutorCore<RevokeGroupMessageData, AsyncSharedDataWrapper<BotContext>, BotContext> {
    public static IServiceAttribute<RevokeGroupMessageData> GetServiceAttribute() {
        return CanRevokeGroupMessageAttribute.GetInstance();
    }

    public static AsyncSharedDataWrapper<BotContext> Init(BotContext initData) {
        return new AsyncSharedDataWrapper<BotContext>(initData);
    }

    public static void Final(AsyncSharedDataWrapper<BotContext> sharedData, Logger2Event? logger) { }

    public async Task ExecuteTask(RevokeGroupMessageData data, AsyncSharedDataWrapper<BotContext> sharedData,
        Logger2Event? logger) {
        await sharedData.ExecuteAsync(async botContext => {
            await botContext.Value.RecallGroupMessage(data.GroupUin, data.MessageSeq);
        });
    }
}