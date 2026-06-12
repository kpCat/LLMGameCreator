using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Generation;
using LLMGameCreator.Application.Settings;

namespace LLMGameCreator.Application.Design;

public sealed class GeneratorPlanDraftService : IGeneratorPlanDraftService
{
    private readonly IGeneratorLibraryRegistry _registry;
    private readonly IDesignKnowledgeRepository _knowledgeRepository;
    private readonly IGeneratorPlanRepository _planRepository;
    private readonly IAppSettingsRepository _settingsRepository;
    private readonly ILlmChatClient _llmChatClient;
    private readonly GeneratorPlanValidator _validator;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public GeneratorPlanDraftService(
        IGeneratorLibraryRegistry registry,
        IDesignKnowledgeRepository knowledgeRepository,
        IGeneratorPlanRepository planRepository,
        IAppSettingsRepository settingsRepository,
        ILlmChatClient llmChatClient,
        GeneratorPlanValidator validator)
    {
        _registry = registry;
        _knowledgeRepository = knowledgeRepository;
        _planRepository = planRepository;
        _settingsRepository = settingsRepository;
        _llmChatClient = llmChatClient;
        _validator = validator;
    }

    public async Task<GeneratorPlanDraftResult> CreateDraftPlanAsync(GeneratorPlanDraftRequest request, CancellationToken cancellationToken)
    {
        var modules = await _registry.ListModulesAsync(cancellationToken).ConfigureAwait(false);
        var capabilities = await _registry.ListCapabilitiesAsync(cancellationToken).ConfigureAwait(false);
        var knowledge = await _knowledgeRepository.ListKnowledgeItemsAsync(cancellationToken).ConfigureAwait(false);
        var decisions = await _knowledgeRepository.ListDecisionsAsync(cancellationToken).ConfigureAwait(false);
        var constraints = await _knowledgeRepository.ListConstraintsAsync(cancellationToken).ConfigureAwait(false);
        var contextText = BuildContextPackText(request, modules, capabilities, knowledge, decisions, constraints);
        var profile = await LoadDefaultProfileAsync(cancellationToken).ConfigureAwait(false);

        var response = await _llmChatClient.CompleteAsync(profile, new LlmChatRequest
        {
            SystemPrompt = BuildSystemPrompt(),
            UserPrompt = BuildUserPrompt(request, contextText),
            Temperature = 0.1,
            MaxTokens = Math.Clamp(request.TokenBudget ?? 1800, 512, 6000)
        }, cancellationToken).ConfigureAwait(false);

        var rawJson = ExtractStrictJson(response.Content);
        if (rawJson == null)
        {
            return new GeneratorPlanDraftResult(
                null,
                Array.Empty<GeneratorPlanStepRecord>(),
                null,
                new[] { new GeneratorPlanValidationIssue("error", "plan.json.strict", "LLM response must be one JSON object without markdown or surrounding text.", "response") },
                response.Content,
                false);
        }

        GeneratorPlanDraft? draft;
        try
        {
            draft = JsonSerializer.Deserialize<GeneratorPlanDraft>(rawJson, JsonOptions);
        }
        catch (JsonException ex)
        {
            return new GeneratorPlanDraftResult(
                null,
                Array.Empty<GeneratorPlanStepRecord>(),
                null,
                new[] { new GeneratorPlanValidationIssue("error", "plan.json.invalid", ex.Message, "response") },
                response.Content,
                false);
        }

        if (draft == null)
        {
            return new GeneratorPlanDraftResult(
                null,
                Array.Empty<GeneratorPlanStepRecord>(),
                null,
                new[] { new GeneratorPlanValidationIssue("error", "plan.empty", "LLM response produced an empty plan.", "response") },
                response.Content,
                false);
        }

        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            draft.Title = string.IsNullOrWhiteSpace(request.Title) ? "Generator plan draft" : request.Title.Trim();
        }

        if (string.IsNullOrWhiteSpace(draft.Goal))
        {
            draft.Goal = request.Goal.Trim();
        }

