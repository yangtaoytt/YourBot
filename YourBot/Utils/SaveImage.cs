namespace YourBot.Utils;

public static partial class YourBotUtil {
    public static async Task SaveImage(string url, string path) {
        var imageBytes = await client.GetByteArrayAsync(url);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, imageBytes);
    }
}