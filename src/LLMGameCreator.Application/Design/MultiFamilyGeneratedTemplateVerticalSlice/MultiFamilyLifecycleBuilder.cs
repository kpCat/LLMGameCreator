using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;

namespace LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class MultiFamilyLifecycleBuilder
{
    public IReadOnlyList<FamilyLifecyclePlan> BuildPlans(
        MultiFamilySourceBundle source,
        FamilyTemplateCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(catalog);

        return catalog.Families
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(entry => BuildPlan(source, entry))
            .ToList();
    }

    public SharedLifecycleContract BuildSharedContract(IReadOnlyList<FamilyLifecyclePlan> plans)
    {
        var rows = plans
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(plan => new FamilyLifecycleContractRow
            {
                FamilyId = plan.FamilyId,
                FamilyExtensionSchemaId = plan.FamilyExtension.ExtensionSchemaId,
                PhaseIds = plan.LifecyclePhases,
                OnlyFamilyExtensionDiffers = plan.UnscopedFamilySpecificFields.Count == 0,
                ArchitectureForked = plan.ArchitectureForkAttempted
            })
            .ToList();
        var shared = MultiFamilyGeneratedTemplateVocabulary.SharedLifecyclePhases.ToList();
        var passed = rows.Count == 3
            && rows.All(row => row.PhaseIds.SequenceEqual(shared, StringComparer.Ordinal))
            && rows.All(row => row.OnlyFamilyExtensionDiffers)
            && rows.All(row => !row.ArchitectureForked);

        return new SharedLifecycleContract
        {
            SharedPhaseIds = shared,
            FamilyCount = rows.Count,
            Passed = passed,
            SharedPhaseHash = MultiFamilyGeneratedTemplateHash.Hash(string.Join("|", shared)),
            Families = rows,
            Diagnostics = passed
                ? [MultiFamilyGeneratedTemplateDiagnostic.Info("goal043.lifecycle.shared_contract.passed", "shared_lifecycle_contract", "All family plans use the same lifecycle phases and keep family-specific data inside extension sections.")]
                : [MultiFamilyGeneratedTemplateDiagnostic.Error("goal043.lifecycle.shared_contract.failed", "shared_lifecycle_contract", "Family lifecycle rows must not fork phases or architecture.")]
        };
    }

    public PreviewExportConsumptionMatrix BuildPreviewExportConsumptionMatrix(
        MultiFamilySourceBundle source,
        IReadOnlyList<FamilyLifecyclePlan> plans)
    {
        var rows = plans
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(plan =>
            {
                var payload = source.Goal040PayloadsByScenario[plan.ScenarioId];
                var fileName = ChunkedRuntimePreviewExportVocabulary.PayloadFileNamesByScenario[plan.ScenarioId];
                return new PreviewExportConsumptionRow
                {
                    FamilyId = plan.FamilyId,
                    ScenarioId = plan.ScenarioId,
                    Goal040PayloadFileName = fileName,
                    Goal040PayloadHash = payload.PayloadHash,
                    CorePayloadSchemaId = payload.CorePayloadSchemaId,
                    FamilyLensFound = payload.FamilyLensViews.Any(view => view.FamilyLensId == plan.FamilyId),
                    TransformedIntoLifecyclePlan = plan.PreviewExportConsumerRefs.Count > 0
                        && plan.LoopCommands.Count > 0
                        && !string.Equals(plan.SourceReferences.FirstOrDefault(item => item.SourceGoal == "Goal040")?.ArtifactHash, MultiFamilyGeneratedTemplateHash.Hash(MultiFamilyGeneratedTemplateHash.Serialize(plan)), StringComparison.Ordinal),
                    PayloadCopiedWithoutTransformation = false
                };
            })
            .ToList();

        return new PreviewExportConsumptionMatrix
        {
            FamilyCount = rows.Count,
            SourceGoal040PreviewExportConsumed = rows.All(row => row.FamilyLensFound && row.CorePayloadSchemaId == MultiFamilyGeneratedTemplateVocabulary.CorePayloadSchemaId),
            Passed = rows.Count == 3
                && rows.All(row => row.FamilyLensFound)
                && rows.All(row => row.TransformedIntoLifecyclePlan)
                && rows.All(row => !row.PayloadCopiedWithoutTransformation),
            Rows = rows
        };
    }

