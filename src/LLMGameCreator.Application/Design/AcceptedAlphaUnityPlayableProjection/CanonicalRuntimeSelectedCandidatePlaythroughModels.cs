using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
{
    public const string GoalId =
        "goal_134_canonical_runtime_selected_candidate_playthrough_matrix";
    public const string ScenarioId =
        "goal-134-canonical-runtime-selected-candidate-playthrough-matrix";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-134-canonical-runtime-selected-candidate-playthrough-matrix";
    public const string DefaultSelectedCandidateHandoffPath =
        ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json";
    public const string DefaultSelectedCandidatePackagePath =
        ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/package.json";
    public const string DocumentationPath =
        "docs/manual-acceptance/canonical-runtime-selected-candidate-playthrough-matrix.md";
    public const string ScriptPath =
        ".devflow/scripts/run-canonical-runtime-selected-candidate-playthrough.ps1";
    public const string CmdPath =
        ".devflow/scripts/run-canonical-runtime-selected-candidate-playthrough.cmd";
    public const string NormalCommand =
        ".devflow\\scripts\\run-canonical-runtime-selected-candidate-playthrough.cmd";
    public const string UnityPassMarker =
        "GOAL134_CANONICAL_RUNTIME_TRANSCRIPT_PLAYER_PASS";
    public const string UnityFailMarker =
        "GOAL134_CANONICAL_RUNTIME_TRANSCRIPT_PLAYER_FAIL";

    public const string PackageValidationFileName =
        "selected-candidate-package-validation.json";
    public const string PlaythroughScriptFileName =
        "canonical-runtime-playthrough-script.json";
    public const string TranscriptFileName =
        "canonical-runtime-transcript.json";
    public const string StateSummaryFileName =
        "canonical-runtime-state-summary.json";
    public const string StateBeforeSaveFileName =
        "canonical-runtime-state-before-save.json";
    public const string StateSaveFileName =
        "canonical-runtime-state-save.json";
    public const string StateAfterLoadFileName =
        "canonical-runtime-state-after-load.json";
    public const string ReplayTranscriptFileName =
        "canonical-runtime-replay-transcript.json";
    public const string SaveLoadReplayResultFileName =
        "canonical-runtime-save-load-replay-result.json";
    public const string MatrixResultFileName =
        "canonical-runtime-selected-candidate-playthrough-matrix-result.json";
    public const string UnitySmokeFileName =
        "unity-player-canonical-transcript-smoke.json";
    public const string ReportJsonFileName =
        "one-click-canonical-runtime-report.json";
    public const string ReportMarkdownFileName =
        "one-click-canonical-runtime-report.md";
    public const string NegativeProofFileName =
        "canonical-runtime-negative-proof.json";
    public const string FileIndexFileName =
        "canonical-runtime-file-index.json";
    public const string DashboardFileName =
        "canonical-runtime-dashboard.json";

    public const string MatrixResultRelativePath =
        ProceduralOutputDirectory + "/" + MatrixResultFileName;
    public const string ReportMarkdownRelativePath =
        ProceduralOutputDirectory + "/" + ReportMarkdownFileName;

    public static IReadOnlyList<string> RequiredAnchors =>
    [
        "map/village",
        "entity/village/sign",
        "interaction/sign_inspect",
        "entity/village/old_guard",
        "dialogue/old_guard_intro",
        "quest/help_healer",
        "inventory/player_start",
        "recipe/healing_potion",
        "node/apple_tree",
        "transaction/buy_healing_potion",
        "encounter/goblin_duel"
    ];
}

public sealed record CanonicalRuntimeSelectedCandidatePlaythroughWriteResult
{
    public CanonicalRuntimeSelectedCandidateDashboard Dashboard { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record CanonicalRuntimeSelectedCandidatePackageValidation
{
    public string GoalId { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public string HandoffPath { get; init; } = string.Empty;
    public bool SelectedCandidateLoaded { get; init; }
    public bool HandoffMatchesPackage { get; init; }
    public bool ExistingValidatorPassed { get; init; }
    public bool RequiredAnchorsPresent { get; init; }
    public bool PackageValidationPassed { get; init; }
    public IReadOnlyList<string> RequiredAnchors { get; init; } = [];
    public IReadOnlyList<string> MissingAnchors { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record CanonicalRuntimeSelectedCandidateUnitySmoke
{
    public string GoalId { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId;
    public bool UnityAvailable { get; init; }
    public bool TranscriptPathExists { get; init; }
    public bool StateSummaryPathExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public bool UnityPlayerConsumedCanonicalTranscript { get; init; }
    public bool Passed { get; init; }
    public string UnityPath { get; init; } = string.Empty;
    public string UnityLogPath { get; init; } = string.Empty;
    public string TranscriptPath { get; init; } = string.Empty;
    public string StateSummaryPath { get; init; } = string.Empty;
    public string Status { get; init; } = "PENDING_UNITY_BATCHMODE";
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record CanonicalRuntimeSelectedCandidateMatrixResult
{
    public string GoalId { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<CanonicalRuntimeSelectedCandidateMatrixRow> Rows { get; init; } = [];
}

public sealed record CanonicalRuntimeSelectedCandidateMatrixRow
{
    public string CandidateId { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public bool PackageValidationPassed { get; init; }
    public bool CanonicalRuntimePassed { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool UnityPlayerConsumedCanonicalTranscript { get; init; }
    public bool Passed { get; init; }
}

public sealed record CanonicalRuntimeSelectedCandidateReport
{
    public string GoalId { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public bool PackageValidationPassed { get; init; }
    public bool CanonicalRuntimePassed { get; init; }
    public int RuntimeCommandCount { get; init; }
    public int RuntimeEventCount { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool UnityPlayerConsumedCanonicalTranscript { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public string NextRecommendedGoal { get; init; } = string.Empty;
    public string ReportPath { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ReportMarkdownRelativePath;
    public string MatrixResultPath { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.MatrixResultRelativePath;
}

public sealed record CanonicalRuntimeSelectedCandidateNegativeProof
{
    public string GoalId { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId;
    public bool ManualInputRejected { get; init; }
    public bool OutputRootUnderGoal134 { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool GamePackageSchemaUnchanged { get; init; }
    public bool GeneratorLibraryProviderLuaUnchanged { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesUnchanged { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public bool Passed { get; init; }
}

public sealed record CanonicalRuntimeSelectedCandidateFileIndex
{
    public string GoalId { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<CanonicalRuntimeSelectedCandidateFileIndexEntry> Files { get; init; } = [];
}

public sealed record CanonicalRuntimeSelectedCandidateFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record CanonicalRuntimeSelectedCandidateDashboard
{
    public string GoalId { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public string CandidateId { get; init; } = string.Empty;
    public bool SelectedCandidateLoaded { get; init; }
    public bool PackageValidationPassed { get; init; }
    public bool CanonicalRuntimeStarted { get; init; }
    public bool CanonicalRuntimePassed { get; init; }
    public int RuntimeCommandCount { get; init; }
    public int RuntimeEventCount { get; init; }
    public bool StateHashChainPresent { get; init; }
    public bool SaveLoadReplayPassed { get; init; }
    public bool UnityConsumedCanonicalTranscript { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool RuntimePrimitiveMissing { get; init; }
    public IReadOnlyList<string> MissingRuntimePrimitives { get; init; } = [];
    public string NormalCommand { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ReportMarkdownRelativePath;
    public string MatrixResultPath { get; init; } =
        CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.MatrixResultRelativePath;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}
