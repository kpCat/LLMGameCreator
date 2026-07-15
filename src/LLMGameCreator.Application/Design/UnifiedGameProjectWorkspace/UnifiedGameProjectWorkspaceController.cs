using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Generation.Procedural;
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
    ProjectStandaloneBuildResult BuildWindowsStandalone(CancellationToken cancellationToken = default) => new()
    {
        Status = "FAILED", Stage = "standalone_not_supported", Diagnostics = ["Standalone build is not available for this controller."]
    };
    void CancelWindowsStandaloneBuild() { }
    void LaunchWindowsStandalone() => throw new InvalidOperationException("Standalone build is not available for this controller.");
    void OpenWindowsStandaloneFolder() => throw new InvalidOperationException("Standalone build is not available for this controller.");
    ProjectStandaloneBuildSettings GetStandaloneBuildSettings() => new();
    ProjectStandaloneBuildSettings SaveStandaloneBuildSettings(ProjectStandaloneBuildSettings settings) => settings;
}

public sealed class UnifiedGameProjectWorkspaceController : IUnifiedGameProjectWorkspaceController
{
    private readonly ICurrentGamePackageService _currentPackageService;
    private readonly GameProjectFeatureModuleAuthoringService _authoring;
    private readonly GameProjectBuildAndQualificationService _builder;
    private readonly GameProjectWorkspaceStatusPresenter _presenter;
    private readonly IProjectStandaloneBuildService _standaloneBuild;
    private readonly GameProjectBuildHistoryReader _historyReader;
    private readonly GameProjectAcceptedMechanicsSummaryService _acceptedMechanicsSummaryService;
    private readonly GameProjectReleaseCandidateRecordService _releaseCandidateRecordService;
    private readonly SeededGeneratedProjectSourceService _generatedSourceService;
    private readonly GameProjectGeneratedWorldSummaryService _generatedWorldSummaryService;
    private GameProjectBuildResult? _lastBuild;
    private GameProjectBuildResult? _lastSuccessfulBuild;
    private ProjectStandaloneBuildResult? _lastStandaloneAttempt;
    private IReadOnlyList<string> _persistedHistoryDiagnostics = [];

