using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.SemanticArtifactContracts;

namespace LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;

public static partial class StrictLlmDraftArtifactLoopValidator
{
    private static readonly string[] BoundaryLeakageNeedles =
    [
        "provider",
        "llm call",
        "rag",
        "runtime",
        "winforms",
        " ui ",
        "unity",
        "lua",
        "gamepackage",
        "schema",
        "c#",
        "code generation",
        "execute"
    ];

    private static readonly string[] FinalProseNeedles =
    [
        "final prose",
        "final dialogue",
        "dialogue line",
        "quest text",
        "lore prose",
        "\"hello"
    ];

    public static IReadOnlyList<StrictLlmDraftDiagnostic> ValidateFamilies(IReadOnlyList<StrictLlmDraftFamily> families)
    {
        var diagnostics = new List<StrictLlmDraftDiagnostic>();
        foreach (var duplicate in families.GroupBy(item => item.FamilyId, StringComparer.Ordinal).Where(item => item.Count() > 1))
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.family_id.duplicate", duplicate.Key, "Draft family ids must be unique."));
        }

        foreach (var family in families.OrderBy(item => item.FamilyId, StringComparer.Ordinal))
        {
            if (!StableIdPattern().IsMatch(family.FamilyId))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.family_id.invalid", family.FamilyId, "Draft family id must be stable."));
            }

