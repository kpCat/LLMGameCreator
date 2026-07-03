namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    public VisualWorldPreviewWinFormsBindingInventory BuildWinFormsBindingInventory(
        string projectRootPath)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var pageRelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.cs";
        var designerRelativePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Designer.cs";
        var compositionRelativePath = "src/LLMGameCreator.WinForms/CompositionRoot.cs";
        var pageText = ReadOptionalText(projectRoot, pageRelativePath);
        var designerText = ReadOptionalText(projectRoot, designerRelativePath);
        var compositionText = ReadOptionalText(projectRoot, compositionRelativePath);
        var diagnostics = new List<VisualWorldPreviewDiagnostic>();

        var pageExists = pageText.Length > 0;
        var designerExists = designerText.Length > 0;
        var serviceRegistered = compositionText.Contains(
            "VisualWorldStreamPreviewWorkspaceService",
            StringComparison.Ordinal);
        var pageRegistered = compositionText.Contains(
            "VisualWorldStreamPreviewWorkspacePageControl",
            StringComparison.Ordinal);
        var registryIncludesPage = compositionText.Contains(
            "resolver.Resolve<VisualWorldStreamPreviewWorkspacePageControl>()",
            StringComparison.Ordinal);
        var activationLoads = pageText.Contains("BuildAndWriteAsync(root)", StringComparison.Ordinal)
            && pageText.Contains("Bind(write.Result)", StringComparison.Ordinal);
        var bindDisplays = pageText.Contains("_groupsListBox", StringComparison.Ordinal)
            && pageText.Contains("_entriesListView", StringComparison.Ordinal)
            && pageText.Contains("_proofsListView", StringComparison.Ordinal)
            && pageText.Contains("_svgPreviewTextBox", StringComparison.Ordinal);
        var bindDisplaysCacheExports = pageText.Contains("CacheRecordCount", StringComparison.Ordinal)
            && pageText.Contains("ExportTargetKind", StringComparison.Ordinal)
            && pageText.Contains("RuntimeHandoffMetadataOnly", StringComparison.Ordinal)
            && pageText.Contains("ReadbackProofPassed", StringComparison.Ordinal);

        AddIfFalse(pageExists, "goal092.winforms.page_missing", pageRelativePath, diagnostics);
        AddIfFalse(designerExists, "goal092.winforms.designer_missing", designerRelativePath, diagnostics);
        AddIfFalse(serviceRegistered, "goal092.winforms.service_not_registered", compositionRelativePath, diagnostics);
        AddIfFalse(pageRegistered, "goal092.winforms.page_not_registered", compositionRelativePath, diagnostics);
        AddIfFalse(registryIncludesPage, "goal092.winforms.registry_missing", compositionRelativePath, diagnostics);
        AddIfFalse(activationLoads, "goal092.winforms.activation_missing", pageRelativePath, diagnostics);
        AddIfFalse(bindDisplays, "goal092.winforms.bind_missing", pageRelativePath, diagnostics);
        AddIfFalse(
            bindDisplaysCacheExports,
            "goal094.winforms.cache_export_bind_missing",
            pageRelativePath,
            diagnostics);

        return new VisualWorldPreviewWinFormsBindingInventory
        {
            Passed = diagnostics.Count == 0,
            PageControlExists = pageExists,
            DesignerExists = designerExists,
            CompositionRootRegistersService = serviceRegistered,
            CompositionRootRegistersPage = pageRegistered,
            EditorRegistryIncludesPage = registryIncludesPage,
            PageActivationLoadsApplicationResult = activationLoads,
            PageBindDisplaysGroupsEntriesProofs = bindDisplays,
            PageBindDisplaysCacheExports = bindDisplaysCacheExports,
            Diagnostics = diagnostics
        };
    }
}
