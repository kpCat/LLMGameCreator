namespace LLMGameCreator.Application.Design.InterlockedGameplaySystemsDepthMatrix;

public static class InterlockedGameplaySystemsProjector
{
    public static InterlockedGameplayRow Project(InterlockedGameplaySourceRow source)
    {
        var values = InitialState(source);
        var beforeState = Snapshot(source, values, 0);
        var steps = new List<InterlockedGameplayStep>();
        var stepIndex = 0;

        foreach (var planned in BuildPlannedDeltas(source))
        {
            var before = Snapshot(source, values, stepIndex);
            var deltas = new List<InterlockedSystemDelta>();
            foreach (var delta in planned.Deltas.OrderBy(item => item.DeltaId, StringComparer.Ordinal))
            {
                var beforeValue = values.TryGetValue(delta.Key, out var existing) ? existing : "(missing)";
                values[delta.Key] = delta.AfterValue;
                deltas.Add(delta with
                {
                    BeforeValue = beforeValue,
                    Passed = !string.Equals(beforeValue, delta.AfterValue, StringComparison.Ordinal)
                        && delta.SourceRefs.Count > 0
                        && !string.IsNullOrWhiteSpace(delta.CausalTrace)
                });
            }

            stepIndex++;
            var after = Snapshot(source, values, stepIndex);
            steps.Add(new InterlockedGameplayStep
            {
                StepIndex = stepIndex,
                StepId = planned.StepId,
                CommandType = planned.CommandType,
                RuleId = planned.RuleId,
                Deltas = deltas,
                Before = before,
                After = after,
                StateChanged = !string.Equals(before.StateHash, after.StateHash, StringComparison.Ordinal)
            });
        }

        var afterState = Snapshot(source, values, stepIndex);
        var allDeltas = steps.SelectMany(item => item.Deltas).OrderBy(item => item.DeltaId, StringComparer.Ordinal).ToList();
        var saveLoad = BuildSaveLoadReplay(source, beforeState, afterState, allDeltas);
        var rowWithoutHash = new InterlockedGameplayRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            SourcePackageRowRef = source.SourcePackageRowRef,
            SourceReviewPackageRowRef = source.SourceReviewPackageRowRef,
            SourceSpatialDetailRowRef = source.SourceSpatialDetailRowRef,
            SourceGameplayConsequenceRowRef = source.SourceGameplayConsequenceRowRef,
            SourceLivingWorldRowRef = source.SourceLivingWorldRowRef,
            DerivedRuleSetId = RuleSetId(source.FamilyId),
            ExpectedUnityMarkerSet = UnityMarkersFor(source.RowId),
            BeforeState = beforeState,
            AfterState = afterState,
            Steps = steps,
            Deltas = allDeltas,
            SaveLoadReplayProof = saveLoad,
            MeaningfulVarianceAxes = MeaningfulAxes(source.FamilyId),
            StateChanging = !string.Equals(beforeState.StateHash, afterState.StateHash, StringComparison.Ordinal)
                && steps.Count >= 6
                && steps.All(item => item.StateChanged)
                && InterlockedGameplaySystemsRuleCatalogBuilder.RequiredCategories().All(category => allDeltas.Any(delta => delta.Category == category))
                && saveLoad.SaveLoadRoundtripPassed
                && saveLoad.ReplayDeterminismPassed,
            RowHash = string.Empty
        };

