namespace LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;

public sealed class StrictLlmDraftRepairPlanner
{
    private static readonly IReadOnlySet<string> FixableCodes = new HashSet<string>(
        [
            "strict_draft.required_field.missing",
            "strict_draft.intent_trace.missing",
            "strict_draft.feature_trace.missing",
            "strict_draft.contract_trace.missing",
            "strict_draft.scope_trace.missing",
            "strict_draft.source_kind.mismatch"
        ],
        StringComparer.Ordinal);

    private static readonly IReadOnlySet<string> NonFixableCodes = new HashSet<string>(
        [
            "strict_draft.final_prose.forbidden",
            "strict_draft.boundary.leakage",
            "strict_draft.candidate.self_promoted",
            "strict_draft.contract.fake",
            "strict_draft.semantic_scope.fake",
            "strict_draft.request.unknown",
            "strict_draft.family.wrong",
            "strict_draft.scenario.incompatible",
            "strict_draft.candidate_count.over_budget",
            "strict_draft.order.nondeterministic"
        ],
        StringComparer.Ordinal);

    public IReadOnlyList<StrictLlmDraftRepairRequest> PlanRepairRequests(
        IReadOnlyList<StrictLlmDraftRequest> requests,
        IReadOnlyList<StrictLlmDraftCandidateEnvelope> candidates,
        IReadOnlyList<StrictLlmDraftDiagnostic> diagnostics,
        int retryNumber = 1,
        int maxRetryCount = 2)
    {
        var requestById = requests.ToDictionary(item => item.RequestId, StringComparer.Ordinal);
        var diagnosticsByCandidate = diagnostics
            .Where(item => item.Severity == "error")
            .GroupBy(item => CandidateIdFromTarget(item.Target), StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.ToList(), StringComparer.Ordinal);

        var repairRequests = new List<StrictLlmDraftRepairRequest>();
        foreach (var candidate in candidates.OrderBy(item => item.CandidateId, StringComparer.Ordinal))
        {
            if (!diagnosticsByCandidate.TryGetValue(candidate.CandidateId, out var candidateDiagnostics))
            {
                continue;
            }

            var codes = candidateDiagnostics.Select(item => item.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
            var nonFixable = codes.Any(code => NonFixableCodes.Contains(code) || !FixableCodes.Contains(code));
            var request = requestById.GetValueOrDefault(candidate.RequestId);
            var allowedFields = request == null || nonFixable
                ? Array.Empty<string>()
                : request.RequiredFields
                    .Concat(["linked_intent_ids", "linked_feature_ids", "linked_contract_ids", "linked_semantic_scopes", "source_kind"])
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray();

            repairRequests.Add(new StrictLlmDraftRepairRequest
            {
                RepairRequestId = $"repair-request/{candidate.CandidateId.Replace("candidate/", string.Empty, StringComparison.Ordinal)}/{retryNumber:00}",
                CandidateId = candidate.CandidateId,
                RequestId = candidate.RequestId,
                BlockingDiagnosticCodes = codes,
                AllowedFieldsToFix = allowedFields,
                ImmutableFields =
                [
                    "candidate_id",
                    "request_id",
                    "scenario_id",
                    "profile_id",
                    "draft_family_id",
                    "provenance_id",
                    "provenance_details"
                ],
                SemanticContextDigest = BuildDigest(candidate, request),
                BoundedHumanHint = nonFixable
                    ? "candidate must be rejected or manually resubmitted under the same contract"
                    : "repair only listed fields; preserve ids, family, scenario, profile and provenance",
                RetryNumber = retryNumber,
                MaxRetryCount = maxRetryCount,
                Status = retryNumber > maxRetryCount ? "retry_cap_reached" : nonFixable ? "blocked" : "planned",
                PreservedProvenanceId = candidate.ProvenanceId
            });
        }

        return repairRequests.OrderBy(item => item.RepairRequestId, StringComparer.Ordinal).ToList();
    }

    public static bool IsFixableForPromotion(IReadOnlyList<StrictLlmDraftDiagnostic> diagnostics)
    {
        var codes = diagnostics.Where(item => item.Severity == "error").Select(item => item.Code).Distinct(StringComparer.Ordinal).ToList();
        return codes.Count > 0 && codes.All(FixableCodes.Contains);
    }

    private static string CandidateIdFromTarget(string target)
    {
        var index = target.IndexOf(':', StringComparison.Ordinal);
        return index < 0 ? target : target[..index];
    }

    private static string BuildDigest(StrictLlmDraftCandidateEnvelope candidate, StrictLlmDraftRequest? request) =>
        request == null
            ? $"{candidate.CandidateId}|unknown_request"
            : $"{candidate.CandidateId}|{request.TargetDraftFamily}|fields={request.RequiredFields.Count}|scopes={request.AllowedSemanticScopes.Count}|contracts={request.AllowedArtifactContractIds.Count}";
}
