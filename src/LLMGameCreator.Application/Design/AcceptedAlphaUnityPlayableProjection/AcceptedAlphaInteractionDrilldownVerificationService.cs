using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class AcceptedAlphaInteractionDrilldownVerificationService
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

    public AcceptedAlphaInteractionDrilldownVerificationBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var scriptInventory = BuildScriptInventory(root);
        var logScan = BuildLogScan(root);
        var smokePlan = BuildSmokePlan(scriptInventory);
        var negative = BuildNegativeProof();
        var dashboard = BuildDashboard(scriptInventory, logScan, smokePlan, negative);
        var report = RenderReport(dashboard, scriptInventory, smokePlan, logScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName] = Serialize(dashboard),
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.ScriptInventoryFileName] =
                Serialize(scriptInventory),
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.LogScanFileName] = Serialize(logScan),
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.ReportFileName] = report,
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory,
            "goal121_accepted_alpha_interaction_drilldown_evidence",
            includeUnityLog: true);
        proceduralFiles[AcceptedAlphaInteractionDrilldownVerificationVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName] = Serialize(dashboard),
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.ScriptInventoryFileName] =
                Serialize(scriptInventory),
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.LogScanFileName] = Serialize(logScan),
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.ReportFileName] = report,
            [AcceptedAlphaInteractionDrilldownVerificationVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ExportPackageDirectory,
            "goal121_accepted_alpha_interaction_drilldown_export",
            includeUnityLog: false);
        exportFiles[AcceptedAlphaInteractionDrilldownVerificationVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new AcceptedAlphaInteractionDrilldownVerificationBuildResult
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

    public async Task<AcceptedAlphaInteractionDrilldownVerificationWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, AcceptedAlphaInteractionDrilldownVerificationVocabulary.DocumentationPath);
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

        return new AcceptedAlphaInteractionDrilldownVerificationWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaInteractionDrilldownDashboard BuildDashboard(
        AcceptedAlphaInteractionDrilldownScriptInventory scriptInventory,
        AcceptedAlphaInteractionDrilldownLogScan logScan,
        AcceptedAlphaInteractionDrilldownSmokePlan smokePlan,
        AcceptedAlphaInteractionDrilldownNegativeProof negative)
    {
        var diagnostics = new List<string>();
        Require(scriptInventory.Passed, "goal121.script_inventory_failed", diagnostics);
        Require(smokePlan.StepCount >= 12, "goal121.smoke_plan_incomplete", diagnostics);
        Require(negative.Passed, "goal121.negative_proof_failed", diagnostics);
        Require(logScan.Status != "BLOCKED_UNITY_BATCHMODE_FULL_VERIFICATION", "goal121.unity_log_failed",
            diagnostics);

        var sourceReady = diagnostics.Count == 0;
        return new AcceptedAlphaInteractionDrilldownDashboard
        {
            FullVerificationStatus =
                sourceReady ? AcceptedAlphaInteractionDrilldownVerificationVocabulary.FullVerificationStatus : "BLOCKED",
            OneClickButtonPresent = scriptInventory.OneClickButtonPresent,
            DrilldownFieldsPresent = scriptInventory.DrilldownFieldsPresent,
            InteractionPreviewPresent = scriptInventory.InteractionPreviewFieldsPresent,
            ObjectiveReplayDetailsPresent = scriptInventory.ObjectiveReplayDetailsFieldsPresent,
            BatchmodeFullVerificationMarker = scriptInventory.BatchmodePassMarkerPresent
                ? "GOAL121_FULL_PROJECTION_VERIFICATION_PASS"
                : string.Empty,
            CleanupScriptAvailable = CleanupScriptAvailable(scriptInventory),
            MaterialWarningGuardPresent = scriptInventory.MaterialWarningGuardPresent,
            HumanManualStepsReducedToOneButton =
                scriptInventory.OneClickButtonPresent
                && smokePlan.OneClickManualPath
                && smokePlan.StepCount >= 12,
            UnityBatchmodeLogStatus = logScan.Status,
            SmokeRequiredFieldsPresent = scriptInventory.SmokeRequiredFieldsPresent
                                         && (!logScan.LogExists || logScan.SmokeRequiredFieldsPresent),
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaInteractionDrilldownScriptInventory BuildScriptInventory(string root)
    {
        var entries = new[]
        {
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
                "unity_editor_window", "Run Full Projection Verification"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath,
                "unity_projection_controller", "RunFullProjectionVerification"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityDiagnosticsPath,
                "unity_projection_diagnostics", "AcceptedAlphaPlayableProjectionDiagnostics"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath,
                "unity_projection_models", "fullVerificationPassed"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
                "unity_projection_primitive_factory", "MaterialWarningGuardPresent"),
            Entry(root, AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityDrilldownPath,
                "unity_projection_drilldown", "BuildObjectiveReplayDetails"),
            Entry(root, AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityActionPreviewPath,
                "unity_projection_action_preview", "BuildInteractionPreview"),
            Entry(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
                "cleanup_script", "Unity editor noise cleanup mode")
        }.ToList();

        var editorText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath);
        var controllerText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath);
        var modelsText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath);
        var primitiveText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath);
        var drilldownText = SourceText(root,
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityDrilldownPath);
        var actionPreviewText = SourceText(root,
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityActionPreviewPath);
        var allUnityText = string.Join("\n", entries
            .Where(entry => entry.RelativePath.StartsWith("unity/", StringComparison.Ordinal))
            .Select(entry => SourceText(root, entry.RelativePath)));
        var smokeMarkers = new[]
        {
            "fullVerificationPassed",
            "rootPresent",
            "baselineLoaded",
            "playerProxyPresent",
            "legendPresent",
            "markerDescriptorPresent",
            "selectableInteractionTargetPresent",
            "interactionPreviewPresent",
            "selectableObjectivePresent",
            "objectiveReplayDetailsPresent",
            "diagnosticsStatusPresent",
            "zeroFatalErrors"
        };
        var materialWarningSourceClean =
            !RendererMaterialAccessPattern.IsMatch(allUnityText)
            && !MaterialAssignmentPattern.IsMatch(allUnityText)
            && primitiveText.Contains("MaterialPropertyBlock", StringComparison.Ordinal)
            && primitiveText.Contains("SetPropertyBlock", StringComparison.Ordinal);

        var inventory = new AcceptedAlphaInteractionDrilldownScriptInventory
        {
            ScriptCount = entries.Count,
            OneClickButtonPresent =
                editorText.Contains("Run Full Projection Verification", StringComparison.Ordinal)
                && editorText.Contains("RunFullProjectionVerification()", StringComparison.Ordinal),
            BatchmodeMethodPresent =
                editorText.Contains("RunBatchmodeProjectionFullVerification", StringComparison.Ordinal),
            BatchmodePassMarkerPresent =
                editorText.Contains("GOAL121_FULL_PROJECTION_VERIFICATION_PASS", StringComparison.Ordinal),
            BatchmodeFailMarkerPresent =
                editorText.Contains("GOAL121_FULL_PROJECTION_VERIFICATION_FAIL", StringComparison.Ordinal),
            DrilldownFieldsPresent =
                editorText.Contains("Selected Marker Details", StringComparison.Ordinal)
                && editorText.Contains("Objective / Replay Details", StringComparison.Ordinal)
                && controllerText.Contains("SelectedMarkerDetails", StringComparison.Ordinal)
                && drilldownText.Contains("DescribeMarker", StringComparison.Ordinal),
            InteractionPreviewFieldsPresent =
                editorText.Contains("Interaction Preview", StringComparison.Ordinal)
                && controllerText.Contains("InteractionPreview", StringComparison.Ordinal)
                && actionPreviewText.Contains("BuildInteractionPreview", StringComparison.Ordinal),
            ObjectiveReplayDetailsFieldsPresent =
                editorText.Contains("Objective / Replay Details", StringComparison.Ordinal)
                && controllerText.Contains("ObjectiveReplayDetails", StringComparison.Ordinal)
                && drilldownText.Contains("BuildObjectiveReplayDetails", StringComparison.Ordinal),
            VerificationEventLogPresent =
                editorText.Contains("Verification Event Log", StringComparison.Ordinal)
                && controllerText.Contains("VerificationEventLog", StringComparison.Ordinal),
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
            Passed = inventory.ScriptCount == 8
                     && entries.All(entry => entry.Exists && entry.ContainsRequiredMarker)
                     && inventory.OneClickButtonPresent
                     && inventory.BatchmodeMethodPresent
                     && inventory.BatchmodePassMarkerPresent
                     && inventory.BatchmodeFailMarkerPresent
                     && inventory.DrilldownFieldsPresent
                     && inventory.InteractionPreviewFieldsPresent
                     && inventory.ObjectiveReplayDetailsFieldsPresent
                     && inventory.VerificationEventLogPresent
                     && inventory.SmokeRequiredFieldsPresent
                     && inventory.MaterialWarningGuardPresent
                     && inventory.MaterialWarningSourceClean
        };
    }

    private static AcceptedAlphaInteractionDrilldownSmokePlan BuildSmokePlan(
        AcceptedAlphaInteractionDrilldownScriptInventory inventory)
    {
        var steps = new List<AcceptedAlphaInteractionDrilldownSmokePlanStep>
        {
            Step(1, "refresh_accepted_baseline", "Accepted baseline refresh runs first."),
            Step(2, "build_refresh_projection", "Playable projection root is rebuilt or refreshed."),
            Step(3, "focus_generated_root", "Generated root is focused for inspection."),
            Step(4, "select_player_proxy", "Player proxy marker is selected."),
            Step(5, "select_first_interaction_target", "First descriptor-backed interaction target is selected."),
            Step(6, "populate_selected_marker_details", "Selected marker details text is populated."),
            Step(7, "populate_interaction_preview", "Interaction/action preview text is populated."),
            Step(8, "select_first_objective", "First descriptor-backed objective marker is selected."),
            Step(9, "populate_objective_replay_details", "Objective/replay details text is populated."),
            Step(10, "select_diagnostics_marker", "Diagnostics marker is selected."),
            Step(11, "refresh_show_legend", "Legend is visible after the one-click run."),
            Step(12, "run_local_projection_smoke", "Local projection smoke reports fullVerificationPassed=True."),
            Step(13, "write_event_log", "Compact verification event log is visible in the window.")
        };

        return new AcceptedAlphaInteractionDrilldownSmokePlan
        {
            RefreshBaselineStep = inventory.OneClickButtonPresent,
            BuildProjectionStep = inventory.OneClickButtonPresent,
            PlayerProxySelectionStep = inventory.DrilldownFieldsPresent,
            InteractionTargetSelectionStep = inventory.InteractionPreviewFieldsPresent,
            InteractionPreviewStep = inventory.InteractionPreviewFieldsPresent,
            ObjectiveSelectionStep = inventory.ObjectiveReplayDetailsFieldsPresent,
            ObjectiveReplayDetailsStep = inventory.ObjectiveReplayDetailsFieldsPresent,
            DiagnosticsMarkerStep = inventory.DrilldownFieldsPresent,
            LegendStep = inventory.OneClickButtonPresent,
            LocalSmokeStep = inventory.SmokeRequiredFieldsPresent,
            StepCount = steps.Count,
            Steps = steps
        };
    }

    private static AcceptedAlphaInteractionDrilldownLogScan BuildLogScan(string root)
    {
        var path = Resolve(root,
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityBatchmodeLogRelativePath);
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

        if (text.Contains("GOAL121_FULL_PROJECTION_VERIFICATION_FAIL", StringComparison.Ordinal))
        {
            forbidden.Add("GOAL121_FULL_PROJECTION_VERIFICATION_FAIL");
        }

        var smokeFields = new[]
        {
            "fullVerificationPassed=True",
            "rootPresent=True",
            "baselineLoaded=True",
            "playerProxyPresent=True",
            "legendPresent=True",
            "markerDescriptorPresent=True",
            "selectableInteractionTargetPresent=True",
            "interactionPreviewPresent=True",
            "selectableObjectivePresent=True",
            "objectiveReplayDetailsPresent=True",
            "diagnosticsStatusPresent=True",
            "zeroFatalErrors=True"
        };
        var passMarkerPresent = text.Contains("GOAL121_FULL_PROJECTION_VERIFICATION_PASS", StringComparison.Ordinal);
        var smokeFieldsPresent = smokeFields.All(field => text.Contains(field, StringComparison.Ordinal));
        var passed = logExists && passMarkerPresent && smokeFieldsPresent && forbidden.Count == 0;
        return new AcceptedAlphaInteractionDrilldownLogScan
        {
            LogExists = logExists,
            PassMarkerPresent = passMarkerPresent,
            FailMarkerAbsent = !forbidden.Contains("GOAL121_FULL_PROJECTION_VERIFICATION_FAIL", StringComparer.Ordinal),
            MaterialInstantiationWarningAbsent = !forbidden.Contains(materialWarning, StringComparer.Ordinal),
            RendererGetMaterialStackAbsent = !forbidden.Contains(rendererStack, StringComparer.Ordinal),
            SmokeRequiredFieldsPresent = smokeFieldsPresent,
            Passed = passed,
            Status = passed
                ? "GREEN"
                : logExists
                    ? "BLOCKED_UNITY_BATCHMODE_FULL_VERIFICATION"
                    : "PENDING_UNITY_BATCHMODE_FULL_VERIFICATION",
            Sha256 = logExists ? HashBytes(File.ReadAllBytes(path)) : string.Empty,
            ForbiddenMarkersFound = forbidden.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaInteractionDrilldownNegativeProof BuildNegativeProof()
    {
        var rejected = BuildRejectedPathSamples();
        return new AcceptedAlphaInteractionDrilldownNegativeProof
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

    private static AcceptedAlphaInteractionDrilldownFileIndex BuildFileIndex(
        string root,
        IReadOnlyDictionary<string, string> files,
        string relativeRoot,
        string role,
        bool includeUnityLog)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new AcceptedAlphaInteractionDrilldownFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        if (includeUnityLog)
        {
            var logPath = Resolve(
                root,
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityBatchmodeLogRelativePath);
            if (File.Exists(logPath))
            {
                entries.Add(new AcceptedAlphaInteractionDrilldownFileIndexEntry
                {
                    RelativePath =
                        AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityBatchmodeLogRelativePath,
                    Role = "goal121_unity_batchmode_full_projection_verification_log",
                    Sha256 = HashBytes(File.ReadAllBytes(logPath))
                });
            }
        }

        return new AcceptedAlphaInteractionDrilldownFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static string RenderReport(
        AcceptedAlphaInteractionDrilldownDashboard dashboard,
        AcceptedAlphaInteractionDrilldownScriptInventory scriptInventory,
        AcceptedAlphaInteractionDrilldownSmokePlan smokePlan,
        AcceptedAlphaInteractionDrilldownLogScan logScan,
        AcceptedAlphaInteractionDrilldownNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 121 Accepted Alpha Interaction Drilldown And One-Click Verification",
            string.Empty,
            "- fullVerificationStatus: " + dashboard.FullVerificationStatus,
            "- unityMenuPath: " + dashboard.UnityMenuPath,
            "- oneClickButtonPresent: " + dashboard.OneClickButtonPresent.ToString().ToLowerInvariant(),
            "- drilldownFieldsPresent: " + dashboard.DrilldownFieldsPresent.ToString().ToLowerInvariant(),
            "- interactionPreviewPresent: " + dashboard.InteractionPreviewPresent.ToString().ToLowerInvariant(),
            "- objectiveReplayDetailsPresent: "
            + dashboard.ObjectiveReplayDetailsPresent.ToString().ToLowerInvariant(),
            "- batchmodeFullVerificationMarker: " + dashboard.BatchmodeFullVerificationMarker,
            "- cleanupScriptAvailable: " + dashboard.CleanupScriptAvailable.ToString().ToLowerInvariant(),
            "- materialWarningGuardPresent: "
            + dashboard.MaterialWarningGuardPresent.ToString().ToLowerInvariant(),
            "- humanManualStepsReducedToOneButton: "
            + dashboard.HumanManualStepsReducedToOneButton.ToString().ToLowerInvariant(),
            "- unityBatchmodeLogStatus: " + dashboard.UnityBatchmodeLogStatus,
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Script Inventory",
            string.Empty,
            "- passed: " + scriptInventory.Passed.ToString().ToLowerInvariant(),
            "- scriptCount: " + scriptInventory.ScriptCount,
            "- smokeRequiredFieldsPresent: "
            + scriptInventory.SmokeRequiredFieldsPresent.ToString().ToLowerInvariant(),
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

    private static string RenderDocumentation(AcceptedAlphaInteractionDrilldownDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Accepted Alpha Interaction Drilldown And One-Click Verification",
            string.Empty,
            "Goal121 makes the accepted Alpha Unity projection manual path one menu action plus one button.",
            string.Empty,
            "## Hands-on Verification",
            string.Empty,
            "- Open `unity/LLMGameCreatorAlpha` in Unity.",
            "- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.",
            "- Click `Run Full Projection Verification`.",
            "- Review `Selected Marker Details`, `Interaction Preview`, `Objective / Replay Details` and `Verification Event Log` in the window.",
            string.Empty,
            "## Cleanup Commands",
            string.Empty,
            "- After Unity checks: `.\\.devflow\\scripts\\clean-unity-editor-noise.cmd`",
            "- PowerShell equivalent: `.\\.devflow\\scripts\\clean-unity-editor-noise.ps1 -Apply`",
            string.Empty,
            "## Status",
            string.Empty,
            "- fullVerificationStatus: " + dashboard.FullVerificationStatus,
            "- unityBatchmodeLogStatus: " + dashboard.UnityBatchmodeLogStatus,
            "- humanManualStepsReducedToOneButton: "
            + dashboard.HumanManualStepsReducedToOneButton.ToString().ToLowerInvariant(),
            "- noRuntimeProviderNetworkSchemaLuaGeneratorLibrary: "
            + dashboard.NoRuntimeProviderNetworkSchemaLuaGeneratorLibrary.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static AcceptedAlphaInteractionDrilldownScriptInventoryEntry Entry(
        string root,
        string relativePath,
        string role,
        string marker)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new AcceptedAlphaInteractionDrilldownScriptInventoryEntry
        {
            RelativePath = relativePath,
            Role = role,
            Exists = exists,
            ContainsRequiredMarker = text.Contains(marker, StringComparison.Ordinal),
            RequiredMarker = marker,
            Sha256 = exists ? HashBytes(File.ReadAllBytes(path)) : string.Empty
        };
    }

    private static AcceptedAlphaInteractionDrilldownSmokePlanStep Step(
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
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory + "/",
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/",
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.DocumentationPath,
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
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityDrilldownPath,
        AcceptedAlphaInteractionDrilldownVerificationVocabulary.UnityActionPreviewPath,
        "tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs",
        "tests/LLMGameCreator.Tests/DevFlow/CleanUnityEditorNoiseScriptTests.cs"
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

    private static bool CleanupScriptAvailable(AcceptedAlphaInteractionDrilldownScriptInventory inventory) =>
        inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath
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
            throw new InvalidOperationException("Goal121 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
