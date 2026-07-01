using System.Text;

namespace LLMGameCreator.Application.Design.InterlockedGameplaySystemsDepthMatrix;

public sealed class InterlockedGameplaySystemsEvidenceService
{
    public const string SourceManifestJsonFileName = "source-manifest.json";
    public const string RuleCatalogJsonFileName = "system-rule-catalog.json";
    public const string RowPlanMatrixJsonFileName = "row-plan-matrix.json";
    public const string EconomyCraftingLedgerJsonFileName = "economy-crafting-ledger.json";
    public const string CombatProgressionLedgerJsonFileName = "combat-progression-ledger.json";
    public const string StatusEffectLedgerJsonFileName = "status-effect-ledger.json";
    public const string SaveLoadReplayProofJsonFileName = "save-load-replay-proof.json";
    public const string VarianceMetricsJsonFileName = "variance-metrics.json";
    public const string UnityCommandPlanJsonFileName = "unity-command-plan.json";
    public const string UnityProofSummaryJsonFileName = "unity-player-proof.json";
    public const string PreviewExportGameplayPayloadJsonFileName = "preview-export-gameplay-payload.json";
    public const string InvalidDiagnosticsMatrixJsonFileName = "invalid-diagnostics-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "interlocked-gameplay-systems-depth-matrix-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public InterlockedGameplaySystemsBuildResult Build(string projectRootPath, InterlockedUnityProof? unityProof = null)
    {
        var source = new InterlockedGameplaySystemsSourceLoader().Load(projectRootPath);
        var sourceManifest = BuildSourceManifest(source);
        var catalog = InterlockedGameplaySystemsRuleCatalogBuilder.Build();
        var rows = source.Rows
            .Select(InterlockedGameplaySystemsProjector.Project)
            .OrderBy(item => InterlockedGameplaySystemsDepthMatrixVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => InterlockedGameplaySystemsDepthMatrixVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .ToList();
        var matrix = BuildRowPlanMatrix(rows);
        var economyCrafting = BuildLedger("economy_crafting", rows, new HashSet<string>(["economy", "crafting"], StringComparer.Ordinal));
        var combatProgression = BuildLedger("combat_progression_inventory", rows, new HashSet<string>(["combat", "progression", "inventory"], StringComparer.Ordinal));
        var statusEffect = BuildLedger("status_effect", rows, new HashSet<string>(["status", "living_world"], StringComparer.Ordinal));
        var replay = BuildSaveLoadReplayProof(rows);
        var variance = BuildVarianceMetrics(rows);
        var unityCommandPlan = BuildUnityCommandPlan(rows);
        var proof = unityProof ?? InterlockedGameplaySystemsUnityProofRunner.NotRequested(unityCommandPlan);
        var preview = BuildPreviewExportPayload(rows);
        var invalid = new InterlockedGameplaySystemsValidator().BuildInvalidMatrix();
        var diagnostics = BuildDiagnostics(sourceManifest, catalog, matrix, economyCrafting, combatProgression, statusEffect, replay, variance, unityCommandPlan, proof.PlayerProof, preview, invalid);
        var reportWithoutHash = BuildReport(sourceManifest, catalog, matrix, economyCrafting, combatProgression, statusEffect, replay, variance, unityCommandPlan, proof.PlayerProof, preview, invalid, diagnostics);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new InterlockedGameplaySystemsBuildResult
        {
            SourceManifest = sourceManifest,
            RuleCatalog = catalog,
            RowPlanMatrix = matrix,
            EconomyCraftingLedger = economyCrafting,
            CombatProgressionLedger = combatProgression,
            StatusEffectLedger = statusEffect,
            SaveLoadReplayProof = replay,
            VarianceMetrics = variance,
            UnityCommandPlan = unityCommandPlan,
            UnityProofSummary = proof.PlayerProof,
            PreviewExportPayload = preview,
            InvalidMatrix = invalid,
            Report = report,
            Rows = rows,
            StagingFiles = BuildStagingFiles(source, unityCommandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, matrix, economyCrafting, combatProgression, statusEffect, replay, variance, proof.PlayerProof, invalid)
        };
    }

    public async Task<InterlockedGameplaySystemsWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        InterlockedGameplaySystemsOptions options,
        CancellationToken cancellationToken = default)
    {
        var initial = Build(projectRootPath);
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutput: true, cancellationToken).ConfigureAwait(false);
        if (!options.ExecuteUnityProof)
        {
            return initialWrite;
        }

