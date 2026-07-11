using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public interface IUnifiedGameProjectWorkspaceController
{
    bool HasOpenProject { get; }
    bool BuildRunning { get; }
    int DirtyTransitionCount { get; }
    GameProjectBuildResult? LastBuild { get; }
    UnifiedGameProjectWorkspaceSnapshot OpenProject(string projectFolder);
    UnifiedGameProjectWorkspaceSnapshot Snapshot();
    UnifiedGameProjectWorkspaceSnapshot SetModuleSelected(string moduleId, bool selected);
    UnifiedGameProjectWorkspaceSnapshot SetParameterValue(string moduleId, string parameterId, JsonElement value);
    UnifiedGameProjectWorkspaceSnapshot SaveAuthoring();
    GameProjectBuildResult BuildAndQualify(CancellationToken cancellationToken = default);
}

public sealed class UnifiedGameProjectWorkspaceController : IUnifiedGameProjectWorkspaceController
{
    private readonly ICurrentGamePackageService _currentPackageService;
    private readonly GameProjectFeatureModuleAuthoringService _authoring;
    private readonly GameProjectBuildAndQualificationService _builder;
    private readonly GameProjectWorkspaceStatusPresenter _presenter;
    private GameProjectBuildResult? _lastBuild;

    public UnifiedGameProjectWorkspaceController(
        ICurrentGamePackageService currentPackageService,
        GameProjectFeatureModuleAuthoringService authoring,
        GameProjectBuildAndQualificationService builder,
        GameProjectWorkspaceStatusPresenter? presenter = null)
    {
        _currentPackageService = currentPackageService ?? throw new ArgumentNullException(nameof(currentPackageService));
        _authoring = authoring ?? throw new ArgumentNullException(nameof(authoring));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _presenter = presenter ?? new GameProjectWorkspaceStatusPresenter();
    }

    public bool HasOpenProject { get; private set; }
    public bool BuildRunning => _builder.BuildRunning;
    public int DirtyTransitionCount => HasOpenProject ? _authoring.State.DirtyTransitionCount : 0;
    public GameProjectBuildResult? LastBuild => _lastBuild;

    public UnifiedGameProjectWorkspaceSnapshot OpenProject(string projectFolder)
    {
        var currentFolder = _currentPackageService.CurrentFolder;
        var currentPackage = _currentPackageService.CurrentPackage;
        if (string.IsNullOrWhiteSpace(currentFolder) || currentPackage is null)
            throw new InvalidOperationException("Open the game package before opening its workspace.");
        var requested = Path.GetFullPath(projectFolder);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!string.Equals(Path.GetFullPath(currentFolder), requested, comparison))
            throw new InvalidOperationException("Workspace project must match the currently opened package folder.");

        _authoring.OpenProject(requested, currentPackage.Manifest.Title);
        HasOpenProject = true;
        _lastBuild = null;
        return Snapshot();
    }

    public UnifiedGameProjectWorkspaceSnapshot Snapshot()
    {
        if (!HasOpenProject) throw new InvalidOperationException("Open a game project first.");
        var state = _authoring.State;
        var validation = _authoring.Validate();
        var missingDependencies = validation.Diagnostics.Any(line =>
            line.Contains("dependency", StringComparison.OrdinalIgnoreCase)
            || line.Contains("unknown module", StringComparison.OrdinalIgnoreCase));
        var mechanics = _presenter.Mechanics(state.Library.Catalog, state.Document.SelectedModuleIds);
        var parameterValidation = new FeatureModuleParameterValidator().Validate(
            state.Library.Catalog,
            state.Document.SelectedModuleIds,
            state.Document.ParameterValues);
        var effective = parameterValidation.EffectiveValues.ToDictionary(
            item => item.ModuleId + "|" + item.ParameterId,
            StringComparer.Ordinal);
        var moduleTitles = mechanics.ToDictionary(item => item.ModuleId, item => item.Title, StringComparer.Ordinal);
        var parameters = _authoring.ActiveParameterDefinitions().Select(definition =>
        {
            var key = definition.ModuleId + "|" + definition.ParameterId;
            var value = effective.TryGetValue(key, out var resolved) ? resolved.Value : definition.DefaultValue;
            return new GameProjectParameterPresentation
            {
                ModuleId = definition.ModuleId,
                ModuleTitle = moduleTitles.GetValueOrDefault(definition.ModuleId, definition.ModuleId),
                ParameterId = definition.ParameterId,
                Title = definition.Title,
                Description = definition.Description,
                ValueType = definition.ValueType,
                Value = value.Clone(),
                Minimum = definition.Minimum,
                Maximum = definition.Maximum,
                Step = definition.Step,
                AllowedValues = definition.AllowedValues,
                Unit = definition.Unit,
                ValidationError = parameterValidation.Diagnostics.FirstOrDefault(line => line.Contains(key, StringComparison.Ordinal)) ?? string.Empty
            };
        }).ToList();
        var lastGreen = state.Document.LastQualificationStatus == "GREEN";
        return new UnifiedGameProjectWorkspaceSnapshot
        {
            ProjectFolder = state.ProjectFolder,
            ProjectTitle = _currentPackageService.CurrentPackage?.Manifest.Title ?? state.Document.DisplayName,
            PackageStatus = _presenter.PackageStatus(state.Document.LastQualificationStatus, state.Dirty),
            AuthoringStatus = _presenter.AuthoringStatus(state.Dirty, validation.Passed, missingDependencies),
            SelectedMechanicCount = mechanics.Count(item => item.Selected),
            LastSuccessfulBuild = lastGreen ? "Готово" : "Проверка ещё не запускалась",
            LastRuntimeQualification = lastGreen ? "Готово" : "Проверка ещё не запускалась",
            Dirty = state.Dirty,
            Revision = state.Document.Revision,
            CatalogFingerprint = state.Library.CatalogFingerprint,
            Mechanics = mechanics,
            Parameters = parameters,
            Diagnostics = validation.Diagnostics.Concat(parameterValidation.Diagnostics).Distinct(StringComparer.Ordinal).ToList(),
            PackageSha256 = state.Document.LastMaterializedPackageSha256,
            FinalStateHash = state.Document.LastQualifiedFinalStateHash,
            LastCertificationExecutedCount = _lastBuild?.CertificationExecutedCount ?? 0,
            LastCertificationReusedCount = _lastBuild?.CertificationReusedCount ?? 0
        };
    }

    public UnifiedGameProjectWorkspaceSnapshot SetModuleSelected(string moduleId, bool selected)
    {
        EnsureOpen();
        _authoring.SetModuleSelected(moduleId, selected);
        return Snapshot();
    }

    public UnifiedGameProjectWorkspaceSnapshot SetParameterValue(string moduleId, string parameterId, JsonElement value)
    {
        EnsureOpen();
        _authoring.SetParameterValue(moduleId, parameterId, value);
        return Snapshot();
    }

    public UnifiedGameProjectWorkspaceSnapshot SaveAuthoring()
    {
        EnsureOpen();
        _authoring.Save();
        return Snapshot();
    }

    public GameProjectBuildResult BuildAndQualify(CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        _lastBuild = _builder.Build(_authoring, cancellationToken);
        return _lastBuild;
    }

    private void EnsureOpen()
    {
        if (!HasOpenProject) throw new InvalidOperationException("Open a game project first.");
    }
}
