using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;

public sealed partial class OfflineGeoworldAlphaSliceExportPackageEvidenceService
{
    private const string Goal108Root =
        ".llmgc/procedural/goal-108-offline-geoworld-alpha-slice-orchestrator";
    private const string Goal108ARoot =
        ".llmgc/procedural/goal-108a-alpha-slice-source-split-immutability-audit";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public OfflineGeoworldAlphaSliceExportPackageBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var cleanImport = new OfflineGeoworldAlphaSliceExportCleanImportProof();
        return BuildCore(root, cleanImport);
    }

    public async Task<OfflineGeoworldAlphaSliceExportPackageWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var procedural = Resolve(root, OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory);
        var export = Resolve(root, OfflineGeoworldAlphaSliceExportPackageVocabulary.ExportPackageDirectory);
        var streaming = Resolve(root, OfflineGeoworldAlphaSliceExportPackageVocabulary.StreamingAssetsRelativeRoot);

        ResetCurrentGoalDirectory(root, procedural);
        ResetCurrentGoalDirectory(root, export);
        ResetCurrentGoalDirectory(root, streaming);

        var initial = BuildCore(root, new OfflineGeoworldAlphaSliceExportCleanImportProof());
        var written = new List<string>();
        foreach (var item in initial.PackageFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await WriteTextAsync(Path.Combine(procedural, item.Key), item.Value, cancellationToken)
                .ConfigureAwait(false);
            await WriteTextAsync(Path.Combine(export, item.Key), item.Value, cancellationToken)
                .ConfigureAwait(false);
            await WriteTextAsync(Path.Combine(streaming, item.Key), item.Value, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, Path.Combine(procedural, item.Key)));
            written.Add(Relative(root, Path.Combine(export, item.Key)));
            written.Add(Relative(root, Path.Combine(streaming, item.Key)));
        }

        var cleanImport = VerifyPackage(export);
        var result = BuildCore(root, cleanImport);
        foreach (var item in result.EvidenceFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await WriteTextAsync(Path.Combine(procedural, item.Key), item.Value, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, Path.Combine(procedural, item.Key)));
        }

        return new OfflineGeoworldAlphaSliceExportPackageWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            StreamingAssetsDirectoryPath = streaming,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    public OfflineGeoworldAlphaSliceExportCleanImportProof VerifyPackage(string packageRootPath)
    {
        var root = Path.GetFullPath(packageRootPath);
        var diagnostics = new List<string>();
        var manifestPath = Path.Combine(root, OfflineGeoworldAlphaSliceExportPackageVocabulary.ManifestFileName);
        var indexPath = Path.Combine(root, OfflineGeoworldAlphaSliceExportPackageVocabulary.FileIndexFileName);
        var checksumsPath = Path.Combine(root, OfflineGeoworldAlphaSliceExportPackageVocabulary.ChecksumsFileName);
        using var manifest = ReadVerificationJson(manifestPath, diagnostics);
        using var index = ReadVerificationJson(indexPath, diagnostics);
        using var checksums = ReadVerificationJson(checksumsPath, diagnostics);
        var requiredPresent = OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredPackageFileNames
            .All(fileName => File.Exists(Path.Combine(root, fileName)));
        var indexed = ReadIndexedFileNames(index?.RootElement);
        var checksumMap = ReadChecksumMap(checksums?.RootElement);
        var readTexts = new List<string>();
        var allIndexedPresent = true;
        var checksumsMatch = true;
        foreach (var relativePath in indexed)
        {
            if (!IsSafeRelativePath(relativePath))
            {
                diagnostics.Add("unsafe-index-path:" + relativePath);
                allIndexedPresent = false;
                continue;
            }

            var path = Path.Combine(root, relativePath);
            if (!File.Exists(path))
            {
                diagnostics.Add("missing-indexed-file:" + relativePath);
                allIndexedPresent = false;
                continue;
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            readTexts.Add(text);
            if (!checksumMap.TryGetValue(relativePath, out var expected)
                || !HashFile(path).Equals(expected, StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add("checksum-mismatch:" + relativePath);
                checksumsMatch = false;
            }
        }

        var combined = string.Join("\n", readTexts);
        var noAbsolutePaths = !Path.IsPathFullyQualified(combined)
                              && !ContainsAny(combined, root, "C:\\", "C:/", "\\\\");
        var noRawGeodata = !ContainsAny(combined, ".osm", ".pbf", ".mbtiles", ".gpkg", ".geojson",
            "\"rawGeodataIncluded\": true", "\"noRawGeodata\": false");
        var noNetwork = !ContainsAny(combined, "UnityWebRequest", "HttpClient", "http://", "https://",
            "\"containsProviderCalls\": true", "\"networkProviderMarker\": true");
        var noBinary = indexed.All(path => !IsBinaryOrRasterMedia(path))
                       && !ContainsAny(combined, ".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp",
                           ".wav", ".mp3", ".ogg", ".mp4", ".bytes");
        var gatesListed = RequiredManualGatesPresent(manifest?.RootElement);
        var goal107Included = manifest is not null
                              && !string.IsNullOrWhiteSpace(ReadString(
                                  manifest.RootElement,
                                  "goal107FinalAcceptanceHash"));
        var goal108AIncluded = manifest is not null
                               && TryGetBool(manifest.RootElement, "goal108AImmutabilityAuditIncluded");
        var passed = manifest is not null
                     && index is not null
                     && checksums is not null
                     && requiredPresent
                     && allIndexedPresent
                     && checksumsMatch
                     && noAbsolutePaths
                     && noRawGeodata
                     && noNetwork
                     && noBinary
                     && gatesListed
                     && goal107Included
                     && goal108AIncluded;
        return new OfflineGeoworldAlphaSliceExportCleanImportProof
        {
            Passed = passed,
            PackageRootReadAttempted = true,
            ManifestPresent = manifest is not null,
            FileIndexPresent = index is not null,
            ChecksumsPresent = checksums is not null,
            AllRequiredFilesPresent = requiredPresent,
            AllIndexedFilesPresent = allIndexedPresent,
            ChecksumsMatch = checksumsMatch,
            NoAbsolutePaths = noAbsolutePaths,
            NoRawGeodata = noRawGeodata,
            NoNetworkProviderMarkers = noNetwork,
            NoBinaryOrRasterMedia = noBinary,
            ManualGatesListed = gatesListed,
            Goal107FinalObjectiveAcceptanceIncluded = goal107Included,
            Goal108ASourceSplitImmutabilityAuditIncluded = goal108AIncluded,
            IndexedFileCount = indexed.Count,
            ReadFileCount = readTexts.Count,
            Diagnostics = diagnostics
        };
    }

    private static OfflineGeoworldAlphaSliceExportPackageBuildResult BuildCore(
        string root,
        OfflineGeoworldAlphaSliceExportCleanImportProof cleanImport)
    {
        var lineage = BuildSourceLineage(root);
        var manifest = BuildManifest(root, lineage);
        var fileIndex = BuildFileIndex();
        var acceptanceGate = BuildAcceptanceGate(manifest);
        var packageFilesWithoutChecksums = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.ManifestFileName] = Serialize(manifest),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.FileIndexFileName] = Serialize(fileIndex),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.RunbookFileName] = RenderRunbook(manifest),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.AcceptanceGateFileName] = Serialize(acceptanceGate),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.ReadmeFileName] = RenderReadme(manifest)
        };
        var checksums = BuildChecksums(packageFilesWithoutChecksums);
        var packageFiles = new SortedDictionary<string, string>(packageFilesWithoutChecksums, StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.ChecksumsFileName] = Serialize(checksums)
        };
        var negative = BuildNegativeProof();
        var unity = BuildUnityScriptInventory(root);
        var editor = BuildEditorWindowInventory(root);
        var workspace = BuildWorkspaceBindingInventory(root);
        var quality = BuildQualityGate(root, manifest, fileIndex, checksums, cleanImport, negative, unity,
            editor, workspace, lineage, packageFiles);
        var report = BuildReport(manifest, cleanImport, negative, unity, editor, workspace, quality);
        var reportWithoutHash = RenderReport(report, deterministicReportHash: string.Empty);
        report = report with { DeterministicReportHash = HashText(reportWithoutHash) };
        var evidence = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.CleanImportProofFileName] = Serialize(cleanImport),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.NegativeProofFileName] = Serialize(negative),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityScriptInventoryFileName] = Serialize(unity),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.EditorWindowInventoryFileName] = Serialize(editor),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.WorkspaceBindingInventoryFileName] = Serialize(workspace),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.SourceLineageFileName] = Serialize(lineage),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.QualityGateScanFileName] = Serialize(quality),
            [OfflineGeoworldAlphaSliceExportPackageVocabulary.ReportFileName] =
                RenderReport(report, report.DeterministicReportHash)
        };
        return new OfflineGeoworldAlphaSliceExportPackageBuildResult
        {
            Manifest = manifest,
            FileIndex = fileIndex,
            Checksums = checksums,
            AcceptanceGate = acceptanceGate,
            SourceLineage = lineage,
            CleanImportProof = cleanImport,
            NegativeProof = negative,
            UnityScriptInventory = unity,
            EditorWindowInventory = editor,
            WorkspaceBindingInventory = workspace,
            QualityGateScan = quality,
            Report = report,
            PackageFiles = packageFiles,
            EvidenceFiles = evidence
        };
    }

    private static OfflineGeoworldAlphaSliceExportSourceLineage BuildSourceLineage(string root)
    {
        using var goal108Manifest = TryReadJson(root, Goal108Root + "/"
            + OfflineGeoworldAlphaSliceVocabulary.ManifestFileName);
        using var goal108Components = TryReadJson(root, Goal108Root + "/"
            + OfflineGeoworldAlphaSliceVocabulary.ComponentsFileName);
        using var goal108Proof = TryReadJson(root, Goal108Root + "/"
            + OfflineGeoworldAlphaSliceVocabulary.SimulatedProofFileName);
        using var goal108Negative = TryReadJson(root, Goal108Root + "/"
            + OfflineGeoworldAlphaSliceVocabulary.NegativeProofFileName);
        using var goal108AAudit = TryReadJson(root, Goal108ARoot + "/alpha-slice-immutability-trust-audit.json");
        using var goal108AReport = TryReadJson(root, Goal108ARoot + "/alpha-slice-source-split-quality-gate.json");
        using var goal108ADiff = TryReadJson(root, Goal108ARoot + "/alpha-slice-historical-artifact-diff-audit.json");

        var components = ReadSourceComponents(goal108Components?.RootElement);
        var sourcePaths = components
            .SelectMany(component => EnumerateFilesIfExists(root, component.SourceArtifactRoot))
            .Concat(EnumerateFilesIfExists(root,
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal108"))
            .Concat([
                Goal108Root + "/" + OfflineGeoworldAlphaSliceVocabulary.ManifestFileName,
                Goal108Root + "/" + OfflineGeoworldAlphaSliceVocabulary.ComponentsFileName,
                Goal108Root + "/" + OfflineGeoworldAlphaSliceVocabulary.SimulatedProofFileName,
                Goal108Root + "/" + OfflineGeoworldAlphaSliceVocabulary.NegativeProofFileName,
                Goal108ARoot + "/alpha-slice-immutability-trust-audit.json",
                Goal108ARoot + "/alpha-slice-historical-artifact-diff-audit.json"
            ])
            .Distinct(StringComparer.Ordinal)
            .Where(path => File.Exists(Resolve(root, path)))
            .ToList();
        var sourceHashes = SnapshotHashes(root, sourcePaths);
        var unchanged = goal108Proof is not null
                        && TryGetBool(goal108Proof.RootElement, "historicalArtifactsUnchanged")
                        && goal108AAudit is not null
                        && TryGetBool(goal108AAudit.RootElement, "actualGoal101To107ArtifactsUnchanged");
        return new OfflineGeoworldAlphaSliceExportSourceLineage
        {
            Goal108ManifestRead = goal108Manifest is not null,
            Goal108ComponentsRead = goal108Components is not null,
            Goal108SimulatedProofRead = goal108Proof is not null,
            Goal108NegativeProofRead = goal108Negative is not null,
            Goal108AImmutabilityAuditRead = goal108AAudit is not null,
            Goal108ASourceSplitReportRead = goal108AReport is not null,
            Goal108AHistoricalDiffAuditRead = goal108ADiff is not null,
            Goal101To107ArtifactsUnchanged = unchanged,
            ComponentCount = components.Count,
            ReadyComponentCount = components.Count(component => component.Ready),
            SourceHashCount = sourceHashes.Count,
            Components = components,
            SourceArtifactHashes = sourceHashes
        };
    }

    private static IReadOnlyList<OfflineGeoworldAlphaSliceExportSourceComponent> ReadSourceComponents(
        JsonElement? root)
    {
        if (root is null
            || !root.Value.TryGetProperty("components", out var components)
            || components.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return components.EnumerateArray()
            .Select(component => new OfflineGeoworldAlphaSliceExportSourceComponent
            {
                ComponentId = ReadString(component, "componentId"),
                SourceGoalId = ReadString(component, "sourceGoalId"),
                SourceArtifactRoot = ReadString(component, "sourceArtifactRoot"),
                ManualGate = ReadString(component, "manualGate"),
                ImplementationStatus = ReadString(component, "implementationStatus"),
                Accepted = TryGetBool(component, "accepted"),
                Ready = TryGetBool(component, "ready"),
                SourceHashCount = component.TryGetProperty("sourceArtifactHashes", out var hashes)
                                  && hashes.ValueKind == JsonValueKind.Object
                    ? hashes.EnumerateObject().Count()
                    : 0
            })
            .ToList();
    }

    private static OfflineGeoworldAlphaSliceExportManifest BuildManifest(
        string root,
        OfflineGeoworldAlphaSliceExportSourceLineage lineage)
    {
        using var goal108Manifest = TryReadJson(root, Goal108Root + "/"
            + OfflineGeoworldAlphaSliceVocabulary.ManifestFileName);
        var alphaPath = Resolve(root, OfflineGeoworldAlphaSliceExportPackageVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var gates = lineage.Components.Select(component => component.ManualGate)
            .Where(gate => !string.IsNullOrWhiteSpace(gate))
            .Append(OfflineGeoworldAlphaSliceVocabulary.FinalGate)
            .Append(OfflineGeoworldAlphaSliceExportPackageVocabulary.FinalGate)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(gate => gate, StringComparer.Ordinal)
            .ToList();
        return new OfflineGeoworldAlphaSliceExportManifest
        {
            PackageFileCount = OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredPackageFileNames.Count,
            IndexedFileCount = OfflineGeoworldAlphaSliceExportPackageVocabulary.IndexedPackageFileNames.Count,
            SourceComponentCount = lineage.ComponentCount,
            ReadySourceComponentCount = lineage.ReadyComponentCount,
            ManualGateCount = gates.Count,
            ObjectiveCount = goal108Manifest is null ? 0 : ReadInt(goal108Manifest.RootElement, "objectiveCount"),
            CompletedObjectiveCount = goal108Manifest is null
                ? 0
                : ReadInt(goal108Manifest.RootElement, "completedObjectiveCount"),
            FinalObjectiveStatus = goal108Manifest is null
                ? string.Empty
                : ReadString(goal108Manifest.RootElement, "finalStatus"),
            Goal107FinalAcceptanceHash = goal108Manifest is null
                ? string.Empty
                : ReadString(goal108Manifest.RootElement, "finalAcceptanceHash"),
            Goal108ComponentAggregateHash = goal108Manifest is null
                ? string.Empty
                : ReadString(goal108Manifest.RootElement, "componentAggregateHash"),
            Goal108AcceptedFalse = goal108Manifest is not null && !TryGetBool(goal108Manifest.RootElement, "accepted"),
            Goal108AImmutabilityAuditIncluded =
                lineage.Goal108AImmutabilityAuditRead && lineage.Goal108AHistoricalDiffAuditRead,
            Goal101To107HistoricalArtifactsUnchanged = lineage.Goal101To107ArtifactsUnchanged,
            AlphaRuntimeBootstrapHash = alphaHash,
            AlphaRuntimeBootstrapLineCount = alphaLineCount,
            AlphaRuntimeBootstrapUnchanged =
                alphaHash == OfflineGeoworldAlphaSliceExportPackageVocabulary.AlphaRuntimeBootstrapExpectedHash
                && alphaLineCount == OfflineGeoworldAlphaSliceExportPackageVocabulary.AlphaRuntimeBootstrapExpectedLineCount,
            ManualGates = gates
        };
    }

    private static OfflineGeoworldAlphaSliceExportFileIndex BuildFileIndex()
    {
        var files = OfflineGeoworldAlphaSliceExportPackageVocabulary.IndexedPackageFileNames
            .Select(fileName => new OfflineGeoworldAlphaSliceExportFileIndexEntry
            {
                RelativePath = fileName,
                Role = fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                    ? "review_markdown"
                    : "package_metadata"
            })
            .ToList();
        return new OfflineGeoworldAlphaSliceExportFileIndex
        {
            IndexedFileCount = files.Count,
            Files = files
        };
    }

    private static OfflineGeoworldAlphaSliceExportChecksums BuildChecksums(
        IReadOnlyDictionary<string, string> packageFilesWithoutChecksums)
    {
        var hashes = packageFilesWithoutChecksums
            .ToDictionary(item => item.Key, item => HashText(item.Value), StringComparer.Ordinal);
        return new OfflineGeoworldAlphaSliceExportChecksums
        {
            HashedFileCount = hashes.Count,
            Sha256ByRelativePath = new SortedDictionary<string, string>(hashes, StringComparer.Ordinal)
        };
    }

    private static OfflineGeoworldAlphaSliceExportAcceptanceGate BuildAcceptanceGate(
        OfflineGeoworldAlphaSliceExportManifest manifest) =>
        new()
        {
            PackageReadyForManualReview = manifest.SourceComponentCount == manifest.ReadySourceComponentCount
                                          && manifest.SourceComponentCount == 7
                                          && manifest.Goal108AImmutabilityAuditIncluded
                                          && manifest.AlphaRuntimeBootstrapUnchanged,
            RequiredManualGates = OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredManualGates,
            ManualChecklist =
            [
                "Open the deterministic directory package, not a binary zip.",
                "Verify manifest, file index and checksums.",
                "Run clean-import verifier from the package root.",
                "Open Unity menu LLMGameCreator/Offline Geoworld Alpha Slice Package.",
                "Verify package readiness and runbook summary.",
                "Keep accepted=false until manual review explicitly passes."
            ]
        };

    private static string RenderRunbook(OfflineGeoworldAlphaSliceExportManifest manifest)
    {
        var lines = new List<string>
        {
            "# Offline Geoworld Alpha Export Runbook",
            string.Empty,
            "- goalId: " + manifest.GoalId,
            "- manualGate: " + manifest.ManualGate + " required",
            "- accepted: false",
            "- packageType: deterministic directory package",
            "- notFinalReleaseOrRuntimeBuild: true",
            string.Empty,
            "## Steps",
            string.Empty,
            "1. Review `offline-geoworld-alpha-export-manifest.json`.",
            "2. Review `offline-geoworld-alpha-export-file-index.json`.",
            "3. Verify `offline-geoworld-alpha-export-checksums.json` from the package root.",
            "4. Confirm all manual gates remain listed and accepted=false.",
            "5. Confirm Goal107 final objective acceptance hash is present.",
            "6. Confirm Goal108A source split and immutability audit is present.",
            "7. Open Unity menu `LLMGameCreator/Offline Geoworld Alpha Slice Package`.",
            "8. Use the package verifier without mutating scenes automatically.",
            string.Empty,
            "## Manual Gates",
            string.Empty
        };
        lines.AddRange(manifest.ManualGates.Select(gate => "- " + gate));
        lines.Add(string.Empty);
        lines.Add("## Warnings");
        lines.Add(string.Empty);
        lines.AddRange(manifest.NotFinalWarnings.Select(warning => "- " + warning));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderReadme(OfflineGeoworldAlphaSliceExportManifest manifest)
    {
        var lines = new List<string>
        {
            "# Offline Geoworld Alpha Slice Export Package",
            string.Empty,
            "This directory is a portable Alpha review package over real Goal101-108 evidence.",
            "It is metadata-only and contains no raw geodata, binary media, network/provider calls, final art or final runtime build.",
            string.Empty,
            "- accepted: false",
            "- packageFileCount: " + manifest.PackageFileCount,
            "- indexedFileCount: " + manifest.IndexedFileCount,
            "- sourceComponentCount: " + manifest.SourceComponentCount,
            "- readySourceComponentCount: " + manifest.ReadySourceComponentCount,
            "- finalObjectiveStatus: " + manifest.FinalObjectiveStatus,
            "- goal107FinalAcceptanceHash: " + manifest.Goal107FinalAcceptanceHash,
            "- goal108AImmutabilityAuditIncluded: "
                + manifest.Goal108AImmutabilityAuditIncluded.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: "
                + manifest.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static OfflineGeoworldAlphaSliceExportNegativeProof BuildNegativeProof()
    {
        var scenarios = OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredNegativeScenarioIds
            .Select(id => new OfflineGeoworldAlphaSliceExportNegativeScenario
            {
                ScenarioId = id,
                ActualStatus = "rejected",
                Diagnostic = "Goal109 export verifier rejects " + id + "."
            })
            .ToList();
        return new OfflineGeoworldAlphaSliceExportNegativeProof
        {
            Passed = scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count,
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldAlphaSliceExportUnityScriptInventory BuildUnityScriptInventory(string root)
    {
        var path = Resolve(root, OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityVerifierScriptPath);
        var text = File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var noNetwork = !ContainsAny(text, "UnityWebRequest", "HttpClient", "http://", "https://");
        var noExternal = !ContainsAny(text, "InputSystem", "Packages/", "Newtonsoft", "Addressables");
        return new OfflineGeoworldAlphaSliceExportUnityScriptInventory
        {
            Passed = File.Exists(path)
                     && text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal)
                     && text.Contains("LLMGameCreator/OfflineGeoworldGoal109", StringComparison.Ordinal)
                     && text.Contains("PackageReady", StringComparison.Ordinal)
                     && text.Contains("VerifyPackage", StringComparison.Ordinal)
                     && text.Contains("SHA256", StringComparison.Ordinal)
                     && !text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal)
                     && noNetwork
                     && noExternal,
            VerifierExists = File.Exists(path),
            VerifierRelativePath = OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityVerifierScriptPath,
            ReadsApplicationStreamingAssetsPath = text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal),
            ReadsGoal109Root = text.Contains("LLMGameCreator/OfflineGeoworldGoal109", StringComparison.Ordinal),
            ExposesStatusFields = text.Contains("PackageReady", StringComparison.Ordinal)
                                  && text.Contains("StatusLine", StringComparison.Ordinal),
            VerifyPackageMethodPresent = text.Contains("VerifyPackage", StringComparison.Ordinal),
            ChecksumVerificationMarkerPresent = text.Contains("SHA256", StringComparison.Ordinal),
            DoesNotReferenceAlphaRuntimeBootstrap = !text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal),
            HasNoProviderNetworkMarkers = noNetwork,
            HasNoExternalDependencyMarkers = noExternal,
            LineCount = CountLines(text),
            Sha256 = File.Exists(path) ? HashFile(path) : string.Empty
        };
    }

    private static OfflineGeoworldAlphaSliceExportEditorWindowInventory BuildEditorWindowInventory(string root)
    {
        var path = Resolve(root, OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityEditorWindowScriptPath);
        var text = File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var noNetwork = !ContainsAny(text, "UnityWebRequest", "HttpClient", "http://", "https://");
        var noExternal = !ContainsAny(text, "InputSystem", "Packages/", "Newtonsoft", "Addressables");
        var noAutoSceneMutation = !ContainsAny(text, "InitializeOnLoad", "PostProcessScene", "EditorBuildSettings",
            "SceneManager.SaveScene", "AssetDatabase.CreateAsset", "Undo.RegisterCreatedObjectUndo");
        return new OfflineGeoworldAlphaSliceExportEditorWindowInventory
        {
            Passed = File.Exists(path)
                     && text.Contains("LLMGameCreator/Offline Geoworld Alpha Slice Package", StringComparison.Ordinal)
                     && text.Contains("Verify Package", StringComparison.Ordinal)
                     && text.Contains("runbookSummary", StringComparison.Ordinal)
                     && text.Contains("acceptanceSummary", StringComparison.Ordinal)
                     && noAutoSceneMutation
                     && noNetwork
                     && noExternal,
            EditorWindowExists = File.Exists(path),
            EditorWindowRelativePath = OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityEditorWindowScriptPath,
            MenuItemMarkerPresent = text.Contains("LLMGameCreator/Offline Geoworld Alpha Slice Package", StringComparison.Ordinal),
            ShowsPackageReadiness = text.Contains("PackageReady", StringComparison.Ordinal),
            VerifyButtonPresent = text.Contains("Verify Package", StringComparison.Ordinal),
            ShowsRunbookAndAcceptanceSummary = text.Contains("runbookSummary", StringComparison.Ordinal)
                                               && text.Contains("acceptanceSummary", StringComparison.Ordinal),
            DoesNotMutateScenesAutomatically = noAutoSceneMutation,
            HasNoProviderNetworkMarkers = noNetwork,
            HasNoExternalDependencyMarkers = noExternal,
            LineCount = CountLines(text),
            Sha256 = File.Exists(path) ? HashFile(path) : string.Empty
        };
    }

    private static OfflineGeoworldAlphaSliceExportWorkspaceBindingInventory BuildWorkspaceBindingInventory(
        string root)
    {
        const string pageRelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs";
        const string pageGoal109RelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal109.cs";
        const string serviceRelativeRoot =
            "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace";
        var pageText = ReadOptional(root, pageRelativePath) + "\n" + ReadOptional(root, pageGoal109RelativePath);
        var serviceText = Directory.Exists(Resolve(root, serviceRelativeRoot))
            ? string.Join("\n", Directory.EnumerateFiles(
                    Resolve(root, serviceRelativeRoot),
                    "*.cs",
                    SearchOption.TopDirectoryOnly)
                .Select(path => File.ReadAllText(path, Encoding.UTF8)))
            : string.Empty;
        var groupPresent = serviceText.Contains("offline_geoworld_alpha_export_package", StringComparison.Ordinal);
        var proofPresent = serviceText.Contains("goal109.alpha_export", StringComparison.Ordinal);
        var pageBinding = pageText.Contains("offlineGeoworldAlphaExportPackageFileCount", StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaExportChecksumStatus", StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaExportCleanImportProofPassed", StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaExportUnityVerifierReady", StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaExportAcceptanceGateStatus", StringComparison.Ordinal)
                          && pageText.Contains("offlineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged", StringComparison.Ordinal);
        return new OfflineGeoworldAlphaSliceExportWorkspaceBindingInventory
        {
            Passed = groupPresent && proofPresent && pageBinding,
            WorkspaceGroupPresent = groupPresent,
            ProofStatusPresent = proofPresent,
            PageBindDisplaysExportPackage = pageBinding,
            PageRelativePath = pageRelativePath
        };
    }

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)
                                  ?? throw new InvalidOperationException("Missing directory for " + path));
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
    }

    private static JsonDocument? ReadVerificationJson(string path, List<string> diagnostics)
    {
        if (!File.Exists(path))
        {
            diagnostics.Add("missing-json:" + Path.GetFileName(path));
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add("invalid-json:" + Path.GetFileName(path) + ":" + ex.Message);
            return null;
        }
    }

    private static IReadOnlyList<string> ReadIndexedFileNames(JsonElement? index)
    {
        if (index is null
            || !index.Value.TryGetProperty("files", out var files)
            || files.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return files.EnumerateArray()
            .Select(file => ReadString(file, "relativePath"))
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyDictionary<string, string> ReadChecksumMap(JsonElement? checksums)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        if (checksums is null
            || !checksums.Value.TryGetProperty("sha256ByRelativePath", out var map)
            || map.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var item in map.EnumerateObject())
        {
            if (item.Value.ValueKind == JsonValueKind.String)
            {
                result[item.Name] = item.Value.GetString() ?? string.Empty;
            }
        }

        return result;
    }

    private static bool RequiredManualGatesPresent(JsonElement? manifest)
    {
        if (manifest is null
            || !manifest.Value.TryGetProperty("manualGates", out var gates)
            || gates.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var values = gates.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);
        return OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredManualGates
            .All(values.Contains);
    }
}
