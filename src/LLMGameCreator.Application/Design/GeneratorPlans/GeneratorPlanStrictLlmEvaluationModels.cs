namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanStrictLlmEvaluationRequest
{
    public bool EvaluateLatestAuditOnly { get; init; }
    public string LlmProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> ContractIds { get; init; } = Array.Empty<string>();
    public int IterationsPerContract { get; init; } = 1;
    public bool EnableRepairAttempt { get; init; } = true;
    public int MaxRepairAttempts { get; init; } = 1;
    public bool StageValidArtifactsForReview { get; init; }
    public int MaxTokens { get; init; } = 4000;
    public double Temperature { get; init; } = 0.2;
    public string ExtraUserBrief { get; init; } = string.Empty;
}

public sealed record GeneratorPlanStrictLlmEvaluationResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public string EvaluationId { get; init; } = string.Empty;
    public DateTimeOffset EvaluatedAtUtc { get; init; }
    public string SourceCapabilitySelectionId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequestedContractIds { get; init; } = Array.Empty<string>();
    public int IterationsPerContract { get; init; }
    public bool RepairEnabled { get; init; }
    public int MaxRepairAttempts { get; init; }
    public bool StageValidArtifactsForReview { get; init; }
    public int ExpectedMaxLlmCalls { get; init; }
    public GeneratorPlanStrictLlmEvaluationSummary Summary { get; init; } = new();
    public IReadOnlyList<GeneratorPlanStrictLlmEvaluationContractSummary> ContractSummaries { get; init; } = Array.Empty<GeneratorPlanStrictLlmEvaluationContractSummary>();
    public IReadOnlyList<GeneratorPlanStrictLlmEvaluationDiagnosticSummary> DiagnosticSummaries { get; init; } = Array.Empty<GeneratorPlanStrictLlmEvaluationDiagnosticSummary>();
    public IReadOnlyList<GeneratorPlanStrictLlmEvaluationSample> Samples { get; init; } = Array.Empty<GeneratorPlanStrictLlmEvaluationSample>();
    public IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();
    public string MarkdownReport { get; init; } = string.Empty;
}

public sealed record GeneratorPlanStrictLlmEvaluationSummary
{
    public int TotalContractsRequested { get; init; }
    public int TotalGenerationRuns { get; init; }
    public int TotalAttempts { get; init; }
    public int InitialPassCount { get; init; }
    public int RepairPassCount { get; init; }
    public int FailedCount { get; init; }
    public int ValidArtifactCount { get; init; }
    public int StagedForReviewCount { get; init; }
    public int MarkdownFenceErrorCount { get; init; }
    public int JsonWrapperErrorCount { get; init; }
    public int JsonInvalidCount { get; init; }
    public int WrongArtifactKindCount { get; init; }
    public int ForbiddenFieldCount { get; init; }
    public int InvalidIdCount { get; init; }
    public int MissingFieldCount { get; init; }
    public int ExpectedMaxLlmCalls { get; init; }
    public double OverallPassRate { get; init; }
    public double RepairRecoveryRate { get; init; }
}

public sealed record GeneratorPlanStrictLlmEvaluationContractSummary
{
    public string ContractId { get; init; } = string.Empty;
    public int Runs { get; init; }
    public int InitialPass { get; init; }
    public int RepairPass { get; init; }
    public int Failed { get; init; }
    public int ValidArtifacts { get; init; }
    public IReadOnlyList<string> TopDiagnosticCodes { get; init; } = Array.Empty<string>();
    public double AverageAttempts { get; init; }
}

public sealed record GeneratorPlanStrictLlmEvaluationDiagnosticSummary
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public int Count { get; init; }
    public string ExampleMessage { get; init; } = string.Empty;
}

public sealed record GeneratorPlanStrictLlmEvaluationSample
{
    public string ContractId { get; init; } = string.Empty;
    public string ArtifactId { get; init; } = string.Empty;
    public bool Valid { get; init; }
    public bool Repaired { get; init; }
    public string ContentExcerpt { get; init; } = string.Empty;
    public string DiagnosticExcerpt { get; init; } = string.Empty;
}

public sealed record GeneratorPlanStrictLlmEvaluationArtifactSaveResult
{
    public GeneratedArtifactRecord EvaluationArtifact { get; init; } = GeneratorPlanStrictLlmEvaluationArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public sealed record GeneratorPlanStrictLlmEvaluationArtifactReadResult
{
    public bool Exists { get; init; }
    public GeneratedArtifactRecord? EvaluationArtifact { get; init; }
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public GeneratorPlanStrictLlmEvaluationResult Result { get; init; } = new();
    public string MarkdownReport { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public static class GeneratorPlanStrictLlmEvaluationMode
{
    public const string LatestAudit = "latest_audit";
    public const string Batch = "batch";
}

public static class GeneratorPlanStrictLlmEvaluationStatus
{
    public const string Evaluated = "evaluated";
    public const string EvaluatedWithWarnings = "evaluated_with_warnings";
    public const string Invalid = "invalid";
}

public static class GeneratorPlanStrictLlmEvaluationDiagnosticCodes
{
    public const string MissingAudit = "strict_llm_evaluation.missing_audit";
    public const string MissingLlmProfile = "strict_llm_evaluation.missing_llm_profile";
    public const string MissingCapabilitySelection = "strict_llm_evaluation.missing_capability_selection";
    public const string MissingContracts = "strict_llm_evaluation.missing_contracts";
    public const string UnknownContract = "strict_llm_evaluation.unknown_contract";
    public const string GenericTextWarning = "strict_llm_evaluation.generic_text_warning";
    public const string ShortDescriptionWarning = "strict_llm_evaluation.short_description_warning";
    public const string EmptyTagsWarning = "strict_llm_evaluation.empty_tags_warning";
    public const string MissingSourceContextWarning = "strict_llm_evaluation.missing_source_context_warning";
    public const string VariantMismatchWarning = "strict_llm_evaluation.variant_mismatch_warning";
    public const string RepeatedTitleWarning = "strict_llm_evaluation.repeated_title_warning";
}

public static class GeneratorPlanStrictLlmEvaluationArtifactIds
{
    public const string GeneratedBy = "generator_plan_strict_llm_evaluation";
    public const string EvaluationArtifactId = "artifact/generator_plan_strict_llm_evaluation/latest";
    public const string EvaluationArtifactKind = "generator_plan.strict_llm_evaluation";
    public const string EvaluationArtifactPath = ".llmgc/generator-plans/generator_plan_strict_llm_evaluation.json";
    public const string MarkdownArtifactId = "artifact/generator_plan_strict_llm_evaluation/report/latest";
    public const string MarkdownArtifactKind = "generator_plan.strict_llm_evaluation.report";
    public const string MarkdownArtifactPath = ".llmgc/generator-plans/generator_plan_strict_llm_evaluation_report.md";
}
