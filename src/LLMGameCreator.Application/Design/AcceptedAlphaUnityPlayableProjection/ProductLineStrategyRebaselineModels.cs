namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class ProductLineStrategyRebaselineVocabulary
{
    public const string GoalId =
        "goal_133a_product_line_strategy_rebaseline_and_canonical_runtime_pivot";
    public const string ScenarioId =
        "goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot";
    public const string Gate = "product_line_strategy_rebaseline_verification";
    public const string Goal134Gate =
        "canonical_runtime_selected_candidate_playthrough_matrix_verification";
    public const string Goal135Gate =
        "canonical_runtime_playable_player_loop_readiness_verification";
    public const string Goal136Gate =
        "canonical_runtime_player_command_loop_execution_matrix_verification";
    public const string Goal137Gate =
        "canonical_runtime_unity_player_loop_playback_harness_verification";
    public const string Goal138Gate =
        "runtime_backed_unity_player_loop_stepper_hud_harness_verification";
    public const string Goal139Gate =
        "runtime_backed_unity_player_loop_interactive_controls_harness_verification";
    public const string Goal140Gate =
        "runtime_backed_unity_player_loop_controls_ux_polish_and_noise_guard_verification";
    public const string Goal141Gate =
        "runtime_backed_unity_player_command_roundtrip_bridge_verification";
    public const string Goal142Gate =
        "runtime_significant_product_line_variant_matrix_and_selection_handoff_verification";
    public const string Goal143Gate =
        "selected_runtime_variant_end_to_end_playeradapter_handoff_verification";
    public const string Goal144Gate =
        "selected_runtime_variant_interactive_action_session_and_save_replay_verification";
    public const string NextGoal =
        "goal_134_canonical_runtime_selected_candidate_playthrough_matrix";
    public const string PostGoal134NextGoal =
        "goal_135_canonical_runtime_playable_player_loop_readiness";
    public const string PostGoal135NextGoal =
        "goal_136_canonical_runtime_player_command_loop_execution_matrix";
    public const string PostGoal136NextGoal =
        "goal_137_canonical_runtime_unity_player_loop_playback_harness";

    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot";
    public const string DocumentationPath =
        "docs/manual-acceptance/product-line-strategy-rebaseline-and-canonical-runtime-pivot.md";

    public const string DashboardFileName =
        "product-line-strategy-rebaseline-dashboard.json";
    public const string DocScanFileName =
        "product-line-strategy-rebaseline-doc-scan.json";
    public const string NegativeProofFileName =
        "product-line-strategy-rebaseline-negative-proof.json";
    public const string ReportFileName =
        "product-line-strategy-rebaseline-report.md";
    public const string FileIndexFileName =
        "product-line-strategy-rebaseline-file-index.json";

    public const string DashboardRelativePath =
        ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string DocScanRelativePath =
        ProceduralOutputDirectory + "/" + DocScanFileName;
    public const string NegativeProofRelativePath =
        ProceduralOutputDirectory + "/" + NegativeProofFileName;
    public const string ReportRelativePath =
        ProceduralOutputDirectory + "/" + ReportFileName;
    public const string FileIndexRelativePath =
        ProceduralOutputDirectory + "/" + FileIndexFileName;

    public static readonly IReadOnlyList<string> StrategyDocs =
    [
        "docs/PRODUCT_LINE_CORE_STRATEGY.md",
        "docs/NARROW_ALPHA_EXPANSION_POLICY.md",
        "docs/AUTOMATED_VALIDATION_TIERS.md"
    ];

    public static readonly IReadOnlyList<string> RequiredSeams =
    [
        "FeatureModule",
        "RuntimePrimitive",
        "SemanticPack",
        "VisualPartPack",
        "WorldSourceAdapter",
        "PlayerAdapter"
    ];
}

