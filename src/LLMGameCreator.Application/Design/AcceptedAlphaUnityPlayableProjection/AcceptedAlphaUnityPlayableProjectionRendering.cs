namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed partial class AcceptedAlphaUnityPlayableProjectionService
{
    private static string RenderReport(
        AcceptedAlphaUnityPlayableProjectionDashboard dashboard,
        AcceptedAlphaUnityPlayableProjectionScriptInventory inventory,
        AcceptedAlphaUnityPlayableProjectionSmokePlan smokePlan,
        AcceptedAlphaUnityPlayableProjectionQualityGateScan quality,
        AcceptedAlphaUnityPlayableProjectionNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 119 Accepted Alpha Unity Playable Projection",
            string.Empty,
            "- implementationStatus: " + quality.ImplementationStatus,
            "- projectionStatus: " + dashboard.ProjectionStatus,
            "- unityMenuPath: " + dashboard.UnityMenuPath,
            "- baselineId: " + dashboard.BaselineId,
            "- acceptedBaselineReady: " + dashboard.AcceptedBaselineReady.ToString().ToLowerInvariant(),
            "- manualGateStatus: " + dashboard.ManualGateStatus,
            "- expectedGeneratedRootName: " + dashboard.ExpectedGeneratedRootName,
            "- scriptInventoryCount: " + dashboard.ScriptInventoryCount,
            "- smokePlanStepCount: " + dashboard.SmokePlanStepCount,
            "- previewCommandCount: " + dashboard.PreviewCommandCount,
            "- chunkWindowStepCount: " + dashboard.ChunkWindowStepCount,
            "- boundaryCrossingCount: " + dashboard.BoundaryCrossingCount,
            "- interactionTargetCount: " + dashboard.InteractionTargetCount,
            "- objectiveCount: " + dashboard.ObjectiveCount,
            "- completedObjectiveCount: " + dashboard.CompletedObjectiveCount,
            "- replayStepCount: " + dashboard.ReplayStepCount,
            "- forbiddenUnitySurfaceClean: "
            + dashboard.ForbiddenUnitySurfaceClean.ToString().ToLowerInvariant(),
            "- notFinalReleaseOrRuntimeBuild: true",
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Unity Scripts",
            string.Empty
        };
        lines.AddRange(inventory.Scripts.Select(script =>
            "- " + script.RelativePath
            + ": exists=" + script.Exists.ToString().ToLowerInvariant()
            + ", marker=" + script.ContainsRequiredMarker.ToString().ToLowerInvariant()));
        lines.AddRange(
        [
            string.Empty,
            "## Smoke Plan",
            string.Empty
        ]);
        lines.AddRange(smokePlan.Steps.Select(step =>
            "- " + step.StepIndex + ". " + step.StepId + ": " + step.ExpectedResult));
        lines.AddRange(
        [
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- manualInputRejected: " + negative.ManualInputRejected.ToString().ToLowerInvariant(),
            "- runtimeSchemaProviderLuaGeneratorLibraryRejected: "
            + negative.RuntimeSchemaProviderLuaGeneratorLibraryRejected.ToString().ToLowerInvariant(),
            "- unityScenesPrefabsSettingsPackagesStreamingAssetsRejected: "
            + negative.UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected.ToString().ToLowerInvariant(),
            "- finalReleasePackagingRejected: "
            + negative.FinalReleasePackagingRejected.ToString().ToLowerInvariant(),
            "- liveGeodataProviderNetworkRejected: "
            + negative.LiveGeodataProviderNetworkRejected.ToString().ToLowerInvariant()
        ]);
        AddDiagnostics(lines, "Diagnostics", quality.Diagnostics);
        AddDiagnostics(lines, "Errors", dashboard.Errors);
        AddDiagnostics(lines, "Warnings", dashboard.Warnings);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(
        AcceptedAlphaUnityPlayableProjectionDashboard dashboard,
        AcceptedAlphaUnityPlayableProjectionQualityGateScan quality)
    {
        var lines = new List<string>
        {
            "# Accepted Alpha Unity Playable Projection",
            string.Empty,
            "Goal119 creates a Unity Editor entrypoint that builds a temporary primitive projection over the accepted Alpha baseline.",
            string.Empty,
            "## Hands-on Verification",
            string.Empty,
            "- Open `unity/LLMGameCreatorAlpha` in Unity.",
            "- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.",
            "- Click `Build/Refresh Playable Projection`.",
            "- The current scene should contain `" + dashboard.ExpectedGeneratedRootName + "` with a player proxy, map markers, chunk/window diagnostics, interaction targets, objectives and smoke diagnostics.",
            "- Use `Clear Projection` to remove only the generated root object.",
            string.Empty,
            "## Boundaries",
            string.Empty,
            "Goal119 is not final release and does not authorize live geodata, providers, Runtime, schema, Lua, generator-library, final art, atlas, Unity scene/prefab/project-settings/StreamingAssets changes or release packaging.",
            string.Empty,
            "## Status",
            string.Empty,
            "- projectionStatus: " + dashboard.ProjectionStatus,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            "- unityMenuPath: " + dashboard.UnityMenuPath,
            "- acceptedBaselineReady: " + dashboard.AcceptedBaselineReady.ToString().ToLowerInvariant()
        };
        AddDiagnostics(lines, "Diagnostics", quality.Diagnostics);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static void AddDiagnostics(List<string> lines, string title, IReadOnlyList<string> diagnostics)
    {
        if (diagnostics.Count == 0)
        {
            return;
        }

        lines.Add(string.Empty);
        lines.Add("## " + title);
        lines.Add(string.Empty);
        lines.AddRange(diagnostics.Select(item => "- " + item));
    }
}
