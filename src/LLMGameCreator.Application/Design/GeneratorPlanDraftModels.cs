using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design;

public sealed record GeneratorPlanDraftRequest(
    string Title,
    string Goal,
    string DesignBrief,
    string? RuntimeTarget = null,
    string? TurnMode = null,
    string? CombatMode = null,
    int? TokenBudget = null,
    string? OutputMode = null);

public sealed record GeneratorPlanDraftResult(
    GeneratorPlanRecord? Plan,
    IReadOnlyList<GeneratorPlanStepRecord> Steps,
    PromptContextPackRecord? PromptContextPack,
    IReadOnlyList<GeneratorPlanValidationIssue> ValidationIssues,
    string RawLlmResponse,
    bool Saved);

public sealed record GeneratorPlanReviewResult(
    GeneratorPlanRecord? Plan,
    IReadOnlyList<GeneratorPlanStepRecord> Steps,
    IReadOnlyList<GeneratorPlanValidationIssue> ValidationIssues,
    bool CanApprove);

public sealed record GeneratorPlanStatusUpdateResult(
    GeneratorPlanRecord? Plan,
    IReadOnlyList<GeneratorPlanValidationIssue> ValidationIssues,
    bool Updated,
    string Message);

public sealed record GeneratorPlanValidationIssue(
    string Severity,
    string Code,
    string Message,
    string Target);

public sealed class GeneratorPlanDraft
{
    public string Title { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public List<GeneratorPlanDraftStep> Steps { get; set; } = new();
}

public sealed class GeneratorPlanDraftStep
{
    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("module_id")]
    public string ModuleId { get; set; } = string.Empty;

    [JsonPropertyName("config")]
    public JsonElement Config { get; set; }

    public string ConfigJson { get; set; } = "{}";

    [JsonPropertyName("depends_on")]
    public List<string> DependsOn { get; set; } = new();
}
