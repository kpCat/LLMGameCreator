namespace LLMGameCreator.Application.Generation.Procedural;

public sealed record GeneratedCampaignRegionalEventCorrelationResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public static class GeneratedCampaignRegionalEventCorrelationService
{
    public static GeneratedCampaignRegionalEventCorrelationResult Validate(
        string packageSha256,
        GameProjectGeneratedCampaignRegionalEventSummary events,
        GameProjectGeneratedCampaignRelationshipSummary relationships)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageSha256);
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(relationships);
        var diagnostics = new List<string>();
        if (events.StrictProofSchemaVersion !=
            GameProjectGeneratedCampaignRegionalEventSummary
                .StrictProofSchema)
            diagnostics.Add(
                "generated_regional_event.history.strict_proof_schema");
        if (relationships.ArcQuestCount !=
            relationships.QualifiedArcQuestCount)
            diagnostics.Add(
                "generated_regional_event.history.qualified_arc_count");
        if (relationships.RelationshipBranchMatrixSha256 !=
            GeneratedCampaignChoiceCanonical.Hash(
                relationships.BranchQualifications))
            diagnostics.Add(
                "generated_regional_event.history.branch_matrix_hash");
        if (events.RelationshipBranchMatrixSha256 !=
            relationships.RelationshipBranchMatrixSha256)
            diagnostics.Add(
                "generated_regional_event.history.branch_matrix_correlation");
        if (events.ExactPackageSha256 != packageSha256)
            diagnostics.Add(
                "generated_regional_event.history.package_hash");

        var expectedCount = relationships.BranchQualifications.Count(item =>
            item.Available);
        if (expectedCount == 0)
        {
            if (events.Present || events.Status != "ABSENT"
                || !events.Passed || events.EventCount != 0
                || events.EventInventory.Count != 0
                || events.EventQualifications.Count != 0
                || events.RuntimeFrames.Count != 0
                || events.ReplaySignatures.Count != 0)
                diagnostics.Add(
                    "generated_regional_event.history.absent_graph");
            return Result(diagnostics);
        }

        if (!events.Present || !events.Passed
            || events.Status != "REGIONAL_EVENTS_CURRENT"
            || events.Overlay is not { Passed: true } overlay)
        {
            diagnostics.Add(
                "generated_regional_event.history.overlay_missing");
            return Result(diagnostics);
        }
        if (overlay.OutputPackageSha256 != packageSha256)
            diagnostics.Add(
                "generated_regional_event.history.overlay_package_hash");
        if (events.RegionalEventOverlaySha256 !=
            GeneratedCampaignChoiceCanonical.Hash(overlay))
            diagnostics.Add(
                "generated_regional_event.history.overlay_hash");
        if (!CanonicalEqual(events.EventInventory,
                overlay.Inventory))
            diagnostics.Add(
                "generated_regional_event.history.inventory_overlay");
        var inventoryHash =
            GeneratedCampaignChoiceCanonical.Hash(events.EventInventory);
        if (events.RegionalEventInventorySha256 != inventoryHash)
            diagnostics.Add(
                "generated_regional_event.history.inventory_hash");
        if (overlay.InventorySha256 != inventoryHash)
            diagnostics.Add(
                "generated_regional_event.history.overlay_inventory_hash");

        if (events.EventCount != expectedCount
            || events.EventCount != overlay.EventCount
            || events.EventCount != overlay.Bindings.Count
            || events.EventCount != events.EventInventory.Count
            || events.EventCount != events.EventQualifications.Count)
            diagnostics.Add(
                "generated_regional_event.history.graph_count");
        if (events.SupportGratitudeCount != events.EventInventory.Count(
                item => item.EventKind ==
                        GeneratedCampaignRegionalEventKind
                            .SUPPORT_GRATITUDE)
            || events.ChallengeAftermathCount !=
            events.EventInventory.Count(item => item.EventKind ==
                GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH)
            || events.RefusalFalloutCount !=
            events.EventInventory.Count(item => item.EventKind ==
                GeneratedCampaignRegionalEventKind.REFUSAL_FALLOUT))
            diagnostics.Add(
                "generated_regional_event.history.kind_counts");

        Unique(overlay.Bindings.Select(item => item.RegionalEventId),
            "binding", diagnostics);
        Unique(events.EventInventory.Select(item =>
            item.RegionalEventId), "inventory", diagnostics);
        Unique(events.EventQualifications.Select(item =>
            item.RegionalEventId), "qualification", diagnostics);

        foreach (var row in events.EventInventory)
        {
            if (row.EventSemanticFingerprint !=
                GeneratedCampaignRegionalEventInventoryService
                    .SemanticFingerprint(row))
                diagnostics.Add(
                    "generated_regional_event.history.semantic_fingerprint");
            var binding = overlay.Bindings.SingleOrDefault(item =>
                item.RegionalEventId == row.RegionalEventId);
            if (binding is null
                || !CanonicalEqual(
                    GeneratedCampaignRegionalEventInventoryService
                        .Create(binding), row))
                diagnostics.Add(
                    "generated_regional_event.history.binding_inventory");
        }

        foreach (var relationship in relationships
                     .RelationshipInventory)
        {
            var supportFact = relationships.BranchQualifications
                .SingleOrDefault(item =>
                    item.RelationshipId ==
                    relationship.RelationshipId
                    && item.Branch ==
                    GeneratedCampaignRelationshipBranch.SUPPORT);
            if (supportFact is { Available: true }
                && supportFact.ArcLength !=
                relationship.OrderedQuestSourceIds.Count)
                diagnostics.Add(
                    "generated_regional_event.history.support_arc_length");
        }

        foreach (var fact in relationships.BranchQualifications)
        {
            var kind = EventKind(fact.Branch);
            var bindings = overlay.Bindings.Where(item =>
                item.RelationshipId == fact.RelationshipId
                && item.RelationshipBranch == fact.Branch
                && item.EventKind == kind).ToList();
            var inventory = events.EventInventory.Where(item =>
                item.RelationshipId == fact.RelationshipId
                && item.RelationshipBranch == fact.Branch
                && item.EventKind == kind).ToList();
            var qualifications = events.EventQualifications.Where(item =>
                item.RelationshipId == fact.RelationshipId
                && item.RelationshipBranch == fact.Branch
                && item.EventKind == kind).ToList();
            var expected = fact.Available ? 1 : 0;
            if (bindings.Count != expected
                || inventory.Count != expected
                || qualifications.Count != expected)
                diagnostics.Add(
                    "generated_regional_event.history.branch_event_correlation");
        }

        var expectedSignatureCount = events.EventCount * 4;
        if (events.ReplaySignatures.Count != expectedSignatureCount
            || !CanonicalEqual(events.ReplaySignatures,
                events.EventQualifications.SelectMany(item =>
                    item.ReplaySignatures).ToList()))
            diagnostics.Add(
                "generated_regional_event.history.signature_inventory");
        foreach (var qualification in events.EventQualifications)
        {
            var signatures = qualification.ReplaySignatures;
            if (!qualification.LockedStatePassed
                || !qualification.AvailableStatePassed
                || !qualification.ResolvedStatePassed
                || !qualification.ExactlyOncePassed
                || !qualification.ReplayPassed
                || qualification.RuntimeStartCount != 4
                || signatures.Count != 4)
                diagnostics.Add(
                    "generated_regional_event.history.qualification");
            foreach (var route in Enum.GetValues<
                         GeneratedCampaignRegionalEventReplayRouteKind>())
            {
                var routeSignatures = signatures.Where(item =>
                        item.RouteKind == route)
                    .OrderBy(item => item.ReplayIndex).ToList();
                if (routeSignatures.Count != 2
                    || !routeSignatures.Select(item => item.ReplayIndex)
                        .SequenceEqual([1, 2]))
                {
                    diagnostics.Add(
                        "generated_regional_event.history.route_replays");
                    continue;
                }
                foreach (var signature in routeSignatures)
                {
                    var routeFrames = events.RuntimeFrames.Where(item =>
                            item.RegionalEventId ==
                            qualification.RegionalEventId
                            && item.RouteKind == route
                            && item.ReplayIndex ==
                            signature.ReplayIndex)
                        .OrderBy(item => item.SequenceIndex).ToList();
                    var validation =
                        GeneratedCampaignRegionalEventReplayService
                            .ValidateSignature(signature, routeFrames);
                    diagnostics.AddRange(validation.Diagnostics.Select(item =>
                        "generated_regional_event.history." + item[
                            "generated_regional_event.".Length..]));
                }
                var comparison =
                    GeneratedCampaignRegionalEventReplayService.Compare(
                        routeSignatures[0], routeSignatures[1]);
                diagnostics.AddRange(comparison.Diagnostics.Select(item =>
                    "generated_regional_event.history." + item[
                        "generated_regional_event.".Length..]));
            }
            var resolution = signatures.SingleOrDefault(item =>
                item.RouteKind ==
                GeneratedCampaignRegionalEventReplayRouteKind.RESOLUTION
                && item.ReplayIndex == 1);
            if (resolution is null
                || qualification.FinalStateHash !=
                resolution.FinalStateHash)
                diagnostics.Add(
                    "generated_regional_event.history.qualification_final_state");
        }
        if (events.RuntimeFrames.Any(item =>
                events.EventQualifications.All(qualification =>
                    qualification.RegionalEventId !=
                    item.RegionalEventId)))
            diagnostics.Add(
                "generated_regional_event.history.orphan_frame");
        if (events.RuntimeFrames.Any(item => !item.Passed))
            diagnostics.Add(
                "generated_regional_event.history.frame_failed");

        var finalState = GeneratedCampaignChoiceCanonical.Hash(
            events.EventQualifications.Select(item => new
            {
                item.RegionalEventId,
                item.FinalStateHash,
                ResolutionSignature = item.ReplaySignatures
                    .Single(signature => signature.RouteKind ==
                        GeneratedCampaignRegionalEventReplayRouteKind
                            .RESOLUTION
                        && signature.ReplayIndex == 1)
                    .SignatureSha256
            }).ToList());
        if (events.FinalStateHash != finalState)
            diagnostics.Add(
                "generated_regional_event.history.final_state");
        return Result(diagnostics);
    }

    private static GeneratedCampaignRegionalEventKind EventKind(
        GeneratedCampaignRelationshipBranch branch) => branch switch
    {
        GeneratedCampaignRelationshipBranch.SUPPORT =>
            GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE,
        GeneratedCampaignRelationshipBranch.CHALLENGE =>
            GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH,
        _ => GeneratedCampaignRegionalEventKind.REFUSAL_FALLOUT
    };

    private static void Unique(
        IEnumerable<string> ids,
        string dimension,
        ICollection<string> diagnostics)
    {
        var values = ids.ToList();
        if (values.Distinct(StringComparer.Ordinal).Count() != values.Count)
            diagnostics.Add(
                "generated_regional_event.history.duplicate_" + dimension);
    }

    private static bool CanonicalEqual<T>(T left, T right) =>
        GeneratedCampaignChoiceCanonical.Serialize(left) ==
        GeneratedCampaignChoiceCanonical.Serialize(right);

    private static GeneratedCampaignRegionalEventCorrelationResult Result(
        IEnumerable<string> diagnostics)
    {
        var values = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        return new GeneratedCampaignRegionalEventCorrelationResult
        {
            Passed = values.Count == 0,
            Diagnostics = values
        };
    }
}
