using Xunit;

namespace LLMGameCreator.Tests.Devflow;

public sealed class RunCapabilityRuntimeEquipmentSliceScriptTests
{
    [Fact]
    public void Goal149_runner_is_bounded_in_process_and_requires_green_unaccepted_dashboard()
    {
        var root = FindRoot();
        var script = File.ReadAllText(Path.Combine(root, ".devflow", "scripts",
            "run-capability-runtime-equipment-slice.ps1"));
        Assert.Contains("FullyQualifiedName~Goal149", script, StringComparison.Ordinal);
        Assert.Contains("GOAL149_CAPABILITY_RUNTIME_EQUIPMENT_GREEN", script, StringComparison.Ordinal);
        Assert.Contains("capability-runtime-playthrough-dashboard.json", script, StringComparison.Ordinal);
        Assert.Contains("goal149Accepted", script, StringComparison.Ordinal);
        Assert.Contains("Restore-Goal149Directory", script, StringComparison.Ordinal);
        Assert.DoesNotContain("Start-Process", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".llmgc/manual", script.Replace("refuses .llmgc/manual", string.Empty),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
