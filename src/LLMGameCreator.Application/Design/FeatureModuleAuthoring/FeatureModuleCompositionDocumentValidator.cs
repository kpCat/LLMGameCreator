using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleCompositionDocumentValidator
{
    private static readonly Regex IdPattern = new("^[a-z0-9][a-z0-9._-]{0,127}$", RegexOptions.CultureInvariant);
    private readonly FeatureModuleCompositionValidator _compositionValidator;
    private readonly FeatureModuleParameterValidator _parameterValidator;

    public FeatureModuleCompositionDocumentValidator(
        FeatureModuleCompositionValidator? compositionValidator = null,
        FeatureModuleParameterValidator? parameterValidator = null)
    {
        _compositionValidator = compositionValidator ?? new FeatureModuleCompositionValidator();
        _parameterValidator = parameterValidator ?? new FeatureModuleParameterValidator();
    }

    public FeatureModuleCompositionDocumentValidation Validate(
        FeatureModuleCompositionDocument document,
        FeatureModuleLibrarySnapshot library)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(library);
        var diagnostics = new List<string>();
        var schema = document.SchemaVersion == FeatureModuleCompositionDocumentVocabulary.SchemaVersion;
        if (!schema) diagnostics.Add("unsupported composition schema rejected: " + document.SchemaVersion);
        var idValid = IsValidCompositionId(document.CompositionId);
        if (!idValid) diagnostics.Add("invalid composition ID rejected: " + document.CompositionId);
        if (document.BaseCandidateId != FeatureModuleCompositionVocabulary.BaselineCandidateId)
            diagnostics.Add("unsupported base candidate rejected: " + document.BaseCandidateId);
        if (document.SelectedModuleIds.Distinct(StringComparer.Ordinal).Count() != document.SelectedModuleIds.Count)
            diagnostics.Add("duplicate selected module rejected");
        var required = library.Catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId);
        var composition = _compositionValidator.Validate(
            library.Catalog,
            required.Concat(document.SelectedModuleIds).ToList());
        diagnostics.AddRange(composition.Diagnostics);
        var parameters = _parameterValidator.Validate(library.Catalog, document.SelectedModuleIds, document.ParameterValues);
        diagnostics.AddRange(parameters.Diagnostics);
        return new FeatureModuleCompositionDocumentValidation
        {
            Passed = diagnostics.Count == 0,
            SchemaVersionSupported = schema,
            CompositionIdValid = idValid,
            SelectedModulesResolved = composition.AllModuleIdsExist && composition.DependenciesSatisfied
                                      && composition.ConflictsAbsent,
            ParameterValuesValid = parameters.Passed,
            Diagnostics = diagnostics
        };
    }

    public static bool IsValidCompositionId(string value) =>
        !string.IsNullOrWhiteSpace(value) && IdPattern.IsMatch(value);
}

public sealed class FeatureModuleCompositionStalenessService
{
    public FeatureModuleCompositionStaleness Evaluate(
        FeatureModuleCompositionDocument document,
        FeatureModuleLibrarySnapshot library)
    {
        var missing = document.SelectedModuleIds.Where(id => !library.ModuleFingerprints.ContainsKey(id))
            .OrderBy(id => id, StringComparer.Ordinal).ToList();
        var changed = document.SelectedModuleIds.Where(id => library.ModuleFingerprints.TryGetValue(id, out var current)
                && (!document.ModuleFingerprints.TryGetValue(id, out var saved)
                    || !string.Equals(current, saved, StringComparison.Ordinal)))
            .OrderBy(id => id, StringComparer.Ordinal).ToList();
        var catalogChanged = !string.Equals(document.CatalogFingerprint, library.CatalogFingerprint, StringComparison.Ordinal);
        var diagnostics = missing.Select(id => "missing selected module unresolved: " + id)
            .Concat(changed.Select(id => "selected module fingerprint changed: " + id))
            .ToList();
        if (catalogChanged) diagnostics.Add("catalog fingerprint changed");
        var unresolved = missing.Count > 0;
        var stale = unresolved || catalogChanged || changed.Count > 0;
        return new FeatureModuleCompositionStaleness
        {
            Status = unresolved ? "UNRESOLVED" : stale ? "STALE" : "CURRENT",
            Stale = stale,
            Unresolved = unresolved,
            CatalogFingerprintChanged = catalogChanged,
            ChangedModuleIds = changed,
            MissingModuleIds = missing,
            Diagnostics = diagnostics
        };
    }
}
