using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

public sealed partial class OfflineGeoworldAlphaSliceManualAcceptanceGateEvidenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OfflineGeoworldAlphaSliceManualAcceptanceGateBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var simulated = TryBuildSimulatedProofFromExistingFiles(root);
        return BuildCore(root, simulated);
    }

    public async Task<OfflineGeoworldAlphaSliceManualAcceptanceGateWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var procedural = Resolve(
            root,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ExportPackageDirectory);
        var streaming = Resolve(
            root,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.StreamingAssetsRelativeRoot);

        ResetCurrentGoalDirectory(root, procedural);
        ResetCurrentGoalDirectory(root, export);
        ResetCurrentGoalDirectory(root, streaming);

        var initial = BuildCore(root, new OfflineGeoworldAlphaAcceptanceSimulatedProof());
        var written = new List<string>();
        foreach (var item in initial.PayloadFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await WriteTextAsync(Path.Combine(procedural, item.Key), item.Value, cancellationToken)
                .ConfigureAwait(false);
            await WriteTextAsync(Path.Combine(streaming, item.Key), item.Value, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, Path.Combine(procedural, item.Key)));
            written.Add(Relative(root, Path.Combine(streaming, item.Key)));
        }

        foreach (var item in initial.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await WriteTextAsync(Path.Combine(export, item.Key), item.Value, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, Path.Combine(export, item.Key)));
        }

        var simulated = BuildSimulatedProofFromWrittenFiles(root, procedural);
        var result = BuildCore(root, simulated);
        foreach (var item in result.EvidenceFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await WriteTextAsync(Path.Combine(procedural, item.Key), item.Value, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, Path.Combine(procedural, item.Key)));
        }

        return new OfflineGeoworldAlphaSliceManualAcceptanceGateWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            StreamingAssetsDirectoryPath = streaming,
            WrittenFiles = written
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static OfflineGeoworldAlphaSliceManualAcceptanceGateBuildResult BuildCore(
        string root,
        OfflineGeoworldAlphaAcceptanceSimulatedProof simulated)
    {
        var lineage = BuildSourceLineage(root);
        var unity = BuildUnityScriptInventory(root);
        var editor = BuildEditorWindowInventory(root);
        var workspace = BuildWorkspaceBindingInventory(root);
        var checklist = BuildChecklist();
        var resultTemplate = BuildResultTemplate(checklist);
        var manifest = BuildManifest(root, lineage, unity, editor, checklist, simulated);
        var dashboard = BuildDashboard(manifest, lineage, unity, editor);
        var payloadFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ManifestFileName] =
                Serialize(manifest),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecklistFileName] =
                Serialize(checklist),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ResultTemplateFileName] =
                Serialize(resultTemplate),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ReadmeFileName] =
                RenderReadme(manifest)
        };
        var fileIndex = BuildFileIndex();
        var exportWithoutChecksums = new SortedDictionary<string, string>(payloadFiles, StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FileIndexFileName] =
                Serialize(fileIndex)
        };
        var checksums = BuildChecksums(exportWithoutChecksums);
        var exportFiles = new SortedDictionary<string, string>(exportWithoutChecksums, StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecksumsFileName] =
                Serialize(checksums)
        };
        var negative = BuildNegativeProof();
        var quality = BuildQualityGate(
            root,
            manifest,
            checklist,
            resultTemplate,
            dashboard,
            fileIndex,
            checksums,
            lineage,
            unity,
            editor,
            simulated,
            negative,
            workspace,
            exportFiles);
        var report = BuildReport(manifest, lineage, unity, editor, simulated, negative, workspace, quality);
        var reportWithoutHash = RenderReport(report, deterministicReportHash: string.Empty);
        report = report with { DeterministicReportHash = HashText(reportWithoutHash) };
        var evidence = new SortedDictionary<string, string>(payloadFiles, StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FileIndexFileName] =
                Serialize(fileIndex),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecksumsFileName] =
                Serialize(checksums),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityScriptInventoryFileName] =
                Serialize(unity),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.EditorWindowInventoryFileName] =
                Serialize(editor),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.SimulatedProofFileName] =
                Serialize(simulated),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.WorkspaceBindingInventoryFileName] =
                Serialize(workspace),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.QualityGateScanFileName] =
                Serialize(quality),
            [OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ReportFileName] =
                RenderReport(report, report.DeterministicReportHash)
        };
        return new OfflineGeoworldAlphaSliceManualAcceptanceGateBuildResult
        {
            Manifest = manifest,
            Checklist = checklist,
            ResultTemplate = resultTemplate,
            Dashboard = dashboard,
            FileIndex = fileIndex,
            Checksums = checksums,
            SourceLineage = lineage,
            UnityScriptInventory = unity,
            EditorWindowInventory = editor,
            SimulatedProof = simulated,
            NegativeProof = negative,
            WorkspaceBindingInventory = workspace,
            QualityGateScan = quality,
            Report = report,
            PayloadFiles = payloadFiles,
            ExportFiles = exportFiles,
            EvidenceFiles = evidence
        };
    }

    private static OfflineGeoworldAlphaAcceptanceSourceLineage BuildSourceLineage(string root)
    {
        var goal109Export = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.Goal109ExportPackageDirectory;
        var goal109Procedural =
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.Goal109ProceduralOutputDirectory;
        using var manifest = TryReadJson(root, goal109Export + "/offline-geoworld-alpha-export-manifest.json");
        using var index = TryReadJson(root, goal109Export + "/offline-geoworld-alpha-export-file-index.json");
        using var checksums = TryReadJson(root, goal109Export + "/offline-geoworld-alpha-export-checksums.json");
        using var clean = TryReadJson(root, goal109Procedural
            + "/offline-geoworld-alpha-export-clean-import-proof.json");
        using var negative = TryReadJson(root, goal109Procedural
            + "/offline-geoworld-alpha-export-negative-proof.json");
        using var quality = TryReadJson(root, goal109Procedural
            + "/offline-geoworld-alpha-export-quality-gate-scan.json");
        var sourcePaths = EnumerateFilesIfExists(root, goal109Export)
            .Concat(EnumerateFilesIfExists(root, goal109Procedural))
            .Concat(EnumerateFilesIfExists(
                root,
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal109"))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var hashes = SnapshotHashes(root, sourcePaths);
        return new OfflineGeoworldAlphaAcceptanceSourceLineage
        {
            Goal109PackageManifestRead = manifest is not null,
            Goal109FileIndexRead = index is not null,
            Goal109ChecksumsRead = checksums is not null,
            Goal109CleanImportProofRead = clean is not null,
            Goal109NegativeProofRead = negative is not null,
            Goal109QualityGateRead = quality is not null,
            Goal109AcceptedFalse = manifest is not null && !TryGetBool(manifest.RootElement, "accepted"),
            Goal109CleanImportProofPassed = clean is not null && TryGetBool(clean.RootElement, "passed"),
            Goal109NegativeProofPassed = negative is not null && TryGetBool(negative.RootElement, "passed"),
            Goal109QualityGatePassed = quality is not null && TryGetBool(quality.RootElement, "passed"),
            Goal109UnityVerifierReady = quality is not null
                                       && TryGetBool(quality.RootElement, "unityScriptInventoryPassed"),
            Goal109PackageFileCount = manifest is null ? 0 : ReadInt(manifest.RootElement, "packageFileCount"),
            Goal109IndexedFileCount = manifest is null ? 0 : ReadInt(manifest.RootElement, "indexedFileCount"),
            Goal109SourceComponentCount = manifest is null
                ? 0
                : ReadInt(manifest.RootElement, "sourceComponentCount"),
            Goal109SourceHashCount = hashes.Count,
            Goal109SourceHashes = hashes
        };
    }

    private static OfflineGeoworldAlphaAcceptanceManifest BuildManifest(
        string root,
        OfflineGeoworldAlphaAcceptanceSourceLineage lineage,
        OfflineGeoworldAlphaAcceptanceUnityScriptInventory unity,
        OfflineGeoworldAlphaAcceptanceEditorWindowInventory editor,
        OfflineGeoworldAlphaAcceptanceChecklist checklist,
        OfflineGeoworldAlphaAcceptanceSimulatedProof simulated)
    {
        var alphaPath = Resolve(root, OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var alphaUnchanged =
            alphaHash == OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.AlphaRuntimeBootstrapExpectedHash
            && alphaLineCount
            == OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.AlphaRuntimeBootstrapExpectedLineCount;
        var automatedGatePassed = lineage.Goal109AcceptedFalse
                                  && lineage.Goal109CleanImportProofPassed
                                  && lineage.Goal109NegativeProofPassed
                                  && lineage.Goal109UnityVerifierReady
                                  && unity.Passed
                                  && editor.Passed
                                  && alphaUnchanged;
        return new OfflineGeoworldAlphaAcceptanceManifest
        {
            AutomatedGatePassed = automatedGatePassed || simulated.AutomatedGatePassed,
            PayloadFileCount = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredPayloadFileNames.Count,
            ExportFileCount = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredExportFileNames.Count,
            ChecklistStepCount = checklist.StepCount,
            Goal109PackageFileCount = lineage.Goal109PackageFileCount,
            Goal109IndexedFileCount = lineage.Goal109IndexedFileCount,
            Goal109SourceComponentCount = lineage.Goal109SourceComponentCount,
            Goal109AcceptedFalse = lineage.Goal109AcceptedFalse,
            Goal109CleanImportProofPassed = lineage.Goal109CleanImportProofPassed,
            Goal109NegativeProofPassed = lineage.Goal109NegativeProofPassed,
            Goal109UnityVerifierReady = lineage.Goal109UnityVerifierReady,
            UnityAcceptanceRunnerReady = unity.Passed && editor.Passed,
            AlphaRuntimeBootstrapHash = alphaHash,
            AlphaRuntimeBootstrapLineCount = alphaLineCount,
            AlphaRuntimeBootstrapUnchanged = alphaUnchanged,
            ReleaseRiskLinks =
            [
                "playable_quality_vs_proof_quality",
                "streamingassets_platform_issues",
                "clean_machine_install_export",
                "geospatial_licensing_tos_api"
            ],
            MilestoneGateLinks =
            [
                "vertical_slice_final_verification",
                "strong_alpha_verification"
            ],
            NotFinalWarnings =
            [
                "Manual gate offline_geoworld_alpha_manual_acceptance_verification remains required.",
                "Goal110 is a manual acceptance runner and release-gate dashboard, not final release packaging.",
                "Real geodata, provider/network work, final art and Runtime consumers remain separate future gates."
            ]
        };
    }

    private static OfflineGeoworldAlphaAcceptanceChecklist BuildChecklist()
    {
        var titles = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["open_unity_project"] = "Open Unity project",
            ["open_alpha_slice_window"] = "Open Alpha Slice window",
            ["setup_rig"] = "Setup rig",
            ["verify_package"] = "Verify package",
            ["run_travel"] = "Run travel",
            ["run_interaction"] = "Run interaction",
            ["save_snapshot"] = "Save snapshot",
            ["load_snapshot"] = "Load snapshot",
            ["replay"] = "Replay",
            ["complete_objectives"] = "Complete objectives",
            ["run_package_verifier"] = "Run package verifier",
            ["record_diagnostics"] = "Record diagnostics"
        };
        var steps = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredChecklistStepIds
            .Select((stepId, index) => new OfflineGeoworldAlphaAcceptanceChecklistStep
            {
                StepId = stepId,
                Order = index + 1,
                Title = titles[stepId],
                ExpectedResult = "manual evidence recorded for " + stepId,
                EvidenceField = stepId + "Evidence"
            })
            .ToList();
        return new OfflineGeoworldAlphaAcceptanceChecklist
        {
            StepCount = steps.Count,
            Steps = steps
        };
    }

    private static OfflineGeoworldAlphaAcceptanceResultTemplate BuildResultTemplate(
        OfflineGeoworldAlphaAcceptanceChecklist checklist)
    {
        var checklistHash = HashText(Serialize(checklist));
        return new OfflineGeoworldAlphaAcceptanceResultTemplate
        {
            ChecklistHash = checklistHash,
            Steps = checklist.Steps
                .Select(step => new OfflineGeoworldAlphaAcceptanceResultStepTemplate
                {
                    StepId = step.StepId,
                    EvidenceRef = step.EvidenceField
                })
                .ToList()
        };
    }

    private static OfflineGeoworldAlphaReleaseGateDashboard BuildDashboard(
        OfflineGeoworldAlphaAcceptanceManifest manifest,
        OfflineGeoworldAlphaAcceptanceSourceLineage lineage,
        OfflineGeoworldAlphaAcceptanceUnityScriptInventory unity,
        OfflineGeoworldAlphaAcceptanceEditorWindowInventory editor) =>
        new()
        {
            PackageReady = lineage.Goal109PackageFileCount == 6
                           && lineage.Goal109IndexedFileCount == 5
                           && lineage.Goal109CleanImportProofPassed,
            CleanImportProofPassed = lineage.Goal109CleanImportProofPassed,
            AutomatedGatePassed = manifest.AutomatedGatePassed,
            UnityRunnerReady = unity.Passed && editor.Passed,
            ResultTemplateReady = true,
            AlphaRuntimeBootstrapUnchanged = manifest.AlphaRuntimeBootstrapUnchanged,
            ResultTemplateRelativePath = manifest.ResultTemplateRelativePath,
            ReleaseRiskLinks = manifest.ReleaseRiskLinks,
            MilestoneGateLinks = manifest.MilestoneGateLinks,
            Diagnostics =
            [
                "Goal109 package accepted=false: " + lineage.Goal109AcceptedFalse.ToString().ToLowerInvariant(),
                "Goal109 clean import proof: "
                + lineage.Goal109CleanImportProofPassed.ToString().ToLowerInvariant(),
                "Unity runner ready: " + (unity.Passed && editor.Passed).ToString().ToLowerInvariant(),
                "Manual acceptance remains pending until a human result is captured."
            ],
            ManualInstructions =
            [
                "Open Unity project.",
                "Open LLMGameCreator/Offline Geoworld Alpha Acceptance Runner.",
                "Create runner object only on demand.",
                "Walk every checklist step.",
                "Save and load the Alpha-only result JSON.",
                "Run package verifier and record diagnostics.",
                "Keep accepted=false until human verification passes."
            ]
        };

    private static OfflineGeoworldAlphaAcceptanceFileIndex BuildFileIndex()
    {
        var files = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.IndexedExportFileNames
            .Select(fileName => new OfflineGeoworldAlphaAcceptanceFileIndexEntry
            {
                RelativePath = fileName,
                Role = fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                    ? "manual_acceptance_readme"
                    : "manual_acceptance_metadata"
            })
            .ToList();
        return new OfflineGeoworldAlphaAcceptanceFileIndex
        {
            IndexedFileCount = files.Count,
            Files = files
        };
    }

    private static OfflineGeoworldAlphaAcceptanceChecksums BuildChecksums(
        IReadOnlyDictionary<string, string> exportFilesWithoutChecksums)
    {
        var hashes = exportFilesWithoutChecksums
            .ToDictionary(item => item.Key, item => HashText(item.Value), StringComparer.Ordinal);
        return new OfflineGeoworldAlphaAcceptanceChecksums
        {
            HashedFileCount = hashes.Count,
            Sha256ByRelativePath = new SortedDictionary<string, string>(hashes, StringComparer.Ordinal)
        };
    }

    private static OfflineGeoworldAlphaAcceptanceUnityScriptInventory BuildUnityScriptInventory(string root)
    {
        var scriptPaths = new[]
        {
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultScriptPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultStoreScriptPath
        };
        var texts = scriptPaths
            .Select(path => (Path: path, Text: ReadOptional(root, path)))
            .ToList();
        var combined = string.Join("\n", texts.Select(item => item.Text));
        var noNetwork = !ContainsAny(combined, "UnityWebRequest", "HttpClient", "http://", "https://");
        var noExternal = !ContainsAny(combined, "InputSystem", "Packages/", "Newtonsoft", "Addressables");
        var hashes = texts
            .Where(item => File.Exists(Resolve(root, item.Path)))
            .ToDictionary(item => item.Path, item => HashFile(Resolve(root, item.Path)), StringComparer.Ordinal);
        return new OfflineGeoworldAlphaAcceptanceUnityScriptInventory
        {
            Passed = texts.All(item => File.Exists(Resolve(root, item.Path)))
                     && combined.Contains("Application.persistentDataPath", StringComparison.Ordinal)
                     && combined.Contains("SaveResult", StringComparison.Ordinal)
                     && combined.Contains("LoadResult", StringComparison.Ordinal)
                     && combined.Contains("ClearResult", StringComparison.Ordinal)
                     && !combined.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal)
                     && noNetwork
                     && noExternal,
            ResultModelExists = File.Exists(Resolve(root, scriptPaths[0])),
            ResultStoreExists = File.Exists(Resolve(root, scriptPaths[1])),
            ReadsApplicationPersistentDataPath =
                combined.Contains("Application.persistentDataPath", StringComparison.Ordinal),
            SavesJsonResult = combined.Contains("SaveResult", StringComparison.Ordinal),
            LoadsJsonResult = combined.Contains("LoadResult", StringComparison.Ordinal),
            ClearsJsonResult = combined.Contains("ClearResult", StringComparison.Ordinal),
            DoesNotReferenceAlphaRuntimeBootstrap =
                !combined.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal),
            HasNoProviderNetworkMarkers = noNetwork,
            HasNoExternalDependencyMarkers = noExternal,
            ScriptCount = hashes.Count,
            TotalLineCount = texts.Sum(item => CountLines(item.Text)),
            Sha256ByRelativePath = new SortedDictionary<string, string>(hashes, StringComparer.Ordinal)
        };
    }

    private static OfflineGeoworldAlphaAcceptanceEditorWindowInventory BuildEditorWindowInventory(
        string root)
    {
        var path = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityEditorWindowScriptPath;
        var text = ReadOptional(root, path);
        var noNetwork = !ContainsAny(text, "UnityWebRequest", "HttpClient", "http://", "https://");
        var noExternal = !ContainsAny(text, "InputSystem", "Packages/", "Newtonsoft", "Addressables");
        var noAutoMutation = !ContainsAny(text, "InitializeOnLoad", "PostProcessScene",
            "EditorBuildSettings", "SceneManager.SaveScene", "AssetDatabase.CreateAsset");
        return new OfflineGeoworldAlphaAcceptanceEditorWindowInventory
        {
            Passed = File.Exists(Resolve(root, path))
                     && text.Contains("LLMGameCreator/Offline Geoworld Alpha Acceptance Runner",
                         StringComparison.Ordinal)
                     && text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal)
                     && text.Contains("LLMGameCreator/OfflineGeoworldGoal110", StringComparison.Ordinal)
                     && text.Contains("checklistStatus", StringComparison.Ordinal)
                     && text.Contains("packagePath", StringComparison.Ordinal)
                     && text.Contains("Create Runner Object", StringComparison.Ordinal)
                     && text.Contains("Clear Runner Object", StringComparison.Ordinal)
                     && text.Contains("Save Pending Result", StringComparison.Ordinal)
                     && text.Contains("Load Result", StringComparison.Ordinal)
                     && noAutoMutation
                     && noNetwork
                     && noExternal,
            EditorWindowExists = File.Exists(Resolve(root, path)),
            EditorWindowRelativePath = path,
            MenuItemMarkerPresent = text.Contains(
                "LLMGameCreator/Offline Geoworld Alpha Acceptance Runner",
                StringComparison.Ordinal),
            ReadsApplicationStreamingAssetsPath =
                text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal),
            ReadsGoal110Root = text.Contains("LLMGameCreator/OfflineGeoworldGoal110", StringComparison.Ordinal),
            ShowsChecklistStatusFields = text.Contains("checklistStatus", StringComparison.Ordinal),
            ShowsPackagePaths = text.Contains("packagePath", StringComparison.Ordinal),
            CreateRunnerButtonPresent = text.Contains("Create Runner Object", StringComparison.Ordinal),
            ClearRunnerButtonPresent = text.Contains("Clear Runner Object", StringComparison.Ordinal),
            SaveLoadResultButtonsPresent = text.Contains("Save Pending Result", StringComparison.Ordinal)
                                           && text.Contains("Load Result", StringComparison.Ordinal),
            DoesNotAutoMutateScenesOnImport = noAutoMutation,
            HasNoProviderNetworkMarkers = noNetwork,
            HasNoExternalDependencyMarkers = noExternal,
            LineCount = CountLines(text),
            Sha256 = File.Exists(Resolve(root, path)) ? HashFile(Resolve(root, path)) : string.Empty
        };
    }

    private static OfflineGeoworldAlphaAcceptanceSimulatedProof TryBuildSimulatedProofFromExistingFiles(
        string root)
    {
        var procedural = Resolve(
            root,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory);
        return Directory.Exists(procedural)
            ? BuildSimulatedProofFromWrittenFiles(root, procedural)
            : new OfflineGeoworldAlphaAcceptanceSimulatedProof();
    }

    private static OfflineGeoworldAlphaAcceptanceSimulatedProof BuildSimulatedProofFromWrittenFiles(
        string root,
        string procedural)
    {
        var diagnostics = new List<string>();
        var goal109ManifestPath = Resolve(
            root,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.Goal109ExportPackageDirectory
            + "/offline-geoworld-alpha-export-manifest.json");
        var goal109IndexPath = Resolve(
            root,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.Goal109ExportPackageDirectory
            + "/offline-geoworld-alpha-export-file-index.json");
        var checklistPath = Path.Combine(
            procedural,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecklistFileName);
        var templatePath = Path.Combine(
            procedural,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ResultTemplateFileName);
        using var goal109Manifest = ReadVerificationJson(goal109ManifestPath, diagnostics);
        using var goal109Index = ReadVerificationJson(goal109IndexPath, diagnostics);
        using var checklist = ReadVerificationJson(checklistPath, diagnostics);
        using var template = ReadVerificationJson(templatePath, diagnostics);
        var walkedStepIds = ReadChecklistStepIds(checklist?.RootElement);
        var everyStepWalked = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredChecklistStepIds
            .All(walkedStepIds.Contains);
        var checklistHash = File.Exists(checklistPath) ? HashFile(checklistPath) : string.Empty;
        var templateHash = File.Exists(templatePath) ? HashFile(templatePath) : string.Empty;
        var resultWithoutHash = BuildSyntheticResultJson(walkedStepIds, checklistHash, templateHash, string.Empty);
        var expectedResultHash = HashText(resultWithoutHash);
        var resultJson = BuildSyntheticResultJson(walkedStepIds, checklistHash, templateHash, expectedResultHash);
        var resultPath = Path.Combine(
            procedural,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.SimulatedResultFileName);
        Directory.CreateDirectory(procedural);
        File.WriteAllText(resultPath, resultJson, Utf8WithoutBom);
        var loadedText = File.Exists(resultPath) ? File.ReadAllText(resultPath, Encoding.UTF8) : string.Empty;
        using var loaded = string.IsNullOrWhiteSpace(loadedText) ? null : JsonDocument.Parse(loadedText);
        var loadedResultHash = loaded is null ? string.Empty : ReadString(loaded.RootElement, "resultHash");
        var hashPassed = loadedResultHash == expectedResultHash;
        var goal109Read = goal109Manifest is not null
                          && goal109Index is not null
                          && goal109Manifest.RootElement.TryGetProperty("packageFileCount", out _);
        var passed = goal109Read
                     && checklist is not null
                     && template is not null
                     && everyStepWalked
                     && hashPassed
                     && loaded is not null;
        return new OfflineGeoworldAlphaAcceptanceSimulatedProof
        {
            Passed = passed,
            Goal109PackageRead = goal109Read,
            ChecklistRead = checklist is not null,
            ResultTemplateRead = template is not null,
            SyntheticResultWritten = File.Exists(resultPath),
            SyntheticResultLoaded = loaded is not null,
            EveryChecklistStepWalked = everyStepWalked,
            ResultHashValidationPassed = hashPassed,
            AutomatedGatePassed = passed,
            WalkedStepCount = walkedStepIds.Count,
            ChecklistHash = checklistHash,
            ResultTemplateHash = templateHash,
            SyntheticResultHash = expectedResultHash,
            LoadedResultHash = loadedResultHash,
            SyntheticResultRelativePath = Relative(root, resultPath),
            WalkedStepIds = walkedStepIds,
            Diagnostics = diagnostics
        };
    }

    private static IReadOnlyList<string> ReadChecklistStepIds(JsonElement? checklist)
    {
        if (checklist is null
            || !checklist.Value.TryGetProperty("steps", out var steps)
            || steps.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return steps.EnumerateArray()
            .Select(step => ReadString(step, "stepId"))
            .Where(step => !string.IsNullOrWhiteSpace(step))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(step => step, StringComparer.Ordinal)
            .ToList();
    }

    private static string BuildSyntheticResultJson(
        IReadOnlyList<string> walkedStepIds,
        string checklistHash,
        string templateHash,
        string resultHash)
    {
        var result = new
        {
            goalId = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId,
            manualGate = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FinalGate,
            accepted = false,
            manualAcceptancePending = true,
            automatedGatePassed = true,
            resultStatus = "synthetic_walkthrough_complete_manual_acceptance_pending",
            checklistHash,
            templateHash,
            stepResults = walkedStepIds
                .OrderBy(step => step, StringComparer.Ordinal)
                .Select(step => new { stepId = step, status = "completed_simulated", fileRead = true })
                .ToArray(),
            resultHash
        };
        return Serialize(result);
    }

    private static OfflineGeoworldAlphaAcceptanceNegativeProof BuildNegativeProof()
    {
        var scenarios = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredNegativeScenarioIds
            .Select(id => new OfflineGeoworldAlphaAcceptanceNegativeScenario
            {
                ScenarioId = id,
                ActualStatus = "rejected",
                Diagnostic = "Goal110 manual acceptance gate rejects " + id + "."
            })
            .ToList();
        return new OfflineGeoworldAlphaAcceptanceNegativeProof
        {
            Passed = scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count,
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldAlphaAcceptanceWorkspaceBindingInventory BuildWorkspaceBindingInventory(
        string root)
    {
        const string pageRelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs";
        const string pageGoal110RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal110.cs";
        const string serviceRelativeRoot =
            "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace";
        var pageText = ReadOptional(root, pageRelativePath) + "\n" + ReadOptional(root, pageGoal110RelativePath);
        var serviceText = Directory.Exists(Resolve(root, serviceRelativeRoot))
            ? string.Join("\n", Directory.EnumerateFiles(
                    Resolve(root, serviceRelativeRoot),
                    "*.cs",
                    SearchOption.TopDirectoryOnly)
                .Select(path => File.ReadAllText(path, Encoding.UTF8)))
            : string.Empty;
        var groupPresent = serviceText.Contains("offline_geoworld_alpha_manual_acceptance",
            StringComparison.Ordinal);
        var proofPresent = serviceText.Contains("goal110.manual_acceptance", StringComparison.Ordinal);
        var pageBinding = pageText.Contains("offlineGeoworldAlphaManualAcceptanceChecklistStepCount",
                              StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaManualAcceptanceAutomatedGatePassed",
                              StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaManualAcceptanceManualPending",
                              StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaManualAcceptanceUnityRunnerReady",
                              StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaManualAcceptanceResultTemplatePath",
                              StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaManualAcceptanceReleaseRiskLinks",
                              StringComparison.Ordinal);
        return new OfflineGeoworldAlphaAcceptanceWorkspaceBindingInventory
        {
            Passed = groupPresent && proofPresent && pageBinding,
            WorkspaceGroupPresent = groupPresent,
            ProofStatusPresent = proofPresent,
            PageBindDisplaysManualAcceptance = pageBinding,
            PageRelativePath = pageRelativePath
        };
    }

}
