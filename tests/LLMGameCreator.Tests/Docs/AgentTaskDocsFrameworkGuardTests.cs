using Xunit;

namespace LLMGameCreator.Tests.Docs;

public sealed class AgentTaskDocsFrameworkGuardTests
{
    [Fact]
    public void RequiredFrameworkDocsExist()
    {
        Assert.True(File.Exists(Path.Combine(AgentTaskDocsTestSupport.DocsAgentTasksDir, "000_INDEX.md")), "000_INDEX.md is missing.");
        Assert.True(File.Exists(Path.Combine(AgentTaskDocsTestSupport.DocsAgentTasksDir, "001_TASK_PACK_LEDGER.md")), "001_TASK_PACK_LEDGER.md is missing.");
        Assert.True(File.Exists(Path.Combine(AgentTaskDocsTestSupport.DocsAgentTasksDir, "002_NEXT_PACK_REQUEST.md")), "002_NEXT_PACK_REQUEST.md is missing.");
    }

    [Fact]
    public void IndexReferencesExistingTaskSpecFiles()
    {
        var indexPath = Path.Combine(AgentTaskDocsTestSupport.DocsAgentTasksDir, "000_INDEX.md");
        var indexText = File.ReadAllText(indexPath);

        var referencedSpecs = indexText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .SelectMany(AgentTaskDocsTestSupport.ExtractSpecPaths)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        Assert.NotEmpty(referencedSpecs);

        foreach (var relativePath in referencedSpecs)
        {
            var fullPath = Path.Combine(AgentTaskDocsTestSupport.RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(fullPath), $"Task spec referenced in 000_INDEX.md is missing: {relativePath}");
        }
    }

    [Fact]
    public void NextPackRequestPointsToPackIdAndHasQualityBar()
    {
        var path = Path.Combine(AgentTaskDocsTestSupport.DocsAgentTasksDir, "002_NEXT_PACK_REQUEST.md");
        Assert.True(File.Exists(path), "002_NEXT_PACK_REQUEST.md is missing.");

        var text = File.ReadAllText(path);

        Assert.Contains("agent-task-pack-", text.ToLowerInvariant());
        Assert.Contains("quality bar", text.ToLowerInvariant());
    }

    [Fact]
    public void RequiredM41SupportDocsExist()
    {
        var expected = new[]
        {
            "docs/agent-tasks/M4_1/018_EXEC_QUEUE.md",
            "docs/agent-tasks/M4_1/019_KILO_PROMPTS.md",
            "docs/agent-tasks/M4_1/020_REVIEW_GATE.md"
        };

        foreach (var relativePath in expected)
        {
            var fullPath = Path.Combine(AgentTaskDocsTestSupport.RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(fullPath), $"Required M4.1 support doc is missing: {relativePath}");
        }
    }
}
