using System.Text;

namespace LLMGameCreator.Application.Design.IntegratedCampaignTimelineSimulationMatrix;

public sealed class IntegratedCampaignTimelineEvidenceService
{
    public const string SourceManifestJsonFileName = "source-manifest.json";
    public const string MatrixSummaryJsonFileName = "timeline-matrix-summary.json";
    public const string CascadeLedgerJsonFileName = "cross-system-cascade-ledger.json";
    public const string ArbitrationLedgerJsonFileName = "conflict-arbitration-ledger.json";
    public const string SaveLoadReplayAuditJsonFileName = "save-load-replay-audit.json";
    public const string VarianceMetricsJsonFileName = "variance-metrics.json";
    public const string UnityCommandPlanJsonFileName = "unity-command-plan.json";
    public const string UnityPlayerProofSummaryJsonFileName = "unity-player-proof-summary.json";
    public const string PreviewExportTimelinePayloadJsonFileName = "preview-export-timeline-payload.json";
    public const string InvalidDiagnosticsMatrixJsonFileName = "invalid-diagnostics-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "integrated-campaign-timeline-simulation-matrix-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public TimelineBuildResult Build(string projectRootPath, TimelineUnityProof? unityProof = null)
    {
        var source = new IntegratedCampaignTimelineSourceLoader().Load(projectRootPath);
        var projector = new IntegratedCampaignTimelineProjector();
        var sourceManifest = projector.BuildSourceManifest(source);
        var rows = projector.BuildRows(source);
        var matrix = projector.BuildMatrixSummary(rows);
        var cascades = projector.BuildCascadeLedger(rows);
        var arbitrations = projector.BuildArbitrationLedger(rows);
        var replay = projector.BuildSaveLoadReplayAudit(rows);
        var variance = projector.BuildVarianceMetrics(rows);
        var preview = projector.BuildPreviewExportPayload(rows);
        var unityCommandPlan = projector.BuildUnityCommandPlan(rows);
        var proof = unityProof ?? IntegratedCampaignTimelineUnityProofRunner.NotRequested(unityCommandPlan);
        var invalid = projector.BuildInvalidMatrix();
        var diagnostics = BuildDiagnostics(sourceManifest, matrix, cascades, arbitrations, replay, variance, preview, unityCommandPlan, proof.PlayerProof, invalid);
        var reportWithoutHash = BuildReport(sourceManifest, matrix, cascades, arbitrations, replay, variance, preview, unityCommandPlan, proof.PlayerProof, invalid, diagnostics);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new TimelineBuildResult
        {
            SourceManifest = sourceManifest,
            MatrixSummary = matrix,
            CascadeLedger = cascades,
            ArbitrationLedger = arbitrations,
            SaveLoadReplayAudit = replay,
            VarianceMetrics = variance,
            UnityCommandPlan = unityCommandPlan,
            UnityProofSummary = proof.PlayerProof,
            PreviewExportPayload = preview,
            InvalidMatrix = invalid,
            Report = report,
            Rows = rows,
            StagingFiles = projector.BuildStagingFiles(source, unityCommandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, matrix, cascades, arbitrations, replay, variance, proof.PlayerProof, invalid)
        };
    }

