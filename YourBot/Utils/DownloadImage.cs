namespace YourBot.Utils;

public static partial class Util {
    public static byte[] DownloadImage(string url) {
        return client.GetByteArrayAsync(url).Result;
    }
}