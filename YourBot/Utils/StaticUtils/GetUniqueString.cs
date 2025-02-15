using System.Text;

namespace YourBot.Utils;

public static partial class YourBotUtil {
    private static readonly Lock TimeStringLock = new();
    private static int _counter;

    public static string GetUniqueTimeString() {
        lock (TimeStringLock) {
            var currentTimestamp = DateTime.Now;
            var timeString = currentTimestamp.ToString("yyyyMMddHHmmss");

            timeString += '_';
            timeString += (++_counter).ToString("D10");
            if (timeString.Length > 128) {
                throw new Exception("Time string is too long");
            }

            return timeString;
        }
    }
}