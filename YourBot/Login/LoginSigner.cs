using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Common;
using Lagrange.Core.Utility.Sign;

namespace YourBot.Login;

public class LoginSigner : SignProvider {
    private readonly HttpClient _client;

    private readonly string _platform;

    private readonly string _signProxyUrl;
    private readonly string _signServerUrl;

    private readonly string _version;

    public LoginSigner(string signServerUrl, string signProxyUrl, string platform, string version) {
        _signServerUrl = signServerUrl;
        _signProxyUrl = signProxyUrl;

        try {
            _client = new HttpClient(new HttpClientHandler {
                Proxy = new WebProxy {
                    Address = new Uri(_signProxyUrl),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false
                }
            }, true);
        } catch (UriFormatException e) {
            throw new Exception("Signer proxy url format error", e);
        }

        _platform = platform;
        _version = version;
    }


    public override byte[]? Sign(string cmd, uint seq, byte[] body, [UnscopedRef] out byte[]? ver,
        [UnscopedRef] out string? token) {
        if (!WhiteListCommand.Contains(cmd)) {
            ver = null;
            token = null;
            return null;
        }

        using HttpRequestMessage request = new();
        try {
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_signServerUrl);
            request.Content = JsonContent.Create(new JsonObject {
                { "cmd", cmd },
                { "seq", seq },
                { "src", Convert.ToHexString(body) }
            });
        } catch (UriFormatException e) {
            throw new Exception("Signer server url format error", e);
        }

        using var message = _client.Send(request);
        if (message.StatusCode != HttpStatusCode.OK) {
            throw new Exception($"Signer server returned a {message.StatusCode}");
        }

        var json = JsonDocument.Parse(message.Content.ReadAsStream()).RootElement;

        if (!json.TryGetProperty("platform", out var platformJson)) {
            throw new Exception("Signer platform miss");
        }

        if (!json.TryGetProperty("version", out var versionJson)) {
            throw new Exception("Signer version miss");
        }

        if (platformJson.GetString() != _platform || versionJson.GetString() != _version) {
            throw new Exception("Signer platform or version mismatch");
        }


        if (!json.TryGetProperty("value", out var valueJson)) {
            throw new Exception("Signer value miss");
        }

        var extraJson = valueJson.GetProperty("extra");
        var tokenJson = valueJson.GetProperty("token");
        var signJson = valueJson.GetProperty("sign");

        ver = extraJson.GetString() != null ? Convert.FromHexString(extraJson.GetString()!) : [];
        token = tokenJson.GetString() != null
            ? Encoding.UTF8.GetString(Convert.FromHexString(tokenJson.GetString()!))
            : "";

        var sign = signJson.GetString() ?? throw new Exception("Signer server returned an empty sign");
        return Convert.FromHexString(sign);
    }

    public static async Task<BotAppInfo> CreateAppInfoAsync(string signServerUrl, string signProxyUrl) {
        HttpClient client;
        try {
            client = new HttpClient(new HttpClientHandler {
                Proxy = new WebProxy {
                    Address = new Uri(signProxyUrl),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false
                }
            }, true);
        } catch (UriFormatException e) {
            throw new Exception("Signer proxy url format error", e);
        }

        return await client.GetFromJsonAsync<BotAppInfo>($"{signServerUrl}/appinfo") ??
               throw new InvalidOperationException("Signer server created an empty app info");
    }
}