public sealed record ProductLineStrategyRebaselineBuildResult
{
    public ProductLineStrategyRebaselineDashboard Dashboard { get; init; } = new();
    public ProductLineStrategyRebaselineDocScan DocScan { get; init; } = new();
    public ProductLineStrategyRebaselineNegativeProof NegativeProof { get; init; } = new();
    public ProductLineStrategyRebaselineFileIndex ProceduralFileIndex { get; init; } = new();
    public ProductLineStrategyRebaselineFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record ProductLineStrategyRebaselineWriteResult
{
    public ProductLineStrategyRebaselineBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record ProductLineStrategyRebaselineDashboard
{
    public string GoalId { get; init; } = ProductLineStrategyRebaselineVocabulary.GoalId;
    public string Gate { get; init; } = ProductLineStrategyRebaselineVocabulary.Gate;
    public bool Accepted { get; init; }
    public string ImplementationStatus { get; init; } = "BLOCKED";
    public bool ProductLineCombiner { get; init; }
    public bool NotPromptToGame { get; init; }
    public bool LlmOptionalAuthoringOnly { get; init; }
    public bool NewDocsPresent { get; init; }
    public bool AgentsRoutingUpdated { get; init; }
    public bool ContextIndexRoutingUpdated { get; init; }
    public bool CurrentStateUpdated { get; init; }
    public bool QueueUpdated { get; init; }
    public string NextGoal { get; init; } = ProductLineStrategyRebaselineVocabulary.NextGoal;
    public bool ManualUnityOptional { get; init; } = true;
    public bool ProjectionOnlyStopCondition { get; init; } = true;
    public bool RuntimeUnchanged { get; init; }
    public bool UnityUnchanged { get; init; }
    public bool SchemaUnchanged { get; init; }
    public bool SamplePackageUnchanged { get; init; }
    public bool ManualInputUnchanged { get; init; }
    public string EvidencePath { get; init; } =
        ProductLineStrategyRebaselineVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record ProductLineStrategyRebaselineDocScan
{
    public string GoalId { get; init; } = ProductLineStrategyRebaselineVocabulary.GoalId;
    public bool ReadmeProductIdentityPresent { get; init; }
    public bool ReadmeLlmRuntimeBoundaryPresent { get; init; }
    public bool ReadmeCanonicalTruthPresent { get; init; }
    public bool ProductLineStrategyDocPresent { get; init; }
    public bool NarrowAlphaPolicyDocPresent { get; init; }
    public bool AutomatedValidationTiersDocPresent { get; init; }
    public bool RequiredSeamsPresent { get; init; }
    public bool RequiredPolicyStatementsPresent { get; init; }
    public bool AgentsRoutingUpdated { get; init; }
    public bool ContextIndexRoutingUpdated { get; init; }
    public bool CurrentStateUpdated { get; init; }
    public bool QueueUpdated { get; init; }
    public bool MilestoneGateUpdated { get; init; }
    public bool RiskRegisterUpdated { get; init; }
    public bool TechnicalDebtUpdated { get; init; }
    public bool OldGoal133Rerouted { get; init; }
    public bool Goal131EvidencePresent { get; init; }
    public bool Goal132EvidencePresent { get; init; }
    public bool ArtifactScopeScenarioPresent { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> MissingMarkers { get; init; } = [];
}

public sealed record ProductLineStrategyRebaselineNegativeProof
{
    public string GoalId { get; init; } = ProductLineStrategyRebaselineVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool RuntimeUnchanged { get; init; }
    public bool RuntimeAbstractionsUnchanged { get; init; }
    public bool UnityUnchanged { get; init; }
    public bool SchemaUnchanged { get; init; }
    public bool GamePackageProjectUnchanged { get; init; }
    public bool GenerationUnchanged { get; init; }
    public bool AssetPipelineUnchanged { get; init; }
    public bool ScriptingUnchanged { get; init; }
    public bool SamplePackageUnchanged { get; init; }
    public bool ManualInputUnchanged { get; init; }
    public bool ProviderMediaLuaGeneratorLibraryUnchanged { get; init; }
    public int PlannedWriteCount { get; init; }
    public IReadOnlyList<string> ForbiddenPrefixes { get; init; } = [];
    public IReadOnlyList<string> PlannedWrites { get; init; } = [];
    public IReadOnlyList<string> Violations { get; init; } = [];
}

public sealed record ProductLineStrategyRebaselineFileIndex
{
    public string GoalId { get; init; } = ProductLineStrategyRebaselineVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<ProductLineStrategyRebaselineFileIndexEntry> Files { get; init; } = [];
}

public sealed record ProductLineStrategyRebaselineFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
