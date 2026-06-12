using System.Text.Json;
using System.Text.Json.Nodes;

namespace LLMGameCreator.Application.Design;

public sealed class GeneratorPlanPreviewService : IGeneratorPlanPreviewService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    private readonly IGeneratorPlanRepository _planRepository;
    private readonly IGeneratorLibraryRegistry _registry;
    private readonly IGeneratorPlanReviewService _reviewService;
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanPreviewService(
        IGeneratorPlanRepository planRepository,
        IGeneratorLibraryRegistry registry,
        IGeneratorPlanReviewService reviewService,
        IGeneratedArtifactRepository artifactRepository)
    {
        _planRepository = planRepository;
        _registry = registry;
        _reviewService = reviewService;
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanPreviewResult> CreatePreviewArtifactAsync(GeneratorPlanPreviewRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PlanId))
        {
            return Failure(null, Array.Empty<GeneratorPlanStepRecord>(), "Plan id is required.", "preview.plan.id.empty", "plan");
        }

        var plan = await _planRepository.GetGeneratorPlanByIdAsync(request.PlanId, cancellationToken).ConfigureAwait(false);
        if (plan == null)
        {
            return Failure(null, Array.Empty<GeneratorPlanStepRecord>(), $"Plan was not found: {request.PlanId}", "preview.plan.not_found", request.PlanId);
        }

        var steps = await _planRepository.GetGeneratorPlanStepsAsync(plan.Id, cancellationToken).ConfigureAwait(false);
        if (!plan.Status.Equals("approved", StringComparison.OrdinalIgnoreCase))
        {
            return Failure(plan, steps, "Only approved plans can create preview artifacts.", "preview.plan.not_approved", plan.Id);
        }

        var review = await _reviewService.RevalidatePlanAsync(plan.Id, cancellationToken).ConfigureAwait(false);
        var validationIssues = review.ValidationIssues;
        var validationResults = ToPreviewValidationResults(BuildArtifactId(plan.Id), validationIssues, request.IncludeWarnings);
        if (HasErrors(validationIssues))
        {
            return new GeneratorPlanPreviewResult(
                review.Plan ?? plan,
                review.Steps.Count == 0 ? steps : review.Steps,
                null,
                validationResults,
                validationIssues,
                false,
                "Plan has validation errors; preview artifact was not saved.");
        }

        var modules = await _registry.ListModulesAsync(cancellationToken).ConfigureAwait(false);
        var modulesById = modules.ToDictionary(module => module.Id, StringComparer.OrdinalIgnoreCase);
        var missingModules = steps
            .Where(step => !modulesById.ContainsKey(step.ModuleId))
            .Select(step => step.ModuleId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (missingModules.Count > 0)
        {
            var missingResults = missingModules
                .Select((moduleId, index) => new GeneratedArtifactValidationResultRecord(
                    BuildValidationResultId(BuildArtifactId(plan.Id), "preview.module.path.missing_in_registry", index),
                    BuildArtifactId(plan.Id),
                    "error",
                    "preview.module.path.missing_in_registry",
                    $"Selected module is not available in the current registry: {moduleId}",
                    moduleId,
                    "{}"))
                .ToList();
            return new GeneratorPlanPreviewResult(
                plan,
                steps,
                null,
                validationResults.Concat(missingResults).ToList(),
                validationIssues,
                false,
                "Selected modules are missing from the current registry; preview artifact was not saved.");
        }

        validationResults = validationResults.Concat(new[]
        {
            new GeneratedArtifactValidationResultRecord(
                BuildValidationResultId(BuildArtifactId(plan.Id), "preview.policy.no_execution", validationResults.Count),
                BuildArtifactId(plan.Id),
                "warning",
                "preview.policy.no_execution",
                "Preview staging records module metadata only; Lua, modules, codegen and GamePackage mutation are disabled.",
                plan.Id,
                "{}")
        }).ToList();

        var artifactId = BuildArtifactId(plan.Id);
        var previewJson = BuildPreviewJson(plan, steps, modulesById);
        var validationState = ValidationState(validationResults);
        var artifact = new GeneratedArtifactRecord(
            artifactId,
            "generator_plan_preview",
            $"design-db://generator-plans/{plan.Id}/preview",
            previewJson,
            plan.Id,
            validationState,
            BuildMetadataJson(plan, steps.Count, validationResults));

        await _artifactRepository.SaveGeneratedArtifactAsync(artifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(artifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        return new GeneratorPlanPreviewResult(
            plan,
            steps,
            artifact,
            validationResults,
            validationIssues,
            true,
            $"Preview artifact saved: {artifact.Path}");
    }

    private static GeneratorPlanPreviewResult Failure(
        GeneratorPlanRecord? plan,
        IReadOnlyList<GeneratorPlanStepRecord> steps,
        string message,
        string code,
        string target)
    {
        var artifactId = plan == null ? "artifact/generator-plan-preview/missing" : BuildArtifactId(plan.Id);
        var issue = new GeneratorPlanValidationIssue("error", code, message, target);
        var result = new GeneratedArtifactValidationResultRecord(
            BuildValidationResultId(artifactId, code, 0),
            artifactId,
            "error",
            code,
            message,
            target,
            "{}");

        return new GeneratorPlanPreviewResult(plan, steps, null, new[] { result }, new[] { issue }, false, message);
    }

    private static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToPreviewValidationResults(
        string artifactId,
        IReadOnlyList<GeneratorPlanValidationIssue> issues,
        bool includeWarnings)
    {
        return issues
            .Where(issue => issue.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)
                || (includeWarnings && issue.Severity.Equals("warning", StringComparison.OrdinalIgnoreCase)))
            .Select((issue, index) => new GeneratedArtifactValidationResultRecord(
                BuildValidationResultId(artifactId, issue.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)
                    ? "preview.plan.validation_error"
                    : "preview.plan.validation_warning", index),
                artifactId,
                issue.Severity.Equals("error", StringComparison.OrdinalIgnoreCase) ? "error" : "warning",
                issue.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)
                    ? "preview.plan.validation_error"
                    : "preview.plan.validation_warning",
                issue.Message,
                issue.Target,
                JsonSerializer.Serialize(new { source_code = issue.Code }, JsonOptions)))
            .ToList();
    }

    private static string BuildPreviewJson(
        GeneratorPlanRecord plan,
        IReadOnlyList<GeneratorPlanStepRecord> steps,
        IReadOnlyDictionary<string, GeneratorModuleRecord> modulesById)
    {
        var root = new JsonObject
        {
            ["kind"] = "generator_plan_preview",
            ["schema_version"] = 1,
            ["plan"] = new JsonObject
            {
                ["id"] = plan.Id,
                ["title"] = plan.Title,
                ["goal"] = plan.Goal,
                ["status"] = plan.Status
            },
            ["steps"] = new JsonArray(steps
                .OrderBy(step => step.StepOrder)
                .ThenBy(step => step.ModuleId, StringComparer.OrdinalIgnoreCase)
                .Select(step => BuildStepJson(step, modulesById))
                .ToArray<JsonNode?>()),
            ["execution_policy"] = new JsonObject
            {
                ["lua_execution"] = false,
                ["module_execution"] = false,
                ["game_package_mutation"] = false,
                ["codegen_execution"] = false
            }
        };

        return root.ToJsonString(JsonOptions);
    }

    private static JsonObject BuildStepJson(GeneratorPlanStepRecord step, IReadOnlyDictionary<string, GeneratorModuleRecord> modulesById)
    {
        modulesById.TryGetValue(step.ModuleId, out var module);
        return new JsonObject
        {
            ["order"] = step.StepOrder,
            ["module_id"] = step.ModuleId,
            ["module_path"] = module?.Path ?? string.Empty,
            ["category"] = module?.Category ?? string.Empty,
            ["config"] = ParseObjectOrEmpty(step.ConfigJson),
            ["depends_on"] = ParseStringArray(step.DependsOnJson)
        };
    }

    private static JsonNode ParseObjectOrEmpty(string json)
    {
        try
        {
            var node = JsonNode.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            return node is JsonObject ? node : new JsonObject();
        }
        catch (JsonException)
        {
            return new JsonObject();
        }
    }

    private static JsonArray ParseStringArray(string json)
    {
        try
        {
            var values = JsonSerializer.Deserialize<List<string>>(string.IsNullOrWhiteSpace(json) ? "[]" : json)
                ?? new List<string>();
            return new JsonArray(values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => JsonValue.Create(value.Trim())).ToArray<JsonNode?>());
        }
        catch (JsonException)
        {
            return new JsonArray();
        }
    }

    private static string BuildMetadataJson(
        GeneratorPlanRecord plan,
        int moduleCount,
        IReadOnlyList<GeneratedArtifactValidationResultRecord> validationResults)
    {
        var errorCount = validationResults.Count(result => result.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
        var warningCount = validationResults.Count(result => result.Severity.Equals("warning", StringComparison.OrdinalIgnoreCase));
        return JsonSerializer.Serialize(new
        {
            created_utc = DateTimeOffset.UtcNow.ToString("O"),
            plan_status = plan.Status,
            module_count = moduleCount,
            warning_count = warningCount,
            error_count = errorCount
        }, JsonOptions);
    }

    private static string ValidationState(IReadOnlyList<GeneratedArtifactValidationResultRecord> validationResults)
    {
        if (validationResults.Any(result => result.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)))
        {
            return "invalid";
        }

        return validationResults.Any(result => result.Severity.Equals("warning", StringComparison.OrdinalIgnoreCase))
            ? "warning"
            : "valid";
    }

    private static bool HasErrors(IReadOnlyList<GeneratorPlanValidationIssue> issues)
    {
        return issues.Any(issue => issue.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildArtifactId(string planId)
    {
        return $"artifact/generator-plan-preview/{planId}";
    }

    private static string BuildValidationResultId(string artifactId, string code, int index)
    {
        return $"{artifactId}/validation/{index:D3}/{code}";
    }
}
