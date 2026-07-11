using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleLibraryFingerprintService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string ModuleFingerprint(FeatureModuleDefinition module) =>
        Hash(JsonSerializer.Serialize(Normalize(module), JsonOptions));

    public string CatalogFingerprint(IReadOnlyDictionary<string, string> moduleFingerprints)
    {
        var canonical = string.Join("\n", moduleFingerprints.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => pair.Key + ":" + pair.Value)) + "\n";
        return Hash(canonical);
    }

    public string ParameterDefaultsFingerprint(FeatureModuleDefinition module)
    {
        var values = module.ParameterDefinitions.OrderBy(item => item.ParameterId, StringComparer.Ordinal)
            .Select(item => item.ParameterId + ":" + item.DefaultValue.GetRawText());
        return Hash(string.Join("\n", values) + "\n");
    }

    public static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    public static FeatureModuleDefinition Normalize(FeatureModuleDefinition module) => module with
    {
        Dependencies = Sort(module.Dependencies),
        Conflicts = Sort(module.Conflicts),
        RequiredSchemaSections = Sort(module.RequiredSchemaSections),
        RequiredRuntimePrimitives = Sort(module.RequiredRuntimePrimitives),
        RequiredValidationRules = Sort(module.RequiredValidationRules),
        RequiredSaveLoadPolicy = Sort(module.RequiredSaveLoadPolicy),
        RequiredPlayerAdapterSurface = Sort(module.RequiredPlayerAdapterSurface),
        GeneratorInputs = Sort(module.GeneratorInputs),
        AuthoringControls = Sort(module.AuthoringControls),
        GoldenPackages = Sort(module.GoldenPackages),
        SmokePlaythroughs = Sort(module.SmokePlaythroughs),
        KnownLimitations = Sort(module.KnownLimitations),
        FutureExpansionNotes = Sort(module.FutureExpansionNotes),
        MutationOperations = module.MutationOperations.OrderBy(item => item.OperationId, StringComparer.Ordinal).ToList(),
        RuntimeEffectContracts = module.RuntimeEffectContracts.OrderBy(item => item.EffectId, StringComparer.Ordinal)
            .Select(item => item with { SourceOperationIds = Sort(item.SourceOperationIds) }).ToList(),
        ParameterDefinitions = module.ParameterDefinitions.OrderBy(item => item.ParameterId, StringComparer.Ordinal)
            .Select(item => item with
            {
                AllowedValues = Sort(item.AllowedValues),
                Bindings = item.Bindings.OrderBy(binding => binding.OperationId, StringComparer.Ordinal).ToList(),
                ValidationRules = Sort(item.ValidationRules),
                RuntimeEffectIds = Sort(item.RuntimeEffectIds)
            }).ToList(),
        SourceLineage = module.SourceLineage with { OperationIds = Sort(module.SourceLineage.OperationIds) }
    };

    private static IReadOnlyList<string> Sort(IEnumerable<string> values) =>
        values.OrderBy(value => value, StringComparer.Ordinal).ToList();
}
