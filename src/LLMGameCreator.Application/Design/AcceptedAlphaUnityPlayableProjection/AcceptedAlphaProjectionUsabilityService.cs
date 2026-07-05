using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class AcceptedAlphaProjectionUsabilityService
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

    public AcceptedAlphaProjectionUsabilityBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var scriptInventory = BuildScriptInventory(root);
        var cleanupScriptScan = BuildCleanupScriptScan(root);
        var smokePlan = BuildSmokePlan(scriptInventory);
        var negative = BuildNegativeProof();
        var goal119ARemainsGreen = Goal119ARemainsGreen(root);
        var unitySmokeStatus = BuildUnitySmokeStatus(root);
        var dashboard = BuildDashboard(
            scriptInventory,
            cleanupScriptScan,
            smokePlan,
            negative,
            goal119ARemainsGreen,
            unitySmokeStatus);
        var report = RenderReport(dashboard, scriptInventory, smokePlan, cleanupScriptScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName] = Serialize(dashboard),
            [AcceptedAlphaProjectionUsabilityVocabulary.ScriptInventoryFileName] = Serialize(scriptInventory),
            [AcceptedAlphaProjectionUsabilityVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptScanFileName] = Serialize(cleanupScriptScan),
            [AcceptedAlphaProjectionUsabilityVocabulary.ReportFileName] = report,
            [AcceptedAlphaProjectionUsabilityVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory,
            "goal120_accepted_alpha_projection_usability_evidence",
            includeUnityLog: true);
        proceduralFiles[AcceptedAlphaProjectionUsabilityVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName] = Serialize(dashboard),
            [AcceptedAlphaProjectionUsabilityVocabulary.ScriptInventoryFileName] = Serialize(scriptInventory),
            [AcceptedAlphaProjectionUsabilityVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptScanFileName] = Serialize(cleanupScriptScan),
            [AcceptedAlphaProjectionUsabilityVocabulary.ReportFileName] = report,
            [AcceptedAlphaProjectionUsabilityVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            AcceptedAlphaProjectionUsabilityVocabulary.ExportPackageDirectory,
            "goal120_accepted_alpha_projection_usability_export",
            includeUnityLog: false);
        exportFiles[AcceptedAlphaProjectionUsabilityVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new AcceptedAlphaProjectionUsabilityBuildResult
        {
            Dashboard = dashboard,
            ScriptInventory = scriptInventory,
            SmokePlan = smokePlan,
            CleanupScriptScan = cleanupScriptScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public async Task<AcceptedAlphaProjectionUsabilityWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(root, AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory);
        var export = Resolve(root, AcceptedAlphaProjectionUsabilityVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, AcceptedAlphaProjectionUsabilityVocabulary.DocumentationPath);
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

        return new AcceptedAlphaProjectionUsabilityWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaProjectionUsabilityDashboard BuildDashboard(
        AcceptedAlphaProjectionUsabilityScriptInventory scriptInventory,
        AcceptedAlphaProjectionCleanupScriptScan cleanupScriptScan,
        AcceptedAlphaProjectionUsabilitySmokePlan smokePlan,
        AcceptedAlphaProjectionUsabilityNegativeProof negative,
        bool goal119ARemainsGreen,
        string unitySmokeStatus)
    {
        var diagnostics = new List<string>();
        Require(goal119ARemainsGreen, "goal120.goal119a_not_green", diagnostics);
        Require(scriptInventory.Passed, "goal120.script_inventory_failed", diagnostics);
        Require(cleanupScriptScan.Passed, "goal120.cleanup_script_contract_failed", diagnostics);
        Require(smokePlan.StepCount >= 8, "goal120.smoke_plan_incomplete", diagnostics);
        Require(negative.Passed, "goal120.negative_proof_failed", diagnostics);

        var passed = diagnostics.Count == 0;
        return new AcceptedAlphaProjectionUsabilityDashboard
        {
            UsabilityStatus = passed ? "GREEN" : "BLOCKED",
            Goal119ARemainsGreen = goal119ARemainsGreen,
            LegendPresent = scriptInventory.LegendPresent,
            MarkerDescriptorPresent = scriptInventory.MarkerDescriptorPresent,
            SelectionControlsPresent = scriptInventory.SelectionControlsPresent,
            FocusCameraControlPresent = scriptInventory.FocusCameraControlPresent,
            MaterialWarningGuardPresent = scriptInventory.MaterialWarningGuardPresent,
            CleanupScriptContractPassed = cleanupScriptScan.Passed,
            UnitySmokeStatus = unitySmokeStatus,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaProjectionUsabilityScriptInventory BuildScriptInventory(string root)
    {
        var entries = new[]
        {
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
                "unity_editor_window", "RunBatchmodeProjectionUsabilitySmoke"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath,
                "unity_projection_controller", "goal120_legend"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityDiagnosticsPath,
                "unity_projection_diagnostics", "AcceptedAlphaPlayableProjectionDiagnostics"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath,
                "unity_projection_models", "AcceptedAlphaPlayableProjectionMarkerDescriptor"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
                "unity_projection_primitive_factory", "MaterialWarningGuardPresent")
        }.ToList();

        var editorText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath);
        var controllerText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath);
        var modelsText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath);
        var primitiveText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath);
        var allSourceText = string.Join("\n", entries.Select(entry =>
            SourceText(root, entry.RelativePath)));
        var controls = new[]
        {
            "Focus Projection Camera",
            "Select Player Proxy",
            "Select Next Interaction Target",
            "Select Next Objective",
            "Select Diagnostics Marker",
            "Toggle/Refresh Legend"
        };
        var materialWarningSourceClean =
            !RendererMaterialAccessPattern.IsMatch(allSourceText)
            && !MaterialAssignmentPattern.IsMatch(allSourceText)
            && primitiveText.Contains("MaterialPropertyBlock", StringComparison.Ordinal)
            && primitiveText.Contains("SetPropertyBlock", StringComparison.Ordinal);

        var inventory = new AcceptedAlphaProjectionUsabilityScriptInventory
        {
            ScriptCount = entries.Count,
            UnityMenuPathPresent = editorText.Contains(
                AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
                StringComparison.Ordinal),
            BatchmodeMethodPresent = editorText.Contains(
                "RunBatchmodeProjectionUsabilitySmoke",
                StringComparison.Ordinal),
            BatchmodePassMarkerPresent = editorText.Contains(
                "GOAL120_PROJECTION_USABILITY_SMOKE_PASS",
                StringComparison.Ordinal),
            LegendPresent = controllerText.Contains("goal120_legend", StringComparison.Ordinal)
                            && modelsText.Contains("AcceptedAlphaPlayableProjectionLegend",
                                StringComparison.Ordinal),
            MarkerDescriptorPresent =
                modelsText.Contains("AcceptedAlphaPlayableProjectionMarkerDescriptor",
                    StringComparison.Ordinal)
                && controllerText.Contains("AttachDescriptor(", StringComparison.Ordinal),
            SelectionControlsPresent = controls.All(control =>
                editorText.Contains(control, StringComparison.Ordinal))
                && controllerText.Contains("FindNextMarkerByKind", StringComparison.Ordinal),
            FocusCameraControlPresent = editorText.Contains("Focus Projection Camera",
                StringComparison.Ordinal)
                && editorText.Contains("SceneView.FrameLastActiveSceneView",
                    StringComparison.Ordinal),
            MaterialWarningGuardPresent =
                primitiveText.Contains("MaterialWarningGuardPresent", StringComparison.Ordinal)
                && primitiveText.Contains("MaterialPropertyBlock", StringComparison.Ordinal),
            MaterialWarningSourceClean = materialWarningSourceClean,
            Scripts = entries
        };

        return inventory with
        {
            Passed = inventory.ScriptCount == 5
                     && entries.All(entry => entry.Exists && entry.ContainsRequiredMarker)
                     && inventory.UnityMenuPathPresent
                     && inventory.BatchmodeMethodPresent
                     && inventory.BatchmodePassMarkerPresent
                     && inventory.LegendPresent
                     && inventory.MarkerDescriptorPresent
                     && inventory.SelectionControlsPresent
                     && inventory.FocusCameraControlPresent
                     && inventory.MaterialWarningGuardPresent
                     && inventory.MaterialWarningSourceClean
        };
    }

    private static AcceptedAlphaProjectionCleanupScriptScan BuildCleanupScriptScan(string root)
    {
        var psPath = Resolve(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath);
        var cmdPath = Resolve(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath);
        var psExists = File.Exists(psPath);
        var cmdExists = File.Exists(cmdPath);
        var psText = psExists ? File.ReadAllText(psPath, Encoding.UTF8) : string.Empty;
        var cmdText = cmdExists ? File.ReadAllText(cmdPath, Encoding.UTF8) : string.Empty;
        var scan = new AcceptedAlphaProjectionCleanupScriptScan
        {
            PowerShellScriptExists = psExists,
            CmdWrapperExists = cmdExists,
            DryRunDefaultPresent = psText.Contains("-not $DryRun -and -not $Apply", StringComparison.Ordinal),
            ApplySwitchPresent = psText.Contains("[switch]$Apply", StringComparison.Ordinal),
            AllowStagedSwitchPresent = psText.Contains("[switch]$AllowStaged", StringComparison.Ordinal),
            GitStatusPorcelainAllPresent =
                psText.Contains("git status --porcelain=v1 --untracked-files=all", StringComparison.Ordinal),
            RefusesStagedByDefault = psText.Contains("Refusing cleanup because staged files are present",
                StringComparison.Ordinal),
            RemovesOnlyAllowedUnityNoise =
                psText.Contains(".meta", StringComparison.Ordinal)
                && psText.Contains("Packages/packages-lock.json", StringComparison.Ordinal)
                && psText.Contains("ProjectSettings/", StringComparison.Ordinal)
                && psText.Contains(".asset", StringComparison.Ordinal),
            RestoresOnlyProjectVersion =
                psText.Contains("git restore -- $ProjectVersionPath", StringComparison.Ordinal)
                && psText.Contains("ProjectVersion.txt", StringComparison.Ordinal),
            NeverRemoveSourceOrPayloadExtensions =
                psText.Contains("\".cs\"", StringComparison.Ordinal)
                && psText.Contains("\".json\"", StringComparison.Ordinal)
                && psText.Contains("\".md\"", StringComparison.Ordinal)
                && psText.Contains("\".unity\"", StringComparison.Ordinal)
                && psText.Contains("\".prefab\"", StringComparison.Ordinal),
            NoBroadGitClean =
                !psText.Contains("git clean -fd -- unity/LLMGameCreatorAlpha/Assets",
                    StringComparison.Ordinal),
            PowerShellSha256 = psExists ? HashBytes(File.ReadAllBytes(psPath)) : string.Empty,
            CmdSha256 = cmdExists ? HashBytes(File.ReadAllBytes(cmdPath)) : string.Empty
        };

        return scan with
        {
            Passed = scan.PowerShellScriptExists
                     && scan.CmdWrapperExists
                     && scan.DryRunDefaultPresent
                     && scan.ApplySwitchPresent
                     && scan.AllowStagedSwitchPresent
                     && scan.GitStatusPorcelainAllPresent
                     && scan.RefusesStagedByDefault
                     && scan.RemovesOnlyAllowedUnityNoise
                     && scan.RestoresOnlyProjectVersion
                     && scan.NeverRemoveSourceOrPayloadExtensions
                     && scan.NoBroadGitClean
                     && cmdText.Contains("clean-unity-editor-noise.ps1", StringComparison.Ordinal)
                     && cmdText.Contains("-Apply", StringComparison.Ordinal)
        };
    }

    private static AcceptedAlphaProjectionUsabilitySmokePlan BuildSmokePlan(
        AcceptedAlphaProjectionUsabilityScriptInventory inventory)
    {
        var steps = new List<AcceptedAlphaProjectionUsabilitySmokePlanStep>
        {
            Step(1, "root_present", "Generated root is found by exact name."),
            Step(2, "player_proxy_selectable", "Select Player Proxy selects a descriptor-backed player object."),
            Step(3, "legend_visible", "Legend is present under the generated root."),
            Step(4, "marker_descriptor_present", "At least one marker descriptor is attached."),
            Step(5, "interaction_target_selectable", "Select Next Interaction Target resolves a marker."),
            Step(6, "objective_selectable", "Select Next Objective resolves a marker."),
            Step(7, "diagnostics_marker_selectable", "Select Diagnostics Marker resolves diagnostics text."),
            Step(8, "material_warning_guard", "Material property blocks remain the color path.")
        };
        return new AcceptedAlphaProjectionUsabilitySmokePlan
        {
            PlayerProxySelectionCheck = inventory.SelectionControlsPresent,
            LegendCheck = inventory.LegendPresent,
            MarkerDescriptorCheck = inventory.MarkerDescriptorPresent,
            InteractionSelectionCheck = inventory.SelectionControlsPresent,
            ObjectiveSelectionCheck = inventory.SelectionControlsPresent,
            DiagnosticsMarkerSelectionCheck = inventory.SelectionControlsPresent,
            MaterialWarningGuardCheck = inventory.MaterialWarningGuardPresent,
            StepCount = steps.Count,
            Steps = steps
        };
    }

    private static AcceptedAlphaProjectionUsabilityNegativeProof BuildNegativeProof()
    {
        var rejected = BuildRejectedPathSamples();
        return new AcceptedAlphaProjectionUsabilityNegativeProof
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

    private static bool Goal119ARemainsGreen(string root)
    {
        using var dashboard = LoadJson(
            root,
            AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ProceduralOutputDirectory
            + "/"
            + AcceptedAlphaUnityMaterialWarningHotfixVocabulary.DashboardFileName);
        return dashboard is not null
               && StringProperty(dashboard.RootElement, "implementationStatus") == "GREEN"
               && StringProperty(dashboard.RootElement, "unitySmokeStatus") == "GREEN"
               && TryGetBool(dashboard.RootElement, "materialWarningAbsent")
               && TryGetBool(dashboard.RootElement, "materialPropertyBlockUsed");
    }

    private static string BuildUnitySmokeStatus(string root)
    {
        var path = Resolve(root, AcceptedAlphaProjectionUsabilityVocabulary.UnityBatchmodeLogRelativePath);
        if (!File.Exists(path))
        {
            return "PENDING_UNITY_BATCHMODE_SMOKE";
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        var materialWarning =
            "Instantiating material due to calling renderer" + ".material during edit mode";
        var forbidden = text.Contains("GOAL120_PROJECTION_USABILITY_SMOKE_FAIL", StringComparison.Ordinal)
                        || text.Contains(materialWarning, StringComparison.Ordinal)
                        || text.Contains("UnityEngine.Renderer:get_material()", StringComparison.Ordinal);
        return text.Contains("GOAL120_PROJECTION_USABILITY_SMOKE_PASS", StringComparison.Ordinal)
               && !forbidden
            ? "GREEN"
            : "BLOCKED_UNITY_BATCHMODE_SMOKE";
    }

    private static AcceptedAlphaProjectionUsabilityFileIndex BuildFileIndex(
        string root,
        IReadOnlyDictionary<string, string> files,
        string relativeRoot,
        string role,
        bool includeUnityLog)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new AcceptedAlphaProjectionUsabilityFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        if (includeUnityLog)
        {
            var logPath = Resolve(root, AcceptedAlphaProjectionUsabilityVocabulary.UnityBatchmodeLogRelativePath);
            if (File.Exists(logPath))
            {
                entries.Add(new AcceptedAlphaProjectionUsabilityFileIndexEntry
                {
                    RelativePath = AcceptedAlphaProjectionUsabilityVocabulary.UnityBatchmodeLogRelativePath,
                    Role = "goal120_unity_batchmode_projection_usability_smoke_log",
                    Sha256 = HashBytes(File.ReadAllBytes(logPath))
                });
            }
        }

        return new AcceptedAlphaProjectionUsabilityFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static string RenderReport(
        AcceptedAlphaProjectionUsabilityDashboard dashboard,
        AcceptedAlphaProjectionUsabilityScriptInventory scriptInventory,
        AcceptedAlphaProjectionUsabilitySmokePlan smokePlan,
        AcceptedAlphaProjectionCleanupScriptScan cleanupScriptScan,
        AcceptedAlphaProjectionUsabilityNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 120 Accepted Alpha Projection Usability And Cleanup",
            string.Empty,
            "- usabilityStatus: " + dashboard.UsabilityStatus,
            "- unityMenuPath: " + dashboard.UnityMenuPath,
            "- cleanupScriptPath: " + dashboard.CleanupScriptPath,
            "- cleanupScriptCmdPath: " + dashboard.CleanupScriptCmdPath,
            "- legendPresent: " + dashboard.LegendPresent.ToString().ToLowerInvariant(),
            "- markerDescriptorPresent: " + dashboard.MarkerDescriptorPresent.ToString().ToLowerInvariant(),
            "- selectionControlsPresent: " + dashboard.SelectionControlsPresent.ToString().ToLowerInvariant(),
            "- focusCameraControlPresent: " + dashboard.FocusCameraControlPresent.ToString().ToLowerInvariant(),
            "- materialWarningGuardPresent: " + dashboard.MaterialWarningGuardPresent.ToString().ToLowerInvariant(),
            "- cleanupScriptContractPassed: " + dashboard.CleanupScriptContractPassed.ToString().ToLowerInvariant(),
            "- goal119aRemainsGreen: " + dashboard.Goal119ARemainsGreen.ToString().ToLowerInvariant(),
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- doNotStartAutomatically: " + dashboard.DoNotStartAutomatically.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Script Inventory",
            string.Empty,
            "- passed: " + scriptInventory.Passed.ToString().ToLowerInvariant(),
            "- scriptCount: " + scriptInventory.ScriptCount,
            string.Empty,
            "## Smoke Plan",
            string.Empty,
            "- stepCount: " + smokePlan.StepCount,
            string.Empty,
            "## Cleanup Script",
            string.Empty,
            "- passed: " + cleanupScriptScan.Passed.ToString().ToLowerInvariant(),
            "- dryRunDefaultPresent: " + cleanupScriptScan.DryRunDefaultPresent.ToString().ToLowerInvariant(),
            "- applySwitchPresent: " + cleanupScriptScan.ApplySwitchPresent.ToString().ToLowerInvariant(),
            "- allowStagedSwitchPresent: " + cleanupScriptScan.AllowStagedSwitchPresent.ToString().ToLowerInvariant(),
            "- gitStatusPorcelainAllPresent: "
            + cleanupScriptScan.GitStatusPorcelainAllPresent.ToString().ToLowerInvariant(),
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

    private static string RenderDocumentation(AcceptedAlphaProjectionUsabilityDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Accepted Alpha Projection Usability And Cleanup",
            string.Empty,
            "Goal120 keeps the accepted Alpha Unity projection as a manual Editor-only surface and adds usability controls plus a bounded Unity editor-noise cleanup script.",
            string.Empty,
            "## Hands-on Verification",
            string.Empty,
            "- Open `unity/LLMGameCreatorAlpha` in Unity.",
            "- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.",
            "- Click `Build/Refresh Playable Projection`.",
            "- Use `Focus Projection Camera`, `Select Player Proxy`, `Select Next Interaction Target`, `Select Next Objective`, `Select Diagnostics Marker` and `Toggle/Refresh Legend`.",
            "- Use `Clear Projection` to remove only `"
            + AcceptedAlphaUnityPlayableProjectionVocabulary.GeneratedRootName
            + "`.",
            string.Empty,
            "## Cleanup Commands",
            string.Empty,
            "- Dry run: `.\\.devflow\\scripts\\clean-unity-editor-noise.ps1 -DryRun`",
            "- Apply after Unity batchmode: `.\\.devflow\\scripts\\clean-unity-editor-noise.ps1 -Apply`",
            "- Cmd wrapper: `.\\.devflow\\scripts\\clean-unity-editor-noise.cmd`",
            string.Empty,
            "## Status",
            string.Empty,
            "- usabilityStatus: " + dashboard.UsabilityStatus,
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- cleanupScriptContractPassed: " + dashboard.CleanupScriptContractPassed.ToString().ToLowerInvariant(),
            "- doNotStartAutomatically: " + dashboard.DoNotStartAutomatically.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static AcceptedAlphaProjectionUsabilityScriptInventoryEntry Entry(
        string root,
        string relativePath,
        string role,
        string marker)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new AcceptedAlphaProjectionUsabilityScriptInventoryEntry
        {
            RelativePath = relativePath,
            Role = role,
            Exists = exists,
            ContainsRequiredMarker = text.Contains(marker, StringComparison.Ordinal),
            RequiredMarker = marker,
            Sha256 = exists ? HashBytes(File.ReadAllBytes(path)) : string.Empty
        };
    }

    private static AcceptedAlphaProjectionUsabilitySmokePlanStep Step(
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
        AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory + "/",
        AcceptedAlphaProjectionUsabilityVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-120-accepted-alpha-projection-usability-and-cleanup/",
        AcceptedAlphaProjectionUsabilityVocabulary.DocumentationPath,
        ".devflow/artifact-scope/artifact-scope-policy.json",
        AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
        AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath,
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
        "tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/DevFlow/CleanUnityEditorNoiseScriptTests.cs",
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
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal120/example.json",
        ".llmgc/exports/final-release/package.zip",
        "provider/live-geodata/Adapter.cs"
    ];

    private static bool IsAllowedChangedPath(string path) =>
        BuildExpectedChangedPaths().Any(prefix =>
            prefix.EndsWith("/", StringComparison.Ordinal)
                ? path.StartsWith(prefix, StringComparison.Ordinal)
                : string.Equals(path, prefix, StringComparison.Ordinal));

    private static void Require(bool condition, string code, List<string> errors)
    {
        if (!condition)
        {
            errors.Add(code);
        }
    }

    private static JsonDocument? LoadJson(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string StringProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool TryGetBool(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

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
            throw new InvalidOperationException("Goal120 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
