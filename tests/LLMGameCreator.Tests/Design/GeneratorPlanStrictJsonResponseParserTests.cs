using LLMGameCreator.Application.Design.GeneratorPlans;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanStrictJsonResponseParserTests
{
    [Fact]
    public void AcceptsSingleJsonObject()
    {
        var result = new GeneratorPlanStrictJsonResponseParser().Parse("""{"schema_version":"0.1"}""");

        Assert.True(result.Ok);
        Assert.Equal("""{"schema_version":"0.1"}""", result.Json);
    }

    [Fact]
    public void RejectsMarkdownFence()
    {
        var result = new GeneratorPlanStrictJsonResponseParser().Parse("""
        ```json
        {"schema_version":"0.1"}
        ```
        """);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonMarkdownFence);
    }

    [Fact]
    public void RejectsTextBeforeOrAfterJson()
    {
        var result = new GeneratorPlanStrictJsonResponseParser().Parse("""Here: {"schema_version":"0.1"}""");

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonTextWrapper);
    }

    [Fact]
    public void RejectsJsonArrayRoot()
    {
        var result = new GeneratorPlanStrictJsonResponseParser().Parse("""[]""");

        Assert.False(result.Ok);
    }

    [Fact]
    public void RejectsInvalidJson()
    {
        var result = new GeneratorPlanStrictJsonResponseParser().Parse("""{"schema_version":""");

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid);
    }
}
