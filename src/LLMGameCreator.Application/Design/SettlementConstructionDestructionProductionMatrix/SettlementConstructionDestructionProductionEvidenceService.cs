using System.Text;

namespace LLMGameCreator.Application.Design.SettlementConstructionDestructionProductionMatrix;

public sealed class SettlementConstructionDestructionProductionEvidenceService
{
    public const string SourceManifestJsonFileName = "settlement-construction-source-manifest.json";
    public const string RowMatrixJsonFileName = "settlement-construction-row-matrix.json";
    public const string BuildingCatalogJsonFileName = "settlement-building-catalog.json";
    public const string ProductionLedgerJsonFileName = "settlement-production-ledger.json";
    public const string DestructionRepairLedgerJsonFileName = "settlement-destruction-repair-ledger.json";
    public const string DefenseThreatLedgerJsonFileName = "settlement-defense-threat-ledger.json";
    public const string LivingWorldLinkageJsonFileName = "settlement-living-world-linkage.json";
    public const string SaveLoadReplayProofJsonFileName = "settlement-save-load-replay-proof.json";
    public const string UnityCommandPlanJsonFileName = "settlement-unity-command-plan.json";
    public const string UnityProofSummaryJsonFileName = "settlement-unity-player-proof-summary.json";
    public const string InvalidDiagnosticsMatrixJsonFileName = "settlement-invalid-diagnostics-matrix.json";
    public const string PreviewExportPayloadJsonFileName = "settlement-preview-export-payload.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "settlement-construction-destruction-production-matrix-report.md";
    public const string RowsDirectoryName = "rows";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public SettlementBuildResult Build(string projectRootPath, SettlementUnityProof? unityProof = null)
    {
        var source = new SettlementConstructionDestructionProductionSourceLoader().Load(projectRootPath);
        var builder = new SettlementConstructionDestructionProductionBuilder();
        var sourceManifest = builder.BuildSourceManifest(source);
        var catalog = builder.BuildBuildingCatalog();
        var rows = builder.BuildRows(source);
        var matrix = builder.BuildRowMatrix(rows);
        var production = builder.BuildProductionLedger(rows);
        var destructionRepair = builder.BuildDestructionRepairLedger(rows);
        var defenseThreat = builder.BuildDefenseThreatLedger(rows);
        var livingWorld = builder.BuildLivingWorldLinkage(rows);
        var replay = builder.BuildSaveLoadReplayProof(rows);
        var meaningfulVariancePassed = builder.MeaningfulVariancePassed(rows);
        var unityCommandPlan = builder.BuildUnityCommandPlan(rows);
        var proof = unityProof ?? SettlementConstructionUnityProofRunner.NotRequested(unityCommandPlan);
        var preview = builder.BuildPreviewExportPayload(rows);
        var invalid = builder.BuildInvalidMatrix();
        var diagnostics = BuildDiagnostics(sourceManifest, catalog, matrix, production, destructionRepair, defenseThreat, livingWorld, replay, meaningfulVariancePassed, unityCommandPlan, proof.PlayerProof, preview, invalid);
        var reportWithoutHash = BuildReport(sourceManifest, catalog, matrix, production, destructionRepair, defenseThreat, livingWorld, replay, meaningfulVariancePassed, unityCommandPlan, proof.PlayerProof, preview, invalid, diagnostics);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new SettlementBuildResult
        {
            SourceManifest = sourceManifest,
            BuildingCatalog = catalog,
            RowMatrix = matrix,
            ProductionLedger = production,
            DestructionRepairLedger = destructionRepair,
            DefenseThreatLedger = defenseThreat,
            LivingWorldLinkage = livingWorld,
            SaveLoadReplayProof = replay,
            UnityCommandPlan = unityCommandPlan,
            UnityProofSummary = proof.PlayerProof,
            PreviewExportPayload = preview,
            InvalidMatrix = invalid,
            Report = report,
            Rows = rows,
            StagingFiles = builder.BuildStagingFiles(source, unityCommandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, matrix, production, destructionRepair, defenseThreat, livingWorld, replay, proof.PlayerProof, invalid)
        };
    }

