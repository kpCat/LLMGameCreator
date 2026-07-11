using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public sealed class GameProjectFeatureModuleAuthoringService
{
    private readonly string _repositoryRoot;
    private readonly FeatureModuleLibraryLoader _libraryLoader;
    private FeatureModuleCompositionPersistenceService? _persistence;
    private FeatureModuleLibrarySnapshot? _library;
    private FeatureModuleCompositionDocument? _document;
    private string? _projectFolder;
    private bool _dirty;
    private int _dirtyTransitionCount;

    public GameProjectFeatureModuleAuthoringService(
        string repositoryRoot,
        FeatureModuleLibraryLoader? libraryLoader = null)
    {
        if (string.IsNullOrWhiteSpace(repositoryRoot)) throw new ArgumentException("repository root is required", nameof(repositoryRoot));
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _libraryLoader = libraryLoader ?? new FeatureModuleLibraryLoader();
    }

    public GameProjectAuthoringState State => new()
    {
        ProjectFolder = RequireProjectFolder(),
        Library = RequireLibrary(),
        Document = RequireDocument(),
        Dirty = _dirty,
        DirtyTransitionCount = _dirtyTransitionCount
    };

    public string AuthoringRoot => ConfinedPath(RequireProjectFolder(), UnifiedGameProjectWorkspaceVocabulary.AuthoringRelativeRoot);
    public string DocumentPath => ConfinedPath(
        AuthoringRoot,
        UnifiedGameProjectWorkspaceVocabulary.CompositionId + FeatureModuleCompositionDocumentVocabulary.FileExtension);

    public GameProjectAuthoringState OpenProject(string projectFolder, string projectTitle)
    {
        var fullProjectFolder = RequireProject(projectFolder);
        _projectFolder = fullProjectFolder;
        _library = _libraryLoader.Load(Path.Combine(_repositoryRoot,
            FeatureModuleLibraryVocabulary.DefaultRelativeRoot.Replace('/', Path.DirectorySeparatorChar)));
        _persistence = new FeatureModuleCompositionPersistenceService(AuthoringRoot);
        if (File.Exists(DocumentPath))
        {
            _document = _persistence.Load(UnifiedGameProjectWorkspaceVocabulary.CompositionId, _library);
        }
        else
        {
            _document = _persistence.CreateNew(
                UnifiedGameProjectWorkspaceVocabulary.CompositionId,
                string.IsNullOrWhiteSpace(projectTitle) ? "Моя игра" : projectTitle,
                "Настройки механик открытого игрового проекта.",
                _library);
            _document = _persistence.Save(_document, _library);
        }

        _dirty = false;
        _dirtyTransitionCount = 0;
        return State;
    }

    public bool SetModuleSelected(string moduleId, bool selected)
    {
        var library = RequireLibrary();
        var document = RequireDocument();
        var module = library.Catalog.Modules.SingleOrDefault(item => item.ModuleId == moduleId)
                     ?? throw new InvalidOperationException("unknown module rejected: " + moduleId);
        if (module.Required && !selected) throw new InvalidOperationException("required module cannot be disabled: " + moduleId);
        if (!module.Selectable && !module.Required) throw new InvalidOperationException("module is not selectable: " + moduleId);

        var ids = document.SelectedModuleIds.ToHashSet(StringComparer.Ordinal);
        var changed = selected ? ids.Add(moduleId) : ids.Remove(moduleId);
        if (!changed) return false;
        _document = document with
        {
            SelectedModuleIds = ids.OrderBy(id => id, StringComparer.Ordinal).ToList(),
            ParameterValues = document.ParameterValues.Where(value => ids.Contains(value.ModuleId)).ToList(),
            CatalogFingerprint = library.CatalogFingerprint,
            ModuleFingerprints = ids.Where(library.ModuleFingerprints.ContainsKey)
                .ToDictionary(id => id, id => library.ModuleFingerprints[id], StringComparer.Ordinal)
        };
        MarkDirty();
        return true;
    }

    public bool SetParameterValue(string moduleId, string parameterId, JsonElement value)
    {
        var document = RequireDocument();
        var existing = document.ParameterValues.SingleOrDefault(item =>
            item.ModuleId == moduleId && item.ParameterId == parameterId);
        if (existing is not null
            && string.Equals(existing.Value.GetRawText(), value.GetRawText(), StringComparison.Ordinal)) return false;
        var values = document.ParameterValues
            .Where(item => item.ModuleId != moduleId || item.ParameterId != parameterId)
            .Append(new FeatureModuleParameterValue
            {
                ModuleId = moduleId,
                ParameterId = parameterId,
                Value = value.Clone()
            })
            .OrderBy(item => item.ModuleId, StringComparer.Ordinal)
            .ThenBy(item => item.ParameterId, StringComparer.Ordinal)
            .ToList();
        _document = document with { ParameterValues = values };
        MarkDirty();
        return true;
    }

    public FeatureModuleCompositionDocumentValidation Validate() =>
        new FeatureModuleCompositionDocumentValidator().Validate(RequireDocument(), RequireLibrary());

    public FeatureModuleCompositionDocument Save()
    {
        _document = RequirePersistence().Save(RequireDocument(), RequireLibrary());
        _dirty = false;
        return _document;
    }

    public void ApplyQualifiedDocument(FeatureModuleCompositionDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    public void RestoreInMemoryDocument(FeatureModuleCompositionDocument document, bool dirty)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _dirty = dirty;
    }

    public IReadOnlyList<FeatureModuleParameterDefinition> ActiveParameterDefinitions()
    {
        var library = RequireLibrary();
        var selected = RequireDocument().SelectedModuleIds.ToHashSet(StringComparer.Ordinal);
        return library.Catalog.Modules
            .Where(module => selected.Contains(module.ModuleId))
            .SelectMany(module => module.ParameterDefinitions)
            .OrderBy(definition => definition.ModuleId, StringComparer.Ordinal)
            .ThenBy(definition => definition.ParameterId, StringComparer.Ordinal)
            .ToList();
    }

    private void MarkDirty()
    {
        if (_dirty) return;
        _dirty = true;
        _dirtyTransitionCount++;
    }

    private string RequireProjectFolder() => _projectFolder
        ?? throw new InvalidOperationException("Open a game project first.");

    private FeatureModuleLibrarySnapshot RequireLibrary() => _library
        ?? throw new InvalidOperationException("Open a game project first.");

    private FeatureModuleCompositionDocument RequireDocument() => _document
        ?? throw new InvalidOperationException("Open a game project first.");

    private FeatureModuleCompositionPersistenceService RequirePersistence() => _persistence
        ?? throw new InvalidOperationException("Open a game project first.");

    private static string RequireProject(string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder)) throw new ArgumentException("project folder is required", nameof(projectFolder));
        var full = Path.GetFullPath(projectFolder);
        if (!Directory.Exists(full)) throw new DirectoryNotFoundException("Game project folder was not found: " + full);
        if (!File.Exists(Path.Combine(full, "package.json")))
            throw new FileNotFoundException("Game project package.json was not found.", Path.Combine(full, "package.json"));
        return full;
    }

    internal static string ConfinedPath(string root, string relativePath)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var candidate = Path.GetFullPath(Path.Combine(fullRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!candidate.Equals(fullRoot, comparison)
            && !candidate.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("project authoring path escape rejected");
        return candidate;
    }
}
