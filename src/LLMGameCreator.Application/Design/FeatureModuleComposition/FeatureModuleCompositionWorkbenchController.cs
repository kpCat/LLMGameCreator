namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public sealed class FeatureModuleCompositionWorkbenchController
{
    private readonly FeatureModuleCompositionService _service;
    private readonly FeatureModuleCompositionOperatorRunner _runner;

    public FeatureModuleCompositionWorkbenchController(
        FeatureModuleCompositionService service,
        FeatureModuleCompositionOperatorRunner runner)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
    }

    public FeatureModuleCatalogDocument? Catalog { get; private set; }
    public IReadOnlyList<string> SelectedOptionalModuleIds { get; private set; } = [];
    public int MaterializationInvocationCount { get; private set; }

    public FeatureModuleCatalogDocument LoadCatalog(string repositoryRoot)
    {
        return BindCatalog(_service.LoadCatalog(repositoryRoot));
    }

    public FeatureModuleCatalogDocument BindCatalog(FeatureModuleCatalogDocument catalog)
    {
        Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        SelectedOptionalModuleIds = Catalog.Modules
            .Where(module => module.Selectable && !module.Required)
            .OrderBy(module => module.ModuleId, StringComparer.Ordinal)
            .Select(module => module.ModuleId)
            .ToList();
        return Catalog;
    }

    public void SetSelectedOptionalModules(IReadOnlyList<string> moduleIds)
    {
        SelectedOptionalModuleIds = moduleIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
    }

    public FeatureModuleCompositionValidation ValidateSelection()
    {
        if (Catalog is null) throw new InvalidOperationException("Load the FeatureModule catalog first.");
        return _service.ValidateSelection(Catalog, SelectedOptionalModuleIds);
    }

    public async Task<FeatureModuleCompositionWriteResult> MaterializeAndQualifyAsync(
        string repositoryRoot,
        string compositionId = "",
        CancellationToken cancellationToken = default)
    {
        MaterializationInvocationCount++;
        return await _runner.RunAsync(repositoryRoot, SelectedOptionalModuleIds, compositionId, cancellationToken)
            .ConfigureAwait(false);
    }
}
