using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmArtifactPromptBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public GeneratorPlanStrictLlmArtifactPrompt Build(
        GeneratorPlanStrictLlmArtifactContractDefinition contract,
        GeneratorPlanCapabilitySelection selection,
        GeneratorPlanStrictLlmArtifactGenerationRequest request)
    {
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(request);

        return new GeneratorPlanStrictLlmArtifactPrompt
        {
            ContractId = contract.ContractId,
            SystemPrompt = BuildSystemPrompt(contract),
            UserPrompt = BuildUserPrompt(contract, selection, request)
        };
    }

    private static string BuildSystemPrompt(GeneratorPlanStrictLlmArtifactContractDefinition contract)
    {
        var lines = new List<string>
        {
            "You are a strict artifact producer for LLMGameCreator.",
            $"Artifact contract id: {contract.ContractId}",
            "Return exactly one JSON object.",
            "Do not wrap the response in Markdown fences.",
            "Do not include explanations, comments, prose, or text before or after the JSON object.",
            "Do not output code, scripts, C#, Lua, SQL, PowerShell, shell commands, provider instructions, tool instructions, eval, execute, or package mutation instructions.",
            "Do not mutate a GamePackage. The output is a draft artifact only.",
            "Preserve machine-readable ids and enum values exactly. Do not translate ids or enums.",
            "C# will parse and validate this output. Invalid output will be rejected or sent to a bounded repair prompt."
        };

        lines.AddRange(contract.SystemPromptAdditions);
        return string.Join(Environment.NewLine, lines);
    }

    private static string BuildUserPrompt(
        GeneratorPlanStrictLlmArtifactContractDefinition contract,
        GeneratorPlanCapabilitySelection selection,
        GeneratorPlanStrictLlmArtifactGenerationRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Produce one strict JSON artifact for the selected capability selection.");
        builder.AppendLine();
        builder.AppendLine("Capability selection summary:");
        builder.AppendLine($"selection_id: {EmptyAsDash(selection.SelectionId)}");
        builder.AppendLine($"title: {EmptyAsDash(selection.Title)}");
        builder.AppendLine($"purpose: {EmptyAsDash(selection.Purpose)}");
        builder.AppendLine("selected_variant_ids:");
        builder.AppendLine(JsonSerializer.Serialize(selection.SelectedVariantIds, JsonOptions));
        builder.AppendLine("selected_feature_bundle_ids:");
        builder.AppendLine(JsonSerializer.Serialize(selection.SelectedFeatureBundleIds, JsonOptions));
        AppendOptionalArray(builder, "selected_module_ids", selection.SelectedModuleIds);
        AppendOptionalArray(builder, "selected_modifier_ids", selection.SelectedModifierIds);
        AppendOptionalArray(builder, "selected_constraint_ids", selection.SelectedConstraintIds);
        AppendOptionalArray(builder, "runtime_requirement_ids", selection.RuntimeRequirementIds);
        builder.AppendLine("resolved_artifact_contracts:");
        builder.AppendLine(JsonSerializer.Serialize(selection.ResolvedArtifactContracts, JsonOptions));
        builder.AppendLine("resolved_validators:");
        builder.AppendLine(JsonSerializer.Serialize(selection.ResolvedValidators, JsonOptions));
        builder.AppendLine("resolved_runtime_targets:");
        builder.AppendLine(JsonSerializer.Serialize(selection.ResolvedRuntimeTargets, JsonOptions));
        builder.AppendLine("capability_gaps_or_warnings:");
        builder.AppendLine(JsonSerializer.Serialize(selection.RequiredLuaModulesOrGaps.Concat(selection.Warnings).Concat(selection.Errors).ToList(), JsonOptions));
        builder.AppendLine();
        builder.AppendLine("Selected contract:");
        builder.AppendLine(contract.ContractId);
        builder.AppendLine();
        builder.AppendLine("Required top-level fields:");
        builder.AppendLine(JsonSerializer.Serialize(contract.RequiredTopLevelFields, JsonOptions));
        builder.AppendLine("Required payload fields:");
        builder.AppendLine(JsonSerializer.Serialize(contract.RequiredPayloadFields, JsonOptions));
        builder.AppendLine("Validation rules:");
        builder.AppendLine(JsonSerializer.Serialize(contract.ValidationRules, JsonOptions));
        builder.AppendLine();
        builder.AppendLine("Exact output schema:");
        builder.AppendLine(contract.OutputSchema.Trim());

        if (!string.IsNullOrWhiteSpace(request.ExtraUserBrief))
        {
            builder.AppendLine();
            builder.AppendLine("Extra user brief:");
            builder.AppendLine(request.ExtraUserBrief.Trim());
        }

        builder.AppendLine();
        builder.AppendLine("Forbidden outputs:");
        builder.AppendLine("- markdown");
        builder.AppendLine("- code");
        builder.AppendLine("- scripts");
        builder.AppendLine("- C#, Lua, SQL, PowerShell, commands, eval, execute");
        builder.AppendLine("- provider or tool instructions");
        builder.AppendLine("- GamePackage mutation or package export");
        builder.AppendLine("- multiple contracts mixed in one artifact");
        builder.AppendLine();
        builder.AppendLine("Return only the JSON object.");

        return builder.ToString();
    }

    private static string EmptyAsDash(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static void AppendOptionalArray(StringBuilder builder, string label, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        builder.AppendLine(label + ":");
        builder.AppendLine(JsonSerializer.Serialize(values, JsonOptions));
    }
}
