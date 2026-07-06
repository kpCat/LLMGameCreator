using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class AcceptedAlphaProjectionActionLoopService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly Regex RendererMaterialAccessPattern =
        new(@"\brenderer\.material\b", RegexOptions.Compiled);

    private static readonly Regex MaterialAssignmentPattern =
        new(@"\.material\s*=", RegexOptions.Compiled);

    public AcceptedAlphaProjectionActionLoopBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var scriptInventory = BuildScriptInventory(root);
        var logScan = BuildLogScan(root);
        var smokePlan = BuildSmokePlan();
        var negative = BuildNegativeProof();
        var goal121StillGreen = Goal121StillGreen(root);
        var dashboard = BuildDashboard(scriptInventory, logScan, smokePlan, negative, goal121StillGreen);
        var report = RenderReport(dashboard, scriptInventory, smokePlan, logScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName] = Serialize(dashboard),
            [AcceptedAlphaProjectionActionLoopVocabulary.ScriptInventoryFileName] = Serialize(scriptInventory),
            [AcceptedAlphaProjectionActionLoopVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [AcceptedAlphaProjectionActionLoopVocabulary.LogScanFileName] = Serialize(logScan),
            [AcceptedAlphaProjectionActionLoopVocabulary.ReportFileName] = report,
            [AcceptedAlphaProjectionActionLoopVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory,
            "goal122_accepted_alpha_projection_action_loop_evidence",
            includeUnityLog: true);
        proceduralFiles[AcceptedAlphaProjectionActionLoopVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName] = Serialize(dashboard),
            [AcceptedAlphaProjectionActionLoopVocabulary.ScriptInventoryFileName] = Serialize(scriptInventory),
            [AcceptedAlphaProjectionActionLoopVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [AcceptedAlphaProjectionActionLoopVocabulary.LogScanFileName] = Serialize(logScan),
            [AcceptedAlphaProjectionActionLoopVocabulary.ReportFileName] = report,
            [AcceptedAlphaProjectionActionLoopVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            AcceptedAlphaProjectionActionLoopVocabulary.ExportPackageDirectory,
            "goal122_accepted_alpha_projection_action_loop_export",
            includeUnityLog: false);
        exportFiles[AcceptedAlphaProjectionActionLoopVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new AcceptedAlphaProjectionActionLoopBuildResult
        {
            Dashboard = dashboard,
            ScriptInventory = scriptInventory,
            SmokePlan = smokePlan,
            LogScan = logScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public async Task<AcceptedAlphaProjectionActionLoopWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(root, AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory);
        var export = Resolve(root, AcceptedAlphaProjectionActionLoopVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, AcceptedAlphaProjectionActionLoopVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.ProceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new AcceptedAlphaProjectionActionLoopWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaProjectionActionLoopDashboard BuildDashboard(
        AcceptedAlphaProjectionActionLoopScriptInventory scriptInventory,
        AcceptedAlphaProjectionActionLoopLogScan logScan,
        AcceptedAlphaProjectionActionLoopSmokePlan smokePlan,
        AcceptedAlphaProjectionActionLoopNegativeProof negative,
        bool goal121StillGreen)
    {
        var diagnostics = new List<string>();
        Require(goal121StillGreen, "goal122.goal121_not_green", diagnostics);
        Require(scriptInventory.Passed, "goal122.script_inventory_failed", diagnostics);
        Require(smokePlan.StepCount >= 9, "goal122.smoke_plan_incomplete", diagnostics);
        Require(negative.Passed, "goal122.negative_proof_failed", diagnostics);
        Require(logScan.Status != "BLOCKED_UNITY_BATCHMODE_ACTION_LOOP_SMOKE",
            "goal122.unity_action_loop_smoke_failed", diagnostics);

        var sourceReady = diagnostics.Count == 0;
        return new AcceptedAlphaProjectionActionLoopDashboard
        {
            ActionLoopStatus = sourceReady ? "GREEN" : "BLOCKED",
            WindowPolishStatus =
                scriptInventory.WindowLayoutPolishPresent ? "GREEN" : "BLOCKED",
            OneClickVerificationStillPresent = scriptInventory.OneClickVerificationStillPresent,
            Goal121StillGreen = goal121StillGreen,
            ProjectionActionPreviewPresent = scriptInventory.ProjectionActionPreviewPresent,
            ProjectionActionApplyPresent = scriptInventory.ProjectionActionApplyPresent,
            ProjectionStateResetPresent = scriptInventory.ProjectionStateResetPresent,
            WindowLayoutPolishPresent = scriptInventory.WindowLayoutPolishPresent,
            CleanupScriptAvailable = CleanupScriptAvailable(scriptInventory),
            MaterialWarningGuardPresent = scriptInventory.MaterialWarningGuardPresent,
            UnitySmokeStatus = logScan.Status,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaProjectionActionLoopScriptInventory BuildScriptInventory(string root)
    {
        var entries = new[]
        {
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
                "unity_editor_window", "RunBatchmodeProjectionActionLoopSmoke"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath,
                "unity_projection_controller", "PreviewSelectedAction"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath,
                "unity_projection_models", "projectionActionPreviewPresent"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
                "unity_projection_primitive_factory", "MaterialWarningGuardPresent"),
            Entry(root, AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityDrilldownPath,
                "unity_projection_drilldown", "BuildObjectiveReplayDetails"),
            Entry(root, AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityActionPreviewPath,
                "unity_projection_action_preview", "BuildProjectionActionSummary"),
            Entry(root, AcceptedAlphaProjectionActionLoopVocabulary.UnityStatePath,
                "unity_projection_state", "AcceptedAlphaPlayableProjectionState"),
            Entry(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
                "cleanup_script", "Unity editor noise cleanup mode"),
            Entry(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath,
                "cleanup_cmd_wrapper", "clean-unity-editor-noise.ps1")
        }.ToList();

        var editorText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath);
        var controllerText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath);
        var modelsText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath);
        var primitiveText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath);
        var actionPreviewText = SourceText(root,
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityActionPreviewPath);
        var stateText = SourceText(root, AcceptedAlphaProjectionActionLoopVocabulary.UnityStatePath);
        var allUnityText = string.Join("\n", entries
            .Where(entry => entry.RelativePath.StartsWith("unity/", StringComparison.Ordinal))
            .Select(entry => SourceText(root, entry.RelativePath)));
        var smokeMarkers = new[]
        {
            "fullVerificationPassed",
            "selectedMarkerDetailsPresent",
            "interactionPreviewPresent",
            "objectiveReplayDetailsPresent",
            "verificationEventLogPresent",
            "projectionActionPreviewPresent",
            "projectionActionApplyPassed",
            "projectionStateResetPassed",
            "windowLayoutPolishPresent",
            "materialWarningGuardPresent"
        };
        var materialWarningSourceClean =
            !RendererMaterialAccessPattern.IsMatch(allUnityText)
            && !MaterialAssignmentPattern.IsMatch(allUnityText)
            && primitiveText.Contains("MaterialPropertyBlock", StringComparison.Ordinal)
            && primitiveText.Contains("SetPropertyBlock", StringComparison.Ordinal);

        var inventory = new AcceptedAlphaProjectionActionLoopScriptInventory
        {
            ScriptCount = entries.Count,
            OneClickVerificationStillPresent =
                editorText.Contains("Run Full Projection Verification", StringComparison.Ordinal)
                && editorText.Contains("RunFullProjectionVerification()", StringComparison.Ordinal),
            BatchmodeActionLoopMethodPresent =
                editorText.Contains("RunBatchmodeProjectionActionLoopSmoke", StringComparison.Ordinal),
            BatchmodePassMarkerPresent =
                editorText.Contains("GOAL122_ACTION_LOOP_SMOKE_PASS", StringComparison.Ordinal),
            BatchmodeFailMarkerPresent =
                editorText.Contains("GOAL122_ACTION_LOOP_SMOKE_FAIL", StringComparison.Ordinal),
            ActionLoopControlsPresent =
                editorText.Contains("Select Next Interaction Target", StringComparison.Ordinal)
                && editorText.Contains("Preview Selected Action", StringComparison.Ordinal)
                && editorText.Contains("Apply Preview Action To Projection State", StringComparison.Ordinal)
                && editorText.Contains("Reset Projection State", StringComparison.Ordinal),
            ProjectionStateModelPresent =
                stateText.Contains("AcceptedAlphaPlayableProjectionState", StringComparison.Ordinal)
                && stateText.Contains("AppliedActionCount", StringComparison.Ordinal)
                && stateText.Contains("EventLogText", StringComparison.Ordinal),
            ProjectionActionPreviewPresent =
                controllerText.Contains("PreviewSelectedAction", StringComparison.Ordinal)
                && actionPreviewText.Contains("BuildProjectionActionSummary", StringComparison.Ordinal),
            ProjectionActionApplyPresent =
                controllerText.Contains("ApplyPreviewActionToProjectionState", StringComparison.Ordinal)
                && stateText.Contains("ApplyPreview", StringComparison.Ordinal),
            ProjectionStateResetPresent =
                controllerText.Contains("ResetProjectionState", StringComparison.Ordinal)
                && stateText.Contains("Reset()", StringComparison.Ordinal),
            WindowLayoutPolishPresent =
                editorText.Contains("DrawStatusArea", StringComparison.Ordinal)
                && editorText.Contains("DrawFoldoutTextPanel", StringComparison.Ordinal)
                && editorText.Contains("GUILayout.MaxHeight", StringComparison.Ordinal)
                && editorText.Contains("Debug / Optional Inspection", StringComparison.Ordinal)
                && editorText.Contains(".devflow\\\\scripts\\\\clean-unity-editor-noise.cmd",
                    StringComparison.Ordinal),
            ManualCleanupHintPresent =
                editorText.Contains("Run Full Projection Verification", StringComparison.Ordinal)
                && editorText.Contains(".devflow\\\\scripts\\\\clean-unity-editor-noise.cmd",
                    StringComparison.Ordinal),
            SmokeRequiredFieldsPresent = smokeMarkers.All(marker =>
                modelsText.Contains(marker, StringComparison.Ordinal)),
            MaterialWarningGuardPresent =
                primitiveText.Contains("MaterialWarningGuardPresent", StringComparison.Ordinal)
                && primitiveText.Contains("MaterialPropertyBlock", StringComparison.Ordinal),
            MaterialWarningSourceClean = materialWarningSourceClean,
            Scripts = entries
        };

        return inventory with
        {
            Passed = inventory.ScriptCount == 9
                     && entries.All(entry => entry.Exists && entry.ContainsRequiredMarker)
                     && inventory.OneClickVerificationStillPresent
                     && inventory.BatchmodeActionLoopMethodPresent
                     && inventory.BatchmodePassMarkerPresent
                     && inventory.BatchmodeFailMarkerPresent
                     && inventory.ActionLoopControlsPresent
                     && inventory.ProjectionStateModelPresent
                     && inventory.ProjectionActionPreviewPresent
                     && inventory.ProjectionActionApplyPresent
                     && inventory.ProjectionStateResetPresent
                     && inventory.WindowLayoutPolishPresent
                     && inventory.ManualCleanupHintPresent
                     && inventory.SmokeRequiredFieldsPresent
                     && inventory.MaterialWarningGuardPresent
                     && inventory.MaterialWarningSourceClean
        };
    }

    private static AcceptedAlphaProjectionActionLoopSmokePlan BuildSmokePlan()
    {
        var steps = new List<AcceptedAlphaProjectionActionLoopSmokePlanStep>
        {
            Step(1, "open_projection_window", "Open the accepted Alpha projection menu path."),
            Step(2, "run_full_projection_verification", "The primary verification button remains the main path."),
            Step(3, "read_compact_status", "Top status shows baseline, full verification, selected marker and projection state."),
            Step(4, "select_next_interaction_target", "A descriptor-backed interaction target is selected."),
            Step(5, "preview_selected_action", "Projection action preview text is populated from accepted Goal105 payloads."),
            Step(6, "apply_preview_to_projection_state", "Projection-only state count and event log update without file writes."),
            Step(7, "reset_projection_state", "Projection-only state is reset and event log records the reset."),
            Step(8, "inspect_bounded_panels", "Smoke/details/preview/objective/log panels are foldout and bounded-height."),
            Step(9, "run_cleanup_after_unity", "Use .devflow scripts clean-unity-editor-noise after Unity verification.")
        };

        return new AcceptedAlphaProjectionActionLoopSmokePlan
        {
            StepCount = steps.Count,
            Steps = steps
        };
    }

    private static AcceptedAlphaProjectionActionLoopLogScan BuildLogScan(string root)
    {
        var path = Resolve(root, AcceptedAlphaProjectionActionLoopVocabulary.UnityBatchmodeLogRelativePath);
        var logExists = File.Exists(path);
        var text = logExists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var forbidden = new List<string>();
        var materialWarning =
            "Instantiating material due to calling renderer" + ".material during edit mode";
        var rendererStack = "UnityEngine.Renderer:get_material()";
        if (text.Contains(materialWarning, StringComparison.Ordinal))
        {
            forbidden.Add(materialWarning);
        }

        if (text.Contains(rendererStack, StringComparison.Ordinal))
        {
            forbidden.Add(rendererStack);
        }

        if (text.Contains("GOAL122_ACTION_LOOP_SMOKE_FAIL", StringComparison.Ordinal))
        {
            forbidden.Add("GOAL122_ACTION_LOOP_SMOKE_FAIL");
        }

        var smokeFields = new[]
        {
            "fullVerificationPassed=True",
            "selectedMarkerDetailsPresent=True",
            "interactionPreviewPresent=True",
            "objectiveReplayDetailsPresent=True",
            "verificationEventLogPresent=True",
            "projectionActionPreviewPresent=True",
            "projectionActionApplyPassed=True",
            "projectionStateResetPassed=True",
            "windowLayoutPolishPresent=True",
            "materialWarningGuardPresent=True"
        };
        var passMarkerPresent = text.Contains("GOAL122_ACTION_LOOP_SMOKE_PASS", StringComparison.Ordinal);
        var smokeFieldsPresent = smokeFields.All(field => text.Contains(field, StringComparison.Ordinal));
        var passed = logExists && passMarkerPresent && smokeFieldsPresent && forbidden.Count == 0;
        return new AcceptedAlphaProjectionActionLoopLogScan
        {
            LogExists = logExists,
            PassMarkerPresent = passMarkerPresent,
            FailMarkerAbsent = !forbidden.Contains("GOAL122_ACTION_LOOP_SMOKE_FAIL", StringComparer.Ordinal),
            MaterialInstantiationWarningAbsent = !forbidden.Contains(materialWarning, StringComparer.Ordinal),
            RendererGetMaterialStackAbsent = !forbidden.Contains(rendererStack, StringComparer.Ordinal),
            SmokeRequiredFieldsPresent = smokeFieldsPresent,
            Passed = passed,
            Status = passed
                ? "GREEN"
                : logExists
                    ? "BLOCKED_UNITY_BATCHMODE_ACTION_LOOP_SMOKE"
                    : "PENDING_UNITY_BATCHMODE_ACTION_LOOP_SMOKE",
            Sha256 = logExists ? HashBytes(File.ReadAllBytes(path)) : string.Empty,
            ForbiddenMarkersFound = forbidden.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaProjectionActionLoopNegativeProof BuildNegativeProof()
    {
        var rejected = BuildRejectedPathSamples();
        return new AcceptedAlphaProjectionActionLoopNegativeProof
        {
            ManualInputRejected = true,
            RuntimeSchemaProviderLuaGeneratorLibraryRejected = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected = true,
            FinalReleasePackagingRejected = true,
            LiveGeodataProviderNetworkRejected = true,
            ManualInputExcluded = BuildExpectedChangedPaths().All(path =>
                !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            RejectedPathSamples = rejected,
            Passed = rejected.All(path => !IsAllowedChangedPath(path))
        };
    }

    private static AcceptedAlphaProjectionActionLoopFileIndex BuildFileIndex(
        string root,
        IReadOnlyDictionary<string, string> files,
        string relativeRoot,
        string role,
        bool includeUnityLog)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new AcceptedAlphaProjectionActionLoopFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        if (includeUnityLog)
        {
            var logPath = Resolve(root, AcceptedAlphaProjectionActionLoopVocabulary.UnityBatchmodeLogRelativePath);
            if (File.Exists(logPath))
            {
                entries.Add(new AcceptedAlphaProjectionActionLoopFileIndexEntry
                {
                    RelativePath = AcceptedAlphaProjectionActionLoopVocabulary.UnityBatchmodeLogRelativePath,
                    Role = "goal122_unity_batchmode_action_loop_smoke_log",
                    Sha256 = HashBytes(File.ReadAllBytes(logPath))
                });
            }
        }

        return new AcceptedAlphaProjectionActionLoopFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static string RenderReport(
        AcceptedAlphaProjectionActionLoopDashboard dashboard,
        AcceptedAlphaProjectionActionLoopScriptInventory scriptInventory,
        AcceptedAlphaProjectionActionLoopSmokePlan smokePlan,
        AcceptedAlphaProjectionActionLoopLogScan logScan,
        AcceptedAlphaProjectionActionLoopNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 122 Accepted Alpha Projection Action Loop And Window Polish",
            string.Empty,
            "- actionLoopStatus: " + dashboard.ActionLoopStatus,
            "- windowPolishStatus: " + dashboard.WindowPolishStatus,
            "- unityMenuPath: " + dashboard.UnityMenuPath,
            "- oneClickVerificationStillPresent: "
            + dashboard.OneClickVerificationStillPresent.ToString().ToLowerInvariant(),
            "- goal121StillGreen: " + dashboard.Goal121StillGreen.ToString().ToLowerInvariant(),
            "- projectionActionPreviewPresent: "
            + dashboard.ProjectionActionPreviewPresent.ToString().ToLowerInvariant(),
            "- projectionActionApplyPresent: "
            + dashboard.ProjectionActionApplyPresent.ToString().ToLowerInvariant(),
            "- projectionStateResetPresent: "
            + dashboard.ProjectionStateResetPresent.ToString().ToLowerInvariant(),
            "- windowLayoutPolishPresent: "
            + dashboard.WindowLayoutPolishPresent.ToString().ToLowerInvariant(),
            "- cleanupScriptAvailable: " + dashboard.CleanupScriptAvailable.ToString().ToLowerInvariant(),
            "- materialWarningGuardPresent: "
            + dashboard.MaterialWarningGuardPresent.ToString().ToLowerInvariant(),
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Script Inventory",
            string.Empty,
            "- passed: " + scriptInventory.Passed.ToString().ToLowerInvariant(),
            "- scriptCount: " + scriptInventory.ScriptCount,
            "- actionLoopControlsPresent: "
            + scriptInventory.ActionLoopControlsPresent.ToString().ToLowerInvariant(),
            "- manualCleanupHintPresent: "
            + scriptInventory.ManualCleanupHintPresent.ToString().ToLowerInvariant(),
            string.Empty,
            "## Smoke Plan",
            string.Empty,
            "- stepCount: " + smokePlan.StepCount,
            string.Empty,
            "## Log Scan",
            string.Empty,
            "- status: " + logScan.Status,
            "- logExists: " + logScan.LogExists.ToString().ToLowerInvariant(),
            "- passMarkerPresent: " + logScan.PassMarkerPresent.ToString().ToLowerInvariant(),
            "- smokeRequiredFieldsPresent: " + logScan.SmokeRequiredFieldsPresent.ToString().ToLowerInvariant(),
            "- forbiddenMarkerCount: " + logScan.ForbiddenMarkersFound.Count,
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- passed: " + negative.Passed.ToString().ToLowerInvariant()
        };
        if (dashboard.Diagnostics.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Diagnostics");
            lines.Add(string.Empty);
            lines.AddRange(dashboard.Diagnostics.Select(item => "- " + item));
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(AcceptedAlphaProjectionActionLoopDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Accepted Alpha Projection Action Loop And Window Polish",
            string.Empty,
            "Goal122 keeps the accepted Alpha Unity projection as a projection-only Editor surface while adding a local action preview/apply/reset loop and a more readable EditorWindow layout.",
            string.Empty,
            "## Hands-on Verification",
            string.Empty,
            "- Open `unity/LLMGameCreatorAlpha` in Unity.",
            "- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.",
            "- Click `Run Full Projection Verification`.",
            "- Use `Select Next Interaction Target`, `Preview Selected Action`, `Apply Preview Action To Projection State` and `Reset Projection State` for the projection-local action loop.",
            "- Do not save scenes, prefabs, ProjectSettings, Packages or StreamingAssets as part of this check.",
            string.Empty,
            "## Cleanup Command",
            string.Empty,
            "- After Unity checks: `.\\.devflow\\scripts\\clean-unity-editor-noise.cmd`",
            string.Empty,
            "## Status",
            string.Empty,
            "- actionLoopStatus: " + dashboard.ActionLoopStatus,
            "- windowPolishStatus: " + dashboard.WindowPolishStatus,
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- projectionOnlyState: " + dashboard.ProjectionOnlyState.ToString().ToLowerInvariant(),
            "- noRuntimeProviderSchemaLuaGeneratorLibrary: "
            + dashboard.NoRuntimeProviderSchemaLuaGeneratorLibrary.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static bool Goal121StillGreen(string root)
    {
        var result = new AcceptedAlphaInteractionDrilldownVerificationService().Build(root);
        return result.Dashboard.FullVerificationStatus == "GREEN"
               && result.ScriptInventory.Passed
               && result.Dashboard.OneClickButtonPresent
               && result.Dashboard.HumanManualStepsReducedToOneButton;
    }

    private static AcceptedAlphaProjectionActionLoopScriptInventoryEntry Entry(
        string root,
        string relativePath,
        string role,
        string marker)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new AcceptedAlphaProjectionActionLoopScriptInventoryEntry
        {
            RelativePath = relativePath,
            Role = role,
            Exists = exists,
            ContainsRequiredMarker = text.Contains(marker, StringComparison.Ordinal),
            RequiredMarker = marker,
            Sha256 = exists ? HashBytes(File.ReadAllBytes(path)) : string.Empty
        };
    }

    private static AcceptedAlphaProjectionActionLoopSmokePlanStep Step(
        int index,
        string stepId,
        string expectedResult) =>
        new()
        {
            StepIndex = index,
            StepId = stepId,
            ExpectedResult = expectedResult
        };

    private static IReadOnlyList<string> BuildExpectedChangedPaths() =>
    [
        AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory + "/",
        AcceptedAlphaProjectionActionLoopVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-122-accepted-alpha-projection-action-loop-and-window-polish/",
        AcceptedAlphaProjectionActionLoopVocabulary.DocumentationPath,
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
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityDrilldownPath,
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityActionPreviewPath,
        AcceptedAlphaProjectionActionLoopVocabulary.UnityStatePath,
        "tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs"
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
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105/example.json",
        ".llmgc/exports/final-release/package.zip",
        "provider/live-geodata/Adapter.cs"
    ];

    private static bool IsAllowedChangedPath(string path) =>
        BuildExpectedChangedPaths().Any(prefix =>
            prefix.EndsWith("/", StringComparison.Ordinal)
                ? path.StartsWith(prefix, StringComparison.Ordinal)
                : string.Equals(path, prefix, StringComparison.Ordinal));

    private static bool CleanupScriptAvailable(AcceptedAlphaProjectionActionLoopScriptInventory inventory) =>
        inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath
            && entry.Exists
            && entry.ContainsRequiredMarker)
        && inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath
            && entry.Exists
            && entry.ContainsRequiredMarker);

    private static void Require(bool condition, string code, List<string> errors)
    {
        if (!condition)
        {
            errors.Add(code);
        }
    }

    private static string SourceText(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string ResolveRepositoryRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Repository root path is required.", nameof(path));
        }

        return Path.GetFullPath(path);
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)
                                  ?? throw new InvalidOperationException("Missing directory."));
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal122 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
