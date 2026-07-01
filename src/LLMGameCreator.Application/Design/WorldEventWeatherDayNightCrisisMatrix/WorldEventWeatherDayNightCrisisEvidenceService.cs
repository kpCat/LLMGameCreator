using System.Text;

namespace LLMGameCreator.Application.Design.WorldEventWeatherDayNightCrisisMatrix;

public sealed class WorldEventWeatherDayNightCrisisEvidenceService
{
    public const string SourceManifestJsonFileName = "source-manifest.json";
    public const string WorldClockCalendarPolicyJsonFileName = "world-clock-calendar-policy.json";
    public const string WeatherHazardCatalogJsonFileName = "weather-hazard-catalog.json";
    public const string CrisisEventCatalogJsonFileName = "crisis-event-catalog.json";
    public const string RowMatrixJsonFileName = "world-event-weather-daynight-row-matrix.json";
    public const string SaveLoadReplayProofJsonFileName = "save-load-replay-proof.json";
    public const string VarianceMetricsJsonFileName = "variance-metrics.json";
    public const string UnityCommandPlanJsonFileName = "unity-command-plan.json";
    public const string UnityProofSummaryJsonFileName = "unity-proof-summary.json";
    public const string InvalidDiagnosticsMatrixJsonFileName = "invalid-diagnostics-matrix.json";
    public const string PreviewExportPayloadJsonFileName = "preview-export-payload.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "world-event-weather-daynight-crisis-matrix-report.md";
    public const string RowsDirectoryName = "rows";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public WorldEventBuildResult Build(string projectRootPath, WorldEventUnityProof? unityProof = null)
    {
        var source = new WorldEventWeatherDayNightCrisisSourceLoader().Load(projectRootPath);
        var projector = new WorldEventWeatherDayNightCrisisProjector();
        var sourceManifest = projector.BuildSourceManifest(source);
        var clockPolicy = projector.BuildWorldClockPolicy();
        var weatherCatalog = projector.BuildWeatherHazardCatalog();
        var crisisCatalog = projector.BuildCrisisEventCatalog();
        var rows = projector.BuildRows(source);
        var matrix = projector.BuildRowMatrix(rows);
        var replay = projector.BuildSaveLoadReplayProof(rows);
        var variance = projector.BuildVarianceMetrics(rows);
        var preview = projector.BuildPreviewExportPayload(rows);
        var unityCommandPlan = projector.BuildUnityCommandPlan(rows);
        var proof = unityProof ?? WorldEventWeatherDayNightCrisisUnityProofRunner.NotRequested(unityCommandPlan);
        var invalid = projector.BuildInvalidMatrix();
        var diagnostics = BuildDiagnostics(sourceManifest, clockPolicy, weatherCatalog, crisisCatalog, matrix, replay, variance, preview, unityCommandPlan, proof.PlayerProof, invalid);
        var reportWithoutHash = BuildReport(sourceManifest, clockPolicy, weatherCatalog, crisisCatalog, matrix, replay, variance, preview, unityCommandPlan, proof.PlayerProof, invalid, diagnostics);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new WorldEventBuildResult
        {
            SourceManifest = sourceManifest,
            WorldClockPolicy = clockPolicy,
            WeatherHazardCatalog = weatherCatalog,
            CrisisEventCatalog = crisisCatalog,
            RowMatrix = matrix,
            SaveLoadReplayProof = replay,
            VarianceMetrics = variance,
            UnityCommandPlan = unityCommandPlan,
            UnityProofSummary = proof.PlayerProof,
            InvalidMatrix = invalid,
            PreviewExportPayload = preview,
            Report = report,
            Rows = rows,
            StagingFiles = projector.BuildStagingFiles(source, unityCommandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, matrix, replay, variance, proof.PlayerProof, invalid)
        };
    }

