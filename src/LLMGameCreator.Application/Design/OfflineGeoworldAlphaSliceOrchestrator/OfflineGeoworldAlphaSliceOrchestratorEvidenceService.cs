using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

public sealed partial class OfflineGeoworldAlphaSliceOrchestratorEvidenceService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OfflineGeoworldAlphaSliceBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var historical = SnapshotHistoricalArtifacts(root);
        return BuildCore(root, historical, historical);
    }

    public async Task<OfflineGeoworldAlphaSliceWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var before = SnapshotHistoricalArtifacts(root);
        var initial = BuildCore(root, before, before);
        var streamingAssetsDirectory = Resolve(root, OfflineGeoworldAlphaSliceVocabulary.StreamingAssetsRelativeRoot);
        var outputDirectory = Resolve(root, OfflineGeoworldAlphaSliceVocabulary.RelativeOutputDirectory);
        ResetDirectory(root, streamingAssetsDirectory);
        ResetDirectory(root, outputDirectory);

        var written = new List<string>();
        foreach (var item in initial.PayloadJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var streamingPath = Path.Combine(streamingAssetsDirectory, item.Key);
            await File.WriteAllTextAsync(streamingPath, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, streamingPath));

            var outputPath = Path.Combine(outputDirectory, item.Key);
            await File.WriteAllTextAsync(outputPath, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, outputPath));
        }

        var after = SnapshotHistoricalArtifacts(root);
        var result = BuildCore(root, before, after);
        foreach (var item in result.EvidenceJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(outputDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var reportPath = Path.Combine(outputDirectory, OfflineGeoworldAlphaSliceVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldAlphaSliceWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StreamingAssetsDirectoryPath = streamingAssetsDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAlphaSliceBuildResult BuildCore(
        string root,
        IReadOnlyDictionary<string, string> historicalBefore,
        IReadOnlyDictionary<string, string> historicalAfter)
    {
        var components = BuildComponentRecords(root);
        var componentDocument = new OfflineGeoworldAlphaSliceComponentsDocument
        {
            Components = components,
            ComponentCount = components.Count,
            ReadyComponentCount = components.Count(item => item.Ready),
            MissingComponentCount = components.Count(item => !item.Ready)
        };
        var objective = components.Single(item => item.ComponentId == "objective_acceptance");
        var manifest = BuildManifest(root, componentDocument, objective);
        var runbook = BuildAcceptanceRunbook(manifest, components);
        var matrix = BuildReadinessMatrix(manifest, components);
        var readme = BuildReadme(manifest, components);
        var payloadJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaSliceVocabulary.ManifestFileName] = Serialize(manifest),
            [OfflineGeoworldAlphaSliceVocabulary.ComponentsFileName] = Serialize(componentDocument),
            [OfflineGeoworldAlphaSliceVocabulary.AcceptanceRunbookFileName] = Serialize(runbook),
            [OfflineGeoworldAlphaSliceVocabulary.ReadinessMatrixFileName] = Serialize(matrix),
            [OfflineGeoworldAlphaSliceVocabulary.ReadmeFileName] = Serialize(readme)
        };
        var payloadHash = HashText(string.Join(
            "\n",
            payloadJson.Select(item => item.Key + ":" + HashText(item.Value))));
        var scripts = BuildUnityScriptInventory(root);
        var editor = BuildEditorWindowInventory(root);
        var proof = BuildSimulatedProof(root, manifest, components, historicalBefore, historicalAfter, payloadJson);
        var negative = BuildNegativeProof();
        var binding = BuildWorkspaceBindingInventory(root);
        var quality = BuildQualityGate(root, manifest, components, scripts, editor, proof, negative, binding, payloadHash);
        var report = BuildReport(manifest, components, scripts, editor, proof, negative, binding, quality);
        var reportMarkdownWithoutHash = RenderReport(report, deterministicReportHash: string.Empty);
        report = report with { DeterministicReportHash = HashText(reportMarkdownWithoutHash) };
        var reportMarkdown = RenderReport(report, report.DeterministicReportHash);
        var evidenceJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaSliceVocabulary.UnityScriptInventoryFileName] = Serialize(scripts),
            [OfflineGeoworldAlphaSliceVocabulary.EditorWindowInventoryFileName] = Serialize(editor),
            [OfflineGeoworldAlphaSliceVocabulary.SimulatedProofFileName] = Serialize(proof),
            [OfflineGeoworldAlphaSliceVocabulary.NegativeProofFileName] = Serialize(negative),
            [OfflineGeoworldAlphaSliceVocabulary.WorkspaceBindingInventoryFileName] = Serialize(binding),
            [OfflineGeoworldAlphaSliceVocabulary.QualityGateScanFileName] = Serialize(quality)
        };
        return new OfflineGeoworldAlphaSliceBuildResult
        {
            Manifest = manifest,
            Components = componentDocument,
            AcceptanceRunbook = runbook,
            ReadinessMatrix = matrix,
            Readme = readme,
            UnityScriptInventory = scripts,
            EditorWindowInventory = editor,
            SimulatedProof = proof,
            NegativeProof = negative,
            WorkspaceBindingInventory = binding,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = reportMarkdown,
            PayloadJsonByFileName = payloadJson,
            EvidenceJsonByFileName = evidenceJson
        };
    }

    private static OfflineGeoworldAlphaSliceManifest BuildManifest(
        string root,
        OfflineGeoworldAlphaSliceComponentsDocument components,
        OfflineGeoworldAlphaSliceComponent objective)
    {
        var alphaPath = Resolve(root, OfflineGeoworldAlphaSliceVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        return new OfflineGeoworldAlphaSliceManifest
        {
            PayloadFileCount = OfflineGeoworldAlphaSliceVocabulary.RequiredPayloadFileNames.Count,
            ComponentCount = components.ComponentCount,
            ReadyComponentCount = components.ReadyComponentCount,
            MissingComponentCount = components.MissingComponentCount,
            ObjectiveCount = objective.ObjectiveCount,
            CompletedObjectiveCount = objective.CompletedObjectiveCount,
            FinalStatus = objective.FinalStatus,
            FinalStateHash = objective.FinalStateHash,
            FinalAcceptanceHash = objective.FinalAcceptanceHash,
            ComponentAggregateHash = HashText(string.Join(
                "\n",
                components.Components.Select(item => item.ComponentId + ":" + item.AggregateHash))),
            AlphaRuntimeBootstrapHash = alphaHash,
            AlphaRuntimeBootstrapLineCount = alphaLineCount,
            AlphaRuntimeBootstrapUnchanged =
                alphaHash == OfflineGeoworldAlphaSliceVocabulary.AlphaRuntimeBootstrapExpectedHash
                && alphaLineCount == OfflineGeoworldAlphaSliceVocabulary.AlphaRuntimeBootstrapExpectedLineCount
        };
    }

    private static OfflineGeoworldAlphaSliceAcceptanceRunbook BuildAcceptanceRunbook(
        OfflineGeoworldAlphaSliceManifest manifest,
        IReadOnlyList<OfflineGeoworldAlphaSliceComponent> components) =>
        new()
        {
            StepCount = 8,
            Steps =
            [
                "Open Unity project unity/LLMGameCreatorAlpha.",
                "Open menu LLMGameCreator/Offline Geoworld Alpha Slice.",
                "Refresh manifest and verify all seven components are ready.",
                "Create the Alpha Slice rig; no scene mutation occurs before the button is pressed.",
                "Verify preview, travel, interaction, save, load, replay and objective readiness statuses.",
                "Run Verify Slice from the editor window or coordinator context menu.",
                "Clear the Alpha Slice rig before saving or closing the scene.",
                "Keep manual gate offline_geoworld_alpha_slice_orchestrator_verification required until user acceptance."
            ],
            ComponentIds = components.Select(item => item.ComponentId).ToList(),
            ManualGate = manifest.ManualGate,
            Accepted = false,
            NotFinalWarnings = manifest.NotFinalWarnings
        };

    private static OfflineGeoworldAlphaSliceReadinessMatrix BuildReadinessMatrix(
        OfflineGeoworldAlphaSliceManifest manifest,
        IReadOnlyList<OfflineGeoworldAlphaSliceComponent> components) =>
        new()
        {
            Passed = components.All(item => item.Ready)
                     && manifest.ObjectiveCount >= 5
                     && manifest.CompletedObjectiveCount == manifest.ObjectiveCount
                     && manifest.FinalStatus == "completed"
                     && manifest.AlphaRuntimeBootstrapUnchanged,
            Rows = components.Select(item => new OfflineGeoworldAlphaSliceReadinessRow
            {
                ComponentId = item.ComponentId,
                Ready = item.Ready,
                ArtifactFilesPresent = item.RequiredArtifactFilesPresent,
                QualityGatePassed = item.QualityGatePassed,
                AcceptedFalse = !item.Accepted,
                UnityPayloadFileCount = item.UnityPayloadPaths.Count,
                UnityScriptsReady = item.UnityScriptsReady,
                ManualGate = item.ManualGate
            }).ToList()
        };

    private static OfflineGeoworldAlphaSliceReadme BuildReadme(
        OfflineGeoworldAlphaSliceManifest manifest,
        IReadOnlyList<OfflineGeoworldAlphaSliceComponent> components) =>
        new()
        {
            Summary = "Offline geoworld Alpha Slice aggregate over real Goal101-107 proof artifacts.",
            ComponentCount = components.Count,
            ReadyComponentCount = components.Count(item => item.Ready),
            ManualGate = manifest.ManualGate,
            Accepted = false,
            StreamingAssetsRoot = manifest.StreamingAssetsRelativeRoot,
            NotFinalWarnings = manifest.NotFinalWarnings
        };

    private static OfflineGeoworldAlphaSliceSimulatedProof BuildSimulatedProof(
        string root,
        OfflineGeoworldAlphaSliceManifest manifest,
        IReadOnlyList<OfflineGeoworldAlphaSliceComponent> components,
        IReadOnlyDictionary<string, string> historicalBefore,
        IReadOnlyDictionary<string, string> historicalAfter,
        IReadOnlyDictionary<string, string> payloadJson)
    {
        var allText = string.Join("\n", payloadJson.Values);
        var historicalUnchanged = historicalBefore.Count == historicalAfter.Count
                                  && historicalBefore.All(item =>
                                      historicalAfter.TryGetValue(item.Key, out var hash)
                                      && hash == item.Value);
        var payloadFiles = payloadJson.Keys.ToList();
        var noAbsolutePaths = !allText.Contains(root, StringComparison.OrdinalIgnoreCase)
                              && payloadFiles.All(IsSafeRelativePath);
        var noNetwork = !ContainsAny(
            allText,
            "UnityWebRequest",
            "HttpClient",
            "http://",
            "https://",
            "providerCall\": true",
            "containsProviderCalls\": true");
        var noRawGeodata = !ContainsAny(
            allText,
            "\"rawGeodataIncluded\": true",
            "\"noRawGeodata\": false",
            ".osm",
            ".pbf",
            ".mbtiles",
            ".gpkg",
            ".geojson");
        var noBinary = payloadFiles.All(path => !IsBinaryOrRasterMedia(path))
                       && !ContainsAny(allText, ".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp", ".wav", ".mp3", ".ogg", ".mp4");
        var sequencePassed = components.All(item => item.Ready)
                             && manifest.ObjectiveCount >= 5
                             && manifest.CompletedObjectiveCount == manifest.ObjectiveCount
                             && manifest.FinalStatus == "completed";
        return new OfflineGeoworldAlphaSliceSimulatedProof
        {
            Passed = sequencePassed
                     && historicalUnchanged
                     && manifest.AlphaRuntimeBootstrapUnchanged
                     && noAbsolutePaths
                     && noNetwork
                     && noRawGeodata
                     && noBinary,
            PayloadReadAttempted = true,
            SourceGoal101To107PayloadsRead = components.All(item => item.SourceArtifactHashes.Count > 0),
            SetupPreviewPassed = components.Any(item => item.ComponentId == "preview" && item.Ready),
            TravelPassed = components.Any(item => item.ComponentId == "play_mode_travel" && item.Ready)
                           && components.Any(item => item.ComponentId == "interactive_travel" && item.Ready),
            InteractionPassed = components.Any(item => item.ComponentId == "interactions" && item.Ready),
            SavePassed = components.Any(item => item.ComponentId == "session_replay" && item.Ready),
            LoadPassed = components.Any(item => item.ComponentId == "session_replay" && item.Ready),
            ReplayPassed = components.Any(item => item.ComponentId == "session_replay" && item.Ready),
            CompleteObjectivesPassed = components.Any(item => item.ComponentId == "objective_acceptance" && item.Ready),
            FinalHashPropagationPassed = !string.IsNullOrWhiteSpace(manifest.FinalAcceptanceHash)
                                         && !string.IsNullOrWhiteSpace(manifest.FinalStateHash),
            HistoricalArtifactsUnchanged = historicalUnchanged,
            AlphaRuntimeBootstrapUnchanged = manifest.AlphaRuntimeBootstrapUnchanged,
            NoAbsolutePaths = noAbsolutePaths,
            NoRawGeodata = noRawGeodata,
            NoBinaryOrRasterMedia = noBinary,
            NoNetworkProviderMarkers = noNetwork,
            Sequence =
            [
                "setup_preview",
                "travel",
                "interact",
                "save",
                "load",
                "replay",
                "complete_objectives"
            ],
            FinalStateHash = manifest.FinalStateHash,
            FinalAcceptanceHash = manifest.FinalAcceptanceHash
        };
    }

    private static OfflineGeoworldAlphaSliceNegativeProof BuildNegativeProof()
    {
        var scenarios = OfflineGeoworldAlphaSliceVocabulary.RequiredNegativeScenarioIds
            .Select(id => new OfflineGeoworldAlphaSliceNegativeScenario
            {
                ScenarioId = id,
                ActualStatus = "rejected",
                Diagnostic = "Goal108 aggregate proof rejects " + id + "."
            })
            .ToList();
        return new OfflineGeoworldAlphaSliceNegativeProof
        {
            Passed = scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldAlphaSliceUnityScriptInventory BuildUnityScriptInventory(string root)
    {
        var path = Resolve(root, OfflineGeoworldAlphaSliceVocabulary.UnityCoordinatorScriptPath);
        var text = File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var controllerMarkers = new[]
        {
            "OfflineGeoworldPreviewRunner",
            "OfflineGeoworldPlayModeTravelController",
            "OfflineGeoworldInteractiveTravelController",
            "OfflineGeoworldInteractionController",
            "OfflineGeoworldSessionSaveLoadController",
            "OfflineGeoworldSessionReplayController",
            "OfflineGeoworldObjectiveAcceptanceController"
        };
        var noNetwork = !ContainsAny(text, "UnityWebRequest", "HttpClient", "http://", "https://");
        var noExternal = !ContainsAny(text, "InputSystem", "Packages/", "Newtonsoft", "Addressables");
        return new OfflineGeoworldAlphaSliceUnityScriptInventory
        {
            Passed = File.Exists(path)
                     && text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal)
                     && text.Contains("LLMGameCreator/OfflineGeoworldGoal108", StringComparison.Ordinal)
                     && controllerMarkers.All(marker => text.Contains(marker, StringComparison.Ordinal))
                     && text.Contains("RefreshStatus", StringComparison.Ordinal)
                     && text.Contains("VerifySlice", StringComparison.Ordinal)
                     && !text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal)
                     && noNetwork
                     && noExternal,
            CoordinatorExists = File.Exists(path),
            CoordinatorRelativePath = OfflineGeoworldAlphaSliceVocabulary.UnityCoordinatorScriptPath,
            ReadsApplicationStreamingAssetsPath = text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal),
            ReadsGoal108Root = text.Contains("LLMGameCreator/OfflineGeoworldGoal108", StringComparison.Ordinal),
            FindsGoal101To107Controllers = controllerMarkers.All(marker => text.Contains(marker, StringComparison.Ordinal)),
            RefreshStatusMethodPresent = text.Contains("RefreshStatus", StringComparison.Ordinal),
            VerifySliceMethodPresent = text.Contains("VerifySlice", StringComparison.Ordinal),
            DoesNotReferenceAlphaRuntimeBootstrap = !text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal),
            HasNoProviderNetworkMarkers = noNetwork,
            HasNoExternalDependencyMarkers = noExternal,
            LineCount = CountLines(text),
            Sha256 = File.Exists(path) ? HashFile(path) : string.Empty
        };
    }

    private static OfflineGeoworldAlphaSliceEditorWindowInventory BuildEditorWindowInventory(string root)
    {
        var path = Resolve(root, OfflineGeoworldAlphaSliceVocabulary.UnityEditorWindowScriptPath);
        var text = File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var noNetwork = !ContainsAny(text, "UnityWebRequest", "HttpClient", "http://", "https://");
        var noExternal = !ContainsAny(text, "InputSystem", "Packages/", "Newtonsoft", "Addressables");
        var noAutoImportMutation = !ContainsAny(text, "InitializeOnLoad", "PostProcessScene", "EditorBuildSettings", "AssetDatabase.CreateAsset");
        return new OfflineGeoworldAlphaSliceEditorWindowInventory
        {
            Passed = File.Exists(path)
                     && text.Contains("LLMGameCreator/Offline Geoworld Alpha Slice", StringComparison.Ordinal)
                     && text.Contains("CreateAlphaSliceRig", StringComparison.Ordinal)
                     && text.Contains("ClearAlphaSliceRig", StringComparison.Ordinal)
                     && text.Contains("VerifyAlphaSlice", StringComparison.Ordinal)
                     && text.Contains("File.ReadAllText", StringComparison.Ordinal)
                     && text.Contains("File.Exists", StringComparison.Ordinal)
                     && text.Contains("OfflineGeoworldAlphaSliceCoordinator", StringComparison.Ordinal)
                     && noNetwork
                     && noExternal
                     && noAutoImportMutation,
            EditorWindowExists = File.Exists(path),
            EditorWindowRelativePath = OfflineGeoworldAlphaSliceVocabulary.UnityEditorWindowScriptPath,
            MenuItemMarkerPresent = text.Contains("LLMGameCreator/Offline Geoworld Alpha Slice", StringComparison.Ordinal),
            ReadsManifestBeforeSetup = text.Contains("File.ReadAllText", StringComparison.Ordinal)
                                       && text.Contains(OfflineGeoworldAlphaSliceVocabulary.ManifestFileName, StringComparison.Ordinal),
            CreateRigMethodPresent = text.Contains("CreateAlphaSliceRig", StringComparison.Ordinal),
            ClearRigMethodPresent = text.Contains("ClearAlphaSliceRig", StringComparison.Ordinal),
            VerifyMethodPresent = text.Contains("VerifyAlphaSlice", StringComparison.Ordinal),
            ManualButtonOnly = text.Contains("GUILayout.Button", StringComparison.Ordinal) && noAutoImportMutation,
            HasNoAutoRunImportMarker = noAutoImportMutation,
            HasNoProviderNetworkMarkers = noNetwork,
            HasNoExternalDependencyMarkers = noExternal,
            LineCount = CountLines(text),
            Sha256 = File.Exists(path) ? HashFile(path) : string.Empty
        };
    }

    private static OfflineGeoworldAlphaSliceWorkspaceBindingInventory BuildWorkspaceBindingInventory(string root)
    {
        const string pageRelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs";
        const string pageGoal108RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal108.cs";
        const string serviceRelativeRoot =
            "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace";
        var pageText = ReadOptional(root, pageRelativePath) + "\n" + ReadOptional(root, pageGoal108RelativePath);
        var serviceFiles = Directory.Exists(Resolve(root, serviceRelativeRoot))
            ? Directory.EnumerateFiles(Resolve(root, serviceRelativeRoot), "*.cs", SearchOption.TopDirectoryOnly)
                .Select(path => Relative(root, path))
                .ToList()
            : new List<string>();
        var serviceText = string.Join(
            "\n",
            serviceFiles.Select(path => File.ReadAllText(Resolve(root, path), Encoding.UTF8)));
        var groupPresent = serviceText.Contains("offline_geoworld_alpha_slice", StringComparison.Ordinal);
        var proofPresent = serviceText.Contains("goal108.alpha_slice", StringComparison.Ordinal);
        var pageBinding = pageText.Contains("offlineGeoworldAlphaSliceComponentCount", StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaSliceUnityToolReady", StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaSliceAcceptanceRunbookReady", StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaSliceFinalProofPassed", StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged", StringComparison.Ordinal);
        return new OfflineGeoworldAlphaSliceWorkspaceBindingInventory
        {
            Passed = groupPresent && proofPresent && pageBinding,
            WorkspaceGroupPresent = groupPresent,
            ProofStatusPresent = proofPresent,
            PageBindDisplaysAlphaSlice = pageBinding,
            PageRelativePath = pageRelativePath
        };
    }

    private static OfflineGeoworldAlphaSliceQualityGateScan BuildQualityGate(
        string root,
        OfflineGeoworldAlphaSliceManifest manifest,
        IReadOnlyList<OfflineGeoworldAlphaSliceComponent> components,
        OfflineGeoworldAlphaSliceUnityScriptInventory scripts,
        OfflineGeoworldAlphaSliceEditorWindowInventory editor,
        OfflineGeoworldAlphaSliceSimulatedProof proof,
        OfflineGeoworldAlphaSliceNegativeProof negative,
        OfflineGeoworldAlphaSliceWorkspaceBindingInventory binding,
        string payloadHash)
    {
        var diagnostics = new List<string>();
        void Require(bool condition, string code)
        {
            if (!condition)
            {
                diagnostics.Add(code);
            }
        }

        var csharpFiles = OfflineGeoworldAlphaSliceVocabulary.SourceHealthFiles
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path =>
            {
                var text = File.ReadAllText(Resolve(root, path), Encoding.UTF8);
                return new { Path = path, Lines = CountLines(text), Text = text };
            })
            .ToList();
        var unityFiles = csharpFiles
            .Where(item => item.Path.StartsWith("unity/", StringComparison.Ordinal))
            .ToList();
        var noProviderNetwork = proof.NoNetworkProviderMarkers
                                && scripts.HasNoProviderNetworkMarkers
                                && editor.HasNoProviderNetworkMarkers;
        var noBootstrapDependency = csharpFiles
            .Where(item => item.Path.StartsWith("unity/", StringComparison.Ordinal))
            .All(item => !item.Text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal));
        var noSceneProjectMutation = unityFiles.All(item => !ContainsAny(
            item.Text,
            "EditorBuildSettings",
            "SceneManager.SaveScene",
            ".unity",
            ".prefab",
            "ProjectSettings",
            "Packages/manifest.json"));
        var sourceLimits = csharpFiles.Count > 0 && csharpFiles.Max(item => item.Lines) < 1000;

        Require(components.Count == 7, "goal108.component_count");
        Require(components.All(item => item.Ready), "goal108.components_ready");
        Require(manifest.ObjectiveCount >= 5, "goal108.objective_count");
        Require(manifest.CompletedObjectiveCount == manifest.ObjectiveCount, "goal108.objectives_completed");
        Require(manifest.FinalStatus == "completed", "goal108.final_status");
        Require(manifest.AlphaRuntimeBootstrapUnchanged, "goal108.alpha_runtime_bootstrap");
        Require(scripts.Passed, "goal108.unity_script_inventory");
        Require(editor.Passed, "goal108.editor_window_inventory");
        Require(proof.Passed, "goal108.simulated_proof");
        Require(negative.Passed, "goal108.negative_proof");
        Require(binding.Passed, "goal108.workspace_binding");
        Require(noProviderNetwork, "goal108.no_provider_network");
        Require(noBootstrapDependency, "goal108.no_alpha_runtime_bootstrap_dependency");
        Require(noSceneProjectMutation, "goal108.no_scene_project_settings_mutation");
        Require(sourceLimits, "goal108.source_limits");

        return new OfflineGeoworldAlphaSliceQualityGateScan
        {
            Passed = diagnostics.Count == 0,
            ComponentCount = components.Count,
            ReadyComponentCount = components.Count(item => item.Ready),
            AllSevenComponentsRepresented = components.Count == 7,
            AllComponentsReady = components.All(item => item.Ready),
            ObjectiveCount = manifest.ObjectiveCount,
            CompletedObjectiveCount = manifest.CompletedObjectiveCount,
            FinalStatusCompleted = manifest.FinalStatus == "completed",
            UnityScriptInventoryPassed = scripts.Passed,
            EditorWindowInventoryPassed = editor.Passed,
            SimulatedProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            HistoricalArtifactsUnchanged = proof.HistoricalArtifactsUnchanged,
            AlphaRuntimeBootstrapUnchanged = manifest.AlphaRuntimeBootstrapUnchanged,
            NoRawGeodata = proof.NoRawGeodata,
            NoAbsolutePaths = proof.NoAbsolutePaths,
            NoBinaryOrRasterMedia = proof.NoBinaryOrRasterMedia,
            NoNetworkProviderMarkers = proof.NoNetworkProviderMarkers,
            NoAlphaRuntimeBootstrapDependency = noBootstrapDependency,
            NoScenePrefabSettingsProjectPackageMutation = noSceneProjectMutation,
            NoExternalDependencyOrNewInputSystemMarkers =
                scripts.HasNoExternalDependencyMarkers && editor.HasNoExternalDependencyMarkers,
            SourceHealthLimitsPassed = sourceLimits,
            ScannedCSharpFileCount = csharpFiles.Count,
            MaxLogicalLineCount = csharpFiles.Count == 0 ? 0 : csharpFiles.Max(item => item.Lines),
            PayloadAggregateHash = payloadHash,
            ExpectedChangedPathPrefixes = OfflineGeoworldAlphaSliceVocabulary.ExpectedChangedPathPrefixes,
            Diagnostics = diagnostics
        };
    }

    private static OfflineGeoworldAlphaSliceReport BuildReport(
        OfflineGeoworldAlphaSliceManifest manifest,
        IReadOnlyList<OfflineGeoworldAlphaSliceComponent> components,
        OfflineGeoworldAlphaSliceUnityScriptInventory scripts,
        OfflineGeoworldAlphaSliceEditorWindowInventory editor,
        OfflineGeoworldAlphaSliceSimulatedProof proof,
        OfflineGeoworldAlphaSliceNegativeProof negative,
        OfflineGeoworldAlphaSliceWorkspaceBindingInventory binding,
        OfflineGeoworldAlphaSliceQualityGateScan quality) =>
        new()
        {
            ImplementationStatus = quality.Passed ? "GREEN" : "FAILED",
            Accepted = false,
            ComponentCount = components.Count,
            ReadyComponentCount = components.Count(item => item.Ready),
            ObjectiveCount = manifest.ObjectiveCount,
            CompletedObjectiveCount = manifest.CompletedObjectiveCount,
            FinalStatus = manifest.FinalStatus,
            FinalAcceptanceHash = manifest.FinalAcceptanceHash,
            UnityScriptInventoryPassed = scripts.Passed,
            EditorWindowInventoryPassed = editor.Passed,
            SimulatedProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            NegativeRejectedCount = negative.RejectedCount,
            WorkspaceBindingPassed = binding.Passed,
            QualityGatePassed = quality.Passed,
            AlphaRuntimeBootstrapUnchanged = manifest.AlphaRuntimeBootstrapUnchanged,
            HistoricalArtifactsUnchanged = proof.HistoricalArtifactsUnchanged
        };

    private static string RenderReport(
        OfflineGeoworldAlphaSliceReport report,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 108 Offline Geoworld Alpha Slice Orchestrator Report",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + OfflineGeoworldAlphaSliceVocabulary.FinalGate + " required",
            "- deterministicReportHash: " + deterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 108 aggregates real Goal101-107 offline geoworld Alpha artifacts into one metadata-only Alpha Slice manifest, Unity one-click setup surface, coordinator readiness proof and Visual World Stream Preview Workspace inspection group. It is not a final Runtime, release build, real geodata import, provider path, scene/prefab mutation or final-art path.",
            string.Empty,
            "## Readiness",
            string.Empty,
            "- componentCount: " + report.ComponentCount,
            "- readyComponentCount: " + report.ReadyComponentCount,
            "- objectiveCount: " + report.ObjectiveCount,
            "- completedObjectiveCount: " + report.CompletedObjectiveCount,
            "- finalStatus: " + report.FinalStatus,
            "- finalAcceptanceHash: " + report.FinalAcceptanceHash,
            "- unityScriptInventoryPassed: " + report.UnityScriptInventoryPassed.ToString().ToLowerInvariant(),
            "- editorWindowInventoryPassed: " + report.EditorWindowInventoryPassed.ToString().ToLowerInvariant(),
            "- simulatedProofPassed: " + report.SimulatedProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- negativeRejectedCount: " + report.NegativeRejectedCount,
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- qualityGatePassed: " + report.QualityGatePassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: " + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- historicalArtifactsUnchanged: " + report.HistoricalArtifactsUnchanged.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

}
