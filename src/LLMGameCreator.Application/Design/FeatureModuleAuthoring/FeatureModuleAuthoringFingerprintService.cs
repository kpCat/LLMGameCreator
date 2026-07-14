using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

/// <summary>Computes a stable identity for the semantic authoring inputs of one selected composition.</summary>
public sealed class FeatureModuleAuthoringFingerprintService
{
    public FeatureModuleAuthoringFingerprintResult Calculate(
        FeatureModuleCompositionDocument document,
        FeatureModuleLibrarySnapshot library)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(library);

        var diagnostics = new List<string>();
        var catalogModules = library.Catalog.Modules.ToDictionary(module => module.ModuleId, StringComparer.Ordinal);
        var explicitSelected = document.SelectedModuleIds.Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal).ToList();
        foreach (var moduleId in explicitSelected.Where(id => !catalogModules.ContainsKey(id)))
            diagnostics.Add("authoring.fingerprint.unknown_selected_module:" + moduleId);

        var selected = catalogModules.Values.Where(module => module.Required).Select(module => module.ModuleId)
            .Concat(explicitSelected).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToList();
        foreach (var moduleId in selected.Where(id => !library.ModuleFingerprints.TryGetValue(id, out var value) || string.IsNullOrWhiteSpace(value)))
            diagnostics.Add("authoring.fingerprint.missing_module_fingerprint:" + moduleId);

        var validation = new FeatureModuleParameterValidator().Validate(library.Catalog, selected, document.ParameterValues);
        diagnostics.AddRange(validation.Diagnostics.Select(value => "authoring.fingerprint." + value));
        foreach (var value in validation.EffectiveValues)
        {
            if (value.ValueType is not FeatureModuleParameterValueTypes.Integer
                and not FeatureModuleParameterValueTypes.Number
                and not FeatureModuleParameterValueTypes.Boolean
                and not FeatureModuleParameterValueTypes.Enum)
                diagnostics.Add("authoring.fingerprint.unsupported_parameter_value_type:" + value.ModuleId + "|" + value.ParameterId);
        }
        if (diagnostics.Count > 0)
            return new FeatureModuleAuthoringFingerprintResult { Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).ToList() };

        var canonical = new CanonicalAuthoring
        {
            BaseCandidateId = document.BaseCandidateId,
            SelectedModuleIds = explicitSelected,
            ModuleFingerprints = selected.Select(id => new CanonicalModuleFingerprint { ModuleId = id, Fingerprint = library.ModuleFingerprints[id] }).ToList(),
            Parameters = validation.EffectiveValues.OrderBy(value => value.ModuleId, StringComparer.Ordinal)
                .ThenBy(value => value.ParameterId, StringComparer.Ordinal).Select(value => new CanonicalParameter
                {
                    ModuleId = value.ModuleId,
                    ParameterId = value.ParameterId,
                    ValueType = value.ValueType,
                    Value = FeatureModuleParameterValidator.CanonicalValue(value.Value, value.ValueType)
                }).ToList()
        };
        var canonicalJson = JsonSerializer.Serialize(canonical);
        return new FeatureModuleAuthoringFingerprintResult
        {
            Passed = true,
            Sha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalJson))).ToLowerInvariant(),
            CanonicalAuthoringJson = canonicalJson
        };
    }

    private sealed record CanonicalAuthoring
    {
        public string BaseCandidateId { get; init; } = string.Empty;
        public IReadOnlyList<string> SelectedModuleIds { get; init; } = [];
        public IReadOnlyList<CanonicalModuleFingerprint> ModuleFingerprints { get; init; } = [];
        public IReadOnlyList<CanonicalParameter> Parameters { get; init; } = [];
    }
    private sealed record CanonicalModuleFingerprint { public string ModuleId { get; init; } = string.Empty; public string Fingerprint { get; init; } = string.Empty; }
    private sealed record CanonicalParameter { public string ModuleId { get; init; } = string.Empty; public string ParameterId { get; init; } = string.Empty; public string ValueType { get; init; } = string.Empty; public string Value { get; init; } = string.Empty; }
}

public sealed record FeatureModuleAuthoringFingerprintResult
{
    public bool Passed { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public string CanonicalAuthoringJson { get; init; } = string.Empty;
}
