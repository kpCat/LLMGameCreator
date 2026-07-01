namespace LLMGameCreator.Application.Design.GeneratorSpineQualityConsolidation;

public static class GeneratorSpineQualityVocabulary
{
    public const string GoalId = "goal_072_generator_spine_quality_consolidation";
    public const string ProductSmokeRoute = "goal-072-generator-spine-quality-consolidation";
    public const string FinalGate = "generator_spine_quality_consolidation_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-072-generator-spine-quality-consolidation";
    public const string TechnicalDebtMarkdownPath = "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md";

    public static readonly IReadOnlyList<string> RequiredEvidenceFiles =
    [
        "quality-inventory-summary.json",
        "source-format-risk-report.json",
        "large-file-and-method-risk-report.json",
        "unity-alpha-bootstrap-risk-report.json",
        "proof-quality-risk-report.json",
        "artifact-reproducibility-risk-report.json",
        "safe-fix-summary.json",
        "technical-debt-register.json",
        "quality-dashboard.json",
        "generator-spine-quality-consolidation-report.md"
    ];

    public static readonly IReadOnlyList<string> SeamRoleNames =
    [
        "SourceLoader",
        "EvidenceService",
        "Hash",
        "Validator",
        "UnityProofRunner",
        "Projector",
        "Builder"
    ];

    public static readonly IReadOnlyList<string> RecentArtifactRoots =
    [
        ".llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix",
        ".llmgc/procedural/goal-061-full-campaign-playable-review-package-rc",
        ".llmgc/procedural/goal-062-constrained-spatial-detail-generation",
        ".llmgc/procedural/goal-063-gameplay-consequence-depth-matrix",
        ".llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix",
        ".llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix",
        ".llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix",
        ".llmgc/procedural/goal-067-programmatic-narrative-quest-dialogue-event-matrix",
        ".llmgc/procedural/goal-068-combat-magic-ability-boss-encounter-matrix",
        ".llmgc/procedural/goal-069-world-event-weather-daynight-crisis-matrix",
        ".llmgc/procedural/goal-070-integrated-campaign-timeline-simulation-matrix",
        ".llmgc/procedural/goal-071-unity-alpha-interactive-campaign-player",
        ".llmgc/procedural/goal-072-generator-spine-quality-consolidation"
    ];
}

public sealed record GeneratorSpineQualityScanOptions
{
    public string ProjectRootPath { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceRoots { get; init; } =
    [
        "src/LLMGameCreator.Application/Design",
        "tests/LLMGameCreator.Tests/Application",
        "tests/LLMGameCreator.Tests/ProductSmoke",
        "unity/LLMGameCreatorAlpha/Assets/Scripts"
    ];
    public IReadOnlyList<string> ProductSmokeRoots { get; init; } = ["tests/LLMGameCreator.Tests/ProductSmoke"];
    public IReadOnlyList<string> ArtifactRoots { get; init; } = GeneratorSpineQualityVocabulary.RecentArtifactRoots;
}

public sealed record SourceFileQualityRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int MaxLineLength { get; init; }
    public int DeclarationTokenCount { get; init; }
    public int SemicolonCount { get; init; }
    public bool IsOneLineOrMinifiedCandidate { get; init; }
    public bool IsLargeFileCandidate { get; init; }
    public bool HasExtremeLineLength { get; init; }
}

public sealed record MethodSizeRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public string MethodName { get; init; } = string.Empty;
    public int StartLine { get; init; }
    public int LineCount { get; init; }
}

public sealed record SeamRoleFolderRecord
{
    public string FolderRelativePath { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public int FileCount { get; init; }
    public IReadOnlyList<string> Files { get; init; } = [];
}

public sealed record UnityAlphaBootstrapRiskRecord
{
    public string RelativePath { get; init; } = "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public int LineCount { get; init; }
    public int MarkerRouteCount { get; init; }
    public IReadOnlyList<string> MarkerRoutes { get; init; } = [];
    public int PrivateNestedTypeCount { get; init; }
    public bool MonolithicGrowthRisk { get; init; }
}

public sealed record ProductSmokeQualityRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public int AssertCount { get; init; }
    public int StrongAssertionSignalCount { get; init; }
    public bool ReportOnlyShallowCandidate { get; init; }
    public IReadOnlyList<string> StrongSignals { get; init; } = [];
}

