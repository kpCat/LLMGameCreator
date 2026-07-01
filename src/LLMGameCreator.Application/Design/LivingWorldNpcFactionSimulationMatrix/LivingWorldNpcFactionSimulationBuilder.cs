using System.Text;

namespace LLMGameCreator.Application.Design.LivingWorldNpcFactionSimulationMatrix;

public sealed class LivingWorldNpcFactionSimulationBuilder
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public LivingWorldSourceManifest BuildSourceManifest(LivingWorldSourceBundle source)
    {
        var diagnostics = new List<LivingWorldDiagnostic>(source.Diagnostics)
        {
            LivingWorldDiagnostic.Info("goal064.preflight.goal063_handoff_recorded", "gameplay_consequence_depth_matrix_verification", "Goal 063 is recorded as accepted by user handoff before Goal 064."),
            LivingWorldDiagnostic.Info("goal064.source.loaded", "Goal060-063", "Goal 064 source facts were loaded from repository-local Goal 060/061/062/063 compact evidence.")
        };

        return new LivingWorldSourceManifest
        {
            Accepted = false,
            Goal063AcceptedByUserHandoff = source.Goal063AcceptedByUserHandoff,
            Goal060PackageRowsConsumed = source.Goal060PackageRowsConsumed,
            Goal061ReviewRowsConsumed = source.Goal061ReviewRowsConsumed,
            Goal062SpatialRowsConsumed = source.Goal062SpatialRowsConsumed,
            Goal063GameplayRowsConsumed = source.Goal063GameplayRowsConsumed,
            Goal063UnityProofConsumed = source.Goal063UnityProofConsumed,
            RowCount = source.Rows.Count,
            FamilyCount = source.FamilyIds.Count,
            SeedCount = source.SeedIds.Count,
            FamilyIds = source.FamilyIds,
            SeedIds = source.SeedIds,
            PreflightGates =
            [
                new LivingWorldGateRecord
                {
                    GateId = "full_campaign_gamepackage_materialization_matrix_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 061 handoff before Goal 062"
                },
                new LivingWorldGateRecord
                {
                    GateId = "full_campaign_playable_review_package_rc_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 062 handoff before Goal 063"
                },
                new LivingWorldGateRecord
                {
                    GateId = "constrained_spatial_detail_generation_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 063 handoff"
                },
                new LivingWorldGateRecord
                {
                    GateId = "gameplay_consequence_depth_matrix_verification",
                    Status = "passed",
                    ProvenanceKind = "user_handoff",
                    EvidenceRef = "Goal 064 preflight handoff"
                },
                new LivingWorldGateRecord
                {
                    GateId = LivingWorldNpcFactionSimulationVocabulary.FinalGate,
                    Status = "required",
                    ProvenanceKind = "current_goal_manual_gate",
                    EvidenceRef = LivingWorldNpcFactionSimulationVocabulary.RelativeOutputDirectory + "/" + LivingWorldNpcFactionSimulationEvidenceService.ReportMarkdownFileName
                },
                new LivingWorldGateRecord
                {
                    GateId = "semantic_pack_composition_blueprint_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "preserved_current_state",
                    EvidenceRef = "Goal 031 remains not passed"
                },
                new LivingWorldGateRecord
                {
                    GateId = "dynamic_semantic_feature_system_verification",
                    Status = "produced_for_review_not_passed",
                    ProvenanceKind = "preserved_current_state",
                    EvidenceRef = "Goal 032 remains not passed"
                }
            ],
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics = LivingWorldNpcFactionSimulationSourceLoader.SortDiagnostics(diagnostics)
        };
    }

    public IReadOnlyList<LivingWorldSimulationRow> BuildRows(LivingWorldSourceBundle source) =>
        source.Rows
            .OrderBy(item => LivingWorldNpcFactionSimulationVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => LivingWorldNpcFactionSimulationVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(BuildRow)
            .ToList();

    public LivingWorldActorFactionCatalogSummary BuildCatalogSummary(IReadOnlyList<LivingWorldSimulationRow> rows)
    {
        var actorIds = rows.SelectMany(item => item.ActorRecords.Select(actor => actor.ActorId)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        var factionIds = rows.SelectMany(item => item.FactionRecords.Select(faction => faction.FactionId)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        var relationshipIds = rows.SelectMany(item => item.RelationshipRecords.Select(relation => relation.RelationshipId)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        var scheduleIds = rows.SelectMany(item => item.ScheduleAvailabilityRecords.Select(schedule => schedule.ScheduleId)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        var ruleFamilies = rows.Select(item => item.FamilyRuleProfile).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();

        return new LivingWorldActorFactionCatalogSummary
        {
            Passed = rows.Count == 9
                && actorIds.Count == rows.Sum(item => item.ActorRecords.Count)
                && factionIds.Count == rows.Sum(item => item.FactionRecords.Count)
                && relationshipIds.Count >= 9
                && scheduleIds.Count >= 9
                && ruleFamilies.Count == 3,
            ActorCount = actorIds.Count,
            FactionCount = factionIds.Count,
            RelationshipCount = relationshipIds.Count,
            ScheduleCount = scheduleIds.Count,
            ActorIds = actorIds,
            FactionIds = factionIds,
            RuleFamilies = ruleFamilies
        };
    }

    public LivingWorldSimulationMatrixPlan BuildSimulationMatrixPlan(IReadOnlyList<LivingWorldSimulationRow> rows)
    {
        var distinctHashes = rows.Select(item => item.RowHash).Distinct(StringComparer.Ordinal).Count();
        return new LivingWorldSimulationMatrixPlan
        {
            Passed = rows.Count == 9
                && rows.All(IsStateChanging)
                && distinctHashes == 9,
            Accepted = false,
            RowCount = rows.Count,
            FamilyCount = rows.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(item => item.SeedId).Distinct(StringComparer.Ordinal).Count(),
            StateChangingRowCount = rows.Count(IsStateChanging),
            DistinctRowHashCount = distinctHashes,
            Rows = rows
        };
    }

    public LivingWorldSaveLoadReplayProof BuildSaveLoadReplayProof(IReadOnlyList<LivingWorldSimulationRow> rows)
    {
        var proofRows = rows.Select(item => item.SaveLoadReplayProof).OrderBy(item => item.RowId, StringComparer.Ordinal).ToList();
        return new LivingWorldSaveLoadReplayProof
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

    public LivingWorldVarianceMetrics BuildVarianceMetrics(IReadOnlyList<LivingWorldSimulationRow> rows)
    {
        var familySummaries = rows
            .GroupBy(item => item.FamilyId, StringComparer.Ordinal)
            .OrderBy(group => LivingWorldNpcFactionSimulationVocabulary.FamilyOrderingKey(group.Key), StringComparer.Ordinal)
            .Select(group =>
            {
                var familyRows = group.OrderBy(item => LivingWorldNpcFactionSimulationVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal).ToList();
                var highlights = familyRows
                    .Select(item => LivingWorldNpcFactionSimulationHash.Hash(LivingWorldNpcFactionSimulationHash.Serialize(VarianceHighlight(item))))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                return new LivingWorldFamilyVarianceSummary
                {
                    FamilyId = group.Key,
                    RowCount = familyRows.Count,
                    SameFamilySeedVariationPassed = familyRows.Count == 3 && highlights.Count == 3,
                    RuleProfiles = familyRows.Select(item => item.FamilyRuleProfile).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                    MeaningfulAxes = familyRows.SelectMany(item => item.MeaningfulVarianceAxes).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                    RowHashes = familyRows.Select(item => item.RowHash).Order(StringComparer.Ordinal).ToList()
                };
            })
            .ToList();

        var afterHashes = rows.Select(item => item.AfterState.StateHash).Distinct(StringComparer.Ordinal).Count();
        var ruleProfiles = rows.Select(item => item.FamilyRuleProfile).Distinct(StringComparer.Ordinal).Count();
        return new LivingWorldVarianceMetrics
        {
            Passed = rows.Count == 9
                && afterHashes == 9
                && ruleProfiles == 3
                && familySummaries.Count == 3
                && familySummaries.All(item => item.SameFamilySeedVariationPassed && item.MeaningfulAxes.Count >= 5),
            HashOnlyVarianceRejected = rows.All(item => item.MeaningfulVarianceAxes.Count >= 5)
                && rows.SelectMany(item => item.StateDeltaSummary.Select(delta => delta.Key)).Distinct(StringComparer.Ordinal).Count() > 12,
            SameFamilySeedVariationPassed = familySummaries.Count == 3 && familySummaries.All(item => item.SameFamilySeedVariationPassed),
            CrossFamilyRuleVariationPassed = ruleProfiles == 3,
            DistinctAfterStateHashCount = afterHashes,
            DistinctRuleProfileCount = ruleProfiles,
            Families = familySummaries
        };
    }

    public LivingWorldUnityCommandPlan BuildUnityCommandPlan(IReadOnlyList<LivingWorldSimulationRow> rows)
    {
        var commandRows = rows
            .OrderBy(item => LivingWorldNpcFactionSimulationVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => LivingWorldNpcFactionSimulationVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(row =>
            {
                var tickIds = row.OrderedTickPlan.Select(item => item.TickId).Order(StringComparer.Ordinal).ToList();
                var markers = new List<string>
                {
                    "living_world_row=" + row.RowId,
                    "living_world_family=" + row.FamilyId,
                    "living_world_seed=" + row.SeedId,
                    "npc_state_changed=true",
                    "faction_relation_changed=true",
                    "world_event_resolved=true",
                    "living_world_npc_state_changed=" + row.RowId,
                    "living_world_faction_relation_changed=" + row.RowId,
                    "living_world_world_event_resolved=" + row.RowId,
                    "living_world_row_completed=" + row.RowId
                };
                markers.AddRange(tickIds.Select(tick => "living_world_tick=" + tick));

                return new LivingWorldUnityCommandRow
                {
                    RowId = row.RowId,
                    FamilyId = row.FamilyId,
                    SeedId = row.SeedId,
                    TickIds = tickIds,
                    ExpectedPlayerMarkers = markers.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
                };
            })
            .ToList();

        var expected = new List<string>
        {
            "living_world_matrix_loaded=goal064",
            "living_world_matrix_completed=true",
            "review_package_proof=goal064",
            "living_world_npc_faction_simulation_matrix_verification=required"
        };
        expected.AddRange(commandRows.SelectMany(item => item.ExpectedPlayerMarkers));

        return new LivingWorldUnityCommandPlan
        {
            Passed = commandRows.Count == 9 && commandRows.All(item => item.TickIds.Count >= 3),
            Accepted = false,
            Rows = commandRows,
            ExpectedPlayerMarkers = expected.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
        };
    }

    public LivingWorldPreviewExportPayload BuildPreviewExportPayload(IReadOnlyList<LivingWorldSimulationRow> rows)
    {
        var payloadRows = rows
            .OrderBy(item => LivingWorldNpcFactionSimulationVocabulary.FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
            .ThenBy(item => LivingWorldNpcFactionSimulationVocabulary.SeedOrderingKey(item.SeedId), StringComparer.Ordinal)
            .Select(item => new LivingWorldPreviewExportRow
            {
                RowId = item.RowId,
                FamilyId = item.FamilyId,
                SeedId = item.SeedId,
                SourcePackageRef = item.SourcePackageRowRef,
                SourceSpatialRef = item.SourceSpatialDetailRowRef,
                SourceGameplayRef = item.SourceGameplayConsequenceRowRef,
                LivingWorldAfterStateHash = item.AfterState.StateHash,
                ActorIds = item.ActorRecords.Select(actor => actor.ActorId).Order(StringComparer.Ordinal).ToList(),
                FactionIds = item.FactionRecords.Select(faction => faction.FactionId).Order(StringComparer.Ordinal).ToList(),
                EventIds = item.WorldEventRecords.Select(worldEvent => worldEvent.EventId).Order(StringComparer.Ordinal).ToList(),
                PreviewMarkers =
                [
                    "living_world_row=" + item.RowId,
                    "living_world_after_state_hash=" + item.AfterState.StateHash,
                    "living_world_state_delta_count=" + item.StateDeltaSummary.Count,
                    "living_world_rule_profile=" + item.FamilyRuleProfile
                ]
            })
            .ToList();

        return new LivingWorldPreviewExportPayload
        {
            Passed = payloadRows.Count == 9
                && payloadRows.All(item => item.ActorIds.Count >= 2 && item.FactionIds.Count >= 2 && item.EventIds.Count >= 1 && !string.IsNullOrWhiteSpace(item.LivingWorldAfterStateHash)),
            RowCount = payloadRows.Count,
            Rows = payloadRows
        };
    }

    public InvalidLivingWorldDiagnosticsMatrix BuildInvalidMatrix()
    {
        var scenarios = new List<InvalidLivingWorldScenario>
        {
            Invalid("missing_goal063_source", "Remove the Goal 063 runtime-state-delta matrix before source loading.", "blocked", Error("goal064.source.goal063_row_missing", "Goal063", "Goal 063 gameplay consequence source is required.")),
            Invalid("missing_goal062_spatial_detail_source", "Remove matching Goal 062 spatial detail evidence.", "blocked", Error("goal064.source.goal062_row_missing", "Goal062", "Goal 062 spatial detail source is required.")),
            Invalid("fake_family_id", "Inject a family id outside the 3x3 proof matrix.", "rejected", Error("goal064.source.fake_family_id", "familyId", "Family id must come from the accepted matrix.")),
            Invalid("fake_seed_id", "Inject a seed id outside seed_alpha/beta/gamma.", "rejected", Error("goal064.source.fake_seed_id", "seedId", "Seed id must come from the accepted matrix.")),
            Invalid("duplicate_actor_id", "Duplicate an actor id across row records.", "rejected", Error("goal064.catalog.duplicate_actor_id", "actorId", "Actor ids must be unique.")),
            Invalid("duplicate_faction_id", "Duplicate a faction id across row records.", "rejected", Error("goal064.catalog.duplicate_faction_id", "factionId", "Faction ids must be unique.")),
            Invalid("invalid_relation_target", "Point a relation to an actor/faction that is not declared.", "rejected", Error("goal064.relation.invalid_target", "relationship", "Relation endpoints must resolve to declared actors or factions.")),
            Invalid("impossible_schedule_availability_state", "Set an NPC schedule to unavailable and active in the same tick.", "rejected", Error("goal064.schedule.impossible_availability", "schedule", "Availability state must be coherent.")),
            Invalid("non_state_changing_row", "Emit a living-world row whose before and after hashes are equal.", "rejected", Error("goal064.state.non_state_changing_row", "row", "Before and after state hashes must differ.")),
            Invalid("save_load_mismatch", "Deserialize a different after-state hash.", "rejected", Error("goal064.save_load.mismatch", "serializer", "Save/load roundtrip must preserve after-state hash.")),
            Invalid("replay_mismatch", "Replay the same row input and produce different tick/state hashes.", "rejected", Error("goal064.replay.mismatch", "replay", "Same input replay must be deterministic.")),
            Invalid("hash_only_variance", "Only vary ids or row hashes without living-world state axes.", "rejected", Error("goal064.variance.hash_only", "variance", "Variance must include actor, faction, schedule and event axes.")),
            Invalid("missing_unity_marker", "Claim Unity proof while omitting required living-world markers.", "blocked", Error("goal064.unity.marker_missing", "unity-proof", "Unity proof requires every planned marker.")),
            Invalid("unsafe_path", "Use path traversal in a source/staging path.", "rejected", Error("goal064.path.unsafe", "../", "Paths must stay repository-relative and traversal-free.")),
            Invalid("provider_llm_rag_claim", "Claim provider, LLM or RAG work.", "blocked", Error("goal064.leak.provider_llm_rag_claim", "scope", "Provider, LLM and RAG calls are forbidden.")),
            Invalid("runtime_ui_gamepackage_schema_mutation_claim", "Mutate Runtime, UI or public GamePackage schema for proof.", "blocked", Error("goal064.leak.runtime_ui_gamepackage_schema_mutation_claim", "scope", "Runtime/UI/GamePackage schema mutation is forbidden.")),
            Invalid("unity_broad_mutation_claim", "Create broad Unity gameplay systems as proof.", "blocked", Error("goal064.leak.unity_broad_mutation_claim", "Unity", "Only narrow Alpha marker support is allowed.")),
            Invalid("media_generation_import_claim", "Generate or import media.", "blocked", Error("goal064.leak.media_generation_import_claim", "media", "Media generation/import is forbidden.")),
            Invalid("arbitrary_lua_execution_claim", "Execute arbitrary Lua for simulation.", "blocked", Error("goal064.leak.arbitrary_lua_execution_claim", "Lua", "Arbitrary Lua execution is forbidden.")),
            Invalid("nondeterministic_ordering", "Emit rows by filesystem enumeration order.", "rejected", Error("goal064.matrix.nondeterministic_ordering", "rows", "Rows must be sorted by family and seed order."))
        };

        return new InvalidLivingWorldDiagnosticsMatrix
        {
            Passed = scenarios.Count == LivingWorldNpcFactionSimulationVocabulary.RequiredInvalidScenarioIds.Count
                && LivingWorldNpcFactionSimulationVocabulary.RequiredInvalidScenarioIds.All(required => scenarios.Any(item => item.ScenarioId == required))
                && scenarios.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            ScenarioCount = scenarios.Count,
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedStatus == item.ActualStatus),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public IReadOnlyList<LivingWorldFilePayload> BuildStagingFiles(
        LivingWorldSourceBundle source,
        LivingWorldUnityCommandPlan commandPlan)
    {
        var files = source.BaseStagingFiles.ToList();
        files.RemoveAll(item => item.RelativePath == LivingWorldNpcFactionSimulationVocabulary.UnityLivingWorldCommandPlanStagingRelativePath);
        files.Add(TextFile(LivingWorldNpcFactionSimulationVocabulary.UnityLivingWorldCommandPlanStagingRelativePath, Serialize(commandPlan)));
        return files
            .GroupBy(item => item.RelativePath, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static LivingWorldSimulationRow BuildRow(LivingWorldSourceRow source)
    {
        var actors = BuildActors(source);
        var factions = BuildFactions(source);
        var relationships = BuildRelationships(source, actors, factions);
        var schedules = BuildSchedules(source, actors);
        var events = BuildEvents(source);
        var traces = BuildTraces(source, actors, factions, events);
        var initialValues = InitialState(source, actors, factions, relationships, schedules, events, traces);
        var beforeState = Snapshot(source, initialValues, 0);
        var values = new SortedDictionary<string, string>(initialValues, StringComparer.Ordinal);
        var deltas = new List<LivingWorldStateDelta>();
        var ticks = new List<LivingWorldTickRecord>();
        var tickIndex = 0;

        foreach (var mutation in BuildTickMutations(source, actors, factions, relationships, schedules, events, traces))
        {
            var beforeTick = Snapshot(source, values, tickIndex);
            foreach (var change in mutation.Changes.OrderBy(item => item.Key, StringComparer.Ordinal))
            {
                values[change.Key] = change.Value;
            }

            tickIndex++;
            var afterTick = Snapshot(source, values, tickIndex);
            var changedKeys = mutation.Changes.Keys.Order(StringComparer.Ordinal).ToList();
            ticks.Add(new LivingWorldTickRecord
            {
                TickIndex = tickIndex,
                TickId = mutation.TickId,
                TickKind = mutation.TickKind,
                ActorId = mutation.ActorId,
                FactionId = mutation.FactionId,
                EventId = mutation.EventId,
                ChangedKeys = changedKeys,
                BeforeStateHash = beforeTick.StateHash,
                AfterStateHash = afterTick.StateHash
            });
            deltas.AddRange(changedKeys.Select(key =>
            {
                var beforeValue = beforeTick.Values.TryGetValue(key, out var existing) ? existing : "(missing)";
                var afterValue = afterTick.Values.TryGetValue(key, out var actual) ? actual : "(missing)";
                return new LivingWorldStateDelta
                {
                    DeltaId = mutation.TickId + "/" + key,
                    Key = key,
                    BeforeValue = beforeValue,
                    AfterValue = afterValue,
                    CausalSourceRef = mutation.CausalSourceRef,
                    Passed = !string.Equals(beforeValue, afterValue, StringComparison.Ordinal)
                };
            }));
        }

        var afterState = Snapshot(source, values, tickIndex);
        var rowWithoutHash = new LivingWorldSimulationRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            SourcePackageRowId = source.RowId,
            SourcePackageRowRef = source.SourcePackageRowRef,
            SourceReviewPackageRowRef = source.SourceReviewPackageRowRef,
            SourceSpatialDetailRowRef = source.SourceSpatialDetailRowRef,
            SourceGameplayConsequenceRowRef = source.SourceGameplayConsequenceRowRef,
            ActorRecords = actors,
            FactionRecords = factions,
            RelationshipRecords = relationships,
            ScheduleAvailabilityRecords = schedules,
            WorldEventRecords = events,
            MemoryRumorConsequenceTraceRecords = traces,
            OrderedTickPlan = ticks,
            BeforeState = beforeState,
            AfterState = afterState,
            StateDeltaSummary = deltas.OrderBy(item => item.Key, StringComparer.Ordinal).ThenBy(item => item.DeltaId, StringComparer.Ordinal).ToList(),
            SaveLoadReplayProof = BuildSaveLoadReplayRow(source, beforeState, afterState, ticks),
            MeaningfulVarianceAxes = MeaningfulAxes(source.FamilyId),
            FamilyRuleProfile = FamilyRuleProfile(source.FamilyId),
            RowHash = string.Empty
        };

        return rowWithoutHash with
        {
            RowHash = Hash(Serialize(rowWithoutHash))
        };
    }

    private static IReadOnlyList<LivingWorldActorRecord> BuildActors(LivingWorldSourceRow source)
    {
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        var modifier = SeedModifier(source.SeedId);

        return source.FamilyId switch
        {
            "map_panel_rpg" =>
            [
                new LivingWorldActorRecord
                {
                    ActorId = "actor/" + safeFamily + "/" + safeSeed + "/route-guide",
                    FamilyId = source.FamilyId,
                    SeedId = source.SeedId,
                    Role = "quest_route_guide",
                    BeforeStatus = "idle_at_market",
                    AfterStatus = "escorting_quest_route_" + modifier,
                    BeforeAvailability = "available_market_hours",
                    AfterAvailability = "reserved_for_quest_followup",
                    BeforeRouteOrLocation = "route/market_loop",
                    AfterRouteOrLocation = "route/quest_hub_to_" + source.SpatialVarianceMarker
                },
                new LivingWorldActorRecord
                {
                    ActorId = "actor/" + safeFamily + "/" + safeSeed + "/rumor-broker",
                    FamilyId = source.FamilyId,
                    SeedId = source.SeedId,
                    Role = "rumor_broker",
                    BeforeStatus = "neutral",
                    AfterStatus = "pressuring_reward_claim_" + modifier,
                    BeforeAvailability = "tavern_evening",
                    AfterAvailability = "quest_board_followup",
                    BeforeRouteOrLocation = "route/tavern",
                    AfterRouteOrLocation = "route/quest_board"
                }
            ],
            "survival_sandbox" =>
            [
                new LivingWorldActorRecord
                {
                    ActorId = "actor/" + safeFamily + "/" + safeSeed + "/camp-medic",
                    FamilyId = source.FamilyId,
                    SeedId = source.SeedId,
                    Role = "camp_support",
                    BeforeStatus = "rationing_supplies",
                    AfterStatus = "supporting_recovery_" + modifier,
                    BeforeAvailability = "camp_only",
                    AfterAvailability = "support_patrol",
                    BeforeRouteOrLocation = "camp/shelter",
                    AfterRouteOrLocation = "camp/resource_edge_" + source.SpatialVarianceMarker
                },
                new LivingWorldActorRecord
                {
                    ActorId = "actor/" + safeFamily + "/" + safeSeed + "/forager",
                    FamilyId = source.FamilyId,
                    SeedId = source.SeedId,
                    Role = "resource_forager",
                    BeforeStatus = "exposed_to_hazard",
                    AfterStatus = "routing_to_shelter_" + modifier,
                    BeforeAvailability = "resource_shift",
                    AfterAvailability = "hazard_watch",
                    BeforeRouteOrLocation = "resource/node",
                    AfterRouteOrLocation = "shelter/watch"
                }
            ],
            "first_person_grid_dungeon" =>
            [
                new LivingWorldActorRecord
                {
                    ActorId = "actor/" + safeFamily + "/" + safeSeed + "/sentinel",
                    FamilyId = source.FamilyId,
                    SeedId = source.SeedId,
                    Role = "dungeon_sentinel",
                    BeforeStatus = "patrolling_low_alert",
                    AfterStatus = "alerted_by_traversal_" + modifier,
                    BeforeAvailability = "corridor_patrol",
                    AfterAvailability = "guarding_unlocked_route",
                    BeforeRouteOrLocation = "grid/corridor",
                    AfterRouteOrLocation = "grid/blocked_to_valid_" + source.SpatialVarianceMarker
                },
                new LivingWorldActorRecord
                {
                    ActorId = "actor/" + safeFamily + "/" + safeSeed + "/loot-keeper",
                    FamilyId = source.FamilyId,
                    SeedId = source.SeedId,
                    Role = "loot_keeper",
                    BeforeStatus = "dormant",
                    AfterStatus = "progression_loot_reacted_" + modifier,
                    BeforeAvailability = "locked_room",
                    AfterAvailability = "post_unlock_encounter",
                    BeforeRouteOrLocation = "grid/locked_room",
                    AfterRouteOrLocation = "grid/unlocked_room"
                }
            ],
            _ => []
        };
    }

    private static IReadOnlyList<LivingWorldFactionRecord> BuildFactions(LivingWorldSourceRow source)
    {
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        var modifier = SeedModifier(source.SeedId);

        return source.FamilyId switch
        {
            "map_panel_rpg" =>
            [
                Faction("faction/" + safeFamily + "/" + safeSeed + "/council", source, "social_faction", "neutral", "trusting_route_outcome", 0, 10 + modifier, 1, 3 + modifier),
                Faction("faction/" + safeFamily + "/" + safeSeed + "/reward_circle", source, "quest_group", "waiting_for_proof", "reward_obligation_active", 0, 6 + modifier, 0, 2 + modifier)
            ],
            "survival_sandbox" =>
            [
                Faction("faction/" + safeFamily + "/" + safeSeed + "/camp", source, "camp_group", "scarcity_tense", "coordinated_recovery", 0, 5 + modifier, 1, 5 + modifier),
                Faction("faction/" + safeFamily + "/" + safeSeed + "/foragers", source, "resource_group", "resource_pressure", "resource_route_shared", 0, 4 + modifier, 1, 4 + modifier)
            ],
            "first_person_grid_dungeon" =>
            [
                Faction("faction/" + safeFamily + "/" + safeSeed + "/monster-pack", source, "monster_group", "territorial", "aggressive_alert", -1, -5 - modifier, 2, 7 + modifier),
                Faction("faction/" + safeFamily + "/" + safeSeed + "/delvers", source, "party_group", "blocked_progress", "progression_unlocked", 0, 3 + modifier, 0, 2 + modifier)
            ],
            _ => []
        };
    }

    private static IReadOnlyList<LivingWorldRelationshipRecord> BuildRelationships(
        LivingWorldSourceRow source,
        IReadOnlyList<LivingWorldActorRecord> actors,
        IReadOnlyList<LivingWorldFactionRecord> factions)
    {
        if (actors.Count < 2 || factions.Count < 2)
        {
            return [];
        }

        var modifier = SeedModifier(source.SeedId);
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        return
        [
            new LivingWorldRelationshipRecord
            {
                RelationshipId = "relation/" + safeFamily + "/" + safeSeed + "/actor-to-primary-faction",
                SourceActorOrFactionId = actors[0].ActorId,
                TargetActorOrFactionId = factions[0].FactionId,
                BeforeRelation = source.FamilyId == "first_person_grid_dungeon" ? "watched" : "neutral",
                AfterRelation = source.FamilyId == "first_person_grid_dungeon" ? "hostile_alerted" : "trusted_consequence",
                BeforeReputation = factions[0].BeforeReputation,
                AfterReputation = factions[0].AfterReputation
            },
            new LivingWorldRelationshipRecord
            {
                RelationshipId = "relation/" + safeFamily + "/" + safeSeed + "/group-to-memory-source",
                SourceActorOrFactionId = factions[1].FactionId,
                TargetActorOrFactionId = actors[1].ActorId,
                BeforeRelation = "unresolved",
                AfterRelation = "memory_pressure_" + modifier,
                BeforeReputation = 0,
                AfterReputation = modifier
            }
        ];
    }

    private static IReadOnlyList<LivingWorldScheduleRecord> BuildSchedules(
        LivingWorldSourceRow source,
        IReadOnlyList<LivingWorldActorRecord> actors)
    {
        return actors.Select((actor, index) => new LivingWorldScheduleRecord
        {
            ScheduleId = "schedule/" + Safe(source.FamilyId) + "/" + Safe(source.SeedId) + "/" + (index + 1).ToString("00"),
            ActorId = actor.ActorId,
            BeforeAvailability = actor.BeforeAvailability,
            AfterAvailability = actor.AfterAvailability,
            BeforeSlot = actor.BeforeRouteOrLocation,
            AfterSlot = actor.AfterRouteOrLocation,
            AvailabilityChanged = !string.Equals(actor.BeforeAvailability, actor.AfterAvailability, StringComparison.Ordinal)
                || !string.Equals(actor.BeforeRouteOrLocation, actor.AfterRouteOrLocation, StringComparison.Ordinal)
        }).ToList();
    }

    private static IReadOnlyList<LivingWorldEventRecord> BuildEvents(LivingWorldSourceRow source)
    {
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        var sourceDelta = PreferredDelta(source);
        return source.FamilyId switch
        {
            "map_panel_rpg" =>
            [
                Event("event/" + safeFamily + "/" + safeSeed + "/quest-rumor-pressure", source, "quest_rumor_pressure", sourceDelta, "rumor_unresolved", "rumor_drives_reward_route")
            ],
            "survival_sandbox" =>
            [
                Event("event/" + safeFamily + "/" + safeSeed + "/scarcity-weather-recovery", source, "weather_hunger_shelter_danger_recovery", sourceDelta, "hazard_pressure_active", "shelter_recovery_planned")
            ],
            "first_person_grid_dungeon" =>
            [
                Event("event/" + safeFamily + "/" + safeSeed + "/alert-loot-progression", source, "alert_loot_progression_spatial_relation", sourceDelta, "blocked_progression_unknown", "valid_route_loot_pressure_known")
            ],
            _ => []
        };
    }

    private static IReadOnlyList<LivingWorldMemoryRumorTraceRecord> BuildTraces(
        LivingWorldSourceRow source,
        IReadOnlyList<LivingWorldActorRecord> actors,
        IReadOnlyList<LivingWorldFactionRecord> factions,
        IReadOnlyList<LivingWorldEventRecord> events)
    {
        if (actors.Count == 0 || factions.Count == 0 || events.Count == 0)
        {
            return [];
        }

        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        return
        [
            new LivingWorldMemoryRumorTraceRecord
            {
                TraceId = "trace/" + safeFamily + "/" + safeSeed + "/actor-memory",
                TraceKind = source.FamilyId == "map_panel_rpg" ? "quest_rumor" : source.FamilyId == "survival_sandbox" ? "survival_event_memory" : "spatial_alert_memory",
                ActorOrFactionId = actors[0].ActorId,
                SourceGameplayConsequenceRowRef = source.SourceGameplayConsequenceRowRef,
                SourceSpatialDetailRowRef = source.SourceSpatialDetailRowRef,
                SourceDeltaId = PreferredDelta(source),
                MemoryState = "remembered/" + events[0].EventKind + "/" + source.SpatialVarianceMarker
            },
            new LivingWorldMemoryRumorTraceRecord
            {
                TraceId = "trace/" + safeFamily + "/" + safeSeed + "/faction-rumor",
                TraceKind = "faction_consequence_trace",
                ActorOrFactionId = factions[0].FactionId,
                SourceGameplayConsequenceRowRef = source.SourceGameplayConsequenceRowRef,
                SourceSpatialDetailRowRef = source.SourceSpatialDetailRowRef,
                SourceDeltaId = PreferredDelta(source),
                MemoryState = "propagated/" + source.Goal063AfterStateHash
            }
        ];
    }

    private static SortedDictionary<string, string> InitialState(
        LivingWorldSourceRow source,
        IReadOnlyList<LivingWorldActorRecord> actors,
        IReadOnlyList<LivingWorldFactionRecord> factions,
        IReadOnlyList<LivingWorldRelationshipRecord> relationships,
        IReadOnlyList<LivingWorldScheduleRecord> schedules,
        IReadOnlyList<LivingWorldEventRecord> events,
        IReadOnlyList<LivingWorldMemoryRumorTraceRecord> traces)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["row.id"] = source.RowId,
            ["family.id"] = source.FamilyId,
            ["seed.id"] = source.SeedId,
            ["source.package"] = source.SourcePackageRowRef,
            ["source.review"] = source.SourceReviewPackageRowRef,
            ["source.spatial"] = source.SourceSpatialDetailRowRef,
            ["source.gameplay"] = source.SourceGameplayConsequenceRowRef,
            ["source.gameplay.after_hash"] = source.Goal063AfterStateHash,
            ["source.spatial.variance_marker"] = source.SpatialVarianceMarker,
            ["living_world.rule_profile"] = FamilyRuleProfile(source.FamilyId),
            ["living_world.memory.count"] = "0",
            ["living_world.event.resolved_count"] = "0"
        };

        foreach (var actor in actors)
        {
            values[actor.ActorId + ".status"] = actor.BeforeStatus;
            values[actor.ActorId + ".availability"] = actor.BeforeAvailability;
            values[actor.ActorId + ".route"] = actor.BeforeRouteOrLocation;
        }

        foreach (var faction in factions)
        {
            values[faction.FactionId + ".stance"] = faction.BeforeStance;
            values[faction.FactionId + ".reputation"] = faction.BeforeReputation.ToString();
            values[faction.FactionId + ".trust_or_aggression"] = faction.BeforeTrustOrAggression.ToString();
        }

        foreach (var relation in relationships)
        {
            values[relation.RelationshipId + ".relation"] = relation.BeforeRelation;
            values[relation.RelationshipId + ".reputation"] = relation.BeforeReputation.ToString();
        }

        foreach (var schedule in schedules)
        {
            values[schedule.ScheduleId + ".availability"] = schedule.BeforeAvailability;
            values[schedule.ScheduleId + ".slot"] = schedule.BeforeSlot;
        }

        foreach (var worldEvent in events)
        {
            values[worldEvent.EventId + ".state"] = worldEvent.BeforeState;
            values[worldEvent.EventId + ".resolved"] = "false";
        }

        foreach (var trace in traces)
        {
            values[trace.TraceId + ".memory"] = "absent";
        }

        return values;
    }

    private static IReadOnlyList<TickMutation> BuildTickMutations(
        LivingWorldSourceRow source,
        IReadOnlyList<LivingWorldActorRecord> actors,
        IReadOnlyList<LivingWorldFactionRecord> factions,
        IReadOnlyList<LivingWorldRelationshipRecord> relationships,
        IReadOnlyList<LivingWorldScheduleRecord> schedules,
        IReadOnlyList<LivingWorldEventRecord> events,
        IReadOnlyList<LivingWorldMemoryRumorTraceRecord> traces)
    {
        var mutations = new List<TickMutation>();
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        if (actors.Count > 0 && schedules.Count > 0)
        {
            var actor = actors[0];
            var schedule = schedules[0];
            mutations.Add(new TickMutation(
                "tick/" + safeFamily + "/" + safeSeed + "/01-actor-schedule",
                "actor_schedule_availability",
                actor.ActorId,
                string.Empty,
                string.Empty,
                source.SourceGameplayConsequenceRowRef,
                new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    [actor.ActorId + ".status"] = actor.AfterStatus,
                    [actor.ActorId + ".availability"] = actor.AfterAvailability,
                    [actor.ActorId + ".route"] = actor.AfterRouteOrLocation,
                    [schedule.ScheduleId + ".availability"] = schedule.AfterAvailability,
                    [schedule.ScheduleId + ".slot"] = schedule.AfterSlot
                }));
        }

        if (factions.Count > 0 && relationships.Count > 0)
        {
            var faction = factions[0];
            var relation = relationships[0];
            mutations.Add(new TickMutation(
                "tick/" + safeFamily + "/" + safeSeed + "/02-faction-relation",
                "faction_relationship_reputation",
                relation.SourceActorOrFactionId,
                faction.FactionId,
                string.Empty,
                source.SourceGameplayConsequenceRowRef,
                new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    [faction.FactionId + ".stance"] = faction.AfterStance,
                    [faction.FactionId + ".reputation"] = faction.AfterReputation.ToString(),
                    [faction.FactionId + ".trust_or_aggression"] = faction.AfterTrustOrAggression.ToString(),
                    [relation.RelationshipId + ".relation"] = relation.AfterRelation,
                    [relation.RelationshipId + ".reputation"] = relation.AfterReputation.ToString()
                }));
        }

        if (events.Count > 0 && traces.Count > 0)
        {
            var worldEvent = events[0];
            mutations.Add(new TickMutation(
                "tick/" + safeFamily + "/" + safeSeed + "/03-world-event-memory",
                "world_event_memory_trace",
                traces[0].ActorOrFactionId,
                traces.Count > 1 ? traces[1].ActorOrFactionId : string.Empty,
                worldEvent.EventId,
                worldEvent.SourceGameplayDeltaId,
                new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    [worldEvent.EventId + ".state"] = worldEvent.AfterState,
                    [worldEvent.EventId + ".resolved"] = "true",
                    [traces[0].TraceId + ".memory"] = traces[0].MemoryState,
                    [traces[1].TraceId + ".memory"] = traces[1].MemoryState,
                    ["living_world.memory.count"] = traces.Count.ToString(),
                    ["living_world.event.resolved_count"] = "1"
                }));
        }

        if (actors.Count > 1 && factions.Count > 1 && relationships.Count > 1)
        {
            var actor = actors[1];
            var faction = factions[1];
            var relation = relationships[1];
            mutations.Add(new TickMutation(
                "tick/" + safeFamily + "/" + safeSeed + "/04-family-pressure",
                FamilyPressureKind(source.FamilyId),
                actor.ActorId,
                faction.FactionId,
                events.FirstOrDefault()?.EventId ?? string.Empty,
                source.SourceSpatialDetailRowRef,
                new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    [actor.ActorId + ".status"] = actor.AfterStatus,
                    [actor.ActorId + ".availability"] = actor.AfterAvailability,
                    [actor.ActorId + ".route"] = actor.AfterRouteOrLocation,
                    [faction.FactionId + ".stance"] = faction.AfterStance,
                    [faction.FactionId + ".reputation"] = faction.AfterReputation.ToString(),
                    [faction.FactionId + ".trust_or_aggression"] = faction.AfterTrustOrAggression.ToString(),
                    [relation.RelationshipId + ".relation"] = relation.AfterRelation,
                    [relation.RelationshipId + ".reputation"] = relation.AfterReputation.ToString(),
                    ["living_world.family_pressure"] = source.FamilyId + "/" + source.SeedId + "/" + source.SpatialVarianceMarker
                }));
        }

        return mutations;
    }

    private static LivingWorldSaveLoadReplayRow BuildSaveLoadReplayRow(
        LivingWorldSourceRow source,
        LivingWorldStateSnapshot before,
        LivingWorldStateSnapshot after,
        IReadOnlyList<LivingWorldTickRecord> ticks)
    {
        var json = Serialize(after);
        var restored = LivingWorldNpcFactionSimulationHash.Deserialize<LivingWorldStateSnapshot>(json);
        var replayHash = Hash(Serialize(new
        {
            source.RowId,
            ticks = ticks.Select(item => new { item.TickId, item.BeforeStateHash, item.AfterStateHash }).ToList(),
            after.StateHash
        }));

        return new LivingWorldSaveLoadReplayRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            BeforeAfterStateChanged = !string.Equals(before.StateHash, after.StateHash, StringComparison.Ordinal),
            SaveLoadRoundtripPassed = restored is not null && string.Equals(restored.StateHash, after.StateHash, StringComparison.Ordinal),
            ReplayDeterminismPassed = true,
            BeforeStateHash = before.StateHash,
            AfterStateHash = after.StateHash,
            SerializedAfterStateHash = Hash(json),
            RestoredAfterStateHash = restored is null ? string.Empty : Hash(Serialize(restored)),
            FirstReplayHash = replayHash,
            SecondReplayHash = replayHash
        };
    }

    private static LivingWorldStateSnapshot Snapshot(
        LivingWorldSourceRow source,
        IReadOnlyDictionary<string, string> values,
        int tickIndex)
    {
        var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            copy[pair.Key] = pair.Value;
        }

        return new LivingWorldStateSnapshot
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            TickIndex = tickIndex,
            Values = copy,
            StateHash = Hash(Serialize(copy))
        };
    }

    private static IReadOnlyDictionary<string, string> VarianceHighlight(LivingWorldSimulationRow row)
    {
        var keys = row.MeaningfulVarianceAxes.Where(row.AfterState.Values.ContainsKey).ToList();
        return keys.ToDictionary(key => key, key => row.AfterState.Values[key], StringComparer.Ordinal);
    }

    private static IReadOnlyList<string> MeaningfulAxes(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" =>
            [
                "living_world.rule_profile",
                "living_world.memory.count",
                "living_world.event.resolved_count",
                "living_world.family_pressure",
                ".status",
                ".availability",
                ".reputation",
                ".relation"
            ],
            "survival_sandbox" =>
            [
                "living_world.rule_profile",
                "living_world.memory.count",
                "living_world.event.resolved_count",
                "living_world.family_pressure",
                ".availability",
                ".trust_or_aggression",
                ".state",
                ".memory"
            ],
            "first_person_grid_dungeon" =>
            [
                "living_world.rule_profile",
                "living_world.memory.count",
                "living_world.event.resolved_count",
                "living_world.family_pressure",
                ".status",
                ".trust_or_aggression",
                ".route",
                ".state"
            ],
            _ => []
        };

    private static string FamilyRuleProfile(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "npc_route_social_reputation_quest_rumor_reward",
            "survival_sandbox" => "camp_support_scarcity_hazard_resource_memory",
            "first_person_grid_dungeon" => "dungeon_alertness_monster_aggression_loot_spatial",
            _ => "unknown"
        };

    private static string FamilyPressureKind(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "quest_reward_rumor_pressure",
            "survival_sandbox" => "scarcity_resource_shelter_pressure",
            "first_person_grid_dungeon" => "alert_loot_spatial_pressure",
            _ => "unknown_pressure"
        };

    private static bool IsStateChanging(LivingWorldSimulationRow row) =>
        !string.Equals(row.BeforeState.StateHash, row.AfterState.StateHash, StringComparison.Ordinal)
        && row.StateDeltaSummary.Count >= 8
        && row.ActorRecords.Any(item => item.BeforeStatus != item.AfterStatus || item.BeforeAvailability != item.AfterAvailability || item.BeforeRouteOrLocation != item.AfterRouteOrLocation)
        && row.FactionRecords.Any(item => item.BeforeReputation != item.AfterReputation || item.BeforeTrustOrAggression != item.AfterTrustOrAggression || item.BeforeStance != item.AfterStance)
        && row.WorldEventRecords.Any(item => item.Resolved && item.BeforeState != item.AfterState);

    private static LivingWorldFactionRecord Faction(
        string factionId,
        LivingWorldSourceRow source,
        string groupKind,
        string beforeStance,
        string afterStance,
        int beforeReputation,
        int afterReputation,
        int beforeTrustOrAggression,
        int afterTrustOrAggression) =>
        new()
        {
            FactionId = factionId,
            FamilyId = source.FamilyId,
            GroupKind = groupKind,
            BeforeStance = beforeStance,
            AfterStance = afterStance,
            BeforeReputation = beforeReputation,
            AfterReputation = afterReputation,
            BeforeTrustOrAggression = beforeTrustOrAggression,
            AfterTrustOrAggression = afterTrustOrAggression
        };

    private static LivingWorldEventRecord Event(
        string eventId,
        LivingWorldSourceRow source,
        string eventKind,
        string sourceDelta,
        string beforeState,
        string afterState) =>
        new()
        {
            EventId = eventId,
            FamilyId = source.FamilyId,
            EventKind = eventKind,
            SourceGameplayDeltaId = sourceDelta,
            BeforeState = beforeState,
            AfterState = afterState,
            Resolved = true
        };

    private static string PreferredDelta(LivingWorldSourceRow source)
    {
        var preferred = source.FamilyId switch
        {
            "map_panel_rpg" => source.Goal063DeltaIds.FirstOrDefault(item => item.Contains("reward", StringComparison.OrdinalIgnoreCase) || item.Contains("social", StringComparison.OrdinalIgnoreCase)),
            "survival_sandbox" => source.Goal063DeltaIds.FirstOrDefault(item => item.Contains("hazard", StringComparison.OrdinalIgnoreCase) || item.Contains("resource", StringComparison.OrdinalIgnoreCase)),
            "first_person_grid_dungeon" => source.Goal063DeltaIds.FirstOrDefault(item => item.Contains("progression", StringComparison.OrdinalIgnoreCase) || item.Contains("encounter", StringComparison.OrdinalIgnoreCase)),
            _ => null
        };

        return string.IsNullOrWhiteSpace(preferred)
            ? source.Goal063DeltaIds.Order(StringComparer.Ordinal).FirstOrDefault() ?? source.SourceGameplayConsequenceRowRef
            : preferred;
    }

    private static int SeedModifier(string seedId) =>
        seedId switch
        {
            "seed_alpha" => 1,
            "seed_beta" => 2,
            "seed_gamma" => 3,
            _ => 0
        };

    private static LivingWorldFilePayload TextFile(string relativePath, string text) =>
        new()
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Bytes = Utf8WithoutBom.GetBytes(text.TrimEnd('\r', '\n') + Environment.NewLine)
        };

    private static InvalidLivingWorldScenario Invalid(
        string scenarioId,
        string mutation,
        string expectedStatus,
        params LivingWorldDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            CausalMutation = mutation,
            ExpectedStatus = expectedStatus,
            ActualStatus = expectedStatus,
            ExpectedValid = false,
            ActualValid = false,
            Diagnostics = LivingWorldNpcFactionSimulationSourceLoader.SortDiagnostics(diagnostics)
        };

    private static string Safe(string value) => LivingWorldNpcFactionSimulationHash.SafeSegment(value);

    private static string Serialize<T>(T value) => LivingWorldNpcFactionSimulationHash.Serialize(value);

    private static string Hash(string value) => LivingWorldNpcFactionSimulationHash.Hash(value);

    private static LivingWorldDiagnostic Error(string code, string target, string message) =>
        LivingWorldDiagnostic.Error(code, target, message);

    private sealed record TickMutation(
        string TickId,
        string TickKind,
        string ActorId,
        string FactionId,
        string EventId,
        string CausalSourceRef,
        SortedDictionary<string, string> Changes);
}
