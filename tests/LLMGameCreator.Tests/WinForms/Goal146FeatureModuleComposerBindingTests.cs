using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class Goal146FeatureModuleComposerBindingTests
{
    [Fact]
    public void Programmatic_module_checks_do_not_invoke_materialization()
    {
        var root = FindRoot();
        var service = new FeatureModuleCompositionService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault());
        var controller = new FeatureModuleCompositionWorkbenchController(service, new FeatureModuleCompositionOperatorRunner(service));
        controller.LoadCatalog(root);
        controller.SetSelectedOptionalModules(FeatureModuleCompositionVocabulary.OptionalModuleIds);

        Assert.Equal(0, controller.MaterializationInvocationCount);
        Assert.True(controller.ValidateSelection().Passed);
    }

    [Fact]
    public void Goal146_page_uses_button_commit_and_starts_no_child_tool_process()
    {
        var source = File.ReadAllText(Path.Combine(FindRoot(), "src", "LLMGameCreator.WinForms", "Pages",
            "VisualWorldStreamPreviewWorkspace", "VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs"));
        Assert.Contains("Goal146 Module Composer", source, StringComparison.Ordinal);
        Assert.Contains("Run Composition Matrix", source, StringComparison.Ordinal);
        Assert.Contains("FeatureModuleCompositionOperatorRunner", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ItemCheck +=", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProcessStartInfo", source, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet test", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("powershell", source, StringComparison.OrdinalIgnoreCase);
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
