using System.Text.RegularExpressions;
using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level1;
using Fuwafuwa.Core.ServiceCore.Level3;
using Lagrange.Core.Message.Entity;
using YourBot.Config.Implement.Level1.Service.Friend;
using YourBot.Fuwafuwa.Application.Attribute.Processor.Friend;
using YourBot.Fuwafuwa.Application.Data;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;
using YourBot.Fuwafuwa.Application.Data.ProcessorData.Friend;
using YourBot.Utils;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Processor.Friend;

public class SubmitCollectProcessor : IProcessorCore<CombinedData,
    SimpleSharedDataWrapper<(Dictionary<uint, (List<FileEntity> submit, List<Regex> regexes, uint homeworkId)> dic, SubmitCollectConfig config)>, SubmitCollectConfig> {
    public static IServiceAttribute<CombinedData> GetServiceAttribute() {
        return ReadCombineData.GetInstance();
    }

    public static SimpleSharedDataWrapper<(Dictionary<uint, (List<FileEntity> submit, List<Regex> regexes, uint homeworkId)> dic, SubmitCollectConfig config)> Init(SubmitCollectConfig initData) {
        return new SimpleSharedDataWrapper<(
            Dictionary<uint, (List<FileEntity> submit, List<Regex> regexes, uint homeworkId)> dic, SubmitCollectConfig
            config)>((new Dictionary<uint, (List<FileEntity> submit, List<Regex> regexes, uint homeworkId)>(),initData));
    }
    public static void Final(SimpleSharedDataWrapper<(Dictionary<uint, (List<FileEntity> submit, List<Regex> regexes, uint homeworkId)> dic, SubmitCollectConfig config)> sharedData, Logger2Event? logger) { }
    public async Task<List<Certificate>> ProcessData(CombinedData data, SimpleSharedDataWrapper<(Dictionary<uint, (List<FileEntity> submit, List<Regex> regexes, uint homeworkId)> dic, SubmitCollectConfig config)> sharedData, Logger2Event? logger) {
        await Task.CompletedTask;
        if (data.AddSubmitMissionData != null) {
            var addSubmitMissionData = data.AddSubmitMissionData!;
            
            sharedData.Execute(reference => {
                reference.Value.dic[addSubmitMissionData.FriendUin] = (new List<FileEntity>(),
                    addSubmitMissionData.Regexes, addSubmitMissionData.HomeworkId);
            });
            return [];


        } else {
            var messageChain = data.FriendMessageData!.MessageChain;
            var friendUin = messageChain.FriendUin;

            var result = sharedData.Execute<List<Certificate>>(reference => {
                var (dic, config) = reference.Value;
                if (!dic.ContainsKey(friendUin)) {
                    return [];
                }

                if (messageChain.Count != 1 || messageChain[0] is not FileEntity fileEntity) {
                    dic.Remove(friendUin);
                    return [
                        YourBotUtil.SendToFriendMessage(friendUin, config.Priority,
                            "The submit is interpreted.please retry and send only one file at a time.")
                    ];
                }

                var (submit, regexes, homeworkId) = dic[friendUin];
                
                submit.Add((FileEntity)messageChain[0]);

                if (submit.Count == regexes.Count) {
                    dic.Remove(friendUin);
                    return [
                        ReadSubmitDataAttribute.GetInstance()
                            .GetCertificate(new SubmitData(
                                homeworkId, friendUin, submit, regexes.Select(regex => (Regex)regex).ToList()))
                    ];
                }

                return [];
            });
            return result;
        }
        
        
        
        

    }
}