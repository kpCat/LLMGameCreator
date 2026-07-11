using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;

namespace LLMGameCreator.Application.Design.FeatureModuleCertification;

public sealed class FeatureModuleCertificationCache
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private readonly string _root;

    public FeatureModuleCertificationCache(string root)
    {
        if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("certification cache root is required", nameof(root));
        _root = Path.GetFullPath(root);
    }

    public FeatureModuleCertificationCacheReadState TryRead(
        FeatureModuleCertificationPlanItem plan,
        out FeatureModuleCertificationEntry? entry)
    {
        entry = null;
        var path = PathFor(plan.ModuleId);
        if (!File.Exists(path)) return FeatureModuleCertificationCacheReadState.Missing;
        try
        {
            var envelope = JsonSerializer.Deserialize<FeatureModuleCertificationCacheEnvelope>(
                               File.ReadAllText(path, Encoding.UTF8), JsonOptions)
                           ?? throw new JsonException("empty cache envelope");
            var actualHash = FeatureModuleLibraryFingerprintService.Hash(JsonSerializer.Serialize(envelope.Entry, JsonOptions));
            if (!string.Equals(actualHash, envelope.EntrySha256, StringComparison.Ordinal)
                || !string.Equals(envelope.Entry.ModuleId, plan.ModuleId, StringComparison.Ordinal))
                return FeatureModuleCertificationCacheReadState.Corrupt;
            if (!string.Equals(envelope.CacheKey, plan.CacheKey, StringComparison.Ordinal)
                || !string.Equals(envelope.Entry.ModuleFingerprint, plan.ModuleFingerprint, StringComparison.Ordinal)
                || !string.Equals(envelope.Entry.DependencyFingerprint, plan.DependencyFingerprint, StringComparison.Ordinal)
                || !string.Equals(envelope.Entry.BasePackageSha256, plan.BasePackageSha256, StringComparison.Ordinal)
                || !string.Equals(envelope.Entry.RuntimeQualifierContractVersion, plan.RuntimeQualifierContractVersion, StringComparison.Ordinal)
                || !string.Equals(envelope.Entry.ActionPlanSignature, plan.ActionPlanSignature, StringComparison.Ordinal)
                || !string.Equals(envelope.Entry.ParameterDefaultsFingerprint, plan.ParameterDefaultsFingerprint, StringComparison.Ordinal))
                return FeatureModuleCertificationCacheReadState.Invalidated;
            if (envelope.Entry.Status != "GREEN") return FeatureModuleCertificationCacheReadState.Invalidated;
            entry = envelope.Entry;
            return FeatureModuleCertificationCacheReadState.Reused;
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            return FeatureModuleCertificationCacheReadState.Corrupt;
        }
    }

    public void Write(FeatureModuleCertificationPlanItem plan, FeatureModuleCertificationEntry entry)
    {
        var entryJson = JsonSerializer.Serialize(entry, JsonOptions);
        var envelope = new FeatureModuleCertificationCacheEnvelope
        {
            CacheKey = plan.CacheKey,
            EntrySha256 = FeatureModuleLibraryFingerprintService.Hash(entryJson),
            Entry = entry
        };
        WriteAtomic(PathFor(plan.ModuleId), JsonSerializer.Serialize(envelope, JsonOptions) + "\n");
    }

    public string PathForModule(string moduleId) => PathFor(moduleId);

    private string PathFor(string moduleId)
    {
        var name = FeatureModuleLibraryFingerprintService.Hash(moduleId) + ".certification.json";
        var path = Path.GetFullPath(Path.Combine(_root, name));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var root = _root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("certification cache path escape rejected");
        return path;
    }

    private static void WriteAtomic(string path, string value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, value, new UTF8Encoding(false));
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
