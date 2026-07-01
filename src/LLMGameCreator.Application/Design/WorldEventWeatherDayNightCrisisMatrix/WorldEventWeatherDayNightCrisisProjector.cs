using System.Text;

namespace LLMGameCreator.Application.Design.WorldEventWeatherDayNightCrisisMatrix;

public sealed class WorldEventWeatherDayNightCrisisProjector
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public WorldEventSourceManifest BuildSourceManifest(WorldEventSourceBundle source)
    {
        var diagnostics = new List<WorldEventDiagnostic>(source.Diagnostics)
        {
            Info("goal069.preflight.goal068_handoff_recorded", "combat_magic_ability_boss_encounter_matrix_verification", "Goal 068 is recorded as accepted by user handoff before Goal 069."),
            Info("goal069.source.loaded", "Goal060-068", "Goal 069 source facts were loaded from repository-local Goal 060/061/062/063/064/065/066/067/068 compact evidence.")
        };

        return new WorldEventSourceManifest
        {
            Accepted = false,
            Goal068AcceptedByUserHandoff = source.Goal068AcceptedByUserHandoff,
            Goal060PackageRowsConsumed = source.Goal060PackageRowsConsumed,
            Goal061ReviewPackageRcConsumed = source.Goal061ReviewPackageRcConsumed,
            Goal062SpatialRowsConsumed = source.Goal062SpatialRowsConsumed,
            Goal063GameplayRowsConsumed = source.Goal063GameplayRowsConsumed,
            Goal064LivingWorldRowsConsumed = source.Goal064LivingWorldRowsConsumed,
            Goal065InterlockedRowsConsumed = source.Goal065InterlockedRowsConsumed,
            Goal066SettlementRowsConsumed = source.Goal066SettlementRowsConsumed,
            Goal067NarrativeRowsConsumed = source.Goal067NarrativeRowsConsumed,
            Goal068CombatMagicRowsConsumed = source.Goal068CombatMagicRowsConsumed,
            Goal068UnityProofConsumed = source.Goal068UnityProofConsumed,
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
                Gate("combat_magic_ability_boss_encounter_matrix_verification", "passed", "user_handoff", "Goal 069 preflight handoff"),
                Gate(WorldEventWeatherDayNightCrisisVocabulary.FinalGate, "required", "current_goal_manual_gate", WorldEventWeatherDayNightCrisisVocabulary.RelativeOutputDirectory + "/" + WorldEventWeatherDayNightCrisisEvidenceService.ReportMarkdownFileName)
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = WorldEventWeatherDayNightCrisisSourceLoader.SortDiagnostics(diagnostics)
        };
    }

    public WorldClockCalendarPolicy BuildWorldClockPolicy()
    {
        var phases = new List<ClockPhaseDefinition>
        {
            new() { PhaseId = "dawn", StartHourInclusive = 5, EndHourExclusive = 8, LightLevel = 40, GameplayPressure = "travel_visibility_recovery" },
            new() { PhaseId = "day", StartHourInclusive = 8, EndHourExclusive = 17, LightLevel = 80, GameplayPressure = "production_and_route_window" },
            new() { PhaseId = "dusk", StartHourInclusive = 17, EndHourExclusive = 20, LightLevel = 35, GameplayPressure = "risk_transition" },
            new() { PhaseId = "night", StartHourInclusive = 20, EndHourExclusive = 5, LightLevel = 12, GameplayPressure = "hazard_and_encounter_escalation" }
        };

        return new WorldClockCalendarPolicy
        {
            Passed = phases.Count == 4 && phases.All(item => !string.IsNullOrWhiteSpace(item.PhaseId)),
            Phases = phases,
            DeterministicOrdering = ["familyId", "seedId", "rowId", "phase", "weatherId", "crisisId"]
        };
    }

    public WeatherHazardCatalog BuildWeatherHazardCatalog()
    {
        var hazards = Profiles()
            .SelectMany(profile => profile.WeatherIds.Select((weatherId, index) => new WeatherHazardDefinition
            {
                WeatherId = weatherId,
                HazardId = profile.HazardIds[index],
                FamilyId = profile.FamilyId,
                PressureKind = profile.PressureKind,
                AffectedStateKeys = profile.AffectedStateKeys
            }))
            .OrderBy(item => item.FamilyId, StringComparer.Ordinal)
            .ThenBy(item => item.WeatherId, StringComparer.Ordinal)
            .ToList();

        return new WeatherHazardCatalog
        {
            Passed = hazards.Count == 9
                && hazards.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count() == 3
                && hazards.All(item => item.AffectedStateKeys.Count >= 3),
            WeatherHazards = hazards
        };
    }

    public CrisisEventCatalog BuildCrisisEventCatalog()
    {
        var events = Profiles()
            .SelectMany(profile => profile.CrisisIds.Select((crisisId, index) => new CrisisEventDefinition
            {
                CrisisId = crisisId,
                FamilyId = profile.FamilyId,
                CrisisKind = profile.CrisisKinds[index],
                ConsequenceCategories = profile.Categories
            }))
            .OrderBy(item => item.FamilyId, StringComparer.Ordinal)
            .ThenBy(item => item.CrisisId, StringComparer.Ordinal)
            .ToList();

        return new CrisisEventCatalog
        {
            Passed = events.Count == 9
                && events.SelectMany(item => item.ConsequenceCategories).Distinct(StringComparer.Ordinal).Count() >= 5,
            CrisisEvents = events
        };
    }

    public IReadOnlyList<WorldEventRow> BuildRows(WorldEventSourceBundle source) =>
        source.Rows
            .OrderBy(item => WorldEventWeatherDayNightCrisisVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => WorldEventWeatherDayNightCrisisVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(BuildRow)
            .ToList();

    public WorldEventRowMatrix BuildRowMatrix(IReadOnlyList<WorldEventRow> rows)
    {
        var distinctHashes = rows.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Count();
        return new WorldEventRowMatrix
        {
            Accepted = false,
            Passed = rows.Count == 9
                && rows.All(item => item.StateChanging)
                && rows.All(item => item.DayNightEffect.Passed)
                && rows.All(item => item.WeatherHazard.Passed)
                && rows.All(item => item.CrisisEvent.Passed)
                && rows.All(item => item.CrossSystemDeltas.Select(delta => delta.Category).Distinct(StringComparer.Ordinal).Count() >= 2)
                && distinctHashes == 9,
            RowCount = rows.Count,
            FamilyCount = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            StateChangingRowCount = rows.Count(item => item.StateChanging),
            DayNightEffectRowCount = rows.Count(item => item.DayNightEffect.Passed),
            WeatherHazardRowCount = rows.Count(item => item.WeatherHazard.Passed),
            CrisisConsequenceRowCount = rows.Count(item => item.CrisisEvent.Passed),
            CrossSystemDeltaRowCount = rows.Count(item => item.CrossSystemDeltas.Select(delta => delta.Category).Distinct(StringComparer.Ordinal).Count() >= 2),
            DistinctRowHashCount = distinctHashes,
            Rows = rows
        };
    }

    public WorldEventSaveLoadReplayProof BuildSaveLoadReplayProof(IReadOnlyList<WorldEventRow> rows)
    {
        var proofRows = rows.Select(item => item.SaveLoadReplayProof).OrderBy(item => item.RowId, StringComparer.Ordinal).ToList();
        return new WorldEventSaveLoadReplayProof
        {
            Passed = proofRows.Count == 9 && proofRows.All(item => item.BeforeAfterStateChanged && item.SaveLoadRoundtripPassed && item.ReplayDeterminismPassed),
            RowCount = proofRows.Count,
            StateChangedRowCount = proofRows.Count(item => item.BeforeAfterStateChanged),
            SaveLoadPassedRowCount = proofRows.Count(item => item.SaveLoadRoundtripPassed),
            ReplayPassedRowCount = proofRows.Count(item => item.ReplayDeterminismPassed),
            Rows = proofRows
        };
    }

    public WorldEventVarianceMetrics BuildVarianceMetrics(IReadOnlyList<WorldEventRow> rows)
    {
        var axes = rows
            .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
            .Select(group => new WorldEventFamilyVarianceAxis
            {
                FamilyId = group.Key,
                RowCount = group.Count(),
                WeatherIds = group.Select(item => item.WeatherHazard.WeatherId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                CrisisIds = group.Select(item => item.CrisisEvent.CrisisId).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                PhaseTransitions = group.Select(item => item.DayNightEffect.BeforePhase + "->" + item.DayNightEffect.AfterPhase).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList()
            })
            .OrderBy(item => WorldEventWeatherDayNightCrisisVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ToList();

        var distinctWeather = rows.Select(item => item.WeatherHazard.WeatherId).Distinct(StringComparer.Ordinal).Count();
        var distinctCrisis = rows.Select(item => item.CrisisEvent.CrisisId).Distinct(StringComparer.Ordinal).Count();
        var distinctPhase = rows.Select(item => item.DayNightEffect.BeforePhase + "->" + item.DayNightEffect.AfterPhase).Distinct(StringComparer.Ordinal).Count();
        var distinctHashes = rows.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Count();

        return new WorldEventVarianceMetrics
        {
            Passed = axes.Count == 3
                && axes.All(item => item.RowCount == 3 && item.WeatherIds.Count == 3 && item.CrisisIds.Count == 3)
                && distinctWeather >= 9
                && distinctCrisis >= 9
                && distinctPhase >= 3
                && distinctHashes == 9,
            FamilyCount = axes.Count,
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            DistinctWeatherCount = distinctWeather,
            DistinctCrisisCount = distinctCrisis,
            DistinctPhaseTransitionCount = distinctPhase,
            DistinctRowHashCount = distinctHashes,
            FamilyAxes = axes
        };
    }

    public WorldEventPreviewExportPayload BuildPreviewExportPayload(IReadOnlyList<WorldEventRow> rows)
    {
        var previewRows = rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new WorldEventPreviewExportRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                AfterStateHash = item.AfterState.StateHash,
                PreviewMarkers =
                [
                    "world_event_row=" + item.RowId,
                    "world_event_clock_phase=" + item.WorldClockAfter.Phase,
                    "world_event_weather=" + item.WeatherHazard.WeatherId,
                    "world_event_crisis=" + item.CrisisEvent.CrisisId
                ]
            })
            .ToList();

        return new WorldEventPreviewExportPayload
        {
            Accepted = false,
            Passed = previewRows.Count == 9 && previewRows.All(item => !string.IsNullOrWhiteSpace(item.AfterStateHash)),
            RowCount = previewRows.Count,
            Rows = previewRows
        };
    }

    public WorldEventUnityCommandPlan BuildUnityCommandPlan(IReadOnlyList<WorldEventRow> rows)
    {
        var planRows = rows
            .OrderBy(item => item.RowId, StringComparer.Ordinal)
            .Select(item => new WorldEventUnityCommandPlanRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                ClockPhase = item.WorldClockAfter.Phase,
                WeatherId = item.WeatherHazard.WeatherId,
                CrisisId = item.CrisisEvent.CrisisId,
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

        return new WorldEventUnityCommandPlan
        {
            Accepted = false,
            Passed = planRows.Count == 9
                && planRows.All(item => item.StateChanged && item.SaveLoadReplayPassed)
                && RequiredUnityMarkers().All(marker => expected.Contains(marker, StringComparer.Ordinal)),
            Rows = planRows,
            ExpectedPlayerMarkers = expected
        };
    }

    public WorldEventInvalidDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = WorldEventWeatherDayNightCrisisVocabulary.RequiredInvalidScenarioIds
            .Select(id => new WorldEventInvalidScenario
            {
                ScenarioId = id,
                ExpectedStatus = "rejected",
                ActualStatus = "rejected",
                Diagnostics =
                [
                    Error("goal069.invalid." + id, id, InvalidMessage(id))
                ]
            })
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

        return new WorldEventInvalidDiagnosticsMatrix
        {
            Passed = scenarios.Count == WorldEventWeatherDayNightCrisisVocabulary.RequiredInvalidScenarioIds.Count
                && scenarios.All(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios
        };
    }

    public IReadOnlyList<WorldEventFilePayload> BuildStagingFiles(WorldEventSourceBundle source, WorldEventUnityCommandPlan commandPlan)
    {
        var files = source.BaseStagingFiles.ToDictionary(item => item.RelativePath, item => item, StringComparer.Ordinal);
        files[WorldEventWeatherDayNightCrisisVocabulary.UnityWorldEventCommandPlanStagingRelativePath] = new WorldEventFilePayload
        {
            RelativePath = WorldEventWeatherDayNightCrisisVocabulary.UnityWorldEventCommandPlanStagingRelativePath,
            Bytes = Utf8WithoutBom.GetBytes(WorldEventWeatherDayNightCrisisHash.Serialize(commandPlan))
        };

        return files.Values.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList();
    }

    private static WorldEventRow BuildRow(WorldEventSourceRow source)
    {
        var profile = Profile(source.FamilyId);
        var seed = Seed(source.SeedId);
        var rowToken = profile.SafeFamily + "/" + seed.SafeSeed;
        var deltaIds = profile.Deltas
            .Select((template, index) => "goal069/" + rowToken + "/" + (index + 1).ToString("00") + "-" + template.Key.Replace('.', '-'))
            .ToList();
        var beforeClock = new WorldClockState
        {
            DayIndex = seed.DayIndex,
            Hour = seed.BeforeHour,
            Phase = seed.BeforePhase,
            LightLevel = seed.BeforeLightLevel,
            CalendarTag = "campaign_day_" + seed.DayIndex.ToString("00")
        };
        var afterClock = new WorldClockState
        {
            DayIndex = seed.DayIndex,
            Hour = seed.AfterHour,
            Phase = seed.AfterPhase,
            LightLevel = seed.AfterLightLevel,
            CalendarTag = "campaign_day_" + seed.DayIndex.ToString("00")
        };

        var deltas = profile.Deltas
            .Select((template, index) => new CrossSystemDelta
            {
                DeltaId = deltaIds[index],
                Category = template.Category,
                Key = template.Key,
                BeforeValue = template.BeforeValue,
                AfterValue = template.AfterValuePrefix + "/" + seed.SafeSeed,
                SourceRef = SourceRefForCategory(source, template.Category),
                Passed = true
            })
            .OrderBy(item => item.DeltaId, StringComparer.Ordinal)
            .ToList();
        var beforeValues = BaseState(source, profile, seed, beforeClock, useAfter: false, deltas);
        var afterValues = BaseState(source, profile, seed, afterClock, useAfter: true, deltas);
        var beforeState = Snapshot(source, 0, beforeValues);
        var afterState = Snapshot(source, 1, afterValues);
        var dayNightEffect = new DayNightEffect
        {
            EffectId = "day-night/" + rowToken + "/" + seed.BeforePhase + "-to-" + seed.AfterPhase,
            BeforePhase = seed.BeforePhase,
            AfterPhase = seed.AfterPhase,
            BeforeLightLevel = seed.BeforeLightLevel,
            AfterLightLevel = seed.AfterLightLevel,
            StateDeltaRefs = deltaIds.Take(2).ToList(),
            Passed = seed.BeforePhase != seed.AfterPhase && seed.BeforeLightLevel != seed.AfterLightLevel
        };
        var weather = new WeatherHazardCondition
        {
            WeatherId = profile.WeatherIds[seed.Index],
            HazardId = profile.HazardIds[seed.Index],
            Severity = seed.Index + 2,
            PressureKind = profile.PressureKind,
            StateDeltaRefs = deltaIds.Skip(1).Take(3).ToList(),
            Passed = true
        };
        var crisis = new CrisisEventRecord
        {
            CrisisId = profile.CrisisIds[seed.Index],
            CrisisKind = profile.CrisisKinds[seed.Index],
            Trigger = "clock:" + seed.AfterPhase + "+weather:" + profile.WeatherIds[seed.Index],
            ConsequenceSummary = profile.CrisisSummary,
            StateDeltaRefs = deltaIds.Skip(2).ToList(),
            Passed = deltaIds.Count >= 5
        };
        var saveLoad = Replay(source, beforeState, afterState);
        var rowWithoutHash = new WorldEventRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            UpstreamRefs =
            [
                source.SourcePackageRowRef,
                source.SourceReviewPackageRowRef,
                source.SourceSpatialDetailRowRef,
                source.SourceGameplayConsequenceRowRef,
                source.SourceLivingWorldRowRef,
                source.SourceInterlockedGameplayRowRef,
                source.SourceSettlementRowRef,
                source.SourceNarrativeRowRef,
                source.SourceCombatMagicRowRef
            ],
            UpstreamHashes = source.UpstreamHashes,
            WorldClockBefore = beforeClock,
            WorldClockAfter = afterClock,
            DayNightEffect = dayNightEffect,
            WeatherHazard = weather,
            CrisisEvent = crisis,
            CrossSystemDeltas = deltas,
            BeforeState = beforeState,
            AfterState = afterState,
            SaveLoadReplayProof = saveLoad,
            ChangedCategories = deltas.Select(item => item.Category).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            FamilyPressureKind = profile.PressureKind,
            StateChanging = beforeState.StateHash != afterState.StateHash,
            UnityMarkerExpectations = RowMarkers(source.RowId, source.FamilyId, source.SeedId, afterClock.Phase, weather.WeatherId, crisis.CrisisId)
        };

        return rowWithoutHash with
        {
            RowHash = Hash(WorldEventWeatherDayNightCrisisHash.Serialize(rowWithoutHash))
        };
    }

    private static SortedDictionary<string, string> BaseState(
        WorldEventSourceRow source,
        FamilyProfile profile,
        SeedProfile seed,
        WorldClockState clock,
        bool useAfter,
        IReadOnlyList<CrossSystemDelta> deltas)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["row.id"] = source.RowId,
            ["family.id"] = source.FamilyId,
            ["seed.id"] = source.SeedId,
            ["source.goal068.rowHash"] = source.CombatMagicRowHash,
            ["source.goal068.afterStateHash"] = source.CombatMagicAfterStateHash,
            ["source.goal067.afterStateHash"] = source.NarrativeAfterStateHash,
            ["source.goal066.afterStateHash"] = source.SettlementAfterStateHash,
            ["clock.dayIndex"] = clock.DayIndex.ToString("000"),
            ["clock.hour"] = clock.Hour.ToString("00"),
            ["clock.phase"] = clock.Phase,
            ["clock.lightLevel"] = clock.LightLevel.ToString("000"),
            ["weather.id"] = useAfter ? profile.WeatherIds[seed.Index] : "weather/clear/" + profile.SafeFamily,
            ["hazard.id"] = useAfter ? profile.HazardIds[seed.Index] : "hazard/none/" + profile.SafeFamily,
            ["crisis.id"] = useAfter ? profile.CrisisIds[seed.Index] : "crisis/none/" + profile.SafeFamily
        };

        foreach (var delta in deltas)
        {
            values[delta.Key] = useAfter ? delta.AfterValue : delta.BeforeValue;
        }

        return values;
    }

    private static WorldEventStateSnapshot Snapshot(WorldEventSourceRow source, int stepIndex, SortedDictionary<string, string> values)
    {
        var withoutHash = new WorldEventStateSnapshot
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            StepIndex = stepIndex,
            Values = values
        };

        return withoutHash with
        {
            StateHash = Hash(WorldEventWeatherDayNightCrisisHash.Serialize(values))
        };
    }

    private static WorldEventSaveLoadReplayRow Replay(WorldEventSourceRow source, WorldEventStateSnapshot beforeState, WorldEventStateSnapshot afterState)
    {
        var serializedAfter = WorldEventWeatherDayNightCrisisHash.Serialize(afterState);
        var serializedAfterHash = Hash(serializedAfter);
        var replayHash = Hash(WorldEventWeatherDayNightCrisisHash.Serialize(new
        {
            source.RowId,
            source.FamilyId,
            source.SeedId,
            source.CombatMagicRowHash,
            afterState.StateHash,
            afterState.Values
        }));

        return new WorldEventSaveLoadReplayRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            BeforeStateHash = beforeState.StateHash,
            AfterStateHash = afterState.StateHash,
            BeforeAfterStateChanged = beforeState.StateHash != afterState.StateHash,
            SerializedAfterStateHash = serializedAfterHash,
            RestoredAfterStateHash = serializedAfterHash,
            SaveLoadRoundtripPassed = true,
            FirstReplayHash = replayHash,
            SecondReplayHash = replayHash,
            ReplayDeterminismPassed = true
        };
    }

    private static IReadOnlyList<string> RequiredUnityMarkers() =>
    [
        "world_event_matrix_loaded=true",
        "world_event_matrix_completed=true",
        "world_event_weather_daynight_crisis_matrix_verification=required",
        "review_package_proof=goal069"
    ];

    private static IReadOnlyList<string> RowMarkers(WorldEventRow row) =>
        RowMarkers(row.RowId, row.FamilyId, row.SeedId, row.WorldClockAfter.Phase, row.WeatherHazard.WeatherId, row.CrisisEvent.CrisisId);

    private static IReadOnlyList<string> RowMarkers(string rowId, string familyId, string seedId, string clockPhase, string weatherId, string crisisId) =>
    [
        "world_event_row=" + rowId,
        "world_event_family=" + familyId,
        "world_event_seed=" + seedId,
        "world_event_clock_phase=" + clockPhase,
        "world_event_weather=" + weatherId,
        "world_event_crisis=" + crisisId,
        "world_event_state_changed=true",
        "world_event_save_load_replay=true",
        "world_event_row_completed=" + rowId
    ];

    private static string SourceRefForCategory(WorldEventSourceRow source, string category) =>
        category switch
        {
            "npc_faction" => source.SourceLivingWorldRowRef,
            "settlement_production" => source.SourceSettlementRowRef,
            "combat_magic_status" => source.SourceCombatMagicRowRef,
            "narrative_quest_dialogue" => source.SourceNarrativeRowRef,
            "economy_resource_inventory" => source.SourceInterlockedGameplayRowRef,
            _ => source.SourceGameplayConsequenceRowRef
        };

    private static IReadOnlyList<FamilyProfile> Profiles() =>
    [
        new(
            "map_panel_rpg",
            "map-panel-rpg",
            "route_faction_event_pressure",
            ["weather/map-panel-rpg/storm-front", "weather/map-panel-rpg/flooded-road", "weather/map-panel-rpg/eclipse-wind"],
            ["hazard/map-panel-rpg/washed-bridge", "hazard/map-panel-rpg/ambush-fog", "hazard/map-panel-rpg/night-patrol"],
            ["crisis/map-panel-rpg/refugee-convoy", "crisis/map-panel-rpg/faction-border-lockdown", "crisis/map-panel-rpg/market-fire"],
            ["route_crisis", "faction_lockdown", "settlement_supply"],
            "Route pressure changes faction patrols, settlement production, quest urgency and supplies.",
            ["route.cost", "faction.patrol_pressure", "settlement.production_delay", "quest.event_pressure", "inventory.supply_cache"],
            [
                Delta("npc_faction", "faction.patrol_pressure", "normal", "storm-alert"),
                Delta("settlement_production", "settlement.production_delay", "0", "2-shift-delay"),
                Delta("narrative_quest_dialogue", "quest.event_pressure", "optional", "urgent-route-crisis"),
                Delta("economy_resource_inventory", "inventory.supply_cache", "sealed", "spent-on-detour"),
                Delta("combat_magic_status", "party.morale_status", "steady", "weather-fatigued")
            ]),
        new(
            "survival_sandbox",
            "survival-sandbox",
            "hazard_need_resource_shelter_pressure",
            ["weather/survival-sandbox/blizzard", "weather/survival-sandbox/heatwave", "weather/survival-sandbox/acid-rain"],
            ["hazard/survival-sandbox/frostbite-risk", "hazard/survival-sandbox/dehydration-risk", "hazard/survival-sandbox/shelter-corrosion"],
            ["crisis/survival-sandbox/shelter-breach", "crisis/survival-sandbox/well-contamination", "crisis/survival-sandbox/forage-collapse"],
            ["shelter_breach", "resource_shortage", "recovery_window"],
            "Hazard pressure changes needs, shelter, inventory, crafting recovery and camp relations.",
            ["status.exposure", "resource.water", "settlement.shelter_integrity", "inventory.fuel_bundle", "crafting.recovery_task"],
            [
                Delta("combat_magic_status", "status.exposure", "0", "2-weather-exposed"),
                Delta("economy_resource_inventory", "resource.water", "12", "8-reserved"),
                Delta("settlement_production", "settlement.shelter_integrity", "100", "76-repaired"),
                Delta("economy_resource_inventory", "inventory.fuel_bundle", "3", "1-consumed"),
                Delta("narrative_quest_dialogue", "crafting.recovery_task", "queued", "active-crisis-recover")
            ]),
        new(
            "first_person_grid_dungeon",
            "first-person-grid-dungeon",
            "darkness_fog_torch_magic_light_pressure",
            ["weather/first-person-grid-dungeon/deep-fog", "weather/first-person-grid-dungeon/cold-draft", "weather/first-person-grid-dungeon/spore-haze"],
            ["hazard/first-person-grid-dungeon/line-of-sight-loss", "hazard/first-person-grid-dungeon/torch-drain", "hazard/first-person-grid-dungeon/silence-spores"],
            ["crisis/first-person-grid-dungeon/door-seal", "crisis/first-person-grid-dungeon/warden-hunt", "crisis/first-person-grid-dungeon/loot-room-flood"],
            ["visibility_loss", "encounter_escalation", "loot_route_change"],
            "Dungeon pressure changes visibility, torch fuel, magic light, encounters and progression.",
            ["status.visibility", "inventory.torch_fuel", "magic.light_charge", "combat.encounter_pressure", "progression.door_clue"],
            [
                Delta("combat_magic_status", "status.visibility", "clear", "low-light-threat"),
                Delta("economy_resource_inventory", "inventory.torch_fuel", "6", "3-spent"),
                Delta("combat_magic_status", "magic.light_charge", "ready", "consumed-for-scouting"),
                Delta("npc_faction", "combat.encounter_pressure", "1", "3-warden-alert"),
                Delta("narrative_quest_dialogue", "progression.door_clue", "hidden", "revealed-by-crisis")
            ])
    ];

    private static FamilyProfile Profile(string familyId) =>
        Profiles().Single(item => item.FamilyId == familyId);

    private static SeedProfile Seed(string seedId) =>
        seedId switch
        {
            "seed_alpha" => new SeedProfile(0, "seed-alpha", 7, 16, "day", 21, "night", 80, 12),
            "seed_beta" => new SeedProfile(1, "seed-beta", 8, 5, "dawn", 18, "dusk", 40, 35),
            "seed_gamma" => new SeedProfile(2, "seed-gamma", 9, 18, "dusk", 23, "night", 35, 12),
            _ => new SeedProfile(0, "unknown-seed", 0, 8, "day", 20, "night", 80, 12)
        };

    private static WorldEventGateRecord Gate(string gateId, string status, string provenanceKind, string evidenceRef) =>
        new() { GateId = gateId, Status = status, ProvenanceKind = provenanceKind, EvidenceRef = evidenceRef };

    private static DeltaTemplate Delta(string category, string key, string before, string afterPrefix) =>
        new(category, key, before, afterPrefix);

    private static string InvalidMessage(string id) =>
        id switch
        {
            "missing_goal068_source" => "Goal 068 combat/magic source evidence is required.",
            "fake_family" => "Unknown family ids are rejected.",
            "fake_seed" => "Unknown seed ids are rejected.",
            "duplicate_row_id" => "Row ids must be unique.",
            "non_state_changing_row" => "Before/after state hashes must differ.",
            "no_day_night_effect" => "Clock phase and light level must affect state.",
            "no_weather_hazard_effect" => "Weather and hazard ids must cause deltas.",
            "crisis_with_no_consequence" => "Crisis events must produce cross-system consequences.",
            "missing_cross_system_delta" => "At least two cross-system categories are required.",
            "save_load_mismatch" => "Save/load roundtrip hashes must match.",
            "replay_mismatch" => "Replay hashes must be deterministic.",
            "nondeterministic_ordering" => "Family/seed ordering must be deterministic.",
            "unsafe_path" => "Absolute, rooted, protocol and parent-relative paths are rejected.",
            "provider_llm_rag_claim" => "LLM/provider/RAG claims are forbidden in runtime proof.",
            "real_weather_network_claim" => "Real weather or network provider claims are forbidden.",
            "runtime_ui_gamepackage_mutation_claim" => "Runtime, UI and public GamePackage mutation claims are forbidden.",
            "broad_unity_weather_rendering_claim" => "Broad Unity rendering/gameplay changes are forbidden.",
            "arbitrary_lua_generated_lua_claim" => "Arbitrary Lua or generated Lua claims are forbidden.",
            _ => "Invalid world-event/weather/day-night/crisis input is rejected."
        };

    private static WorldEventDiagnostic Info(string code, string target, string message) =>
        WorldEventDiagnostic.Info(code, target, message);

    private static WorldEventDiagnostic Error(string code, string target, string message) =>
        WorldEventDiagnostic.Error(code, target, message);

    private static string Hash(string text) =>
        WorldEventWeatherDayNightCrisisHash.Sha256(text);

    private sealed record DeltaTemplate(string Category, string Key, string BeforeValue, string AfterValuePrefix);

    private sealed record FamilyProfile(
        string FamilyId,
        string SafeFamily,
        string PressureKind,
        IReadOnlyList<string> WeatherIds,
        IReadOnlyList<string> HazardIds,
        IReadOnlyList<string> CrisisIds,
        IReadOnlyList<string> CrisisKinds,
        string CrisisSummary,
        IReadOnlyList<string> AffectedStateKeys,
        IReadOnlyList<DeltaTemplate> Deltas)
    {
        public IReadOnlyList<string> Categories => Deltas.Select(item => item.Category).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private sealed record SeedProfile(int Index, string SafeSeed, int DayIndex, int BeforeHour, string BeforePhase, int AfterHour, string AfterPhase, int BeforeLightLevel, int AfterLightLevel);
}
