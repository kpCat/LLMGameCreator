using System.Security.Cryptography;
using System.Text;

namespace LLMGameCreator.Application.Design.GeoworldSourceAdapterStreamingContract;

public static class GeoworldContractHash
{
    public static string Compute(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
