using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal146Tests
{
    [Fact]
    public void Workspace_registers_the_goal146_module_composer_tab_and_primary_action()
    {
        var root = FindRoot();
        var page = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages",
            "VisualWorldStreamPreviewWorkspace", "VisualWorldStreamPreviewWorkspacePageControl.cs"));
        var goal146 = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages",
            "VisualWorldStreamPreviewWorkspace", "VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs"));
        Assert.Contains("ConfigureGoal146ModuleComposerPanel", page, StringComparison.Ordinal);
        Assert.Contains("WireGoal146ModuleComposerEvents", page, StringComparison.Ordinal);
        Assert.Contains("BindGoal146ModuleComposer", page, StringComparison.Ordinal);
        Assert.Contains("Run Composition Matrix", goal146, StringComparison.Ordinal);
        Assert.Contains("Load FeatureModule Catalog", goal146, StringComparison.Ordinal);
        Assert.Contains("Materialize & Qualify Selected Composition", goal146, StringComparison.Ordinal);
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
