using Xunit;
using Xunit.Sdk;

namespace LLMGameCreator.Tests.Docs;

public sealed class AgentTaskDocsExecutableSpecGuardTests
{
    [Fact]
    public void AllExecutableM41TaskSpecsContainRequiredSections()
    {
        var executableSpecs = ReadExecutableSpecsFromIndex();

        Assert.NotEmpty(executableSpecs);

        foreach (var relativePath in executableSpecs)
        {
            var fullPath = Path.Combine(AgentTaskDocsTestSupport.RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(fullPath), $"Executable task spec is missing: {relativePath}");

            var text = File.ReadAllText(fullPath);
            AgentTaskDocsTestSupport.ValidateExecutableSpec(text, relativePath);
        }
    }

    [Fact]
    public void ExpectedFinalReportFieldsArePresentInExecutableSpecs()
    {
        var executableSpecs = ReadExecutableSpecsFromIndex();

        Assert.NotEmpty(executableSpecs);

        foreach (var relativePath in executableSpecs)
        {
            var fullPath = Path.Combine(AgentTaskDocsTestSupport.RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(fullPath), $"Executable task spec is missing: {relativePath}");

            var text = File.ReadAllText(fullPath);
            AgentTaskDocsTestSupport.ValidateExecutableSpec(text, relativePath, "## Expected final report", "## Next task pointer");
        }
    }

    [Fact]
    public void ExtractExecutableTaskSpecPathsFromIndexExtractsTableRowEndingWithPipe()
    {
        var indexText = @"### M4.1

| Task | Status | Spec |
|---|---|---|
| M4_1_008 | Ready with approval | `M4_1/M4_1_008_AGENT_TASK_DOCS_CONSISTENCY_GUARD.md` |
| M4_1_018 | Support doc | `M4_1/018_EXEC_QUEUE.md` |
";

        var executableSpecs = AgentTaskDocsTestSupport.ExtractExecutableTaskSpecPathsFromIndex(indexText).ToList();

        Assert.Contains("docs/agent-tasks/M4_1/M4_1_008_AGENT_TASK_DOCS_CONSISTENCY_GUARD.md", executableSpecs);
        Assert.DoesNotContain("docs/agent-tasks/M4_1/018_EXEC_QUEUE.md", executableSpecs);
    }

    [Fact]
    public void SupportDocsAreNotTreatedAsExecutableSpecs()
    {
        var executableSpecs = ReadExecutableSpecsFromIndex();

        Assert.DoesNotContain("docs/agent-tasks/M4_1/018_EXEC_QUEUE.md", executableSpecs);
        Assert.DoesNotContain("docs/agent-tasks/M4_1/019_KILO_PROMPTS.md", executableSpecs);
        Assert.DoesNotContain("docs/agent-tasks/M4_1/020_REVIEW_GATE.md", executableSpecs);
    }

    [Fact]
    public void MissingProofTestsSectionFailsWithNamedAssertion()
    {
        var badSpec = @"# TMP

## Header
Task ID: tmp

## Stop conditions
Stop if:
- x

";

        var ex = Assert.Throws<XunitException>(() =>
        {
            AgentTaskDocsTestSupport.ValidateExecutableSpec(badSpec, "temp.md");
        });
        Assert.Contains("# Proof tests", ex.Message);
    }

    [Fact]
    public void MissingSystemGatesSectionFailsWithNamedAssertion()
    {
        var badSpec = @"# TMP

## Proof tests
x

## Stop conditions
Stop if:
- x

";

        var ex = Assert.Throws<XunitException>(() =>
        {
            AgentTaskDocsTestSupport.ValidateExecutableSpec(badSpec, "temp.md");
        });
        Assert.Contains("## System gates", ex.Message);
    }

    [Fact]
    public void MissingStopConditionsSectionFailsWithNamedAssertion()
    {
        var badSpec = @"# TMP

## Proof tests
x

## System gates
x

";

        var ex = Assert.Throws<XunitException>(() =>
        {
            AgentTaskDocsTestSupport.ValidateExecutableSpec(badSpec, "temp.md");
        });
        Assert.Contains("## Stop conditions", ex.Message);
    }

    [Fact]
    public void LockedM5AndM6SpecsRemainLockedWhileCurrentStateBlocksM5M6()
    {
        var currentStateText = File.ReadAllText(Path.Combine(AgentTaskDocsTestSupport.RepoRoot, "docs", "CURRENT_GENERATOR_STATE.md"));
        var isStateBlockingM5M6 = currentStateText.IndexOf("M5", StringComparison.OrdinalIgnoreCase) >= 0
                                  && currentStateText.IndexOf("lock", StringComparison.OrdinalIgnoreCase) >= 0;

        Assert.True(isStateBlockingM5M6, "CURRENT_GENERATOR_STATE.md must reference M5/M6 lock semantics for this guard to verify.");

        var indexText = File.ReadAllText(Path.Combine(AgentTaskDocsTestSupport.DocsAgentTasksDir, "000_INDEX.md"));

        foreach (var line in indexText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.Contains("M5_", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("M6_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (AgentTaskDocsTestSupport.TryExtractSingleSpecPath(line, out var specPath))
            {
                var fullPath = Path.Combine(AgentTaskDocsTestSupport.RepoRoot, specPath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(fullPath))
                {
                    continue;
                }

                var specText = File.ReadAllText(fullPath);
                Assert.Contains("Locked", specText, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private static IReadOnlyList<string> ReadExecutableSpecsFromIndex()
    {
        var indexText = File.ReadAllText(Path.Combine(AgentTaskDocsTestSupport.DocsAgentTasksDir, "000_INDEX.md"));
        return AgentTaskDocsTestSupport.ExtractExecutableTaskSpecPathsFromIndex(indexText);
    }
}
