using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmArtifactRepairPromptBuilder
{
    private readonly ContentLanguagePromptInstructionProvider _languageInstructionProvider;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public GeneratorPlanStrictLlmArtifactRepairPromptBuilder()
        : this(new ContentLanguagePromptInstructionProvider())
    {
    }

    public GeneratorPlanStrictLlmArtifactRepairPromptBuilder(ContentLanguagePromptInstructionProvider languageInstructionProvider)
    {
        _languageInstructionProvider = languageInstructionProvider ?? throw new ArgumentNullException(nameof(languageInstructionProvider));
    }

    public GeneratorPlanStrictLlmArtifactPrompt BuildRepairPrompt(
        GeneratorPlanStrictLlmArtifactContractDefinition contract,
        GeneratorPlanStrictLlmArtifactPrompt originalPrompt,
        string invalidResponse,
        IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics,
        int attemptIndex,
        string? contentLanguage = null)
    {
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(originalPrompt);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var system = string.Join(Environment.NewLine, new[]
        {
            "You are a strict targeted repair generator for LLMGameCreator.",
            $"Artifact contract id: {contract.ContractId}",
            "Return exactly one corrected JSON object.",
            "Do not wrap the response in Markdown fences.",
            "Do not include explanations, comments, prose, code, scripts, provider instructions, or package mutation instructions.",
            "Do not redesign selected variants, feature bundles, contract id, artifact_kind, or source context.",
            "Content language policy:",
            _languageInstructionProvider.GetInstruction(contentLanguage),
            "C# will validate the repaired output."
        });

        var builder = new StringBuilder();
        builder.AppendLine("Repair the invalid artifact for the same contract.");
        builder.AppendLine($"repair_attempt_index: {attemptIndex}");
        builder.AppendLine();
        builder.AppendLine("Original contract:");
        builder.AppendLine(contract.ContractId);
        builder.AppendLine();
        builder.AppendLine("Exact output schema:");
        builder.AppendLine(contract.OutputSchema.Trim());
        builder.AppendLine();
        builder.AppendLine("Validation diagnostics:");
        builder.AppendLine(JsonSerializer.Serialize(diagnostics, JsonOptions));
        builder.AppendLine();
        builder.AppendLine("Invalid response content:");
        builder.AppendLine(Excerpt(invalidResponse, 12000));
        builder.AppendLine();
        builder.AppendLine("Repair guidance:");
        foreach (var guidance in contract.RepairGuidance)
        {
            builder.AppendLine("- " + guidance);
        }

        builder.AppendLine();
        builder.AppendLine("Return only the corrected JSON object.");

        return new GeneratorPlanStrictLlmArtifactPrompt
        {
            ContractId = contract.ContractId,
            SystemPrompt = system,
            UserPrompt = builder.ToString()
        };
    }

    private static string Excerpt(string value, int maxLength)
    {
        var text = value ?? string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength];
    }
}
