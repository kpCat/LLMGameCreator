using System.Text;

namespace LLMGameCreator.Application.Design.LivingWorldNpcFactionSimulationMatrix;

public sealed class LivingWorldNpcFactionSimulationEvidenceService
{
    public const string SourceManifestJsonFileName = "source-manifest.json";
    public const string CatalogSummaryJsonFileName = "actor-faction-catalog-summary.json";
    public const string SimulationMatrixPlanJsonFileName = "simulation-matrix-plan.json";
    public const string SaveLoadReplayProofJsonFileName = "save-load-replay-proof.json";
    public const string VarianceMetricsJsonFileName = "variance-metrics.json";
    public const string UnityCommandPlanJsonFileName = "unity-command-plan.json";
    public const string UnityProofSummaryJsonFileName = "unity-player-proof-summary.json";
    public const string PreviewExportPayloadJsonFileName = "preview-export-living-world-payload.json";
    public const string InvalidDiagnosticsMatrixJsonFileName = "invalid-diagnostics-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "living-world-npc-faction-simulation-matrix-report.md";
    public const string RowsDirectoryName = "rows";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public LivingWorldNpcFactionSimulationBuildResult Build(string projectRootPath, LivingWorldUnityProof? unityProof = null)
    {
        var source = new LivingWorldNpcFactionSimulationSourceLoader().Load(projectRootPath);
        var builder = new LivingWorldNpcFactionSimulationBuilder();
        var sourceManifest = builder.BuildSourceManifest(source);
        var rows = builder.BuildRows(source);
        var catalog = builder.BuildCatalogSummary(rows);
        var matrix = builder.BuildSimulationMatrixPlan(rows);
        var replay = builder.BuildSaveLoadReplayProof(rows);
        var variance = builder.BuildVarianceMetrics(rows);
        var unityCommandPlan = builder.BuildUnityCommandPlan(rows);
        var proof = unityProof ?? LivingWorldNpcFactionUnityProofRunner.NotRequested(unityCommandPlan);
        var preview = builder.BuildPreviewExportPayload(rows);
        var invalid = builder.BuildInvalidMatrix();
        var diagnostics = BuildDiagnostics(sourceManifest, catalog, matrix, replay, variance, unityCommandPlan, proof.PlayerProof, preview, invalid);
        var reportWithoutHash = BuildReport(sourceManifest, catalog, matrix, replay, variance, unityCommandPlan, proof.PlayerProof, preview, invalid, diagnostics);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new LivingWorldNpcFactionSimulationBuildResult
        {
            SourceManifest = sourceManifest,
            CatalogSummary = catalog,
            SimulationMatrixPlan = matrix,
            SaveLoadReplayProof = replay,
            VarianceMetrics = variance,
            UnityCommandPlan = unityCommandPlan,
            UnityProofSummary = proof.PlayerProof,
            PreviewExportPayload = preview,
            InvalidMatrix = invalid,
            Report = report,
            Rows = rows,
            StagingFiles = builder.BuildStagingFiles(source, unityCommandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, matrix, replay, variance, proof.PlayerProof, invalid)
        };
    }