    public MultiFamilyRegressionMatrix BuildRegressionMatrix(
        IReadOnlyList<FamilyLifecyclePlan> plans,
        IReadOnlyList<FamilySimulatableLoopProof> proofs,
        SharedLifecycleContract contract,
        PreviewExportConsumptionMatrix previewExport)
    {
        var proofByFamily = proofs.ToDictionary(item => item.FamilyId, StringComparer.Ordinal);
        var rows = plans
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .Select(plan => new MultiFamilyRegressionRow
            {
                FamilyId = plan.FamilyId,
                ScenarioId = plan.ScenarioId,
                UsesSharedLifecycleContract = plan.SharedLifecycleContractId == MultiFamilyGeneratedTemplateVocabulary.SharedLifecycleContractId
                    && plan.LifecyclePhases.SequenceEqual(MultiFamilyGeneratedTemplateVocabulary.SharedLifecyclePhases, StringComparer.Ordinal),
                UsesFamilyScopedExtensionOnly = plan.UnscopedFamilySpecificFields.Count == 0 && !plan.ArchitectureForkAttempted,
                SimulatableLoopProofPassed = proofByFamily.TryGetValue(plan.FamilyId, out var proof)
                    && proof.StateChanged
                    && proof.FamilySpecificMinimumsPassed
                    && proof.BlockedInvalidAction.Blocked,
                SourceGoal040PreviewExportConsumed = previewExport.Rows.Any(row => row.FamilyId == plan.FamilyId && row.FamilyLensFound)
            })
            .ToList();

        return new MultiFamilyRegressionMatrix
        {
            FamilyCount = rows.Count,
            LifecyclePlanCount = plans.Count,
            SimulatableLoopProofCount = proofs.Count,
            SharedLifecycleContractPassed = contract.Passed,
            FamilySpecificMinimumsPassed = proofs.Count == 3 && proofs.All(item => item.FamilySpecificMinimumsPassed),
            PreviewExportConsumptionPassed = previewExport.Passed,
            NoArchitectureForks = rows.All(item => item.UsesSharedLifecycleContract && item.UsesFamilyScopedExtensionOnly),
            Passed = rows.Count == 3
                && contract.Passed
                && previewExport.Passed
                && proofs.Count == 3
                && proofs.All(item => item.StateChanged && item.FamilySpecificMinimumsPassed)
                && rows.All(item => item.UsesSharedLifecycleContract && item.UsesFamilyScopedExtensionOnly && item.SourceGoal040PreviewExportConsumed),
            Rows = rows
        };
    }

    private static FamilyLifecyclePlan BuildPlan(
        MultiFamilySourceBundle source,
        FamilyTemplateCatalogEntry entry)
    {
        var payload = source.Goal040PayloadsByScenario[entry.ScenarioId];
        var plan = source.Goal039PlansByScenario[entry.ScenarioId];
        var draftLuaRefs = source.SourceArtifactRefs
            .Where(item => item.SourceGoal is "Goal034" or "Goal035" or "Goal036" or "Goal037")
            .OrderBy(item => SourceGoalOrder(item.SourceGoal))
            .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
            .ThenBy(item => item.EvidenceRef, StringComparer.Ordinal)
            .ToList();
        var traversalRefs = source.SourceArtifactRefs
            .Where(item => item.SourceGoal is "Goal038" or "Goal039")
            .Where(item => item.ArtifactFamily.Contains("world", StringComparison.Ordinal)
                || item.ArtifactFamily.Contains("map", StringComparison.Ordinal)
                || item.ArtifactFamily.Contains("chunk", StringComparison.Ordinal)
                || item.ArtifactFamily.Contains("traversal", StringComparison.Ordinal)
                || item.ArtifactFamily.Contains("replay", StringComparison.Ordinal)
                || item.ArtifactFamily.Contains("save_load", StringComparison.Ordinal))
            .OrderBy(item => SourceGoalOrder(item.SourceGoal))
            .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
            .ToList();
        var previewRefs = source.SourceArtifactRefs
            .Where(item => item.SourceGoal == "Goal040")
            .OrderBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
            .ToList();

        return new FamilyLifecyclePlan
        {
            FamilyId = entry.FamilyId,
            ScenarioId = entry.ScenarioId,
            ProfileId = entry.ProfileId,
            DeterministicOrderingKey = entry.DeterministicOrderingKey,
            LifecyclePhases = MultiFamilyGeneratedTemplateVocabulary.SharedLifecyclePhases,
            SelectedFeatureRefs = entry.SelectedFeatureRefs,
            SelectedIntentionRefs = entry.SelectedIntentionRefs,
            DraftLuaExpansionSourceRefs = draftLuaRefs,
            RegionChunkTraversalSourceRefs = traversalRefs,
            PreviewExportConsumerRefs = previewRefs,
            SourceReferences = draftLuaRefs.Concat(traversalRefs).Concat(previewRefs)
                .GroupBy(item => item.SourceGoal + "|" + item.EvidenceRef + "|" + item.ArtifactRelativePath, StringComparer.Ordinal)
                .Select(item => item.First())
                .OrderBy(item => SourceGoalOrder(item.SourceGoal))
                .ThenBy(item => item.ArtifactRelativePath, StringComparer.Ordinal)
                .ThenBy(item => item.EvidenceRef, StringComparer.Ordinal)
                .ToList(),
            FamilyExtension = BuildExtension(entry.FamilyId, payload, plan),
            LoopCommands = BuildLoopCommands(entry.FamilyId, payload, plan),
            ValidationTrace = BuildValidationTrace(entry.FamilyId),
            BoundaryClaims = new FamilyTemplateBoundaryClaims()
        };
    }

