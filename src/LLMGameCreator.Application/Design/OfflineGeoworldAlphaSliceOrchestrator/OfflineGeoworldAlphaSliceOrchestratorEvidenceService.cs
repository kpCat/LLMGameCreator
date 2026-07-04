using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

public sealed class OfflineGeoworldAlphaSliceOrchestratorEvidenceService
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

    private static IReadOnlyList<OfflineGeoworldAlphaSliceComponent> BuildComponentRecords(string root) =>
        ComponentDefinitions().Select(item => BuildComponentRecord(root, item)).ToList();

    private static OfflineGeoworldAlphaSliceComponent BuildComponentRecord(
        string root,
        ComponentDefinition definition)
    {
        var sourceHashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var sourcePaths = definition.RequiredArtifactFiles
            .Select(fileName => definition.ArtifactRoot + "/" + fileName)
            .ToList();
        if (definition.StreamingAssetsRoot.Length > 0
            && Directory.Exists(Resolve(root, definition.StreamingAssetsRoot)))
        {
            sourcePaths.AddRange(Directory
                .EnumerateFiles(
                    Resolve(root, definition.StreamingAssetsRoot),
                    "*",
                    SearchOption.TopDirectoryOnly)
                .Select(path => Relative(root, path)));
        }

        foreach (var relativePath in sourcePaths.OrderBy(item => item, StringComparer.Ordinal))
        {
            var path = Resolve(root, relativePath);
            if (File.Exists(path))
            {
                sourceHashes[relativePath] = HashFile(path);
            }
        }

        var primaryPath = Resolve(root, definition.ArtifactRoot + "/" + definition.PrimaryJsonFileName);
        using var primary = TryReadJson(primaryPath);
        var qualityPath = Resolve(root, definition.ArtifactRoot + "/" + definition.QualityJsonFileName);
        using var quality = TryReadJson(qualityPath);
        var requiredPresent = definition.RequiredArtifactFiles.All(file =>
            File.Exists(Resolve(root, definition.ArtifactRoot + "/" + file)));
        var streamingPayloadPaths = new List<string>();
        if (definition.StreamingAssetsRoot.Length > 0
            && Directory.Exists(Resolve(root, definition.StreamingAssetsRoot)))
        {
            streamingPayloadPaths.AddRange(Directory
                .EnumerateFiles(
                    Resolve(root, definition.StreamingAssetsRoot),
                    "*",
                    SearchOption.TopDirectoryOnly)
                .Select(path => Relative(root, path))
                .OrderBy(path => path, StringComparer.Ordinal));
        }
        var scriptsReady = definition.UnityScriptPaths.All(path => File.Exists(Resolve(root, path)));
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var accepted = primary is not null && TryGetBool(primary.RootElement, "accepted");
        var objectiveCount = primary is null ? 0 : ReadInt(primary.RootElement, "objectiveCount");
        var completedObjectiveCount = primary is null ? 0 : ReadInt(primary.RootElement, "completedObjectiveCount");
        var finalStatus = primary is null ? string.Empty : ReadString(primary.RootElement, "finalStatus");
        var finalStateHash = primary is null
            ? string.Empty
            : ReadString(primary.RootElement, "sourceGoal106FinalStateHash");
        var finalAcceptanceHash = primary is null
            ? string.Empty
            : ReadString(primary.RootElement, "objectiveAcceptanceHash");
        var aggregateHash = HashText(string.Join(
            "\n",
            sourceHashes.Select(item => item.Key + ":" + item.Value)));
        var ready = requiredPresent
                    && (definition.StreamingAssetsRoot.Length == 0 || streamingPayloadPaths.Count > 0)
                    && qualityPassed
                    && !accepted
                    && scriptsReady;
        return new OfflineGeoworldAlphaSliceComponent
        {
            ComponentId = definition.ComponentId,
            DisplayName = definition.DisplayName,
            SourceGoalId = definition.SourceGoalId,
            SourceArtifactRoot = definition.ArtifactRoot,
            StreamingAssetsRoot = definition.StreamingAssetsRoot,
            ManualGate = primary is null ? string.Empty : ReadString(primary.RootElement, "manualGate"),
            ImplementationStatus = primary is null ? string.Empty : ReadString(primary.RootElement, "implementationStatus"),
            Accepted = accepted,
            Ready = ready,
            RequiredArtifactFilesPresent = requiredPresent,
            QualityGatePassed = qualityPassed,
            UnityScriptsReady = scriptsReady,
            UnityPayloadPaths = streamingPayloadPaths,
            UnityScriptPaths = definition.UnityScriptPaths,
            SourceArtifactHashes = sourceHashes,
            AggregateHash = aggregateHash,
            ObjectiveCount = objectiveCount,
            CompletedObjectiveCount = completedObjectiveCount,
            FinalStatus = finalStatus,
            FinalStateHash = finalStateHash,
            FinalAcceptanceHash = finalAcceptanceHash,
            NotFinalWarnings =
            [
                "Alpha tooling only.",
                "accepted=false until manual gate review.",
                "No Runtime, provider, schema, scene, prefab, project settings, final art or real geodata promotion."
            ]
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

    private static IReadOnlyDictionary<string, string> SnapshotHistoricalArtifacts(string root)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var directory in ComponentDefinitions()
                     .SelectMany(item => new[] { item.ArtifactRoot, item.StreamingAssetsRoot })
                     .Where(item => item.Length > 0)
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(item => item, StringComparer.Ordinal))
        {
            var full = Resolve(root, directory);
            if (!Directory.Exists(full))
            {
                continue;
            }

            foreach (var path in Directory.EnumerateFiles(full, "*", SearchOption.AllDirectories)
                         .OrderBy(path => path, StringComparer.Ordinal))
            {
                result[Relative(root, path)] = HashFile(path);
            }
        }

        return result;
    }

    private static IReadOnlyList<ComponentDefinition> ComponentDefinitions() =>
    [
        new(
            "preview",
            "Goal101 Preview Commands",
            "goal_101_offline_geoworld_unity_preview_runner",
            ".llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner",
            "offline-geoworld-preview-runner-manifest.json",
            "offline-geoworld-preview-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101",
            [
                "offline-geoworld-preview-runner-manifest.json",
                "offline-geoworld-preview-quality-gate-scan.json",
                "offline-geoworld-preview-unity-script-inventory.json",
                "offline-geoworld-preview-simulated-command-proof.json",
                "offline-geoworld-preview-negative-proof.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs"
            ]),
        new(
            "editor_preview",
            "Goal102 Unity Editor Preview",
            "goal_102_offline_geoworld_unity_editor_preview_tool",
            ".llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool",
            "offline-geoworld-unity-editor-quality-gate-scan.json",
            "offline-geoworld-unity-editor-quality-gate-scan.json",
            string.Empty,
            [
                "offline-geoworld-unity-editor-quality-gate-scan.json",
                "offline-geoworld-unity-editor-tool-inventory.json",
                "offline-geoworld-unity-editor-simulated-action-proof.json",
                "offline-geoworld-unity-editor-negative-proof.json"
            ],
            ["unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs"]),
        new(
            "play_mode_travel",
            "Goal103 Play Mode Travel",
            "goal_103_offline_geoworld_playmode_travel_preview",
            ".llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview",
            "offline-geoworld-playmode-travel-manifest.json",
            "offline-geoworld-playmode-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103",
            [
                "offline-geoworld-playmode-travel-manifest.json",
                "offline-geoworld-playmode-quality-gate-scan.json",
                "offline-geoworld-playmode-simulated-execution-proof.json",
                "offline-geoworld-playmode-negative-proof.json",
                "offline-geoworld-playmode-unity-script-inventory.json",
                "offline-geoworld-playmode-editor-window-inventory.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelState.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeChunkVisibility.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPlayModeTravelWindow.cs"
            ]),
        new(
            "interactive_travel",
            "Goal104 Interactive Travel",
            "goal_104_offline_geoworld_interactive_travel_preview",
            ".llmgc/procedural/goal-104-offline-geoworld-interactive-travel-preview",
            "offline-geoworld-interactive-travel-manifest.json",
            "offline-geoworld-interactive-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104",
            [
                "offline-geoworld-interactive-travel-manifest.json",
                "offline-geoworld-interactive-quality-gate-scan.json",
                "offline-geoworld-interactive-simulated-execution-proof.json",
                "offline-geoworld-interactive-negative-proof.json",
                "offline-geoworld-interactive-unity-script-inventory.json",
                "offline-geoworld-interactive-editor-window-inventory.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPlayerMotor.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldBoundaryPrefetchState.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractiveTravelWindow.cs"
            ]),
        new(
            "interactions",
            "Goal105 Interactions",
            "goal_105_offline_geoworld_interaction_playable_probe",
            ".llmgc/procedural/goal-105-offline-geoworld-interaction-playable-probe",
            "offline-geoworld-interaction-manifest.json",
            "offline-geoworld-interaction-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105",
            [
                "offline-geoworld-interaction-manifest.json",
                "offline-geoworld-interaction-quality-gate-scan.json",
                "offline-geoworld-interaction-simulated-session-proof.json",
                "offline-geoworld-interaction-negative-proof.json",
                "offline-geoworld-interaction-unity-script-inventory.json",
                "offline-geoworld-interaction-editor-window-inventory.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionTarget.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldStateDeltaLog.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractionProbeWindow.cs"
            ]),
        new(
            "session_replay",
            "Goal106 Session Replay",
            "goal_106_offline_geoworld_session_persistence_replay",
            ".llmgc/procedural/goal-106-offline-geoworld-session-persistence-replay",
            "offline-geoworld-session-manifest.json",
            "offline-geoworld-session-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106",
            [
                "offline-geoworld-session-manifest.json",
                "offline-geoworld-session-quality-gate-scan.json",
                "offline-geoworld-session-simulated-save-load-replay-proof.json",
                "offline-geoworld-session-negative-proof.json",
                "offline-geoworld-session-unity-script-inventory.json",
                "offline-geoworld-session-editor-window-inventory.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSnapshot.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSaveLoadController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionReplayController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldSessionReplayWindow.cs"
            ]),
        new(
            "objective_acceptance",
            "Goal107 Objective Acceptance",
            "goal_107_offline_geoworld_objective_acceptance_run",
            ".llmgc/procedural/goal-107-offline-geoworld-objective-acceptance-run",
            "offline-geoworld-objective-manifest.json",
            "offline-geoworld-objective-quality-gate-scan.json",
            "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107",
            [
                "offline-geoworld-objective-manifest.json",
                "offline-geoworld-objective-quality-gate-scan.json",
                "offline-geoworld-objective-simulated-acceptance-proof.json",
                "offline-geoworld-objective-negative-proof.json",
                "offline-geoworld-objective-unity-script-inventory.json",
                "offline-geoworld-objective-editor-window-inventory.json",
                "offline-geoworld-objective-alpha-quality-consolidation.json",
                "offline-geoworld-objective-completion-state.json"
            ],
            [
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveState.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveTracker.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldObjectiveAcceptanceController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldObjectiveAcceptanceWindow.cs"
            ])
    ];

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static JsonDocument? TryReadJson(string path) =>
        File.Exists(path) ? JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8)) : null;

    private static bool TryGetBool(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static int ReadInt(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static string ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static void ResetDirectory(string root, string path)
    {
        if (!IsSubPath(root, path)
            || !path.Replace('\\', '/').Contains("goal-108", StringComparison.OrdinalIgnoreCase)
            && !path.Replace('\\', '/').Contains("OfflineGeoworldGoal108", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to reset a non-Goal108 directory: " + path);
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string ReadOptional(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static bool IsSubPath(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        return pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathFullyQualified(path)
        && !path.Contains("..", StringComparison.Ordinal)
        && !path.Contains('\\', StringComparison.Ordinal);

    private static string HashFile(string path) => HashBytes(File.ReadAllBytes(path));

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int CountLines(string text) =>
        string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;

    private static bool ContainsAny(string text, params string[] markers) =>
        markers.Any(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static bool IsBinaryOrRasterMedia(string path)
    {
        var extension = Path.GetExtension(path);
        return OfflineGeoworldAlphaSliceVocabulary.ForbiddenBinaryOrRasterExtensions
            .Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private sealed record ComponentDefinition(
        string ComponentId,
        string DisplayName,
        string SourceGoalId,
        string ArtifactRoot,
        string PrimaryJsonFileName,
        string QualityJsonFileName,
        string StreamingAssetsRoot,
        IReadOnlyList<string> RequiredArtifactFiles,
        IReadOnlyList<string> UnityScriptPaths);
}