public sealed record ArtifactVolatilityRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineNumber { get; init; }
    public string MatchKind { get; init; } = string.Empty;
}

public sealed record CurrentStateConsistencyRecord
{
    public bool JsonParses { get; init; }
    public bool GateStatusMatchesGoal072 { get; init; }
    public bool ActiveManualGateMentionsGoal072Required { get; init; }
    public bool MarkdownMentionsGoal071Handoff { get; init; }
    public bool MarkdownMentionsGoal072Required { get; init; }
    public bool ContextIndexMentionsGoal072Required { get; init; }
    public bool GoalQueueMentionsGoal072Required { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record Goal071ProofQualityRecord
{
    public bool ReportExists { get; init; }
    public bool CommandPlanExists { get; init; }
    public bool StagedCommandPlanExists { get; init; }
    public bool PlayerProofExists { get; init; }
    public bool TransitionLedgerExists { get; init; }
    public bool InputScriptExists { get; init; }
    public bool CommandPlanPassed { get; init; }
    public bool CommandPlanAcceptedFalse { get; init; }
    public int CommandPlanRowCount { get; init; }
    public int ExpectedMarkerCount { get; init; }
    public bool PlayerProofPassed { get; init; }
    public bool PlayerExecuted { get; init; }
    public int ProvenRowCount { get; init; }
    public int MissingMarkerCount { get; init; }
    public int MatchedMarkerCount { get; init; }
    public int TransitionCount { get; init; }
    public int ActionCount { get; init; }
    public bool ProofQualityPassed { get; init; }
}

public sealed record QualityInventorySummary
{
    public string SchemaVersion { get; init; } = "generator_spine_quality_inventory_v1";
    public string GoalId { get; init; } = GeneratorSpineQualityVocabulary.GoalId;
    public string ManualGate { get; init; } = GeneratorSpineQualityVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public int SourceFileCount { get; init; }
    public int ArtifactFileCount { get; init; }
    public int ProductSmokeFileCount { get; init; }
    public int MinifiedCandidateCount { get; init; }
    public int LargeFileCandidateCount { get; init; }
    public int LargeMethodCandidateCount { get; init; }
    public int AbsolutePathLikeArtifactCount { get; init; }
    public int TimestampLikeArtifactCount { get; init; }
    public int ShallowProductSmokeCandidateCount { get; init; }
    public int SeamRoleFolderCount { get; init; }
    public int UnityBootstrapLineCount { get; init; }
    public int UnityBootstrapMarkerRouteCount { get; init; }
}

public sealed record SourceFormatRiskReport
{
    public string SchemaVersion { get; init; } = "source_format_risk_report_v1";
    public IReadOnlyList<SourceFileQualityRecord> MinifiedCandidates { get; init; } = [];
    public IReadOnlyList<SourceFileQualityRecord> ExtremeLineLengthCandidates { get; init; } = [];
    public IReadOnlyList<SourceFileQualityRecord> TopMaxLineLengthFiles { get; init; } = [];
}

public sealed record LargeFileAndMethodRiskReport
{
    public string SchemaVersion { get; init; } = "large_file_and_method_risk_report_v1";
    public IReadOnlyList<SourceFileQualityRecord> LargeFileCandidates { get; init; } = [];
    public IReadOnlyList<MethodSizeRecord> LargeMethodCandidates { get; init; } = [];
    public IReadOnlyList<SeamRoleFolderRecord> RepeatedSeamRolesByFolder { get; init; } = [];
}

public sealed record ProofQualityRiskReport
{
    public string SchemaVersion { get; init; } = "proof_quality_risk_report_v1";
    public IReadOnlyList<ProductSmokeQualityRecord> ProductSmokeRecords { get; init; } = [];
    public IReadOnlyList<ProductSmokeQualityRecord> ShallowProductSmokeCandidates { get; init; } = [];
    public Goal071ProofQualityRecord Goal071ProofIndicators { get; init; } = new();
}

public sealed record ArtifactReproducibilityRiskReport
{
    public string SchemaVersion { get; init; } = "artifact_reproducibility_risk_report_v1";
    public IReadOnlyList<ArtifactVolatilityRecord> AbsolutePathLikeStrings { get; init; } = [];
    public IReadOnlyList<ArtifactVolatilityRecord> TimestampLikeValues { get; init; } = [];
}

public sealed record GeneratorSpineQualityScanResult
{
    public IReadOnlyList<SourceFileQualityRecord> SourceFiles { get; init; } = [];
    public IReadOnlyList<MethodSizeRecord> LargeMethods { get; init; } = [];
    public IReadOnlyList<SeamRoleFolderRecord> RepeatedSeamRoles { get; init; } = [];
    public UnityAlphaBootstrapRiskRecord UnityAlphaBootstrap { get; init; } = new();
    public IReadOnlyList<ProductSmokeQualityRecord> ProductSmokeRecords { get; init; } = [];
    public IReadOnlyList<ArtifactVolatilityRecord> AbsolutePathLikeArtifacts { get; init; } = [];
    public IReadOnlyList<ArtifactVolatilityRecord> TimestampLikeArtifacts { get; init; } = [];
    public CurrentStateConsistencyRecord CurrentStateConsistency { get; init; } = new();
    public Goal071ProofQualityRecord Goal071ProofIndicators { get; init; } = new();
    public int ArtifactFileCount { get; init; }
}

public sealed record GeneratorSpineQualityFinding
{
    public string FindingId { get; init; } = string.Empty;
    public string Severity { get; init; } = "P3";
    public string Area { get; init; } = string.Empty;
    public string Evidence { get; init; } = string.Empty;
    public string RecommendedFutureGoal { get; init; } = string.Empty;
    public bool FixedInGoal072 { get; init; }
    public string WhyNotFixed { get; init; } = string.Empty;
}

public sealed record SafeFixSummary
{
    public string SchemaVersion { get; init; } = "safe_fix_summary_v1";
    public IReadOnlyList<string> FixedItems { get; init; } = [];
    public IReadOnlyList<string> DeferredItems { get; init; } = [];
    public IReadOnlyList<string> ForbiddenScopeNotTouched { get; init; } = [];
}

public sealed record TechnicalDebtRegister
{
    public string SchemaVersion { get; init; } = "generator_spine_quality_debt_register_v1";
    public IReadOnlyList<GeneratorSpineQualityFinding> Findings { get; init; } = [];
}

public sealed record QualityDashboard
{
    public string SchemaVersion { get; init; } = "generator_spine_quality_dashboard_v1";
    public string Status { get; init; } = "BLOCKED";
    public int P0Count { get; init; }
    public int P1Count { get; init; }
    public int P2Count { get; init; }
    public int P3Count { get; init; }
    public IReadOnlyList<string> RecommendedNextActions { get; init; } = [];
    public string InventoryHash { get; init; } = string.Empty;
    public string DebtRegisterHash { get; init; } = string.Empty;
}

public sealed record GeneratorSpineQualityBuildResult
{
    public QualityInventorySummary Inventory { get; init; } = new();
    public SourceFormatRiskReport SourceFormatRiskReport { get; init; } = new();
    public LargeFileAndMethodRiskReport LargeFileAndMethodRiskReport { get; init; } = new();
    public UnityAlphaBootstrapRiskRecord UnityAlphaBootstrapRiskReport { get; init; } = new();
    public ProofQualityRiskReport ProofQualityRiskReport { get; init; } = new();
    public ArtifactReproducibilityRiskReport ArtifactReproducibilityRiskReport { get; init; } = new();
    public SafeFixSummary SafeFixSummary { get; init; } = new();
    public TechnicalDebtRegister TechnicalDebtRegister { get; init; } = new();
    public QualityDashboard QualityDashboard { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public string DebtRegisterMarkdown { get; init; } = string.Empty;
}

public sealed record GeneratorSpineQualityWriteResult
{
    public GeneratorSpineQualityBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string DebtRegisterMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
