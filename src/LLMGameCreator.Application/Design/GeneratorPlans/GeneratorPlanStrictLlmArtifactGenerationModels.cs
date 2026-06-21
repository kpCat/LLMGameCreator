namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanStrictLlmArtifactGenerationRequest
{
    public string LlmProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> ContractIds { get; init; } = Array.Empty<string>();
    public bool UseLatestCapabilitySelection { get; init; } = true;
    public bool StageForReview { get; init; } = true;
    public bool EnableRepairAttempt { get; init; } = true;
    public int MaxRepairAttempts { get; init; } = 1;
    public int MaxTokens { get; init; } = 4000;
    public double Temperature { get; init; } = 0.2;
    public string ExtraUserBrief { get; init; } = string.Empty;
    public string ContentLanguage { get; init; } = string.Empty;
}

public sealed record GeneratorPlanStrictLlmArtifactGenerationResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; init; }
    public string SourceCapabilitySelectionId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequestedContractIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<GeneratorPlanStrictLlmGeneratedArtifact> Artifacts { get; init; } = Array.Empty<GeneratorPlanStrictLlmGeneratedArtifact>();
    public IReadOnlyList<GeneratorPlanStrictLlmArtifactGenerationAttempt> Attempts { get; init; } = Array.Empty<GeneratorPlanStrictLlmArtifactGenerationAttempt>();
    public IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();
    public GeneratorPlanDraftArtifactApprovalResult? StagingResult { get; init; }
}

public sealed record GeneratorPlanStrictLlmGeneratedArtifact
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string ExpectedArtifactContract { get; init; } = string.Empty;
    public string ContentJson { get; init; } = "{}";
    public bool Valid { get; init; }
    public bool Repaired { get; init; }
    public bool RequiresHumanApproval { get; init; } = true;
}

public sealed record GeneratorPlanStrictLlmArtifactGenerationAttempt
{
    public string ContractId { get; init; } = string.Empty;
    public int AttemptIndex { get; init; }
    public bool IsRepairAttempt { get; init; }
    public string PromptHash { get; init; } = string.Empty;
    public string ResponseHash { get; init; } = string.Empty;
    public bool ParsedOk { get; init; }
    public bool ValidationOk { get; init; }
    public IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();
    public string PromptText { get; init; } = string.Empty;
    public string ResponseText { get; init; } = string.Empty;
}

public sealed record GeneratorPlanStrictLlmArtifactDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
}

public sealed record GeneratorPlanStrictJsonParseResult
{
    public bool Ok { get; init; }
    public string Json { get; init; } = string.Empty;
    public IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();
}

public sealed record GeneratorPlanStrictLlmArtifactPrompt
{
    public string ContractId { get; init; } = string.Empty;
    public string SystemPrompt { get; init; } = string.Empty;
    public string UserPrompt { get; init; } = string.Empty;
    public string CombinedText => SystemPrompt + Environment.NewLine + Environment.NewLine + UserPrompt;
}

public sealed record GeneratorPlanStrictLlmArtifactPromptPreviewResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public string PromptText { get; init; } = string.Empty;
    public IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();
}

public sealed record GeneratorPlanStrictLlmArtifactGenerationArtifactSaveResult
{
    public GeneratedArtifactRecord GenerationArtifact { get; init; } = GeneratorPlanStrictLlmArtifactGenerationArtifactService.EmptyArtifact;
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public sealed record GeneratorPlanStrictLlmArtifactGenerationArtifactReadResult
{
    public bool Exists { get; init; }
    public GeneratedArtifactRecord? GenerationArtifact { get; init; }
    public GeneratorPlanStrictLlmArtifactGenerationResult Result { get; init; } = new();
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public static class GeneratorPlanStrictLlmArtifactGenerationStatus
{
    public const string Generated = "generated";
    public const string GeneratedWithDiagnostics = "generated_with_diagnostics";
    public const string Invalid = "invalid";
}

public static class GeneratorPlanStrictLlmArtifactDiagnosticCodes
{
    public const string MissingLlmProfile = "strict_llm_artifact_generation.missing_llm_profile";
    public const string MissingCapabilitySelection = "strict_llm_artifact_generation.missing_capability_selection";
    public const string MissingContracts = "strict_llm_artifact_generation.missing_contracts";
    public const string UnknownContract = "strict_llm_artifact_generation.unknown_contract";
    public const string LlmCallFailed = "strict_llm_artifact_generation.llm_call_failed";
    public const string JsonMarkdownFence = "strict_llm_artifact_generation.json_markdown_fence";
    public const string JsonTextWrapper = "strict_llm_artifact_generation.json_text_wrapper";
    public const string JsonInvalid = "strict_llm_artifact_generation.json_invalid";
    public const string JsonRootNotObject = "strict_llm_artifact_generation.json_root_not_object";
    public const string MissingField = "strict_llm_artifact_generation.missing_field";
    public const string WrongArtifactKind = "strict_llm_artifact_generation.wrong_artifact_kind";
    public const string ForbiddenField = "strict_llm_artifact_generation.forbidden_field";
    public const string InvalidId = "strict_llm_artifact_generation.invalid_id";
    public const string InvalidArray = "strict_llm_artifact_generation.invalid_array";
    public const string EmptyRequiredArray = "strict_llm_artifact_generation.empty_required_array";
    public const string InvalidContractContent = "strict_llm_artifact_generation.invalid_contract_content";
    public const string ContentLanguageWarning = "strict_llm_artifact_generation.content_language_warning";
}

public static class GeneratorPlanStrictLlmArtifactGenerationArtifactIds
{
    public const string GeneratedBy = "generator_plan_strict_llm_artifact_generation";
    public const string GenerationArtifactId = "artifact/generator_plan_strict_llm_artifact_generation/latest";
    public const string GenerationArtifactKind = "generator_plan.strict_llm_artifact_generation";
    public const string GenerationArtifactPath = ".llmgc/generator-plans/generator_plan_strict_llm_artifact_generation.json";
}
