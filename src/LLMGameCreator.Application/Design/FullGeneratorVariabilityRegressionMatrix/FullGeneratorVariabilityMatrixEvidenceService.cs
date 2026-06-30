using System.Text;

namespace LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;

public sealed class FullGeneratorVariabilityMatrixEvidenceService
{
    public const string RelativeOutputDirectory = FullGeneratorVariabilityMatrixVocabulary.RelativeOutputDirectory;
    public const string SourceManifestJsonFileName = "matrix-source-manifest.json";
    public const string SeedProfileMatrixJsonFileName = "seed-profile-matrix.json";
    public const string VarianceMetricsJsonFileName = "variance-metrics.json";
    public const string ReplayProofJsonFileName = "replay-determinism-proof.json";
    public const string ReviewPackageMatrixManifestJsonFileName = "review-package-matrix-manifest.json";
    public const string PreviewExportMatrixPayloadJsonFileName = "preview-export-matrix-payload.json";
    public const string UnityCommandPlanJsonFileName = "unity-alpha-matrix-command-plan.json";
    public const string UnityPlayerProofJsonFileName = "unity-alpha-matrix-player-proof.json";
    public const string InvalidMatrixJsonFileName = "invalid-matrix-diagnostics.json";
    public const string ArtifactScopeReportMarkdownFileName = "artifact-scope-report.md";
    public const string ReportMarkdownFileName = "full-generator-variability-regression-matrix-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly FullGeneratorVariabilityMatrixSourceLoader _sourceLoader;
    private readonly FullGeneratorVariabilityUnityProofRunner _unityProofRunner;

    public FullGeneratorVariabilityMatrixEvidenceService(
        FullGeneratorVariabilityMatrixSourceLoader? sourceLoader = null,
        FullGeneratorVariabilityUnityProofRunner? unityProofRunner = null)
    {
        _sourceLoader = sourceLoader ?? new FullGeneratorVariabilityMatrixSourceLoader();
        _unityProofRunner = unityProofRunner ?? new FullGeneratorVariabilityUnityProofRunner();
    }

    public static string RowFileName(string familyId, string seedId) =>
        "matrix-row-" + FullGeneratorVariabilityMatrixSourceLoader.SafeSegment(familyId) + "-" + FullGeneratorVariabilityMatrixSourceLoader.SafeSegment(seedId) + ".json";

    public FullGeneratorVariabilityEvidenceResult Build(string projectRootPath, FullGeneratorVariabilityMatrixOptions? options = null)
    {
        var proof = new FullGeneratorVariabilityUnityProof
        {
            Passed = false,
            BlockerCode = "goal059.unity.not_executed_yet",
            BlockerMessage = "Unity proof has not been executed in this in-memory build.",
            PlayerProof = new FullGeneratorVariabilityUnityPlayerProof
            {
                Diagnostics =
                [
                    FullGeneratorVariabilityDiagnostic.Warning("goal059.unity.not_executed_yet", "unity-proof", "Unity proof is produced only by BuildAndWriteAsync with ExecuteUnityProof=true.")
                ]
            }
        };
        return BuildCore(projectRootPath, proof);
    }

    public async Task<FullGeneratorVariabilityWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        FullGeneratorVariabilityMatrixOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var settings = options ?? new FullGeneratorVariabilityMatrixOptions();
        var sourceRoot = string.IsNullOrWhiteSpace(settings.RepositoryRootPath)
            ? projectRootPath
            : settings.RepositoryRootPath;
        var initial = BuildCore(sourceRoot, new FullGeneratorVariabilityUnityProof
        {
            Passed = false,
            BlockerCode = settings.ExecuteUnityProof ? "goal059.unity.pending" : "goal059.unity.not_requested",
            BlockerMessage = settings.ExecuteUnityProof
                ? "Unity proof is pending until staging files are written."
                : "Unity proof execution was not requested.",
            PlayerProof = new FullGeneratorVariabilityUnityPlayerProof()
        });
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutputDirectory: true, cancellationToken).ConfigureAwait(false);

        var proof = _unityProofRunner.Run(
            sourceRoot,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            settings);
        var final = BuildCore(sourceRoot, proof);
        return await WriteAsync(projectRootPath, final, resetOutputDirectory: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<FullGeneratorVariabilityWriteResult> WriteAsync(
        string projectRootPath,
        FullGeneratorVariabilityEvidenceResult result,
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
            var path = Path.GetFullPath(Path.Combine(outputDirectory, FullGeneratorVariabilityMatrixVocabulary.StagingRoot, stagingFile.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
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

        var artifactScopePath = Path.Combine(outputDirectory, ArtifactScopeReportMarkdownFileName);
        await File.WriteAllTextAsync(artifactScopePath, result.ArtifactScopeReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(artifactScopePath);

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new FullGeneratorVariabilityWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, FullGeneratorVariabilityMatrixVocabulary.StagingRoot),
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            Result = result
        };
    }

    private FullGeneratorVariabilityEvidenceResult BuildCore(string projectRootPath, FullGeneratorVariabilityUnityProof unityProof)
    {
        var source = _sourceLoader.Load(projectRootPath);
        var builder = new FullGeneratorVariabilityMatrixBuilder();
        var validator = new FullGeneratorVariabilityMatrixValidator();

        var sourceManifest = builder.BuildSourceManifest(source);
        var rows = builder.BuildRows(source);
        var replayRows = builder.BuildRows(source);
        var seedProfileMatrix = builder.BuildSeedProfileMatrix(rows);
        var varianceMetrics = builder.BuildVarianceMetrics(rows);
        var replayProof = builder.BuildReplayProof(rows, replayRows);
        var reviewPackageMatrixManifest = builder.BuildReviewPackageMatrixManifest(rows);
        var previewExportMatrixPayload = builder.BuildPreviewExportMatrixPayload(rows);
        var unityCommandPlan = builder.BuildUnityCommandPlan(rows);
        var invalidMatrix = builder.BuildInvalidMatrix();
        var stagingFiles = builder.BuildStagingFiles(source, unityCommandPlan);

        var stagingDiagnostics = FullGeneratorVariabilityMatrixValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateRows(seedProfileMatrix, rows))
                .Concat(validator.ValidateVarianceAndReplay(varianceMetrics, replayProof))
                .Concat(validator.ValidateReviewAndPayloads(reviewPackageMatrixManifest, previewExportMatrixPayload, unityCommandPlan, invalidMatrix)));
        var proofDiagnostics = validator.ValidateUnityProof(unityCommandPlan, unityProof);
        var diagnostics = FullGeneratorVariabilityMatrixValidator.Sort(stagingDiagnostics.Concat(proofDiagnostics));

        var stagingPassed = stagingDiagnostics.All(item => item.Severity is not "error" and not "critical")
            && sourceManifest.Goal058AcceptedByUserHandoff
            && sourceManifest.Goal058ReportWasGreenProducedForReview
            && sourceManifest.Goal058UnityProofPassed
            && seedProfileMatrix.Passed
            && rows.Count == 9
            && varianceMetrics.Passed
            && replayProof.Passed
            && reviewPackageMatrixManifest.Passed
            && previewExportMatrixPayload.Passed
            && unityCommandPlan.Passed
            && invalidMatrix.Passed;
        var allMatrixMarkersMatched = unityProof.Passed && unityProof.PlayerProof.MissingMarkers.Count == 0;
        var implementationStatus = stagingPassed && allMatrixMarkersMatched
            ? "GREEN"
            : stagingPassed && !allMatrixMarkersMatched
                ? "BLOCKED"
                : "FAILED";

        var artifactJson = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [SourceManifestJsonFileName] = Serialize(sourceManifest),
            [SeedProfileMatrixJsonFileName] = Serialize(seedProfileMatrix),
            [VarianceMetricsJsonFileName] = Serialize(varianceMetrics),
            [ReplayProofJsonFileName] = Serialize(replayProof),
            [ReviewPackageMatrixManifestJsonFileName] = Serialize(reviewPackageMatrixManifest),
            [PreviewExportMatrixPayloadJsonFileName] = Serialize(previewExportMatrixPayload),
            [UnityCommandPlanJsonFileName] = Serialize(unityCommandPlan),
            [UnityPlayerProofJsonFileName] = Serialize(unityProof.PlayerProof),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        foreach (var row in rows.Values.OrderBy(item => item.RowId, StringComparer.Ordinal))
        {
            artifactJson[RowFileName(row.FamilyId, row.SeedId)] = Serialize(row);
        }

        var reportWithoutHash = new FullGeneratorVariabilityMatrixReport
        {
            ImplementationStatus = implementationStatus,
            Accepted = false,
            Goal058AcceptedByUserHandoff = sourceManifest.Goal058AcceptedByUserHandoff,
            SourceFactsConsumed = sourceManifest.SourceArtifactRefs.All(item => item.Exists && item.HashMatches && item.Diagnostics.Count == 0),
            MatrixRowsPassed = seedProfileMatrix.Passed && rows.Count == 9,
            VarianceMetricsPassed = varianceMetrics.Passed,
            ReplayDeterminismPassed = replayProof.Passed,
            ReviewPackageMatrixManifestPassed = reviewPackageMatrixManifest.Passed,
            PreviewExportMatrixPayloadPassed = previewExportMatrixPayload.Passed,
            UnityEditorOrPlayerExecuted = unityProof.UnityEditorOrPlayerExecuted,
            UnityExitCode = unityProof.PlayerProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerProof.PlayerExitCode,
            AllMatrixMarkersMatched = allMatrixMarkersMatched,
            InvalidMatrixPassed = invalidMatrix.Passed,
            MatrixRowCount = rows.Count,
            DistinctDerivedCampaignHashCount = varianceMetrics.DistinctDerivedCampaignHashCount,
            OverfitWarningCount = varianceMetrics.OverfitWarningCount,
            SourceManifestHash = Hash(artifactJson[SourceManifestJsonFileName]),
            SeedProfileMatrixHash = Hash(artifactJson[SeedProfileMatrixJsonFileName]),
            VarianceMetricsHash = Hash(artifactJson[VarianceMetricsJsonFileName]),
            ReplayProofHash = Hash(artifactJson[ReplayProofJsonFileName]),
            UnityPlayerProofHash = Hash(artifactJson[UnityPlayerProofJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new FullGeneratorVariabilityEvidenceResult
        {
            SourceManifest = sourceManifest,
            SeedProfileMatrix = seedProfileMatrix,
            MatrixRowsByRowId = rows,
            VarianceMetrics = varianceMetrics,
            ReplayProof = replayProof,
            ReviewPackageMatrixManifest = reviewPackageMatrixManifest,
            PreviewExportMatrixPayload = previewExportMatrixPayload,
            UnityCommandPlan = unityCommandPlan,
            UnityPlayerProof = unityProof.PlayerProof,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            StagingFiles = stagingFiles,
            ArtifactScopeReportMarkdown = RenderArtifactScopeReport(),
            ReportMarkdown = RenderReport(report, sourceManifest, seedProfileMatrix, rows, varianceMetrics, replayProof, reviewPackageMatrixManifest, previewExportMatrixPayload, unityCommandPlan, unityProof, invalidMatrix)
        };
    }

    private static string RenderArtifactScopeReport()
    {
        var lines = new[]
        {
            "# Goal 059 Artifact Scope Report",
            "",
            "- Scenario: goal-059-full-generator-variability-regression-matrix",
            "- Declared gate: full_generator_variability_regression_matrix_verification required",
            "- Allowed code root: src/LLMGameCreator.Application/Design/FullGeneratorVariabilityRegressionMatrix/",
            "- Allowed tests root: tests/LLMGameCreator.Tests/Application/FullGeneratorVariabilityRegressionMatrix/",
            "- Allowed product smoke: tests/LLMGameCreator.Tests/ProductSmoke/FullGeneratorVariabilityRegressionMatrixProductSmokeTests.cs",
            "- Allowed artifact root: .llmgc/procedural/goal-059-full-generator-variability-regression-matrix/",
            "- Narrow Unity allowance: unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs",
            "- Forbidden provider/network/LLM/RAG/media generation/runtime/schema/UI/generator-library changes: enforced by task scope and final artifact guard"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderReport(
        FullGeneratorVariabilityMatrixReport report,
        FullGeneratorVariabilitySourceManifest sourceManifest,
        FullGeneratorVariabilitySeedProfileMatrix matrix,
        IReadOnlyDictionary<string, FullGeneratorVariabilityMatrixRow> rows,
        FullGeneratorVariabilityVarianceMetrics variance,
        FullGeneratorVariabilityReplayDeterminismProof replay,
        FullGeneratorVariabilityReviewPackageMatrixManifest review,
        FullGeneratorVariabilityPreviewExportMatrixPayload previewExport,
        FullGeneratorVariabilityUnityMatrixCommandPlan commandPlan,
        FullGeneratorVariabilityUnityProof unityProof,
        InvalidFullGeneratorVariabilityMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Full Generator Variability Regression Matrix Report",
            string.Empty,
            "full_generator_variability_regression_matrix_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            "manualGate=full_generator_variability_regression_matrix_verification",
            $"goal058AcceptedByUserHandoff={report.Goal058AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"sourceFactsConsumed={report.SourceFactsConsumed.ToString().ToLowerInvariant()}",
            $"matrixRowsPassed={report.MatrixRowsPassed.ToString().ToLowerInvariant()}",
            $"varianceMetricsPassed={report.VarianceMetricsPassed.ToString().ToLowerInvariant()}",
            $"replayDeterminismPassed={report.ReplayDeterminismPassed.ToString().ToLowerInvariant()}",
            $"reviewPackageMatrixManifestPassed={report.ReviewPackageMatrixManifestPassed.ToString().ToLowerInvariant()}",
            $"previewExportMatrixPayloadPassed={report.PreviewExportMatrixPayloadPassed.ToString().ToLowerInvariant()}",
            $"unityEditorOrPlayerExecuted={report.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allMatrixMarkersMatched={report.AllMatrixMarkersMatched.ToString().ToLowerInvariant()}",
            $"invalidMatrixPassed={report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"matrixRowCount={report.MatrixRowCount}",
            $"distinctDerivedCampaignHashCount={report.DistinctDerivedCampaignHashCount}",
            $"overfitWarningCount={report.OverfitWarningCount}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"seedProfileMatrixHash={report.SeedProfileMatrixHash}",
            $"varianceMetricsHash={report.VarianceMetricsHash}",
            $"replayProofHash={report.ReplayProofHash}",
            $"unityPlayerProofHash={report.UnityPlayerProofHash}",
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
        lines.Add($"- goal058ReportWasGreenProducedForReview: {sourceManifest.Goal058ReportWasGreenProducedForReview.ToString().ToLowerInvariant()}");
        lines.Add($"- goal058UnityProofPassed: {sourceManifest.Goal058UnityProofPassed.ToString().ToLowerInvariant()}");
        lines.Add($"- sourceCampaignHash: {sourceManifest.SourceCampaignHash}");
        lines.Add($"- sourceArtifactCount: {sourceManifest.SourceArtifactCount}");
        lines.AddRange(sourceManifest.SourceArtifactRefs.Select(item => $"- {item.ArtifactFamily}: artifact={item.ArtifactRelativePath}, exists={item.Exists.ToString().ToLowerInvariant()}, hashMatches={item.HashMatches.ToString().ToLowerInvariant()}, hash={item.ArtifactHash}"));
        lines.Add(string.Empty);
        lines.Add("## Matrix Rows");
        lines.Add(string.Empty);
        lines.Add($"- passed: {matrix.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- rowCount: {matrix.RowCount}");
        foreach (var row in rows.Values.OrderBy(item => FullGeneratorVariabilityMatrixBuilder.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal).ThenBy(item => FullGeneratorVariabilityMatrixBuilder.SeedOrderingKey(item.SeedId), StringComparer.Ordinal))
        {
            lines.Add($"- {row.RowId}: family={row.FamilyId}, seed={row.SeedId}, hash={row.DerivedCampaignHash}, mediaRefs={row.SelectedMediaRefs.Count}, loopRefs={row.SelectedFamilyLoopRefs.Count}, variationDimensions={row.VariationDimensions.Count}");
        }

        lines.Add(string.Empty);
        lines.Add("## Variance Metrics");
        lines.Add(string.Empty);
        lines.Add($"- passed: {variance.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- distinctRowIds: {variance.DistinctRowIdCount}");
        lines.Add($"- distinctDerivedCampaignHashes: {variance.DistinctDerivedCampaignHashCount}");
        lines.Add($"- mediaBindingCoverage: {variance.MediaBindingCoveragePassed.ToString().ToLowerInvariant()} ({variance.MediaBindingCoverageCount})");
        lines.Add($"- familyLoopMarkerCoverage: {variance.FamilyLoopMarkerCoveragePassed.ToString().ToLowerInvariant()} ({variance.FamilyLoopMarkerCoverageCount})");
        lines.Add($"- minimumMeaningfulVariationDimensionsPerFamily: {variance.MinimumMeaningfulVariationDimensionsPerFamily}");
        lines.Add($"- overfitWarningCount: {variance.OverfitWarningCount}");
        lines.AddRange(variance.FamilySummaries.Select(item => $"- family={item.FamilyId}: rows={item.RowCount}, seeds={item.DistinctSeedCount}, hashes={item.DistinctDerivedHashCount}, dimensions={item.MeaningfulVariationDimensionCount}:{string.Join(",", item.MeaningfulVariationDimensions)}"));
        lines.AddRange(variance.PairDifferenceSummaries.Select(item => $"- pair={item.LeftRowId}->{item.RightRowId}: differences={item.DifferenceDimensionCount}:{string.Join(",", item.DifferenceDimensions)}"));
        lines.Add(string.Empty);
        lines.Add("## Replay Determinism");
        lines.Add(string.Empty);
        lines.Add($"- passed: {replay.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- matchedRows: {replay.MatchedRowCount}/{replay.RowCount}");
        lines.AddRange(replay.Rows.Select(item => $"- {item.RowId}: jsonMatches={item.JsonMatches.ToString().ToLowerInvariant()}, hashMatches={item.HashMatches.ToString().ToLowerInvariant()}, hash={item.FirstHash}"));
        lines.Add(string.Empty);
        lines.Add("## Review And Preview/Export");
        lines.Add(string.Empty);
        lines.Add($"- reviewPackageMatrixManifestPassed: {review.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- sourceReviewPackageManifestRef: {review.SourceReviewPackageManifestRef}");
        lines.Add($"- previewExportMatrixPayloadPassed: {previewExport.Passed.ToString().ToLowerInvariant()}");
        lines.AddRange(previewExport.Rows.Select(item => $"- previewExportRow={item.RowId}: preview={item.PreviewPayloadRef}, exportMode={item.ExportMode}, hash={item.DerivedCampaignHash}"));
        lines.Add(string.Empty);
        lines.Add("## Unity Proof");
        lines.Add(string.Empty);
        lines.Add($"- passed: {unityProof.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- unityEditorOrPlayerExecuted: {unityProof.UnityEditorOrPlayerExecuted.ToString().ToLowerInvariant()}");
        lines.Add($"- unityExitCode: {TextOrNone(unityProof.PlayerProof.UnityExitCode?.ToString())}");
        lines.Add($"- playerExitCode: {TextOrNone(unityProof.PlayerProof.PlayerExitCode?.ToString())}");
        lines.Add($"- blockerCode: {TextOrNone(unityProof.BlockerCode)}");
        lines.Add($"- blockerMessage: {TextOrNone(unityProof.BlockerMessage)}");
        lines.Add($"- launchLog: {unityProof.PlayerProof.LaunchLogRelativePath}");
        lines.Add($"- playLoopLog: {unityProof.PlayerProof.PlayLoopLogRelativePath}");
        lines.Add($"- expectedMarkerCount: {commandPlan.ExpectedPlayerMarkers.Count}");
        lines.AddRange(commandPlan.ExpectedPlayerMarkers.Select(marker => $"- requiredMarker: {marker}"));
        lines.AddRange(unityProof.PlayerProof.MatchedMarkers.Select(marker => $"- matchedMarker: {marker}"));
        lines.AddRange(unityProof.PlayerProof.MissingMarkers.Select(marker => $"- missingMarker: {marker}"));
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
        lines.Add("No provider/media generation, network/import/download, LLM/RAG call, arbitrary Lua execution, public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution or project file change is part of this Goal 059 proof. Unity changes are limited to deterministic matrix marker support in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("full_generator_variability_regression_matrix_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => FullGeneratorVariabilityMatrixHash.Serialize(value);

    private static string Hash(string text) => FullGeneratorVariabilityMatrixHash.Hash(text);

    private static string TextOrNone(string? value) =>
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
                    Thread.Sleep(1000);
                }
            }
        }

        return false;
    }

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
