using System.Security.Cryptography;
using System.Text;

namespace YourBot.Utils;

public static partial class YourBotUtil {
    public static int GetIntHash(int input) {
        var inputString = input.ToString();

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(inputString));

        var hashValue = BitConverter.ToInt32(hashBytes, 0);

        return hashValue;
    }
    
    private static readonly string key = "1234567890123456"; // 16 字节密钥
    private static readonly string iv = "6543210987654321";  // 16 字节 IV

    // 静态方法：通过 AES 加密生成哈希（加密的映射）
    public static string Encrypt(int input)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input.ToString());

            byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            return Convert.ToBase64String(encrypted);
        }
    }

    // 静态方法：通过 Base64 解密反向获取原始值
    public static int Decrypt(string encrypted)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            byte[] encryptedBytes = Convert.FromBase64String(encrypted);

            byte[] decrypted = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return int.Parse(Encoding.UTF8.GetString(decrypted));
        }
    }
}