    public UnifiedGameProjectWorkspaceController(
        ICurrentGamePackageService currentPackageService,
        GameProjectFeatureModuleAuthoringService authoring,
        GameProjectBuildAndQualificationService builder,
        GameProjectWorkspaceStatusPresenter? presenter = null,
        IProjectStandaloneBuildService? standaloneBuild = null,
        GameProjectBuildHistoryReader? historyReader = null,
        GameProjectAcceptedMechanicsSummaryService? acceptedMechanicsSummaryService = null,
        GameProjectReleaseCandidateRecordService? releaseCandidateRecordService = null,
        SeededGeneratedProjectSourceService? generatedSourceService = null,
        GameProjectGeneratedWorldSummaryService? generatedWorldSummaryService = null)
    {
        _currentPackageService = currentPackageService ?? throw new ArgumentNullException(nameof(currentPackageService));
        _authoring = authoring ?? throw new ArgumentNullException(nameof(authoring));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _presenter = presenter ?? new GameProjectWorkspaceStatusPresenter();
        _standaloneBuild = standaloneBuild ?? new ProjectStandaloneBuildService(Directory.GetCurrentDirectory());
        _historyReader = historyReader ?? new GameProjectBuildHistoryReader();
        _acceptedMechanicsSummaryService = acceptedMechanicsSummaryService ?? new GameProjectAcceptedMechanicsSummaryService();
        _releaseCandidateRecordService = releaseCandidateRecordService ?? new GameProjectReleaseCandidateRecordService();
        _generatedSourceService = generatedSourceService ?? new SeededGeneratedProjectSourceService();
        _generatedWorldSummaryService = generatedWorldSummaryService ?? new GameProjectGeneratedWorldSummaryService();
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
        _lastStandaloneAttempt = null;
        var persisted = _historyReader.ReadLatestMatchingSocialSuccess(requested, _authoring.State.Document, _authoring.State.Library);
        _lastSuccessfulBuild = persisted.LastSuccessfulBuild;
        _persistedHistoryDiagnostics = persisted.Diagnostics;
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
        var social = _lastSuccessfulBuild?.Social is { Present: true, Passed: true } currentSocial ? currentSocial : null;
        var socialTruth = SocialTruth(state, _lastSuccessfulBuild, social is not null);
        var releaseCandidate = _releaseCandidateRecordService.Read(new GameProjectReleaseCandidateReadRequest
        {
            ProjectFolder = state.ProjectFolder,
            Document = state.Document,
            Library = state.Library,
            Identity = state.Identity
        });
        var acceptedMechanics = _lastSuccessfulBuild?.AcceptedMechanics
                                ?? releaseCandidate.Record?.AcceptedMechanicsSummary;
        var acceptedMechanicsCompatibility = _lastSuccessfulBuild?.AcceptedMechanicsCompatibility;
        var currentFingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(state.Document, state.Library);
        var acceptedMechanicsCurrent = acceptedMechanics is { Passed: true }
                                       && currentFingerprint.Passed
                                       && !string.IsNullOrWhiteSpace(currentFingerprint.Sha256)
                                       && string.Equals(acceptedMechanics.QualifiedAuthoringFingerprint,
                                           currentFingerprint.Sha256, StringComparison.Ordinal);
        var releaseCandidateStatus = ResolveReleaseCandidateStatus(
            acceptedMechanics,
            acceptedMechanicsCurrent,
            _lastSuccessfulBuild,
            releaseCandidate);
        var generatedSource = _generatedSourceService.Validate(state.ProjectFolder);
        var generatedMatchesCurrent = _lastSuccessfulBuild is not null
                                      && currentFingerprint.Passed
                                      && !string.IsNullOrWhiteSpace(currentFingerprint.Sha256)
                                      && string.Equals(_lastSuccessfulBuild.QualifiedAuthoringFingerprint,
                                          currentFingerprint.Sha256, StringComparison.Ordinal);
        var generatedWorldActivation = _lastSuccessfulBuild?.GeneratedWorldActivation;
        var generatedWorld = _generatedWorldSummaryService.Restore(
            generatedSource,
            _lastSuccessfulBuild?.GeneratedWorld,
            generatedMatchesCurrent,
            generatedWorldActivation);
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
            Diagnostics = validation.Diagnostics.Concat(parameterValidation.Diagnostics)
                .Concat(_persistedHistoryDiagnostics).Concat(releaseCandidate.Diagnostics)
                .Distinct(StringComparer.Ordinal).ToList(),
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
            AbilitySummary = _lastBuild?.AbilitySummary ?? string.Empty,
            ManaSummary = _lastBuild?.ManaSummary ?? string.Empty,
            StatusSummary = _lastBuild?.StatusSummary ?? string.Empty,
            AbilityDirectDamage = _lastBuild?.AbilityDirectDamage ?? 0,
            ManaBefore = _lastBuild?.ManaBefore ?? 0,
            ManaSpent = _lastBuild?.ManaSpent ?? 0,
            ManaRemaining = _lastBuild?.ManaRemaining ?? 0,
            StatusTickDamage = _lastBuild?.StatusTickDamage ?? 0,
            StatusRemainingTicks = _lastBuild?.StatusRemainingTicks ?? 0,
            StatusExpired = _lastBuild?.StatusExpired ?? false,
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
            ,LastStandaloneBuild = _lastStandaloneAttempt ?? _standaloneBuild.LastResult
            ,StandaloneUnityEditorPath = _standaloneBuild.LoadSettings(state.ProjectFolder).UnityEditorPath
            ,Social = social
            ,SocialMatchesCurrentConfiguration = socialTruth.Matches
            ,SocialConfigurationStatus = socialTruth.Status
            ,AcceptedMechanics = acceptedMechanics
            ,ReleaseCandidate = releaseCandidate.Record
            ,ReleaseCandidateRecordConfigurationStatus = releaseCandidate.ConfigurationStatus
            ,ReleaseCandidateConfigurationStatus = releaseCandidateStatus
            ,ReleaseCandidateRecordPath = releaseCandidate.RecordPath
            ,GeneratedWorld = generatedWorld
            ,GeneratedWorldActivation = generatedWorld?.Status is "BUILD_CURRENT" or "LAST_SUCCESS"
                ? generatedWorldActivation
                : null
            ,AcceptedMechanicsCompatibility = acceptedMechanicsCompatibility
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
        if (_lastBuild.Passed) _lastSuccessfulBuild = _lastBuild;
        return _lastBuild;
    }