    public async Task<LivingWorldNpcFactionSimulationWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        LivingWorldNpcFactionSimulationOptions options,
        CancellationToken cancellationToken = default)
    {
        var initial = Build(projectRootPath);
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutput: true, cancellationToken).ConfigureAwait(false);
        if (!options.ExecuteUnityProof)
        {
            return initialWrite;
        }

        var proof = new LivingWorldNpcFactionUnityProofRunner().Run(
            projectRootPath,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            options);
        var final = Build(projectRootPath, proof);
        return await WriteAsync(projectRootPath, final, resetOutput: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<LivingWorldNpcFactionSimulationWriteResult> WriteAsync(
        string projectRootPath,
        LivingWorldNpcFactionSimulationBuildResult result,
        bool resetOutput = true,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, LivingWorldNpcFactionSimulationVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
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
        await WriteText(outputDirectory, CatalogSummaryJsonFileName, Serialize(result.CatalogSummary), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SimulationMatrixPlanJsonFileName, Serialize(result.SimulationMatrixPlan), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SaveLoadReplayProofJsonFileName, Serialize(result.SaveLoadReplayProof), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, VarianceMetricsJsonFileName, Serialize(result.VarianceMetrics), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityCommandPlanJsonFileName, Serialize(result.UnityCommandPlan), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityProofSummaryJsonFileName, Serialize(result.UnityProofSummary), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, PreviewExportPayloadJsonFileName, Serialize(result.PreviewExportPayload), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, InvalidDiagnosticsMatrixJsonFileName, Serialize(result.InvalidMatrix), written, cancellationToken).ConfigureAwait(false);
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
            var path = Path.Combine(outputDirectory, LivingWorldNpcFactionSimulationVocabulary.StagingRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, file.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        return new LivingWorldNpcFactionSimulationWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, LivingWorldNpcFactionSimulationVocabulary.StagingRoot),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    public static string RowFileName(LivingWorldSimulationRow row) =>
        row.FamilyId + "-" + row.SeedId + "-living-world-row.json";

    private static IReadOnlyList<LivingWorldDiagnostic> BuildDiagnostics(
        LivingWorldSourceManifest sourceManifest,
        LivingWorldActorFactionCatalogSummary catalog,
        LivingWorldSimulationMatrixPlan matrix,
        LivingWorldSaveLoadReplayProof replay,
        LivingWorldVarianceMetrics variance,
        LivingWorldUnityCommandPlan unityCommandPlan,
        LivingWorldUnityProofSummary unityProof,
        LivingWorldPreviewExportPayload preview,
        InvalidLivingWorldDiagnosticsMatrix invalid)
    {
        var validator = new LivingWorldNpcFactionSimulationValidator();
        return LivingWorldNpcFactionSimulationValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateSimulation(catalog, matrix, preview))
                .Concat(validator.ValidateReplayAndVariance(replay, variance))
                .Concat(validator.ValidateUnityCommandPlan(unityCommandPlan))
                .Concat(validator.ValidateUnityProof(unityCommandPlan, unityProof))
                .Concat(validator.ValidateInvalidMatrix(invalid)));
    }

    private static LivingWorldNpcFactionSimulationReport BuildReport(
        LivingWorldSourceManifest sourceManifest,
        LivingWorldActorFactionCatalogSummary catalog,
        LivingWorldSimulationMatrixPlan matrix,
        LivingWorldSaveLoadReplayProof replay,
        LivingWorldVarianceMetrics variance,
        LivingWorldUnityCommandPlan unityCommandPlan,
        LivingWorldUnityProofSummary unityProof,
        LivingWorldPreviewExportPayload preview,
        InvalidLivingWorldDiagnosticsMatrix invalid,
        IReadOnlyList<LivingWorldDiagnostic> diagnostics)
    {
        var noErrors = diagnostics.All(item => item.Severity != "error");
        var green = noErrors
            && sourceManifest.Goal063AcceptedByUserHandoff
            && sourceManifest.Goal060PackageRowsConsumed
            && sourceManifest.Goal061ReviewRowsConsumed
            && sourceManifest.Goal062SpatialRowsConsumed
            && sourceManifest.Goal063GameplayRowsConsumed
            && catalog.Passed
            && matrix.Passed
            && replay.Passed
            && variance.Passed
            && unityCommandPlan.Passed
            && unityProof.Passed
            && preview.Passed
            && invalid.Passed;

        var failed = diagnostics.Any(item => item.Severity == "error" && !item.Code.StartsWith("goal064.unity.", StringComparison.Ordinal));

        return new LivingWorldNpcFactionSimulationReport
        {
            ImplementationStatus = green ? "GREEN" : failed ? "FAILED" : "BLOCKED",
            Accepted = false,
            Goal063AcceptedByUserHandoff = sourceManifest.Goal063AcceptedByUserHandoff,
            RowCount = matrix.RowCount,
            FamilyCount = matrix.FamilyCount,
            SeedCount = matrix.SeedCount,
            StateChangingRowCount = matrix.StateChangingRowCount,
            SourceFactsConsumed = sourceManifest.Goal060PackageRowsConsumed && sourceManifest.Goal061ReviewRowsConsumed && sourceManifest.Goal062SpatialRowsConsumed && sourceManifest.Goal063GameplayRowsConsumed,
            CatalogPassed = catalog.Passed,
            SimulationMatrixPassed = matrix.Passed,
            SaveLoadReplayPassed = replay.Passed,
            MeaningfulVariancePassed = variance.Passed,
            UnityCommandPlanPassed = unityCommandPlan.Passed,
            UnityProofPassed = unityProof.Passed,
            UnityExitCode = unityProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerExitCode,
            AllLivingWorldMarkersMatched = unityProof.Passed && unityProof.MissingMarkers.Count == 0,
            PreviewExportPayloadPassed = preview.Passed,
            InvalidMatrixPassed = invalid.Passed,
            SourceManifestHash = Hash(Serialize(sourceManifest)),
            CatalogHash = Hash(Serialize(catalog)),
            SimulationMatrixPlanHash = Hash(Serialize(matrix)),
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
        LivingWorldNpcFactionSimulationReport report,
        LivingWorldSourceManifest sourceManifest,
        LivingWorldSimulationMatrixPlan matrix,
        LivingWorldSaveLoadReplayProof replay,
        LivingWorldVarianceMetrics variance,
        LivingWorldUnityProofSummary unityProof,
        InvalidLivingWorldDiagnosticsMatrix invalid)
    {
        var lines = new List<string>
        {
            "# Living World NPC/Faction Simulation Matrix Report",
            string.Empty,
            "living_world_npc_faction_simulation_matrix_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            $"manualGate={LivingWorldNpcFactionSimulationVocabulary.FinalGate}",
            $"goal063AcceptedByUserHandoff={report.Goal063AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"rowCount={report.RowCount}",
            $"familyCount={report.FamilyCount}",
            $"seedCount={report.SeedCount}",
            $"stateChangingRowCount={report.StateChangingRowCount}",
            $"saveLoadReplayPassed={report.SaveLoadReplayPassed.ToString().ToLowerInvariant()}",
            $"meaningfulVariancePassed={report.MeaningfulVariancePassed.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allLivingWorldMarkersMatched={report.AllLivingWorldMarkersMatched.ToString().ToLowerInvariant()}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"catalogHash={report.CatalogHash}",
            $"simulationMatrixPlanHash={report.SimulationMatrixPlanHash}",
            $"saveLoadReplayProofHash={report.SaveLoadReplayProofHash}",
            $"varianceMetricsHash={report.VarianceMetricsHash}",
            $"unityCommandPlanHash={report.UnityCommandPlanHash}",
            $"unityProofSummaryHash={report.UnityProofSummaryHash}",
            $"previewExportPayloadHash={report.PreviewExportPayloadHash}",
            $"invalidMatrixHash={report.InvalidMatrixHash}",
            $"reportHash={report.DeterministicHash}",
            string.Empty,
            "## Source Chain",
            string.Empty
        };
        lines.AddRange(sourceManifest.PreflightGates.Select(item => $"- {item.GateId}: status={item.Status}, provenance={item.ProvenanceKind}, evidence={item.EvidenceRef}"));
        lines.AddRange(sourceManifest.SourceArtifactRefs.Select(item => $"- {item.ArtifactFamily}: artifact={item.ArtifactRelativePath}, exists={item.Exists.ToString().ToLowerInvariant()}, hashMatches={item.HashMatches.ToString().ToLowerInvariant()}, hash={item.ArtifactHash}"));
        lines.Add(string.Empty);
        lines.Add("## Simulation Matrix");
        lines.Add(string.Empty);
        foreach (var row in matrix.Rows)
        {
            lines.Add($"- {row.RowId}: family={row.FamilyId}, seed={row.SeedId}, actors={row.ActorRecords.Count}, factions={row.FactionRecords.Count}, ticks={row.OrderedTickPlan.Count}, deltas={row.StateDeltaSummary.Count}, before={row.BeforeState.StateHash}, after={row.AfterState.StateHash}");
            lines.AddRange(row.OrderedTickPlan.Select(tick => $"  - {tick.TickId}: kind={tick.TickKind}, actor={tick.ActorId}, faction={tick.FactionId}, event={tick.EventId}, changedKeys={string.Join(",", tick.ChangedKeys.Order(StringComparer.Ordinal))}"));
        }

        lines.Add(string.Empty);
        lines.Add("## Save/load/replay");
        lines.Add(string.Empty);
        lines.AddRange(replay.Rows.Select(item => $"- {item.RowId}: changed={item.BeforeAfterStateChanged.ToString().ToLowerInvariant()}, saveLoad={item.SaveLoadRoundtripPassed.ToString().ToLowerInvariant()}, replay={item.ReplayDeterminismPassed.ToString().ToLowerInvariant()}, hash={item.FirstReplayHash}"));
        lines.Add(string.Empty);
        lines.Add("## Variance");
        lines.Add(string.Empty);
        lines.Add($"- hashOnlyVarianceRejected: {variance.HashOnlyVarianceRejected.ToString().ToLowerInvariant()}");
        lines.Add($"- sameFamilySeedVariationPassed: {variance.SameFamilySeedVariationPassed.ToString().ToLowerInvariant()}");
        lines.Add($"- crossFamilyRuleVariationPassed: {variance.CrossFamilyRuleVariationPassed.ToString().ToLowerInvariant()}");
        lines.AddRange(variance.Families.Select(item => $"- {item.FamilyId}: rows={item.RowCount}, seedVariation={item.SameFamilySeedVariationPassed.ToString().ToLowerInvariant()}, axes={string.Join(",", item.MeaningfulAxes)}, rowHashes={item.RowHashes.Count}"));
        lines.Add(string.Empty);
        lines.Add("## Unity Proof");
        lines.Add(string.Empty);
        lines.Add($"- passed: {unityProof.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- unityExitCode: {TextOrNone(unityProof.UnityExitCode?.ToString())}");
        lines.Add($"- playerExitCode: {TextOrNone(unityProof.PlayerExitCode?.ToString())}");
        lines.Add($"- provenRowCount: {unityProof.ProvenRowCount}");
        lines.Add($"- missingMarkers: {unityProof.MissingMarkers.Count}");
        lines.AddRange(unityProof.MatchedMarkers.Select(marker => $"- matchedMarker: {marker}"));
        lines.AddRange(unityProof.MissingMarkers.Select(marker => $"- missingMarker: {marker}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak Matrix");
        lines.Add(string.Empty);
        lines.Add($"- passed: {invalid.Passed.ToString().ToLowerInvariant()}");
        lines.Add($"- scenarioCount: {invalid.ScenarioCount}");
        lines.AddRange(invalid.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution/project file change, new dependency, provider/LLM/RAG/media generation call, or arbitrary Lua execution/source generation is part of this Goal 064 proof. Unity changes are limited to deterministic diagnostic marker loading in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("living_world_npc_faction_simulation_matrix_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            schemaVersion = "goal064_artifact_scope_report_v1",
            scenario = LivingWorldNpcFactionSimulationVocabulary.ProductSmokeRoute,
            gate = LivingWorldNpcFactionSimulationVocabulary.FinalGate + " required",
            allowedArtifactRoot = LivingWorldNpcFactionSimulationVocabulary.RelativeOutputDirectory + "/",
            allowedCodeRoot = "src/LLMGameCreator.Application/Design/LivingWorldNpcFactionSimulationMatrix/",
            allowedTestsRoot = "tests/LLMGameCreator.Tests/Application/LivingWorldNpcFactionSimulationMatrix/",
            allowedProductSmoke = "tests/LLMGameCreator.Tests/ProductSmoke/LivingWorldNpcFactionSimulationMatrixProductSmokeTests.cs",
            narrowUnityAllowance = "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs",
            forbiddenChanges = new[]
            {
                "public GamePackage schema/model definitions",
                "Runtime/Runtime.Abstractions",
                "WinForms UI",
                "Infrastructure provider/LLM/RAG paths",
                "generator-library",
                "solution/project files",
                "external dependencies",
                "arbitrary Lua execution/source generation"
            }
        });

    private static async Task WriteText(
        string directory,
        string fileName,
        string text,
        List<string> written,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(directory, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, text.TrimEnd('\r', '\n') + Environment.NewLine, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(path);
    }

    private static string TextOrNone(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "(none)" : value;

    private static string Serialize<T>(T value) => LivingWorldNpcFactionSimulationHash.Serialize(value);

    private static string Hash(string text) => LivingWorldNpcFactionSimulationHash.Hash(text);

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
}
