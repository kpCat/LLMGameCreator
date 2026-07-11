using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime;
using LLMGameCreator.Tests.Application.FeatureModuleComposition;
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
        var catalog = controller.LoadCatalog(root);
        controller.SetSelectedOptionalModules(catalog.Modules.Where(module => module.Selectable && !module.Required)
            .Select(module => module.ModuleId).ToList());

        Assert.Equal(0, controller.MaterializationInvocationCount);
        Assert.True(controller.ValidateSelection().Passed);
    }

    [Fact]
    public void Synthetic_fourth_optional_module_is_dynamic_and_button_collection_requires_no_branch()
    {
        var root = FindRoot();
        var service = new FeatureModuleCompositionService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault());
        var controller = new FeatureModuleCompositionWorkbenchController(service, new FeatureModuleCompositionOperatorRunner(service));
        var catalog = FeatureModuleCatalog.LoadFromGoal142(root, FeatureModuleCompositionVocabulary.Goal142Root);
        var synthetic = FeatureModuleCompositionTests.SyntheticFuelModule();
        catalog = FeatureModuleCompositionTests.AppendOptional(catalog, synthetic);
        var selected = FeatureModuleCompositionTests.Optional(catalog).Select(module => module.ModuleId).ToList();

        controller.BindCatalog(catalog);
        controller.SetSelectedOptionalModules(selected);

        Assert.Contains(synthetic, catalog.Modules);
        Assert.Contains(synthetic.ModuleId, controller.SelectedOptionalModuleIds);
        Assert.Equal(0, controller.MaterializationInvocationCount);
        Assert.True(controller.ValidateSelection().Passed);
        var source = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages",
            "VisualWorldStreamPreviewWorkspace", "VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs"));
        Assert.DoesNotContain(synthetic.ModuleId, source, StringComparison.Ordinal);
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