    public ProjectStandaloneBuildResult BuildWindowsStandalone(CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        _lastBuild = _builder.Build(_authoring, cancellationToken);
        if (_lastBuild.Passed) _lastSuccessfulBuild = _lastBuild;
        var snapshot = Snapshot();
        if (!_lastBuild.Passed)
        {
            _lastStandaloneAttempt = new ProjectStandaloneBuildResult
            {
                Status = "FAILED",
                Stage = "qualify_current_project",
                Diagnostics = _lastBuild.Diagnostics,
                ProjectFolder = snapshot.ProjectFolder
            };
            return _lastStandaloneAttempt;
        }
        var state = _authoring.State;
        var currentFingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(state.Document, state.Library);
        var releaseCandidateFactsAllowed = _lastBuild.AcceptedMechanics is { Passed: true }
                                           && currentFingerprint.Passed
                                           && string.Equals(_lastBuild.QualifiedAuthoringFingerprint,
                                               currentFingerprint.Sha256, StringComparison.Ordinal);
        var request = new ProjectStandaloneBuildRequest
        {
            ProjectFolder = snapshot.ProjectFolder,
            ProjectTitle = snapshot.ProjectTitle,
            ProjectPackageId = snapshot.ProjectPackageId,
            ProjectVersion = snapshot.ProjectVersion,
            CompositionId = snapshot.ProjectScopedCompositionId,
            SelectedModuleIds = state.Document.SelectedModuleIds.OrderBy(id => id, StringComparer.Ordinal).ToList(),
            Parameters = state.Document.ParameterValues.OrderBy(value => value.ModuleId, StringComparer.Ordinal).ThenBy(value => value.ParameterId, StringComparer.Ordinal).Select(value => new StandaloneParameterValue
            {
                ModuleId = value.ModuleId,
                ParameterId = value.ParameterId,
                Value = value.Value.Clone()
            }).ToList(),
            PackageSha256 = _lastBuild.ActivatedProjectPackageSha256,
            CompositionPackageSha256 = _lastBuild.CompositionPackageSha256,
            FinalStateHash = _lastBuild.FinalStateHash,
            RuntimePlanId = _lastBuild.RuntimePlaythroughPlanId,
            CapabilityCount = _lastBuild.CapabilityCount,
            RequiredMechanicCount = snapshot.Mechanics.Count(item => item.Required),
            SelectedOptionalMechanicCount = snapshot.Mechanics.Count(item => !item.Required && item.Selected),
            ActiveMechanicCount = snapshot.Mechanics.Count(item => item.Selected),
            ConfiguredParameterCount = state.Document.ParameterValues.Count,
            PlannedActionCount = _lastBuild.PlannedActionCount,
            CheckpointActionCount = _lastBuild.CheckpointActionCount,
            FinalReplayActionCount = _lastBuild.FinalReplayActionCount,
            EquipmentSummary = _lastBuild.EquipmentSlotSummary,
            AttributesSummary = _lastBuild.AttributesSummary,
            ProgressionSummary = _lastBuild.ProgressionSummary,
            EquipmentDamageBonus = _lastBuild.WeaponDamageBonus,
            StatDamageBonus = _lastBuild.StatDamageBonus,
            TotalAdditionalDamage = _lastBuild.TotalAdditionalDamage,
            HumanReviewFacts = _generatedWorldSummaryService.StandaloneHumanFacts(_lastBuild.GeneratedWorld)
                .Concat(GameProjectGeneratedWorldSummaryService.StandaloneActivationHumanFacts(
                    _lastBuild.GeneratedWorldActivation))
                .Concat(_acceptedMechanicsSummaryService.StandaloneHumanFacts(
                    _lastBuild, releaseCandidateFactsAllowed)).ToList(),
            RuntimeFrames = _lastBuild.RuntimeFrames.Select(frame => new StandaloneRuntimeFrame
            {
                Index = frame.Index,
                ActionId = frame.ActionId,
                Title = frame.Title,
                Category = frame.Category,
                StateHash = frame.StateHash
            }).ToList()
        };
        var standalone = _standaloneBuild.Build(request, cancellationToken);
        _lastStandaloneAttempt = standalone;
        if (!string.Equals(standalone.Status, "GREEN", StringComparison.Ordinal)) return standalone;
        try
        {
            _releaseCandidateRecordService.Write(state.ProjectFolder, state.Identity, _lastBuild, standalone);
            return standalone;
        }
        catch (Exception exception) when (exception is IOException
                                          or UnauthorizedAccessException
                                          or InvalidOperationException
                                          or JsonException)
        {
            var failed = standalone with
            {
                Status = "FAILED",
                Stage = "release_candidate_record",
                Diagnostics = [exception.Message]
            };
            _lastStandaloneAttempt = failed;
            return failed;
        }
    }

