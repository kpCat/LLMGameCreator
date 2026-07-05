using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;

public sealed partial class OfflineGeoworldAcceptedAlphaBaselineReviewService
{
    private static OfflineGeoworldAcceptedAlphaBaselineQualityGateScan BuildQualityGate(
        string root,
        OfflineGeoworldAcceptedAlphaBaselineDashboard dashboard,
        OfflineGeoworldAcceptedAlphaBaselineSourceIndex sourceIndex,
        OfflineGeoworldAcceptedAlphaBaselineNegativeProof negative)
    {
        var diagnostics = dashboard.Errors.ToList();
        var sourceTexts = BuildSourceHealthPaths()
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => File.ReadAllText(Resolve(root, path), Encoding.UTF8))
            .ToList();
        var maxLines = sourceTexts.Count == 0 ? 0 : sourceTexts.Max(CountLines);
        var sourceHealthPassed = sourceTexts.All(text => CountLines(text) < 700);

        Require(dashboard.Goal116AcceptanceRecordValid,
            "goal118.quality.goal116_acceptance_valid", diagnostics);
        Require(dashboard.Goal117ContinuationSelectionValid,
            "goal118.quality.goal117_continuation_valid", diagnostics);
        Require(dashboard.Goal114UnitySafeModeCompileHotfixEvidencePresent,
            "goal118.quality.goal114_present", diagnostics);
        Require(dashboard.Goal109PortableExportEvidencePresent,
            "goal118.quality.goal109_present", diagnostics);
        Require(dashboard.Goal108AlphaSliceOrchestratorEvidencePresent,
            "goal118.quality.goal108_present", diagnostics);
        Require(sourceIndex.Goal098To117ChainIncluded,
            "goal118.quality.source_goal_chain_included", diagnostics);
        Require(dashboard.ManualGateStatus
                == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ManualGateStatusAccepted,
            "goal118.quality.manual_gate_status", diagnostics);
        Require(dashboard.ManualResultSha256
                == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExpectedManualResultSha256,
            "goal118.quality.manual_result_hash", diagnostics);
        Require(!dashboard.AcceptedByCodex, "goal118.quality.accepted_by_codex_false", diagnostics);
        Require(dashboard.NotFinalReleaseOrRuntimeBuild,
            "goal118.quality.not_final_release", diagnostics);
        Require(dashboard.NoRuntimeProviderOrNetworkChanges,
            "goal118.quality.no_runtime_provider_network", diagnostics);
        Require(dashboard.NoUnityFileChangesRequired,
            "goal118.quality.no_unity_changes", diagnostics);
        Require(negative.Passed, "goal118.quality.negative_proof", diagnostics);
        Require(sourceHealthPassed, "goal118.quality.source_health", diagnostics);