    public async Task<TimelineWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        IntegratedCampaignTimelineOptions options,
        CancellationToken cancellationToken = default)
    {
        var initial = Build(projectRootPath);
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutput: true, cancellationToken).ConfigureAwait(false);
        if (!options.ExecuteUnityProof)
        {
            return initialWrite;
        }

        var proof = new IntegratedCampaignTimelineUnityProofRunner().Run(
            projectRootPath,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            options);
        var final = Build(projectRootPath, proof);
        return await WriteAsync(projectRootPath, final, resetOutput: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TimelineWriteResult> WriteAsync(
        string projectRootPath,
        TimelineBuildResult result,
        bool resetOutput = true,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, IntegratedCampaignTimelineVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        if (resetOutput)
        {
            ResetDirectory(outputDirectory);
        }
        else
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var written = new List<string>();
        await WriteText(outputDirectory, SourceManifestJsonFileName, Serialize(result.SourceManifest), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, MatrixSummaryJsonFileName, Serialize(result.MatrixSummary), written, cancellationToken).ConfigureAwait(false);
        foreach (var row in result.Rows.OrderBy(item => item.RowId, StringComparer.Ordinal))
        {
            await WriteText(outputDirectory, RowFileName(row), Serialize(row), written, cancellationToken).ConfigureAwait(false);
        }

        await WriteText(outputDirectory, CascadeLedgerJsonFileName, Serialize(result.CascadeLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ArbitrationLedgerJsonFileName, Serialize(result.ArbitrationLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SaveLoadReplayAuditJsonFileName, Serialize(result.SaveLoadReplayAudit), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, VarianceMetricsJsonFileName, Serialize(result.VarianceMetrics), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityCommandPlanJsonFileName, Serialize(result.UnityCommandPlan), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityPlayerProofSummaryJsonFileName, Serialize(result.UnityProofSummary), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, PreviewExportTimelinePayloadJsonFileName, Serialize(result.PreviewExportPayload), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, InvalidDiagnosticsMatrixJsonFileName, Serialize(result.InvalidMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ArtifactScopeReportJsonFileName, RenderArtifactScopeReportJson(), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ReportMarkdownFileName, result.ReportMarkdown, written, cancellationToken).ConfigureAwait(false);

        foreach (var file in result.StagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, IntegratedCampaignTimelineVocabulary.StagingRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await WriteBytes(path, file.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        return new TimelineWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, IntegratedCampaignTimelineVocabulary.StagingRoot),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            WrittenFiles = written.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    public static string RowFileName(CampaignTimelineRow row) =>
        "campaign-timeline-row-" + row.FamilyId + "-" + row.SeedId + ".json";

    private static IReadOnlyList<TimelineDiagnostic> BuildDiagnostics(
        TimelineSourceManifest sourceManifest,
        TimelineMatrixSummary matrix,
        CrossSystemCascadeLedger cascades,
        ConflictArbitrationLedger arbitrations,
        SaveLoadReplayAudit replay,
        TimelineVarianceMetrics variance,
        PreviewExportTimelinePayload preview,
        TimelineUnityCommandPlan unityCommandPlan,
        TimelineUnityProofSummary unityProof,
        TimelineInvalidDiagnosticsMatrix invalid)
    {
        var validator = new IntegratedCampaignTimelineValidator();
        return IntegratedCampaignTimelineValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateRows(matrix, preview))
                .Concat(validator.ValidateCascadeAndArbitration(cascades, arbitrations))
                .Concat(validator.ValidateReplay(replay))
                .Concat(validator.ValidateVariance(variance))
                .Concat(validator.ValidateUnityCommandPlan(unityCommandPlan))
                .Concat(validator.ValidateUnityProof(unityCommandPlan, unityProof))
                .Concat(validator.ValidateInvalidMatrix(invalid)));
    }

    private static TimelineReport BuildReport(
        TimelineSourceManifest sourceManifest,
        TimelineMatrixSummary matrix,
        CrossSystemCascadeLedger cascades,
        ConflictArbitrationLedger arbitrations,
        SaveLoadReplayAudit replay,
        TimelineVarianceMetrics variance,
        PreviewExportTimelinePayload preview,
        TimelineUnityCommandPlan unityCommandPlan,
        TimelineUnityProofSummary unityProof,
        TimelineInvalidDiagnosticsMatrix invalid,
        IReadOnlyList<TimelineDiagnostic> diagnostics)
    {
        var noErrors = diagnostics.All(item => item.Severity != "error");
        var sourceConsumed = sourceManifest.Goal060PackageRowsConsumed
            && sourceManifest.Goal061ReviewPackageRcConsumed
            && sourceManifest.Goal062SpatialRowsConsumed
            && sourceManifest.Goal063GameplayRowsConsumed
            && sourceManifest.Goal064LivingWorldRowsConsumed
            && sourceManifest.Goal065InterlockedRowsConsumed
            && sourceManifest.Goal066SettlementRowsConsumed
            && sourceManifest.Goal067NarrativeRowsConsumed
            && sourceManifest.Goal068CombatMagicRowsConsumed
            && sourceManifest.Goal069WorldEventRowsConsumed;
        var green = noErrors
            && sourceManifest.Goal069AcceptedByUserHandoff
            && sourceConsumed
            && matrix.Passed
            && cascades.Passed
            && arbitrations.Passed
            && replay.Passed
            && variance.Passed
            && preview.Passed
            && unityCommandPlan.Passed
            && unityProof.Passed
            && invalid.Passed;
        var failed = diagnostics.Any(item => item.Severity == "error" && !item.Code.StartsWith("goal070.unity.", StringComparison.Ordinal));

        return new TimelineReport
        {
            ImplementationStatus = green ? "GREEN" : failed ? "FAILED" : "BLOCKED",
            Accepted = false,
            Goal069AcceptedByUserHandoff = sourceManifest.Goal069AcceptedByUserHandoff,
            SourceFactsConsumed = sourceConsumed,
            RowMatrixPassed = matrix.Passed,
            CascadeLedgerPassed = cascades.Passed,
            ArbitrationLedgerPassed = arbitrations.Passed,
            SaveLoadReplayPassed = replay.Passed,
            MeaningfulVariancePassed = variance.Passed,
            UnityCommandPlanPassed = unityCommandPlan.Passed,
            UnityProofPassed = unityProof.Passed,
            UnityExitCode = unityProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerExitCode,
            AllTimelineMarkersMatched = unityProof.Passed && unityProof.MissingMarkers.Count == 0,
            PreviewExportPayloadPassed = preview.Passed,
            InvalidMatrixPassed = invalid.Passed,
            RowCount = matrix.RowCount,
            StateChangingRowCount = matrix.StateChangingRowCount,
            CascadeCount = cascades.CascadeCount,
            ArbitrationCount = arbitrations.ArbitrationCount,
            SaveLoadPassedRowCount = replay.SaveLoadPassedRowCount,
            ReplayPassedRowCount = replay.ReplayPassedRowCount,
            FamilyCount = matrix.FamilyCount,
            SeedCount = matrix.SeedCount,
            SourceManifestHash = Hash(Serialize(sourceManifest)),
            MatrixSummaryHash = Hash(Serialize(matrix)),
            CascadeLedgerHash = Hash(Serialize(cascades)),
            ArbitrationLedgerHash = Hash(Serialize(arbitrations)),
            SaveLoadReplayAuditHash = Hash(Serialize(replay)),
            VarianceMetricsHash = Hash(Serialize(variance)),
            UnityCommandPlanHash = Hash(Serialize(unityCommandPlan)),
            UnityProofSummaryHash = Hash(Serialize(unityProof)),
            PreviewExportPayloadHash = Hash(Serialize(preview)),
            InvalidMatrixHash = Hash(Serialize(invalid)),
            Diagnostics = diagnostics
        };
    }

    private static string RenderReport(
        TimelineReport report,
        TimelineSourceManifest sourceManifest,
        TimelineMatrixSummary matrix,
        CrossSystemCascadeLedger cascades,
        ConflictArbitrationLedger arbitrations,
        SaveLoadReplayAudit replay,
        TimelineVarianceMetrics variance,
        TimelineUnityProofSummary unityProof,
        TimelineInvalidDiagnosticsMatrix invalid)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Goal 070 Integrated Campaign Timeline Simulation Matrix Report");
        builder.AppendLine();
        builder.AppendLine("integrated_campaign_timeline_simulation_matrix_verification required");
        builder.AppendLine("accepted=false");
        builder.AppendLine("implementationStatus=" + report.ImplementationStatus);
        builder.AppendLine("rowCount=" + report.RowCount);
        builder.AppendLine("stateChangingRowCount=" + report.StateChangingRowCount);
        builder.AppendLine("familyCount=" + report.FamilyCount);
        builder.AppendLine("seedCount=" + report.SeedCount);
        builder.AppendLine("sourceFactsConsumed=" + report.SourceFactsConsumed);
        builder.AppendLine("goal069AcceptedByUserHandoff=" + report.Goal069AcceptedByUserHandoff);
        builder.AppendLine("rowMatrixPassed=" + report.RowMatrixPassed);
        builder.AppendLine("cascadeLedgerPassed=" + report.CascadeLedgerPassed);
        builder.AppendLine("cascadeCount=" + report.CascadeCount);
        builder.AppendLine("arbitrationLedgerPassed=" + report.ArbitrationLedgerPassed);
        builder.AppendLine("arbitrationCount=" + report.ArbitrationCount);
        builder.AppendLine("saveLoadReplayPassed=" + report.SaveLoadReplayPassed);
        builder.AppendLine("saveLoadPassedRowCount=" + report.SaveLoadPassedRowCount);
        builder.AppendLine("replayPassedRowCount=" + report.ReplayPassedRowCount);
        builder.AppendLine("meaningfulVariancePassed=" + report.MeaningfulVariancePassed);
        builder.AppendLine("unityCommandPlanPassed=" + report.UnityCommandPlanPassed);
        builder.AppendLine("unityProofPassed=" + report.UnityProofPassed);
        builder.AppendLine("unityExitCode=" + (report.UnityExitCode?.ToString() ?? "null"));
        builder.AppendLine("playerExitCode=" + (report.PlayerExitCode?.ToString() ?? "null"));
        builder.AppendLine("provenRowCount=" + unityProof.ProvenRowCount);
        builder.AppendLine("allTimelineMarkersMatched=" + report.AllTimelineMarkersMatched);
        builder.AppendLine("previewExportPayloadPassed=" + report.PreviewExportPayloadPassed);
        builder.AppendLine("invalidMatrixPassed=" + report.InvalidMatrixPassed);
        builder.AppendLine("reportHash=" + report.DeterministicHash);
        builder.AppendLine();
        builder.AppendLine("## Source Gates");
        foreach (var gate in sourceManifest.PreflightGates)
        {
            builder.AppendLine("- " + gate.GateId + " " + gate.Status + " " + gate.ProvenanceKind);
        }

        builder.AppendLine();
        builder.AppendLine("## Matrix Rows");
        foreach (var row in matrix.Rows.OrderBy(item => item.RowId, StringComparer.Ordinal))
        {
            builder.AppendLine("- " + row.RowId
                + " family=" + row.FamilyId
                + " seed=" + row.SeedId
                + " ticks=" + row.Ticks.Count
                + " categories=" + row.TouchedSystemCategories.Count
                + " cascades=" + row.Cascades.Count
                + " arbitration=" + row.Arbitration.Decision
                + " stateChanged=" + row.StateChanging
                + " replay=" + row.SaveLoadReplayProof.ReplayDeterminismPassed);
        }

        builder.AppendLine();
        builder.AppendLine("## Cascade And Arbitration");
        builder.AppendLine("- cascadeRows=" + cascades.RowCount + " cascadeCount=" + cascades.CascadeCount);
        builder.AppendLine("- arbitrationRows=" + arbitrations.RowCount + " arbitrationCount=" + arbitrations.ArbitrationCount);
        builder.AppendLine();
        builder.AppendLine("## Replay And Variance");
        builder.AppendLine("- replayRows=" + replay.RowCount + " saveLoadPassed=" + replay.SaveLoadPassedRowCount + " replayPassed=" + replay.ReplayPassedRowCount);
        builder.AppendLine("- distinctPhaseProfiles=" + variance.DistinctPhaseProfileCount + " distinctRowHashes=" + variance.DistinctRowHashCount);
        builder.AppendLine();
        builder.AppendLine("## Invalid Matrix");
        foreach (var scenario in invalid.Scenarios)
        {
            builder.AppendLine("- " + scenario.ScenarioId + " " + scenario.ActualStatus);
        }

        builder.AppendLine();
        builder.AppendLine("## Diagnostics");
        foreach (var diagnostic in report.Diagnostics.Take(60))
        {
            builder.AppendLine("- [" + diagnostic.Severity + "] " + diagnostic.Code + " " + diagnostic.Target + " - " + diagnostic.Message);
        }

        return builder.ToString();
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            scenario = IntegratedCampaignTimelineVocabulary.ProductSmokeRoute,
            status = "produced",
            allowedArtifactRoot = IntegratedCampaignTimelineVocabulary.RelativeOutputDirectory,
            gate = IntegratedCampaignTimelineVocabulary.FinalGate,
            accepted = false
        });

    private static async Task WriteText(string directory, string fileName, string content, List<string> written, CancellationToken cancellationToken)
    {
        var path = Path.Combine(directory, fileName);
        EnsureContained(directory, path);
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await File.WriteAllTextAsync(path, content, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
                written.Add(path);
                return;
            }
            catch (IOException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task WriteBytes(string path, byte[] bytes, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (IOException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static void ResetDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        Directory.CreateDirectory(directory);
    }

    private static void EnsureContained(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes expected root: " + normalizedPath);
        }
    }

    private static string Serialize<T>(T value) =>
        IntegratedCampaignTimelineHash.Serialize(value);

    private static string Hash(string text) =>
        IntegratedCampaignTimelineHash.Sha256(text);
}
