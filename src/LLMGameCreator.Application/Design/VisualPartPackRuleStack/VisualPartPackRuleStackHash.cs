using System.Security.Cryptography;
using System.Text;

namespace LLMGameCreator.Application.Design.VisualPartPackRuleStack;

public static class VisualPartPackRuleStackHash
{
    public static string Compute(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    public static string StableHash(string value) => Compute(value);
}