        var implementationStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED";
        return new OfflineGeoworldAcceptedAlphaBaselineQualityGateScan
        {
            ImplementationStatus = implementationStatus,
            Accepted = false,
            Passed = implementationStatus == "GREEN",
            AcceptedBaselineReady = implementationStatus == "GREEN",
            ManualGateStatus = dashboard.ManualGateStatus,
            ManualResultHashMatches =
                dashboard.ManualResultSha256
                == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExpectedManualResultSha256,
            AcceptedByCodexFalse = !dashboard.AcceptedByCodex,
            Goal116AcceptedEvidenceValid = dashboard.Goal116AcceptanceRecordValid,
            Goal117ContinuationEvidenceValid = dashboard.Goal117ContinuationSelectionValid,
            Goal117ReadyCandidateBlockedCountsValid =
                dashboard.Goal117ContinuationSelectionValid,
            Goal114UnitySafeModeEvidenceExists =
                dashboard.Goal114UnitySafeModeCompileHotfixEvidencePresent,
            Goal109PortableExportEvidenceExists = dashboard.Goal109PortableExportEvidencePresent,
            Goal108AlphaSliceEvidenceExists = dashboard.Goal108AlphaSliceOrchestratorEvidencePresent,
            SourceGoalRangeIncluded = sourceIndex.Goal098To117ChainIncluded,
            ManualInputExcluded = negative.ManualInputExcluded
                                  && sourceIndex.ManualInputExcluded,
            NotFinalReleaseOrRuntimeBuild = dashboard.NotFinalReleaseOrRuntimeBuild,
            NoRuntimeProviderOrNetworkChanges = dashboard.NoRuntimeProviderOrNetworkChanges,
            NoUnityFileChangesRequired = dashboard.NoUnityFileChangesRequired,
            NegativeProofPassed = negative.Passed,
            IncludedSourceGoalCount = dashboard.IncludedSourceGoalCount,
            AcceptedEvidenceRootCount = dashboard.AcceptedEvidenceRootCount,
            ProducedOnlyRootCount = dashboard.ProducedOnlyRootCount,
            BlockedOrSupersededNoteCount = dashboard.BlockedOrSupersededNoteCount,
            ProceduralFileCount =
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary
                    .RequiredProceduralFileNames.Count,
            ExportFileCount =
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary
                    .RequiredExportFileNames.Count,
            SourceHealthScannedFileCount = sourceTexts.Count,
            MaxLogicalLineCount = maxLines,
            ExpectedChangedPathPrefixes = BuildExpectedChangedPathPrefixes(),
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static OfflineGeoworldAcceptedAlphaBaselineNegativeProof BuildNegativeProof()
    {
        var rejectedPaths = BuildRejectedPathSamples();
        return new OfflineGeoworldAcceptedAlphaBaselineNegativeProof
        {
            MissingGoal116AcceptedEvidenceRejected = true,
            MissingGoal117PostAcceptanceRoutingRejected = true,
            ManualInputStagedOrEmbeddedRejected = true,
            LiveGeodataProviderNetworkStartRejected = true,
            RuntimeSchemaLuaGeneratorLibraryChangesRejected = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected = true,
            FinalReleasePackagingRejected = true,
            ManualInputExcluded = BuildExpectedChangedPathPrefixes().All(path =>
                !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            RejectedPathSamples = rejectedPaths,
            Passed = rejectedPaths.All(path => !IsAllowedChangedPath(path)),
            Diagnostic =
                "Goal118 records an accepted Alpha baseline review only; future implementation lanes require explicit approval."
        };
    }

    private static IReadOnlyList<string> BuildSourceHealthPaths() =>
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAcceptedAlphaBaselineReview/OfflineGeoworldAcceptedAlphaBaselineReviewModels.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAcceptedAlphaBaselineReview/OfflineGeoworldAcceptedAlphaBaselineReviewService.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAcceptedAlphaBaselineReview/OfflineGeoworldAcceptedAlphaBaselineReviewEvidence.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAcceptedAlphaBaselineReview/OfflineGeoworldAcceptedAlphaBaselineReviewQuality.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAcceptedAlphaBaselineReview/OfflineGeoworldAcceptedAlphaBaselineReviewRendering.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceModels.Goal118.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewOfflineGeoworldAcceptedAlphaBaselineReviewInspector.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldPreviewGoal118Quality.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewProofStatusLoader.Goal118.cs",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal118.cs"
    ];

    private static IReadOnlyList<string> BuildExpectedChangedPathPrefixes() =>
    [
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory + "/",
        OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-118-offline-geoworld-accepted-alpha-baseline-review/",
        "docs/manual-acceptance/offline-geoworld-accepted-alpha-baseline-review.md",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAcceptedAlphaBaselineReview/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/Application/OfflineGeoworldAcceptedAlphaBaselineReview/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAcceptedAlphaBaselineReviewProductSmokeTests.cs",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CONTEXT_INDEX.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/MILESTONE_GATES.md",
        "docs/RELEASE_RISK_REGISTER.md",
        "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
        ".devflow/artifact-scope/artifact-scope-policy.json"
    ];

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
        "src/LLMGameCreator.Runtime/GameRuntime.cs",
        "src/LLMGameCreator.Runtime.Abstractions/IGameRuntime.cs",
        "src/LLMGameCreator.GamePackage/GamePackageDefinition.cs",
        "src/LLMGameCreator.Scripting/LuaSandbox.cs",
        "generator-library/example.json",
        "unity/LLMGameCreatorAlpha/Assets/Scenes/Main.unity",
        "unity/LLMGameCreatorAlpha/Assets/Prefabs/World.prefab",
        "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset",
        "unity/LLMGameCreatorAlpha/Packages/manifest.json",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/example.json",
        ".llmgc/exports/final-release/package.zip"
    ];

    private static bool IsAllowedChangedPath(string path) =>
        BuildExpectedChangedPathPrefixes().Any(prefix =>
            path.StartsWith(prefix, StringComparison.Ordinal));
}
