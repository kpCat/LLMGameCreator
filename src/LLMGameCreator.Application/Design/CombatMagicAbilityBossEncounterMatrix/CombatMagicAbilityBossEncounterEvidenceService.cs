using System.Text;

namespace LLMGameCreator.Application.Design.CombatMagicAbilityBossEncounterMatrix;

public sealed class CombatMagicAbilityBossEncounterEvidenceService
{
    public const string SourceManifestJsonFileName = "combat-magic-source-manifest.json";
    public const string AbilityTraitCatalogJsonFileName = "ability-trait-catalog.json";
    public const string StatusEffectCatalogJsonFileName = "status-effect-catalog.json";
    public const string BossEncounterPhaseCatalogJsonFileName = "boss-encounter-phase-catalog.json";
    public const string RowMatrixJsonFileName = "combat-magic-row-matrix.json";
    public const string SaveLoadReplayProofJsonFileName = "combat-magic-save-load-replay-proof.json";
    public const string ProgressionLootLedgerJsonFileName = "combat-magic-progression-loot-ledger.json";
    public const string CounterplayLedgerJsonFileName = "combat-magic-counterplay-ledger.json";
    public const string PreviewExportPayloadJsonFileName = "combat-magic-preview-export-payload.json";
    public const string UnityCommandPlanJsonFileName = "combat-magic-unity-command-plan.json";
    public const string UnityProofSummaryJsonFileName = "combat-magic-unity-player-proof-summary.json";
    public const string InvalidDiagnosticsMatrixJsonFileName = "combat-magic-invalid-diagnostics-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "combat-magic-ability-boss-encounter-matrix-report.md";
    public const string RowsDirectoryName = "rows";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public CombatMagicBuildResult Build(string projectRootPath, CombatMagicUnityProof? unityProof = null)
    {
        var source = new CombatMagicAbilityBossEncounterSourceLoader().Load(projectRootPath);
        var projector = new CombatMagicAbilityBossEncounterProjector();
        var sourceManifest = projector.BuildSourceManifest(source);
        var abilityCatalog = projector.BuildAbilityTraitCatalog();
        var statusCatalog = projector.BuildStatusEffectCatalog();
        var bossCatalog = projector.BuildBossPhaseCatalog();
        var rows = projector.BuildRows(source);
        var matrix = projector.BuildRowMatrix(rows);
        var replay = projector.BuildSaveLoadReplayProof(rows);
        var progressionLoot = projector.BuildProgressionLootLedger(rows);
        var counterplay = projector.BuildCounterplayLedger(rows);
        var preview = projector.BuildPreviewExportPayload(rows);
        var unityCommandPlan = projector.BuildUnityCommandPlan(rows);
        var proof = unityProof ?? CombatMagicAbilityBossEncounterUnityProofRunner.NotRequested(unityCommandPlan);
        var invalid = projector.BuildInvalidMatrix();
        var diagnostics = BuildDiagnostics(sourceManifest, abilityCatalog, statusCatalog, bossCatalog, matrix, replay, progressionLoot, counterplay, preview, unityCommandPlan, proof.PlayerProof, invalid);
        var reportWithoutHash = BuildReport(sourceManifest, abilityCatalog, statusCatalog, bossCatalog, matrix, replay, progressionLoot, counterplay, preview, unityCommandPlan, proof.PlayerProof, invalid, diagnostics);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new CombatMagicBuildResult
        {
            SourceManifest = sourceManifest,
            AbilityTraitCatalog = abilityCatalog,
            StatusEffectCatalog = statusCatalog,
            BossPhaseCatalog = bossCatalog,
            RowMatrix = matrix,
            SaveLoadReplayProof = replay,
            ProgressionLootLedger = progressionLoot,
            CounterplayLedger = counterplay,
            PreviewExportPayload = preview,
            UnityCommandPlan = unityCommandPlan,
            UnityProofSummary = proof.PlayerProof,
            InvalidMatrix = invalid,
            Report = report,
            Rows = rows,
            StagingFiles = projector.BuildStagingFiles(source, unityCommandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, matrix, replay, progressionLoot, counterplay, proof.PlayerProof, invalid)
        };
    }

