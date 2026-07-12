using System.Diagnostics;
using System.Security.Cryptography;
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

        _authoring.OpenProject(requested, currentPackage);
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
        var activatedPackageSha = string.IsNullOrWhiteSpace(state.Document.LastActivatedProjectPackageSha256)
            ? state.Document.LastMaterializedPackageSha256
            : state.Document.LastActivatedProjectPackageSha256;
        var executable = ExecutableProvenance();
        return new UnifiedGameProjectWorkspaceSnapshot
        {
            ProjectFolder = state.ProjectFolder,
            ProjectTitle = state.Identity.Title,
            ProjectPackageId = state.Identity.PackageId,
            ProjectVersion = state.Identity.Version,
            ProjectFormatVersion = state.Identity.FormatVersion,
            ProjectDescription = state.Identity.Description,
            ProjectScopedCompositionId = state.Document.CompositionId,
            IdentitySource = state.Identity.Source,
            IdentityRecoveryDiagnostics = state.Identity.RecoveryDiagnostics,
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
            PackageSha256 = activatedPackageSha,
            CompositionPackageSha256 = string.IsNullOrWhiteSpace(state.Document.LastCompositionPackageSha256)
                ? state.Document.LastMaterializedPackageSha256
                : state.Document.LastCompositionPackageSha256,
            ActivatedProjectPackageSha256 = activatedPackageSha,
            FinalStateHash = state.Document.LastQualifiedFinalStateHash,
            LastCertificationExecutedCount = _lastBuild?.CertificationExecutedCount ?? 0,
            LastCertificationReusedCount = _lastBuild?.CertificationReusedCount ?? 0,
            RuntimePlaythroughPlanId = _lastBuild?.RuntimePlaythroughPlanId ?? string.Empty,
            CapabilityCount = _lastBuild?.CapabilityCount ?? 0,
            PlannedActionCount = _lastBuild?.PlannedActionCount ?? 0,
            CheckpointActionCount = _lastBuild?.CheckpointActionCount ?? 0,
            FinalReplayActionCount = _lastBuild?.FinalReplayActionCount ?? 0,
            PlaythroughSignature = _lastBuild?.PlaythroughSignature ?? string.Empty,
            EquipmentSlotSummary = _lastBuild?.EquipmentSlotSummary ?? string.Empty,
            AttributesSummary = _lastBuild?.AttributesSummary ?? string.Empty,
            ProgressionSummary = _lastBuild?.ProgressionSummary ?? string.Empty,
            StatDamageBonus = _lastBuild?.StatDamageBonus ?? 0,
            EquipmentDamageBonus = _lastBuild?.WeaponDamageBonus ?? 0,
            TotalAdditionalDamage = _lastBuild?.TotalAdditionalDamage ?? 0,
            LastBuildAttemptId = _lastBuild?.AttemptId ?? string.Empty,
            LastBuildAttemptStatus = _lastBuild?.AttemptStatus ?? "NOT_RUN",
            LastBuildFailureStage = _lastBuild?.FailureStage ?? string.Empty,
            LastBuildAttemptedSelectedModuleIds = _lastBuild?.AttemptedSelectedModuleIds ?? [],
            LastBuildAttemptedConfiguredParameterCount = _lastBuild?.AttemptedConfiguredParameterCount ?? 0,
            LastBuildAttemptedCapabilityCount = _lastBuild?.AttemptedCapabilityCount ?? 0,
            LastBuildAttemptedPlannedActionCount = _lastBuild?.AttemptedPlannedActionCount ?? 0,
            LastBuildAttemptedCheckpointActionCount = _lastBuild?.AttemptedCheckpointActionCount ?? 0,
            LastBuildAttemptedFinalReplayActionCount = _lastBuild?.AttemptedFinalReplayActionCount ?? 0,
            LastBuildAttemptedCompositionPackageSha256 = _lastBuild?.AttemptedCompositionPackageSha256 ?? string.Empty,
            LastBuildAttemptedFinalStateHash = _lastBuild?.AttemptedFinalStateHash ?? string.Empty,
            LastBuildAttemptDiagnostics = _lastBuild?.Diagnostics ?? [],
            ExecutablePath = executable.Path,
            ExecutableSha256 = executable.Sha256,
            ExecutableFileVersion = executable.FileVersion,
            ExecutableInformationalVersion = executable.InformationalVersion
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

    private static (string Path, string Sha256, string FileVersion, string InformationalVersion) ExecutableProvenance()
    {
        var path = Environment.ProcessPath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return (path, string.Empty, string.Empty, string.Empty);
        using var stream = File.OpenRead(path);
        var sha = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        var version = FileVersionInfo.GetVersionInfo(path);
        return (path, sha, version.FileVersion ?? string.Empty, version.ProductVersion ?? string.Empty);
    }
}
