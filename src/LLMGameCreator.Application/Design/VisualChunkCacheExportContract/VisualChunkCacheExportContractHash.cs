using System.Security.Cryptography;
using System.Text;

namespace LLMGameCreator.Application.Design.VisualChunkCacheExportContract;

public static class VisualChunkCacheExportContractHash
{
    public static string Compute(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
