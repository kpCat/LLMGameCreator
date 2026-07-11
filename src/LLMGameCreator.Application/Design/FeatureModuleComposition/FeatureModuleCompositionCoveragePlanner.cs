using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public static class FeatureModuleCompositionIdentity
{
    private static readonly HashSet<string> TrailingQualifierTokens =
        new(["focus", "resource"], StringComparer.Ordinal);

    public static string CompositionId(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedModuleIds) =>
        "minimal-map-game-composed-" + ShortName(catalog, selectedModuleIds);

    public static string DisplayName(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedModuleIds)
    {
        if (selectedModuleIds.Count == 0) return "Baseline-Only FeatureModule Composition";
        var byId = catalog.Modules.ToDictionary(module => module.ModuleId, StringComparer.Ordinal);
        return string.Join(" + ", selectedModuleIds
            .OrderBy(id => id, StringComparer.Ordinal)
            .Select(id => TrimFocusSuffix(byId[id].Title))) + " FeatureModule Composition";
    }

    public static string ShortName(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedModuleIds)
    {
        if (selectedModuleIds.Count == 0) return "baseline";
        var slugs = selectedModuleIds.OrderBy(id => id, StringComparer.Ordinal)
            .Select(ModuleSlug)
            .ToList();
        if (slugs.Any(string.IsNullOrWhiteSpace) || slugs.Distinct(StringComparer.Ordinal).Count() != slugs.Count)
            throw new InvalidOperationException("FeatureModule composition slugs must be non-empty and unique.");
        return string.Join("-", slugs);
    }

    private static string ModuleSlug(string moduleId)
    {
        var terminal = moduleId[(moduleId.LastIndexOf('.') + 1)..];
        var slug = Regex.Replace(terminal.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        var tokens = slug.Split('-', StringSplitOptions.RemoveEmptyEntries).ToList();
        while (tokens.Count > 1 && TrailingQualifierTokens.Contains(tokens[^1])) tokens.RemoveAt(tokens.Count - 1);
        return string.Join("-", tokens);
    }

    private static string TrimFocusSuffix(string title) =>
        title.EndsWith(" Focus", StringComparison.Ordinal)
            ? title[..^" Focus".Length]
            : title;
}

public sealed class FeatureModuleCompositionCoveragePlanner
{
    private readonly FeatureModuleCompositionValidator _validator;

    public FeatureModuleCompositionCoveragePlanner(FeatureModuleCompositionValidator? validator = null)
    {
        _validator = validator ?? new FeatureModuleCompositionValidator();
    }

    public FeatureModuleCompositionCoveragePlan Plan(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedModuleIds,
        FeatureModuleCompositionCoveragePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        policy ??= new FeatureModuleCompositionCoveragePolicy();
        ValidatePolicy(policy);
        var optional = catalog.Modules.Where(module => module.Selectable && !module.Required)
            .OrderBy(module => module.ModuleId, StringComparer.Ordinal).ToList();
        var optionalIds = optional.Select(module => module.ModuleId).ToList();
        var selected = selectedModuleIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
        ValidateSelection(catalog, selected);

        return optional.Count <= policy.ExhaustiveOptionalModuleLimit
            ? Exhaustive(catalog, optionalIds, selected, policy)
            : Bounded(catalog, optional, selected, policy);
    }

    private FeatureModuleCompositionCoveragePlan Exhaustive(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> optionalIds,
        IReadOnlyList<string> selected,
        FeatureModuleCompositionCoveragePolicy policy)
    {
        if (optionalIds.Count >= 31)
            throw new InvalidOperationException("Exhaustive FeatureModule coverage is limited to fewer than 31 optional modules.");
        var count = 1 << optionalIds.Count;
        if (count > policy.MaxTotalRows)
            throw new InvalidOperationException("FeatureModule exhaustive coverage exceeds maxTotalRows.");
        var specs = new List<FeatureModuleCompositionCoverageSpec>(count);
        for (var mask = 0; mask < count; mask++)
        {
            var modules = new List<string>();
            for (var index = 0; index < optionalIds.Count; index++)
                if ((mask & (1 << index)) != 0) modules.Add(optionalIds[index]);
            specs.Add(Spec(catalog, modules, ["exhaustive_powerset"]));
        }

        return BuildPlan(
            FeatureModuleCompositionCoverageModes.ExhaustiveSmallCatalog,
            optionalIds.Count,
            selected,
            optionalIds,
            specs,
            policy,
            fullPowersetEnumerated: true,
            bounded: false);
    }

    private FeatureModuleCompositionCoveragePlan Bounded(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<FeatureModuleDefinition> optional,
        IReadOnlyList<string> selected,
        FeatureModuleCompositionCoveragePolicy policy)
    {
        var specs = new List<FeatureModuleCompositionCoverageSpec>();
        var byKey = new Dictionary<string, int>(StringComparer.Ordinal);

        Add(catalog, specs, byKey, [], "baseline", policy.MaxTotalRows);
        Add(catalog, specs, byKey, selected, "operator_selected", policy.MaxTotalRows);
        var all = optional.Select(module => module.ModuleId).ToList();
        if (IsCompatible(catalog, all)) Add(catalog, specs, byKey, all, "all_enabled", policy.MaxTotalRows);
        var compatibleSingletons = optional.Where(module => IsCompatible(catalog, [module.ModuleId])).ToList();
        foreach (var module in compatibleSingletons)
            Add(catalog, specs, byKey, [module.ModuleId], "singleton", policy.MaxTotalRows);

        if (specs.Count(spec => spec.CoverageReasons.Contains("singleton", StringComparer.Ordinal))
            != compatibleSingletons.Count)
            throw new InvalidOperationException("FeatureModule singleton certification exceeds maxTotalRows.");

        var pairRows = 0;
        foreach (var pair in CompatiblePairs(catalog, optional))
        {
            if (pairRows >= policy.MaxPairwiseRows || specs.Count >= policy.MaxTotalRows) break;
            if (Add(catalog, specs, byKey, pair, "pairwise", policy.MaxTotalRows)) pairRows++;
        }

        foreach (var group in SharedInteractionGroups(optional))
        {
            if (specs.Count >= policy.MaxTotalRows) break;
            Add(catalog, specs, byKey, group, "declared_interaction", policy.MaxTotalRows);
        }

        var sampledRows = 0;
        var attempts = Math.Max(policy.MaxSampledRows * 8, 8);
        for (var sample = 0; sample < attempts && sampledRows < policy.MaxSampledRows && specs.Count < policy.MaxTotalRows; sample++)
        {
            var modules = optional.Where(module => SampleIncludes(policy.DeterministicSeed, sample, module.ModuleId))
                .Select(module => module.ModuleId).ToList();
            if (modules.Count < 2 || !IsCompatible(catalog, modules)) continue;
            if (Add(catalog, specs, byKey, modules, "deterministic_sample", policy.MaxTotalRows)) sampledRows++;
        }

        if (!Contains(specs, selected))
            throw new InvalidOperationException("FeatureModule selected composition was dropped by bounded coverage.");
        return BuildPlan(
            FeatureModuleCompositionCoverageModes.BoundedInteractionCoverage,
            optional.Count,
            selected,
            all,
            specs,
            policy,
            fullPowersetEnumerated: false,
            bounded: true);
    }

    private FeatureModuleCompositionCoveragePlan BuildPlan(
        string mode,
        int optionalCount,
        IReadOnlyList<string> selected,
        IReadOnlyList<string> all,
        IReadOnlyList<FeatureModuleCompositionCoverageSpec> specs,
        FeatureModuleCompositionCoveragePolicy policy,
        bool fullPowersetEnumerated,
        bool bounded) => new()
    {
        CoverageMode = mode,
        OptionalModuleCount = optionalCount,
        TheoreticalPowersetSize = TheoreticalPowersetSize(optionalCount),
        GeneratedCompositionCount = specs.Count,
        FullPowersetEnumerated = fullPowersetEnumerated,
        BaselineIncluded = Contains(specs, []),
        SelectedCompositionIncluded = Contains(specs, selected),
        AllEnabledIncluded = Contains(specs, all),
        SingletonCoverageCount = specs.Count(spec => spec.ModuleIds.Count == 1),
        PairwiseCoverageCount = specs.Count(spec => spec.CoverageReasons.Contains("pairwise", StringComparer.Ordinal)),
        SampledCoverageCount = specs.Count(spec => spec.CoverageReasons.Contains("deterministic_sample", StringComparer.Ordinal)),
        Bounded = bounded,
        Policy = policy,
        CompositionSpecs = specs
    };

    private void ValidateSelection(FeatureModuleCatalogDocument catalog, IReadOnlyList<string> selected)
    {
        var required = catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId);
        var validation = _validator.Validate(catalog, required.Concat(selected).ToList());
        if (!validation.Passed)
            throw new InvalidOperationException("FeatureModule coverage selected composition is invalid: " + string.Join("; ", validation.Diagnostics));
    }

    private bool IsCompatible(FeatureModuleCatalogDocument catalog, IReadOnlyList<string> modules)
    {
        var required = catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId);
        return _validator.Validate(catalog, required.Concat(modules).ToList()).Passed;
    }

    private static IEnumerable<IReadOnlyList<string>> CompatiblePairs(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<FeatureModuleDefinition> optional)
    {
        var validator = new FeatureModuleCompositionValidator();
        var required = catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId).ToList();
        for (var left = 0; left < optional.Count; left++)
        for (var right = left + 1; right < optional.Count; right++)
        {
            var pair = new[] { optional[left].ModuleId, optional[right].ModuleId };
            if (validator.Validate(catalog, required.Concat(pair).ToList()).Passed) yield return pair;
        }
    }

    private static IEnumerable<IReadOnlyList<string>> SharedInteractionGroups(IReadOnlyList<FeatureModuleDefinition> optional) =>
        optional.SelectMany(module => module.MutationOperations.Select(operation => (module.ModuleId, operation.RuntimeDimension)))
            .Where(item => !string.IsNullOrWhiteSpace(item.RuntimeDimension))
            .GroupBy(item => item.RuntimeDimension, StringComparer.Ordinal)
            .Select(group => (IReadOnlyList<string>)group.Select(item => item.ModuleId).Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal).ToList())
            .Where(group => group.Count >= 2);

    private static bool Add(
        FeatureModuleCatalogDocument catalog,
        List<FeatureModuleCompositionCoverageSpec> specs,
        Dictionary<string, int> byKey,
        IReadOnlyList<string> modules,
        string reason,
        int maxRows)
    {
        var ordered = modules.OrderBy(id => id, StringComparer.Ordinal).ToList();
        var key = string.Join("\u001f", ordered);
        if (byKey.TryGetValue(key, out var existing))
        {
            var reasons = specs[existing].CoverageReasons.Append(reason).Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToList();
            specs[existing] = specs[existing] with { CoverageReasons = reasons };
            return false;
        }
        if (specs.Count >= maxRows) return false;
        byKey[key] = specs.Count;
        specs.Add(Spec(catalog, ordered, [reason]));
        return true;
    }

    private static FeatureModuleCompositionCoverageSpec Spec(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> modules,
        IReadOnlyList<string> reasons) => new()
    {
        CompositionId = FeatureModuleCompositionIdentity.CompositionId(catalog, modules),
        DisplayName = FeatureModuleCompositionIdentity.DisplayName(catalog, modules),
        ModuleIds = modules.OrderBy(id => id, StringComparer.Ordinal).ToList(),
        CoverageReasons = reasons
    };

    private static bool Contains(
        IReadOnlyList<FeatureModuleCompositionCoverageSpec> specs,
        IReadOnlyList<string> modules) => specs.Any(spec => SameModules(spec.ModuleIds, modules));

    private static bool SameModules(IReadOnlyList<string> left, IReadOnlyList<string> right) =>
        left.OrderBy(id => id, StringComparer.Ordinal)
            .SequenceEqual(right.OrderBy(id => id, StringComparer.Ordinal), StringComparer.Ordinal);

    private static bool SampleIncludes(int seed, int sample, string moduleId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed.ToString(CultureInfo.InvariantCulture) + ":" + sample + ":" + moduleId));
        return (hash[0] & 1) == 1;
    }

    private static string TheoreticalPowersetSize(int count) =>
        count < 63
            ? (1UL << count).ToString(CultureInfo.InvariantCulture)
            : "2^" + count.ToString(CultureInfo.InvariantCulture);

    private static void ValidatePolicy(FeatureModuleCompositionCoveragePolicy policy)
    {
        if (policy.ExhaustiveOptionalModuleLimit < 0 || policy.MaxPairwiseRows < 0
            || policy.MaxSampledRows < 0 || policy.MaxTotalRows < 1)
            throw new ArgumentOutOfRangeException(nameof(policy), "FeatureModule coverage policy values must be non-negative and maxTotalRows must be positive.");
    }
}
