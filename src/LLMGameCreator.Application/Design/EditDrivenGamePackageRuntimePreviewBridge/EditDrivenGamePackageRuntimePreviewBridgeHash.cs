using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;

internal static class EditDrivenGamePackageRuntimePreviewBridgeJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options) + Environment.NewLine;

    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options);
}

internal static class EditDrivenGamePackageRuntimePreviewBridgeHash
{
    public static string Sha256Text(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        return Sha256Bytes(bytes);
    }

    public static string Sha256Bytes(byte[] bytes)
    {
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string Sha256File(string path) =>
        Sha256Bytes(File.ReadAllBytes(path));
}