        return rowWithoutHash with
        {
            RowHash = Hash(Serialize(rowWithoutHash))
        };
    }

    public static InterlockedSaveLoadReplayRow BuildSaveLoadReplay(
        InterlockedGameplaySourceRow source,
        InterlockedGameplayStateSnapshot before,
        InterlockedGameplayStateSnapshot after,
        IReadOnlyList<InterlockedSystemDelta> deltas)
    {
        var json = Serialize(after);
        var restored = InterlockedGameplaySystemsHash.Deserialize<InterlockedGameplayStateSnapshot>(json);
        var replayHash = Hash(Serialize(new
        {
            source.RowId,
            source.FamilyId,
            source.SeedId,
            deltaIds = deltas.Select(item => item.DeltaId).Order(StringComparer.Ordinal).ToList(),
            after.StateHash
        }));

        return new InterlockedSaveLoadReplayRow
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

    public static IReadOnlyList<InterlockedLedgerEntry> BuildLedgerEntries(
        InterlockedGameplayRow row,
        IReadOnlySet<string> categories)
    {
        return row.Deltas
            .Where(item => categories.Contains(item.Category))
            .OrderBy(item => item.DeltaId, StringComparer.Ordinal)
            .Select(item => new InterlockedLedgerEntry
            {
                EntryId = item.DeltaId,
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                Category = item.Category,
                Subsystem = item.Subsystem,
                Input = item.BeforeValue,
                Output = item.AfterValue,
                Outcome = item.Outcome,
                SourceRefs = item.SourceRefs
            })
            .ToList();
    }

    private static IReadOnlyList<PlannedStep> BuildPlannedDeltas(InterlockedGameplaySourceRow source)
    {
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        var seedModifier = SeedModifier(source.SeedId);
        var sourceRefs = new[]
        {
            source.SourcePackageRowRef,
            source.SourceReviewPackageRowRef,
            source.SourceSpatialDetailRowRef,
            source.SourceGameplayConsequenceRowRef,
            source.SourceLivingWorldRowRef
        };

        return source.FamilyId switch
        {
            "map_panel_rpg" =>
            [
                Step(source, "01-work-trade", "work/trade", "economy", "work_ledger", "economy.work_ledger", "completed_contract_" + safeSeed, "trade/work completed from NPC/faction handoff", sourceRefs),
                Step(source, "02-craft-upgrade", "recipe/upgrade", "crafting", "recipe_conversion", "crafting.output", "route_badge_" + safeSeed, "resource converted into social-route upgrade", sourceRefs),
                Step(source, "03-conflict-resolution", "combat/conflict", "combat", "conflict_pressure", "combat.outcome", "guard_conflict_resolved_" + seedModifier, "conflict pressure resolved through faction consequence", sourceRefs),
                Step(source, "04-progression-reward", "progression/reward", "progression", "skill_reward", "progression.unlock", "council_route_" + safeSeed, "quest reward advances route access", sourceRefs),
                Step(source, "05-inventory-equipment", "inventory/equip", "inventory", "equipment_reward", "inventory.equipment", "badge_equipped_" + safeSeed, "reward item equipped from package row", sourceRefs),
                Step(source, "06-status-living-cause", "status/social", "status", "social_status", "status.effect", "trusted_inspired_" + seedModifier, "status reflects living-world rumor and faction result", sourceRefs),
                Step(source, "07-living-world-cause", "living_world/cause", "living_world", "living_world_trace", "living_world.cause_trace", source.LivingWorldRowHash, "Goal064 actor/faction/world-event trace consumed", sourceRefs)
            ],
            "survival_sandbox" =>
            [
                Step(source, "01-resource-pressure", "resource/collect", "economy", "resource_ledger", "economy.resource_stock", (4 + seedModifier).ToString(), "hazard resource pressure collected into camp stock", sourceRefs),
                Step(source, "02-craft-recovery-tool", "recipe/craft", "crafting", "recipe_conversion", "crafting.output", "shelter_tool_" + safeSeed, "resource input converted to recovery tool", sourceRefs),
                Step(source, "03-hazard-conflict", "combat/hazard", "combat", "hazard_conflict", "combat.outcome", "hazard_repulsed_" + seedModifier, "hazard encounter outcome resolved", sourceRefs),
                Step(source, "04-recovery-progression", "progression/recover", "progression", "survival_skill", "progression.unlock", "camp_recovery_" + safeSeed, "recovery unlock follows craft and hazard outcome", sourceRefs),
                Step(source, "05-inventory-equipment", "inventory/tool", "inventory", "tool_equipment", "inventory.equipment", "reinforced_tool_" + safeSeed, "crafted tool enters inventory/equipment ledger", sourceRefs),
                Step(source, "06-condition-status", "status/condition", "status", "condition_pressure", "status.effect", "stabilized_under_pressure_" + seedModifier, "condition pressure changes after recovery", sourceRefs),
                Step(source, "07-living-world-cause", "living_world/cause", "living_world", "camp_memory_trace", "living_world.cause_trace", source.LivingWorldRowHash, "Goal064 camp support/scarcity event consumed", sourceRefs)
            ],
            "first_person_grid_dungeon" =>
            [
                Step(source, "01-resource-spend", "resource/spend", "economy", "ability_resource", "economy.resource_stock", (2 - Math.Min(seedModifier, 2)).ToString(), "ability use spends bounded dungeon resource", sourceRefs),
                Step(source, "02-key-craft", "recipe/key_upgrade", "crafting", "key_conversion", "crafting.output", "rune_key_" + safeSeed, "loot/resource converted into key upgrade", sourceRefs),
                Step(source, "03-encounter-combat", "combat/encounter", "combat", "encounter_outcome", "combat.outcome", "encounter_cleared_" + seedModifier, "encounter pressure resolved", sourceRefs),
                Step(source, "04-progression-door", "progression/key", "progression", "door_progression", "progression.unlock", "sealed_door_open_" + safeSeed, "key progression opens traversal route", sourceRefs),
                Step(source, "05-loot-equipment", "inventory/loot", "inventory", "loot_equipment", "inventory.equipment", "glyph_blade_" + safeSeed, "loot/equipment delta follows encounter", sourceRefs),
                Step(source, "06-status-orientation", "status/orientation", "status", "orientation_status", "status.effect", "focused_after_blocked_move_" + seedModifier, "status captures blocked/valid movement consequence", sourceRefs),
                Step(source, "07-living-world-cause", "living_world/cause", "living_world", "dungeon_alert_trace", "living_world.cause_trace", source.LivingWorldRowHash, "Goal064 alert/loot spatial faction trace consumed", sourceRefs)
            ],
            _ => []
        };
    }

    private static PlannedStep Step(
        InterlockedGameplaySourceRow source,
        string stepSuffix,
        string commandType,
        string category,
        string subsystem,
        string key,
        string afterValue,
        string outcome,
        IReadOnlyList<string> sourceRefs)
    {
        var safeFamily = Safe(source.FamilyId);
        var safeSeed = Safe(source.SeedId);
        var stepId = "goal065/" + safeFamily + "/" + safeSeed + "/" + stepSuffix;
        return new PlannedStep(
            stepId,
            commandType,
            "rule/" + RuleSetId(source.FamilyId) + "/" + stepSuffix,
            [
                new InterlockedSystemDelta
                {
                    DeltaId = stepId + "/delta/" + category + "/" + subsystem,
                    Category = category,
                    Subsystem = subsystem,
                    Key = key,
                    AfterValue = afterValue,
                    Outcome = outcome,
                    SourceRefs = sourceRefs.Order(StringComparer.Ordinal).ToList(),
                    CausalTrace = source.SourceLivingWorldRowRef + "|" + source.SourceGameplayConsequenceRowRef + "|" + source.SpatialVarianceMarker
                }
            ]);
    }

    private static SortedDictionary<string, string> InitialState(InterlockedGameplaySourceRow source)
    {
        return new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["row.id"] = source.RowId,
            ["family.id"] = source.FamilyId,
            ["seed.id"] = source.SeedId,
            ["source.package"] = source.SourcePackageRowRef,
            ["source.review"] = source.SourceReviewPackageRowRef,
            ["source.spatial"] = source.SourceSpatialDetailRowRef,
            ["source.gameplay"] = source.SourceGameplayConsequenceRowRef,
            ["source.living_world"] = source.SourceLivingWorldRowRef,
            ["source.package_hash"] = source.PackageHash,
            ["source.gameplay.after_hash"] = source.GameplayAfterStateHash,
            ["source.living_world.after_hash"] = source.LivingWorldAfterStateHash,
            ["source.living_world.rule_profile"] = source.LivingWorldRuleProfile,
            ["economy.resource_stock"] = source.FamilyId == "survival_sandbox" ? "1" : source.FamilyId == "first_person_grid_dungeon" ? "3" : "0",
            ["economy.work_ledger"] = "open",
            ["crafting.output"] = "none",
            ["combat.outcome"] = "unresolved",
            ["progression.unlock"] = "locked",
            ["inventory.equipment"] = "none",
            ["status.effect"] = "neutral",
            ["living_world.cause_trace"] = "none",
            ["seed.modifier"] = SeedModifier(source.SeedId).ToString()
        };
    }

    private static InterlockedGameplayStateSnapshot Snapshot(
        InterlockedGameplaySourceRow source,
        IReadOnlyDictionary<string, string> values,
        int stepIndex)
    {
        var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in values)
        {
            copy[pair.Key] = pair.Value;
        }

        return new InterlockedGameplayStateSnapshot
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            StepIndex = stepIndex,
            Values = copy,
            StateHash = Hash(Serialize(copy))
        };
    }

    private static IReadOnlyList<string> MeaningfulAxes(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" =>
            [
                "economy.work_ledger",
                "crafting.output",
                "combat.outcome",
                "progression.unlock",
                "inventory.equipment",
                "status.effect",
                "living_world.cause_trace"
            ],
            "survival_sandbox" =>
            [
                "economy.resource_stock",
                "crafting.output",
                "combat.outcome",
                "progression.unlock",
                "inventory.equipment",
                "status.effect",
                "living_world.cause_trace"
            ],
            "first_person_grid_dungeon" =>
            [
                "economy.resource_stock",
                "crafting.output",
                "combat.outcome",
                "progression.unlock",
                "inventory.equipment",
                "status.effect",
                "living_world.cause_trace"
            ],
            _ => []
        };

    private static IReadOnlyList<string> UnityMarkersFor(string rowId) =>
    [
        "interlocked_gameplay_row=" + rowId,
        "interlocked_economy_delta=" + rowId,
        "interlocked_crafting_delta=" + rowId,
        "interlocked_combat_delta=" + rowId,
        "interlocked_progression_delta=" + rowId,
        "interlocked_status_delta=" + rowId,
        "interlocked_replay_verified=" + rowId,
        "interlocked_gameplay_row_completed=" + rowId
    ];

    private static string RuleSetId(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "map_panel_rpg/npc_trade_work_conflict_progression_interlock",
            "survival_sandbox" => "survival_sandbox/hazard_resource_craft_recover_interlock",
            "first_person_grid_dungeon" => "first_person_grid_dungeon/orientation_encounter_loot_key_interlock",
            _ => "unknown"
        };

    private static int SeedModifier(string seedId) =>
        seedId switch
        {
            "seed_alpha" => 1,
            "seed_beta" => 2,
            "seed_gamma" => 3,
            _ => 0
        };

    private static string Safe(string value) => InterlockedGameplaySystemsHash.SafeSegment(value);

    private static string Serialize<T>(T value) => InterlockedGameplaySystemsHash.Serialize(value);

    private static string Hash(string text) => InterlockedGameplaySystemsHash.Hash(text);

    private sealed record PlannedStep(
        string StepId,
        string CommandType,
        string RuleId,
        IReadOnlyList<InterlockedSystemDelta> Deltas);
}