    public async Task<WorldEventWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        WorldEventWeatherDayNightCrisisOptions options,
        CancellationToken cancellationToken = default)
    {
        var initial = Build(projectRootPath);
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutput: true, cancellationToken).ConfigureAwait(false);
        if (!options.ExecuteUnityProof)
        {
            return initialWrite;
        }

        var proof = new WorldEventWeatherDayNightCrisisUnityProofRunner().Run(
            projectRootPath,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            options);
        var final = Build(projectRootPath, proof);
        return await WriteAsync(projectRootPath, final, resetOutput: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<WorldEventWriteResult> WriteAsync(
        string projectRootPath,
        WorldEventBuildResult result,
        bool resetOutput = true,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, WorldEventWeatherDayNightCrisisVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
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
        await WriteText(outputDirectory, WorldClockCalendarPolicyJsonFileName, Serialize(result.WorldClockPolicy), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, WeatherHazardCatalogJsonFileName, Serialize(result.WeatherHazardCatalog), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, CrisisEventCatalogJsonFileName, Serialize(result.CrisisEventCatalog), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, RowMatrixJsonFileName, Serialize(result.RowMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SaveLoadReplayProofJsonFileName, Serialize(result.SaveLoadReplayProof), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, VarianceMetricsJsonFileName, Serialize(result.VarianceMetrics), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityCommandPlanJsonFileName, Serialize(result.UnityCommandPlan), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityProofSummaryJsonFileName, Serialize(result.UnityProofSummary), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, InvalidDiagnosticsMatrixJsonFileName, Serialize(result.InvalidMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, PreviewExportPayloadJsonFileName, Serialize(result.PreviewExportPayload), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ArtifactScopeReportJsonFileName, RenderArtifactScopeReportJson(), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ReportMarkdownFileName, result.ReportMarkdown, written, cancellationToken).ConfigureAwait(false);

        var rowsDirectory = Path.Combine(outputDirectory, RowsDirectoryName);
        Directory.CreateDirectory(rowsDirectory);
        foreach (var row in result.Rows.OrderBy(item => item.RowId, StringComparer.Ordinal))
        {
            await WriteText(rowsDirectory, RowFileName(row), Serialize(row), written, cancellationToken).ConfigureAwait(false);
        }

        foreach (var file in result.StagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, WorldEventWeatherDayNightCrisisVocabulary.StagingRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await WriteBytes(path, file.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        return new WorldEventWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, WorldEventWeatherDayNightCrisisVocabulary.StagingRoot),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            WrittenFiles = written.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    public static string RowFileName(WorldEventRow row) =>
        row.FamilyId.Replace('_', '-') + "-" + row.SeedId.Replace('_', '-') + "-world-event-row.json";

    private static IReadOnlyList<WorldEventDiagnostic> BuildDiagnostics(
        WorldEventSourceManifest sourceManifest,
        WorldClockCalendarPolicy clockPolicy,
        WeatherHazardCatalog weatherCatalog,
        CrisisEventCatalog crisisCatalog,
        WorldEventRowMatrix matrix,
        WorldEventSaveLoadReplayProof replay,
        WorldEventVarianceMetrics variance,
        WorldEventPreviewExportPayload preview,
        WorldEventUnityCommandPlan unityCommandPlan,
        WorldEventUnityProofSummary unityProof,
        WorldEventInvalidDiagnosticsMatrix invalid)
    {
        var validator = new WorldEventWeatherDayNightCrisisValidator();
        return WorldEventWeatherDayNightCrisisValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateCatalogs(clockPolicy, weatherCatalog, crisisCatalog))
                .Concat(validator.ValidateRows(matrix, preview))
                .Concat(validator.ValidateReplay(replay))
                .Concat(validator.ValidateVariance(variance))
                .Concat(validator.ValidateUnityCommandPlan(unityCommandPlan))
                .Concat(validator.ValidateUnityProof(unityCommandPlan, unityProof))
                .Concat(validator.ValidateInvalidMatrix(invalid)));
    }

    private static WorldEventReport BuildReport(
        WorldEventSourceManifest sourceManifest,
        WorldClockCalendarPolicy clockPolicy,
        WeatherHazardCatalog weatherCatalog,
        CrisisEventCatalog crisisCatalog,
        WorldEventRowMatrix matrix,
        WorldEventSaveLoadReplayProof replay,
        WorldEventVarianceMetrics variance,
        WorldEventPreviewExportPayload preview,
        WorldEventUnityCommandPlan unityCommandPlan,
        WorldEventUnityProofSummary unityProof,
        WorldEventInvalidDiagnosticsMatrix invalid,
        IReadOnlyList<WorldEventDiagnostic> diagnostics)
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
            && sourceManifest.Goal068CombatMagicRowsConsumed;
        var green = noErrors
            && sourceManifest.Goal068AcceptedByUserHandoff
            && sourceConsumed
            && clockPolicy.Passed
            && weatherCatalog.Passed
            && crisisCatalog.Passed
            && matrix.Passed
            && replay.Passed
            && variance.Passed
            && preview.Passed
            && unityCommandPlan.Passed
            && unityProof.Passed
            && invalid.Passed;
        var failed = diagnostics.Any(item => item.Severity == "error" && !item.Code.StartsWith("goal069.unity.", StringComparison.Ordinal));

        return new WorldEventReport
        {
            ImplementationStatus = green ? "GREEN" : failed ? "FAILED" : "BLOCKED",
            Accepted = false,
            Goal068AcceptedByUserHandoff = sourceManifest.Goal068AcceptedByUserHandoff,
            SourceFactsConsumed = sourceConsumed,
            WorldClockPolicyPassed = clockPolicy.Passed,
            WeatherHazardCatalogPassed = weatherCatalog.Passed,
            CrisisEventCatalogPassed = crisisCatalog.Passed,
            RowMatrixPassed = matrix.Passed,
            SaveLoadReplayPassed = replay.Passed,
            MeaningfulVariancePassed = variance.Passed,
            UnityCommandPlanPassed = unityCommandPlan.Passed,
            UnityProofPassed = unityProof.Passed,
            UnityExitCode = unityProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerExitCode,
            AllWorldEventMarkersMatched = unityProof.Passed && unityProof.MissingMarkers.Count == 0,
            PreviewExportPayloadPassed = preview.Passed,
            InvalidMatrixPassed = invalid.Passed,
            RowCount = matrix.RowCount,
            StateChangingRowCount = matrix.StateChangingRowCount,
            FamilyCount = matrix.FamilyCount,
            SeedCount = matrix.SeedCount,
            SourceManifestHash = Hash(Serialize(sourceManifest)),
            ClockPolicyHash = Hash(Serialize(clockPolicy)),
            WeatherHazardCatalogHash = Hash(Serialize(weatherCatalog)),
            CrisisEventCatalogHash = Hash(Serialize(crisisCatalog)),
            RowMatrixHash = Hash(Serialize(matrix)),
            SaveLoadReplayProofHash = Hash(Serialize(replay)),
            VarianceMetricsHash = Hash(Serialize(variance)),
            UnityCommandPlanHash = Hash(Serialize(unityCommandPlan)),
            UnityProofSummaryHash = Hash(Serialize(unityProof)),
            PreviewExportPayloadHash = Hash(Serialize(preview)),
            InvalidMatrixHash = Hash(Serialize(invalid)),
            Diagnostics = diagnostics
        };
    }

    private static string RenderReport(
        WorldEventReport report,
        WorldEventSourceManifest sourceManifest,
        WorldEventRowMatrix matrix,
        WorldEventSaveLoadReplayProof replay,
        WorldEventVarianceMetrics variance,
        WorldEventUnityProofSummary unityProof,
        WorldEventInvalidDiagnosticsMatrix invalid)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Goal 069 World Event Weather Day/Night Crisis Matrix Report");
        builder.AppendLine();
        builder.AppendLine("world_event_weather_daynight_crisis_matrix_verification required");
        builder.AppendLine("accepted=false");
        builder.AppendLine("implementationStatus=" + report.ImplementationStatus);
        builder.AppendLine("rowCount=" + report.RowCount);
        builder.AppendLine("stateChangingRowCount=" + report.StateChangingRowCount);
        builder.AppendLine("familyCount=" + report.FamilyCount);
        builder.AppendLine("seedCount=" + report.SeedCount);
        builder.AppendLine("sourceFactsConsumed=" + report.SourceFactsConsumed);
        builder.AppendLine("goal068AcceptedByUserHandoff=" + report.Goal068AcceptedByUserHandoff);
        builder.AppendLine("worldClockPolicyPassed=" + report.WorldClockPolicyPassed);
        builder.AppendLine("weatherHazardCatalogPassed=" + report.WeatherHazardCatalogPassed);
        builder.AppendLine("crisisEventCatalogPassed=" + report.CrisisEventCatalogPassed);
        builder.AppendLine("rowMatrixPassed=" + report.RowMatrixPassed);
        builder.AppendLine("saveLoadReplayPassed=" + report.SaveLoadReplayPassed);
        builder.AppendLine("meaningfulVariancePassed=" + report.MeaningfulVariancePassed);
        builder.AppendLine("unityCommandPlanPassed=" + report.UnityCommandPlanPassed);
        builder.AppendLine("unityProofPassed=" + report.UnityProofPassed);
        builder.AppendLine("unityExitCode=" + (report.UnityExitCode?.ToString() ?? "null"));
        builder.AppendLine("playerExitCode=" + (report.PlayerExitCode?.ToString() ?? "null"));
        builder.AppendLine("provenRowCount=" + unityProof.ProvenRowCount);
        builder.AppendLine("allWorldEventMarkersMatched=" + report.AllWorldEventMarkersMatched);
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
                + " phase=" + row.WorldClockAfter.Phase
                + " weather=" + row.WeatherHazard.WeatherId
                + " crisis=" + row.CrisisEvent.CrisisId
                + " stateChanged=" + row.StateChanging
                + " replay=" + row.SaveLoadReplayProof.ReplayDeterminismPassed);
        }

        builder.AppendLine();
        builder.AppendLine("## Replay And Variance");
        builder.AppendLine("- replayRows=" + replay.RowCount + " saveLoadPassed=" + replay.SaveLoadPassedRowCount + " replayPassed=" + replay.ReplayPassedRowCount);
        builder.AppendLine("- distinctWeather=" + variance.DistinctWeatherCount + " distinctCrisis=" + variance.DistinctCrisisCount + " distinctPhaseTransitions=" + variance.DistinctPhaseTransitionCount);
        builder.AppendLine();
        builder.AppendLine("## Invalid Matrix");
        foreach (var scenario in invalid.Scenarios)
        {
            builder.AppendLine("- " + scenario.ScenarioId + " " + scenario.ActualStatus);
        }

        builder.AppendLine();
        builder.AppendLine("## Diagnostics");
        foreach (var diagnostic in report.Diagnostics.Take(40))
        {
            builder.AppendLine("- [" + diagnostic.Severity + "] " + diagnostic.Code + " " + diagnostic.Target + " - " + diagnostic.Message);
        }

        return builder.ToString();
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            scenario = WorldEventWeatherDayNightCrisisVocabulary.ProductSmokeRoute,
            status = "produced",
            allowedArtifactRoot = WorldEventWeatherDayNightCrisisVocabulary.RelativeOutputDirectory,
            gate = WorldEventWeatherDayNightCrisisVocabulary.FinalGate,
            accepted = false
        });

    private static async Task WriteText(string directory, string fileName, string content, List<string> written, CancellationToken cancellationToken)
    {
        var path = Path.Combine(directory, fileName);
        EnsureContained(directory, path);
        await File.WriteAllTextAsync(path, content, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(path);
    }

    private static async Task WriteText(string directory, string fileName, string content, List<string> written, CancellationToken cancellationToken, int retryCount = 3)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await WriteText(directory, fileName, content, written, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (IOException) when (attempt < retryCount)
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
        WorldEventWeatherDayNightCrisisHash.Serialize(value);

    private static string Hash(string text) =>
        WorldEventWeatherDayNightCrisisHash.Sha256(text);
}