        var validationIssues = _validator.Validate(draft, modules, request, rawJson);
        var hasErrors = validationIssues.Any(issue => issue.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
        if (hasErrors)
        {
            return new GeneratorPlanDraftResult(null, Array.Empty<GeneratorPlanStepRecord>(), null, validationIssues, response.Content, false);
        }

        var now = DateTimeOffset.UtcNow;
        var plan = new GeneratorPlanRecord(
            "plan/" + StableId(draft.Title, draft.Goal, now.ToString("O"))[..16],
            draft.Title.Trim(),
            draft.Goal.Trim(),
            "draft",
            BuildPlanMetadataJson(request, response),
            now,
            now);

        var steps = draft.Steps
            .OrderBy(step => step.Order)
            .ThenBy(step => step.ModuleId, StringComparer.OrdinalIgnoreCase)
            .Select(step => new GeneratorPlanStepRecord(
                plan.Id + "/step/" + step.Order.ToString("D4"),
                plan.Id,
                step.Order,
                step.ModuleId.Trim(),
                string.IsNullOrWhiteSpace(step.ConfigJson) ? "{}" : step.ConfigJson,
                JsonSerializer.Serialize(step.DependsOn.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase), JsonOptions),
                "pending"))
            .ToList();

        var contextPack = new PromptContextPackRecord(
            "context-pack/" + StableId(plan.Id)[..16],
            "generator-plan-draft",
            JsonSerializer.Serialize(knowledge.Select(item => item.Id).Concat(decisions.Select(item => item.Id)).Concat(constraints.Select(item => item.Id)).Take(120).ToList(), JsonOptions),
            JsonSerializer.Serialize(steps.Select(step => step.ModuleId).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToList(), JsonOptions),
            request.TokenBudget ?? 1800,
            JsonSerializer.Serialize(new { request.RuntimeTarget, request.TurnMode, request.CombatMode, context = contextText }, JsonOptions));

        await _planRepository.SaveGeneratorPlanAsync(plan, steps, contextPack, cancellationToken).ConfigureAwait(false);