    private static FamilySpecificExtension BuildExtension(
        string familyId,
        ChunkedPreviewPayload payload,
        LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal.RuntimeChunkTraversalPlan plan)
    {
        var firstRoute = payload.TraversalRoute.First();
        var lastRoute = payload.TraversalRoute.Last();
        var commonValues = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["corePayloadSchemaId"] = payload.CorePayloadSchemaId,
            ["finiteMapId"] = payload.FiniteMapId,
            ["firstChunkId"] = firstRoute.ChunkId,
            ["lastChunkId"] = lastRoute.ChunkId,
            ["replaySeed"] = payload.ReplaySeed,
            ["startRegionId"] = plan.StartRegionId,
            ["targetRegionId"] = lastRoute.RegionId,
            ["worldGraphId"] = payload.WorldGraphId
        };

        return familyId switch
        {
            "map_panel_rpg" => new FamilySpecificExtension
            {
                FamilyId = familyId,
                ExtensionSchemaId = MultiFamilyGeneratedTemplateCatalog.FamilyExtensionSchemaId(familyId),
                PresentationMarkers = ["panel_sequence", "focused_target_panel", "quest_progress_strip"],
                LoopMarkers = MultiFamilyGeneratedTemplateCatalog.RequiredFamilyMarkers(familyId),
                Values = Add(commonValues,
                    ("focusedTargetId", payload.LandmarkDiscoveryIds.FirstOrDefault() ?? lastRoute.LandmarkId),
                    ("questEventId", "quest-event/" + payload.ScenarioId + "/panel-progress"),
                    ("rewardItemId", "item/" + payload.ScenarioId + "/review-token"))
            },
            "survival_sandbox" => new FamilySpecificExtension
            {
                FamilyId = familyId,
                ExtensionSchemaId = MultiFamilyGeneratedTemplateCatalog.FamilyExtensionSchemaId(familyId),
                PresentationMarkers = ["hazard_meter", "resource_strip", "crafting_transition"],
                LoopMarkers = MultiFamilyGeneratedTemplateCatalog.RequiredFamilyMarkers(familyId),
                Values = Add(commonValues,
                    ("hazardId", "hazard/" + payload.ScenarioId + "/chunk-weather"),
                    ("resourceId", "resource/" + payload.ScenarioId + "/salvage"),
                    ("craftRecipeId", "recipe/" + payload.ScenarioId + "/field-filter"))
            },
            "first_person_grid_dungeon" => new FamilySpecificExtension
            {
                FamilyId = familyId,
                ExtensionSchemaId = MultiFamilyGeneratedTemplateCatalog.FamilyExtensionSchemaId(familyId),
                PresentationMarkers = ["party_blob_facing", "corridor_room_step", "locked_route_pressure"],
                LoopMarkers = MultiFamilyGeneratedTemplateCatalog.RequiredFamilyMarkers(familyId),
                Values = Add(commonValues,
                    ("initialFacing", "east"),
                    ("lockedRouteId", "locked-route/" + payload.ScenarioId + "/underway-gate"),
                    ("encounterPressureId", "encounter/" + payload.ScenarioId + "/threshold-guard"))
            },
            _ => new FamilySpecificExtension
            {
                FamilyId = familyId,
                ExtensionSchemaId = MultiFamilyGeneratedTemplateCatalog.FamilyExtensionSchemaId(familyId),
                Values = commonValues
            }
        };
    }

    private static IReadOnlyDictionary<string, string> Add(
        SortedDictionary<string, string> baseValues,
        params (string Key, string Value)[] values)
    {
        var result = new SortedDictionary<string, string>(baseValues, StringComparer.Ordinal);
        foreach (var pair in values)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }

    private static IReadOnlyList<FamilyLoopCommand> BuildLoopCommands(
        string familyId,
        ChunkedPreviewPayload payload,
        LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal.RuntimeChunkTraversalPlan plan)
    {
        var first = payload.TraversalRoute.First();
        var target = payload.TraversalRoute.Last();
        return familyId switch
        {
            "map_panel_rpg" =>
            [
                Command(1, familyId, "move_to_region", target.RegionId, target.ChunkId, "panel_travel", "movement_traversal_marker"),
                Command(2, familyId, "claim_quest_reward", "quest-event/" + payload.ScenarioId + "/panel-progress", "", "premature_reward_claim", "blocked_invalid_action_marker", "blocked"),
                Command(3, familyId, "focus_target", payload.LandmarkDiscoveryIds.FirstOrDefault() ?? target.LandmarkId, "", "npc_or_encounter_item_focus", "focused_target_marker"),
                Command(4, familyId, "obtain_item", "item/" + payload.ScenarioId + "/review-token", "", "reward_marker", "item_reward_marker"),
                Command(5, familyId, "progress_quest", "quest-event/" + payload.ScenarioId + "/panel-progress", "", "progress=1", "quest_event_progress_marker")
            ],
            "survival_sandbox" =>
            [
                Command(1, familyId, "observe_hazard", "hazard/" + payload.ScenarioId + "/chunk-weather", first.ChunkId, "hazard_visible", "hazard_resource_observation_marker"),
                Command(2, familyId, "collect_resource", "resource/" + payload.ScenarioId + "/salvage", target.ChunkId, "2", "chunk_context_state_change_marker"),
                Command(3, familyId, "consume_resource", "resource/" + payload.ScenarioId + "/salvage", "", "1", "collect_consume_craft_survival_marker"),
                Command(4, familyId, "craft_item", "recipe/" + payload.ScenarioId + "/field-filter", "", "1", "collect_consume_craft_survival_marker"),
                Command(5, familyId, "consume_resource", "resource/" + payload.ScenarioId + "/salvage", "", "2", "blocked_invalid_action_marker", "blocked")
            ],
            "first_person_grid_dungeon" =>
            [
                Command(1, familyId, "orient_party", "party/blob/" + payload.ScenarioId, "", "east", "orientation_corridor_room_marker"),
                Command(2, familyId, "enter_locked_route", "locked-route/" + payload.ScenarioId + "/underway-gate", "", "without_key", "blocked_invalid_action_marker", "blocked"),
                Command(3, familyId, "move_corridor", target.RegionId, target.ChunkId, "corridor_step", "party_blob_traversal_marker"),
                Command(4, familyId, "encounter_pressure", "encounter/" + payload.ScenarioId + "/threshold-guard", "", "active", "encounter_locked_route_pressure_marker"),
                Command(5, familyId, "acquire_key", "key/" + payload.ScenarioId + "/underway", "", "acquired", "encounter_locked_route_pressure_marker"),
                Command(6, familyId, "enter_locked_route", "locked-route/" + payload.ScenarioId + "/underway-gate", "", "with_key", "party_blob_traversal_marker")
            ],
            _ => [Command(1, familyId, "noop", plan.StartRegionId, "", "", "unknown_marker")]
        };
    }

    private static FamilyLoopCommand Command(
        int order,
        string familyId,
        string commandType,
        string targetId,
        string secondaryTargetId,
        string value,
        string marker,
        string expectedStatus = "applied") =>
        new()
        {
            Order = order,
            CommandId = $"cmd/{familyId}/{order:000}/{commandType}",
            CommandType = commandType,
            TargetId = targetId,
            SecondaryTargetId = secondaryTargetId,
            Value = value,
            FamilyMarker = marker,
            ExpectedStatus = expectedStatus
        };

    private static IReadOnlyList<FamilyValidationTraceEntry> BuildValidationTrace(string familyId) =>
    [
        new() { Order = 1, TraceId = "trace/" + familyId + "/source_refs", Message = "Goal 037-040 source references are bound." },
        new() { Order = 2, TraceId = "trace/" + familyId + "/shared_lifecycle", Message = "Shared lifecycle phases are reused." },
        new() { Order = 3, TraceId = "trace/" + familyId + "/family_extension_scope", Message = "Family-specific fields are scoped to the extension section." },
        new() { Order = 4, TraceId = "trace/" + familyId + "/simulatable_loop", Message = "Application-owned loop proof is required before review." }
    ];

    private static int SourceGoalOrder(string sourceGoal) =>
        sourceGoal switch
        {
            "Goal034" => 34,
            "Goal035" => 35,
            "Goal036" => 36,
            "Goal037" => 37,
            "Goal038" => 38,
            "Goal039" => 39,
            "Goal040" => 40,
            _ => 999
        };
}
