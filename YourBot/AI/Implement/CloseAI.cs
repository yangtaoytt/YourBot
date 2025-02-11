using System.Diagnostics;
using System.Text.Json;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using OpenAI.Chat;
using YourBot.AI.Interface;
using YourBot.Utils;

namespace YourBot.AI.Implement;

// ReSharper disable InconsistentNaming
public class CloseAI : IAI {
    // ReSharper restore InconsistentNaming

    private static readonly ChatCompletionOptions options = new() {
        ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            "Image-description",
            BinaryData.FromBytes("""
                                 {
                                   "type": "object",
                                   "properties": {
                                     "isFriendly": {
                                       "type": "boolean",
                                       "description": "是否友善。True表示友善，False表示不友善。"
                                     },
                                     "suggestion": {
                                       "type": "string",
                                       "description": "关于如何改进或变得更加友善的建议。"
                                     }
                                   },
                                   "required": ["isFriendly", "suggestion"],
                                   "additionalProperties": false
                                 }

                                 """u8.ToArray()), jsonSchemaIsStrict: true
        ),
        Temperature = 0.5f,
        MaxOutputTokenCount = 50
    };

    private readonly ChatClient _client = new("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

    private readonly object _lock = new();


    public async Task<JudgeFriendlyResult> JudgeFriendly(List<IMessageEntity> messageEntities) {
        try {
            List<ChatMessageContentPart> chatMessageContentParts = new();
            foreach (var messageEntity in messageEntities) {
                chatMessageContentParts.Add(await CreateChatMessageContentPartFromMessageEntity(messageEntity));
            }

            List<ChatMessage> messages = new() {
                new SystemChatMessage(
                    "你是一个检测群聊消息的智能ai, 请检测以下消息是否友善，传达的思想尽量健康向上。" +
                    "eg：‘哎呀，这不是杨老师吗？你有何贵干？’就是一条使用了反讽的不友善的消息。" +
                    "eg: 大量的重复消息，也属于不友善的信息" +
                    "eg: 让人感觉到不安的图片" +
                    "eg： 图片中有头顶带汗珠的人物，也就是流汗的图片表情，图片中的人物虽然在笑，但脸上的汗珠可能让人感觉到某种程度的不安或尴尬。" +
                    "这种笑中带有尴尬的表现可能让人感觉是隐含的讽刺或不舒服，尤其是在群聊中使用时，可能会给人一种“表面欢笑，实则不满”的感觉。" +
                    "eg: 图片中人物类似这个😅，边笑边流汗，也不合适" +
                    "eg: 😓，疑问，等讽刺性emoji" +
                    "eg： 孙子等直接侮辱" +
                    "eg: ”那你真厉害“，“你加油”等消极语气或反讽" +
                    "eg： ？等单个表情" +
                    "eg： 我草" +
                    "eg: '乐'‘典’‘绷’‘急了’等单个词语" +
                    "eg: 展示了三个黄色的表情符号，它们互相抱在一起，分别有不同的表情：一个是看似冷漠的直视表情，一个是略带尴尬或不安的表情（带汗珠），另一个是大笑但同样带汗珠的表情。" +
                    "下面的用户输入就是你要检测的内容："),
                new UserChatMessage(
                    chatMessageContentParts.ToArray()
                )
            };
            lock (_lock) {
                ChatCompletion completion = _client.CompleteChat(messages, options);

                using var structuredJson = JsonDocument.Parse(completion.Content[0].Text);
                var isFriendly = structuredJson.RootElement.GetProperty("isFriendly").GetBoolean();
                var suggestion = structuredJson.RootElement.GetProperty("suggestion").GetString();

                return new JudgeFriendlyResult(isFriendly, suggestion ?? "没有建议, 请自行判断");
            }
        } catch (Exception e) {
            return new JudgeFriendlyResult(false, "我下班了，明天再来吧");
        }
    }

    private static async Task<ChatMessageContentPart> CreateChatMessageContentPartFromMessageEntity(
        IMessageEntity messageEntity) {
        Debug.Assert(messageEntity is not MultiMsgEntity or FileEntity or VideoEntity);
        if (messageEntity is ImageEntity imageEntity) {
            var imageStream = await Util.SaveImageAndConvertToJpegStream(imageEntity.ImageUrl);
            var imageBytes = await BinaryData.FromStreamAsync(imageStream);
            return ChatMessageContentPart.CreateImagePart(imageBytes, "image/jpeg");
        }

        return ChatMessageContentPart.CreateTextPart(messageEntity.ToPreviewString());
    }
}