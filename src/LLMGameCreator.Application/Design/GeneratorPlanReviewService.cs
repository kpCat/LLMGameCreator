using System.Text.Json;

namespace LLMGameCreator.Application.Design;

public sealed class GeneratorPlanReviewService : IGeneratorPlanReviewService
{
    private readonly IGeneratorPlanRepository _planRepository;
    private readonly IGeneratorLibraryRegistry _registry;
    private readonly GeneratorPlanValidator _validator;

    public GeneratorPlanReviewService(
        IGeneratorPlanRepository planRepository,
        IGeneratorLibraryRegistry registry,
        GeneratorPlanValidator validator)
    {
        _planRepository = planRepository;
        _registry = registry;
        _validator = validator;
    }

    public async Task<GeneratorPlanReviewResult> RevalidatePlanAsync(string planId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(planId))
        {
            return new GeneratorPlanReviewResult(
                null,
                Array.Empty<GeneratorPlanStepRecord>(),
                new[] { new GeneratorPlanValidationIssue("error", "plan.id.empty", "Plan id is required.", "plan") },
                false);
        }

        var plan = await _planRepository.GetGeneratorPlanByIdAsync(planId, cancellationToken).ConfigureAwait(false);
        if (plan == null)
        {
            return new GeneratorPlanReviewResult(
                null,
                Array.Empty<GeneratorPlanStepRecord>(),
                new[] { new GeneratorPlanValidationIssue("error", "plan.not_found", $"Plan was not found: {planId}", planId) },
                false);
        }

        var steps = await _planRepository.GetGeneratorPlanStepsAsync(plan.Id, cancellationToken).ConfigureAwait(false);
        var modules = await _registry.ListModulesAsync(cancellationToken).ConfigureAwait(false);
        var draft = ToDraft(plan, steps);
        var issues = _validator.Validate(draft, modules, ToValidationRequest(plan));
        return new GeneratorPlanReviewResult(plan, steps, issues, !HasErrors(issues));
    }

    public async Task<GeneratorPlanStatusUpdateResult> ApprovePlanAsync(string planId, string? note, CancellationToken cancellationToken)
    {
        var review = await RevalidatePlanAsync(planId, cancellationToken).ConfigureAwait(false);
        if (review.Plan == null)
        {
            return new GeneratorPlanStatusUpdateResult(review.Plan, review.ValidationIssues, false, "Plan was not found.");
        }

        if (!review.Plan.Status.Equals("draft", StringComparison.OrdinalIgnoreCase))
        {
            return new GeneratorPlanStatusUpdateResult(
                review.Plan,
                review.ValidationIssues,
                false,
                "Only draft plans can be approved.");
        }

        if (!review.CanApprove)
        {
            return new GeneratorPlanStatusUpdateResult(
                review.Plan,
                review.ValidationIssues,
                false,
                "Plan has validation errors and was not approved.");
        }

        return await UpdateStatusAsync(review.Plan.Id, "approved", note, review.ValidationIssues, "Plan approved.", cancellationToken).ConfigureAwait(false);
    }

    public Task<GeneratorPlanStatusUpdateResult> RejectPlanAsync(string planId, string? note, CancellationToken cancellationToken)
    {
        return SetTerminalStatusAsync(planId, "rejected", note, "Plan rejected.", cancellationToken);
    }

    public Task<GeneratorPlanStatusUpdateResult> ArchivePlanAsync(string planId, string? note, CancellationToken cancellationToken)
    {
        return SetTerminalStatusAsync(planId, "archived", note, "Plan archived.", cancellationToken);
    }

    private async Task<GeneratorPlanStatusUpdateResult> SetTerminalStatusAsync(string planId, string status, string? note, string successMessage, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetGeneratorPlanByIdAsync(planId, cancellationToken).ConfigureAwait(false);
        if (plan == null)
        {
            return new GeneratorPlanStatusUpdateResult(
                null,
                new[] { new GeneratorPlanValidationIssue("error", "plan.not_found", $"Plan was not found: {planId}", planId) },
                false,
                "Plan was not found.");
        }

        return await UpdateStatusAsync(plan.Id, status, note, Array.Empty<GeneratorPlanValidationIssue>(), successMessage, cancellationToken).ConfigureAwait(false);
    }

    private async Task<GeneratorPlanStatusUpdateResult> UpdateStatusAsync(
        string planId,
        string status,
        string? note,
        IReadOnlyList<GeneratorPlanValidationIssue> validationIssues,
        string successMessage,
        CancellationToken cancellationToken)
    {
        var updated = await _planRepository.UpdateGeneratorPlanStatusAsync(planId, status, note, cancellationToken).ConfigureAwait(false);
        var plan = await _planRepository.GetGeneratorPlanByIdAsync(planId, cancellationToken).ConfigureAwait(false);
        return new GeneratorPlanStatusUpdateResult(
            plan,
            validationIssues,
            updated,
            updated ? successMessage : "Plan status was not updated.");
    }

    private static GeneratorPlanDraft ToDraft(GeneratorPlanRecord plan, IReadOnlyList<GeneratorPlanStepRecord> steps)
    {
        var draft = new GeneratorPlanDraft
        {
            Title = plan.Title,
            Goal = plan.Goal
        };

        foreach (var step in steps.OrderBy(step => step.StepOrder).ThenBy(step => step.ModuleId, StringComparer.OrdinalIgnoreCase))
        {
            draft.Steps.Add(new GeneratorPlanDraftStep
            {
                Order = step.StepOrder,
                ModuleId = step.ModuleId,
                ConfigJson = string.IsNullOrWhiteSpace(step.ConfigJson) ? "{}" : step.ConfigJson,
                DependsOn = ReadStringArray(step.DependsOnJson)
            });
        }

        return draft;
    }

    private static List<string> ReadStringArray(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json)?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList()
                ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static GeneratorPlanDraftRequest ToValidationRequest(GeneratorPlanRecord plan)
    {
        string? runtimeTarget = null;
        string? turnMode = null;
        string? combatMode = null;

        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(plan.MetadataJson) ? "{}" : plan.MetadataJson);
            runtimeTarget = ReadString(document.RootElement, "runtimeTarget");
            turnMode = ReadString(document.RootElement, "turnMode");
            combatMode = ReadString(document.RootElement, "combatMode");
        }
        catch (JsonException)
        {
        }

        return new GeneratorPlanDraftRequest(plan.Title, plan.Goal, string.Empty, runtimeTarget, turnMode, combatMode);
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static bool HasErrors(IReadOnlyList<GeneratorPlanValidationIssue> issues)
    {
        return issues.Any(issue => issue.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
    }
}