    public async Task<SettlementWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        SettlementConstructionOptions options,
        CancellationToken cancellationToken = default)
    {
        var initial = Build(projectRootPath);
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutput: true, cancellationToken).ConfigureAwait(false);
        if (!options.ExecuteUnityProof)
        {
            return initialWrite;
        }

        var proof = new SettlementConstructionUnityProofRunner().Run(
            projectRootPath,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            options);
        var final = Build(projectRootPath, proof);
        return await WriteAsync(projectRootPath, final, resetOutput: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SettlementWriteResult> WriteAsync(
        string projectRootPath,
        SettlementBuildResult result,
        bool resetOutput = true,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, SettlementConstructionDestructionProductionVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
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
        await WriteText(outputDirectory, RowMatrixJsonFileName, Serialize(result.RowMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, BuildingCatalogJsonFileName, Serialize(result.BuildingCatalog), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ProductionLedgerJsonFileName, Serialize(result.ProductionLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, DestructionRepairLedgerJsonFileName, Serialize(result.DestructionRepairLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, DefenseThreatLedgerJsonFileName, Serialize(result.DefenseThreatLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, LivingWorldLinkageJsonFileName, Serialize(result.LivingWorldLinkage), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SaveLoadReplayProofJsonFileName, Serialize(result.SaveLoadReplayProof), written, cancellationToken).ConfigureAwait(false);
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
            var path = Path.Combine(outputDirectory, SettlementConstructionDestructionProductionVocabulary.StagingRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await WriteBytes(path, file.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        return new SettlementWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, SettlementConstructionDestructionProductionVocabulary.StagingRoot),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    public static string RowFileName(SettlementRow row) =>
        row.FamilyId.Replace('_', '-') + "-" + row.SeedId.Replace('_', '-') + "-settlement-row.json";

    private static IReadOnlyList<SettlementDiagnostic> BuildDiagnostics(
        SettlementSourceManifest sourceManifest,
        SettlementBuildingCatalog catalog,
        SettlementRowMatrix matrix,
        SettlementLedger production,
        SettlementLedger destructionRepair,
        SettlementLedger defenseThreat,
        SettlementLivingWorldLinkageMatrix livingWorld,
        SettlementSaveLoadReplayProof replay,
        bool meaningfulVariancePassed,
        SettlementUnityCommandPlan unityCommandPlan,
        SettlementUnityProofSummary unityProof,
        SettlementPreviewExportPayload preview,
        InvalidSettlementDiagnosticsMatrix invalid)
    {
        var validator = new SettlementConstructionDestructionProductionValidator();
        return SettlementConstructionDestructionProductionValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateRows(catalog, matrix, preview, meaningfulVariancePassed))
                .Concat(validator.ValidateLedgers(production, destructionRepair, defenseThreat, livingWorld))
                .Concat(validator.ValidateReplay(replay))
                .Concat(validator.ValidateUnityCommandPlan(unityCommandPlan))
                .Concat(validator.ValidateUnityProof(unityCommandPlan, unityProof))
                .Concat(validator.ValidateInvalidMatrix(invalid)));
    }

    private static SettlementReport BuildReport(
        SettlementSourceManifest sourceManifest,
        SettlementBuildingCatalog catalog,
        SettlementRowMatrix matrix,
        SettlementLedger production,
        SettlementLedger destructionRepair,
        SettlementLedger defenseThreat,
        SettlementLivingWorldLinkageMatrix livingWorld,
        SettlementSaveLoadReplayProof replay,
        bool meaningfulVariancePassed,
        SettlementUnityCommandPlan unityCommandPlan,
        SettlementUnityProofSummary unityProof,
        SettlementPreviewExportPayload preview,
        InvalidSettlementDiagnosticsMatrix invalid,
        IReadOnlyList<SettlementDiagnostic> diagnostics)
    {
        var noErrors = diagnostics.All(item => item.Severity != "error");
        var sourceConsumed = sourceManifest.Goal060PackageRowsConsumed
            && sourceManifest.Goal061ReviewRowsConsumed
            && sourceManifest.Goal062SpatialRowsConsumed
            && sourceManifest.Goal063GameplayRowsConsumed
            && sourceManifest.Goal064LivingWorldRowsConsumed
            && sourceManifest.Goal065InterlockedRowsConsumed;
        var green = noErrors
            && sourceManifest.Goal065AcceptedByUserHandoff
            && sourceConsumed
            && catalog.Passed
            && matrix.Passed
            && production.Passed
            && destructionRepair.Passed
            && defenseThreat.Passed
            && livingWorld.Passed
            && replay.Passed
            && meaningfulVariancePassed
            && unityCommandPlan.Passed
            && unityProof.Passed
            && preview.Passed
            && invalid.Passed;
        var failed = diagnostics.Any(item => item.Severity == "error" && !item.Code.StartsWith("goal066.unity.", StringComparison.Ordinal));

        return new SettlementReport
        {
            ImplementationStatus = green ? "GREEN" : failed ? "FAILED" : "BLOCKED",
            Accepted = false,
            Goal065AcceptedByUserHandoff = sourceManifest.Goal065AcceptedByUserHandoff,
            SourceFactsConsumed = sourceConsumed,
            BuildingCatalogPassed = catalog.Passed,
            RowMatrixPassed = matrix.Passed,
            ProductionLedgerPassed = production.Passed,
            DestructionRepairLedgerPassed = destructionRepair.Passed,
            DefenseThreatLedgerPassed = defenseThreat.Passed,
            LivingWorldLinkagePassed = livingWorld.Passed,
            SaveLoadReplayPassed = replay.Passed,
            MeaningfulVariancePassed = meaningfulVariancePassed,
            UnityCommandPlanPassed = unityCommandPlan.Passed,
            UnityProofPassed = unityProof.Passed,
            UnityExitCode = unityProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerExitCode,
            AllSettlementMarkersMatched = unityProof.Passed && unityProof.MissingMarkers.Count == 0,
            PreviewExportPayloadPassed = preview.Passed,
            InvalidMatrixPassed = invalid.Passed,
            RowCount = matrix.RowCount,
            StateChangingRowCount = matrix.StateChangingRowCount,
            FamilyCount = matrix.FamilyCount,
            SeedCount = matrix.SeedCount,
            SourceManifestHash = Hash(Serialize(sourceManifest)),
            BuildingCatalogHash = Hash(Serialize(catalog)),
            RowMatrixHash = Hash(Serialize(matrix)),
            ProductionLedgerHash = Hash(Serialize(production)),
            DestructionRepairLedgerHash = Hash(Serialize(destructionRepair)),
            DefenseThreatLedgerHash = Hash(Serialize(defenseThreat)),
            LivingWorldLinkageHash = Hash(Serialize(livingWorld)),
            SaveLoadReplayProofHash = Hash(Serialize(replay)),
            UnityCommandPlanHash = Hash(Serialize(unityCommandPlan)),
            UnityProofSummaryHash = Hash(Serialize(unityProof)),
            PreviewExportPayloadHash = Hash(Serialize(preview)),
            InvalidMatrixHash = Hash(Serialize(invalid)),
            Diagnostics = diagnostics
        };
    }

    private static string RenderReport(
        SettlementReport report,
        SettlementSourceManifest sourceManifest,
        SettlementRowMatrix matrix,
        SettlementLedger production,
        SettlementLedger destructionRepair,
        SettlementLedger defenseThreat,
        SettlementLivingWorldLinkageMatrix livingWorld,
        SettlementSaveLoadReplayProof replay,
        SettlementUnityProofSummary unityProof,
        InvalidSettlementDiagnosticsMatrix invalid)
    {
        var lines = new List<string>
        {
            "# Settlement Construction Destruction Production Matrix Report",
            string.Empty,
            "settlement_construction_destruction_production_matrix_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            $"manualGate={SettlementConstructionDestructionProductionVocabulary.FinalGate}",
            $"goal065AcceptedByUserHandoff={report.Goal065AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"rowCount={report.RowCount}",
            $"familyCount={report.FamilyCount}",
            $"seedCount={report.SeedCount}",
            $"stateChangingRowCount={report.StateChangingRowCount}",
            $"productionLedgerPassed={report.ProductionLedgerPassed.ToString().ToLowerInvariant()}",
            $"destructionRepairLedgerPassed={report.DestructionRepairLedgerPassed.ToString().ToLowerInvariant()}",
            $"defenseThreatLedgerPassed={report.DefenseThreatLedgerPassed.ToString().ToLowerInvariant()}",
            $"livingWorldLinkagePassed={report.LivingWorldLinkagePassed.ToString().ToLowerInvariant()}",
            $"saveLoadReplayPassed={report.SaveLoadReplayPassed.ToString().ToLowerInvariant()}",
            $"meaningfulVariancePassed={report.MeaningfulVariancePassed.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allSettlementMarkersMatched={report.AllSettlementMarkersMatched.ToString().ToLowerInvariant()}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"buildingCatalogHash={report.BuildingCatalogHash}",
            $"rowMatrixHash={report.RowMatrixHash}",
            $"productionLedgerHash={report.ProductionLedgerHash}",
            $"destructionRepairLedgerHash={report.DestructionRepairLedgerHash}",
            $"defenseThreatLedgerHash={report.DefenseThreatLedgerHash}",
            $"livingWorldLinkageHash={report.LivingWorldLinkageHash}",
            $"saveLoadReplayProofHash={report.SaveLoadReplayProofHash}",
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
        lines.Add("## Row Matrix");
        lines.Add(string.Empty);
        foreach (var row in matrix.Rows)
        {
            lines.Add($"- {row.RowId}: family={row.FamilyId}, seed={row.SeedId}, settlement={row.SettlementId}, building={row.BuildingId}, kind={row.BuildingKind}, before={row.BeforeState.StateHash}, after={row.AfterState.StateHash}, rowHash={row.RowHash}");
            lines.Add($"  - construction: {row.ConstructionAction.ActionKind}, costs={string.Join(",", row.ConstructionCostLedger.Select(item => item.ResourceId + item.Delta))}");
            lines.Add($"  - production: {row.ProductionAction.ActionKind}, outputs={string.Join(",", row.ProductionOutputLedger.Select(item => item.ResourceId + "+" + item.Delta))}");
            lines.Add($"  - damage: {row.DamageDestructionThreatEvent.ActionKind}, repairDefense={row.RepairUpgradeDefenseResponse.ActionKind}");
            lines.Add($"  - livingWorld: actors={row.LivingWorldConsequence.ActorIds.Count}, factions={row.LivingWorldConsequence.FactionIds.Count}, events={row.LivingWorldConsequence.EventIds.Count}");
            lines.Add($"  - interlocked: deltaCount={row.InterlockedGameplayDependency.DeltaIds.Count}, afterHash={row.InterlockedGameplayDependency.AfterStateHash}");
        }

        lines.Add(string.Empty);
        lines.Add("## Ledgers");
        lines.Add(string.Empty);
        lines.Add($"- production: passed={production.Passed.ToString().ToLowerInvariant()}, entries={production.EntryCount}");
        lines.Add($"- destructionRepair: passed={destructionRepair.Passed.ToString().ToLowerInvariant()}, entries={destructionRepair.EntryCount}");
        lines.Add($"- defenseThreat: passed={defenseThreat.Passed.ToString().ToLowerInvariant()}, entries={defenseThreat.EntryCount}");
        lines.Add($"- livingWorldLinkage: passed={livingWorld.Passed.ToString().ToLowerInvariant()}, entries={livingWorld.LinkageCount}");
        lines.Add(string.Empty);
        lines.Add("## Save/load/replay");
        lines.Add(string.Empty);
        lines.AddRange(replay.Rows.Select(item => $"- {item.RowId}: changed={item.BeforeAfterStateChanged.ToString().ToLowerInvariant()}, saveLoad={item.SaveLoadRoundtripPassed.ToString().ToLowerInvariant()}, replay={item.ReplayDeterminismPassed.ToString().ToLowerInvariant()}, hash={item.FirstReplayHash}"));
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
        lines.Add("No public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution/project file change, new dependency, provider/LLM/RAG/media generation call, or arbitrary Lua execution is part of this Goal 066 proof. Unity changes are limited to deterministic settlement marker loading in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("settlement_construction_destruction_production_matrix_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            schemaVersion = "goal066_artifact_scope_report_v1",
            scenario = SettlementConstructionDestructionProductionVocabulary.ProductSmokeRoute,
            gate = SettlementConstructionDestructionProductionVocabulary.FinalGate + " required",
            allowedArtifactRoot = SettlementConstructionDestructionProductionVocabulary.RelativeOutputDirectory + "/",
            allowedCodeRoot = "src/LLMGameCreator.Application/Design/SettlementConstructionDestructionProductionMatrix/",
            allowedTestsRoot = "tests/LLMGameCreator.Tests/Application/SettlementConstructionDestructionProductionMatrix/",
            allowedProductSmoke = "tests/LLMGameCreator.Tests/ProductSmoke/SettlementConstructionDestructionProductionMatrixProductSmokeTests.cs",
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
                "arbitrary Lua execution"
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
        await WriteTextWithRetry(path, text.TrimEnd('\r', '\n') + Environment.NewLine, cancellationToken).ConfigureAwait(false);
        written.Add(path);
    }

    private static async Task WriteTextWithRetry(string path, string text, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 120; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (File.Exists(path) && string.Equals(await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false), text, StringComparison.Ordinal))
                {
                    return;
                }

                await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                if (attempt == 120)
                {
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task WriteBytes(string path, byte[] bytes, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 120; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (File.Exists(path) && File.ReadAllBytes(path).SequenceEqual(bytes))
                {
                    return;
                }

                await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                if (attempt == 120)
                {
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static string TextOrNone(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "(none)" : value;

    private static string Serialize<T>(T value) => SettlementConstructionDestructionProductionHash.Serialize(value);

    private static string Hash(string text) => SettlementConstructionDestructionProductionHash.Hash(text);

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