    public async Task<CombatMagicWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CombatMagicAbilityBossEncounterOptions options,
        CancellationToken cancellationToken = default)
    {
        var initial = Build(projectRootPath);
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutput: true, cancellationToken).ConfigureAwait(false);
        if (!options.ExecuteUnityProof)
        {
            return initialWrite;
        }

        var proof = new CombatMagicAbilityBossEncounterUnityProofRunner().Run(
            projectRootPath,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            options);
        var final = Build(projectRootPath, proof);
        return await WriteAsync(projectRootPath, final, resetOutput: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CombatMagicWriteResult> WriteAsync(
        string projectRootPath,
        CombatMagicBuildResult result,
        bool resetOutput = true,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, CombatMagicAbilityBossEncounterVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
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
        await WriteText(outputDirectory, AbilityTraitCatalogJsonFileName, Serialize(result.AbilityTraitCatalog), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, StatusEffectCatalogJsonFileName, Serialize(result.StatusEffectCatalog), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, BossEncounterPhaseCatalogJsonFileName, Serialize(result.BossPhaseCatalog), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, RowMatrixJsonFileName, Serialize(result.RowMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SaveLoadReplayProofJsonFileName, Serialize(result.SaveLoadReplayProof), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ProgressionLootLedgerJsonFileName, Serialize(result.ProgressionLootLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, CounterplayLedgerJsonFileName, Serialize(result.CounterplayLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, PreviewExportPayloadJsonFileName, Serialize(result.PreviewExportPayload), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityCommandPlanJsonFileName, Serialize(result.UnityCommandPlan), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityProofSummaryJsonFileName, Serialize(result.UnityProofSummary), written, cancellationToken).ConfigureAwait(false);
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
            var path = Path.Combine(outputDirectory, CombatMagicAbilityBossEncounterVocabulary.StagingRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await WriteBytes(path, file.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        return new CombatMagicWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, CombatMagicAbilityBossEncounterVocabulary.StagingRoot),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            WrittenFiles = written.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    public static string RowFileName(CombatMagicRow row) =>
        row.FamilyId.Replace('_', '-') + "-" + row.SeedId.Replace('_', '-') + "-combat-magic-row.json";

    private static IReadOnlyList<CombatMagicDiagnostic> BuildDiagnostics(
        CombatMagicSourceManifest sourceManifest,
        CombatMagicAbilityTraitCatalog abilityCatalog,
        CombatMagicStatusEffectCatalog statusCatalog,
        CombatMagicBossEncounterPhaseCatalog bossCatalog,
        CombatMagicRowMatrix matrix,
        CombatMagicSaveLoadReplayProof replay,
        CombatMagicLedger progressionLoot,
        CombatMagicLedger counterplay,
        CombatMagicPreviewExportPayload preview,
        CombatMagicUnityCommandPlan unityCommandPlan,
        CombatMagicUnityProofSummary unityProof,
        InvalidCombatMagicDiagnosticsMatrix invalid)
    {
        var validator = new CombatMagicAbilityBossEncounterValidator();
        return CombatMagicAbilityBossEncounterValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateCatalogs(abilityCatalog, statusCatalog, bossCatalog))
                .Concat(validator.ValidateRows(abilityCatalog, statusCatalog, bossCatalog, matrix, preview))
                .Concat(validator.ValidateLedgers(progressionLoot, counterplay))
                .Concat(validator.ValidateReplay(replay))
                .Concat(validator.ValidateUnityCommandPlan(unityCommandPlan))
                .Concat(validator.ValidateUnityProof(unityCommandPlan, unityProof))
                .Concat(validator.ValidateInvalidMatrix(invalid)));
    }

    private static CombatMagicReport BuildReport(
        CombatMagicSourceManifest sourceManifest,
        CombatMagicAbilityTraitCatalog abilityCatalog,
        CombatMagicStatusEffectCatalog statusCatalog,
        CombatMagicBossEncounterPhaseCatalog bossCatalog,
        CombatMagicRowMatrix matrix,
        CombatMagicSaveLoadReplayProof replay,
        CombatMagicLedger progressionLoot,
        CombatMagicLedger counterplay,
        CombatMagicPreviewExportPayload preview,
        CombatMagicUnityCommandPlan unityCommandPlan,
        CombatMagicUnityProofSummary unityProof,
        InvalidCombatMagicDiagnosticsMatrix invalid,
        IReadOnlyList<CombatMagicDiagnostic> diagnostics)
    {
        var noErrors = diagnostics.All(item => item.Severity != "error");
        var sourceConsumed = sourceManifest.Goal060PackageRowsConsumed
            && sourceManifest.Goal061ReviewPackageRcConsumed
            && sourceManifest.Goal062SpatialRowsConsumed
            && sourceManifest.Goal063GameplayRowsConsumed
            && sourceManifest.Goal064LivingWorldRowsConsumed
            && sourceManifest.Goal065InterlockedRowsConsumed
            && sourceManifest.Goal066SettlementRowsConsumed
            && sourceManifest.Goal067NarrativeRowsConsumed;
        var noProseLeak = matrix.Rows.All(item => item.NoFinalProse)
            && diagnostics.All(item => item.Code != "goal068.prose.final_leakage");
        var meaningfulVariancePassed = matrix.SameFamilySeedVariancePassed && matrix.FamilyCombatFlavorVariancePassed;
        var green = noErrors
            && sourceManifest.Goal067AcceptedByUserHandoff
            && sourceConsumed
            && abilityCatalog.Passed
            && statusCatalog.Passed
            && bossCatalog.Passed
            && matrix.Passed
            && progressionLoot.Passed
            && counterplay.Passed
            && replay.Passed
            && meaningfulVariancePassed
            && unityCommandPlan.Passed
            && unityProof.Passed
            && preview.Passed
            && invalid.Passed
            && noProseLeak;
        var failed = diagnostics.Any(item => item.Severity == "error" && !item.Code.StartsWith("goal068.unity.", StringComparison.Ordinal));

        return new CombatMagicReport
        {
            ImplementationStatus = green ? "GREEN" : failed ? "FAILED" : "BLOCKED",
            Accepted = false,
            Goal067AcceptedByUserHandoff = sourceManifest.Goal067AcceptedByUserHandoff,
            SourceFactsConsumed = sourceConsumed,
            AbilityTraitCatalogPassed = abilityCatalog.Passed,
            StatusEffectCatalogPassed = statusCatalog.Passed,
            BossPhaseCatalogPassed = bossCatalog.Passed,
            RowMatrixPassed = matrix.Passed,
            ProgressionLootLedgerPassed = progressionLoot.Passed,
            CounterplayLedgerPassed = counterplay.Passed,
            SaveLoadReplayPassed = replay.Passed,
            MeaningfulVariancePassed = meaningfulVariancePassed,
            UnityCommandPlanPassed = unityCommandPlan.Passed,
            UnityProofPassed = unityProof.Passed,
            UnityExitCode = unityProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerExitCode,
            AllCombatMagicMarkersMatched = unityProof.Passed && unityProof.MissingMarkers.Count == 0,
            PreviewExportPayloadPassed = preview.Passed,
            InvalidMatrixPassed = invalid.Passed,
            NoFinalProseLeakage = noProseLeak,
            RowCount = matrix.RowCount,
            StateChangingRowCount = matrix.StateChangingRowCount,
            BossEliteRowCount = matrix.BossEliteRowCount,
            MagicStatusRowCount = matrix.MagicStatusRowCount,
            ResourceGearCraftingRowCount = matrix.ResourceGearCraftingRowCount,
            FamilyCount = matrix.FamilyCount,
            SeedCount = matrix.SeedCount,
            SourceManifestHash = Hash(Serialize(sourceManifest)),
            AbilityTraitCatalogHash = Hash(Serialize(abilityCatalog)),
            StatusEffectCatalogHash = Hash(Serialize(statusCatalog)),
            BossPhaseCatalogHash = Hash(Serialize(bossCatalog)),
            RowMatrixHash = Hash(Serialize(matrix)),
            ProgressionLootLedgerHash = Hash(Serialize(progressionLoot)),
            CounterplayLedgerHash = Hash(Serialize(counterplay)),
            SaveLoadReplayProofHash = Hash(Serialize(replay)),
            UnityCommandPlanHash = Hash(Serialize(unityCommandPlan)),
            UnityProofSummaryHash = Hash(Serialize(unityProof)),
            PreviewExportPayloadHash = Hash(Serialize(preview)),
            InvalidMatrixHash = Hash(Serialize(invalid)),
            Diagnostics = diagnostics
        };
    }

    private static string RenderReport(
        CombatMagicReport report,
        CombatMagicSourceManifest sourceManifest,
        CombatMagicRowMatrix matrix,
        CombatMagicSaveLoadReplayProof replay,
        CombatMagicLedger progressionLoot,
        CombatMagicLedger counterplay,
        CombatMagicUnityProofSummary unityProof,
        InvalidCombatMagicDiagnosticsMatrix invalid)
    {
        var lines = new List<string>
        {
            "# Combat Magic Ability Boss Encounter Matrix Report",
            string.Empty,
            "combat_magic_ability_boss_encounter_matrix_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            $"manualGate={CombatMagicAbilityBossEncounterVocabulary.FinalGate}",
            $"goal067AcceptedByUserHandoff={report.Goal067AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"rowCount={report.RowCount}",
            $"familyCount={report.FamilyCount}",
            $"seedCount={report.SeedCount}",
            $"stateChangingRowCount={report.StateChangingRowCount}",
            $"bossEliteRowCount={report.BossEliteRowCount}",
            $"magicStatusRowCount={report.MagicStatusRowCount}",
            $"resourceGearCraftingRowCount={report.ResourceGearCraftingRowCount}",
            $"progressionLootLedgerPassed={report.ProgressionLootLedgerPassed.ToString().ToLowerInvariant()}",
            $"counterplayLedgerPassed={report.CounterplayLedgerPassed.ToString().ToLowerInvariant()}",
            $"saveLoadReplayPassed={report.SaveLoadReplayPassed.ToString().ToLowerInvariant()}",
            $"meaningfulVariancePassed={report.MeaningfulVariancePassed.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allCombatMagicMarkersMatched={report.AllCombatMagicMarkersMatched.ToString().ToLowerInvariant()}",
            $"noFinalProseLeakage={report.NoFinalProseLeakage.ToString().ToLowerInvariant()}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"abilityTraitCatalogHash={report.AbilityTraitCatalogHash}",
            $"statusEffectCatalogHash={report.StatusEffectCatalogHash}",
            $"bossPhaseCatalogHash={report.BossPhaseCatalogHash}",
            $"rowMatrixHash={report.RowMatrixHash}",
            $"progressionLootLedgerHash={report.ProgressionLootLedgerHash}",
            $"counterplayLedgerHash={report.CounterplayLedgerHash}",
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
            lines.Add($"- {row.RowId}: family={row.FamilyId}, seed={row.SeedId}, encounter={row.EncounterKind}, before={row.BeforeState.StateHash}, after={row.AfterState.StateHash}, rowHash={row.RowHash}");
            lines.Add($"  - sourceRefs: package={row.SourcePackageRowRef}, spatial={row.SourceSpatialDetailRowRef}, interlocked={row.SourceInterlockedGameplayRowRef}, settlement={row.SourceSettlementRowRef}, narrative={row.SourceNarrativeRowRef}");
            lines.Add($"  - abilities={row.ActiveAbilities.Count}, statuses={row.StatusEffects.Count}, rounds={row.RoundPhaseResults.Count}, changedCategories={string.Join(",", row.ChangedCategories)}");
            lines.Add($"  - bossOrElite={row.BossOrElitePhaseRow.ToString().ToLowerInvariant()}, magicStatus={row.MagicStatusHeavyRow.ToString().ToLowerInvariant()}, resourceGearCrafting={row.ResourceGearCraftingLinkedRow.ToString().ToLowerInvariant()}");
        }

        lines.Add(string.Empty);
        lines.Add("## Ledgers");
        lines.Add(string.Empty);
        lines.Add($"- progressionLoot: passed={progressionLoot.Passed.ToString().ToLowerInvariant()}, entries={progressionLoot.EntryCount}");
        lines.Add($"- counterplay: passed={counterplay.Passed.ToString().ToLowerInvariant()}, entries={counterplay.EntryCount}");
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
        lines.AddRange(invalid.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).OrderBy(code => code, StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution/project file change, new dependency, provider/LLM/RAG call, final prose generation, arbitrary Lua execution, generated Lua source, or broad Unity gameplay implementation is part of this Goal 068 proof. Unity changes are limited to deterministic combat/magic marker loading in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("combat_magic_ability_boss_encounter_matrix_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            schemaVersion = "goal068_artifact_scope_report_v1",
            scenario = CombatMagicAbilityBossEncounterVocabulary.ProductSmokeRoute,
            gate = CombatMagicAbilityBossEncounterVocabulary.FinalGate + " required",
            allowedArtifactRoot = CombatMagicAbilityBossEncounterVocabulary.RelativeOutputDirectory + "/",
            allowedCodeRoot = "src/LLMGameCreator.Application/Design/CombatMagicAbilityBossEncounterMatrix/",
            allowedTestsRoot = "tests/LLMGameCreator.Tests/Application/CombatMagicAbilityBossEncounterMatrix/",
            allowedProductSmoke = "tests/LLMGameCreator.Tests/ProductSmoke/CombatMagicAbilityBossEncounterMatrixProductSmokeTests.cs",
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
                "final prose generation",
                "arbitrary Lua execution",
                "generated Lua source",
                "broad Unity gameplay implementation"
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

    private static string Serialize<T>(T value) => CombatMagicAbilityBossEncounterHash.Serialize(value);

    private static string Hash(string text) => CombatMagicAbilityBossEncounterHash.Hash(text);

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
