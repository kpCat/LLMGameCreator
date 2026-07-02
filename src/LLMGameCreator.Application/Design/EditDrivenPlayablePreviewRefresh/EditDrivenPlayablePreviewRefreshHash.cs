using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;

public static class EditDrivenPlayablePreviewRefreshHash
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options);

    public static string Sha256(string text) =>
        Sha256(Encoding.UTF8.GetBytes(text));

    public static string Sha256(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string SafeSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value.ToLowerInvariant())
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                builder.Append(ch);
            }
            else if (ch is '/' or '\\' or '_' or '-' or '.')
            {
                builder.Append('-');
            }
        }

        var result = builder.ToString().Trim('-');
        while (result.Contains("--", StringComparison.Ordinal))
        {
            result = result.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(result) ? "id" : result;
    }
}
