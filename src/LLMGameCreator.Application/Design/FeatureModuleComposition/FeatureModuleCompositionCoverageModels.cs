namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public static class FeatureModuleCompositionCoverageModes
{
    public const string ExhaustiveSmallCatalog = "exhaustive_small_catalog";
    public const string BoundedInteractionCoverage = "bounded_interaction_coverage";
}

public sealed record FeatureModuleCompositionCoveragePolicy
{
    public int ExhaustiveOptionalModuleLimit { get; init; } = 3;
    public int MaxPairwiseRows { get; init; } = 4;
    public int MaxSampledRows { get; init; } = 2;
    public int MaxTotalRows { get; init; } = 24;
    public int DeterministicSeed { get; init; } = 146;
}

public sealed record FeatureModuleCompositionCoverageSpec
{
    public string CompositionId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyList<string> ModuleIds { get; init; } = [];
    public IReadOnlyList<string> CoverageReasons { get; init; } = [];
}

public sealed record FeatureModuleCompositionCoveragePlan
{
    public string SchemaVersion { get; init; } = "featuremodule_composition_coverage_plan_v1";
    public string CoverageMode { get; init; } = string.Empty;
    public int OptionalModuleCount { get; init; }
    public string TheoreticalPowersetSize { get; init; } = string.Empty;
    public int GeneratedCompositionCount { get; init; }
    public bool FullPowersetEnumerated { get; init; }
    public bool BaselineIncluded { get; init; }
    public bool SelectedCompositionIncluded { get; init; }
    public bool AllEnabledIncluded { get; init; }
    public int SingletonCoverageCount { get; init; }
    public int PairwiseCoverageCount { get; init; }
    public int SampledCoverageCount { get; init; }
    public bool Bounded { get; init; }
    public FeatureModuleCompositionCoveragePolicy Policy { get; init; } = new();
    public IReadOnlyList<FeatureModuleCompositionCoverageSpec> CompositionSpecs { get; init; } = [];
}
