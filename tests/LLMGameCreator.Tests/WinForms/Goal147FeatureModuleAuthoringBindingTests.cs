using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class Goal147FeatureModuleAuthoringBindingTests
{
    [Fact]
    public void Production_coordinator_derives_dynamic_controls_and_programmatic_binding_stays_clean()
    {
        var root = FindRoot();
        var workspace = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal147-binder-" + Guid.NewGuid().ToString("N"));
        try
        {
            var controller = new FeatureModuleAuthoringWorkbenchController(
                root, SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), workspaceRoot: workspace,
                clock: new FixedClock());
            var library = controller.RefreshLibrary();
            controller.NewComposition("goal147-binder", "Binder", "Binder test");
            Assert.True(controller.Dirty);
            Assert.Equal(1, controller.DirtyTransitionCount);
            Assert.Equal(8, controller.ActiveParameterDefinitions().Count);
            Assert.All(controller.ActiveParameterDefinitions(), parameter => Assert.Contains(parameter.AuthoringControl,
                new[] { "numeric_up_down", "check_box", "combo_box" }));
            controller.Save();
            Assert.False(controller.Dirty);

            controller.BeginProgrammaticBinding();
            controller.SetSelectedModules(library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
                .Select(module => module.ModuleId).ToList());
            controller.EndProgrammaticBinding();
            Assert.False(controller.Dirty);
            Assert.Equal(0, controller.MaterializationInvocationCount);

            controller.SetParameterValue("feature.profile.alchemy_focus", "healingPotionOutput",
                JsonSerializer.SerializeToElement(3));
            controller.SetParameterValue("feature.profile.combat_focus", "basicAttackDamage",
                JsonSerializer.SerializeToElement(5));
            Assert.True(controller.Dirty);
            Assert.Equal(2, controller.DirtyTransitionCount);
            Assert.Equal(0, controller.MaterializationInvocationCount);
        }
        finally { if (Directory.Exists(workspace)) Directory.Delete(workspace, true); }
    }

    [Fact]
    public void Goal147_is_nested_in_goal146_and_starts_no_child_tool_process()
    {
        var root = FindRoot();
        var goal146 = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages",
            "VisualWorldStreamPreviewWorkspace", "VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs"));
        var goal147 = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages",
            "VisualWorldStreamPreviewWorkspace", "VisualWorldStreamPreviewWorkspacePageControl.Goal147.cs"));
        Assert.Contains("ConfigureGoal147AuthoringSurface(innerTabs)", goal146, StringComparison.Ordinal);
        Assert.Contains("Authoring & Saved Compositions", goal147, StringComparison.Ordinal);
        Assert.Contains("Save, Materialize & Qualify", goal147, StringComparison.Ordinal);
        Assert.Contains("NumericUpDown", goal147, StringComparison.Ordinal);
        Assert.Contains("CheckBox", goal147, StringComparison.Ordinal);
        Assert.Contains("ComboBox", goal147, StringComparison.Ordinal);
        Assert.DoesNotContain("_detailTabs.TabPages.Add(_goal147AuthoringTab)", goal147, StringComparison.Ordinal);
        Assert.DoesNotContain("ProcessStartInfo", goal147, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet test", goal147, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("powershell", goal147, StringComparison.OrdinalIgnoreCase);
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

    private sealed class FixedClock : IFeatureModuleAuthoringClock
    {
        public DateTimeOffset UtcNow => new(2026, 7, 11, 11, 0, 0, TimeSpan.Zero);
    }
}
