using System.Text.Json;
using LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class StrictLlmDraftArtifactLoopProductSmokeTests
{
    [Fact]
    public async Task Goal034StrictLlmDraftArtifactLoopProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var write = await new StrictLlmDraftArtifactLoopEvidenceService().BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "draft-loop-contract-summary.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "draft-request-matrix.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "candidate-quarantine-matrix.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "repair-request-matrix.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "promotion-decision-matrix.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "strict-draft-plan-frontier.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "strict-draft-plan-gothic.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "strict-draft-plan-caravan.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "strict-draft-plan-metamodule-kingdoms.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "invalid-draft-diagnostics-matrix.json")));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var summary = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, "draft-loop-contract-summary.json")));
        Assert.Equal(9, summary.RootElement.GetProperty("familyCount").GetInt32());
        Assert.Contains("strict_llm_draft_artifact_loop_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));
    }

    private static string ResolveProjectFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
