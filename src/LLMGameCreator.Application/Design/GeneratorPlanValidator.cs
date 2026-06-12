using System.Text.Json;

namespace LLMGameCreator.Application.Design;

public sealed class GeneratorPlanValidator
{
    private static readonly HashSet<string> ForbiddenExecutionFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "lua",
        "lua_code",
        "script",
        "code",
        "command",
        "execute",
        "eval",
        "shell",
        "powershell",
        "cmd"
    };

    public IReadOnlyList<GeneratorPlanValidationIssue> Validate(
        GeneratorPlanDraft plan,
        IReadOnlyList<GeneratorModuleRecord> registryModules,
        GeneratorPlanDraftRequest request,
        string? rawPlanJson = null)
    {
        var issues = new List<GeneratorPlanValidationIssue>();
        var modulesById = registryModules.ToDictionary(module => module.Id, StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(rawPlanJson))
        {
            CheckForbiddenExecutionFields(rawPlanJson, issues);
        }

        if (string.IsNullOrWhiteSpace(plan.Title))
        {
            Add(issues, "error", "plan.title.empty", "Plan title is required.", "title");
        }

        if (string.IsNullOrWhiteSpace(plan.Goal))
        {
            Add(issues, "error", "plan.goal.empty", "Plan goal is required.", "goal");
        }

        if (plan.Steps.Count == 0)
        {
            Add(issues, "error", "plan.steps.empty", "Plan must contain at least one step.", "steps");
        }

        var selectedModuleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenOrders = new HashSet<int>();

        foreach (var step in plan.Steps)
        {
            var target = string.IsNullOrWhiteSpace(step.ModuleId) ? $"step/{step.Order}" : step.ModuleId;

            if (step.Order <= 0)
            {
                Add(issues, "error", "plan.step_order.invalid", "Step order must be positive.", target);
            }

            if (!seenOrders.Add(step.Order))
            {
                Add(issues, "error", "plan.step_order.duplicate", $"Duplicate step order {step.Order}.", target);
            }

            if (string.IsNullOrWhiteSpace(step.ModuleId))
            {
                Add(issues, "error", "plan.module_id.empty", "Step module_id is required.", target);
                continue;
            }

            if (!modulesById.TryGetValue(step.ModuleId, out var module))
            {
                Add(issues, "error", "plan.module_id.unknown", $"Unknown module id: {step.ModuleId}", step.ModuleId);
                continue;
            }

            if (!selectedModuleIds.Add(step.ModuleId))
            {
                Add(issues, "warning", "plan.module_id.duplicate", $"Module id is selected more than once: {step.ModuleId}", step.ModuleId);
            }

            CheckJson(step, issues);
            CheckCompatibility(module, request.RuntimeTarget, "runtime target", "plan.runtime_target.incompatible", module.RuntimeTargetsJson, issues);
            CheckCompatibility(module, request.TurnMode, "turn mode", "plan.turn_mode.incompatible", module.TurnModesJson, issues);
            CheckCompatibility(module, request.CombatMode, "combat mode", "plan.combat_mode.incompatible", module.CombatModesJson, issues);

            foreach (var dependency in Clean(step.DependsOn))
            {
                if (!modulesById.ContainsKey(dependency))
                {
                    Add(issues, "error", "plan.dependency.unknown", $"Declared dependency does not exist in registry: {dependency}", step.ModuleId);
                }
            }
        }

        foreach (var step in plan.Steps.Where(step => modulesById.ContainsKey(step.ModuleId)))
        {
            var module = modulesById[step.ModuleId];
            foreach (var requiredDependency in ReadJsonArray(module.DependenciesJson))
            {
                if (!selectedModuleIds.Contains(requiredDependency))
                {
                    Add(issues, "error", "plan.dependency.missing", $"Required dependency is not included in the plan: {requiredDependency}", step.ModuleId);
                }
            }
        }

        return issues
            .OrderBy(issue => issue.Severity.Equals("error", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(issue => issue.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(issue => issue.Target, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void CheckJson(GeneratorPlanDraftStep step, List<GeneratorPlanValidationIssue> issues)
    {
        var configJson = step.ConfigJson;
        if (step.Config.ValueKind != JsonValueKind.Undefined)
        {
            configJson = step.Config.GetRawText();
            step.ConfigJson = string.IsNullOrWhiteSpace(configJson) ? "{}" : configJson;
        }

        if (string.IsNullOrWhiteSpace(step.ConfigJson))
        {
            step.ConfigJson = "{}";
        }

        try
        {
            using var document = JsonDocument.Parse(step.ConfigJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                Add(issues, "error", "plan.config.not_object", "Step config must be a JSON object.", step.ModuleId);
            }
        }
        catch (JsonException ex)
        {
            Add(issues, "error", "plan.config.invalid_json", $"Step config must be valid JSON: {ex.Message}", step.ModuleId);
        }
    }

    private static void CheckCompatibility(
        GeneratorModuleRecord module,
        string? requested,
        string label,
        string code,
        string supportedJson,
        List<GeneratorPlanValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(requested))
        {
            return;
        }

        var supported = ReadJsonArray(supportedJson);
        // Empty compatibility arrays are treated as generic/unspecified by the registry planner.
        if (supported.Count == 0 || supported.Contains(requested.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        Add(issues, "error", code, $"Requested {label} '{requested}' is not supported by module {module.Id}.", module.Id);
    }

    private static void CheckForbiddenExecutionFields(string rawPlanJson, List<GeneratorPlanValidationIssue> issues)
    {
        try
        {
            using var document = JsonDocument.Parse(rawPlanJson);
            CheckForbiddenExecutionFields(document.RootElement, "$", issues);
        }
        catch (JsonException)
        {
            Add(issues, "error", "plan.json.invalid", "LLM response must be valid JSON.", "response");
        }
    }

    private static void CheckForbiddenExecutionFields(JsonElement element, string path, List<GeneratorPlanValidationIssue> issues)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var childPath = path + "." + property.Name;
                if (ForbiddenExecutionFields.Contains(property.Name))
                {
                    Add(issues, "error", "plan.execution_field.forbidden", $"Raw execution/code field is forbidden: {property.Name}", childPath);
                }

                CheckForbiddenExecutionFields(property.Value, childPath, issues);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
            {
                CheckForbiddenExecutionFields(item, $"{path}[{index}]", issues);
                index++;
            }
        }
    }

    private static List<string> ReadJsonArray(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json)?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static IEnumerable<string> Clean(IEnumerable<string>? values)
    {
        return (values ?? Array.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static void Add(List<GeneratorPlanValidationIssue> issues, string severity, string code, string message, string target)
    {
        issues.Add(new GeneratorPlanValidationIssue(severity, code, message, target));
    }
}