            if (family.RequiredFields.Count == 0)
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.family.required_fields.missing", family.FamilyId, "Draft family must declare required fields."));
            }

            if (family.FamilyId == "dialogue_act_template_slot_draft"
                && !family.ForbiddenFields.Contains("dialogue_line", StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.family.dialogue_final_prose_not_forbidden", family.FamilyId, "Dialogue drafts must forbid final dialogue prose."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<StrictLlmDraftDiagnostic> ValidateRequests(
        IReadOnlyList<StrictLlmDraftRequest> requests,
        IReadOnlyList<StrictLlmDraftFamily> families)
    {
        var diagnostics = new List<StrictLlmDraftDiagnostic>();
        var familyById = families.ToDictionary(item => item.FamilyId, StringComparer.Ordinal);
        var requestIds = requests.Select(item => item.RequestId).ToList();
        if (!requestIds.SequenceEqual(requestIds.Order(StringComparer.Ordinal))
            && !requests.Select(item => item.DeterministicOrderingKey).SequenceEqual(requests.Select(item => item.DeterministicOrderingKey).Order(StringComparer.Ordinal)))
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.request_order.nondeterministic", "requests", "Draft requests must be written in stable order."));
        }

        foreach (var duplicate in requests.GroupBy(item => item.RequestId, StringComparer.Ordinal).Where(item => item.Count() > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.request_id.duplicate", duplicate.Key, "Draft request ids must be unique."));
        }

        foreach (var request in requests.OrderBy(item => item.RequestId, StringComparer.Ordinal))
        {
            if (!StableIdPattern().IsMatch(request.RequestId))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.request_id.invalid", request.RequestId, "Draft request id must be stable."));
            }

            if (!familyById.TryGetValue(request.TargetDraftFamily, out var family))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.family.unknown", request.RequestId, "Draft request references an unknown family."));
                continue;
            }

            foreach (var field in family.RequiredFields)
            {
                if (!request.RequiredFields.Contains(field, StringComparer.Ordinal))
                {
                    diagnostics.Add(Diagnostic("error", "strict_draft.request.required_field.missing", request.RequestId, "Draft request dropped a family-required field."));
                }
            }

            if (!request.NoFinalProse || !request.NoRuntimeAuthority)
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.request.boundary_flags.disabled", request.RequestId, "Draft requests must forbid final prose and runtime authority."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<StrictLlmDraftDiagnostic> ValidateCandidates(
        IReadOnlyList<StrictLlmDraftRequest> requests,
        IReadOnlyList<StrictLlmDraftCandidateEnvelope> candidates)
    {
        var diagnostics = new List<StrictLlmDraftDiagnostic>();
        var requestById = requests.GroupBy(item => item.RequestId, StringComparer.Ordinal).ToDictionary(item => item.Key, item => item.First(), StringComparer.Ordinal);
        var contractIds = SemanticArtifactContractRegistry.BuildDefaultContracts().Select(item => item.ContractId).ToHashSet(StringComparer.Ordinal);
        var candidateIds = candidates.Select(item => item.CandidateId).ToList();

        if (!candidateIds.SequenceEqual(candidateIds.Order(StringComparer.Ordinal)))
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.order.nondeterministic", "candidates", "Candidate envelopes must be written in stable candidate id order."));
        }

        foreach (var duplicate in candidates.GroupBy(item => item.CandidateId, StringComparer.Ordinal).Where(item => item.Count() > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.candidate_id.duplicate", duplicate.Key, "Candidate ids must be unique."));
        }

        foreach (var overBudget in candidates.GroupBy(item => item.RequestId, StringComparer.Ordinal).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (requestById.TryGetValue(overBudget.Key, out var request) && overBudget.Count() > request.MaximumCandidates)
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.candidate_count.over_budget", overBudget.Key, "Candidate count exceeds request maximum."));
            }
        }

        foreach (var candidate in candidates.OrderBy(item => item.CandidateId, StringComparer.Ordinal))
        {
            ValidateCandidate(candidate, requestById, contractIds, diagnostics);
        }

        return SortDiagnostics(diagnostics);
    }

    public static IReadOnlyList<StrictLlmDraftDiagnostic> ValidateRepairRequests(
        IReadOnlyList<StrictLlmDraftCandidateEnvelope> candidates,
        IReadOnlyList<StrictLlmDraftRepairRequest> repairRequests)
    {
        var diagnostics = new List<StrictLlmDraftDiagnostic>();
        var candidateById = candidates.ToDictionary(item => item.CandidateId, StringComparer.Ordinal);
        foreach (var repair in repairRequests.OrderBy(item => item.RepairRequestId, StringComparer.Ordinal))
        {
            if (!candidateById.TryGetValue(repair.CandidateId, out var candidate))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.repair_target.invalid", repair.RepairRequestId, "Repair request targets an unknown candidate."));
                continue;
            }

            if (repair.RequestId != candidate.RequestId)
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.repair_target.invalid", repair.RepairRequestId, "Repair request targets a candidate from a different request."));
            }

            if (repair.AllowedFieldsToFix.Intersect(repair.ImmutableFields, StringComparer.Ordinal).Any())
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.repair.immutable_field_mutation", repair.RepairRequestId, "Repair request attempts to change immutable fields."));
            }

            if (repair.BoundedHumanHint.Contains("call provider", StringComparison.OrdinalIgnoreCase)
                || repair.BoundedHumanHint.Contains("prompt the llm", StringComparison.OrdinalIgnoreCase)
                || repair.BoundedHumanHint.Contains("run lua", StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.repair.boundary.leakage", repair.RepairRequestId, "Repair hint must not contain provider/runtime instructions."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public static StrictLlmDraftInvalidMatrix BuildInvalidMatrix()
    {
        var families = StrictLlmDraftArtifactLoopCatalog.BuildDraftFamilies();
        var requestSets = StrictLlmDraftArtifactLoopCatalog.BuildDefaultRequestSets();
        var requests = requestSets.SelectMany(item => item.Requests).OrderBy(item => item.RequestId, StringComparer.Ordinal).ToList();
        var valid = StrictLlmDraftArtifactLoopCatalog.BuildProgrammaticFixtureCandidates(requestSets);
        var first = valid.First();
        var firstRequest = requests.Single(item => item.RequestId == first.RequestId);
        var wrongFamilyId = first.DraftFamilyId == "dialogue_act_template_slot_draft"
            ? "lore_rule_draft"
            : "dialogue_act_template_slot_draft";
        var repairPlanner = new StrictLlmDraftRepairPlanner();
        var missingRequired = first with { PayloadFields = first.PayloadFields.Skip(1).ToList() };
        var missingDiagnostics = ValidateCandidates(requests, [missingRequired]);
        var repair = repairPlanner.PlanRepairRequests(requests, [missingRequired], missingDiagnostics).Single();

        var cases = new List<StrictLlmDraftInvalidScenario>
        {
            Invalid("duplicate_request_id", "duplicate request id", ValidateRequests([requests[0], requests[0]], families)),
            Invalid("duplicate_candidate_id", "duplicate candidate id", ValidateCandidates(requests, [first, first])),
            Invalid("unknown_request", "unknown request", ValidateCandidates(requests, [first with { RequestId = "draft-request/fake/unknown/001" }])),
            Invalid("wrong_family", "wrong family", ValidateCandidates(requests, [first with { DraftFamilyId = wrongFamilyId }])),
            Invalid("missing_required_field", "missing required field", missingDiagnostics),
            Invalid("forbidden_final_prose_field", "forbidden final prose field", ValidateCandidates(requests, [first with { PayloadFields = first.PayloadFields.Concat([new StrictLlmDraftPayloadField { Name = firstRequest.ForbiddenFields[0], ValueKind = "text", Value = "final prose", FinalProse = true }]).ToList() }])),
            Invalid("provider_runtime_ui_unity_lua_gamepackage_code_leakage", "provider/runtime/UI/Unity/Lua/GamePackage/code leakage", ValidateCandidates(requests, [first with { ProvenanceDetails = "call LLM provider and execute Lua to write GamePackage schema code generation" }])),
            Invalid("candidate_self_marked_promoted", "candidate self-marked promoted", ValidateCandidates(requests, [first with { Status = "promoted" }])),
            Invalid("source_provenance_mismatch", "source/provenance mismatch", ValidateCandidates(requests, [first with { SourceKind = "provider_response" }])),
            Invalid("missing_intent_trace", "missing intent trace", ValidateCandidates(requests, [first with { LinkedIntentIds = [] }])),
            Invalid("missing_contract_trace", "missing contract trace", ValidateCandidates(requests, [first with { LinkedContractIds = [] }])),
            Invalid("fake_target_contract", "fake target contract", ValidateCandidates(requests, [first with { LinkedContractIds = ["fake_contract_v1"] }])),
            Invalid("fake_semantic_scope", "fake semantic scope", ValidateCandidates(requests, [first with { LinkedSemanticScopes = ["fake_scope"] }])),
            Invalid("incompatible_scenario_profile", "incompatible scenario/profile", ValidateCandidates(requests, [first with { ScenarioId = "gothic_intrigue" }])),
            Invalid("over_budget_candidate_count", "over-budget candidate count", ValidateCandidates(requests, Enumerable.Range(0, firstRequest.MaximumCandidates + 1).Select(index => first with { CandidateId = $"candidate/over-budget/{index:0000}" }).ToList())),
            Invalid("invalid_repair_target", "invalid repair target", ValidateRepairRequests(valid, [repair with { CandidateId = "candidate/missing" }])),
            Invalid("repair_attempts_immutable_mutation", "repair immutable mutation", ValidateRepairRequests([missingRequired], [repair with { AllowedFieldsToFix = repair.AllowedFieldsToFix.Concat(["candidate_id"]).ToList(), ImmutableFields = ["candidate_id"] }])),
            Invalid("nondeterministic_ordering_mutation", "nondeterministic ordering mutation", ValidateCandidates(requests, valid.Take(3).Reverse().ToList()))
        };

        return new StrictLlmDraftInvalidMatrix
        {
            ScenarioCount = cases.Count,
            MatchedExpectationCount = cases.Count(item => item.ExpectedValid == item.ActualValid),
            RejectedCount = cases.Count(item => !item.ActualValid),
            Passed = cases.All(item => item.ExpectedValid == item.ActualValid),
            Scenarios = cases.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static StrictLlmDraftDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    public static IReadOnlyList<StrictLlmDraftDiagnostic> SortDiagnostics(IEnumerable<StrictLlmDraftDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static void ValidateCandidate(
        StrictLlmDraftCandidateEnvelope candidate,
        IReadOnlyDictionary<string, StrictLlmDraftRequest> requestById,
        IReadOnlySet<string> knownContractIds,
        ICollection<StrictLlmDraftDiagnostic> diagnostics)
    {
        if (!StrictLlmDraftVocabulary.CandidateStatuses.Contains(candidate.Status))
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.candidate_status.unknown", candidate.CandidateId, "Candidate status is unknown."));
        }

        if (candidate.Status is "promoted" or "promotable"
            || candidate.DeclaredConstraints.Any(item => item.Contains("accepted:true", StringComparison.OrdinalIgnoreCase) || item.Contains("promoted:true", StringComparison.OrdinalIgnoreCase)))
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.candidate.self_promoted", candidate.CandidateId, "Candidate cannot self-declare accepted, promotable or promoted."));
        }

        if (!requestById.TryGetValue(candidate.RequestId, out var request))
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.request.unknown", candidate.CandidateId, "Candidate references an unknown request."));
            return;
        }

        if (candidate.DraftFamilyId != request.TargetDraftFamily)
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.family.wrong", candidate.CandidateId, "Candidate family does not match its request."));
        }

        if (candidate.ScenarioId != request.ScenarioId || candidate.ProfileId != request.ProfileId)
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.scenario.incompatible", candidate.CandidateId, "Candidate scenario/profile does not match its request."));
        }

        if (!request.ExpectedSourceKinds.Contains(candidate.SourceKind, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.source_kind.mismatch", candidate.CandidateId, "Candidate source kind is not expected by request."));
        }

        foreach (var required in request.RequiredFields.Order(StringComparer.Ordinal))
        {
            if (candidate.PayloadFields.All(item => item.Name != required))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.required_field.missing", $"{candidate.CandidateId}:{required}", "Candidate payload is missing a required field."));
            }
        }

        foreach (var field in candidate.PayloadFields.OrderBy(item => item.Name, StringComparer.Ordinal))
        {
            if (request.ForbiddenFields.Contains(field.Name, StringComparer.Ordinal) || field.FinalProse || ContainsFinalProse(field.Name, field.Value))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.final_prose.forbidden", $"{candidate.CandidateId}:{field.Name}", "Candidate contains forbidden final prose or final text field."));
            }

            if (ContainsBoundaryLeakage(field.Name, field.Value, field.ValueKind))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.boundary.leakage", $"{candidate.CandidateId}:{field.Name}", "Candidate payload leaks forbidden provider/runtime/UI/Unity/Lua/GamePackage/code boundary."));
            }
        }

        if (ContainsBoundaryLeakage(candidate.ProvenanceDetails, string.Join(" ", candidate.DeclaredConstraints)))
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.boundary.leakage", candidate.CandidateId, "Candidate metadata leaks forbidden provider/runtime/UI/Unity/Lua/GamePackage/code boundary."));
        }

        if (request.SourceIntentIds.Count > 0 && !candidate.LinkedIntentIds.Intersect(request.SourceIntentIds, StringComparer.Ordinal).Any())
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.intent_trace.missing", candidate.CandidateId, "Candidate must keep at least one source intent trace."));
        }

        if (candidate.LinkedFeatureIds.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.feature_trace.missing", candidate.CandidateId, "Candidate must keep feature trace when required by the request."));
        }

        if (candidate.LinkedContractIds.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.contract_trace.missing", candidate.CandidateId, "Candidate must keep target artifact contract trace."));
        }

        foreach (var contractId in candidate.LinkedContractIds.Order(StringComparer.Ordinal))
        {
            if (!knownContractIds.Contains(contractId) || !request.AllowedArtifactContractIds.Contains(contractId, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.contract.fake", $"{candidate.CandidateId}:{contractId}", "Candidate references a fake or disallowed target contract."));
            }
        }

        if (candidate.LinkedSemanticScopes.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "strict_draft.scope_trace.missing", candidate.CandidateId, "Candidate must keep semantic scope trace."));
        }

        foreach (var scope in candidate.LinkedSemanticScopes.Order(StringComparer.Ordinal))
        {
            if (!request.AllowedSemanticScopes.Contains(scope, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "strict_draft.semantic_scope.fake", $"{candidate.CandidateId}:{scope}", "Candidate references a fake or disallowed semantic scope."));
            }
        }
    }

    private static bool ContainsBoundaryLeakage(params string[] values)
    {
        var text = " " + string.Join(" ", values).ToLowerInvariant() + " ";
        return BoundaryLeakageNeedles.Any(text.Contains);
    }

    private static bool ContainsFinalProse(params string[] values)
    {
        var text = string.Join(" ", values).ToLowerInvariant();
        return FinalProseNeedles.Any(text.Contains);
    }

    private static StrictLlmDraftInvalidScenario Invalid(string id, string kind, IReadOnlyList<StrictLlmDraftDiagnostic> diagnostics)
    {
        var sorted = SortDiagnostics(diagnostics);
        return new StrictLlmDraftInvalidScenario
        {
            ScenarioId = id,
            MutatedEvidenceKind = kind,
            ExpectedValid = false,
            ActualValid = sorted.All(item => item.Severity != "error"),
            Diagnostics = sorted
        };
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    [GeneratedRegex("^[a-z0-9][a-z0-9_./:-]*[a-z0-9]$")]
    private static partial Regex StableIdPattern();
}
