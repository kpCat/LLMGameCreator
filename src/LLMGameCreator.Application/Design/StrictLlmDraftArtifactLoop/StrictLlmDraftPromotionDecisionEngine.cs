namespace LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;

public sealed class StrictLlmDraftPromotionDecisionEngine
{
    public IReadOnlyList<StrictLlmDraftPromotionDecision> Decide(
        IReadOnlyList<StrictLlmDraftRequest> requests,
        IReadOnlyList<StrictLlmDraftCandidateEnvelope> candidates)
    {
        var decisions = new List<StrictLlmDraftPromotionDecision>();
        foreach (var candidate in candidates.OrderBy(item => item.CandidateId, StringComparer.Ordinal))
        {
            var diagnostics = StrictLlmDraftArtifactLoopValidator.ValidateCandidates(requests, [candidate])
                .Where(item => item.Target == candidate.CandidateId || item.Target.StartsWith(candidate.CandidateId + ":", StringComparison.Ordinal))
                .ToList();

            if (candidate.Status != "quarantined")
            {
                diagnostics.Add(StrictLlmDraftArtifactLoopValidator.Diagnostic(
                    "error",
                    "strict_draft.promotion.requires_quarantine",
                    candidate.CandidateId,
                    "Promotion engine accepts only quarantined candidates."));
            }

            var blocking = StrictLlmDraftArtifactLoopValidator.SortDiagnostics(diagnostics.Where(item => item.Severity == "error"));
            if (blocking.Count == 0)
            {
                decisions.Add(new StrictLlmDraftPromotionDecision
                {
                    CandidateId = candidate.CandidateId,
                    RequestId = candidate.RequestId,
                    TargetDraftArtifactId = BuildTargetDraftArtifactId(candidate),
                    Promoted = true,
                    Reasons = ["valid_quarantined_candidate", "deterministic_promotion_engine_decision", "provenance_preserved"],
                    Diagnostics = [],
                    PreservedProvenanceId = candidate.ProvenanceId,
                    Status = "promoted"
                });
                continue;
            }

            var fixable = StrictLlmDraftRepairPlanner.IsFixableForPromotion(blocking);
            decisions.Add(new StrictLlmDraftPromotionDecision
            {
                CandidateId = candidate.CandidateId,
                RequestId = candidate.RequestId,
                TargetDraftArtifactId = string.Empty,
                Promoted = false,
                Reasons = fixable
                    ? ["blocking_diagnostics_fixable", "repair_required_before_promotion"]
                    : ["blocking_diagnostics_not_fixable", "candidate_rejected_before_promotion"],
                Diagnostics = blocking,
                PreservedProvenanceId = candidate.ProvenanceId,
                Status = fixable ? "repair_required" : "rejected"
            });
        }

        return decisions.OrderBy(item => item.CandidateId, StringComparer.Ordinal).ToList();
    }

    private static string BuildTargetDraftArtifactId(StrictLlmDraftCandidateEnvelope candidate)
    {
        var suffix = candidate.CandidateId
            .Replace("candidate/", string.Empty, StringComparison.Ordinal)
            .Replace('/', '-')
            .Replace('_', '-');
        return $"draft-artifact/{candidate.DraftFamilyId}/{suffix}";
    }
}
