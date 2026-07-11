using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleAuthoringWorkbenchController
{
    private readonly string _repositoryRoot;
    private readonly string _libraryRoot;
    private readonly FeatureModuleLibraryLoader _loader;
    private readonly FeatureModuleCompositionPersistenceService _persistence;
    private readonly FeatureModuleCompositionDocumentValidator _validator;
    private readonly FeatureModuleCompositionStalenessService _staleness;
    private readonly FeatureModuleParameterizedCompositionService _materializer;
    private readonly FeatureModuleCertificationService _certification;
    private bool _binding;

    public FeatureModuleAuthoringWorkbenchController(
        string repositoryRoot,
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        string? libraryRoot = null,
        string? workspaceRoot = null,
        IFeatureModuleAuthoringClock? clock = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _libraryRoot = Path.GetFullPath(libraryRoot ?? Path.Combine(_repositoryRoot, "catalogs", "feature-modules"));
        var workspace = Path.GetFullPath(workspaceRoot ?? Path.Combine(_repositoryRoot,
            FeatureModuleCompositionDocumentVocabulary.DefaultWorkspaceRelativeRoot.Replace('/', Path.DirectorySeparatorChar)));
        _loader = new FeatureModuleLibraryLoader();
        _validator = new FeatureModuleCompositionDocumentValidator();
        _staleness = new FeatureModuleCompositionStalenessService();
        _persistence = new FeatureModuleCompositionPersistenceService(workspace, clock);
        _materializer = new FeatureModuleParameterizedCompositionService(runtime);
        _certification = new FeatureModuleCertificationService(runtime,
            new FeatureModuleCertificationCache(Path.Combine(_repositoryRoot,
                FeatureModuleCertificationVocabulary.DefaultCacheRelativeRoot.Replace('/', Path.DirectorySeparatorChar))), clock);
    }

    public FeatureModuleLibrarySnapshot? Library { get; private set; }
    public FeatureModuleCompositionDocument? Document { get; private set; }
    public FeatureModuleParameterizedCompositionResult? LastMaterialization { get; private set; }
    public FeatureModuleCertificationLedger? LastCertificationLedger { get; private set; }
    public bool Dirty { get; private set; }
    public int DirtyTransitionCount { get; private set; }
    public int MaterializationInvocationCount { get; private set; }

    public FeatureModuleLibrarySnapshot RefreshLibrary()
    {
        Library = _loader.Load(_libraryRoot);
        return Library;
    }

    public FeatureModuleCompositionDocument NewComposition(
        string compositionId,
        string displayName,
        string description)
    {
        EnsureLibrary();
        Bind(() => Document = _persistence.CreateNew(compositionId, displayName, description, Library!));
        Dirty = true;
        DirtyTransitionCount++;
        LastMaterialization = null;
        return Document!;
    }

    public FeatureModuleCompositionDocument Open(string compositionId)
    {
        EnsureLibrary();
        Bind(() => Document = _persistence.Load(compositionId, Library!));
        Dirty = false;
        LastMaterialization = null;
        return Document!;
    }

    public FeatureModuleCompositionWorkspaceIndex List()
    {
        EnsureLibrary();
        return _persistence.List(Library!);
    }

    public void SetIdentity(string compositionId, string displayName, string description)
    {
        EnsureDocument();
        if (Document!.CompositionId == compositionId && Document.DisplayName == displayName && Document.Description == description) return;
        Document = Document with { CompositionId = compositionId, DisplayName = displayName, Description = description };
        MarkDirty();
    }

    public void SetSelectedModules(IReadOnlyList<string> moduleIds)
    {
        EnsureDocument();
        EnsureLibrary();
        var selected = moduleIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
        if (Document!.SelectedModuleIds.SequenceEqual(selected, StringComparer.Ordinal)) return;
        Document = Document with
        {
            SelectedModuleIds = selected,
            ParameterValues = Document.ParameterValues.Where(value => selected.Contains(value.ModuleId, StringComparer.Ordinal)).ToList(),
            CatalogFingerprint = Library!.CatalogFingerprint,
            ModuleFingerprints = selected.Where(Library.ModuleFingerprints.ContainsKey)
                .ToDictionary(id => id, id => Library.ModuleFingerprints[id], StringComparer.Ordinal)
        };
        MarkDirty();
    }

    public void SetParameterValue(string moduleId, string parameterId, JsonElement value)
    {
        EnsureDocument();
        var values = Document!.ParameterValues.Where(item => item.ModuleId != moduleId || item.ParameterId != parameterId).ToList();
        values.Add(new FeatureModuleParameterValue { ModuleId = moduleId, ParameterId = parameterId, Value = value.Clone() });
        Document = Document with
        {
            ParameterValues = values.OrderBy(item => item.ModuleId, StringComparer.Ordinal)
                .ThenBy(item => item.ParameterId, StringComparer.Ordinal).ToList()
        };
        MarkDirty();
    }

    public IReadOnlyList<FeatureModuleParameterDefinition> ActiveParameterDefinitions()
    {
        EnsureDocument();
        EnsureLibrary();
        var selected = Document!.SelectedModuleIds.ToHashSet(StringComparer.Ordinal);
        return Library!.Catalog.Modules.Where(module => selected.Contains(module.ModuleId))
            .SelectMany(module => module.ParameterDefinitions).OrderBy(item => item.ModuleId, StringComparer.Ordinal)
            .ThenBy(item => item.ParameterId, StringComparer.Ordinal).ToList();
    }

    public FeatureModuleCompositionDocumentValidation Validate()
    {
        EnsureDocument();
        EnsureLibrary();
        return _validator.Validate(Document!, Library!);
    }

    public FeatureModuleCompositionStaleness Staleness()
    {
        EnsureDocument();
        EnsureLibrary();
        return _staleness.Evaluate(Document!, Library!);
    }

    public FeatureModuleCompositionCoveragePlan CoveragePlan()
    {
        EnsureLibrary();
        var selected = Document?.SelectedModuleIds ?? Library!.Catalog.Modules
            .Where(module => module.Selectable && !module.Required).Select(module => module.ModuleId).ToList();
        return new FeatureModuleCompositionCoveragePlanner().Plan(Library!.Catalog, selected);
    }

    public FeatureModuleCompositionDocument Save()
    {
        EnsureDocument();
        EnsureLibrary();
        Document = _persistence.Save(Document!, Library!);
        Dirty = false;
        return Document;
    }

    public FeatureModuleCompositionDocument SaveAsClone(string newCompositionId, string newDisplayName)
    {
        EnsureDocument();
        EnsureLibrary();
        Document = _persistence.SaveAs(Document!, newCompositionId, newDisplayName, Library!);
        Dirty = false;
        LastMaterialization = null;
        return Document;
    }

    public void Delete()
    {
        EnsureDocument();
        _persistence.Delete(Document!.CompositionId);
        Document = null;
        LastMaterialization = null;
        Dirty = false;
    }

    public FeatureModuleParameterizedCompositionResult MaterializeAndQualify()
    {
        EnsureDocument();
        EnsureLibrary();
        MaterializationInvocationCount++;
        var output = Path.Combine(_repositoryRoot, ".llmgc", "workspace", "featuremodule-materializations",
            Document!.CompositionId);
        LastMaterialization = _materializer.MaterializeAndQualify(_repositoryRoot, Library!, Document, output);
        Document = LastMaterialization.QualifiedDocument;
        MarkDirty();
        return LastMaterialization;
    }

    public FeatureModuleParameterizedCompositionResult SaveMaterializeAndQualify()
    {
        Save();
        var result = MaterializeAndQualify();
        Save();
        LastCertificationLedger = _certification.Certify(
            _repositoryRoot,
            Library!,
            BaselineSha(),
            Path.Combine(_repositoryRoot, ".llmgc", "workspace", "featuremodule-certification-execution"));
        return result;
    }

    public void BeginProgrammaticBinding() => _binding = true;
    public void EndProgrammaticBinding() => _binding = false;

    private void MarkDirty()
    {
        if (_binding || Dirty) return;
        Dirty = true;
        DirtyTransitionCount++;
    }

    private void Bind(Action action)
    {
        var prior = _binding;
        _binding = true;
        try { action(); }
        finally { _binding = prior; }
    }

    private void EnsureLibrary()
    {
        if (Library is null) RefreshLibrary();
    }

    private void EnsureDocument()
    {
        if (Document is null) throw new InvalidOperationException("Create or open a FeatureModule composition first.");
    }

    private string BaselineSha()
    {
        var path = Path.Combine(_repositoryRoot,
            FeatureModuleCompositionVocabulary.Goal142Root.Replace('/', Path.DirectorySeparatorChar),
            "product-line-runtime-variant-matrix-result.json");
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        return json.RootElement.GetProperty("candidates").EnumerateArray()
            .Single(item => item.GetProperty("candidateId").GetString() == FeatureModuleCompositionVocabulary.BaselineCandidateId)
            .GetProperty("packageSha256").GetString()!;
    }
}