        return new GeneratorPlanDraftResult(plan, steps, contextPack, validationIssues, response.Content, true);
    }

    private async Task<LlmEndpointSettings> LoadDefaultProfileAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
        var profile = settings.LlmProfiles.FirstOrDefault(item => string.Equals(item.Id, settings.DefaultLlmProfileId, StringComparison.Ordinal))
            ?? settings.LlmProfiles.FirstOrDefault();
        if (profile == null)
        {
            throw new InvalidOperationException("No LLM profile is configured.");
        }

        if (string.IsNullOrWhiteSpace(profile.Endpoint) || string.IsNullOrWhiteSpace(profile.Model))
        {
            throw new InvalidOperationException($"LLM profile '{profile.Id}' must contain endpoint and model.");
        }

        return profile;
    }

    private static string BuildSystemPrompt()
    {
        return string.Join(Environment.NewLine, new[]
        {
            "You are a deterministic planner for LLMGameCreator generator modules.",
            "Return ONLY one valid JSON object. No markdown. No comments.",
            "Do not generate Lua, C#, shell, PowerShell, SQL, commands, scripts, code, eval, execute fields, or runnable content.",
            "Do not apply the plan to a GamePackage.",
            "Select only module_id values that appear in the registry context.",
            "Use this JSON shape: { \"title\": \"...\", \"goal\": \"...\", \"steps\": [ { \"order\": 1, \"module_id\": \"core/example/v1\", \"config\": {}, \"depends_on\": [] } ] }."
        });
    }

    private static string BuildUserPrompt(GeneratorPlanDraftRequest request, string contextText)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Create a draft generator plan from this design request.");
        builder.AppendLine($"Title: {EmptyAsDash(request.Title)}");
        builder.AppendLine($"Goal: {EmptyAsDash(request.Goal)}");
        builder.AppendLine($"RuntimeTarget: {EmptyAsDash(request.RuntimeTarget)}");
        builder.AppendLine($"TurnMode: {EmptyAsDash(request.TurnMode)}");
        builder.AppendLine($"CombatMode: {EmptyAsDash(request.CombatMode)}");
        builder.AppendLine();
        builder.AppendLine("DesignBrief:");
        builder.AppendLine(EmptyAsDash(request.DesignBrief));
        builder.AppendLine();
        builder.AppendLine("Compact registry and design context:");
        builder.AppendLine(contextText);
        return builder.ToString();
    }

    private static string BuildContextPackText(
        GeneratorPlanDraftRequest request,
        IReadOnlyList<GeneratorModuleRecord> modules,
        IReadOnlyList<CapabilityModuleRecord> capabilities,
        IReadOnlyList<DesignKnowledgeItem> knowledge,
        IReadOnlyList<DesignDecision> decisions,
        IReadOnlyList<DesignConstraint> constraints)
    {
        var builder = new StringBuilder();
        builder.AppendLine("modules:");
        foreach (var module in modules.OrderBy(module => module.Category, StringComparer.OrdinalIgnoreCase).ThenBy(module => module.Id, StringComparer.OrdinalIgnoreCase).Take(80))
        {
            builder.AppendLine(JsonSerializer.Serialize(new
            {
                id = module.Id,
                category = module.Category,
                path = module.Path,
                capabilities = ReadJsonArray(module.CapabilitiesJson),
                dependencies = ReadJsonArray(module.DependenciesJson),
                runtime_targets = ReadJsonArray(module.RuntimeTargetsJson),
                turn_modes = ReadJsonArray(module.TurnModesJson),
                combat_modes = ReadJsonArray(module.CombatModesJson)
            }, JsonOptions));
        }

        builder.AppendLine("capabilities:");
        foreach (var capability in capabilities.OrderBy(capability => capability.Category, StringComparer.OrdinalIgnoreCase).ThenBy(capability => capability.Id, StringComparer.OrdinalIgnoreCase).Take(120))
        {
            builder.AppendLine(JsonSerializer.Serialize(new { id = capability.Id, category = capability.Category }, JsonOptions));
        }

        builder.AppendLine("knowledge:");
        foreach (var item in knowledge.OrderBy(item => item.Kind, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase).Take(30))
        {
            builder.AppendLine(JsonSerializer.Serialize(new { item.Id, item.Kind, item.Title, Body = Truncate(item.Body, 240), item.Status }, JsonOptions));
        }

        builder.AppendLine("decisions:");
        foreach (var decision in decisions.OrderByDescending(item => item.UpdatedUtc).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase).Take(30))
        {
            builder.AppendLine(JsonSerializer.Serialize(new { decision.Id, decision.Question, decision.ChosenAnswer, decision.Status }, JsonOptions));
        }

        builder.AppendLine("constraints:");
        foreach (var constraint in constraints.OrderBy(item => item.Scope, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase).Take(30))
        {
            builder.AppendLine(JsonSerializer.Serialize(new { constraint.Id, constraint.Scope, constraint.Rule, constraint.Severity, constraint.Status }, JsonOptions));
        }

        builder.AppendLine("requested_compatibility:");
        builder.AppendLine(JsonSerializer.Serialize(new { request.RuntimeTarget, request.TurnMode, request.CombatMode }, JsonOptions));
        return builder.ToString();
    }

    private static string BuildPlanMetadataJson(GeneratorPlanDraftRequest request, LlmChatResponse response)
    {
        return JsonSerializer.Serialize(new
        {
            request.RuntimeTarget,
            request.TurnMode,
            request.CombatMode,
            request.TokenBudget,
            response.Endpoint,
            response.Model
        }, JsonOptions);
    }

    private static string? ExtractStrictJson(string content)
    {
        var trimmed = content.Trim();
        return trimmed.StartsWith("{", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal) && !trimmed.StartsWith("```", StringComparison.Ordinal)
            ? trimmed
            : null;
    }

    private static List<string> ReadJsonArray(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json)?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static string EmptyAsDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string StableId(params string[] parts)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
