using System.Security.Cryptography;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public sealed class GameProjectBuildAndQualificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _repositoryRoot;
    private readonly ISelectedRuntimeVariantInteractiveSessionService _runtime;
    private readonly IGamePackageRepository _packageRepository;
    private readonly IGamePackageValidator _packageValidator;
    private readonly ICurrentGamePackageService _currentPackageService;
    private readonly IGameProjectPackageActivationStore _activationStore;
    private readonly IGameProjectSupportFileSource _supportFileSource;
    private readonly GameProjectSupportFileMaterializer _supportFileMaterializer = new();
    private readonly GameProjectPackageIdentityOverlayService _identityOverlay = new();
    private readonly SeededGeneratedProjectSourceService _generatedSource;
    private readonly GameProjectGeneratedWorldSummaryService _generatedSummary;
    private readonly GameProjectGeneratedWorldActivationService? _generatedActivation;
    private int _buildRunning;

    public GameProjectBuildAndQualificationService(
        string repositoryRoot,
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        IGamePackageRepository packageRepository,
        IGamePackageValidator packageValidator,
        ICurrentGamePackageService currentPackageService,
        IGameProjectPackageActivationStore? activationStore = null,
        IGameProjectSupportFileSource? supportFileSource = null,
        SeededGeneratedProjectSourceService? generatedSource = null,
        GameProjectGeneratedWorldSummaryService? generatedSummary = null,
        GameProjectGeneratedWorldActivationService? generatedActivation = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _packageValidator = packageValidator ?? throw new ArgumentNullException(nameof(packageValidator));
        _currentPackageService = currentPackageService ?? throw new ArgumentNullException(nameof(currentPackageService));
        _activationStore = activationStore ?? new AtomicGameProjectPackageActivationStore();
        _supportFileSource = supportFileSource ?? new NarrowAlphaTemplateSupportFileSource(
            Path.Combine(_repositoryRoot, "samples", "minimal-map-game"));
        _generatedSource = generatedSource ?? new SeededGeneratedProjectSourceService(packageValidator);
        _generatedSummary = generatedSummary ?? new GameProjectGeneratedWorldSummaryService();
        _generatedActivation = generatedActivation;
    }

    public bool BuildRunning => Volatile.Read(ref _buildRunning) != 0;

    public GameProjectBuildResult Build(
        GameProjectFeatureModuleAuthoringService authoring,
        CancellationToken cancellationToken = default)
    {
        var attempt = new GameProjectBuildResult
        {
            AttemptId = Guid.NewGuid().ToString("N"),
            AttemptStatus = "RUNNING"
        };
        if (Interlocked.CompareExchange(ref _buildRunning, 1, 0) != 0)
            return Failure("Сборка уже выполняется. Дождитесь её завершения.",
                ["build.concurrent_rejected"], "build.concurrent", attempt);

        string? stagingRoot = null;
        GameProjectBuildTransaction? transaction = null;
        FeatureModuleCompositionDocument? savedDocument = null;
        FeatureModuleCompositionDocument? preBuildDocument = null;
        var preBuildDirty = false;
        GameProjectSupportFilePlan? supportFilePlan = null;
        string? projectFolder = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var preBuildState = authoring.State;
            projectFolder = preBuildState.ProjectFolder;
            preBuildDocument = preBuildState.Document;
            preBuildDirty = preBuildState.Dirty;
            transaction = new GameProjectBuildTransaction(
                preBuildState.ProjectFolder,
                authoring.DocumentPath,
                authoring.IdentityPath,
                authoring.LegacyDocumentPath,
                _currentPackageService,
                _activationStore);
            savedDocument = authoring.Save();
            var state = authoring.State;
            attempt = attempt with
            {
                AttemptedSelectedModuleIds = savedDocument.SelectedModuleIds,
                AttemptedConfiguredParameterCount = savedDocument.ParameterValues.Count
            };
            var validation = new FeatureModuleCompositionDocumentValidator().Validate(savedDocument, state.Library);
            if (!validation.Passed)
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Исправьте настройки механик перед сборкой.",
                    validation.Diagnostics,
                    "authoring.validation",
                    attempt);
            var authoringFingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(savedDocument, state.Library);
            if (!authoringFingerprint.Passed)
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Не удалось подтвердить семантическую конфигурацию механик.",
                    authoringFingerprint.Diagnostics,
                    "authoring.fingerprint",
                    attempt);

            stagingRoot = GameProjectFeatureModuleAuthoringService.ConfinedPath(
                state.ProjectFolder,
                UnifiedGameProjectWorkspaceVocabulary.BuildStagingRelativeRoot + "/" + Guid.NewGuid().ToString("N"));
            var materializationRoot = Path.Combine(stagingRoot, "materialized");
            var certificationExecutionRoot = Path.Combine(stagingRoot, "certification");
            Directory.CreateDirectory(stagingRoot);

            var generatedSource = _generatedSource.Validate(state.ProjectFolder);
            if (generatedSource.Present && !generatedSource.Passed)
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Источник сгенерированного проекта повреждён.",
                    generatedSource.Diagnostics,
                    "generated_source.validation",
                    attempt);
            FeatureModuleCompositionBasePackage? explicitBase = null;
            if (generatedSource is { Present: true, Passed: true, Source: not null })
            {
                var sourceBasePath = GameProjectFeatureModuleAuthoringService.ConfinedPath(
                    state.ProjectFolder,
                    SeededGeneratedProjectVocabulary.GenerationRelativeRoot + "/"
                    + SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName);
                var stagedBasePath = Path.Combine(materializationRoot, "base", "generated-base-package.json");
                Directory.CreateDirectory(Path.GetDirectoryName(stagedBasePath)!);
                File.Copy(sourceBasePath, stagedBasePath, overwrite: false);
                var stagedBaseSha = HashFile(stagedBasePath);
                if (!string.Equals(stagedBaseSha, generatedSource.Source.GeneratedBasePackageSha256, StringComparison.Ordinal))
                    return RollbackFailure(
                        authoring,
                        preBuildDocument,
                        preBuildDirty,
                        transaction,
                        "Сгенерированная база проекта не прошла проверку целостности.",
                        ["generated_source.sidecar_hash_mismatch:" + SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName],
                        "generated_source.base_hash",
                        attempt);
                explicitBase = new FeatureModuleCompositionBasePackage
                {
                    PackagePath = stagedBasePath,
                    PackageSha256 = stagedBaseSha,
                    SourceKind = FeatureModuleCompositionBasePackageSourceKinds.SeededGeneratedBase,
                    SourceIdentity = generatedSource.Source.PlanId
                };
            }

            var certification = new FeatureModuleCertificationService(
                _runtime,
                new FeatureModuleCertificationCache(GameProjectFeatureModuleAuthoringService.ConfinedPath(
                    state.ProjectFolder,
                    UnifiedGameProjectWorkspaceVocabulary.CertificationCacheRelativeRoot)));
            var ledger = certification.Certify(
                _repositoryRoot,
                state.Library,
                ResolveBaselineSha256(),
                certificationExecutionRoot);
            if (ledger.Status != "GREEN")
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Не удалось подтвердить совместимость выбранных механик.",
                    ledger.Entries.SelectMany(entry => entry.Diagnostics).ToList(),
                    "composition.certification",
                    attempt);

            var materializer = new FeatureModuleParameterizedCompositionService(_runtime);
            // Keep the generic mechanics package byte-compatible with the accepted Goal146/147
            // composition. The project-scoped identity remains the persisted authoring identity
            // and filename; project manifest identity is applied only in the overlay below.
            var materializationDocument = savedDocument with
            {
                CompositionId = UnifiedGameProjectWorkspaceVocabulary.LegacyCompositionId
            };
            var materialized = explicitBase is null
                ? materializer.MaterializeAndQualify(
                    _repositoryRoot,
                    state.Library,
                    materializationDocument,
                    materializationRoot,
                    useCapabilityDrivenRuntimePlaythrough: true)
                : materializer.MaterializeAndQualify(
                    _repositoryRoot,
                    state.Library,
                    materializationDocument,
                    materializationRoot,
                    useCapabilityDrivenRuntimePlaythrough: true,
                    explicitBase);
            var attemptedPlan = materialized.Qualification.Artifacts.Session.CapabilityPlan;
            attempt = attempt with
            {
                AttemptedCapabilityCount = attemptedPlan?.CapabilityIds.Count ?? 0,
                AttemptedPlannedActionCount = attemptedPlan?.OrderedActions.Count ?? 0,
                AttemptedCheckpointActionCount = materialized.Qualification.Artifacts.CheckpointReplay.ReplayedActionCount,
                AttemptedFinalReplayActionCount = materialized.Qualification.Artifacts.FinalReplay.ReplayedActionCount,
                AttemptedCompositionPackageSha256 = materialized.PackageSha256,
                AttemptedFinalStateHash = materialized.FinalStateHash
            };
            if (!materialized.Passed)
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Игра не прошла проверку Runtime.",
                    materialized.Diagnostics,
                    materialized.FailureStage,
                    attempt);

            var qualifiedPackagePath = Path.Combine(
                materializationRoot,
                "compositions",
                materialized.Qualification.Result.CompositionId,
                "package.json");
            if (!File.Exists(qualifiedPackagePath))
                throw new FileNotFoundException("Qualified package was not materialized.", qualifiedPackagePath);
            var qualifiedHash = HashFile(qualifiedPackagePath);
            if (!string.Equals(qualifiedHash, materialized.PackageSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("Qualified package hash mismatch rejected.");

            var activatedPackagePath = Path.Combine(stagingRoot, "compatibility-identity-overlaid", "package.json");
            var overlay = _identityOverlay.Overlay(qualifiedPackagePath, activatedPackagePath, state.Identity);
            if (!string.Equals(overlay.CompositionPackageSha256, materialized.PackageSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("Composition package hash changed during identity overlay.");

            var qualifiedPackage = _packageRepository.LoadAsync(Path.GetDirectoryName(activatedPackagePath)!, cancellationToken)
                .GetAwaiter().GetResult();
            var compositionPackage = _packageRepository.LoadAsync(Path.GetDirectoryName(qualifiedPackagePath)!, cancellationToken)
                .GetAwaiter().GetResult();
            AssertIdentity(qualifiedPackage, state.Identity);
            GameProjectGeneratedWorldSummary? generatedWorld = generatedSource.Present
                ? _generatedSummary.BuildCurrent(generatedSource, compositionPackage, qualifiedPackage)
                : null;
            if (generatedWorld is { Present: true, Passed: false })
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Сгенерированные записи не сохранились в собранном пакете.",
                    generatedWorld.Diagnostics,
                    "generated_content.preservation",
                    attempt);
            var effectiveSelectedModules = materialized.Plan.ParameterBinding.EffectiveCatalog.Modules
                .Where(module => module.Required || savedDocument.SelectedModuleIds.Contains(module.ModuleId, StringComparer.Ordinal))
                .OrderBy(module => module.ModuleId, StringComparer.Ordinal)
                .ToList();
            var projectQualification = QualifyIdentityOverlaidPackage(
                qualifiedPackage,
                compositionPackage,
                materialized.Plan.ParameterBinding.EffectiveCatalog,
                materializationDocument,
                activatedPackagePath,
                overlay.ActivatedProjectPackageSha256);
            if (!string.Equals(projectQualification.Session.CurrentStateHash, materialized.FinalStateHash, StringComparison.Ordinal))
                throw new InvalidOperationException("Project identity overlay changed canonical Runtime final-state semantics.");
            if (!projectQualification.CheckpointReplay.Passed
                || !projectQualification.FinalReplay.Passed
                || !projectQualification.ActionDescriptorExecutionBindingPassed)
                throw new InvalidOperationException("Identity-overlaid project package failed canonical Runtime qualification.");

            var effectEvaluator = new FeatureModuleRuntimeEffectEvaluator();
            var socialObservations = effectEvaluator.Evaluate(
                effectiveSelectedModules,
                projectQualification.Session,
                new LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession(),
                qualifiedPackage);
            IReadOnlyList<FeatureModuleRuntimeEffectObservation> acceptedObservations = socialObservations;
            var combatMetricKinds = new HashSet<string>(StringComparer.Ordinal)
            {
                FeatureModuleRuntimeEffectMetricKinds.CombatDamageDelta,
                FeatureModuleRuntimeEffectMetricKinds.CombatStatDamageDelta
            };
            var combatEffectModuleIds = effectiveSelectedModules
                .Where(module => module.RuntimeEffectContracts.Any(effect => combatMetricKinds.Contains(effect.MetricKind)))
                .Select(module => module.ModuleId)
                .ToHashSet(StringComparer.Ordinal);
            var combatEffectCount = effectiveSelectedModules.Sum(module =>
                module.RuntimeEffectContracts.Count(effect => combatMetricKinds.Contains(effect.MetricKind)));
            if (combatEffectModuleIds.Count > 0)
            {
                var combatProbeModules = effectiveSelectedModules
                    .Where(module => module.Required || combatEffectModuleIds.Contains(module.ModuleId))
                    .ToList();
                var combatProbe = QualifyIdentityOverlaidPackage(
                    qualifiedPackage,
                    compositionPackage,
                    materialized.Plan.ParameterBinding.EffectiveCatalog,
                    materializationDocument,
                    activatedPackagePath,
                    overlay.ActivatedProjectPackageSha256,
                    combatProbeModules);
                if (!combatProbe.CheckpointReplay.Passed
                    || !combatProbe.FinalReplay.Passed
                    || !combatProbe.ActionDescriptorExecutionBindingPassed)
                    return RollbackFailure(
                        authoring,
                        preBuildDocument,
                        preBuildDirty,
                        transaction,
                        "Интегрированные боевые эффекты не прошли Runtime-проверку.",
                        ["accepted mechanics combat probe failed"],
                        "accepted_mechanics.combat_probe",
                        attempt);
                var combatObservations = effectEvaluator.Evaluate(
                        combatProbeModules,
                        combatProbe.Session,
                        new LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession(),
                        qualifiedPackage)
                    .Where(observation => combatMetricKinds.Contains(observation.MetricKind))
                    .ToList();
                if (combatObservations.Count != combatEffectCount || combatObservations.Any(observation => !observation.Passed))
                    return RollbackFailure(
                        authoring,
                        preBuildDocument,
                        preBuildDirty,
                        transaction,
                        "Интегрированные боевые эффекты не подтверждены.",
                        combatObservations.SelectMany(observation => observation.Diagnostics)
                            .DefaultIfEmpty("accepted mechanics combat observations failed").ToList(),
                        "accepted_mechanics.combat_observations",
                        attempt);
                acceptedObservations = socialObservations
                    .Where(observation => !combatMetricKinds.Contains(observation.MetricKind))
                    .Concat(combatObservations)
                    .ToList();
            }
            var social = new SocialRuntimeReviewProjectionService().Project(
                effectiveSelectedModules,
                qualifiedPackage,
                projectQualification.StartRequest.CapabilityPlan
                    ?? throw new InvalidOperationException("Capability-driven Runtime plan is missing."),
                projectQualification.Session,
                socialObservations,
                projectQualification.CheckpointReplay.Passed,
                projectQualification.FinalReplay.Passed
                    && projectQualification.FinalReplay.ActualStateHash == projectQualification.Session.CurrentStateHash);
            if (social.Present && !social.Passed)
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Социальные последствия не прошли причинную проверку.",
                    social.Diagnostics,
                    "social.projection",
                    attempt);

            var capabilityPlan = projectQualification.StartRequest.CapabilityPlan
                                 ?? throw new InvalidOperationException("Capability-driven Runtime plan is missing.");
            var compatibilityFrames = projectQualification.Session.ActionJournal
                .OrderBy(entry => entry.ActionIndex)
                .ThenBy(entry => entry.ActionRequestId, StringComparer.Ordinal)
                .Select(entry => new GameProjectRuntimeFrame
                {
                    Index = entry.ActionIndex,
                    ActionId = entry.ActionId,
                    Title = string.IsNullOrWhiteSpace(entry.CanonicalStepId) ? entry.ActionId : entry.CanonicalStepId,
                    Category = entry.Category,
                    StateHash = entry.StateHashAfter
                }).ToList();
            var compatibility = new GameProjectAcceptedMechanicsCompatibilityResult
            {
                Passed = projectQualification.CheckpointReplay.Passed
                         && projectQualification.FinalReplay.Passed
                         && projectQualification.ActionDescriptorExecutionBindingPassed,
                CompatibilityCompositionPackageSha256 = materialized.PackageSha256,
                CompatibilityActivatedPackageSha256 = overlay.ActivatedProjectPackageSha256,
                CompatibilityFinalStateHash = projectQualification.Session.CurrentStateHash,
                CheckpointReloadPassed = projectQualification.CheckpointReplay.Passed,
                FullReplayEquivalent = projectQualification.FinalReplay.Passed
                                       && projectQualification.FinalReplay.ActualStateHash
                                       == projectQualification.Session.CurrentStateHash,
                ActionBindingPassed = projectQualification.ActionDescriptorExecutionBindingPassed,
                RuntimeFrames = compatibilityFrames,
                Social = social,
                Diagnostics = []
            };

            var finalCompositionPackage = compositionPackage;
            var finalPackage = qualifiedPackage;
            var finalActivatedPath = activatedPackagePath;
            var primaryCompositionSha256 = overlay.CompositionPackageSha256;
            var primaryPackageSha256 = overlay.ActivatedProjectPackageSha256;
            var primaryFinalStateHash = projectQualification.Session.CurrentStateHash;
            var primaryCheckpointReloadPassed = projectQualification.CheckpointReplay.Passed;
            var primaryFullReplayEquivalent = projectQualification.FinalReplay.Passed
                                              && projectQualification.FinalReplay.ActualStateHash
                                              == projectQualification.Session.CurrentStateHash;
            var primaryActionBindingPassed = projectQualification.ActionDescriptorExecutionBindingPassed;
            IReadOnlyList<GameProjectRuntimeFrame> primaryRuntimeFrames = compatibilityFrames;
            var primaryRuntimePlanId = capabilityPlan.PlanId;
            var primaryCapabilityCount = capabilityPlan.CapabilityIds.Count;
            var primaryPlannedActionCount = capabilityPlan.OrderedActions.Count;
            var primaryCheckpointActionCount = projectQualification.CheckpointActionCount;
            var primaryFinalReplayActionCount = projectQualification.FinalReplay.ReplayedActionCount;
            var primaryPlaythroughSignature = capabilityPlan.ActionPlanSignature;
            GameProjectGeneratedWorldActivationSummary? generatedWorldActivation = null;
            if (generatedSource.Present)
            {
                if (_generatedActivation is null)
                    return RollbackFailure(
                        authoring,
                        preBuildDocument,
                        preBuildDirty,
                        transaction,
                        "Runtime-активация сгенерированного мира недоступна.",
                        ["generated_activation.runtime_unavailable"],
                        "generated_activation.runtime",
                        attempt);
                var activation = _generatedActivation.Activate(new GameProjectGeneratedWorldActivationRequest
                {
                    CompatibilityPackagePath = qualifiedPackagePath,
                    CompatibilityPackage = compositionPackage,
                    GeneratedSource = generatedSource,
                    ProjectIdentity = state.Identity,
                    OutputRoot = Path.Combine(stagingRoot, "generated-world-activation")
                });
                if (!activation.Passed)
                    return RollbackFailure(
                        authoring,
                        preBuildDocument,
                        preBuildDirty,
                        transaction,
                        "Сгенерированный мир не прошёл игровой старт, движение и взаимодействие.",
                        activation.Diagnostics,
                        "generated_activation.runtime",
                        attempt);
                finalCompositionPackage = activation.PlayerCompositionPackage;
                finalPackage = activation.ActivatedProjectPackage;
                finalActivatedPath = activation.ActivatedProjectPackagePath;
                primaryCompositionSha256 = activation.PlayerCompositionPackageSha256;
                primaryPackageSha256 = activation.ActivatedProjectPackageSha256;
                primaryFinalStateHash = activation.Summary.FinalStateHash;
                primaryCheckpointReloadPassed = activation.Summary.StateRoundtripPassed;
                primaryFullReplayEquivalent = activation.Summary.ReplayEquivalent;
                primaryActionBindingPassed = activation.Summary.StartSucceeded
                                             && activation.Summary.MoveSucceeded
                                             && activation.Summary.InteractSucceeded
                                             && activation.Summary.GeneratedInteractionObserved;
                primaryRuntimeFrames = activation.Summary.RuntimeFrames;
                primaryRuntimePlanId = "generated-world-activation-v1";
                primaryCapabilityCount = 3;
                primaryPlannedActionCount = 3;
                primaryCheckpointActionCount = 3;
                primaryFinalReplayActionCount = 3;
                primaryPlaythroughSignature = "start>move_right>interact";
                generatedWorldActivation = activation.Summary;
                generatedWorld = _generatedSummary.BuildCurrent(
                    generatedSource,
                    finalCompositionPackage,
                    finalPackage,
                    generatedWorldActivation);
                if (generatedWorld is { Present: true, Passed: false })
                    return RollbackFailure(
                        authoring,
                        preBuildDocument,
                        preBuildDirty,
                        transaction,
                        "Сгенерированные записи не сохранились в активированном пакете.",
                        generatedWorld.Diagnostics,
                        "generated_content.activation_preservation",
                        attempt);
            }

            supportFilePlan = _supportFileMaterializer.CreatePlan(
                finalPackage,
                state.ProjectFolder,
                _supportFileSource);
            if (!supportFilePlan.IsValid)
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Не удалось подготовить файлы проекта.",
                    supportFilePlan.Diagnostics,
                    "project.support_files",
                    attempt,
                    supportFilePlan);

            var validationProjectRoot = _supportFileMaterializer.StageValidationProject(
                finalActivatedPath,
                supportFilePlan,
                Path.Combine(stagingRoot, "validation-project"));
            var stagedPackage = _packageRepository.LoadAsync(validationProjectRoot, cancellationToken)
                .GetAwaiter().GetResult();
            var stagedValidation = _packageValidator.Validate(stagedPackage, validationProjectRoot);
            if (!stagedValidation.IsValid)
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Собранный пакет содержит ошибки.",
                    stagedValidation.Issues.Select(issue => issue.ToString()).ToList(),
                    "project.staged_validation",
                    attempt,
                    supportFilePlan);

            var supportActivation = transaction.ActivateSupportFiles(supportFilePlan, cancellationToken);
            transaction.ActivatePackageAsync(finalActivatedPath, cancellationToken)
                .GetAwaiter().GetResult();

            var realProjectValidation = _packageValidator.Validate(finalPackage, state.ProjectFolder);
            if (!realProjectValidation.IsValid)
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Собранный пакет содержит ошибки после активации.",
                    realProjectValidation.Issues.Select(issue => issue.ToString()).ToList(),
                    "project.activated_validation",
                    attempt,
                    supportFilePlan);

            transaction.ReplaceCurrentPackage(finalPackage);

            authoring.ApplyQualifiedDocument(savedDocument with
            {
                LastMaterializedPackageSha256 = primaryCompositionSha256,
                LastCompositionPackageSha256 = primaryCompositionSha256,
                LastActivatedProjectPackageSha256 = primaryPackageSha256,
                LastQualifiedFinalStateHash = primaryFinalStateHash,
                LastQualificationStatus = "GREEN"
            });
            var qualifiedDocument = authoring.Save();

            var equipmentAction = capabilityPlan.OrderedActions.FirstOrDefault(action =>
                action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.EquipItem);
            var equipmentSummary = projectQualification.Session.LatestEquipmentSummary;
            var attributesSummary = projectQualification.Session.LatestAttributesSummary;
            var progressionSummary = projectQualification.Session.LatestProgressionSummary;
            var runtimeEvents = projectQualification.Session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();
            var damageEvent = runtimeEvents.LastOrDefault(runtimeEvent => runtimeEvent.EventType == "DamageApplied"
                                  && (runtimeEvent.Args.ContainsKey("equipmentDamageBonus") || runtimeEvent.Args.ContainsKey("statDamageBonus")))
                              ?? runtimeEvents.LastOrDefault(runtimeEvent => runtimeEvent.EventType == "DamageApplied");
            var qualifiedEquipmentDamage = QualifiedObservationDecimal(
                acceptedObservations,
                FeatureModuleRuntimeEffectMetricKinds.CombatDamageDelta);
            var qualifiedStatDamage = QualifiedObservationDecimal(
                acceptedObservations,
                FeatureModuleRuntimeEffectMetricKinds.CombatStatDamageDelta);
            var weaponDamageBonus = 0;
            var combatDamageDelta = 0;
            if (equipmentAction is not null)
            {
                var rawDelta = damageEvent?.Args.GetValueOrDefault("equipmentDamageBonus");
                if (qualifiedEquipmentDamage.HasValue)
                    weaponDamageBonus = combatDamageDelta = (int)qualifiedEquipmentDamage.Value;
                else if (!string.IsNullOrWhiteSpace(rawDelta))
                    weaponDamageBonus = combatDamageDelta = (int)decimal.Parse(
                        rawDelta, NumberStyles.Number, CultureInfo.InvariantCulture);
            }
            var runtimeState = projectQualification.Session.CanonicalSession.RuntimeSession.GameplayState;
            var attributesAction = capabilityPlan.OrderedActions.FirstOrDefault(action =>
                action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.InspectAttributes);
            var statDamageBonus = 0m;
            var statDamageObserved = false;
            StatValueState? inspectedStat = null;
            if (attributesAction is not null)
            {
                inspectedStat = runtimeState.Stats.Single(stat => stat.StatId == attributesAction.ResolvedTargetId);
                var rawStatDamage = damageEvent?.Args.GetValueOrDefault("statDamageBonus");
                statDamageObserved = qualifiedStatDamage.HasValue || !string.IsNullOrWhiteSpace(rawStatDamage);
                if (qualifiedStatDamage.HasValue)
                    statDamageBonus = qualifiedStatDamage.Value;
                else if (statDamageObserved)
                    statDamageBonus = decimal.Parse(rawStatDamage!, NumberStyles.Number, CultureInfo.InvariantCulture);
            }
            var progressionAction = capabilityPlan.OrderedActions.FirstOrDefault(action =>
                action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.InspectProgression);
            ProgressionState? inspectedProgression = null;
            if (progressionAction is not null)
                inspectedProgression = runtimeState.Progressions.Single(progression =>
                    progression.ProgressionId == progressionAction.ResolvedTargetId);
            var rawTotalAdditionalDamage = damageEvent?.Args.GetValueOrDefault("totalAdditionalDamage");
            var totalAdditionalDamage = qualifiedEquipmentDamage.HasValue && qualifiedStatDamage.HasValue
                ? qualifiedEquipmentDamage.Value + qualifiedStatDamage.Value
                : string.IsNullOrWhiteSpace(rawTotalAdditionalDamage)
                ? damageEvent is null ? 0m : weaponDamageBonus + statDamageBonus
                : decimal.Parse(rawTotalAdditionalDamage, NumberStyles.Number, CultureInfo.InvariantCulture);
            var useAbilityAction = capabilityPlan.OrderedActions.FirstOrDefault(action =>
                action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.UseAbility);
            var useAbilityId = useAbilityAction?.Args.GetValueOrDefault("abilityId", useAbilityAction.ResolvedTargetId)
                               ?? string.Empty;
            var allSnapshots = projectQualification.Session.CanonicalSession.Snapshots;
            var abilitySnapshot = useAbilityAction is null ? null : allSnapshots.LastOrDefault(snapshot =>
                snapshot.RuntimeEvents.Any(runtimeEvent => runtimeEvent.EventType == "AbilityUsed"
                    && runtimeEvent.TargetId == useAbilityId));
            var abilityDefinition = useAbilityAction is null ? null : qualifiedPackage.Game.Abilities
                .SingleOrDefault(ability => ability.Id == useAbilityId);
            var abilityDirectDamage = abilitySnapshot?.RuntimeEvents.Where(runtimeEvent => runtimeEvent.EventType == "DamageApplied")
                .Select(RuntimeEventDamage).FirstOrDefault() ?? 0m;
            var manaEvent = abilitySnapshot?.RuntimeEvents.LastOrDefault(runtimeEvent => runtimeEvent.EventType == "CostConsumed"
                && runtimeEvent.TargetId == "resource/mana");
            var manaBefore = EventDecimal(manaEvent, "before");
            var manaSpent = EventDecimal(manaEvent, "cost");
            var manaRemaining = EventDecimal(manaEvent, "after");
            var statusAdded = abilitySnapshot?.RuntimeEvents.LastOrDefault(runtimeEvent => runtimeEvent.EventType == "StatusAdded");
            var statusId = statusAdded?.Args.GetValueOrDefault("statusId") ?? string.Empty;
            var statusDefinition = qualifiedPackage.Game.Statuses.SingleOrDefault(status => status.Id == statusId);
            var statusDuration = (int)EventDecimal(statusAdded, "duration");
            var tickSnapshot = allSnapshots.FirstOrDefault(snapshot => snapshot.RuntimeEvents.Any(runtimeEvent =>
                runtimeEvent.EventType == "StatusTicked" && runtimeEvent.Args.GetValueOrDefault("statusId") == statusId));
            var statusTickDamage = tickSnapshot?.RuntimeEvents.Where(runtimeEvent => runtimeEvent.EventType == "DamageApplied")
                .Select(RuntimeEventDamage).FirstOrDefault() ?? 0m;
            var statusExpired = statusId.Length > 0 && allSnapshots.SelectMany(snapshot => snapshot.RuntimeEvents)
                .Any(runtimeEvent => runtimeEvent.EventType == "StatusRemoved"
                    && runtimeEvent.Message.Contains("expired", StringComparison.OrdinalIgnoreCase));
            var statusRemainingTicks = runtimeState.ActiveEncounter?.Participants.SelectMany(participant => participant.Statuses)
                .SingleOrDefault(status => status.StatusId == statusId)?.RemainingTicks ?? 0;
            var abilitySummary = abilityDefinition?.Name ?? string.Empty;
            var manaSummary = manaEvent is null ? string.Empty : FormatNumber((double)manaBefore) + " → "
                + FormatNumber((double)manaRemaining) + " (стоимость " + FormatNumber((double)manaSpent) + ")";
            var statusSummary = statusDefinition is null ? string.Empty : statusDefinition.Name + ", " + statusDuration + " ходов";
            var summaryLines = new List<string>
            {
                "Игра успешно собрана и проверена.",
                "Механик включено: " + (state.Library.Manifest.RequiredCoreModuleCount + savedDocument.SelectedModuleIds.Count),
                "Параметров настроено: " + qualifiedDocument.ParameterValues.Count,
                "Сохранение/загрузка: пройдено",
                "Повтор действий: пройден",
                "Файлы проекта подготовлены: " + supportFilePlan.RequiredFileCount,
                "Пакет проекта обновлён"
            };
            if (equipmentAction is not null)
            {
                summaryLines.Add("Экипировано: " + equipmentAction.Args.GetValueOrDefault("itemTitle", equipmentAction.Args.GetValueOrDefault("itemId", string.Empty)));
                summaryLines.Add("Слот: " + equipmentAction.Args.GetValueOrDefault("slotTitle", equipmentAction.Args.GetValueOrDefault("slotId", string.Empty)));
                if (damageEvent is not null)
                    summaryLines.Add("Бонус урона: +" + weaponDamageBonus.ToString(CultureInfo.InvariantCulture));
            }
            if (attributesAction is not null && inspectedStat is not null)
            {
                summaryLines.Add(attributesAction.Args.GetValueOrDefault("title", inspectedStat.StatId)
                                 + ": " + FormatNumber(inspectedStat.Value));
                if (statDamageObserved)
                    summaryLines.Add(attributesAction.Args.GetValueOrDefault("damageBonusTitle", "Бонус урона")
                                     + ": +" + statDamageBonus.ToString(CultureInfo.InvariantCulture));
            }
            if (progressionAction is not null && inspectedProgression is not null)
            {
                var stage = inspectedProgression.StageId ?? string.Empty;
                var stageValue = stage.Contains('/')
                    ? stage[(stage.LastIndexOf('/') + 1)..] : stage;
                summaryLines.Add(progressionAction.Args.GetValueOrDefault("stageTitle", "Уровень") + ": " + stageValue);
                summaryLines.Add(progressionAction.Args.GetValueOrDefault("amountTitle", "Опыт")
                                 + ": " + FormatNumber(inspectedProgression.Amount));
            }
            if (useAbilityAction is not null)
            {
                summaryLines.Add("Способность: " + abilitySummary);
                summaryLines.Add("Прямой урон: " + abilityDirectDamage.ToString(CultureInfo.InvariantCulture));
                if (manaEvent is not null) summaryLines.Add("Мана: " + manaSummary);
                if (statusDefinition is not null)
                {
                    summaryLines.Add("Эффект: " + statusSummary);
                    summaryLines.Add("Урон эффекта: " + statusTickDamage.ToString(CultureInfo.InvariantCulture) + " за ход");
                    summaryLines.Add("Эффект завершён: " + (statusExpired ? "да" : "нет"));
                }
                summaryLines.Add("Сохранение/повтор: "
                    + (projectQualification.CheckpointReplay.Passed && projectQualification.FinalReplay.Passed ? "пройдено" : "не пройдено"));
            }
            summaryLines.AddRange(SocialRuntimeReviewProjectionService.HumanSummaryLines(social));
            if (generatedWorld is { Present: true, Passed: true })
                summaryLines.Add("Сгенерированный мир: источник и записи подтверждены");
            if (generatedWorldActivation is { Present: true, Passed: true })
                summaryLines.AddRange(generatedWorldActivation.HumanFacts.Select(fact => fact.Label + ": " + fact.Value));

            var buildResult = new GameProjectBuildResult
            {
                Status = "GREEN",
                Passed = true,
                HumanSummary = string.Join(Environment.NewLine, summaryLines),
                SelectedMechanicCount = state.Library.Manifest.RequiredCoreModuleCount + savedDocument.SelectedModuleIds.Count,
                ConfiguredParameterCount = qualifiedDocument.ParameterValues.Count,
                PackageSha256 = primaryPackageSha256,
                CompositionPackageSha256 = primaryCompositionSha256,
                ActivatedProjectPackageSha256 = primaryPackageSha256,
                FinalStateHash = primaryFinalStateHash,
                CheckpointReloadPassed = primaryCheckpointReloadPassed,
                FullReplayEquivalent = primaryFullReplayEquivalent,
                ActionBindingPassed = primaryActionBindingPassed,
                PackageActivated = true,
                PackageActivationTransactional = true,
                CertificationExecutedCount = ledger.ExecutedCount,
                CertificationReusedCount = ledger.ReusedCount,
                RequiredSupportFileCount = supportFilePlan.RequiredFileCount,
                CopiedSupportFileCount = supportActivation.CopiedFileCount,
                ReusedSupportFileCount = supportActivation.ReusedFileCount,
                SupportFilesPrepared = true,
                SupportFileDiagnostics = supportFilePlan.Diagnostics,
                StagedProjectValidationPassed = true,
                RealProjectValidationPassed = true,
                RuntimePlaythroughPlanId = primaryRuntimePlanId,
                CapabilityCount = primaryCapabilityCount,
                PlannedActionCount = primaryPlannedActionCount,
                CheckpointActionCount = primaryCheckpointActionCount,
                FinalReplayActionCount = primaryFinalReplayActionCount,
                PlaythroughSignature = primaryPlaythroughSignature,
                EquipmentSlotSummary = equipmentSummary,
                WeaponDamageBonus = weaponDamageBonus,
                CombatDamageDelta = combatDamageDelta,
                AttributesSummary = attributesSummary,
                ProgressionSummary = progressionSummary,
                StatDamageBonus = statDamageBonus,
                TotalAdditionalDamage = totalAdditionalDamage,
                AbilitySummary = abilitySummary,
                ManaSummary = manaSummary,
                StatusSummary = statusSummary,
                AbilityDirectDamage = abilityDirectDamage,
                ManaBefore = manaBefore,
                ManaSpent = manaSpent,
                ManaRemaining = manaRemaining,
                StatusTickDamage = statusTickDamage,
                StatusRemainingTicks = (int)statusRemainingTicks,
                StatusExpired = statusExpired,
                AttemptId = attempt.AttemptId,
                AttemptStatus = "GREEN",
                AttemptedSelectedModuleIds = attempt.AttemptedSelectedModuleIds,
                AttemptedConfiguredParameterCount = attempt.AttemptedConfiguredParameterCount,
                AttemptedCapabilityCount = primaryCapabilityCount,
                AttemptedPlannedActionCount = primaryPlannedActionCount,
                AttemptedCheckpointActionCount = primaryCheckpointActionCount,
                AttemptedFinalReplayActionCount = primaryFinalReplayActionCount,
                AttemptedCompositionPackageSha256 = primaryCompositionSha256,
                AttemptedFinalStateHash = primaryFinalStateHash,
                RuntimeFrames = primaryRuntimeFrames
                ,Social = social
                ,QualifiedAuthoringFingerprint = authoringFingerprint.Sha256
                ,GeneratedWorld = generatedWorld
                ,GeneratedWorldActivation = generatedWorldActivation
                ,AcceptedMechanicsCompatibility = compatibility
            };
            var acceptedMechanics = new GameProjectAcceptedMechanicsSummaryService().Project(buildResult);
            buildResult = buildResult with
            {
                AcceptedMechanics = acceptedMechanics,
                AcceptedMechanicsCompatibility = compatibility with { AcceptedMechanics = acceptedMechanics }
            };
            var historyPath = WriteHistory(state.ProjectFolder, buildResult, ledger);
            transaction.Commit();
            return buildResult with { BuildHistoryPath = historyPath };
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or JsonException
            or OperationCanceledException)
        {
            var rollback = transaction?.Rollback() ?? false;
            if (preBuildDocument is not null) authoring.RestoreInMemoryDocument(preBuildDocument, preBuildDirty);
            var failed = Failure(
                "Сборка не завершена. Текущий пакет не изменён.",
                [exception.Message],
                "build.exception",
                attempt,
                rollback,
                supportFilePlan);
            return projectFolder is null ? failed : RecordFailure(projectFolder, failed);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(stagingRoot) && Directory.Exists(stagingRoot))
                Directory.Delete(stagingRoot, recursive: true);
            Volatile.Write(ref _buildRunning, 0);
        }
    }

    private static decimal EventDecimal(CanonicalRuntimePlayerCommandLoopRuntimeEvent? runtimeEvent, string key) =>
        runtimeEvent is not null
        && decimal.TryParse(runtimeEvent.Args.GetValueOrDefault(key), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value : 0;

    private static decimal RuntimeEventDamage(CanonicalRuntimePlayerCommandLoopRuntimeEvent runtimeEvent)
    {
        var raw = runtimeEvent.Args.GetValueOrDefault("damage");
        if (string.IsNullOrWhiteSpace(raw))
            raw = System.Text.RegularExpressions.Regex.Match(runtimeEvent.Message, @"-(?<value>\d+(?:\.\d+)?)$").Groups["value"].Value;
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    private static decimal? QualifiedObservationDecimal(
        IReadOnlyList<FeatureModuleRuntimeEffectObservation> observations,
        string metricKind)
    {
        var raw = observations.LastOrDefault(observation =>
            observation.Passed
            && string.Equals(observation.MetricKind, metricKind, StringComparison.Ordinal))?.ActualValue;
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private GameProjectBuildResult RollbackFailure(
        GameProjectFeatureModuleAuthoringService authoring,
        FeatureModuleCompositionDocument preBuildDocument,
        bool preBuildDirty,
        GameProjectBuildTransaction transaction,
        string summary,
        IReadOnlyList<string> diagnostics,
        string failureStage,
        GameProjectBuildResult attempt,
        GameProjectSupportFilePlan? supportFilePlan = null)
    {
        var rolledBack = transaction.Rollback();
        authoring.RestoreInMemoryDocument(preBuildDocument, preBuildDirty);
        var failed = Failure(summary + " Текущий пакет не изменён.", diagnostics,
            failureStage, attempt, rolledBack, supportFilePlan);
        return RecordFailure(authoring.State.ProjectFolder, failed);
    }

    private string ResolveBaselineSha256()
    {
        var path = Path.Combine(
            _repositoryRoot,
            ".llmgc",
            "procedural",
            "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff",
            "product-line-runtime-variant-matrix-result.json");
        using var json = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        return json.RootElement.GetProperty("candidates").EnumerateArray()
            .Single(item => item.GetProperty("candidateId").GetString() == "minimal-map-game-balanced-baseline")
            .GetProperty("packageSha256").GetString()
               ?? throw new InvalidOperationException("Baseline package SHA is missing.");
    }

    private static string WriteHistory(
        string projectFolder,
        GameProjectBuildResult build,
        FeatureModuleCertificationLedger ledger,
        string? fileName = null)
    {
        var root = GameProjectFeatureModuleAuthoringService.ConfinedPath(
            projectFolder,
            UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot);
        Directory.CreateDirectory(root);
        fileName ??= DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffffffZ") + ".json";
        var path = Path.Combine(root, fileName);
        var entry = new GameProjectBuildHistoryEntry
        {
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Status = "GREEN",
            PackageSha256 = build.PackageSha256,
            CompositionPackageSha256 = build.CompositionPackageSha256,
            ActivatedProjectPackageSha256 = build.ActivatedProjectPackageSha256,
            FinalStateHash = build.FinalStateHash,
            SelectedMechanicCount = build.SelectedMechanicCount,
            ConfiguredParameterCount = build.ConfiguredParameterCount,
            CertificationExecutedCount = ledger.ExecutedCount,
            CertificationReusedCount = ledger.ReusedCount,
            CheckpointReloadPassed = build.CheckpointReloadPassed,
            FullReplayEquivalent = build.FullReplayEquivalent,
            ActionBindingPassed = build.ActionBindingPassed,
            AttemptId = build.AttemptId,
            AttemptStatus = "GREEN",
            AttemptedSelectedModuleIds = build.AttemptedSelectedModuleIds,
            AttemptedCapabilityCount = build.AttemptedCapabilityCount,
            AttemptedPlannedActionCount = build.AttemptedPlannedActionCount,
            AttemptedCheckpointActionCount = build.AttemptedCheckpointActionCount,
            AttemptedFinalReplayActionCount = build.AttemptedFinalReplayActionCount,
            Social = build.Social is { Present: true, Passed: true } ? build.Social : null,
            QualifiedAuthoringFingerprint = build.QualifiedAuthoringFingerprint,
            AcceptedMechanics = build.AcceptedMechanics
            ,GeneratedWorld = build.GeneratedWorld
            ,GeneratedWorldActivation = build.GeneratedWorldActivation
            ,AcceptedMechanicsCompatibility = build.AcceptedMechanicsCompatibility
        };
        File.WriteAllText(path, JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine, new UTF8Encoding(false));
        return path;
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string FormatNumber(double value) =>
        value.ToString("0.####", CultureInfo.InvariantCulture);

    private ProductLineRuntimeQualificationResult QualifyIdentityOverlaidPackage(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        LLMGameCreator.GamePackage.GamePackageDefinition compositionPackage,
        LLMGameCreator.Application.Design.FeatureModuleComposition.FeatureModuleCatalogDocument effectiveCatalog,
        FeatureModuleCompositionDocument document,
        string packagePath,
        string packageSha256,
        IReadOnlyList<FeatureModuleDefinition>? qualificationModules = null)
    {
        var selected = document.SelectedModuleIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
        qualificationModules ??= effectiveCatalog.Modules.Where(module => module.Required
                                                                          || selected.Contains(module.ModuleId, StringComparer.Ordinal)).ToList();
        var capabilityPlan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(
            qualificationModules,
            compositionPackage);
        var qualifier = new ProductLineRuntimeQualifier(
            new GameProjectIdentityRuntimeQualificationAdapter(_runtime, compositionPackage.Manifest));
        return qualifier.Qualify(package, new ProductLineRuntimeQualificationRequest
        {
            SessionId = "project-" + document.CompositionId + "-session",
            CandidateId = document.CompositionId,
            VariantKind = selected.Count == 0 ? "baseline_composition" : string.Join("+", selected),
            PackagePath = packagePath,
            PackageSha256 = packageSha256,
            CheckpointId = "project-" + document.CompositionId + "-checkpoint-after-craft",
            FinalCheckpointId = "project-" + document.CompositionId + "-final-journal",
            CapabilityPlan = capabilityPlan
        });
    }

    private static void AssertIdentity(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        GameProjectIdentityDocument identity)
    {
        var manifest = package.Manifest;
        if (!string.Equals(manifest.PackageId, identity.PackageId, StringComparison.Ordinal)
            || !string.Equals(manifest.Title, identity.Title, StringComparison.Ordinal)
            || !string.Equals(manifest.Version, identity.Version, StringComparison.Ordinal)
            || !string.Equals(manifest.FormatVersion, identity.FormatVersion, StringComparison.Ordinal)
            || !string.Equals(manifest.Description ?? string.Empty, identity.Description, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Activated package manifest does not preserve project identity.");
        }
    }

    private static GameProjectBuildResult RecordFailure(string projectFolder, GameProjectBuildResult result)
    {
        var root = GameProjectFeatureModuleAuthoringService.ConfinedPath(
            projectFolder,
            UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot);
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffffffZ") + ".json");
        var entry = new GameProjectBuildHistoryEntry
        {
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Status = "FAILED",
            AttemptId = result.AttemptId,
            AttemptStatus = result.AttemptStatus,
            FailureStage = result.FailureStage,
            AttemptedSelectedModuleIds = result.AttemptedSelectedModuleIds,
            ConfiguredParameterCount = result.AttemptedConfiguredParameterCount,
            AttemptedCapabilityCount = result.AttemptedCapabilityCount,
            AttemptedPlannedActionCount = result.AttemptedPlannedActionCount,
            AttemptedCheckpointActionCount = result.AttemptedCheckpointActionCount,
            AttemptedFinalReplayActionCount = result.AttemptedFinalReplayActionCount,
            CompositionPackageSha256 = result.AttemptedCompositionPackageSha256,
            FinalStateHash = result.AttemptedFinalStateHash,
            Diagnostics = result.Diagnostics.Take(50).ToList()
        };
        File.WriteAllText(path, JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine, new UTF8Encoding(false));
        return result with { BuildHistoryPath = path };
    }

    private static GameProjectBuildResult Failure(
        string summary,
        IReadOnlyList<string> diagnostics,
        string failureStage,
        GameProjectBuildResult attempt,
        bool rollback = false,
        GameProjectSupportFilePlan? supportFilePlan = null) => attempt with
        {
            Status = "FAILED",
            AttemptStatus = "FAILED",
            FailureStage = failureStage,
            HumanSummary = summary,
            Diagnostics = diagnostics.Count == 0 ? [failureStage + ".failed"] : diagnostics,
            RollbackApplied = rollback,
            PackageActivationTransactional = true,
            RequiredSupportFileCount = supportFilePlan?.RequiredFileCount ?? 0,
            SupportFileDiagnostics = supportFilePlan?.Diagnostics ?? []
        };
}
