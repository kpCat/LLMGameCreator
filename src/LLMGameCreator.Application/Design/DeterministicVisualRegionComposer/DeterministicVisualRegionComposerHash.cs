using System.Security.Cryptography;
using System.Text;

namespace LLMGameCreator.Application.Design.DeterministicVisualRegionComposer;

public static class DeterministicVisualRegionComposerHash
{
    public static string Compute(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