        var proof = new InterlockedGameplaySystemsUnityProofRunner().Run(
            projectRootPath,
            initialWrite.OutputDirectoryPath,
            initialWrite.StagingDirectoryPath,
            initial.UnityCommandPlan,
            options);
        var final = Build(projectRootPath, proof);
        return await WriteAsync(projectRootPath, final, resetOutput: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<InterlockedGameplaySystemsWriteResult> WriteAsync(
        string projectRootPath,
        InterlockedGameplaySystemsBuildResult result,
        bool resetOutput = true,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, InterlockedGameplaySystemsDepthMatrixVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
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
        await WriteText(outputDirectory, RuleCatalogJsonFileName, Serialize(result.RuleCatalog), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, RowPlanMatrixJsonFileName, Serialize(result.RowPlanMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, EconomyCraftingLedgerJsonFileName, Serialize(result.EconomyCraftingLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, CombatProgressionLedgerJsonFileName, Serialize(result.CombatProgressionLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, StatusEffectLedgerJsonFileName, Serialize(result.StatusEffectLedger), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SaveLoadReplayProofJsonFileName, Serialize(result.SaveLoadReplayProof), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, VarianceMetricsJsonFileName, Serialize(result.VarianceMetrics), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityCommandPlanJsonFileName, Serialize(result.UnityCommandPlan), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityProofSummaryJsonFileName, Serialize(result.UnityProofSummary), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, PreviewExportGameplayPayloadJsonFileName, Serialize(result.PreviewExportPayload), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, InvalidDiagnosticsMatrixJsonFileName, Serialize(result.InvalidMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ArtifactScopeReportJsonFileName, RenderArtifactScopeReportJson(), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ReportMarkdownFileName, result.ReportMarkdown, written, cancellationToken).ConfigureAwait(false);

        foreach (var row in result.Rows.OrderBy(item => item.RowId, StringComparer.Ordinal))
        {
            await WriteText(outputDirectory, RowFileName(row), Serialize(row), written, cancellationToken).ConfigureAwait(false);
        }

        foreach (var file in result.StagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, InterlockedGameplaySystemsDepthMatrixVocabulary.StagingRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, file.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        return new InterlockedGameplaySystemsWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, InterlockedGameplaySystemsDepthMatrixVocabulary.StagingRoot),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    public static string RowFileName(InterlockedGameplayRow row) =>
        "row-" + row.FamilyId.Replace('_', '-') + "-" + row.SeedId.Replace('_', '-') + ".json";

    private static InterlockedGameplaySourceManifest BuildSourceManifest(InterlockedGameplaySourceBundle source)
    {
        var diagnostics = new List<InterlockedGameplayDiagnostic>(source.Diagnostics)
        {
            InterlockedGameplayDiagnostic.Info("goal065.preflight.goal064_handoff_recorded", "living_world_npc_faction_simulation_matrix_verification", "Goal 064 is recorded as accepted by user handoff before Goal 065."),
            InterlockedGameplayDiagnostic.Info("goal065.source.loaded", "Goal060-064", "Goal 065 source facts were loaded from repository-local Goal 060/061/062/063/064 compact evidence.")
        };

        return new InterlockedGameplaySourceManifest
        {
            Accepted = false,
            Goal064AcceptedByUserHandoff = source.Goal064AcceptedByUserHandoff,
            Goal060PackageRowsConsumed = source.Goal060PackageRowsConsumed,
            Goal061ReviewRowsConsumed = source.Goal061ReviewRowsConsumed,
            Goal062SpatialRowsConsumed = source.Goal062SpatialRowsConsumed,
            Goal063GameplayRowsConsumed = source.Goal063GameplayRowsConsumed,
            Goal064LivingWorldRowsConsumed = source.Goal064LivingWorldRowsConsumed,
            Goal064UnityProofConsumed = source.Goal064UnityProofConsumed,
            RowCount = source.Rows.Count,
            FamilyCount = source.FamilyIds.Count,
            SeedCount = source.SeedIds.Count,
            FamilyIds = source.FamilyIds,
            SeedIds = source.SeedIds,
            PreflightGates =
            [
                new InterlockedGameplayGateRecord
                {
                    GateId = "full_campaign_gamepackage_materialization_matrix_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 061 handoff before Goal 062"
                },
                new InterlockedGameplayGateRecord
                {
                    GateId = "full_campaign_playable_review_package_rc_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 062 handoff before Goal 063"
                },
                new InterlockedGameplayGateRecord
                {
                    GateId = "constrained_spatial_detail_generation_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 063 handoff"
                },
                new InterlockedGameplayGateRecord
                {
                    GateId = "gameplay_consequence_depth_matrix_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 064 handoff"
                },
                new InterlockedGameplayGateRecord
                {
                    GateId = "living_world_npc_faction_simulation_matrix_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 065 preflight handoff"
                },
                new InterlockedGameplayGateRecord
                {
                    GateId = InterlockedGameplaySystemsDepthMatrixVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "current_goal_manual_gate",
                    EvidenceRef = InterlockedGameplaySystemsDepthMatrixVocabulary.RelativeOutputDirectory + "/" + ReportMarkdownFileName
                },
                new InterlockedGameplayGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "preserved_current_state",
                    EvidenceRef = "Goal 031 remains not passed"
                },
                new InterlockedGameplayGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "preserved_current_state",
                    EvidenceRef = "Goal 032 remains not passed"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = InterlockedGameplaySystemsSourceLoader.SortDiagnostics(diagnostics)
        };
    }

    private static InterlockedGameplayRowPlanMatrix BuildRowPlanMatrix(IReadOnlyList<InterlockedGameplayRow> rows)
    {
        var distinctHashes = rows.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Count();
        return new InterlockedGameplayRowPlanMatrix
        {
            Passed = rows.Count == 9
                && rows.All(item => item.StateChanging)
                && distinctHashes == 9,
            Accepted = false,
            RowCount = rows.Count,
            FamilyCount = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            StateChangingRowCount = rows.Count(item => item.StateChanging),
            DistinctRowHashCount = distinctHashes,
            Rows = rows
        };
    }

    private static InterlockedGameplayLedger BuildLedger(
        string ledgerKind,
        IReadOnlyList<InterlockedGameplayRow> rows,
        IReadOnlySet<string> categories)
    {
        var entries = rows
            .SelectMany(row => InterlockedGameplaySystemsProjector.BuildLedgerEntries(row, categories))
            .OrderBy(item => item.EntryId, StringComparer.Ordinal)
            .ToList();

        return new InterlockedGameplayLedger
        {
            LedgerKind = ledgerKind,
            Passed = rows.Count == 9
                && categories.All(category => entries.Count(item => item.Category == category) >= 9)
                && entries.All(item => item.SourceRefs.Count > 0 && !string.IsNullOrWhiteSpace(item.Outcome)),
            EntryCount = entries.Count,
            Entries = entries
        };
    }

    private static InterlockedSaveLoadReplayProof BuildSaveLoadReplayProof(IReadOnlyList<InterlockedGameplayRow> rows)
    {
        var proofRows = rows.Select(item => item.SaveLoadReplayProof).OrderBy(item => item.RowId, StringComparer.Ordinal).ToList();
        return new InterlockedSaveLoadReplayProof
        {
            Passed = proofRows.Count == 9
                && proofRows.All(item => item.BeforeAfterStateChanged && item.SaveLoadRoundtripPassed && item.ReplayDeterminismPassed),
            RowCount = proofRows.Count,
            StateChangedRowCount = proofRows.Count(item => item.BeforeAfterStateChanged),
            SaveLoadPassedRowCount = proofRows.Count(item => item.SaveLoadRoundtripPassed),
            ReplayPassedRowCount = proofRows.Count(item => item.ReplayDeterminismPassed),
            Rows = proofRows
        };
    }

    private static InterlockedVarianceMetrics BuildVarianceMetrics(IReadOnlyList<InterlockedGameplayRow> rows)
    {
        var families = rows
            .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
            .OrderBy(group => InterlockedGameplaySystemsDepthMatrixVocabulary.FamilyOrderingKey(group.Key), StringComparer.Ordinal)
            .Select(group =>
            {
                var familyRows = group.OrderBy(item => InterlockedGameplaySystemsDepthMatrixVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal).ToList();
                var highlights = familyRows
                    .Select(row => Hash(Serialize(row.MeaningfulVarianceAxes.Where(row.AfterState.Values.ContainsKey).ToDictionary(key => key, key => row.AfterState.Values[key], StringComparer.Ordinal))))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                return new InterlockedFamilyVarianceSummary
                {
                    FamilyId = group.Key,
                    RowCount = familyRows.Count,
                    SameFamilySeedVariationPassed = familyRows.Count == 3 && highlights.Count == 3,
                    RuleSetIds = familyRows.Select(item => item.DerivedRuleSetId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                    MeaningfulAxes = familyRows.SelectMany(item => item.MeaningfulVarianceAxes).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                    RowHashes = familyRows.Select(item => item.RowHash).Order(StringComparer.Ordinal).ToList()
                };
            })
            .ToList();

        var distinctAfterHashes = rows.Select(item => item.AfterState.StateHash).Distinct(StringComparer.Ordinal).Count();
        var distinctRuleSets = rows.Select(item => item.DerivedRuleSetId).Distinct(StringComparer.Ordinal).Count();
        return new InterlockedVarianceMetrics
        {
            Passed = rows.Count == 9
                && distinctAfterHashes == 9
                && distinctRuleSets == 3
                && families.Count == 3
                && families.All(item => item.SameFamilySeedVariationPassed && item.MeaningfulAxes.Count >= 7),
            HashOnlyVarianceRejected = rows.All(item => item.MeaningfulVarianceAxes.Count >= 7)
                && rows.SelectMany(item => item.Deltas.Select(delta => delta.Key)).Distinct(StringComparer.Ordinal).Count() >= 7,
            SameFamilySeedVariationPassed = families.Count == 3 && families.All(item => item.SameFamilySeedVariationPassed),
            CrossFamilyRuleVariationPassed = distinctRuleSets == 3,
            DistinctAfterStateHashCount = distinctAfterHashes,
            DistinctRuleSetCount = distinctRuleSets,
            Families = families
        };
    }

    private static InterlockedUnityCommandPlan BuildUnityCommandPlan(IReadOnlyList<InterlockedGameplayRow> rows)
    {
        var commandRows = rows
            .OrderBy(item => InterlockedGameplaySystemsDepthMatrixVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => InterlockedGameplaySystemsDepthMatrixVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(row =>
            {
                var economy = DeltaIds(row, "economy");
                var crafting = DeltaIds(row, "crafting");
                var combat = DeltaIds(row, "combat");
                var progression = DeltaIds(row, "progression");
                var status = DeltaIds(row, "status");
                return new InterlockedUnityCommandRow
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    EconomyDeltaIds = economy,
                    CraftingDeltaIds = crafting,
                    CombatDeltaIds = combat,
                    ProgressionDeltaIds = progression,
                    StatusDeltaIds = status,
                    ExpectedPlayerMarkers = row.ExpectedUnityMarkerSet.Order(StringComparer.Ordinal).ToList()
                };
            })
            .ToList();

        var expected = new List<string>
        {
            "interlocked_gameplay_loaded=true",
            "interlocked_gameplay_completed=true",
            "review_package_proof=goal065",
            "interlocked_gameplay_systems_depth_matrix_verification=required"
        };
        expected.AddRange(commandRows.SelectMany(item => item.ExpectedPlayerMarkers));

        return new InterlockedUnityCommandPlan
        {
            Passed = commandRows.Count == 9
                && commandRows.All(item => item.EconomyDeltaIds.Count > 0 && item.CraftingDeltaIds.Count > 0 && item.CombatDeltaIds.Count > 0 && item.ProgressionDeltaIds.Count > 0 && item.StatusDeltaIds.Count > 0),
            Accepted = false,
            Rows = commandRows,
            ExpectedPlayerMarkers = expected.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
        };
    }

    private static InterlockedPreviewExportPayload BuildPreviewExportPayload(IReadOnlyList<InterlockedGameplayRow> rows)
    {
        var payloadRows = rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new InterlockedPreviewExportRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                SourcePackageRef = item.SourcePackageRowRef,
                SourceGameplayRef = item.SourceGameplayConsequenceRowRef,
                SourceLivingWorldRef = item.SourceLivingWorldRowRef,
                InterlockedAfterStateHash = item.AfterState.StateHash,
                PreviewMarkers =
                [
                    "interlocked_gameplay_row=" + item.RowId,
                    "interlocked_state_hash=" + item.AfterState.StateHash,
                    "interlocked_delta_count=" + item.Deltas.Count,
                    "interlocked_rule_set=" + item.DerivedRuleSetId
                ]
            })
            .ToList();

        return new InterlockedPreviewExportPayload
        {
            Passed = payloadRows.Count == 9 && payloadRows.All(item => !string.IsNullOrWhiteSpace(item.InterlockedAfterStateHash)),
            RowCount = payloadRows.Count,
            Rows = payloadRows
        };
    }

    private static IReadOnlyList<InterlockedGameplayFilePayload> BuildStagingFiles(
        InterlockedGameplaySourceBundle source,
        InterlockedUnityCommandPlan unityCommandPlan)
    {
        var files = source.BaseStagingFiles.ToList();
        files.RemoveAll(item => item.RelativePath == InterlockedGameplaySystemsDepthMatrixVocabulary.UnityInterlockedCommandPlanStagingRelativePath);
        files.Add(TextFile(InterlockedGameplaySystemsDepthMatrixVocabulary.UnityInterlockedCommandPlanStagingRelativePath, Serialize(unityCommandPlan)));
        return files
            .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<InterlockedGameplayDiagnostic> BuildDiagnostics(
        InterlockedGameplaySourceManifest sourceManifest,
        InterlockedGameplayRuleCatalog catalog,
        InterlockedGameplayRowPlanMatrix matrix,
        InterlockedGameplayLedger economyCrafting,
        InterlockedGameplayLedger combatProgression,
        InterlockedGameplayLedger statusEffect,
        InterlockedSaveLoadReplayProof replay,
        InterlockedVarianceMetrics variance,
        InterlockedUnityCommandPlan unityCommandPlan,
        InterlockedUnityProofSummary unityProof,
        InterlockedPreviewExportPayload preview,
        InvalidInterlockedGameplayDiagnosticsMatrix invalid)
    {
        var validator = new InterlockedGameplaySystemsValidator();
        return InterlockedGameplaySystemsValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateRows(catalog, matrix, preview))
                .Concat(validator.ValidateLedgers(economyCrafting, combatProgression, statusEffect))
                .Concat(validator.ValidateReplayAndVariance(replay, variance))
                .Concat(validator.ValidateUnityCommandPlan(unityCommandPlan))
                .Concat(validator.ValidateUnityProof(unityCommandPlan, unityProof))
                .Concat(validator.ValidateInvalidMatrix(invalid)));
    }

    private static InterlockedGameplaySystemsReport BuildReport(
        InterlockedGameplaySourceManifest sourceManifest,
        InterlockedGameplayRuleCatalog catalog,
        InterlockedGameplayRowPlanMatrix matrix,
        InterlockedGameplayLedger economyCrafting,
        InterlockedGameplayLedger combatProgression,
        InterlockedGameplayLedger statusEffect,
        InterlockedSaveLoadReplayProof replay,
        InterlockedVarianceMetrics variance,
        InterlockedUnityCommandPlan unityCommandPlan,
        InterlockedUnityProofSummary unityProof,
        InterlockedPreviewExportPayload preview,
        InvalidInterlockedGameplayDiagnosticsMatrix invalid,
        IReadOnlyList<InterlockedGameplayDiagnostic> diagnostics)
    {
        var noErrors = diagnostics.All(item => item.Severity != "error");
        var sourceConsumed = sourceManifest.Goal060PackageRowsConsumed
            && sourceManifest.Goal061ReviewRowsConsumed
            && sourceManifest.Goal062SpatialRowsConsumed
            && sourceManifest.Goal063GameplayRowsConsumed
            && sourceManifest.Goal064LivingWorldRowsConsumed;
        var green = noErrors
            && sourceManifest.Goal064AcceptedByUserHandoff
            && sourceConsumed
            && catalog.Passed
            && matrix.Passed
            && economyCrafting.Passed
            && combatProgression.Passed
            && statusEffect.Passed
            && replay.Passed
            && variance.Passed
            && unityCommandPlan.Passed
            && unityProof.Passed
            && preview.Passed
            && invalid.Passed;
        var failed = diagnostics.Any(item => item.Severity == "error" && !item.Code.StartsWith("goal065.unity.", StringComparison.Ordinal));

        return new InterlockedGameplaySystemsReport
        {
            ImplementationStatus = green ? "GREEN" : failed ? "FAILED" : "BLOCKED",
            Accepted = false,
            Goal064AcceptedByUserHandoff = sourceManifest.Goal064AcceptedByUserHandoff,
            RowCount = matrix.RowCount,
            FamilyCount = matrix.FamilyCount,
            SeedCount = matrix.SeedCount,
            StateChangingRowCount = matrix.StateChangingRowCount,
            SourceFactsConsumed = sourceConsumed,
            RuleCatalogPassed = catalog.Passed,
            RowPlanPassed = matrix.Passed,
            EconomyCraftingLedgerPassed = economyCrafting.Passed,
            CombatProgressionLedgerPassed = combatProgression.Passed,
            StatusEffectLedgerPassed = statusEffect.Passed,
            SaveLoadReplayPassed = replay.Passed,
            MeaningfulVariancePassed = variance.Passed,
            UnityCommandPlanPassed = unityCommandPlan.Passed,
            UnityProofPassed = unityProof.Passed,
            UnityExitCode = unityProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerExitCode,
            AllInterlockedMarkersMatched = unityProof.Passed && unityProof.MissingMarkers.Count == 0,
            PreviewExportPayloadPassed = preview.Passed,
            InvalidMatrixPassed = invalid.Passed,
            SourceManifestHash = Hash(Serialize(sourceManifest)),
            RuleCatalogHash = Hash(Serialize(catalog)),
            RowPlanMatrixHash = Hash(Serialize(matrix)),
            EconomyCraftingLedgerHash = Hash(Serialize(economyCrafting)),
            CombatProgressionLedgerHash = Hash(Serialize(combatProgression)),
            StatusEffectLedgerHash = Hash(Serialize(statusEffect)),
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
        InterlockedGameplaySystemsReport report,
        InterlockedGameplaySourceManifest sourceManifest,
        InterlockedGameplayRowPlanMatrix matrix,
        InterlockedGameplayLedger economyCrafting,
        InterlockedGameplayLedger combatProgression,
        InterlockedGameplayLedger statusEffect,
        InterlockedSaveLoadReplayProof replay,
        InterlockedVarianceMetrics variance,
        InterlockedUnityProofSummary unityProof,
        InvalidInterlockedGameplayDiagnosticsMatrix invalid)
    {
        var lines = new List<string>
        {
            "# Interlocked Gameplay Systems Depth Matrix Report",
            string.Empty,
            "interlocked_gameplay_systems_depth_matrix_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            $"manualGate={InterlockedGameplaySystemsDepthMatrixVocabulary.FinalGate}",
            $"goal064AcceptedByUserHandoff={report.Goal064AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"rowCount={report.RowCount}",
            $"familyCount={report.FamilyCount}",
            $"seedCount={report.SeedCount}",
            $"stateChangingRowCount={report.StateChangingRowCount}",
            $"saveLoadReplayPassed={report.SaveLoadReplayPassed.ToString().ToLowerInvariant()}",
            $"meaningfulVariancePassed={report.MeaningfulVariancePassed.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allInterlockedMarkersMatched={report.AllInterlockedMarkersMatched.ToString().ToLowerInvariant()}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"ruleCatalogHash={report.RuleCatalogHash}",
            $"rowPlanMatrixHash={report.RowPlanMatrixHash}",
            $"economyCraftingLedgerHash={report.EconomyCraftingLedgerHash}",
            $"combatProgressionLedgerHash={report.CombatProgressionLedgerHash}",
            $"statusEffectLedgerHash={report.StatusEffectLedgerHash}",
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
        lines.Add("## Row Matrix");
        lines.Add(string.Empty);
        foreach (var row in matrix.Rows)
        {
            lines.Add($"- {row.RowId}: family={row.FamilyId}, seed={row.SeedId}, rule={row.DerivedRuleSetId}, deltas={row.Deltas.Count}, before={row.BeforeState.StateHash}, after={row.AfterState.StateHash}, rowHash={row.RowHash}");
            lines.AddRange(row.Deltas.Select(delta => $"  - {delta.DeltaId}: category={delta.Category}, subsystem={delta.Subsystem}, key={delta.Key}, before={delta.BeforeValue}, after={delta.AfterValue}, outcome={delta.Outcome}"));
        }

        lines.Add(string.Empty);
        lines.Add("## Ledgers");
        lines.Add(string.Empty);
        lines.Add($"- economyCrafting: passed={economyCrafting.Passed.ToString().ToLowerInvariant()}, entries={economyCrafting.EntryCount}");
        lines.Add($"- combatProgression: passed={combatProgression.Passed.ToString().ToLowerInvariant()}, entries={combatProgression.EntryCount}");
        lines.Add($"- statusEffect: passed={statusEffect.Passed.ToString().ToLowerInvariant()}, entries={statusEffect.EntryCount}");
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
        lines.Add("No public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution/project file change, new dependency, provider/LLM/RAG/media generation call, or arbitrary Lua execution is part of this Goal 065 proof. Unity changes are limited to deterministic diagnostic marker loading in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("interlocked_gameplay_systems_depth_matrix_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            schemaVersion = "goal065_artifact_scope_report_v1",
            scenario = InterlockedGameplaySystemsDepthMatrixVocabulary.ProductSmokeRoute,
            gate = InterlockedGameplaySystemsDepthMatrixVocabulary.FinalGate + " required",
            allowedArtifactRoot = InterlockedGameplaySystemsDepthMatrixVocabulary.RelativeOutputDirectory + "/",
            allowedCodeRoot = "src/LLMGameCreator.Application/Design/InterlockedGameplaySystemsDepthMatrix/",
            allowedTestsRoot = "tests/LLMGameCreator.Tests/Application/InterlockedGameplaySystemsDepthMatrix/",
            allowedProductSmoke = "tests/LLMGameCreator.Tests/ProductSmoke/InterlockedGameplaySystemsDepthMatrixProductSmokeTests.cs",
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

    private static IReadOnlyList<string> DeltaIds(InterlockedGameplayRow row, string category) =>
        row.Deltas
            .Where(item => item.Category == category)
            .Select(item => item.DeltaId)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

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

    private static InterlockedGameplayFilePayload TextFile(string relativePath, string text) =>
        new()
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Bytes = Utf8WithoutBom.GetBytes(text.TrimEnd('\r', '\n') + Environment.NewLine)
        };

    private static string TextOrNone(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "(none)" : value;

    private static string Serialize<T>(T value) => InterlockedGameplaySystemsHash.Serialize(value);

    private static string Hash(string text) => InterlockedGameplaySystemsHash.Hash(text);

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
