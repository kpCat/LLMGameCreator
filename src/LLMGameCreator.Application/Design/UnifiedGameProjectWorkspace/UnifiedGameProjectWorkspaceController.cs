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
    bool RegenerationRunning => false;
    int DirtyTransitionCount { get; }
    GameProjectBuildResult? LastBuild { get; }
    UnifiedGameProjectWorkspaceSnapshot OpenProject(string projectFolder);
    UnifiedGameProjectWorkspaceSnapshot Snapshot();
    UnifiedGameProjectWorkspaceSnapshot SetModuleSelected(string moduleId, bool selected);
    UnifiedGameProjectWorkspaceSnapshot SetParameterValue(string moduleId, string parameterId, JsonElement value);
    UnifiedGameProjectWorkspaceSnapshot SaveAuthoring();
    GameProjectBuildResult BuildAndQualify(CancellationToken cancellationToken = default);
    GameProjectSeedRegenerationRequest CreateGeneratedWorldRegenerationRequest(
        SeededGeneratedProjectGenerationRequest generationRequest) =>
        throw new InvalidOperationException("Перегенерация мира недоступна для этого контроллера.");
    GameProjectSeedRegenerationPreview PreviewGeneratedWorldRegeneration(
        GameProjectSeedRegenerationRequest request,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Перегенерация мира недоступна для этого контроллера.");
    GameProjectSeedRegenerationResult ApplyGeneratedWorldRegeneration(
        GameProjectSeedRegenerationRequest request,
        GameProjectSeedRegenerationPreview preview) =>
        throw new InvalidOperationException("Перегенерация мира недоступна для этого контроллера.");
    GeneratedWorldHistoryReadResult ReadGeneratedWorldHistory() => new();
    GameProjectGeneratedWorldRollbackRequest CreateGeneratedWorldRollbackRequest(string targetWorldId) =>
        throw new InvalidOperationException("История миров недоступна для этого контроллера.");
    GameProjectGeneratedWorldRollbackPreview PreviewGeneratedWorldRollback(
        GameProjectGeneratedWorldRollbackRequest request,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("История миров недоступна для этого контроллера.");
    GameProjectGeneratedWorldRollbackResult ApplyGeneratedWorldRollback(
        GameProjectGeneratedWorldRollbackRequest request,
        GameProjectGeneratedWorldRollbackPreview preview) =>
        throw new InvalidOperationException("История миров недоступна для этого контроллера.");
    GeneratedGameplaySaveListResult ListGeneratedGameplaySaves() => new();
    GeneratedGameplaySaveMigrationPreview PreviewGeneratedGameplaySaveMigration(string slotName) => new()
    {
        SlotName = slotName,
        Diagnostics = ["generated_save.not_available"]
    };
    GeneratedGameplaySaveMigrationResult ApplyGeneratedGameplaySaveMigration(
        GeneratedGameplaySaveMigrationPreview preview) => new()
    {
        SlotName = preview.SlotName,
        SourceRevisionSha256 = preview.SourceRevisionSha256,
        Diagnostics = ["generated_save.not_available"]
    };
    ProjectStandaloneBuildResult BuildWindowsStandalone(CancellationToken cancellationToken = default) => new()
    {
        Status = "FAILED", Stage = "standalone_not_supported", Diagnostics = ["Standalone build is not available for this controller."]
    };
    GameProjectReleaseCandidateFinalizationResult FinalizeCurrentReleaseCandidate() => new()
    {
        Stage = "rc.finalize.current_build_missing",
        Diagnostics = ["Release-candidate finalization is not available for this controller."]
    };
    void CancelWindowsStandaloneBuild() { }
    void LaunchWindowsStandalone() => throw new InvalidOperationException("Standalone build is not available for this controller.");
    void OpenWindowsStandaloneFolder() => throw new InvalidOperationException("Standalone build is not available for this controller.");
    ProjectStandaloneBuildSettings GetStandaloneBuildSettings() => new();
    ProjectStandaloneBuildSettings SaveStandaloneBuildSettings(ProjectStandaloneBuildSettings settings) => settings;
}

public sealed record GameProjectReleaseCandidateFinalizationResult
{
    public string Status { get; init; } = "FAILED";
    public string Stage { get; init; } = "rc.finalize.current_build_missing";
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public GameProjectBuildResult? Build { get; init; }
    public ProjectStandaloneBuildResult? Standalone { get; init; }
    public GameProjectReleaseCandidateReadResult? ReleaseCandidate { get; init; }
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
    private readonly GameProjectSeedRegenerationService? _regenerationService;
    private readonly GameProjectGeneratedWorldRollbackService? _worldRollbackService;
    private readonly GeneratedGameplaySaveService? _generatedGameplaySaveService;
    private readonly GeneratedGameplaySaveMigrationService? _generatedGameplaySaveMigrationService;
    private readonly GeneratedGameplaySavesSummaryService? _generatedGameplaySavesSummaryService;
    private readonly IGameProjectOperationCoordinator _operationCoordinator;
    private GameProjectBuildResult? _lastBuild;
    private GameProjectBuildResult? _lastSuccessfulBuild;
    private ProjectStandaloneBuildResult? _lastStandaloneAttempt;
    private IReadOnlyList<string> _persistedHistoryDiagnostics = [];
    private GameProjectSeedRegenerationResult? _lastRegenerationAttempt;
    private GameProjectGeneratedWorldRollbackResult? _lastWorldRollbackAttempt;
    private GeneratedGameplaySavesSummary? _lastGeneratedGameplaySaves;
    private GeneratedGameplaySaveMigrationResult? _lastGeneratedGameplaySaveMigration;

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
        GameProjectGeneratedWorldSummaryService? generatedWorldSummaryService = null,
        GameProjectSeedRegenerationService? regenerationService = null,
        IGameProjectOperationCoordinator? operationCoordinator = null,
        GameProjectGeneratedWorldRollbackService? worldRollbackService = null,
        GeneratedGameplaySaveService? generatedGameplaySaveService = null,
        GeneratedGameplaySaveMigrationService? generatedGameplaySaveMigrationService = null,
        GeneratedGameplaySavesSummaryService? generatedGameplaySavesSummaryService = null)
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
        _regenerationService = regenerationService;
        _operationCoordinator = operationCoordinator ?? builder.OperationCoordinator;
        _worldRollbackService = worldRollbackService;
        _generatedGameplaySaveService = generatedGameplaySaveService;
        _generatedGameplaySaveMigrationService = generatedGameplaySaveMigrationService;
        _generatedGameplaySavesSummaryService = generatedGameplaySavesSummaryService;
    }

    public bool HasOpenProject { get; private set; }
    public bool BuildRunning => _builder.BuildRunning;
    public bool RegenerationRunning => _regenerationService?.Running == true;
    public int DirtyTransitionCount => HasOpenProject ? _authoring.State.DirtyTransitionCount : 0;
    public GameProjectBuildResult? LastBuild => _lastBuild;

    public UnifiedGameProjectWorkspaceSnapshot OpenProject(string projectFolder)
        => OpenProjectCore(projectFolder, null);

    public UnifiedGameProjectWorkspaceSnapshot OpenProject(
        string projectFolder,
        GameProjectOperationLease operationLease)
        => OpenProjectCore(projectFolder, operationLease);

    private UnifiedGameProjectWorkspaceSnapshot OpenProjectCore(
        string projectFolder,
        GameProjectOperationLease? operationLease)
    {
        var currentFolder = _currentPackageService.CurrentFolder;
        var currentPackage = _currentPackageService.CurrentPackage;
        if (string.IsNullOrWhiteSpace(currentFolder) || currentPackage is null)
            throw new InvalidOperationException("Open the game package before opening its workspace.");
        var requested = Path.GetFullPath(projectFolder);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!string.Equals(Path.GetFullPath(currentFolder), requested, comparison))
            throw new InvalidOperationException("Workspace project must match the currently opened package folder.");

        if (_regenerationService is not null)
        {
            var recovery = operationLease is null
                ? _regenerationService.Recover(requested)
                : _regenerationService.Recover(requested, operationLease);
            if (!recovery.Passed)
                throw new InvalidOperationException(string.Join(Environment.NewLine, recovery.Diagnostics));
            _currentPackageService.LoadAsync(requested, CancellationToken.None).GetAwaiter().GetResult();
            currentPackage = _currentPackageService.CurrentPackage
                             ?? throw new InvalidOperationException("Не удалось повторно открыть пакет после восстановления транзакции.");
        }

        if (operationLease is null) _authoring.OpenProject(requested, currentPackage);
        else _authoring.OpenProject(requested, currentPackage, operationLease);
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
        var lastWorldChange = _regenerationService?.ReadLastWorldChange(state.ProjectFolder);
        var worldMutationInProgress = _operationCoordinator.ActiveOperationKind is
            GameProjectOperationKinds.RegenerationPreview
            or GameProjectOperationKinds.RegenerationApply
            or GameProjectOperationKinds.WorldHistoryRollbackPreview
            or GameProjectOperationKinds.WorldHistoryRollbackApply;
        var worldChangeRequiresStandalone = WorldChangeRequiresStandalone(
            state.ProjectFolder, releaseCandidate, lastWorldChange);
        var effectiveReleaseCandidate = worldMutationInProgress || worldChangeRequiresStandalone
            ? releaseCandidate with
            {
                ConfigurationStatus = releaseCandidate.Record is null ? "ABSENT" : "LAST_SUCCESS"
            }
            : releaseCandidate;
        var releaseCandidateStatus = GameProjectReleaseCandidateRecordService.ResolveOverallStatus(
            acceptedMechanics,
            acceptedMechanicsCurrent,
            _lastSuccessfulBuild?.PackageSha256 ?? activatedPackageSha,
            _lastSuccessfulBuild?.CompositionPackageSha256 ?? state.Document.LastCompositionPackageSha256,
            _lastSuccessfulBuild?.FinalStateHash ?? state.Document.LastQualifiedFinalStateHash,
            effectiveReleaseCandidate);
        var generatedSource = _generatedSourceService.Validate(state.ProjectFolder);
        if (generatedSource is { Present: true, Passed: true }
            && _generatedGameplaySavesSummaryService is not null
            && !_operationCoordinator.IsBusy)
            _lastGeneratedGameplaySaves = _generatedGameplaySavesSummaryService.Read(state.ProjectFolder);
        var generatedGameplaySaves = generatedSource is { Present: true }
            ? _lastGeneratedGameplaySaves
            : null;
        var lastRegeneration = _regenerationService?.ReadLastSuccessful(state.ProjectFolder);
        var worldHistory = _regenerationService?.ReadWorldHistory(state.ProjectFolder);
        var generatedMatchesCurrent = _lastSuccessfulBuild is not null
                                      && currentFingerprint.Passed
                                      && !string.IsNullOrWhiteSpace(currentFingerprint.Sha256)
                                      && string.Equals(_lastSuccessfulBuild.QualifiedAuthoringFingerprint,
                                          currentFingerprint.Sha256, StringComparison.Ordinal);
        var generatedWorldActivation = _lastSuccessfulBuild?.GeneratedWorldActivation;
        var generatedRegionTravel = _lastSuccessfulBuild?.GeneratedRegionTravel;
        var generatedEncounterCombat = _lastSuccessfulBuild?.GeneratedEncounterCombat;
        var generatedCampaignChoices = _lastSuccessfulBuild?.GeneratedCampaignChoices;
        var generatedWorld = _generatedWorldSummaryService.Restore(
            generatedSource,
            _lastSuccessfulBuild?.GeneratedWorld,
            generatedMatchesCurrent,
            generatedWorldActivation,
            generatedRegionTravel,
            generatedEncounterCombat,
            generatedCampaignChoices);
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
            ,ReleaseCandidateRecordConfigurationStatus = effectiveReleaseCandidate.ConfigurationStatus
            ,ReleaseCandidateConfigurationStatus = releaseCandidateStatus
            ,ReleaseCandidateRecordPath = releaseCandidate.RecordPath
            ,GeneratedWorld = generatedWorld
            ,GeneratedWorldActivation = generatedWorld?.Status is "BUILD_CURRENT" or "START_CURRENT" or "TRAVEL_CURRENT" or "CAMPAIGN_CURRENT" or "LAST_SUCCESS"
                ? generatedWorldActivation
                : null
            ,GeneratedWorldTravelOverlay = generatedWorld?.Status is "TRAVEL_CURRENT" or "CAMPAIGN_CURRENT" or "LAST_SUCCESS"
                ? _lastSuccessfulBuild?.GeneratedWorldTravelOverlay
                : null
            ,GeneratedRegionTravel = generatedWorld?.Status is "TRAVEL_CURRENT" or "CAMPAIGN_CURRENT" or "LAST_SUCCESS"
                ? generatedRegionTravel
                : null
            ,GeneratedEncounterCombat = generatedWorld?.Status is "TRAVEL_CURRENT" or "CAMPAIGN_CURRENT" or "LAST_SUCCESS"
                ? generatedEncounterCombat
                : null
            ,GeneratedCampaignChoices = generatedWorld?.Status is "TRAVEL_CURRENT" or "CAMPAIGN_CURRENT" or "LAST_SUCCESS"
                ? generatedCampaignChoices
                : null
            ,AcceptedMechanicsCompatibility = acceptedMechanicsCompatibility
            ,CanRegenerateGeneratedWorld = _regenerationService is not null
                                           && !BuildRunning
                                           && !RegenerationRunning
                                           && !_operationCoordinator.IsBusy
                                           && !state.Dirty
                                           && validation.Passed
                                           && generatedSource is { Present: true, Passed: true,
                                               GenerationRequest: not null, ResolvedGenerationOptions: not null }
            ,RegenerationRunning = RegenerationRunning
            ,GeneratedWorldGenerationRequest = generatedSource.GenerationRequest
            ,GeneratedWorldResolvedOptions = generatedSource.ResolvedGenerationOptions
            ,LastSuccessfulRegeneration = lastRegeneration is { Passed: true } ? lastRegeneration.Record : null
            ,LastRegenerationAttempt = _lastRegenerationAttempt
            ,ProjectOperationBusy = _operationCoordinator.IsBusy
            ,ActiveProjectOperationKind = _operationCoordinator.ActiveOperationKind
            ,GeneratedWorldHistory = worldHistory is { Passed: true } ? worldHistory : null
            ,CanOpenGeneratedWorldHistory = _worldRollbackService is not null
                                             && !_operationCoordinator.IsBusy
                                             && generatedSource is { Present: true, Passed: true }
            ,LastSuccessfulWorldChange = lastWorldChange is { Passed: true } ? lastWorldChange.Record : null
            ,LastWorldRollbackAttempt = _lastWorldRollbackAttempt
            ,GeneratedGameplaySaves = generatedGameplaySaves
            ,GeneratedGameplaySaveCurrentCount = generatedGameplaySaves?.CurrentCount ?? 0
            ,GeneratedGameplaySaveMigrationRequiredCount = generatedGameplaySaves?.MigrationRequiredCount ?? 0
            ,GeneratedGameplaySaveInvalidCount = generatedGameplaySaves?.InvalidCount ?? 0
            ,LastGeneratedGameplaySaveMigration = _lastGeneratedGameplaySaveMigration?.Revision?.Migration
                                                      ?? generatedGameplaySaves?.LastMigration
        };
    }

    public UnifiedGameProjectWorkspaceSnapshot SetModuleSelected(string moduleId, bool selected)
    {
        EnsureOpen();
        using (var operation = RequireOperation(GameProjectOperationKinds.AuthoringSave))
            _authoring.SetModuleSelected(moduleId, selected, operation);
        return Snapshot();
    }

    public UnifiedGameProjectWorkspaceSnapshot SetParameterValue(string moduleId, string parameterId, JsonElement value)
    {
        EnsureOpen();
        using (var operation = RequireOperation(GameProjectOperationKinds.AuthoringSave))
            _authoring.SetParameterValue(moduleId, parameterId, value, operation);
        return Snapshot();
    }

    public UnifiedGameProjectWorkspaceSnapshot SaveAuthoring()
    {
        EnsureOpen();
        using (var operation = RequireOperation(GameProjectOperationKinds.AuthoringSave))
            _authoring.Save(operation);
        return Snapshot();
    }

    public GameProjectBuildResult BuildAndQualify(CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        if (RegenerationRunning) throw new InvalidOperationException("Дождитесь завершения перегенерации мира.");
        _lastBuild = _builder.Build(_authoring, cancellationToken);
        if (_lastBuild.Passed) _lastSuccessfulBuild = _lastBuild;
        return _lastBuild;
    }

    public ProjectStandaloneBuildResult BuildWindowsStandalone(CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        using var operation = _operationCoordinator.TryAcquire(
            _authoring.State.ProjectFolder, GameProjectOperationKinds.Standalone);
        if (!operation.Acquired) return new ProjectStandaloneBuildResult
        {
            Status = "FAILED",
            Stage = "project_operation_busy",
            Diagnostics = [operation.Diagnostic]
        };
        using var buildOperation = _operationCoordinator.TryAcquireChild(
            operation, _authoring.State.ProjectFolder, GameProjectOperationKinds.Build);
        if (!buildOperation.Acquired) return new ProjectStandaloneBuildResult
        {
            Status = "FAILED",
            Stage = "project_operation_busy",
            Diagnostics = [buildOperation.Diagnostic]
        };
        _lastBuild = _builder.Build(_authoring, buildOperation, cancellationToken);
        if (_lastBuild.Passed) _lastSuccessfulBuild = _lastBuild;
        if (_lastBuild.Passed && _generatedGameplaySavesSummaryService is not null)
            _lastGeneratedGameplaySaves = _generatedGameplaySavesSummaryService.Read(
                _authoring.State.ProjectFolder, operation);
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
                .Concat(GameProjectGeneratedWorldSummaryService.StandaloneTravelHumanFacts(
                    _lastBuild.GeneratedRegionTravel))
                .Concat(GameProjectGeneratedWorldSummaryService.StandaloneCombatHumanFacts(
                    _lastBuild.GeneratedEncounterCombat))
                .Concat(GameProjectGeneratedWorldSummaryService.StandaloneChoiceHumanFacts(
                    _lastBuild.GeneratedCampaignChoices))
                .Concat(_acceptedMechanicsSummaryService.StandaloneHumanFacts(
                    _lastBuild, releaseCandidateFactsAllowed))
                .Concat(GeneratedGameplaySavesSummaryService.StandaloneHumanFacts(
                    _lastGeneratedGameplaySaves).Select(fact => new StandaloneHumanReviewFact
                    {
                        Label = fact.Label,
                        Value = fact.Value
                    })).ToList(),
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
        if (_lastBuild.AcceptedMechanics is not { Passed: true }) return standalone;
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

    public GameProjectReleaseCandidateFinalizationResult FinalizeCurrentReleaseCandidate()
    {
        EnsureOpen();
        var state = _authoring.State;
        var history = _historyReader.ReadLatestMatchingSocialSuccess(
            state.ProjectFolder, state.Document, state.Library);
        var currentBuild = history.LastSuccessfulBuild;
        if (currentBuild is null || !history.MatchesCurrentConfiguration
            || !string.Equals(currentBuild.PackageSha256, state.Document.LastActivatedProjectPackageSha256, StringComparison.Ordinal)
            || !string.Equals(currentBuild.CompositionPackageSha256, state.Document.LastCompositionPackageSha256, StringComparison.Ordinal)
            || !string.Equals(currentBuild.FinalStateHash, state.Document.LastQualifiedFinalStateHash, StringComparison.Ordinal))
            return FinalizationFailure("rc.finalize.current_build_missing", history.Diagnostics);

        var standaloneRead = _standaloneBuild.LoadCurrentQualifiedResult(
            state.ProjectFolder, state.Identity.PackageId);
        if (!standaloneRead.Passed || standaloneRead.Result is null)
            return FinalizationFailure("rc.finalize.current_standalone_missing", [standaloneRead.Diagnostics]);
        var standalone = standaloneRead.Result;
        try
        {
            var record = _releaseCandidateRecordService.Write(
                state.ProjectFolder, state.Identity, currentBuild, standalone);
            _lastBuild = currentBuild;
            _lastSuccessfulBuild = currentBuild;
            _lastStandaloneAttempt = standalone;
            var read = _releaseCandidateRecordService.Read(new GameProjectReleaseCandidateReadRequest
            {
                ProjectFolder = state.ProjectFolder,
                Document = state.Document,
                Library = state.Library,
                Identity = state.Identity
            });
            if (read.Record is null || read.ConfigurationStatus != "CURRENT")
                return new GameProjectReleaseCandidateFinalizationResult
                {
                    Stage = "rc.finalize.read_not_current", Build = currentBuild, Standalone = standalone,
                    ReleaseCandidate = read, Diagnostics = read.Diagnostics
                };
            return new GameProjectReleaseCandidateFinalizationResult
            {
                Status = "GREEN", Stage = "rc.finalize.success", Build = currentBuild,
                Standalone = standalone, ReleaseCandidate = read
            };
        }
        catch (Exception exception) when (exception is IOException
                                          or UnauthorizedAccessException
                                          or InvalidOperationException
                                          or JsonException)
        {
            var stage = exception.Message.StartsWith("rc.write.payload", StringComparison.Ordinal)
                        || exception.Message.StartsWith("rc.write.actual_payload", StringComparison.Ordinal)
                ? "rc.finalize.payload_invalid"
                : "rc.finalize.write_failed";
            return FinalizationFailure(stage, [exception.Message], currentBuild, standalone);
        }
    }

    private static GameProjectReleaseCandidateFinalizationResult FinalizationFailure(
        string stage,
        IReadOnlyList<string> diagnostics,
        GameProjectBuildResult? build = null,
        ProjectStandaloneBuildResult? standalone = null) => new()
    {
        Stage = stage, Diagnostics = diagnostics, Build = build, Standalone = standalone
    };

    public GameProjectSeedRegenerationRequest CreateGeneratedWorldRegenerationRequest(
        SeededGeneratedProjectGenerationRequest generationRequest)
    {
        EnsureOpen();
        if (_regenerationService is null)
            throw new InvalidOperationException("Перегенерация мира недоступна для этого контроллера.");
        return _regenerationService.CreateRequest(_authoring.State.ProjectFolder, generationRequest);
    }

    public GameProjectSeedRegenerationPreview PreviewGeneratedWorldRegeneration(
        GameProjectSeedRegenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        if (_regenerationService is null)
            throw new InvalidOperationException("Перегенерация мира недоступна для этого контроллера.");
        return _regenerationService.Preview(request, cancellationToken);
    }

    public GameProjectSeedRegenerationResult ApplyGeneratedWorldRegeneration(
        GameProjectSeedRegenerationRequest request,
        GameProjectSeedRegenerationPreview preview)
    {
        EnsureOpen();
        if (_regenerationService is null)
            throw new InvalidOperationException("Перегенерация мира недоступна для этого контроллера.");
        var projectFolder = _authoring.State.ProjectFolder;
        var result = _regenerationService.Apply(request, preview);
        _lastRegenerationAttempt = result;
        if (!result.Applied) return result;
        var refreshed = OpenProject(projectFolder);
        _lastRegenerationAttempt = result;
        return result with { AuthoritativeSnapshot = refreshed with { LastRegenerationAttempt = result } };
    }

    public GeneratedWorldHistoryReadResult ReadGeneratedWorldHistory()
    {
        EnsureOpen();
        return _regenerationService?.ReadWorldHistory(_authoring.State.ProjectFolder)
               ?? new GeneratedWorldHistoryReadResult
               {
                   Diagnostics = ["world_history.not_available"]
               };
    }

    public GameProjectGeneratedWorldRollbackRequest CreateGeneratedWorldRollbackRequest(string targetWorldId)
    {
        EnsureOpen();
        if (_worldRollbackService is null)
            throw new InvalidOperationException("История миров недоступна для этого контроллера.");
        return _worldRollbackService.CreateRequest(_authoring.State.ProjectFolder, targetWorldId);
    }

    public GameProjectGeneratedWorldRollbackPreview PreviewGeneratedWorldRollback(
        GameProjectGeneratedWorldRollbackRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        if (_worldRollbackService is null)
            throw new InvalidOperationException("История миров недоступна для этого контроллера.");
        return _worldRollbackService.Preview(request, cancellationToken);
    }

    public GameProjectGeneratedWorldRollbackResult ApplyGeneratedWorldRollback(
        GameProjectGeneratedWorldRollbackRequest request,
        GameProjectGeneratedWorldRollbackPreview preview)
    {
        EnsureOpen();
        if (_worldRollbackService is null)
            throw new InvalidOperationException("История миров недоступна для этого контроллера.");
        var projectFolder = _authoring.State.ProjectFolder;
        var result = _worldRollbackService.Apply(request, preview);
        _lastWorldRollbackAttempt = result;
        if (!result.Applied) return result;
        var refreshed = OpenProject(projectFolder);
        _lastWorldRollbackAttempt = result;
        return result with
        {
            AuthoritativeSnapshot = refreshed with { LastWorldRollbackAttempt = result }
        };
    }

    public GeneratedGameplaySaveListResult ListGeneratedGameplaySaves()
    {
        EnsureOpen();
        return _generatedGameplaySaveService?.List(_authoring.State.ProjectFolder)
               ?? new GeneratedGameplaySaveListResult { Diagnostics = ["generated_save.not_available"] };
    }

    public GeneratedGameplaySaveMigrationPreview PreviewGeneratedGameplaySaveMigration(string slotName)
    {
        EnsureOpen();
        return _generatedGameplaySaveMigrationService?.Preview(_authoring.State.ProjectFolder, slotName)
               ?? new GeneratedGameplaySaveMigrationPreview
               {
                   SlotName = slotName,
                   Diagnostics = ["generated_save.not_available"]
               };
    }

    public GeneratedGameplaySaveMigrationResult ApplyGeneratedGameplaySaveMigration(
        GeneratedGameplaySaveMigrationPreview preview)
    {
        EnsureOpen();
        ArgumentNullException.ThrowIfNull(preview);
        if (_generatedGameplaySaveMigrationService is null)
            return new GeneratedGameplaySaveMigrationResult
            {
                SlotName = preview.SlotName,
                SourceRevisionSha256 = preview.SourceRevisionSha256,
                Diagnostics = ["generated_save.not_available"]
            };
        var result = _generatedGameplaySaveMigrationService.Apply(
            new GeneratedGameplaySaveMigrationApplyRequest
            {
                ProjectFolder = _authoring.State.ProjectFolder,
                SlotName = preview.SlotName,
                SourceRevisionSha256 = preview.SourceRevisionSha256,
                CandidateSessionSha256 = preview.CandidateSessionSha256
            });
        _lastGeneratedGameplaySaveMigration = result;
        if (result.Passed && _generatedGameplaySavesSummaryService is not null)
            _lastGeneratedGameplaySaves = _generatedGameplaySavesSummaryService.Read(
                _authoring.State.ProjectFolder);
        return result;
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


    private static bool WorldChangeRequiresStandalone(
        string projectFolder,
        GameProjectReleaseCandidateReadResult releaseCandidate,
        GameProjectGeneratedWorldChangeReadResult? worldChange)
    {
        if (releaseCandidate.Record is null
            || worldChange is not { Passed: true, Record: not null }) return false;
        var path = releaseCandidate.RecordPath;
        if (!File.Exists(path)) return false;
        using var stream = File.OpenRead(path);
        var current = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        return string.Equals(current, worldChange.Record.PreviousReleaseCandidateRecordSha256,
            StringComparison.Ordinal);
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

    private GameProjectOperationLease RequireOperation(string operationKind)
    {
        var operation = _operationCoordinator.TryAcquire(_authoring.State.ProjectFolder, operationKind);
        if (!operation.Acquired) throw new InvalidOperationException(operation.Diagnostic);
        return operation;
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
