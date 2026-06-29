namespace LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;

public static class StrictLlmDraftVocabulary
{
    public const string SchemaVersion = "strict_llm_draft_artifact_loop_v1";

    public static readonly IReadOnlySet<string> SourceKinds = new HashSet<string>(
        ["manual", "llm", "imported", "programmatic_fixture"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> CandidateStatuses = new HashSet<string>(
        ["quarantined", "rejected", "repair_required", "promotable", "promoted"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> RepairStatuses = new HashSet<string>(
        ["planned", "blocked", "retry_cap_reached"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> PromotionStatuses = new HashSet<string>(
        ["rejected", "repair_required", "promotable", "promoted"],
        StringComparer.Ordinal);
}

public sealed record StrictLlmDraftFamily
{
    public string FamilyId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<string> ForbiddenFields { get; init; } = [];
    public IReadOnlyList<string> AllowedSemanticScopes { get; init; } = [];
    public IReadOnlyList<string> AllowedIntentFamilies { get; init; } = [];
    public IReadOnlyList<string> AllowedArtifactContractIds { get; init; } = [];
    public bool NoFinalProse { get; init; } = true;
    public bool NoRuntimeAuthority { get; init; } = true;
    public string OrderingKey { get; init; } = string.Empty;
}

public sealed record StrictLlmDraftRequest
{
    public string RequestId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string TargetDraftFamily { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceIntentIds { get; init; } = [];
    public IReadOnlyList<string> AllowedArtifactContractIds { get; init; } = [];
    public IReadOnlyList<string> AllowedSemanticScopes { get; init; } = [];
    public IReadOnlyList<string> RequiredFields { get; init; } = [];
    public IReadOnlyList<string> ForbiddenFields { get; init; } = [];
    public int MaximumCandidates { get; init; }
    public IReadOnlyList<string> ExpectedSourceKinds { get; init; } = [];
    public bool NoFinalProse { get; init; } = true;
    public bool NoRuntimeAuthority { get; init; } = true;
    public string RepairPolicyId { get; init; } = string.Empty;
    public string DeterministicOrderingKey { get; init; } = string.Empty;
}

public sealed record StrictLlmDraftRequestSet
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<StrictLlmDraftRequest> Requests { get; init; } = [];
    public int SpeciesArchetypeSlotRequestCount { get; init; }
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record StrictLlmDraftPayloadField
{
    public string Name { get; init; } = string.Empty;
    public string ValueKind { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool FinalProse { get; init; }
}

public sealed record StrictLlmDraftCandidateEnvelope
{
    public string CandidateId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string SourceKind { get; init; } = string.Empty;
    public string ProvenanceId { get; init; } = string.Empty;
    public string ProvenanceDetails { get; init; } = string.Empty;
    public string DraftFamilyId { get; init; } = string.Empty;
    public IReadOnlyList<StrictLlmDraftPayloadField> PayloadFields { get; init; } = [];
    public IReadOnlyList<string> LinkedIntentIds { get; init; } = [];
    public IReadOnlyList<string> LinkedFeatureIds { get; init; } = [];
    public IReadOnlyList<string> LinkedContractIds { get; init; } = [];
    public IReadOnlyList<string> LinkedSemanticScopes { get; init; } = [];
    public IReadOnlyList<string> DeclaredConstraints { get; init; } = [];
    public string Status { get; init; } = "quarantined";
    public IReadOnlyList<StrictLlmDraftDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record StrictLlmDraftRepairRequest
{
    public string RepairRequestId { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public IReadOnlyList<string> BlockingDiagnosticCodes { get; init; } = [];
    public IReadOnlyList<string> AllowedFieldsToFix { get; init; } = [];
    public IReadOnlyList<string> ImmutableFields { get; init; } = [];
    public string SemanticContextDigest { get; init; } = string.Empty;
    public string BoundedHumanHint { get; init; } = string.Empty;
    public int RetryNumber { get; init; }
    public int MaxRetryCount { get; init; }
    public string Status { get; init; } = "planned";
    public string PreservedProvenanceId { get; init; } = string.Empty;
}

public sealed record StrictLlmDraftPromotionDecision
{
    public string CandidateId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string TargetDraftArtifactId { get; init; } = string.Empty;
    public bool Promoted { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<StrictLlmDraftDiagnostic> Diagnostics { get; init; } = [];
    public string PreservedProvenanceId { get; init; } = string.Empty;
    public string Status { get; init; } = "rejected";
}

public sealed record StrictLlmDraftDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record StrictLlmDraftContractSummary
{
    public string SchemaVersion { get; init; } = StrictLlmDraftVocabulary.SchemaVersion;
    public int FamilyCount { get; init; }
    public int RequestCount { get; init; }
    public int CandidateCount { get; init; }
    public int RepairRequestCount { get; init; }
    public int PromotionDecisionCount { get; init; }
    public bool NoProviderLlmRagCallHappened { get; init; } = true;
    public bool NoFinalProseGeneratedOrPromoted { get; init; } = true;
    public bool NoGamePackageMaterializationHappened { get; init; } = true;
    public IReadOnlyList<StrictLlmDraftFamily> Families { get; init; } = [];
    public IReadOnlyList<StrictLlmDraftDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record StrictLlmDraftRequestMatrix
{
    public string SchemaVersion { get; init; } = "strict_llm_draft_request_matrix_v1";
    public int RequestCount { get; init; }
    public IReadOnlyList<StrictLlmDraftRequestSet> ScenarioRequestSets { get; init; } = [];
}

public sealed record StrictLlmDraftCandidateQuarantineMatrix
{
    public string SchemaVersion { get; init; } = "strict_llm_draft_candidate_quarantine_matrix_v1";
    public int CandidateCount { get; init; }
    public int QuarantinedCount { get; init; }
    public IReadOnlyList<StrictLlmDraftCandidateEnvelope> Candidates { get; init; } = [];
}

public sealed record StrictLlmDraftRepairRequestMatrix
{
    public string SchemaVersion { get; init; } = "strict_llm_draft_repair_request_matrix_v1";
    public int RepairRequestCount { get; init; }
    public IReadOnlyList<StrictLlmDraftRepairRequest> RepairRequests { get; init; } = [];
}

public sealed record StrictLlmDraftPromotionDecisionMatrix
{
    public string SchemaVersion { get; init; } = "strict_llm_draft_promotion_decision_matrix_v1";
    public int DecisionCount { get; init; }
    public int PromotedCount { get; init; }
    public int RepairRequiredCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<StrictLlmDraftPromotionDecision> Decisions { get; init; } = [];
}

public sealed record StrictLlmDraftInvalidMatrix
{
    public string SchemaVersion { get; init; } = "strict_llm_draft_invalid_matrix_v1";
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<StrictLlmDraftInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record StrictLlmDraftInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<StrictLlmDraftDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record StrictLlmDraftArtifactLoopReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public int FamilyCount { get; init; }
    public int RequestCount { get; init; }
    public int CandidateCount { get; init; }
    public int RepairRequestCount { get; init; }
    public int PromotionDecisionCount { get; init; }
    public int MetamoduleSpeciesArchetypeRequestCount { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool ProviderLlmRagCalled { get; init; }
    public bool FinalProseGeneratedOrPromoted { get; init; }
    public bool GamePackageMaterialized { get; init; }
    public bool RuntimeUiUnityLuaGeneratorLibraryTouched { get; init; }
    public string ContractSummaryHash { get; init; } = string.Empty;
    public string RequestMatrixHash { get; init; } = string.Empty;
    public string CandidateMatrixHash { get; init; } = string.Empty;
    public string RepairMatrixHash { get; init; } = string.Empty;
    public string PromotionMatrixHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<StrictLlmDraftDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record StrictLlmDraftArtifactLoopEvidenceResult
{
    public StrictLlmDraftContractSummary ContractSummary { get; init; } = new();
    public StrictLlmDraftRequestMatrix RequestMatrix { get; init; } = new();
    public StrictLlmDraftCandidateQuarantineMatrix CandidateMatrix { get; init; } = new();
    public StrictLlmDraftRepairRequestMatrix RepairRequestMatrix { get; init; } = new();
    public StrictLlmDraftPromotionDecisionMatrix PromotionDecisionMatrix { get; init; } = new();
    public StrictLlmDraftInvalidMatrix InvalidMatrix { get; init; } = new();
    public StrictLlmDraftArtifactLoopReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ScenarioPlanJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record StrictLlmDraftArtifactLoopEvidenceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}
