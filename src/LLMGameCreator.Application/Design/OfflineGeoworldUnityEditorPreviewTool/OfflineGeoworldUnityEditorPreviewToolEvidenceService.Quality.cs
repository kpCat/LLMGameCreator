using LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;
using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

public sealed partial class OfflineGeoworldUnityEditorPreviewToolEvidenceService
{
    private static OfflineGeoworldUnityEditorWorkspaceBindingInventory BuildWorkspaceBindingInventory(
        string root)
    {
        var diagnostics = new List<OfflineGeoworldUnityEditorPreviewDiagnostic>();
        var workspaceDirectory = Resolve(
            root,
            "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace");
        var pagePath = Resolve(
            root,
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.cs");
        var workspaceText = Directory.Exists(workspaceDirectory)
            ? string.Join(Environment.NewLine, Directory.EnumerateFiles(workspaceDirectory, "*.cs")
                .Select(path => File.ReadAllText(path, Encoding.UTF8)))
            : string.Empty;
        var pageText = File.Exists(pagePath) ? File.ReadAllText(pagePath, Encoding.UTF8) : string.Empty;
        var group = workspaceText.Contains("offline_geoworld_unity_editor_preview", StringComparison.Ordinal);
        var relative = workspaceText.Contains(
            OfflineGeoworldUnityEditorPreviewToolVocabulary.RelativeOutputDirectory,
            StringComparison.Ordinal);
        var winForms = pageText.Contains("offlineGeoworldUnityEditorPreviewCommandCount", StringComparison.Ordinal)
                       && pageText.Contains("offlineGeoworldUnityEditorPreviewMenuItemMarker", StringComparison.Ordinal);
        var scriptPath = pageText.Contains("offlineGeoworldUnityEditorPreviewEditorWindowScriptPath", StringComparison.Ordinal);
        var menu = pageText.Contains("offlineGeoworldUnityEditorPreviewMenuItemMarker", StringComparison.Ordinal);
        var payload = pageText.Contains("offlineGeoworldUnityEditorPreviewPayloadPath", StringComparison.Ordinal);
        var commandCount = pageText.Contains(
            "offlineGeoworldUnityEditorPreviewCommandCount",
            StringComparison.Ordinal);
        var travel = pageText.Contains(
            "offlineGeoworldUnityEditorPreviewTravelWindowStepCount",
            StringComparison.Ordinal);
        var simulated = pageText.Contains(
            "offlineGeoworldUnityEditorPreviewSimulatedActionProofPassed",
            StringComparison.Ordinal);
        var clear = pageText.Contains(
            "offlineGeoworldUnityEditorPreviewClearOperationProofPassed",
            StringComparison.Ordinal);
        var alpha = pageText.Contains(
            "offlineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged",
            StringComparison.Ordinal);
        var manual = pageText.Contains(
            "offlineGeoworldUnityEditorPreviewManualInstructions",
            StringComparison.Ordinal);

        AddIfFalse(group, "goal102.workspace.group_missing", "workspace", diagnostics);
        AddIfFalse(relative, "goal102.workspace.relative_path_missing", "workspace", diagnostics);
        AddIfFalse(winForms, "goal102.workspace.winforms_fields_missing", "page", diagnostics);
        AddIfFalse(scriptPath, "goal102.workspace.script_path_missing", "page", diagnostics);
        AddIfFalse(menu, "goal102.workspace.menu_missing", "page", diagnostics);
        AddIfFalse(payload, "goal102.workspace.payload_missing", "page", diagnostics);
        AddIfFalse(commandCount, "goal102.workspace.command_count_missing", "page", diagnostics);
        AddIfFalse(travel, "goal102.workspace.travel_missing", "page", diagnostics);
        AddIfFalse(simulated, "goal102.workspace.simulated_missing", "page", diagnostics);
        AddIfFalse(clear, "goal102.workspace.clear_missing", "page", diagnostics);
        AddIfFalse(alpha, "goal102.workspace.alpha_missing", "page", diagnostics);
        AddIfFalse(manual, "goal102.workspace.manual_missing", "page", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldUnityEditorWorkspaceBindingInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            WorkspaceCatalogIncludesEditorPreviewGroup = group,
            WorkspaceReadsGoal102EvidenceByRelativePath = relative,
            WinFormsPageDisplaysEditorPreviewFields = winForms,
            ShowsEditorWindowScriptPath = scriptPath,
            ShowsMenuItemMarker = menu,
            ShowsGoal101PayloadPath = payload,
            ShowsPreviewCommandCount = commandCount,
            ShowsTravelWindowSteps = travel,
            ShowsSimulatedEditorActionProof = simulated,
            ShowsClearCleanupProof = clear,
            ShowsAlphaRuntimeBootstrapUnchangedStatus = alpha,
            ShowsManualLaunchInstructions = manual,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldUnityEditorSourceLineage BuildSourceLineage(
        string root,
        Goal101EditorPreviewContext context)
    {
        var records = SourceLineageInputs()
            .Select(item => SourceLineageRecord(root, item.Path, item.Purpose))
            .ToList();
        var diagnostics = new List<OfflineGeoworldUnityEditorPreviewDiagnostic>();
        foreach (var missing in records.Where(item => !item.Exists))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102.lineage.missing",
                missing.RelativePath,
                "Required source lineage artifact is missing."));
        }

        AddIfFalse(context.Goal101AcceptedFalse, "goal102.lineage.goal101_accepted", "Goal101", diagnostics);
        AddIfFalse(context.Goal101CountsProven, "goal102.lineage.goal101_counts", "Goal101", diagnostics);
        AddIfFalse(
            context.Goal101SimulatedCommandProofPassed,
            "goal102.lineage.goal101_simulated",
            "Goal101",
            diagnostics);
        AddIfFalse(
            context.Goal101NegativeProofPassed,
            "goal102.lineage.goal101_negative",
            "Goal101",
            diagnostics);
        AddIfFalse(
            context.Goal101AlphaRuntimeBootstrapUnchanged,
            "goal102.lineage.goal101_alpha",
            "Goal101",
            diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldUnityEditorSourceLineage
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal101AcceptedFalsePreserved = context.Goal101AcceptedFalse,
            Goal101PayloadConsumed = context.Goal101CountsProven && context.Goal101PayloadFilesExist,
            Goal101SimulatedCommandProofPassed = context.Goal101SimulatedCommandProofPassed,
            Goal101NegativeProofPassed = context.Goal101NegativeProofPassed,
            Goal101AlphaRuntimeBootstrapUnchanged = context.Goal101AlphaRuntimeBootstrapUnchanged,
            Records = records,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldUnityEditorQualityGateScan BuildQualityGate(
        string root,
        Goal101EditorPreviewContext context,
        OfflineGeoworldUnityEditorToolInventory inventory,
        OfflineGeoworldUnityEditorSimulatedActionProof proof,
        OfflineGeoworldUnityEditorNegativeProof negative,
        OfflineGeoworldUnityEditorWorkspaceBindingInventory binding,
        OfflineGeoworldUnityEditorSourceLineage lineage)
    {
        var diagnostics = new List<OfflineGeoworldUnityEditorPreviewDiagnostic>();
        diagnostics.AddRange(context.Diagnostics);
        diagnostics.AddRange(inventory.Diagnostics);
        diagnostics.AddRange(proof.Diagnostics);
        diagnostics.AddRange(binding.Diagnostics);
        diagnostics.AddRange(lineage.Diagnostics);
        var sourceFiles = CandidateSourceFiles(root)
            .Where(File.Exists)
            .Select(path => ScanSourceFile(root, path))
            .ToList();
        foreach (var file in sourceFiles.Where(item => item.Lines > 700))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102.source.file_over_700",
                file.RelativePath,
                "New or touched Goal102 C# files must remain below 700 logical lines."));
        }

