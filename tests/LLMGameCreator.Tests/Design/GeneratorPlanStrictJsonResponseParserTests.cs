using System.IO;
using System.Reflection;
using LLMGameCreator.Application.Design.GeneratorPlans;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanStrictJsonResponseParserTests
{
    private static string FixturePath(string name)
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyPath)!;
        var projectDir = Directory.GetParent(assemblyDir)!.Parent!.Parent!.Parent!.FullName;
        return Path.Combine(projectDir, "fixtures", "strict-llm-raw-output", name + ".txt");
    }

    private static string ReadFixture(string name) => File.ReadAllText(FixturePath(name));

    [Fact]
    public void AcceptsSingleJsonObject()
    {
        var result = new GeneratorPlanStrictJsonResponseParser().Parse("{\"schema_version\":\"0.1\"}");

        Assert.True(result.Ok);
        Assert.Equal("{\"schema_version\":\"0.1\"}", result.Json);
    }

    [Fact]
    public void RejectsMarkdownFence()
    {
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(@"
```json
{""schema_version"":""0.1""}
```
");

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonMarkdownFence);
    }

    [Fact]
    public void RejectsTextBeforeOrAfterJson()
    {
        var result = new GeneratorPlanStrictJsonResponseParser().Parse("Here: {\"schema_version\":\"0.1\"}");

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonTextWrapper);
    }

    [Fact]
    public void RejectsJsonArrayRoot()
    {
        var result = new GeneratorPlanStrictJsonResponseParser().Parse("[]");

        Assert.False(result.Ok);
    }

    [Fact]
    public void RejectsInvalidJson()
    {
        var result = new GeneratorPlanStrictJsonResponseParser().Parse("{\"schema_version\":\"");

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid);
    }

    [Fact]
    public void Fixture_ValidMinimalJsonObject_ReturnsOk()
    {
        var fixture = ReadFixture("valid_minimal_json_object");
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(fixture);

        Assert.True(result.Ok);
        Assert.Equal(fixture, result.Json);
    }

    [Fact]
    public void Fixture_EmptyResponse_ReturnsJsonInvalid()
    {
        var fixture = ReadFixture("empty_response");
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(fixture);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, d => d.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid);
    }

    [Fact]
    public void Fixture_MarkdownFencedJson_ReturnsJsonMarkdownFence()
    {
        var fixture = ReadFixture("markdown_fenced_json");
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(fixture);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, d => d.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonMarkdownFence);
    }

    [Fact]
    public void Fixture_TextBeforeJson_ReturnsJsonTextWrapper()
    {
        var fixture = ReadFixture("text_before_json");
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(fixture);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, d => d.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonTextWrapper);
    }

    [Fact]
    public void Fixture_TextAfterJson_ReturnsJsonTextWrapper()
    {
        var fixture = ReadFixture("text_after_json");
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(fixture);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, d => d.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonTextWrapper);
    }

    [Fact]
    public void Fixture_TwoJsonObjects_ReturnsDeterministicError()
    {
        var fixture = ReadFixture("two_json_objects");
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(fixture);

        Assert.False(result.Ok);
        Assert.Single(result.Diagnostics);
    }

    [Fact]
    public void Fixture_JsonArrayRoot_ReturnsJsonRootNotObject()
    {
        var fixture = ReadFixture("json_array_root");
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(fixture);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, d => d.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonRootNotObject);
    }

    [Fact]
    public void Fixture_BrokenTrailingComma_ReturnsJsonInvalid()
    {
        var fixture = ReadFixture("broken_trailing_comma");
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(fixture);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, d => d.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid);
    }

    [Fact]
    public void Fixture_InvalidEscape_ReturnsJsonInvalid()
    {
        var fixture = ReadFixture("invalid_escape");
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(fixture);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, d => d.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid);
    }

    [Fact]
    public void Diagnostic_PreservesContractId()
    {
        var fixture = ReadFixture("markdown_fenced_json");
        var result = new GeneratorPlanStrictJsonResponseParser().Parse(fixture, "test.contract.v1");

        Assert.False(result.Ok);
        var diag = Assert.Single(result.Diagnostics);
        Assert.Equal("test.contract.v1", diag.ContractId);
    }
}