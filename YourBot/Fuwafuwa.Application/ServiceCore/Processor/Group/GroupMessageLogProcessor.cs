using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Fuwafuwa.Core.Subjects;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface.Api;
using Microsoft.Extensions.Logging;
using YourBot.Config.Implement.Level1.Service.Group;
using YourBot.Fuwafuwa.Application.Attribute.Executor;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ExecutorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Logger;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Group;

public class GroupMessageLogProcessor : IProcessorCore<MessageData,
    AsyncSharedDataWrapper<(AppLogger appLogger, BotContext botContext, GroupMessageLogConfig logInitData,
        Dictionary<uint, (BotGroup botGroup, List<BotGroupMember>? groupMembers)> groupDic)>, (AppLogger appLogger,
    BotContext botContext, GroupMessageLogConfig logInitData)> {
    public static IServiceAttribute<MessageData> GetServiceAttribute() {
        return ReadGroupQMessageAttribute.GetInstance();
    }

    public static
        AsyncSharedDataWrapper<(AppLogger appLogger, BotContext botContext, GroupMessageLogConfig logInitData,
            Dictionary<uint, (BotGroup botGroup, List<BotGroupMember>? groupMembers)> groupDic)> Init(
            (AppLogger appLogger, BotContext botContext, GroupMessageLogConfig logInitData) initData) {
        var task = initData.botContext.FetchGroups();
        task.Wait();
        var groupList = task.Result;
        var groupDic = groupList.ToDictionary(item => item.GroupUin,
            item => (item, (List<BotGroupMember>?)null));
        return new AsyncSharedDataWrapper<(AppLogger appLogger, BotContext botContext, GroupMessageLogConfig
            logInitData, Dictionary<uint, (BotGroup botGroup, List<BotGroupMember>? groupMembers)> groupDic)>(
            (initData.appLogger, initData.botContext, initData.logInitData, groupDic));
    }

    public static void Final(
        AsyncSharedDataWrapper<(AppLogger appLogger, BotContext botContext, GroupMessageLogConfig logInitData,
            Dictionary<uint, (BotGroup botGroup, List<BotGroupMember>? groupMembers)> groupDic)> sharedData,
        Logger2Event? logger) { }

    public async Task<List<Certificate>> ProcessData(MessageData data,
        AsyncSharedDataWrapper<(AppLogger appLogger, BotContext botContext, GroupMessageLogConfig logInitData,
            Dictionary<uint, (BotGroup botGroup, List<BotGroupMember>? groupMembers)> groupDic)> sharedData,
        Logger2Event? logger) {

        var messageChain = data.MessageChain;
        var groupUin = messageChain.GroupUin!.Value;
        
        var config = await sharedData.ExecuteAsync(reference => Task.FromResult(reference.Value.logInitData));
        if (!Utils.Util.CheckSimpleGroupPermission(config, groupUin)) {
            return [];
        }


        var (group, member) = await sharedData.ExecuteAsync(async reference => {
            var value = reference.Value;
            if (value.groupDic[groupUin].groupMembers == null) {
                var botGroup = value.groupDic[groupUin].botGroup;
                value.groupDic[groupUin] = (botGroup, await value.botContext.FetchMembers(groupUin));
            }

            var group = value.groupDic[groupUin].botGroup;
            var member = value.groupDic[groupUin].groupMembers!.First(item => item.Uin == messageChain.FriendUin);
            return (group, member);
        });

        var messageSource = $"Group: {group.GroupName}, User: {member.MemberName}, Content: ";
        var messages = Util.GetSummaryStringInLine(messageChain);

        var logToConsoleData = new LogToConsoleData(new Priority(100, PriorityStrategy.Share),
            messageSource + messages, "GroupMessageLogProcessor", LogLevel.Information);

        return [CanLogToConsoleAttribute.GetInstance().GetCertificate(logToConsoleData)];
    }
}