namespace LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;

public sealed class FullGeneratorReviewPromotionWorkflow
{
    public FullGeneratorReviewPromotionLedger BuildLedger(
        FullGeneratorSourceBundle source,
        FullGeneratorDryRunManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manifest);

        var transitions = new List<FullGeneratorReviewTransitionRecord>();
        foreach (var family in manifest.FamilySourceSummaries.OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal))
        {
            transitions.Add(Transition(
                family,
                1,
                "candidate_loaded",
                "validated",
                "programmatic",
                "validated_for_dry_run",
                family.Goal043PlanRef,
                family.Goal043PlanHash,
                "goal047.review.candidate_validated"));
            transitions.Add(Transition(
                family,
                2,
                "validated",
                "approved_for_dry_run",
                "user_handoff",
                "approved_for_goal047_dry_run",
                "multi_family_generated_template_vertical_slice_verification",
                family.Goal043LoopProofHash,
                "goal047.review.goal043_handoff_accepted"));
            transitions.Add(Transition(
                family,
                3,
                "approved_for_dry_run",
                "promoted_to_preview_payload",
                "inherited",
                "promoted_to_runtime_preview_payload",
                family.Goal040PayloadRef,
                family.Goal040PayloadHash,
                "goal047.review.preview_payload_promoted"));
            transitions.Add(Transition(
                family,
                4,
                "promoted_to_preview_payload",
                "promoted_to_export_candidate",
                "programmatic",
                "promoted_to_export_candidate_without_media",
                "chunked-export-manifest.json",
                source.Goal040ExportManifest.ManifestHash,
                "goal047.review.export_candidate_promoted"));
        }

        var diagnostics = FullGeneratorWithoutMediaDryRunValidator.SortDiagnostics(
            ValidateTransitions(transitions));
        var passed = diagnostics.All(item => item.Severity != "error")
            && FullGeneratorWithoutMediaDryRunVocabulary.FamilyIds.All(familyId =>
                transitions.Any(item => item.FamilyId == familyId && item.AfterState == "promoted_to_export_candidate"));

        return new FullGeneratorReviewPromotionLedger
        {
            States = FullGeneratorWithoutMediaDryRunVocabulary.ReviewStates,
            TransitionTable = BuildTransitionTable(),
            Transitions = transitions
                .OrderBy(item => FamilyOrderingKey(item.FamilyId), StringComparer.Ordinal)
                .ThenBy(item => item.TransitionId, StringComparer.Ordinal)
                .ToList(),
            Deterministic = true,
            Passed = passed,
            FamilyCount = FullGeneratorWithoutMediaDryRunVocabulary.FamilyIds.Count,
            TransitionCount = transitions.Count,
            Diagnostics = diagnostics
        };
    }

    public static IReadOnlyList<FullGeneratorReviewTransitionRule> BuildTransitionTable() =>
    [
        Rule("candidate_loaded", "validated"),
        Rule("validated", "repair_required"),
        Rule("repair_required", "validated"),
        Rule("validated", "approved_for_dry_run"),
        Rule("approved_for_dry_run", "promoted_to_preview_payload"),
        Rule("promoted_to_preview_payload", "promoted_to_export_candidate"),
        Rule("candidate_loaded", "blocked", terminal: true),
        Rule("validated", "blocked", terminal: true),
        Rule("candidate_loaded", "rejected", terminal: true),
        Rule("validated", "rejected", terminal: true)
    ];

    public static IReadOnlyList<FullGeneratorDiagnostic> ValidateTransitions(
        IReadOnlyList<FullGeneratorReviewTransitionRecord> transitions)
    {
        var diagnostics = new List<FullGeneratorDiagnostic>();
        var transitionTable = BuildTransitionTable();
        var legal = transitionTable
            .Select(rule => rule.FromState + ">" + rule.ToState)
            .ToHashSet(StringComparer.Ordinal);

        if (transitions.Select(item => item.TransitionId).Distinct(StringComparer.Ordinal).Count() != transitions.Count)
        {
            diagnostics.Add(FullGeneratorDiagnostic.Error(
                "goal047.review.transition_id.duplicate",
                "review-promotion-ledger",
                "Review transition ids must be unique."));
        }

        foreach (var transition in transitions)
        {
            if (!FullGeneratorWithoutMediaDryRunVocabulary.ReviewStates.Contains(transition.BeforeState, StringComparer.Ordinal)
                || !FullGeneratorWithoutMediaDryRunVocabulary.ReviewStates.Contains(transition.AfterState, StringComparer.Ordinal))
            {
                diagnostics.Add(FullGeneratorDiagnostic.Error(
                    "goal047.review.state.unknown",
                    transition.TransitionId,
                    "Review transition states must come from the fixed Goal 047 state list."));
            }

            if (!legal.Contains(transition.BeforeState + ">" + transition.AfterState))
            {
                diagnostics.Add(FullGeneratorDiagnostic.Error(
                    "goal047.review.transition_order.invalid",
                    transition.TransitionId,
                    "Review transition is not present in the deterministic transition table."));
            }

            if (string.IsNullOrWhiteSpace(transition.RequiredEvidenceHash))
            {
                diagnostics.Add(FullGeneratorDiagnostic.Error(
                    "goal047.review.evidence_hash.missing",
                    transition.TransitionId,
                    "Review transitions require a source evidence hash."));
            }

            if (string.IsNullOrWhiteSpace(transition.ProvenanceKind))
            {
                diagnostics.Add(FullGeneratorDiagnostic.Error(
                    "goal047.review.provenance.missing",
                    transition.TransitionId,
                    "Review transitions require provenance kind."));
            }
        }

        foreach (var group in transitions.GroupBy(item => item.FamilyId, StringComparer.Ordinal))
        {
            var states = group
                .OrderBy(item => item.TransitionId, StringComparer.Ordinal)
                .Select(item => item.BeforeState + ">" + item.AfterState)
                .ToList();
            var expected =
                "candidate_loaded>validated|validated>approved_for_dry_run|approved_for_dry_run>promoted_to_preview_payload|promoted_to_preview_payload>promoted_to_export_candidate";
            if (!string.Equals(string.Join("|", states), expected, StringComparison.Ordinal))
            {
                diagnostics.Add(FullGeneratorDiagnostic.Error(
                    "goal047.review.transition_sequence.invalid",
                    group.Key,
                    "Each family must follow the same review/promotion transition sequence."));
            }
        }

        return FullGeneratorWithoutMediaDryRunValidator.SortDiagnostics(diagnostics);
    }

    private static FullGeneratorReviewTransitionRecord Transition(
        FullGeneratorFamilySourceSummary family,
        int order,
        string before,
        string after,
        string provenance,
        string decision,
        string sourceArtifactId,
        string evidenceHash,
        string infoCode) =>
        new()
        {
            TransitionId = $"review/{family.FamilyId}/{order:000}-{after}",
            FamilyId = family.FamilyId,
            SourceArtifactId = sourceArtifactId,
            BeforeState = before,
            AfterState = after,
            RequiredEvidenceHash = evidenceHash,
            ProvenanceKind = provenance,
            PromotionDecision = decision,
            Diagnostics =
            [
                FullGeneratorDiagnostic.Info(
                    infoCode,
                    family.FamilyId,
                    $"{family.FamilyId} transitioned from {before} to {after}.")
            ]
        };

    private static FullGeneratorReviewTransitionRule Rule(
        string fromState,
        string toState,
        bool terminal = false) =>
        new() { FromState = fromState, ToState = toState, Terminal = terminal };

    private static string FamilyOrderingKey(string familyId) =>
        familyId switch
        {
            "map_panel_rpg" => "001-map-panel-rpg",
            "survival_sandbox" => "002-survival-sandbox",
            "first_person_grid_dungeon" => "003-first-person-grid-dungeon",
            _ => "999-" + familyId
        };
}
