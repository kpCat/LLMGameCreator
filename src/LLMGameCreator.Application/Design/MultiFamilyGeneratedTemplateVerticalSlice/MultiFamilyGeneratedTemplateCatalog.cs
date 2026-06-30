using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;

namespace LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class MultiFamilyGeneratedTemplateCatalog
{
    public FamilyTemplateCatalog Build(MultiFamilySourceBundle source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var entries = MultiFamilyGeneratedTemplateVocabulary.FamilyIds
            .Select(familyId => BuildEntry(source, familyId))
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();

        return new FamilyTemplateCatalog
        {
            Accepted = false,
            Goal040AcceptedByUserHandoff = true,
            FamilyCount = entries.Count,
            SourceGoal037HybridExpansionConsumed = HasSourceGoal(source, "Goal037"),
            SourceGoal038WorldMapConsumed = HasSourceGoal(source, "Goal038"),
            SourceGoal039RuntimeTraversalConsumed = HasSourceGoal(source, "Goal039"),
            SourceGoal040PreviewExportConsumed = source.Goal040ConsumptionProof.Goal039RuntimeDeltasConsumed
                && source.Goal040ConsumptionProof.PreviewExportManifestReferencesPayloads,
            Families = entries,
            SourceArtifactRefs = source.SourceArtifactRefs,
            Diagnostics =
            [
                MultiFamilyGeneratedTemplateDiagnostic.Info(
                    "goal043.goal040_handoff.accepted",
                    MultiFamilyGeneratedTemplateVocabulary.FinalGate,
                    "Goal 040 is recorded as accepted by user handoff before producing Goal 043 evidence.")
            ]
        };
    }

    private static FamilyTemplateCatalogEntry BuildEntry(
        MultiFamilySourceBundle source,
        string familyId)
    {
        var scenarioId = MultiFamilyGeneratedTemplateVocabulary.ScenarioByFamilyId[familyId];
        var payload = source.Goal040PayloadsByScenario[scenarioId];
        var lens = payload.FamilyLensViews.Single(item => string.Equals(item.FamilyLensId, familyId, StringComparison.Ordinal));
        return new FamilyTemplateCatalogEntry
        {
            FamilyId = familyId,
            ScenarioId = scenarioId,
            ProfileId = payload.ProfileId,
            FamilyExtensionSchemaId = FamilyExtensionSchemaId(familyId),
            DeterministicOrderingKey = OrderingKey(familyId),
            LifecyclePlanFileName = MultiFamilyGeneratedTemplateEvidenceService.PlanFileName(familyId),
            LoopProofFileName = MultiFamilyGeneratedTemplateEvidenceService.LoopProofFileName(familyId),
            SourceGoal040PayloadFileName = ChunkedRuntimePreviewExportVocabulary.PayloadFileNamesByScenario[scenarioId],
            SourceGoal040PayloadHash = payload.PayloadHash,
            SelectedFeatureRefs = SelectedFeatureRefs(payload, lens),
            SelectedIntentionRefs = SelectedIntentionRefs(familyId, scenarioId),
            RequiredFamilyMarkers = RequiredFamilyMarkers(familyId)
        };
    }

    public static string OrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };

    public static string FamilyExtensionSchemaId(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "family_extension_map_panel_rpg_v1",
            "survival_sandbox" => "family_extension_survival_sandbox_v1",
            "first_person_grid_dungeon" => "family_extension_first_person_grid_dungeon_v1",
            _ => "family_extension_unknown_v1"
        };

    public static IReadOnlyList<string> RequiredFamilyMarkers(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" =>
            [
                "movement_traversal_marker",
                "focused_target_marker",
                "quest_event_progress_marker"
            ],
            "survival_sandbox" =>
            [
                "hazard_resource_observation_marker",
                "collect_consume_craft_survival_marker",
                "chunk_context_state_change_marker"
            ],
            "first_person_grid_dungeon" =>
            [
                "orientation_corridor_room_marker",
                "encounter_locked_route_pressure_marker",
                "party_blob_traversal_marker"
            ],
            _ => []
        };

    private static IReadOnlyList<string> SelectedFeatureRefs(
        ChunkedPreviewPayload payload,
        ChunkedFamilyLensPayloadView lens) =>
        lens.ExpectedConsumerNeeds
            .Concat(lens.RouteOrientationHints)
            .Concat(payload.SourceEvidence.Goal038EvidenceRefs.Select(item => item.EvidenceRef))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<string> SelectedIntentionRefs(string familyId, string scenarioId) =>
        familyId switch
        {
            "map_panel_rpg" =>
            [
                $"intent/{scenarioId}/panel_region_travel",
                $"intent/{scenarioId}/focused_npc_encounter_item_target",
                $"intent/{scenarioId}/quest_event_progress_marker"
            ],
            "survival_sandbox" =>
            [
                $"intent/{scenarioId}/hazard_resource_observation",
                $"intent/{scenarioId}/collect_consume_craft_transition",
                $"intent/{scenarioId}/chunk_context_state_change"
            ],
            "first_person_grid_dungeon" =>
            [
                $"intent/{scenarioId}/party_blob_orientation",
                $"intent/{scenarioId}/corridor_room_traversal",
                $"intent/{scenarioId}/locked_route_encounter_pressure"
            ],
            _ => []
        };

    private static bool HasSourceGoal(MultiFamilySourceBundle source, string sourceGoal) =>
        source.SourceArtifactRefs.Any(item => string.Equals(item.SourceGoal, sourceGoal, StringComparison.Ordinal));
}
