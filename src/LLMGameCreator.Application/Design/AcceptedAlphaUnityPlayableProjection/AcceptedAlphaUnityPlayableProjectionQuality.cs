using System.Text;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed partial class AcceptedAlphaUnityPlayableProjectionService
{
    private static AcceptedAlphaUnityPlayableProjectionQualityGateScan BuildQualityGate(
        string root,
        AcceptedAlphaUnityPlayableProjectionDashboard dashboard,
        AcceptedAlphaUnityPlayableProjectionScriptInventory inventory,
        AcceptedAlphaUnityPlayableProjectionSmokePlan smokePlan,
        AcceptedAlphaUnityPlayableProjectionNegativeProof negative)
    {
        var diagnostics = dashboard.Errors.ToList();
        Require(dashboard.AcceptedBaselineReady, "goal119.quality.accepted_baseline_ready", diagnostics);
        Require(
            dashboard.ManualGateStatus
            == AcceptedAlphaUnityPlayableProjectionVocabulary.ManualGateStatusAccepted,
            "goal119.quality.manual_gate_accepted",
            diagnostics);
        Require(inventory.MenuPathExistsExactly, "goal119.quality.menu_path", diagnostics);
        Require(inventory.AllScriptsPresent, "goal119.quality.unity_scripts", diagnostics);
        Require(
            inventory.Scripts.Any(script =>
                script.RelativePath == AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath
                && script.ContainsRequiredMarker),
            "goal119.quality.controller_root_marker",
            diagnostics);
        Require(smokePlan.StepCount >= 5, "goal119.quality.smoke_plan_steps", diagnostics);
        Require(smokePlan.BaselineLoaded, "goal119.quality.smoke_baseline", diagnostics);
        Require(smokePlan.HasPlayerProxyStep, "goal119.quality.smoke_player", diagnostics);
        Require(smokePlan.HasChunkWindowStep, "goal119.quality.smoke_chunk", diagnostics);
        Require(smokePlan.HasInteractionOrObjectiveStep, "goal119.quality.smoke_interaction_objective", diagnostics);
        Require(smokePlan.HasDiagnosticsStatusStep, "goal119.quality.smoke_diagnostics", diagnostics);
        Require(negative.Passed, "goal119.quality.negative_proof", diagnostics);

        var sourceTexts = BuildSourceHealthPaths()
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => File.ReadAllText(Resolve(root, path), Encoding.UTF8))
            .ToList();
        var maxLines = sourceTexts.Count == 0 ? 0 : sourceTexts.Max(CountLines);
        Require(sourceTexts.All(text => CountLines(text) < 1000), "goal119.quality.source_health", diagnostics);

        var expectedChangedPaths = BuildExpectedChangedPaths();
        var noSettingsPackagesStreamingAssets = expectedChangedPaths.All(path =>
            !path.StartsWith("unity/LLMGameCreatorAlpha/ProjectSettings/", StringComparison.Ordinal)
            && !path.StartsWith("unity/LLMGameCreatorAlpha/Packages/", StringComparison.Ordinal)
            && !path.StartsWith("unity/LLMGameCreatorAlpha/Assets/StreamingAssets/", StringComparison.Ordinal));
        var noRuntimeSchemaProviderLuaGenerator = expectedChangedPaths.All(path =>
            !path.StartsWith("src/LLMGameCreator.Runtime/", StringComparison.Ordinal)
            && !path.StartsWith("src/LLMGameCreator.Runtime.Abstractions/", StringComparison.Ordinal)
            && !path.StartsWith("src/LLMGameCreator.GamePackage/", StringComparison.Ordinal)
            && !path.StartsWith("src/LLMGameCreator.Scripting/", StringComparison.Ordinal)
            && !path.StartsWith("generator-library/", StringComparison.Ordinal));
        var manualExcluded = expectedChangedPaths.All(path =>
            !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Require(noSettingsPackagesStreamingAssets,
            "goal119.quality.no_project_settings_packages_streamingassets", diagnostics);
        Require(noRuntimeSchemaProviderLuaGenerator,
            "goal119.quality.no_runtime_schema_provider_lua_generator", diagnostics);
        Require(manualExcluded, "goal119.quality.manual_excluded", diagnostics);

        var passed = diagnostics.Count == 0;
        return new AcceptedAlphaUnityPlayableProjectionQualityGateScan
        {
            ImplementationStatus = passed ? "GREEN" : "BLOCKED",
            Passed = passed,
            MenuPathExistsExactly = inventory.MenuPathExistsExactly,
            NewUnityScriptsPresent = inventory.AllScriptsPresent,
            ProjectionRootNamePresent = inventory.Scripts.Any(script =>
                script.RelativePath == AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath
                && script.ContainsRequiredMarker),
            BaselineFromGoal118Ready = dashboard.AcceptedBaselineReady,
            Goal116ManualGateAccepted =
                dashboard.ManualGateStatus
                == AcceptedAlphaUnityPlayableProjectionVocabulary.ManualGateStatusAccepted,
            SmokePlanCoversRequiredChecks = smokePlan.StepCount >= 5
                                           && smokePlan.BaselineLoaded
                                           && smokePlan.HasPlayerProxyStep
                                           && smokePlan.HasChunkWindowStep
                                           && smokePlan.HasInteractionOrObjectiveStep
                                           && smokePlan.HasDiagnosticsStatusStep,
            ForbiddenUnitySurfaceClean = negative.Passed && inventory.NoForbiddenUnityPathsExpected,
            NoProjectSettingsPackagesStreamingAssetsExpected = noSettingsPackagesStreamingAssets,
            NoRuntimeSchemaProviderLuaGeneratorLibraryExpected = noRuntimeSchemaProviderLuaGenerator,
            ManualInputExcluded = manualExcluded && negative.ManualInputExcluded,
            NotFinalReleaseOrRuntimeBuild = true,
            NegativeProofPassed = negative.Passed,
            ExpectedChangedPathCount = expectedChangedPaths.Count,
            SourceHealthScannedFileCount = sourceTexts.Count,
            MaxLogicalLineCount = maxLines,
            ExpectedChangedPaths = expectedChangedPaths,
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static IReadOnlyList<string> BuildExpectedChangedPaths() =>
    [
        AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory + "/",
        AcceptedAlphaUnityPlayableProjectionVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-119-accepted-alpha-unity-playable-projection/",
        AcceptedAlphaUnityPlayableProjectionVocabulary.DocumentationPath,
        ".devflow/artifact-scope/artifact-scope-policy.json",
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/CONTEXT_INDEX.md",
        "docs/MILESTONE_GATES.md",
        "docs/RELEASE_RISK_REGISTER.md",
        "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
        "src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityDiagnosticsPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityDrilldownPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityActionPreviewPath,
        "tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs"
    ];

    private static IReadOnlyList<string> BuildSourceHealthPaths() =>
    [
        "src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/AcceptedAlphaUnityPlayableProjectionModels.cs",
        "src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/AcceptedAlphaUnityPlayableProjectionQuality.cs",
        "src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/AcceptedAlphaUnityPlayableProjectionRendering.cs",
        "src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/AcceptedAlphaUnityPlayableProjectionService.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceModels.Goal119.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewAcceptedAlphaUnityPlayableProjectionInspector.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldPreviewGoal119Quality.cs",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal119.cs",
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityDiagnosticsPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityDrilldownPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityActionPreviewPath
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
        "unity/LLMGameCreatorAlpha/Assets/Prefabs/AcceptedAlpha.prefab",
        "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset",
        "unity/LLMGameCreatorAlpha/Packages/manifest.json",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal119/example.json",
        ".llmgc/exports/final-release/package.zip",
        "provider/live-geodata/Adapter.cs"
    ];

    private static bool IsAllowedChangedPath(string path) =>
        BuildExpectedChangedPaths().Any(prefix =>
            prefix.EndsWith("/", StringComparison.Ordinal)
                ? path.StartsWith(prefix, StringComparison.Ordinal)
                : string.Equals(path, prefix, StringComparison.Ordinal));
}
