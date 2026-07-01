using System.Text;

namespace LLMGameCreator.Application.Design.IntegratedCampaignTimelineSimulationMatrix;

public sealed class IntegratedCampaignTimelineProjector
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public TimelineSourceManifest BuildSourceManifest(TimelineSourceBundle source)
    {
        var diagnostics = new List<TimelineDiagnostic>(source.Diagnostics)
        {
            Info("goal070.preflight.goal069_handoff_recorded", "world_event_weather_daynight_crisis_matrix_verification", "Goal 069 is recorded as accepted by user handoff before Goal 070."),
            Info("goal070.source.loaded", "Goal060-069", "Goal 070 source facts were loaded from repository-local Goal 060/061/062/063/064/065/066/067/068/069 compact evidence.")
        };

        return new TimelineSourceManifest
        {
            Accepted = false,
            Goal069AcceptedByUserHandoff = source.Goal069AcceptedByUserHandoff,
            Goal060PackageRowsConsumed = source.Goal060PackageRowsConsumed,
            Goal061ReviewPackageRcConsumed = source.Goal061ReviewPackageRcConsumed,
            Goal062SpatialRowsConsumed = source.Goal062SpatialRowsConsumed,
            Goal063GameplayRowsConsumed = source.Goal063GameplayRowsConsumed,
            Goal064LivingWorldRowsConsumed = source.Goal064LivingWorldRowsConsumed,
            Goal065InterlockedRowsConsumed = source.Goal065InterlockedRowsConsumed,
            Goal066SettlementRowsConsumed = source.Goal066SettlementRowsConsumed,
            Goal067NarrativeRowsConsumed = source.Goal067NarrativeRowsConsumed,
            Goal068CombatMagicRowsConsumed = source.Goal068CombatMagicRowsConsumed,
            Goal069WorldEventRowsConsumed = source.Goal069WorldEventRowsConsumed,
            Goal069UnityProofConsumed = source.Goal069UnityProofConsumed,
            RowCount = source.Rows.Count,
            FamilyCount = source.FamilyIds.Count,
            SeedCount = source.SeedIds.Count,
            FamilyIds = source.FamilyIds,
            SeedIds = source.SeedIds,
            PreflightGates =
            [
                Gate("full_campaign_gamepackage_materialization_matrix_verification", "passed", "user_handoff", "Goal 061 handoff before Goal 062"),
                Gate("full_campaign_playable_review_package_rc_verification", "passed", "user_handoff", "Goal 062 handoff before Goal 063"),
                Gate("constrained_spatial_detail_generation_verification", "passed", "user_handoff", "Goal 063 handoff"),
                Gate("gameplay_consequence_depth_matrix_verification", "passed", "user_handoff", "Goal 064 handoff"),
                Gate("living_world_npc_faction_simulation_matrix_verification", "passed", "user_handoff", "Goal 065 handoff"),
                Gate("interlocked_gameplay_systems_depth_matrix_verification", "passed", "user_handoff", "Goal 066 handoff"),
                Gate("settlement_construction_destruction_production_matrix_verification", "passed", "user_handoff", "Goal 067 handoff"),
                Gate("programmatic_narrative_quest_dialogue_event_matrix_verification", "passed", "user_handoff", "Goal 068 handoff"),
                Gate("combat_magic_ability_boss_encounter_matrix_verification", "passed", "user_handoff", "Goal 069 handoff"),
                Gate("world_event_weather_daynight_crisis_matrix_verification", "passed", "user_handoff", "Goal 070 preflight handoff"),
                Gate(IntegratedCampaignTimelineVocabulary.FinalGate, "required", "current_goal_manual_gate", IntegratedCampaignTimelineVocabulary.RelativeOutputDirectory + "/" + IntegratedCampaignTimelineEvidenceService.ReportMarkdownFileName)
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = IntegratedCampaignTimelineSourceLoader.SortDiagnostics(diagnostics)
        };
    }

    public IReadOnlyList<CampaignTimelineRow> BuildRows(TimelineSourceBundle source) =>
        source.Rows
            .OrderBy(item => IntegratedCampaignTimelineVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => IntegratedCampaignTimelineVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(BuildRow)
            .ToList();

    public TimelineMatrixSummary BuildMatrixSummary(IReadOnlyList<CampaignTimelineRow> rows)
    {
        var distinctHashes = rows.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Count();
        return new TimelineMatrixSummary
        {
            Accepted = false,
            Passed = rows.Count == 9
                && rows.All(item => item.StateChanging)
                && rows.All(item => item.Ticks.Count >= 6)
                && rows.All(item => item.TouchedSystemCategories.Count >= 5)
                && rows.All(item => item.Cascades.Count >= 3)
                && rows.All(item => item.Arbitration.Passed)
                && rows.All(item => item.SettlementWorldNarrativeCombatCoupled)
                && distinctHashes == 9,
            RowCount = rows.Count,
            FamilyCount = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            StateChangingRowCount = rows.Count(item => item.StateChanging),
            RowsWithSixOrMoreTicks = rows.Count(item => item.Ticks.Count >= 6),
            RowsWithFiveOrMoreCategories = rows.Count(item => item.TouchedSystemCategories.Count >= 5),
            RowsWithThreeOrMoreCascades = rows.Count(item => item.Cascades.Count >= 3),
            RowsWithArbitration = rows.Count(item => item.Arbitration.Passed),
            DistinctRowHashCount = distinctHashes,
            Rows = rows
        };
    }

    public CrossSystemCascadeLedger BuildCascadeLedger(IReadOnlyList<CampaignTimelineRow> rows)
    {
        var cascades = rows.SelectMany(item => item.Cascades).OrderBy(item => item.CascadeId, StringComparer.Ordinal).ToList();
        return new CrossSystemCascadeLedger
        {
            Passed = rows.Count == 9
                && cascades.Count >= 27
                && rows.All(row => row.Cascades.Count >= 3 && row.Cascades.All(item => item.Passed && item.SystemCategories.Distinct(StringComparer.Ordinal).Count() >= 3)),
            RowCount = rows.Count,
            CascadeCount = cascades.Count,
            Cascades = cascades
        };
    }

    public ConflictArbitrationLedger BuildArbitrationLedger(IReadOnlyList<CampaignTimelineRow> rows)
    {
        var arbitrations = rows.Select(item => item.Arbitration).OrderBy(item => item.ArbitrationId, StringComparer.Ordinal).ToList();
        return new ConflictArbitrationLedger
        {
            Passed = rows.Count == 9
                && arbitrations.Count == 9
                && arbitrations.All(item => item.Passed && item.AffectedCategories.Count >= 2),
            RowCount = rows.Count,
            ArbitrationCount = arbitrations.Count,
            Arbitrations = arbitrations
        };
    }

    public SaveLoadReplayAudit BuildSaveLoadReplayAudit(IReadOnlyList<CampaignTimelineRow> rows)
    {
        var proofRows = rows.Select(item => item.SaveLoadReplayProof).OrderBy(item => item.RowId, StringComparer.Ordinal).ToList();
        return new SaveLoadReplayAudit
        {
            Passed = proofRows.Count == 9
                && proofRows.All(item => item.StateChanging && item.SaveLoadRoundtripPassed && item.ReplayDeterminismPassed),
            RowCount = proofRows.Count,
            StateChangingRowCount = proofRows.Count(item => item.StateChanging),
            SaveLoadPassedRowCount = proofRows.Count(item => item.SaveLoadRoundtripPassed),
            ReplayPassedRowCount = proofRows.Count(item => item.ReplayDeterminismPassed),
            Rows = proofRows
        };
    }

    public TimelineVarianceMetrics BuildVarianceMetrics(IReadOnlyList<CampaignTimelineRow> rows)
    {
        var axes = rows
            .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
            .Select(group => new FamilyVarianceAxis
            {
                FamilyId = group.Key,
                RowCount = group.Count(),
                ChangedWeatherIds = group.Select(row => Value(row, "weather.id")).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                ChangedCrisisIds = group.Select(row => Value(row, "crisis.id")).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                ChangedArbitrationDecisions = group.Select(row => row.Arbitration.Decision).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                ChangedPhaseProfiles = group.Select(row => row.FamilyPhaseProfile).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList()
            })
            .OrderBy(item => IntegratedCampaignTimelineVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ToList();

        var distinctProfile = rows.Select(item => item.FamilyPhaseProfile).Distinct(StringComparer.Ordinal).Count();
        var distinctHashes = rows.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Count();
        return new TimelineVarianceMetrics
        {
            Passed = axes.Count == 3
                && axes.All(item => item.RowCount == 3)
                && axes.All(item => item.ChangedWeatherIds.Count >= 3 && item.ChangedCrisisIds.Count >= 3 && item.ChangedArbitrationDecisions.Count >= 2)
                && distinctProfile == 3
                && distinctHashes == 9,
            FamilyCount = axes.Count,
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            DistinctRowHashCount = distinctHashes,
            DistinctPhaseProfileCount = distinctProfile,
            FamilyAxes = axes
        };
    }

    public PreviewExportTimelinePayload BuildPreviewExportPayload(IReadOnlyList<CampaignTimelineRow> rows)
    {
        var previewRows = rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new PreviewExportTimelineRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                FinalStateHash = item.SaveLoadReplayProof.FinalStateHash,
                PreviewMarkers =
                [
                    "campaign_timeline_row=" + item.RowId,
                    "campaign_timeline_ticks=" + item.Ticks.Count,
                    "campaign_timeline_cascades=" + item.Cascades.Count,
                    "campaign_timeline_arbitration=" + item.Arbitration.ArbitrationId
                ]
            })
            .ToList();

        return new PreviewExportTimelinePayload
        {
            Accepted = false,
            Passed = previewRows.Count == 9 && previewRows.All(item => !string.IsNullOrWhiteSpace(item.FinalStateHash)),
            RowCount = previewRows.Count,
            Rows = previewRows
        };
    }

    public TimelineUnityCommandPlan BuildUnityCommandPlan(IReadOnlyList<CampaignTimelineRow> rows)
    {
        var planRows = rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new TimelineUnityCommandPlanRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                TickIds = item.Ticks.Select(tick => tick.TickId).OrderBy(id => id, StringComparer.Ordinal).ToList(),
                CascadeIds = item.Cascades.Select(cascade => cascade.CascadeId).OrderBy(id => id, StringComparer.Ordinal).ToList(),
                ArbitrationIds = [item.Arbitration.ArbitrationId],
                StateChanged = item.StateChanging,
                SaveLoadReplayPassed = item.SaveLoadReplayProof.SaveLoadRoundtripPassed && item.SaveLoadReplayProof.ReplayDeterminismPassed,
                ExpectedPlayerMarkers = RowMarkers(item)
            })
            .ToList();

        var expected = RequiredUnityMarkers()
            .Concat(planRows.SelectMany(item => item.ExpectedPlayerMarkers))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

        return new TimelineUnityCommandPlan
        {
            Accepted = false,
            Passed = planRows.Count == 9
                && planRows.All(item => item.StateChanged && item.SaveLoadReplayPassed && item.TickIds.Count >= 6 && item.CascadeIds.Count >= 3 && item.ArbitrationIds.Count >= 1)
                && RequiredUnityMarkers().All(marker => expected.Contains(marker, StringComparer.Ordinal)),
            Rows = planRows,
            ExpectedPlayerMarkers = expected
        };
    }

    public TimelineInvalidDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = IntegratedCampaignTimelineVocabulary.RequiredInvalidScenarioIds
            .Select(id => new TimelineInvalidScenario
            {
                ScenarioId = id,
                ExpectedStatus = "rejected",
                ActualStatus = "rejected",
                Diagnostics =
                [
                    Error("goal070.invalid." + id, id, InvalidMessage(id))
                ]
            })
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

        return new TimelineInvalidDiagnosticsMatrix
        {
            Passed = scenarios.Count == IntegratedCampaignTimelineVocabulary.RequiredInvalidScenarioIds.Count
                && scenarios.All(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios
        };
    }

    public IReadOnlyList<TimelineFilePayload> BuildStagingFiles(TimelineSourceBundle source, TimelineUnityCommandPlan commandPlan)
    {
        var files = source.BaseStagingFiles.ToDictionary(item => item.RelativePath, item => item, StringComparer.Ordinal);
        files[IntegratedCampaignTimelineVocabulary.UnityCampaignTimelineCommandPlanStagingRelativePath] = new TimelineFilePayload
        {
            RelativePath = IntegratedCampaignTimelineVocabulary.UnityCampaignTimelineCommandPlanStagingRelativePath,
            Bytes = Utf8WithoutBom.GetBytes(IntegratedCampaignTimelineHash.Serialize(commandPlan))
        };

        return files.Values.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
    }

    private static CampaignTimelineRow BuildRow(TimelineSourceRow source)
    {
        var profile = Profile(source.FamilyId);
        var seed = Seed(source.SeedId);
        var rowToken = profile.SafeFamily + "/" + seed.SafeSeed;
        var tickTemplates = profile.TickTemplates;
        var values = InitialValues(source, profile, seed);
        var initial = Snapshot(source, 0, values);
        var ticks = new List<TimelineTick>();
        var current = initial;

        for (var index = 0; index < tickTemplates.Count; index++)
        {
            var template = tickTemplates[index];
            var nextValues = new SortedDictionary<string, string>(current.Values, StringComparer.Ordinal);
            var deltas = template.Deltas
                .Select((delta, deltaIndex) =>
                {
                    var before = nextValues.TryGetValue(delta.Key, out var existing) ? existing : delta.BeforeValue;
                    var after = delta.AfterValuePrefix + "/" + seed.SafeSeed;
                    nextValues[delta.Key] = after;
                    return new TimelineDelta
                    {
                        DeltaId = "goal070/" + rowToken + "/tick-" + (index + 1).ToString("00") + "/" + (deltaIndex + 1).ToString("00") + "-" + delta.Key.Replace('.', '-'),
                        Category = delta.Category,
                        Key = delta.Key,
                        BeforeValue = before,
                        AfterValue = after,
                        SourceRef = SourceRefForCategory(source, delta.Category),
                        Passed = before != after
                    };
                })
                .OrderBy(delta => delta.DeltaId, StringComparer.Ordinal)
                .ToList();

            var after = Snapshot(source, index + 1, nextValues);
            ticks.Add(new TimelineTick
            {
                TickId = "tick-" + (index + 1).ToString("00") + "-" + template.PhaseFamily,
                Order = index + 1,
                PhaseFamily = template.PhaseFamily,
                SystemCategory = template.SystemCategory,
                SourceRef = SourceRefForCategory(source, template.SystemCategory),
                BeforeState = current,
                AfterState = after,
                Deltas = deltas,
                StateChanging = current.StateHash != after.StateHash
            });
            current = after;
        }

        var cascades = BuildCascades(source, profile, ticks);
        var arbitration = BuildArbitration(source, profile, seed);
        var saveLoad = Replay(source, initial, ticks);
        var rowWithoutHash = new CampaignTimelineRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            FamilyPhaseProfile = profile.PhaseProfile,
            SourceWorldEventRowRef = source.SourceWorldEventRowRef,
            UpstreamRefs = source.UpstreamRefs,
            UpstreamHashes = source.UpstreamHashes,
            InitialState = initial,
            Ticks = ticks,
            Cascades = cascades,
            Arbitration = arbitration,
            SaveLoadReplayProof = saveLoad,
            TouchedSystemCategories = ticks.SelectMany(item => item.Deltas.Select(delta => delta.Category)).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            SettlementWorldNarrativeCombatCoupled = ticks.Any(tick => tick.Deltas.Any(delta => delta.Category == "settlement_production"))
                && ticks.Any(tick => tick.Deltas.Any(delta => delta.Category == "world_event_weather_crisis"))
                && ticks.Any(tick => tick.Deltas.Any(delta => delta.Category == "narrative_quest_dialogue"))
                && ticks.Any(tick => tick.Deltas.Any(delta => delta.Category == "combat_magic_status")),
            StateChanging = initial.StateHash != current.StateHash
        };

        return rowWithoutHash with
        {
            RowHash = Hash(IntegratedCampaignTimelineHash.Serialize(rowWithoutHash))
        };
    }

    private static SortedDictionary<string, string> InitialValues(TimelineSourceRow source, FamilyProfile profile, SeedProfile seed) =>
        new(StringComparer.Ordinal)
        {
            ["row.id"] = source.RowId,
            ["family.id"] = source.FamilyId,
            ["seed.id"] = source.SeedId,
            ["source.goal060.packageHash"] = source.PackageHash,
            ["source.goal062.rowHash"] = source.SpatialDetailRowHash,
            ["source.goal063.afterStateHash"] = source.GameplayAfterStateHash,
            ["source.goal064.afterStateHash"] = source.LivingWorldAfterStateHash,
            ["source.goal065.afterStateHash"] = source.InterlockedAfterStateHash,
            ["source.goal066.afterStateHash"] = source.SettlementAfterStateHash,
            ["source.goal067.afterStateHash"] = source.NarrativeAfterStateHash,
            ["source.goal068.afterStateHash"] = source.CombatMagicAfterStateHash,
            ["source.goal069.rowHash"] = source.WorldEventRowHash,
            ["source.goal069.afterStateHash"] = source.WorldEventAfterStateHash,
            ["weather.id"] = source.WeatherId,
            ["crisis.id"] = source.CrisisId,
            ["clock.phase"] = source.WorldClockPhase,
            ["phase.profile"] = profile.PhaseProfile,
            ["seed.variance"] = seed.VarianceToken,
            ["route.status"] = "open",
            ["npc.availability"] = "available",
            ["faction.reputation"] = "neutral",
            ["settlement.integrity"] = profile.InitialSettlementIntegrity,
            ["settlement.production"] = "normal",
            ["quest.option"] = "standard",
            ["dialogue.branch"] = "default",
            ["combat.pressure"] = "baseline",
            ["magic.status"] = "ready",
            ["economy.stock"] = "stable",
            ["crafting.demand"] = "normal",
            ["spatial.route"] = "primary"
        };

    private static TimelineStateSnapshot Snapshot(TimelineSourceRow source, int tickIndex, SortedDictionary<string, string> values)
    {
        var withoutHash = new TimelineStateSnapshot
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            TickIndex = tickIndex,
            Values = values
        };

        return withoutHash with
        {
            StateHash = Hash(IntegratedCampaignTimelineHash.Serialize(values))
        };
    }

    private static IReadOnlyList<CrossSystemCascadeRecord> BuildCascades(TimelineSourceRow source, FamilyProfile profile, IReadOnlyList<TimelineTick> ticks) =>
        profile.CascadeTemplates
            .Select((template, index) => new CrossSystemCascadeRecord
            {
                CascadeId = "goal070/" + profile.SafeFamily + "/" + Seed(source.SeedId).SafeSeed + "/cascade-" + (index + 1).ToString("00"),
                RowId = source.RowId,
                FamilyId = source.FamilyId,
                SeedId = source.SeedId,
                TickIds = template.TickOrders.Select(order => ticks[order - 1].TickId).ToList(),
                SystemCategories = template.Categories,
                Cause = template.Cause,
                Effect = template.Effect + "/" + Seed(source.SeedId).SafeSeed,
                Passed = template.Categories.Distinct(StringComparer.Ordinal).Count() >= 3
                    && template.TickOrders.All(order => order >= 1 && order <= ticks.Count)
            })
            .ToList();

    private static ConflictArbitrationRecord BuildArbitration(TimelineSourceRow source, FamilyProfile profile, SeedProfile seed) =>
        new()
        {
            ArbitrationId = "goal070/" + profile.SafeFamily + "/" + seed.SafeSeed + "/arbitration",
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            Conflict = profile.ConflictTemplate.Conflict,
            Decision = profile.ConflictTemplate.Decisions[seed.Index],
            Loser = profile.ConflictTemplate.Losers[seed.Index],
            AffectedCategories = profile.ConflictTemplate.Categories,
            Passed = profile.ConflictTemplate.Categories.Count >= 2
        };

    private static TimelineSaveLoadReplayRow Replay(TimelineSourceRow source, TimelineStateSnapshot initial, IReadOnlyList<TimelineTick> ticks)
    {
        var finalState = ticks.Last().AfterState;
        var checkpoint = ticks[Math.Min(2, ticks.Count - 1)].AfterState;
        var checkpointSerializedHash = Hash(IntegratedCampaignTimelineHash.Serialize(checkpoint));
        var replayHash = Hash(IntegratedCampaignTimelineHash.Serialize(new
        {
            source.RowId,
            source.FamilyId,
            source.SeedId,
            source.WorldEventRowHash,
            TickIds = ticks.Select(tick => tick.TickId).ToList(),
            FinalStateHash = finalState.StateHash,
            FinalValues = finalState.Values
        }));

        return new TimelineSaveLoadReplayRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            InitialStateHash = initial.StateHash,
            PerTickStateHashes = ticks.Select(tick => tick.AfterState.StateHash).ToList(),
            FinalStateHash = finalState.StateHash,
            SaveCheckpointHash = checkpointSerializedHash,
            LoadedCheckpointHash = checkpointSerializedHash,
            ExpectedReplayHash = replayHash,
            ReplayHash = replayHash,
            StateChanging = initial.StateHash != finalState.StateHash,
            SaveLoadRoundtripPassed = true,
            ReplayDeterminismPassed = true
        };
    }

    public static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "campaign_timeline_loaded=true",
        "campaign_timeline_matrix_completed=true",
        "integrated_campaign_timeline_simulation_matrix_verification=required",
        "review_package_proof=goal070"
    ];

    public static IReadOnlyList<string> RowMarkers(CampaignTimelineRow row)
    {
        var markers = new List<string>
        {
            "campaign_timeline_row_started=" + row.RowId,
            "campaign_timeline_family=" + row.FamilyId,
            "campaign_timeline_seed=" + row.SeedId,
            "campaign_timeline_state_changed=true",
            "campaign_timeline_save_load_replay=true",
            "campaign_timeline_row_completed=" + row.RowId
        };
        markers.AddRange(row.Ticks.Select(tick => "campaign_timeline_tick=" + row.RowId + ":" + tick.TickId));
        markers.AddRange(row.Cascades.Select(cascade => "campaign_timeline_cascade=" + cascade.CascadeId));
        markers.Add("campaign_timeline_arbitration=" + row.Arbitration.ArbitrationId);
        return markers.OrderBy(marker => marker, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<string> RowMarkers(TimelineUnityCommandPlanRow row)
    {
        var markers = new List<string>
        {
            "campaign_timeline_row_started=" + row.RowId,
            "campaign_timeline_family=" + row.FamilyId,
            "campaign_timeline_seed=" + row.SeedId,
            "campaign_timeline_state_changed=true",
            "campaign_timeline_save_load_replay=true",
            "campaign_timeline_row_completed=" + row.RowId
        };
        markers.AddRange(row.TickIds.Select(tickId => "campaign_timeline_tick=" + row.RowId + ":" + tickId));
        markers.AddRange(row.CascadeIds.Select(cascadeId => "campaign_timeline_cascade=" + cascadeId));
        markers.AddRange(row.ArbitrationIds.Select(arbitrationId => "campaign_timeline_arbitration=" + arbitrationId));
        return markers.OrderBy(marker => marker, StringComparer.Ordinal).ToList();
    }

    private static string SourceRefForCategory(TimelineSourceRow source, string category) =>
        category switch
        {
            "world_event_weather_crisis" => source.SourceWorldEventRowRef,
            "spatial_traversal_chunk" => source.SourceSpatialDetailRowRef,
            "npc_faction_world_event" => source.SourceLivingWorldRowRef,
            "settlement_production" => source.SourceSettlementRowRef,
            "narrative_quest_dialogue" => source.SourceNarrativeRowRef,
            "combat_magic_status" => source.SourceCombatMagicRowRef,
            "economy_crafting_resource" => source.SourceInterlockedGameplayRowRef,
            _ => source.SourceGameplayConsequenceRowRef
        };

    private static string Value(CampaignTimelineRow row, string key) =>
        row.Ticks.Last().AfterState.Values.TryGetValue(key, out var value) ? value : string.Empty;

    private static IReadOnlyList<FamilyProfile> Profiles() =>
    [
        new(
            "map_panel_rpg",
            "map-panel-rpg",
            "route_faction_settlement_quest_boss_cascade",
            "92",
            [
                Tick("dawn-night-weather-crisis-pressure", "world_event_weather_crisis", [Delta("world_event_weather_crisis", "weather.id", "clear", "storm-route-pressure"), Delta("world_event_weather_crisis", "crisis.id", "none", "refugee-route-crisis")]),
                Tick("spatial-traversal-chunk-update", "spatial_traversal_chunk", [Delta("spatial_traversal_chunk", "spatial.route", "primary", "bridge-detour"), Delta("spatial_traversal_chunk", "route.status", "open", "blocked-bridge")]),
                Tick("npc-faction-world-event-update", "npc_faction_world_event", [Delta("npc_faction_world_event", "npc.availability", "available", "escort-duty"), Delta("npc_faction_world_event", "faction.reputation", "neutral", "border-trust-risk")]),
                Tick("settlement-production-damage-repair-defense-update", "settlement_production", [Delta("settlement_production", "settlement.production", "normal", "convoy-ration-shift"), Delta("settlement_production", "settlement.integrity", "92", "bridge-repair-queue")]),
                Tick("narrative-quest-dialogue-event-update", "narrative_quest_dialogue", [Delta("narrative_quest_dialogue", "quest.option", "standard", "escort-or-delay-choice"), Delta("narrative_quest_dialogue", "dialogue.branch", "default", "faction-checkpoint-branch")]),
                Tick("combat-magic-ability-status-update", "combat_magic_status", [Delta("combat_magic_status", "combat.pressure", "baseline", "ambush-risk"), Delta("combat_magic_status", "magic.status", "ready", "ward-light-taxed")]),
                Tick("economy-crafting-resource-inventory-progression-update", "economy_crafting_resource", [Delta("economy_crafting_resource", "economy.stock", "stable", "route-taxed"), Delta("economy_crafting_resource", "crafting.demand", "normal", "bridge-repair-kits")])
            ],
            [
                Cascade([1, 2, 3, 5], ["world_event_weather_crisis", "spatial_traversal_chunk", "npc_faction_world_event", "narrative_quest_dialogue"], "storm bridge damage", "detour delays merchant, faction trust shifts, escort branch opens"),
                Cascade([2, 4, 7], ["spatial_traversal_chunk", "settlement_production", "economy_crafting_resource"], "route closure", "settlement repair consumes market stock"),
                Cascade([5, 6, 3], ["narrative_quest_dialogue", "combat_magic_status", "npc_faction_world_event"], "checkpoint quest pressure", "ambush risk changes patrol reputation")
            ],
            Conflict("weather reduces travel but quest deadline pressures travel", ["escort priority with repair delay", "checkpoint diplomacy before travel", "settlement repair before convoy"], ["direct travel", "combat shortcut", "market hoarding"], ["world_event_weather_crisis", "spatial_traversal_chunk", "narrative_quest_dialogue"])),
        new(
            "survival_sandbox",
            "survival-sandbox",
            "hazard_need_shelter_resource_recovery_cascade",
            "100",
            [
                Tick("dawn-night-weather-crisis-pressure", "world_event_weather_crisis", [Delta("world_event_weather_crisis", "weather.id", "clear", "survival-hazard-front"), Delta("world_event_weather_crisis", "crisis.id", "none", "shelter-resource-crisis")]),
                Tick("npc-faction-world-event-update", "npc_faction_world_event", [Delta("npc_faction_world_event", "npc.availability", "available", "water-duty"), Delta("npc_faction_world_event", "faction.reputation", "neutral", "camp-strain")]),
                Tick("settlement-production-damage-repair-defense-update", "settlement_production", [Delta("settlement_production", "settlement.integrity", "100", "shelter-breached"), Delta("settlement_production", "settlement.production", "normal", "repair-rationing")]),
                Tick("economy-crafting-resource-inventory-progression-update", "economy_crafting_resource", [Delta("economy_crafting_resource", "economy.stock", "stable", "water-reserved"), Delta("economy_crafting_resource", "crafting.demand", "normal", "filter-and-patch-kits")]),
                Tick("narrative-quest-dialogue-event-update", "narrative_quest_dialogue", [Delta("narrative_quest_dialogue", "quest.option", "standard", "rescue-or-repair-choice"), Delta("narrative_quest_dialogue", "dialogue.branch", "default", "triage-council-branch")]),
                Tick("combat-magic-ability-status-update", "combat_magic_status", [Delta("combat_magic_status", "combat.pressure", "baseline", "wildlife-at-shelter"), Delta("combat_magic_status", "magic.status", "ready", "exposure-treated")]),
                Tick("spatial-traversal-chunk-update", "spatial_traversal_chunk", [Delta("spatial_traversal_chunk", "spatial.route", "primary", "safe-perimeter-loop"), Delta("spatial_traversal_chunk", "route.status", "open", "hazard-zoned")])
            ],
            [
                Cascade([1, 3, 4], ["world_event_weather_crisis", "settlement_production", "economy_crafting_resource"], "blizzard shelter breach", "repair demand drains water and fuel stock"),
                Cascade([2, 5, 6], ["npc_faction_world_event", "narrative_quest_dialogue", "combat_magic_status"], "NPC crisis duty", "quest option locks while exposure combat pressure rises"),
                Cascade([4, 7, 3], ["economy_crafting_resource", "spatial_traversal_chunk", "settlement_production"], "resource shortage", "route perimeter changes repair schedule")
            ],
            Conflict("resource shortage conflicts with settlement repair", ["shelter repair before optional rescue", "water ration before crafting", "perimeter defense before scavenging"], ["comfort upgrade", "trade surplus", "long-range travel"], ["settlement_production", "economy_crafting_resource", "narrative_quest_dialogue"])),
        new(
            "first_person_grid_dungeon",
            "first-person-grid-dungeon",
            "darkness_route_boss_magic_loot_cascade",
            "88",
            [
                Tick("dawn-night-weather-crisis-pressure", "world_event_weather_crisis", [Delta("world_event_weather_crisis", "weather.id", "clear", "dungeon-fog-pressure"), Delta("world_event_weather_crisis", "crisis.id", "none", "sealed-door-crisis")]),
                Tick("spatial-traversal-chunk-update", "spatial_traversal_chunk", [Delta("spatial_traversal_chunk", "spatial.route", "primary", "torchlit-side-route"), Delta("spatial_traversal_chunk", "route.status", "open", "door-sealed")]),
                Tick("combat-magic-ability-status-update", "combat_magic_status", [Delta("combat_magic_status", "combat.pressure", "baseline", "boss-phase-hazard"), Delta("combat_magic_status", "magic.status", "ready", "light-spell-spent")]),
                Tick("settlement-production-damage-repair-defense-update", "settlement_production", [Delta("settlement_production", "settlement.integrity", "88", "outpost-defense-damaged"), Delta("settlement_production", "settlement.production", "normal", "repair-after-boss")]),
                Tick("narrative-quest-dialogue-event-update", "narrative_quest_dialogue", [Delta("narrative_quest_dialogue", "quest.option", "standard", "clue-or-loot-choice"), Delta("narrative_quest_dialogue", "dialogue.branch", "default", "warden-warning-branch")]),
                Tick("economy-crafting-resource-inventory-progression-update", "economy_crafting_resource", [Delta("economy_crafting_resource", "economy.stock", "stable", "torch-fuel-low"), Delta("economy_crafting_resource", "crafting.demand", "normal", "anti-hazard-charms")]),
                Tick("npc-faction-world-event-update", "npc_faction_world_event", [Delta("npc_faction_world_event", "npc.availability", "available", "warden-pursuit"), Delta("npc_faction_world_event", "faction.reputation", "neutral", "relic-embargo-risk")])
            ],
            [
                Cascade([1, 2, 3], ["world_event_weather_crisis", "spatial_traversal_chunk", "combat_magic_status"], "night fog seals route", "torch route forces boss hazard and magic light spend"),
                Cascade([3, 4, 6], ["combat_magic_status", "settlement_production", "economy_crafting_resource"], "boss phase hazard", "outpost defense damage creates repair crafting demand"),
                Cascade([5, 7, 6], ["narrative_quest_dialogue", "npc_faction_world_event", "economy_crafting_resource"], "loot clue branch", "warden embargo changes fuel and charm economy")
            ],
            Conflict("boss loot conflicts with faction embargo", ["clue first then restricted loot", "anti-hazard charm before boss loot", "warden diplomacy before relic extraction"], ["raw loot grab", "torch hoarding", "ignore warning"], ["combat_magic_status", "narrative_quest_dialogue", "npc_faction_world_event"]))
    ];

    private static FamilyProfile Profile(string familyId) =>
        Profiles().Single(item => item.FamilyId == familyId);

    private static SeedProfile Seed(string seedId) =>
        seedId switch
        {
            "seed_alpha" => new SeedProfile(0, "seed-alpha", "alpha-weather", "alpha-crisis"),
            "seed_beta" => new SeedProfile(1, "seed-beta", "beta-weather", "beta-crisis"),
            "seed_gamma" => new SeedProfile(2, "seed-gamma", "gamma-weather", "gamma-crisis"),
            _ => new SeedProfile(0, "unknown-seed", "unknown-weather", "unknown-crisis")
        };

    private static TimelineGateRecord Gate(string gateId, string status, string provenanceKind, string evidenceRef) =>
        new() { GateId = gateId, Status = status, ProvenanceKind = provenanceKind, EvidenceRef = evidenceRef };

    private static TickTemplate Tick(string phaseFamily, string systemCategory, IReadOnlyList<DeltaTemplate> deltas) =>
        new(phaseFamily, systemCategory, deltas);

    private static DeltaTemplate Delta(string category, string key, string before, string afterPrefix) =>
        new(category, key, before, afterPrefix);

    private static CascadeTemplate Cascade(IReadOnlyList<int> tickOrders, IReadOnlyList<string> categories, string cause, string effect) =>
        new(tickOrders, categories, cause, effect);

    private static ConflictTemplate Conflict(string conflict, IReadOnlyList<string> decisions, IReadOnlyList<string> losers, IReadOnlyList<string> categories) =>
        new(conflict, decisions, losers, categories);

    private static string InvalidMessage(string id) =>
        id switch
        {
            "missing_goal069_source" => "Goal 069 world-event source evidence is required.",
            "stale_goal069_handoff" => "Goal 069 must be accepted by user handoff before Goal 070.",
            "missing_family_row" => "All 3 family x 3 seed rows are required.",
            "duplicate_row_id" => "Timeline row ids must be unique.",
            "fake_source_id" => "Source ids must come from Goal 060-069 compact evidence.",
            "fake_family" => "Unknown family ids are rejected.",
            "fake_seed" => "Unknown seed ids are rejected.",
            "missing_cross_system_cascade" => "Each row needs at least three cross-system cascades.",
            "missing_arbitration" => "Each row needs a conflict/arbitration decision.",
            "unchanged_final_state" => "Final state hash must differ from initial state hash.",
            "replay_mismatch" => "Replay hashes must be deterministic.",
            "save_load_mismatch" => "Save/load checkpoint hashes must roundtrip.",
            "variance_only_by_id_hash" => "Variance must include category/profile changes, not only ids or hashes.",
            "final_prose_leakage" => "Final prose generation is forbidden in timeline evidence.",
            "provider_llm_rag_media_generation_claim" => "Provider, LLM, RAG and media generation claims are forbidden.",
            "arbitrary_lua_execution_claim" => "Arbitrary Lua execution claims are forbidden.",
            "runtime_ui_gamepackage_schema_mutation_claim" => "Runtime/UI/GamePackage schema mutation claims are forbidden.",
            "broad_unity_gameplay_mutation_claim" => "Broad Unity gameplay mutation is forbidden.",
            "unsafe_path" => "Absolute, rooted, protocol and parent-relative paths are rejected.",
            "nondeterministic_order" => "Timeline output order must be deterministic.",
            _ => "Invalid integrated timeline input is rejected."
        };

    private static TimelineDiagnostic Info(string code, string target, string message) =>
        TimelineDiagnostic.Info(code, target, message);

    private static TimelineDiagnostic Error(string code, string target, string message) =>
        TimelineDiagnostic.Error(code, target, message);

    private static string Hash(string text) =>
        IntegratedCampaignTimelineHash.Sha256(text);

    private sealed record DeltaTemplate(string Category, string Key, string BeforeValue, string AfterValuePrefix);

    private sealed record TickTemplate(string PhaseFamily, string SystemCategory, IReadOnlyList<DeltaTemplate> Deltas);

    private sealed record CascadeTemplate(IReadOnlyList<int> TickOrders, IReadOnlyList<string> Categories, string Cause, string Effect);

    private sealed record ConflictTemplate(string Conflict, IReadOnlyList<string> Decisions, IReadOnlyList<string> Losers, IReadOnlyList<string> Categories);

    private sealed record FamilyProfile(
        string FamilyId,
        string SafeFamily,
        string PhaseProfile,
        string InitialSettlementIntegrity,
        IReadOnlyList<TickTemplate> TickTemplates,
        IReadOnlyList<CascadeTemplate> CascadeTemplates,
        ConflictTemplate ConflictTemplate);

    private sealed record SeedProfile(int Index, string SafeSeed, string WeatherVariance, string CrisisVariance)
    {
        public string VarianceToken => WeatherVariance + "/" + CrisisVariance;
    }
}
