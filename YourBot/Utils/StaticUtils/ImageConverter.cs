using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace YourBot.Utils;

public static partial class YourBotUtil {
    // 静态 HttpClient 实例，避免每次创建
    private static readonly HttpClient client = new();

    public static async Task<Stream> SaveImageAndConvertToJpegStream(string url) {
        var imageBytes = await client.GetByteArrayAsync(url);

        using var ms = new MemoryStream(imageBytes);
        using var image = await Image.LoadAsync(ms);

        // 检查图像是否已经是 Jpeg 格式
        if (image.Metadata?.DecodedImageFormat == JpegFormat.Instance) {
            // 创建一个新的 MemoryStream 来复制数据
            var copyStream = new MemoryStream(ms.ToArray());
            copyStream.Seek(0, SeekOrigin.Begin); // 重置流的位置为0
            return copyStream;
        }

        // 将图像保存为 Jpeg 格式到新的 MemoryStream
        var jpegStream = new MemoryStream();
        await image.SaveAsync(jpegStream, new JpegEncoder());
        jpegStream.Seek(0, SeekOrigin.Begin); // 重置流的位置为0

        return jpegStream;
    }
}