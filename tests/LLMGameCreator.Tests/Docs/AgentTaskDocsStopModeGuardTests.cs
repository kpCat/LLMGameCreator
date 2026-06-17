using Xunit;

namespace LLMGameCreator.Tests.Docs;

public sealed class AgentTaskDocsStopModeGuardTests
{
    private const string ValidStopNextTask = @"# NEXT_TASK

Mode: stop
Task source: stop
Task id: STOP_REVIEW
Reason: M4.1 deterministic hardening queue reached the human review gate.
User approval:
Expected stop after completion: yes
Stop action: Do not start future work. Review the completed task, check-all output, and whether M4.1 gate review should continue.
";

    private const string StopNextTaskWithoutStopAction = @"# NEXT_TASK

Mode: stop
Task source: stop
Task id: STOP_REVIEW
Reason: M4.1 deterministic hardening queue reached the human review gate.
User approval:
Expected stop after completion: yes
";

    [Fact]
    public void StopModeNextTaskRequiresStopReview()
    {
        var text = ReadNextTask();

        Assert.Contains("Mode: stop", text);
        Assert.Contains("Task id: STOP_REVIEW", text);
    }

    [Fact]
    public void StopModeNextTaskRequiresStopAction()
    {
        var text = ReadNextTask();

        Assert.Contains("Stop action:", text);
        Assert.Contains("Do not start future work", text);
    }

    [Fact]
    public void StopModeNextTaskDoesNotRequireTaskSpecFile()
    {
        var text = ReadNextTask();

        Assert.DoesNotContain("Task spec file:", text);
    }

    [Fact]
    public void CheckDevflowStateAcceptsValidStopMode()
    {
        var result = AgentTaskDocsTestSupport.RunCheckDevflowStateWithTemporaryNextTask(ValidStopNextTask);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Current mode: stop", result.StandardOutput);
        Assert.DoesNotContain("NEXT_TASK.md does not clearly contain a task id", result.StandardOutput + result.StandardError);
    }

    [Fact]
    public void CheckDevflowStateFailsInvalidStopModeWithoutStopAction()
    {
        var result = AgentTaskDocsTestSupport.RunCheckDevflowStateWithTemporaryNextTask(StopNextTaskWithoutStopAction);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("Stop action", result.StandardOutput + result.StandardError, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadNextTask()
    {
        return File.ReadAllText(Path.Combine(AgentTaskDocsTestSupport.RepoRoot, ".devflow", "NEXT_TASK.md"));
    }
}