        foreach (var file in sourceFiles.Where(item => item.Lines > 1000))
        {
            diagnostics.Add(OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                "goal102.source.file_over_1000",
                file.RelativePath,
                "Changed C# files must remain below 1000 logical lines."));
        }

        var alphaPath = Resolve(root, OfflineGeoworldUnityEditorPreviewToolVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldUnityEditorPreviewToolVocabulary.AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldUnityEditorPreviewToolVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        var goal101Consumed = context.Goal101CountsProven
                              && context.Goal101PayloadFilesExist
                              && context.Goal101QualityGatePassed;
        var editorReady = inventory.Passed;
        var noNetwork = proof.NoProviderOrNetworkMarkers && inventory.HasNoProviderNetworkMarkers;
        var noRaw = proof.NoRawGeodata
                    && context.Commands.Commands.All(item => !item.RawGeodataIncluded);
        var noAbsolute = proof.NoAbsolutePaths;
        var noBinary = proof.NoBinaryOrRasterMedia;
        var noScenePrefab = proof.NoScenePrefabSettingsChangeMarkers
                            && inventory.HasNoScenePrefabSettingsMutationMarkers;

        AddIfFalse(goal101Consumed, "goal102.quality.goal101", "Goal101", diagnostics);
        AddIfFalse(editorReady, "goal102.quality.editor_window", "editor script", diagnostics);
        AddIfFalse(inventory.MenuItemMarkerPresent, "goal102.quality.menu", "editor script", diagnostics);
        AddIfFalse(inventory.Goal101PayloadPathMarkerPresent, "goal102.quality.payload_path", "editor script", diagnostics);
        AddIfFalse(inventory.CreatePreviewObjectsMethodPresent, "goal102.quality.create", "editor script", diagnostics);
        AddIfFalse(inventory.ClearPreviewObjectsMethodPresent, "goal102.quality.clear", "editor script", diagnostics);
        AddIfFalse(proof.Passed, "goal102.quality.proof", "simulated action proof", diagnostics);
        AddIfFalse(proof.ClearOperationModelPassed, "goal102.quality.clear_proof", "simulated action proof", diagnostics);
        AddIfFalse(negative.Passed, "goal102.quality.negative", "negative proof", diagnostics);
        AddIfFalse(binding.Passed, "goal102.quality.workspace", "workspace", diagnostics);
        AddIfFalse(lineage.Passed, "goal102.quality.lineage", "source lineage", diagnostics);
        AddIfFalse(alphaUnchanged, "goal102.quality.alpha_bootstrap",
            OfflineGeoworldUnityEditorPreviewToolVocabulary.AlphaRuntimeBootstrapPath, diagnostics);
        AddIfFalse(noNetwork, "goal102.quality.network_provider", "payload/editor", diagnostics);
        AddIfFalse(noRaw, "goal102.quality.raw_geodata", "payload", diagnostics);
        AddIfFalse(noAbsolute, "goal102.quality.absolute_paths", "payload", diagnostics);
        AddIfFalse(noBinary, "goal102.quality.binary_media", "payload", diagnostics);
        AddIfFalse(noScenePrefab, "goal102.quality.scene_prefab_settings", "editor", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldUnityEditorQualityGateScan
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal101Consumed = goal101Consumed,
            EditorWindowScriptReady = editorReady,
            MenuItemMarkerPresent = inventory.MenuItemMarkerPresent,
            Goal101PayloadPathMarkerPresent = inventory.Goal101PayloadPathMarkerPresent,
            CreatePreviewObjectsMethodPresent = inventory.CreatePreviewObjectsMethodPresent,
            ClearPreviewObjectsMethodPresent = inventory.ClearPreviewObjectsMethodPresent,
            SimulatedActionProofPassed = proof.Passed,
            ClearOperationProofPassed = proof.ClearOperationModelPassed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            SourceLineagePassed = lineage.Passed,
            AlphaRuntimeBootstrapUnchanged = alphaUnchanged,
            AlphaRuntimeBootstrapAfterHash = alphaHash,
            AlphaRuntimeBootstrapAfterLineCount = alphaLineCount,
            NoNetworkOrProviderImplementation = noNetwork,
            NoRawGeodataDump = noRaw,
            NoAbsolutePaths = noAbsolute,
            NoBinaryOrRasterMedia = noBinary,
            NoScenePrefabSettingsChanges = noScenePrefab,
            CommandCount = proof.CommandCount,
            CommandKindCount = proof.CommandKindCount,
            TravelWindowStepCount = proof.TravelWindowStepCount,
            ExpectedObjectCount = proof.ExpectedObjectCount,
            UnityPayloadFileCount = OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredPayloadFileNames.Count,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => item.Lines),
            FilesOver700LogicalLinesCount = sourceFiles.Count(item => item.Lines > 700),
            FilesOver1000LogicalLinesCount = sourceFiles.Count(item => item.Lines > 1000),
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldUnityEditorPreviewTool/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityEditorPreviewTool/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityEditorPreviewToolProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs",
                ".llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool/",
                "docs/agent-tasks/goal-102-offline-geoworld-unity-editor-preview-tool/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = ordered
        };
    }

    private static IReadOnlyList<(string Path, string Purpose)> SourceLineageInputs() =>
    [
        ("docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md", "geoworld pattern study"),
        ("docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md", "streaming policy"),
        ("docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md", "source adapter policy"),
        (OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
         + "/"
         + OfflineGeoworldUnityPreviewRunnerVocabulary.ReportMarkdownFileName,
            "Goal101 report"),
        (OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
         + "/"
         + OfflineGeoworldUnityPreviewRunnerVocabulary.CommandCatalogFileName,
            "Goal101 command catalog"),
        (OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
         + "/"
         + OfflineGeoworldUnityPreviewRunnerVocabulary.TravelWindowScriptFileName,
            "Goal101 travel window script"),
        (OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
         + "/"
         + OfflineGeoworldUnityPreviewRunnerVocabulary.SimulatedCommandProofFileName,
            "Goal101 simulated command proof"),
        (OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
         + "/"
         + OfflineGeoworldUnityPreviewRunnerVocabulary.NegativeProofFileName,
            "Goal101 negative proof"),
        (OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath,
            "Goal102 Unity Editor preview window")
    ];

    private static IEnumerable<string> CandidateSourceFiles(string root)
    {
        var paths = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/OfflineGeoworldUnityEditorPreviewTool");
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityEditorPreviewTool");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace");
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityEditorPreviewToolProductSmokeTests.cs"));
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs"));
        paths.Add(Resolve(root, OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath));
        return paths;
    }

    private static void AddDirectory(ISet<string> paths, string root, string relativePath)
    {
        var directory = Resolve(root, relativePath);
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            paths.Add(file);
        }
    }

    private static (string RelativePath, int Lines) ScanSourceFile(string root, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        return (Relative(root, path), CountLines(text));
    }

    private static OfflineGeoworldUnityEditorSourceLineageRecord SourceLineageRecord(
        string root,
        string relativePath,
        string purpose)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        return new OfflineGeoworldUnityEditorSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            Purpose = purpose
        };
    }
}
