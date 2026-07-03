using System.Security.Cryptography;
using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

public static class OfflineGeoworldWorldSourceGraphHash
{
    public static string Compute(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
