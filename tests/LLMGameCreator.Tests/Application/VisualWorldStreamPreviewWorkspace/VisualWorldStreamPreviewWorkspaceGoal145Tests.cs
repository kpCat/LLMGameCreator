using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal145Tests
{
    [Fact]
    public void Goal145_tab_exposes_in_process_selectable_runtime_session_workflow()
    {
        var root = FindRoot();
        var source = File.ReadAllText(Path.Combine(root,
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal145.cs"));
        Assert.Contains("Goal145 Variant Sessions", source, StringComparison.Ordinal);
        Assert.Contains("Load Candidate Matrix", source, StringComparison.Ordinal);
        Assert.Contains("Start Selected Variant", source, StringComparison.Ordinal);
        Assert.Contains("Execute Selected Action", source, StringComparison.Ordinal);
        Assert.Contains("Run All Variant Sessions", source, StringComparison.Ordinal);
        Assert.Contains("ProductLineInteractiveSessionMatrixOperatorRunner", source, StringComparison.Ordinal);
        Assert.Contains("SelectionChangeCommitted", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectedValueChanged", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProcessStartInfo", source, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet test", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("powershell", source, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (Directory.GetParent(current) is { } parent)
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = parent.FullName;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
