using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;

internal static class EditDrivenUnityAlphaStreamingAssetsHandoffJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options) + Environment.NewLine;

    public static T? Deserialize<T>(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, Options);
}

internal static class EditDrivenUnityAlphaStreamingAssetsHandoffHash
{
    public static string Sha256Text(string text) =>
        Sha256Bytes(Encoding.UTF8.GetBytes(text));

    public static string Sha256Bytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string Sha256File(string path) =>
        Sha256Bytes(File.ReadAllBytes(path));

    public static string HashJson<T>(T value) =>
        Sha256Text(EditDrivenUnityAlphaStreamingAssetsHandoffJson.Serialize(value));
}
