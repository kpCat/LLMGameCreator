using System.Text;

namespace LLMGameCreator.Application.Design.GameplayConsequenceDepthMatrix;

public sealed class GameplayConsequenceDepthMatrixEvidenceService
{
    public const string SourceManifestJsonFileName = "source-manifest.json";
    public const string CatalogJsonFileName = "gameplay-consequence-catalog.json";
    public const string CommandPlanMatrixJsonFileName = "gameplay-command-plan-matrix.json";
    public const string RuntimeStateDeltaMatrixJsonFileName = "runtime-state-delta-matrix.json";
    public const string SaveLoadReplayAuditJsonFileName = "save-load-replay-audit.json";
    public const string FamilyConsequenceSummaryJsonFileName = "family-consequence-summary.json";
    public const string UnityCommandPlanJsonFileName = "unity-command-plan.json";
    public const string UnityProofSummaryJsonFileName = "unity-player-proof-summary.json";
    public const string PreviewExportGameplayPayloadJsonFileName = "preview-export-gameplay-payload.json";
    public const string InvalidDiagnosticsMatrixJsonFileName = "invalid-diagnostics-matrix.json";
    public const string ArtifactScopeReportJsonFileName = "artifact-scope-report.json";
    public const string ReportMarkdownFileName = "gameplay-consequence-depth-matrix-report.md";
    public const string RowsDirectoryName = "rows";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public GameplayConsequenceDepthMatrixBuildResult Build(string projectRootPath, GameplayConsequenceUnityProof? unityProof = null)
    {
        var source = new GameplayConsequenceDepthMatrixSourceLoader().Load(projectRootPath);
        var sourceManifest = BuildSourceManifest(source);
        var catalog = BuildCatalog();
        var commandPlan = BuildCommandPlan(source);
        var rowProofs = commandPlan.Rows.Select(GameplayConsequenceDepthMatrixProjector.Project).ToList();
        var replayProofs = commandPlan.Rows.Select(GameplayConsequenceDepthMatrixProjector.Project).ToDictionary(item => item.RowId, StringComparer.Ordinal);
        var runtimeMatrix = BuildRuntimeStateMatrix(rowProofs);
        var replayAudit = BuildReplayAudit(rowProofs, replayProofs);
        var familySummary = BuildFamilySummary(rowProofs);
        var unityCommandPlan = BuildUnityCommandPlan(commandPlan);
        var proof = unityProof ?? GameplayConsequenceUnityProofRunner.NotRequested(unityCommandPlan);
        var previewPayload = BuildPreviewPayload(rowProofs);
        var invalidMatrix = new GameplayConsequenceDepthMatrixValidator().BuildInvalidMatrix();
        var diagnostics = BuildDiagnostics(sourceManifest, catalog, commandPlan, runtimeMatrix, replayAudit, familySummary, unityCommandPlan, proof.PlayerProof, previewPayload, invalidMatrix);
        var reportWithoutHash = BuildReport(
            sourceManifest,
            catalog,
            commandPlan,
            runtimeMatrix,
            replayAudit,
            familySummary,
            unityCommandPlan,
            proof.PlayerProof,
            previewPayload,
            invalidMatrix,
            diagnostics);
        var report = reportWithoutHash with
        {
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new GameplayConsequenceDepthMatrixBuildResult
        {
            SourceManifest = sourceManifest,
            Catalog = catalog,
            CommandPlanMatrix = commandPlan,
            RuntimeStateDeltaMatrix = runtimeMatrix,
            SaveLoadReplayAudit = replayAudit,
            FamilySummary = familySummary,
            UnityCommandPlan = unityCommandPlan,
            UnityProofSummary = proof.PlayerProof,
            PreviewExportPayload = previewPayload,
            InvalidMatrix = invalidMatrix,
            Report = report,
            RowProofs = rowProofs,
            StagingFiles = BuildStagingFiles(source, unityCommandPlan),
            ReportMarkdown = RenderReport(report, sourceManifest, commandPlan, runtimeMatrix, replayAudit, familySummary, proof.PlayerProof, invalidMatrix)
        };
    }

    public async Task<GameplayConsequenceDepthMatrixWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        GameplayConsequenceDepthMatrixOptions options,
        CancellationToken cancellationToken = default)
    {
        var initial = Build(projectRootPath);
        var initialWrite = await WriteAsync(projectRootPath, initial, resetOutput: true, cancellationToken).ConfigureAwait(false);
        if (!options.ExecuteUnityProof)
        {
            return initialWrite;
        }

        var proof = new GameplayConsequenceUnityProofRunner().Run(
            projectRootPath,
            initialWrite.OutputDirectoryPath,
            Path.Combine(initialWrite.OutputDirectoryPath, GameplayConsequenceDepthMatrixVocabulary.StagingRoot),
            initial.UnityCommandPlan,
            options);
        var final = Build(projectRootPath, proof);
        return await WriteAsync(projectRootPath, final, resetOutput: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GameplayConsequenceDepthMatrixWriteResult> WriteAsync(
        string projectRootPath,
        GameplayConsequenceDepthMatrixBuildResult result,
        bool resetOutput = true,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, GameplayConsequenceDepthMatrixVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
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
        await WriteText(outputDirectory, CatalogJsonFileName, Serialize(result.Catalog), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, CommandPlanMatrixJsonFileName, Serialize(result.CommandPlanMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, RuntimeStateDeltaMatrixJsonFileName, Serialize(result.RuntimeStateDeltaMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, SaveLoadReplayAuditJsonFileName, Serialize(result.SaveLoadReplayAudit), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, FamilyConsequenceSummaryJsonFileName, Serialize(result.FamilySummary), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityCommandPlanJsonFileName, Serialize(result.UnityCommandPlan), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, UnityProofSummaryJsonFileName, Serialize(result.UnityProofSummary), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, PreviewExportGameplayPayloadJsonFileName, Serialize(result.PreviewExportPayload), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, InvalidDiagnosticsMatrixJsonFileName, Serialize(result.InvalidMatrix), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ArtifactScopeReportJsonFileName, RenderArtifactScopeReportJson(), written, cancellationToken).ConfigureAwait(false);
        await WriteText(outputDirectory, ReportMarkdownFileName, result.ReportMarkdown, written, cancellationToken).ConfigureAwait(false);

        var rowsDirectory = Path.Combine(outputDirectory, RowsDirectoryName);
        Directory.CreateDirectory(rowsDirectory);
        foreach (var row in result.RowProofs.OrderBy(item => item.RowId, StringComparer.Ordinal))
        {
            await WriteText(rowsDirectory, RowFileName(row), Serialize(row), written, cancellationToken).ConfigureAwait(false);
        }

        foreach (var file in result.StagingFiles.OrderBy(item => item.RelativePath, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, GameplayConsequenceDepthMatrixVocabulary.StagingRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, file.Bytes, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        return new GameplayConsequenceDepthMatrixWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    public static string RowFileName(GameplayConsequenceRowProof row) =>
        row.FamilyId + "-" + row.SeedId + "-gameplay-proof.json";

    private static GameplayConsequenceSourceManifest BuildSourceManifest(GameplayConsequenceSourceBundle source) =>
        new()
        {
            Accepted = false,
            Goal060AcceptedByUserHandoff = source.Goal060AcceptedByUserHandoff,
            Goal061AcceptedByUserHandoff = source.Goal061AcceptedByUserHandoff,
            Goal062AcceptedByUserHandoff = source.Goal062AcceptedByUserHandoff,
            Goal060PackageRowsConsumed = source.Goal060PackageRowsConsumed,
            Goal061ReviewRowsConsumed = source.Goal061ReviewRowsConsumed,
            Goal062SpatialRowsConsumed = source.Goal062SpatialRowsConsumed,
            RowCount = source.Rows.Count,
            FamilyCount = source.FamilyIds.Count,
            SeedCount = source.SeedIds.Count,
            FamilyIds = source.FamilyIds,
            SeedIds = source.SeedIds,
            PreflightGates =
            [
                new GameplayConsequenceGateRecord
                {
                    GateId = "full_campaign_gamepackage_materialization_matrix_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal061 handoff before Goal062"
                },
                new GameplayConsequenceGateRecord
                {
                    GateId = "full_campaign_playable_review_package_rc_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal062 handoff"
                },
                new GameplayConsequenceGateRecord
                {
                    GateId = "constrained_spatial_detail_generation_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "commit e82718bc, 9/9 spatial-detail rows, Unity proof passed, check-all 1077/1077"
                },
                new GameplayConsequenceGateRecord
                {
                    GateId = GameplayConsequenceDepthMatrixVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "current_goal_manual_gate",
                    EvidenceRef = GameplayConsequenceDepthMatrixVocabulary.RelativeOutputDirectory + "/" + ReportMarkdownFileName
                },
                new GameplayConsequenceGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review",
                    ProvenanceKind = "preserved_current_state",
                    EvidenceRef = "Goal 031 remains not passed"
                },
                new GameplayConsequenceGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review",
                    ProvenanceKind = "preserved_current_state",
                    EvidenceRef = "Goal 032 remains not passed"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = source.Diagnostics
        };

    private static GameplayConsequenceCatalog BuildCatalog()
    {
        var families = new List<GameplayConsequenceFamilyTemplate>
        {
            new()
            {
                FamilyId = "map_panel_rpg",
                ConsequenceShape = "travel_npc_quest_reward_reputation",
                RequiredStateAxes = ["location.detail", "quest.progress", "inventory.relic", "reputation.local_faction", "social.trust"],
                RequiredCommandTypes = ["travel/detail", "quest/npc_event", "inventory/reward", "faction/social"],
                ForbiddenClaims = BoundaryClaims()
            },
            new()
            {
                FamilyId = "survival_sandbox",
                ConsequenceShape = "hazard_resource_craft_recover",
                RequiredStateAxes = ["survival.hazard", "survival.stamina", "inventory.resource", "inventory.relic", "progression.unlock"],
                RequiredCommandTypes = ["survival/hazard_pressure", "survival/resource_collect", "survival/craft_mitigation", "survival/recover"],
                ForbiddenClaims = BoundaryClaims()
            },
            new()
            {
                FamilyId = "first_person_grid_dungeon",
                ConsequenceShape = "grid_traversal_blocked_encounter_unlock",
                RequiredStateAxes = ["grid.orientation", "grid.blocked_moves", "encounter.pressure", "progression.unlock", "progression.xp"],
                RequiredCommandTypes = ["grid/traverse", "grid/blocked_move", "encounter/pressure", "progression/unlock"],
                ForbiddenClaims = BoundaryClaims()
            }
        };

        return new GameplayConsequenceCatalog
        {
            Passed = families.Count == 3,
            FamilyTemplateCount = families.Count,
            Families = families
        };
    }

    private static GameplayConsequenceCommandPlanMatrix BuildCommandPlan(GameplayConsequenceSourceBundle source)
    {
        var rows = source.Rows
            .Select(row => new GameplayConsequenceCommandPlanRow
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                SourcePackageRowRef = row.SourcePackageRowRef,
                SourceReviewPackageRowRef = row.SourceReviewPackageRowRef,
                SourceSpatialDetailRowRef = row.SourceSpatialDetailRowRef,
                Commands = BuildCommands(row),
                StateChangingStepCount = BuildCommands(row).Count(item => item.ExpectedStateChanging && item.ExpectedChanges.Count > 0)
            })
            .ToList();

        return new GameplayConsequenceCommandPlanMatrix
        {
            Passed = rows.Count == 9 && rows.All(item => item.StateChangingStepCount >= 3),
            Accepted = false,
            RowCount = rows.Count,
            FamilyCount = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            Rows = rows
        };
    }

    private static IReadOnlyList<GameplayConsequenceCommandStep> BuildCommands(GameplayConsequenceSourceRow row)
    {
        var safeFamily = GameplayConsequenceDepthMatrixHash.SafeSegment(row.FamilyId);
        var safeSeed = GameplayConsequenceDepthMatrixHash.SafeSegment(row.SeedId);
        var seedModifier = SeedModifier(row.SeedId);
        var sourceRefs = new[]
        {
            row.SourcePackageRowRef,
            row.SourceReviewPackageRowRef,
            row.SourceSpatialDetailRowRef
        };

        return row.FamilyId switch
        {
            "map_panel_rpg" =>
            [
                Step(row, "01-travel-detail", "travel/detail", "travel_spatial", "spatial/" + safeFamily + "/" + safeSeed, sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["location.region"] = "region/" + safeFamily + "/quest-hub",
                    ["location.detail"] = "detail/" + safeFamily + "/" + safeSeed + "/objective",
                    ["quest.discovery"] = row.SpatialVarianceMarker
                }),
                Step(row, "02-npc-quest-event", "quest/npc_event", "quest_progress", "quest/" + safeFamily + "/" + safeSeed, sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["quest.progress"] = "1",
                    ["dialogue.seen"] = "guide-" + safeSeed,
                    ["event.row"] = row.RowId
                }),
                Step(row, "03-inventory-reward", "inventory/reward", "reward_item", row.PackageId, sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["inventory.relic"] = "1",
                    ["quest.reward"] = "claimed",
                    ["inventory.reward_source_hash"] = row.PackageHash
                }),
                Step(row, "04-faction-social", "faction/social", "social_reputation", "faction/" + safeFamily, sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["reputation.local_faction"] = (10 + seedModifier).ToString(),
                    ["social.trust"] = (2 + seedModifier).ToString(),
                    ["social.last_consequence"] = "quest_handoff_" + safeSeed
                })
            ],
            "survival_sandbox" =>
            [
                Step(row, "01-hazard-pressure", "survival/hazard_pressure", "hazard_pressure", "hazard/" + safeSeed, sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["survival.hazard"] = (3 + seedModifier).ToString(),
                    ["survival.stamina"] = (8 - seedModifier).ToString(),
                    ["survival.exposure_source"] = row.SpatialVarianceMarker
                }),
                Step(row, "02-resource-collect", "survival/resource_collect", "resource_delta", "resource/" + safeSeed, sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["inventory.resource"] = (3 + seedModifier).ToString(),
                    ["location.detail"] = "detail/" + safeFamily + "/" + safeSeed + "/resource-node",
                    ["survival.last_gathered"] = "resource_bundle_" + safeSeed
                }),
                Step(row, "03-craft-mitigation", "survival/craft_mitigation", "craft_mitigation", "recipe/" + safeSeed, sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["inventory.resource"] = (2 + seedModifier).ToString(),
                    ["inventory.relic"] = "1",
                    ["progression.unlock"] = "shelter_" + safeSeed
                }),
                Step(row, "04-recover", "survival/recover", "recover_state", "camp/" + safeSeed, sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["survival.hazard"] = "1",
                    ["survival.stamina"] = (9 + seedModifier).ToString(),
                    ["survival.status"] = "stabilized_" + safeSeed
                })
            ],
            "first_person_grid_dungeon" =>
            [
                Step(row, "01-grid-traverse", "grid/traverse", "grid_position", "grid/" + safeSeed + "/forward", sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["grid.orientation"] = seedModifier == 1 ? "east" : seedModifier == 2 ? "south" : "west",
                    ["location.detail"] = "detail/" + safeFamily + "/" + safeSeed + "/corridor",
                    ["grid.visited_cells"] = "entry>corridor_" + safeSeed
                }),
                Step(row, "02-blocked-move", "grid/blocked_move", "blocked_movement", "grid/" + safeSeed + "/wall", sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["grid.blocked_moves"] = "1",
                    ["grid.last_blocked_direction"] = seedModifier == 1 ? "west" : seedModifier == 2 ? "north" : "east",
                    ["grid.valid_blocked_distinction"] = "proved"
                }),
                Step(row, "03-encounter-pressure", "encounter/pressure", "encounter_pressure", "encounter/" + safeSeed, sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["encounter.pressure"] = (4 + seedModifier).ToString(),
                    ["survival.stamina"] = (7 - seedModifier).ToString(),
                    ["encounter.last_ability"] = "strike_" + safeSeed
                }),
                Step(row, "04-progression-unlock", "progression/unlock", "loot_unlock", row.PackageId, sourceRefs, new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["inventory.relic"] = "1",
                    ["progression.unlock"] = "door_opened_" + safeSeed,
                    ["progression.xp"] = (5 + seedModifier).ToString()
                })
            ],
            _ => []
        };
    }

    private static GameplayConsequenceCommandStep Step(
        GameplayConsequenceSourceRow row,
        string stepId,
        string commandType,
        string deltaKind,
        string targetRef,
        IReadOnlyList<string> sourceRefs,
        IReadOnlyDictionary<string, string> expectedChanges)
    {
        var commandId = "goal063/" + GameplayConsequenceDepthMatrixHash.SafeSegment(row.FamilyId) + "/" + GameplayConsequenceDepthMatrixHash.SafeSegment(row.SeedId) + "/" + stepId;
        return new GameplayConsequenceCommandStep
        {
            StepId = stepId,
            CommandId = commandId,
            CommandType = commandType,
            DeltaId = commandId + "/delta/" + deltaKind,
            TargetRef = targetRef,
            ConsequenceShape = deltaKind,
            ExpectedStateChanging = true,
            ExpectedChanges = ToSorted(expectedChanges),
            SourceRefs = sourceRefs
        };
    }

    private static SortedDictionary<string, string> ToSorted(IReadOnlyDictionary<string, string> values)
    {
        var sorted = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            sorted[pair.Key] = pair.Value;
        }

        return sorted;
    }

    private static GameplayConsequenceRuntimeStateDeltaMatrix BuildRuntimeStateMatrix(IReadOnlyList<GameplayConsequenceRowProof> rowProofs) =>
        new()
        {
            Passed = rowProofs.Count == 9 && rowProofs.All(item => item.StateTransitionProofPassed && item.StateChangingStepCount >= 3),
            RowCount = rowProofs.Count,
            StateChangingRowCount = rowProofs.Count(item => item.StateTransitionProofPassed && item.StateChangingStepCount >= 3),
            Rows = rowProofs
        };

    private static GameplayConsequenceSaveLoadReplayAudit BuildReplayAudit(
        IReadOnlyList<GameplayConsequenceRowProof> rowProofs,
        IReadOnlyDictionary<string, GameplayConsequenceRowProof> replayProofs)
    {
        var rows = rowProofs
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item =>
            {
                var second = replayProofs[item.RowId];
                GameplayConsequenceDepthMatrixProjector.SerializerRoundtrip(item.AfterState, out var restoredHash);
                return new GameplayConsequenceSaveLoadReplayAuditRow
                {
                    RowId = item.RowId,
                    FamilyId = item.FamilyId,
                    SeedId = item.SeedId,
                    SaveLoadRoundtripPassed = item.SerializerRoundtripPassed && string.Equals(restoredHash, item.AfterState.StateHash, StringComparison.Ordinal),
                    ReplayDeterminismPassed = string.Equals(item.AfterState.StateHash, second.AfterState.StateHash, StringComparison.Ordinal)
                        && string.Equals(item.RowHash, second.RowHash, StringComparison.Ordinal),
                    SerializedAfterStateHash = item.AfterState.StateHash,
                    RestoredAfterStateHash = restoredHash,
                    FirstReplayHash = item.RowHash,
                    SecondReplayHash = second.RowHash
                };
            })
            .ToList();

        return new GameplayConsequenceSaveLoadReplayAudit
        {
            Passed = rows.Count == 9 && rows.All(item => item.SaveLoadRoundtripPassed && item.ReplayDeterminismPassed),
            RowCount = rows.Count,
            SaveLoadPassedRowCount = rows.Count(item => item.SaveLoadRoundtripPassed),
            ReplayPassedRowCount = rows.Count(item => item.ReplayDeterminismPassed),
            Rows = rows
        };
    }

    private static GameplayConsequenceFamilySummary BuildFamilySummary(IReadOnlyList<GameplayConsequenceRowProof> rowProofs)
    {
        var families = rowProofs
            .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
            .OrderBy(group => GameplayConsequenceDepthMatrixVocabulary.FamilyOrderingKey(group.Key), StringComparer.Ordinal)
            .Select(group => new GameplayConsequenceFamilySummaryRow
            {
                FamilyId = group.Key,
                RowCount = group.Count(),
                StateChangingRowCount = group.Count(item => item.StateTransitionProofPassed),
                ConsequenceShapes = group.SelectMany(item => item.Transitions.Select(transition => transition.CommandType)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                MeaningfulVarianceAxes = group.SelectMany(item => item.VarianceContribution.MeaningfulAxes).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                RowHashes = group.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
            })
            .ToList();

        var distinctHashes = rowProofs.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Count();
        return new GameplayConsequenceFamilySummary
        {
            Passed = families.Count == 3 && families.All(item => item.RowCount == 3 && item.StateChangingRowCount == 3) && distinctHashes == 9,
            FamilyCount = families.Count,
            SeedCount = rowProofs.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            MeaningfulVariancePassed = distinctHashes == 9
                && families.All(item => item.MeaningfulVarianceAxes.Count >= 5)
                && families.All(item => item.RowHashes.Count == 3),
            Families = families
        };
    }

    private static GameplayConsequenceUnityCommandPlan BuildUnityCommandPlan(GameplayConsequenceCommandPlanMatrix commandPlan)
    {
        var rows = commandPlan.Rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item =>
            {
                var stepIds = item.Commands.Select(command => command.StepId).Order(StringComparer.Ordinal).ToList();
                var deltaIds = item.Commands.Select(command => command.DeltaId).Order(StringComparer.Ordinal).ToList();
                var markers = new List<string>
                {
                    "gameplay_consequence_row=" + item.FamilyId + "/" + item.SeedId,
                    "gameplay_consequence_completed=" + item.FamilyId + "/" + item.SeedId
                };
                markers.AddRange(stepIds.Select(step => "gameplay_consequence_step=" + step));
                markers.AddRange(deltaIds.Select(delta => "gameplay_consequence_delta=" + delta));
                return new GameplayConsequenceUnityCommandRow
                {
                    RowId = item.RowId,
                    FamilyId = item.FamilyId,
                    SeedId = item.SeedId,
                    StepIds = stepIds,
                    DeltaIds = deltaIds,
                    ExpectedPlayerMarkers = markers.Order(StringComparer.Ordinal).ToList()
                };
            })
            .ToList();

        var expected = new List<string>
        {
            "gameplay_consequence_goal=goal063",
            "gameplay_consequence_matrix_completed=true",
            "gameplay_consequence_depth_matrix_verification=required"
        };
        expected.AddRange(rows.SelectMany(item => item.ExpectedPlayerMarkers));

        return new GameplayConsequenceUnityCommandPlan
        {
            Passed = rows.Count == 9 && rows.All(item => item.StepIds.Count >= 3 && item.DeltaIds.Count >= 3),
            Accepted = false,
            Rows = rows,
            ExpectedPlayerMarkers = expected.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
        };
    }

    private static GameplayConsequencePreviewExportPayload BuildPreviewPayload(IReadOnlyList<GameplayConsequenceRowProof> rows)
    {
        var payloadRows = rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new GameplayConsequencePreviewExportRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                SourcePackageRef = item.SourcePackageRowRef,
                SourceSpatialRef = item.SourceSpatialDetailRowRef,
                GameplayStateHash = item.AfterState.StateHash,
                PreviewMarkers =
                [
                    "gameplay_consequence_row=" + item.FamilyId + "/" + item.SeedId,
                    "state_hash=" + item.AfterState.StateHash,
                    "state_changing_steps=" + item.StateChangingStepCount
                ]
            })
            .ToList();

        return new GameplayConsequencePreviewExportPayload
        {
            Passed = payloadRows.Count == 9 && payloadRows.All(item => !string.IsNullOrWhiteSpace(item.GameplayStateHash)),
            RowCount = payloadRows.Count,
            Rows = payloadRows
        };
    }

    private static IReadOnlyList<GameplayConsequenceFilePayload> BuildStagingFiles(
        GameplayConsequenceSourceBundle source,
        GameplayConsequenceUnityCommandPlan unityCommandPlan)
    {
        var files = source.BaseStagingFiles.ToList();
        files.RemoveAll(item => item.RelativePath == GameplayConsequenceDepthMatrixVocabulary.UnityGameplayCommandPlanStagingRelativePath);
        files.Add(TextFile(GameplayConsequenceDepthMatrixVocabulary.UnityGameplayCommandPlanStagingRelativePath, Serialize(unityCommandPlan)));
        return files
            .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<GameplayConsequenceDiagnostic> BuildDiagnostics(
        GameplayConsequenceSourceManifest sourceManifest,
        GameplayConsequenceCatalog catalog,
        GameplayConsequenceCommandPlanMatrix commandPlan,
        GameplayConsequenceRuntimeStateDeltaMatrix runtimeMatrix,
        GameplayConsequenceSaveLoadReplayAudit replayAudit,
        GameplayConsequenceFamilySummary familySummary,
        GameplayConsequenceUnityCommandPlan unityCommandPlan,
        GameplayConsequenceUnityProofSummary unityProof,
        GameplayConsequencePreviewExportPayload previewPayload,
        InvalidGameplayConsequenceDiagnosticsMatrix invalidMatrix)
    {
        var validator = new GameplayConsequenceDepthMatrixValidator();
        return GameplayConsequenceDepthMatrixValidator.Sort(
            validator.ValidateSourceManifest(sourceManifest)
                .Concat(validator.ValidateCommandPlan(catalog, commandPlan))
                .Concat(validator.ValidateStateProofs(runtimeMatrix, replayAudit, familySummary, previewPayload))
                .Concat(validator.ValidateUnityCommandPlan(unityCommandPlan))
                .Concat(validator.ValidateUnityProof(unityCommandPlan, unityProof))
                .Concat(validator.ValidateInvalidMatrix(invalidMatrix)));
    }

    private static GameplayConsequenceDepthMatrixReport BuildReport(
        GameplayConsequenceSourceManifest sourceManifest,
        GameplayConsequenceCatalog catalog,
        GameplayConsequenceCommandPlanMatrix commandPlan,
        GameplayConsequenceRuntimeStateDeltaMatrix runtimeMatrix,
        GameplayConsequenceSaveLoadReplayAudit replayAudit,
        GameplayConsequenceFamilySummary familySummary,
        GameplayConsequenceUnityCommandPlan unityCommandPlan,
        GameplayConsequenceUnityProofSummary unityProof,
        GameplayConsequencePreviewExportPayload previewPayload,
        InvalidGameplayConsequenceDiagnosticsMatrix invalidMatrix,
        IReadOnlyList<GameplayConsequenceDiagnostic> diagnostics)
    {
        var green = diagnostics.All(item => item.Severity != "error")
            && sourceManifest.Goal062AcceptedByUserHandoff
            && commandPlan.Passed
            && runtimeMatrix.Passed
            && replayAudit.Passed
            && familySummary.MeaningfulVariancePassed
            && unityCommandPlan.Passed
            && unityProof.Passed
            && previewPayload.Passed
            && invalidMatrix.Passed;

        return new GameplayConsequenceDepthMatrixReport
        {
            ImplementationStatus = green ? "GREEN" : "BLOCKED",
            Accepted = false,
            Goal062AcceptedByUserHandoff = sourceManifest.Goal062AcceptedByUserHandoff,
            RowCount = runtimeMatrix.RowCount,
            FamilyCount = commandPlan.FamilyCount,
            SeedCount = commandPlan.SeedCount,
            StateChangingRowCount = runtimeMatrix.StateChangingRowCount,
            SourceFactsConsumed = sourceManifest.Goal060PackageRowsConsumed && sourceManifest.Goal061ReviewRowsConsumed && sourceManifest.Goal062SpatialRowsConsumed,
            CommandPlanPassed = commandPlan.Passed,
            StateDeltaProofPassed = runtimeMatrix.Passed,
            SaveLoadReplayPassed = replayAudit.Passed,
            MeaningfulVariancePassed = familySummary.MeaningfulVariancePassed,
            UnityCommandPlanPassed = unityCommandPlan.Passed,
            UnityProofPassed = unityProof.Passed,
            UnityExitCode = unityProof.UnityExitCode,
            PlayerExitCode = unityProof.PlayerExitCode,
            AllGameplayMarkersMatched = unityProof.Passed && unityProof.MissingMarkers.Count == 0,
            PreviewExportPayloadPassed = previewPayload.Passed,
            InvalidMatrixPassed = invalidMatrix.Passed,
            SourceManifestHash = Hash(Serialize(sourceManifest)),
            CatalogHash = Hash(Serialize(catalog)),
            CommandPlanHash = Hash(Serialize(commandPlan)),
            RuntimeStateDeltaMatrixHash = Hash(Serialize(runtimeMatrix)),
            SaveLoadReplayAuditHash = Hash(Serialize(replayAudit)),
            FamilySummaryHash = Hash(Serialize(familySummary)),
            UnityCommandPlanHash = Hash(Serialize(unityCommandPlan)),
            UnityProofSummaryHash = Hash(Serialize(unityProof)),
            PreviewExportPayloadHash = Hash(Serialize(previewPayload)),
            InvalidMatrixHash = Hash(Serialize(invalidMatrix)),
            Diagnostics = diagnostics
        };
    }

    private static string RenderReport(
        GameplayConsequenceDepthMatrixReport report,
        GameplayConsequenceSourceManifest sourceManifest,
        GameplayConsequenceCommandPlanMatrix commandPlan,
        GameplayConsequenceRuntimeStateDeltaMatrix runtimeMatrix,
        GameplayConsequenceSaveLoadReplayAudit replayAudit,
        GameplayConsequenceFamilySummary familySummary,
        GameplayConsequenceUnityProofSummary unityProof,
        InvalidGameplayConsequenceDiagnosticsMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# Gameplay Consequence Depth Matrix Report",
            string.Empty,
            "gameplay_consequence_depth_matrix_verification required",
            $"implementationStatus={report.ImplementationStatus}",
            "accepted=false",
            "manualGate=gameplay_consequence_depth_matrix_verification",
            $"goal062AcceptedByUserHandoff={report.Goal062AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"rowCount={report.RowCount}",
            $"familyCount={report.FamilyCount}",
            $"seedCount={report.SeedCount}",
            $"stateChangingRowCount={report.StateChangingRowCount}",
            $"saveLoadReplayPassed={report.SaveLoadReplayPassed.ToString().ToLowerInvariant()}",
            $"meaningfulVariancePassed={report.MeaningfulVariancePassed.ToString().ToLowerInvariant()}",
            $"unityExitCode={TextOrNone(report.UnityExitCode?.ToString())}",
            $"playerExitCode={TextOrNone(report.PlayerExitCode?.ToString())}",
            $"allGameplayMarkersMatched={report.AllGameplayMarkersMatched.ToString().ToLowerInvariant()}",
            $"sourceManifestHash={report.SourceManifestHash}",
            $"catalogHash={report.CatalogHash}",
            $"commandPlanHash={report.CommandPlanHash}",
            $"runtimeStateDeltaMatrixHash={report.RuntimeStateDeltaMatrixHash}",
            $"saveLoadReplayAuditHash={report.SaveLoadReplayAuditHash}",
            $"familySummaryHash={report.FamilySummaryHash}",
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
        lines.Add("## Command Plan");
        lines.Add(string.Empty);
        foreach (var row in commandPlan.Rows)
        {
            lines.Add($"- {row.RowId}: family={row.FamilyId}, seed={row.SeedId}, stateChangingSteps={row.StateChangingStepCount}, packageRef={row.SourcePackageRowRef}, reviewRef={row.SourceReviewPackageRowRef}, spatialRef={row.SourceSpatialDetailRowRef}");
            lines.AddRange(row.Commands.Select(command => $"  - {command.StepId}: command={command.CommandType}, delta={command.DeltaId}, changedKeys={string.Join(",", command.ExpectedChanges.Keys.Order(StringComparer.Ordinal))}"));
        }

        lines.Add(string.Empty);
        lines.Add("## Runtime/state Proof");
        lines.Add(string.Empty);
        foreach (var row in runtimeMatrix.Rows)
        {
            lines.Add($"- {row.RowId}: changedSteps={row.StateChangingStepCount}, before={row.BeforeState.StateHash}, after={row.AfterState.StateHash}, serializerRoundtrip={row.SerializerRoundtripPassed.ToString().ToLowerInvariant()}, replay={row.ReplayDeterminismPassed.ToString().ToLowerInvariant()}, variance={row.VarianceContribution.ContributionId}");
        }

        lines.Add(string.Empty);
        lines.Add("## Save/load/replay");
        lines.Add(string.Empty);
        lines.AddRange(replayAudit.Rows.Select(item => $"- {item.RowId}: saveLoad={item.SaveLoadRoundtripPassed.ToString().ToLowerInvariant()}, replay={item.ReplayDeterminismPassed.ToString().ToLowerInvariant()}, hash={item.FirstReplayHash}"));
        lines.Add(string.Empty);
        lines.Add("## Family variance");
        lines.Add(string.Empty);
        lines.AddRange(familySummary.Families.Select(item => $"- {item.FamilyId}: rows={item.RowCount}, stateChangingRows={item.StateChangingRowCount}, axes={string.Join(",", item.MeaningfulVarianceAxes)}, rowHashes={item.RowHashes.Count}"));
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
        lines.Add("No public GamePackage schema change, Runtime/Runtime.Abstractions change, WinForms UI change, Infrastructure provider path change, generator-library change, solution/project file change, new dependency, provider/LLM/RAG/media generation call, or arbitrary Lua execution/source generation is part of this Goal 063 proof. Unity changes are limited to deterministic diagnostic marker loading in AlphaRuntimeBootstrap.");
        lines.Add(string.Empty);
        lines.Add("gameplay_consequence_depth_matrix_verification required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderArtifactScopeReportJson() =>
        Serialize(new
        {
            schemaVersion = "goal063_artifact_scope_report_v1",
            scenario = GameplayConsequenceDepthMatrixVocabulary.ProductSmokeRoute,
            gate = GameplayConsequenceDepthMatrixVocabulary.FinalGate + " required",
            allowedArtifactRoot = GameplayConsequenceDepthMatrixVocabulary.RelativeOutputDirectory + "/",
            allowedCodeRoot = "src/LLMGameCreator.Application/Design/GameplayConsequenceDepthMatrix/",
            allowedTestsRoot = "tests/LLMGameCreator.Tests/Application/GameplayConsequenceDepthMatrix/",
            allowedProductSmoke = "tests/LLMGameCreator.Tests/ProductSmoke/GameplayConsequenceDepthMatrixProductSmokeTests.cs",
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

    private static GameplayConsequenceFilePayload TextFile(string relativePath, string text) =>
        new()
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Bytes = Utf8WithoutBom.GetBytes(text.TrimEnd('\r', '\n') + Environment.NewLine)
        };

    private static IReadOnlyList<string> BoundaryClaims() =>
    [
        "provider_llm_rag_media_generation",
        "runtime_runtime_abstractions_mutation",
        "winforms_ui_mutation",
        "gamepackage_schema_mutation",
        "arbitrary_lua_execution_or_source_generation",
        "new_dependency"
    ];

    private static int SeedModifier(string seedId) =>
        seedId switch
        {
            "seed_alpha" => 1,
            "seed_beta" => 2,
            "seed_gamma" => 3,
            _ => 0
        };

    private static string TextOrNone(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "(none)" : value;

    private static string Serialize<T>(T value) => GameplayConsequenceDepthMatrixHash.Serialize(value);

    private static string Hash(string text) => GameplayConsequenceDepthMatrixHash.Hash(text);

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