    private static (bool Matches, string Status) SocialTruth(
        GameProjectAuthoringState state,
        GameProjectBuildResult? lastSuccessfulBuild,
        bool socialPresent)
    {
        if (!socialPresent) return (false, "ABSENT");
        var qualified = lastSuccessfulBuild?.QualifiedAuthoringFingerprint;
        if (string.IsNullOrWhiteSpace(qualified)) return (false, "UNKNOWN");
        var current = new FeatureModuleAuthoringFingerprintService().Calculate(state.Document, state.Library);
        if (!current.Passed || string.IsNullOrWhiteSpace(current.Sha256)) return (false, "UNKNOWN");
        var matches = string.Equals(current.Sha256, qualified, StringComparison.Ordinal);
        return (matches, matches ? "CURRENT" : "LAST_SUCCESS");
    }


    private static string ResolveReleaseCandidateStatus(
        GameProjectAcceptedMechanicsSummary? acceptedMechanics,
        bool acceptedMechanicsCurrent,
        GameProjectBuildResult? lastSuccessfulBuild,
        GameProjectReleaseCandidateReadResult releaseCandidate)
    {
        if (acceptedMechanics is { Passed: true } && acceptedMechanicsCurrent)
        {
            var recordMatchesBuild = releaseCandidate.Record is not null
                                     && (lastSuccessfulBuild is null
                                         || string.Equals(releaseCandidate.Record.PackageSha256,
                                             lastSuccessfulBuild.PackageSha256, StringComparison.Ordinal)
                                         && string.Equals(releaseCandidate.Record.CompositionPackageSha256,
                                             lastSuccessfulBuild.CompositionPackageSha256, StringComparison.Ordinal)
                                         && string.Equals(releaseCandidate.Record.FinalStateHash,
                                             lastSuccessfulBuild.FinalStateHash, StringComparison.Ordinal));
            return releaseCandidate.ConfigurationStatus == "CURRENT" && recordMatchesBuild
                ? "CURRENT"
                : "BUILD_GREEN_STANDALONE_PENDING";
        }
        if (releaseCandidate.Record is not null) return releaseCandidate.ConfigurationStatus;
        return "ABSENT";
    }

    public void CancelWindowsStandaloneBuild() => _standaloneBuild.Cancel();
    public void LaunchWindowsStandalone() => _standaloneBuild.LaunchLastBuild();
    public void OpenWindowsStandaloneFolder() => _standaloneBuild.OpenLastBuildFolder();
    public ProjectStandaloneBuildSettings GetStandaloneBuildSettings() { EnsureOpen(); return _standaloneBuild.LoadSettings(_authoring.State.ProjectFolder); }
    public ProjectStandaloneBuildSettings SaveStandaloneBuildSettings(ProjectStandaloneBuildSettings settings) { EnsureOpen(); return _standaloneBuild.SaveSettings(_authoring.State.ProjectFolder, settings); }

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
