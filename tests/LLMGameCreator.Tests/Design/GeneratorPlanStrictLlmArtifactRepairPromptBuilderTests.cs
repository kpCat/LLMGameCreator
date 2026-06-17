using LLMGameCreator.Application.Design.GeneratorPlans;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanStrictLlmArtifactRepairPromptBuilderTests
{
    private readonly GeneratorPlanStrictLlmArtifactContractCatalog _catalog = new();
    private readonly GeneratorPlanStrictLlmArtifactRepairPromptBuilder _builder = new();

    [Fact]
    public void BuildRepairPromptContainsContractId()
    {
        var contract = Contract("game_profile_v1");
        var originalPrompt = CreatePrompt(contract.ContractId);
        var diagnostics = new[]
        {
            new GeneratorPlanStrictLlmArtifactDiagnostic
            {
                Severity = GeneratorPlanPreviewDiagnosticSeverity.Error,
                Code = "test_code",
                Message = "Test message",
                Target = "test_target",
                ContractId = contract.ContractId
            }
        };

        var prompt = _builder.BuildRepairPrompt(contract, originalPrompt, "{\"test\": true}", diagnostics, 1);

        Assert.Contains(contract.ContractId, prompt.SystemPrompt);
        Assert.Contains(contract.ContractId, prompt.UserPrompt);
        Assert.Equal(contract.ContractId, prompt.ContractId);
    }

    [Fact]
    public void BuildRepairPromptContainsExactOutputSchema()
    {
        var contract = Contract("game_profile_v1");
        var originalPrompt = CreatePrompt(contract.ContractId);
        var diagnostics = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();

        var prompt = _builder.BuildRepairPrompt(contract, originalPrompt, "{}", diagnostics, 0);

        Assert.Contains("Exact output schema:", prompt.UserPrompt);
        Assert.Contains(contract.OutputSchema.Trim(), prompt.UserPrompt);
    }

    [Fact]
    public void BuildRepairPromptContainsSerializedDiagnostics()
    {
        var contract = Contract("game_profile_v1");
        var originalPrompt = CreatePrompt(contract.ContractId);
        var diagnostics = new[]
        {
            new GeneratorPlanStrictLlmArtifactDiagnostic
            {
                Severity = GeneratorPlanPreviewDiagnosticSeverity.Error,
                Code = GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingField,
                Message = "Missing required field 'game'.",
                Target = "game",
                ContractId = contract.ContractId
            }
        };

        var prompt = _builder.BuildRepairPrompt(contract, originalPrompt, "{}", diagnostics, 0);

        Assert.Contains("Validation diagnostics:", prompt.UserPrompt);
        Assert.Contains(GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingField, prompt.UserPrompt);
        Assert.Contains("\"target\": \"game\"", prompt.UserPrompt);
    }

    [Fact]
    public void BuildRepairPromptTruncatesOverlongInvalidResponse()
    {
        var contract = Contract("game_profile_v1");
        var originalPrompt = CreatePrompt(contract.ContractId);
        var diagnostics = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();
        var overlongResponse = new string('x', 15000);

        var prompt = _builder.BuildRepairPrompt(contract, originalPrompt, overlongResponse, diagnostics, 0);

        Assert.Contains("Invalid response content:", prompt.UserPrompt);
        var contentAfterLabel = prompt.UserPrompt.Substring(prompt.UserPrompt.IndexOf("Invalid response content:\n") + "Invalid response content:\n".Length);
        var linesAfterContent = contentAfterLabel.Split('\n');
        var contentLine = linesAfterContent[0];
        Assert.True(contentLine.Length <= 12000, $"Content line length {contentLine.Length} exceeds 12000");
    }

    [Fact]
    public void BuildRepairPromptForbidsMarkdownAndProse()
    {
        var contract = Contract("game_profile_v1");
        var originalPrompt = CreatePrompt(contract.ContractId);
        var diagnostics = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();

        var prompt = _builder.BuildRepairPrompt(contract, originalPrompt, "{}", diagnostics, 0);

        Assert.Contains("Do not wrap the response in Markdown fences.", prompt.SystemPrompt);
        Assert.Contains("Do not include explanations, comments, prose, code, scripts, provider instructions, or package mutation instructions.", prompt.SystemPrompt);
        Assert.Contains("prose", prompt.SystemPrompt);
        Assert.Contains("scripts", prompt.SystemPrompt);
    }

    [Fact]
    public void BuildRepairPromptForbidsPackageMutation()
    {
        var contract = Contract("game_profile_v1");
        var originalPrompt = CreatePrompt(contract.ContractId);
        var diagnostics = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();

        var prompt = _builder.BuildRepairPrompt(contract, originalPrompt, "{}", diagnostics, 0);

        Assert.Contains("Do not redesign selected variants, feature bundles, contract id, artifact_kind, or source context.", prompt.SystemPrompt);
    }

    [Fact]
    public void BuildRepairPromptPreservesContractIdAndDoesNotAskToRedesignSelectedVariants()
    {
        var contract = Contract("game_profile_v1");
        var originalPrompt = CreatePrompt(contract.ContractId);
        var diagnostics = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();

        var prompt = _builder.BuildRepairPrompt(contract, originalPrompt, "{}", diagnostics, 0);

        Assert.DoesNotContain("redesign", prompt.UserPrompt.ToLowerInvariant());
        Assert.Contains("Original contract:", prompt.UserPrompt);
        Assert.DoesNotContain("Change selected variant", prompt.UserPrompt);
        Assert.DoesNotContain("Change artifact_kind", prompt.UserPrompt);
        Assert.DoesNotContain("Change source_context", prompt.UserPrompt);
    }

    [Fact]
    public void BuildRepairPromptThrowsOnNullContract()
    {
        var originalPrompt = CreatePrompt("game_profile_v1");
        var diagnostics = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();

        Assert.Throws<ArgumentNullException>(() => _builder.BuildRepairPrompt(null!, originalPrompt, "{}", diagnostics, 0));
    }

    [Fact]
    public void BuildRepairPromptThrowsOnNullOriginalPrompt()
    {
        var contract = Contract("game_profile_v1");
        var diagnostics = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();

        Assert.Throws<ArgumentNullException>(() => _builder.BuildRepairPrompt(contract, null!, "{}", diagnostics, 0));
    }

    [Fact]
    public void BuildRepairPromptThrowsOnNullDiagnostics()
    {
        var contract = Contract("game_profile_v1");
        var originalPrompt = CreatePrompt(contract.ContractId);

        Assert.Throws<ArgumentNullException>(() => _builder.BuildRepairPrompt(contract, originalPrompt, "{}", null!, 0));
    }

    [Fact]
    public void BuildRepairPromptIncludesRepairGuidance()
    {
        var contract = Contract("game_profile_v1");
        var originalPrompt = CreatePrompt(contract.ContractId);
        var diagnostics = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();

        var prompt = _builder.BuildRepairPrompt(contract, originalPrompt, "{}", diagnostics, 0);

        Assert.Contains("Repair guidance:", prompt.UserPrompt);
        foreach (var guidance in contract.RepairGuidance)
        {
            Assert.Contains(guidance, prompt.UserPrompt);
        }
    }

    [Fact]
    public void BuildRepairPromptIncludesRepairAttemptIndex()
    {
        var contract = Contract("game_profile_v1");
        var originalPrompt = CreatePrompt(contract.ContractId);
        var diagnostics = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();

        var prompt = _builder.BuildRepairPrompt(contract, originalPrompt, "{}", diagnostics, 2);

        Assert.Contains("repair_attempt_index: 2", prompt.UserPrompt);
    }

    private GeneratorPlanStrictLlmArtifactContractDefinition Contract(string id)
    {
        Assert.True(_catalog.TryGet(id, out var contract));
        return contract;
    }

    private static GeneratorPlanStrictLlmArtifactPrompt CreatePrompt(string contractId)
    {
        return new GeneratorPlanStrictLlmArtifactPrompt
        {
            ContractId = contractId,
            SystemPrompt = "Original system prompt",
            UserPrompt = "Original user prompt"
        };
    }
}