using System.Text;

namespace LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;

public sealed class UnityAlphaMediaBoundPlayablePackageEvidenceService
{
    public const string RelativeOutputDirectory = UnityAlphaMediaBoundPlayablePackageVocabulary.RelativeOutputDirectory;
    public const string SourceManifestJsonFileName = "source-evidence-manifest.json";
    public const string StagingManifestJsonFileName = "unity-streamingassets-staging-manifest.json";
    public const string FamilyPanelModelsJsonFileName = "media-bound-family-panel-models.json";
    public const string UnityLoadContractJsonFileName = "unity-media-load-contract.json";
    public const string UnityLoadProofJsonFileName = "unity-media-load-proof.json";
    public const string SmokeLogSummaryJsonFileName = "unity-alpha-media-bound-smoke-log-summary.json";
    public const string PreviewExportPayloadsJsonFileName = "preview-export-media-bound-payloads.json";
    public const string HashInventoryJsonFileName = "staged-file-hash-inventory.json";
    public const string InvalidMatrixJsonFileName = "invalid-unity-media-bound-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "unity-alpha-media-bound-playable-package-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly UnityAlphaMediaBoundPlayablePackageSourceLoader _sourceLoader;
    private readonly UnityAlphaMediaBoundUnityProofRunner _unityProofRunner;

    public UnityAlphaMediaBoundPlayablePackageEvidenceService(
        UnityAlphaMediaBoundPlayablePackageSourceLoader? sourceLoader = null,
        UnityAlphaMediaBoundUnityProofRunner? unityProofRunner = null)
    {
        _sourceLoader = sourceLoader ?? new UnityAlphaMediaBoundPlayablePackageSourceLoader();
        _unityProofRunner = unityProofRunner ?? new UnityAlphaMediaBoundUnityProofRunner();
    }

    public UnityAlphaMediaBoundEvidenceResult Build(
        string projectRootPath,
        UnityAlphaMediaBoundOptions? options = null)
    {
        var proof = new UnityAlphaMediaBoundLoadProof
        {
            Passed = false,
            BlockerCode = "goal056.unity.not_executed_yet",
            BlockerMessage = "Unity proof has not been executed in this in-memory build.",
            SmokeLogSummary = new UnityAlphaMediaBoundSmokeLogSummary
            {
                Diagnostics =
                [
                    UnityAlphaMediaBoundDiagnostic.Warning("goal056.unity.not_executed_yet", "unity-proof", "Unity proof is produced only by BuildAndWriteAsync with ExecuteUnityProof=true.")
                ]
            }
        };
        return BuildCore(projectRootPath, proof);
    }

    public async Task<UnityAlphaMediaBoundWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        UnityAlphaMediaBoundOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var settings = options ?? new UnityAlphaMediaBoundOptions();
        var sourceRoot = string.IsNullOrWhiteSpace(settings.RepositoryRootPath)
            ? projectRootPath
            : settings.RepositoryRootPath;
        var initial = BuildCore(sourceRoot, new UnityAlphaMediaBoundLoadProof
        {
            Passed = false,
            BlockerCode = settings.ExecuteUnityProof ? "goal056.unity.pending" : "goal056.unity.not_requested",
            BlockerMessage = settings.ExecuteUnityProof
                ? "Unity proof is pending until staging files are written."
                : "Unity proof execution was not requested.",
            SmokeLogSummary = new UnityAlphaMediaBoundSmokeLogSummary()
        });
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutputDirectory: true, cancellationToken).ConfigureAwait(false);

        var proof = _unityProofRunner.Run(
            projectRootPath,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityLoadContract,
            settings);
        var final = BuildCore(sourceRoot, proof);
        return await WriteAsync(projectRootPath, final, resetOutputDirectory: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<UnityAlphaMediaBoundWriteResult> WriteAsync(
        string projectRootPath,
        UnityAlphaMediaBoundEvidenceResult result,
        bool resetOutputDirectory = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        if (resetOutputDirectory)
        {
            ResetDirectory(outputDirectory);
        }
        else
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var written = new List<string>();
        foreach (var stagingFile in result.StagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.GetFullPath(Path.Combine(outputDirectory, UnityAlphaMediaBoundPlayablePackageVocabulary.StagingRoot, stagingFile.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, stagingFile.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        foreach (var pair in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, pair.Key);
            await File.WriteAllTextAsync(path, pair.Value + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new UnityAlphaMediaBoundWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, UnityAlphaMediaBoundPlayablePackageVocabulary.StagingRoot),
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            Result = result
        };
    }

    private UnityAlphaMediaBoundEvidenceResult BuildCore(string projectRootPath, UnityAlphaMediaBoundLoadProof unityProof)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var builder = new UnityAlphaMediaBoundPlayablePackageBuilder();
        var validator = new UnityAlphaMediaBoundPlayablePackageValidator();

        var sourceManifest = builder.BuildSourceManifest(source);
        var staging = builder.BuildStagingFiles(source);
        var stagingManifest = builder.BuildStagingManifest(source, staging.Bindings);
        var panelModels = builder.BuildFamilyPanelModels(staging.Bindings);
        var unityLoadContract = builder.BuildUnityLoadContract(staging.Bindings);
        var previewExportPayloads = builder.BuildPreviewExportPayloads(panelModels);
        var hashInventory = builder.BuildHashInventory(staging.Bindings);
        var invalidMatrix = builder.BuildInvalidMatrix();
        var artifactScopeReport = builder.BuildArtifactScopeReport();

        var stagingDiagnostics = UnityAlphaMediaBoundPlayablePackageValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateStagingManifest(stagingManifest))
                .Concat(validator.ValidatePanelModels(panelModels))
                .Concat(validator.ValidateInvalidMatrix(invalidMatrix)));
        var proofDiagnostics = validator.ValidateUnityContractAndProof(unityLoadContract, unityProof);
        var diagnostics = UnityAlphaMediaBoundPlayablePackageValidator.Sort(stagingDiagnostics.Concat(proofDiagnostics));

        var stagingPassed = stagingDiagnostics.All(item => item.Severity is not "error" and not "critical")
            && sourceManifest.Goal055AcceptedByUserHandoff
            && sourceManifest.Goal055ReportWasGreenProducedForReview
            && stagingManifest.Passed
            && panelModels.Passed
            && unityLoadContract.Passed
            && previewExportPayloads.Passed
            && hashInventory.Passed
            && invalidMatrix.Passed
            && artifactScopeReport.Passed;
        var implementationStatus = stagingPassed && unityProof.Passed
            ? "GREEN"
            : stagingPassed && !unityProof.Passed
                ? "BLOCKED"
                : "FAILED";

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(sourceManifest),
            [StagingManifestJsonFileName] = Serialize(stagingManifest),
            [FamilyPanelModelsJsonFileName] = Serialize(panelModels),
            [UnityLoadContractJsonFileName] = Serialize(unityLoadContract),
            [UnityLoadProofJsonFileName] = Serialize(unityProof),
            [SmokeLogSummaryJsonFileName] = Serialize(unityProof.SmokeLogSummary),
            [PreviewExportPayloadsJsonFileName] = Serialize(previewExportPayloads),
            [HashInventoryJsonFileName] = Serialize(hashInventory),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix),
            [ArtifactScopeReportJsonFileName] = Serialize(artifactScopeReport)
        };

        var reportWithoutHash = new UnityAlphaMediaBoundPlayablePackageReport
        {
            ImplementationStatus = implementationStatus,
            Accepted = false,
            Goal055AcceptedByUserHandoff = sourceManifest.Goal055AcceptedByUserHandoff,
            StreamingAssetsPayloadStaged = stagingManifest.Passed,
            PhysicalMediaFileCount = stagingManifest.PhysicalMediaFileCount,
            PngLoadProofPassed = unityProof.PngLoadProofPassed,
            WavLoadProofPassed = unityProof.WavLoadProofPassed,
            BundleProofPassed = unityProof.BundleProofPassed,
            UnityEditorOrPlayerExecuted = unityProof.UnityEditorOrPlayerExecuted,
            UnityMediaLoadContractPassed = unityLoadContract.Passed && unityProof.Passed,
            FamilyMediaPanelProofPassed = unityProof.FamilyMediaPanelProofPassed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            UnitySourceChanged = true,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            StagingManifestHash = Hash(artifactJson[StagingManifestJsonFileName]),
            FamilyPanelModelsHash = Hash(artifactJson[FamilyPanelModelsJsonFileName]),
            UnityLoadContractHash = Hash(artifactJson[UnityLoadContractJsonFileName]),
            UnityLoadProofHash = Hash(artifactJson[UnityLoadProofJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new UnityAlphaMediaBoundEvidenceResult
        {
            SourceManifest = sourceManifest,
            StagingManifest = stagingManifest,
            FamilyPanelModels = panelModels,
            UnityLoadContract = unityLoadContract,
            UnityLoadProof = unityProof,
            SmokeLogSummary = unityProof.SmokeLogSummary,
            PreviewExportPayloads = previewExportPayloads,
            HashInventory = hashInventory,
            InvalidMatrix = invalidMatrix,
            ArtifactScopeReport = artifactScopeReport,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            StagingFiles = staging.StagingFiles,
            ReportMarkdown = RenderReport(report, sourceManifest, stagingManifest, panelModels, unityLoadContract, unityProof, invalidMatrix)
        };
    }

    private static string RenderReport(
        UnityAlphaMediaBoundPlayablePackageReport report,
        UnityAlphaMediaBoundSourceManifest sourceManifest,
        UnityAlphaMediaBoundStagingManifest stagingManifest,
        UnityAlphaMediaBoundFamilyPanelModels panelModels,
        UnityAlphaMediaBoundLoadContract unityLoadContract,
        UnityAlphaMediaBoundLoadProof unityLoadProof,
        InvalidUnityAlphaMediaBoundMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Unity Alpha Media-Bound Playable Package Report",
            string.Empty,
            "unity_alpha_media_bound_playable_package_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            "manualGate=unity_alpha_media_bound_playable_package_verification",
            $"goal055AcceptedByUserHandoff={report.Goal055AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"streamingAssetsPayloadStaged={report.StreamingAssetsPayloadStaged.ToString().ToLowerInvariant()}",
            $"physicalMediaFileCount={report.PhysicalMediaFileCount}",
            $"pngLoadProofPassed={report.PngLoadProofPassed.ToString().ToLowerInvariant()}",
            $"wavLoadProofPassed={report.WavLoadProofPassed.ToString().ToLowerInvariant()}",
            $"bundleProofPassed={report.BundleProofPassed.ToString().ToLowerInvariant()}",
            $"unityEditorOrPlayerExecuted={report.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}",
            $"unityMediaLoadContractPassed={report.UnityMediaLoadContractPassed.ToString().ToLowerInvariant()}",
            $"familyMediaPanelProofPassed={report.FamilyMediaPanelProofPassed.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"unitySourceChanged={report.UnitySourceChanged.ToString().ToLowerInvariant()}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"stagingManifestHash={report.StagingManifestHash}",
            $"familyPanelModelsHash={report.FamilyPanelModelsHash}",
            $"unityLoadContractHash={report.UnityLoadContractHash}",
            $"unityLoadProofHash={report.UnityLoadProofHash}",
            $"invalidMatrixHash={report.InvalidMatrixHash}",
            $"reportHash={report.DeterministicHash}",
            string.Empty,
            "## Preflight",
            string.Empty
        };
        lines.AddRange(sourceManifest.PreflightGates.Select(item => $"- {item.GateId}: status={item.Status}, provenance={item.ProvenanceKind}, evidence={item.EvidenceRef}"));
        lines.Add(string.Empty);
        lines.Add("## Source Facts");
        lines.Add(string.Empty);
        lines.Add($"- goal055AcceptedByUserHandoff: {sourceManifest.Goal055AcceptedByUserHandoff.ToString().ToLowerInvariant()}");
        lines.Add($"- goal055ReportWasGreenProducedForReview: {sourceManifest.Goal055ReportWasGreenProducedForReview.ToString().ToLowerInvariant()}");
        lines.Add($"- goal055PhysicalMediaFileCount: {sourceManifest.Goal055PhysicalMediaFileCount}");
        lines.Add($"- baseAlphaPayloadSourceRoot: {sourceManifest.BaseAlphaPayloadSourceRoot}");
        lines.AddRange(sourceManifest.SourceArtifactRefs.Select(item => $"- {item.SourceGoal}: artifact={item.ArtifactRelativePath}, exists={item.Exists.ToString().ToLowerInvariant()}, hashMatches={item.HashMatches.ToString().ToLowerInvariant()}"));
        lines.Add(string.Empty);
        lines.Add("## StreamingAssets Payload");
        lines.Add(string.Empty);
        lines.Add($"- passed: {stagingManifest.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- manifest: {stagingManifest.ManifestRelativePath}");
        lines.Add($"- basePayloadFileCount: {stagingManifest.BasePayloadFileCount}");
        lines.Add($"- physicalMediaFileCount: {stagingManifest.PhysicalMediaFileCount}");
        lines.AddRange(stagingManifest.Bindings.Select(item => $"- {item.FamilyId}/{item.SlotId}: kind={item.MediaKind}, path={item.RelativePath}, sha256={item.Sha256}, bytes={item.SizeBytes}, reviewTrace={item.ReviewTrace}"));
        lines.Add(string.Empty);
        lines.Add("## Family Panels");
        lines.Add(string.Empty);
        lines.Add($"- passed: {panelModels.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(panelModels.Families.Select(item => $"- {item.FamilyId}: marker={item.PanelProofMarker}, bindings={item.BindingIds.Count}"));
        lines.Add(string.Empty);
        lines.Add("## Unity Proof");
        lines.Add(string.Empty);
        lines.Add($"- passed: {unityLoadProof.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- unityEditorOrPlayerExecuted: {unityLoadProof.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- blockerCode: {TextOrNone(unityLoadProof.BlockerCode)}");
        lines.Add($"- blockerMessage: {TextOrNone(unityLoadProof.BlockerMessage)}");
        lines.Add($"- launchLog: {unityLoadProof.SmokeLogSummary.LaunchLogRelativePath}");
        lines.Add($"- playLoopLog: {unityLoadProof.SmokeLogSummary.PlayLoopLogRelativePath}");
        lines.AddRange(unityLoadContract.RequiredLogMarkers.Select(marker => $"- requiredMarker: {marker}"));
        lines.AddRange(unityLoadProof.SmokeLogSummary.MatchedMarkers.Select(marker => $"- matchedMarker: {marker}"));
        lines.AddRange(unityLoadProof.SmokeLogSummary.MissingMarkers.Select(marker => $"- missingMarker: {marker}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak Matrix");
        lines.Add(string.Empty);
        lines.Add($"- passed: {invalidMatrix.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- scenarioCount: {invalidMatrix.ScenarioCount}");
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No provider/media generation, no network/import/download, no LLM/RAG call, no Lua execution, no public GamePackage schema, Runtime, Runtime.Abstractions, WinForms UI, provider path, generator-library, solution or project file change is part of this Goal 056 proof. Unity changes are limited to the repo-local Alpha media manifest loader, diagnostics and compact presentation panel.");
        lines.Add(string.Empty);
        lines.Add("unity_alpha_media_bound_playable_package_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => UnityAlphaMediaBoundPlayablePackageHash.Serialize(value);

    private static string Hash(string text) => UnityAlphaMediaBoundPlayablePackageHash.Hash(text);

    private static string TextOrNone(string value) =>
        string.IsNullOrWhiteSpace(value) ? "(none)" : value;

    private static void ResetDirectory(string path)
    {
        if (!TryResetDirectory(path, maxAttempts: 120, out var exception))
        {
            throw new IOException($"Directory could not be reset: {path}", exception);
        }
    }

    private static bool TryResetDirectory(string path, int maxAttempts, out Exception? lastException)
    {
        lastException = null;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }

                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                lastException = exception;
                if (attempt < maxAttempts)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        return false;
    }

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
