using System.Security.Cryptography;
using System.Text;

namespace LLMGameCreator.Application.Design.DeterministicVisualMapPatchComposer;

public static class DeterministicVisualMapPatchComposerHash
{
    public static string Compute(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    public static int StableInt(string value, int minInclusive, int maxInclusive)
    {
        if (maxInclusive < minInclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maxInclusive), "Maximum must be greater than or equal to minimum.");
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        var raw = BitConverter.ToUInt32(bytes, 0);
        var range = (uint)(maxInclusive - minInclusive + 1);
        return minInclusive + (int)(raw % range);
    }
}
