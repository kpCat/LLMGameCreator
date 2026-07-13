using System.Security.Cryptography;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
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
    private int _buildRunning;

    public GameProjectBuildAndQualificationService(
        string repositoryRoot,
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        IGamePackageRepository packageRepository,
        IGamePackageValidator packageValidator,
        ICurrentGamePackageService currentPackageService,
        IGameProjectPackageActivationStore? activationStore = null,
        IGameProjectSupportFileSource? supportFileSource = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _packageValidator = packageValidator ?? throw new ArgumentNullException(nameof(packageValidator));
        _currentPackageService = currentPackageService ?? throw new ArgumentNullException(nameof(currentPackageService));
        _activationStore = activationStore ?? new AtomicGameProjectPackageActivationStore();
        _supportFileSource = supportFileSource ?? new NarrowAlphaTemplateSupportFileSource(
            Path.Combine(_repositoryRoot, "samples", "minimal-map-game"));
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

            stagingRoot = GameProjectFeatureModuleAuthoringService.ConfinedPath(
                state.ProjectFolder,
                UnifiedGameProjectWorkspaceVocabulary.BuildStagingRelativeRoot + "/" + Guid.NewGuid().ToString("N"));
            var materializationRoot = Path.Combine(stagingRoot, "materialized");
            var certificationExecutionRoot = Path.Combine(stagingRoot, "certification");
            Directory.CreateDirectory(stagingRoot);

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
            var materialized = materializer.MaterializeAndQualify(
                _repositoryRoot,
                state.Library,
                materializationDocument,
                materializationRoot,
                useCapabilityDrivenRuntimePlaythrough: true);
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
                materializationDocument.CompositionId,
                "package.json");
            if (!File.Exists(qualifiedPackagePath))
                throw new FileNotFoundException("Qualified package was not materialized.", qualifiedPackagePath);
            var qualifiedHash = HashFile(qualifiedPackagePath);
            if (!string.Equals(qualifiedHash, materialized.PackageSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("Qualified package hash mismatch rejected.");

            var activatedPackagePath = Path.Combine(stagingRoot, "identity-overlaid", "package.json");
            var overlay = _identityOverlay.Overlay(qualifiedPackagePath, activatedPackagePath, state.Identity);
            if (!string.Equals(overlay.CompositionPackageSha256, materialized.PackageSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("Composition package hash changed during identity overlay.");

            var qualifiedPackage = _packageRepository.LoadAsync(Path.GetDirectoryName(activatedPackagePath)!, cancellationToken)
                .GetAwaiter().GetResult();
            var compositionPackage = _packageRepository.LoadAsync(Path.GetDirectoryName(qualifiedPackagePath)!, cancellationToken)
                .GetAwaiter().GetResult();
            AssertIdentity(qualifiedPackage, state.Identity);
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

            supportFilePlan = _supportFileMaterializer.CreatePlan(
                qualifiedPackage,
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
                activatedPackagePath,
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
            transaction.ActivatePackageAsync(activatedPackagePath, cancellationToken)
                .GetAwaiter().GetResult();

            var realProjectValidation = _packageValidator.Validate(qualifiedPackage, state.ProjectFolder);
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

            transaction.ReplaceCurrentPackage(qualifiedPackage);

            authoring.ApplyQualifiedDocument(savedDocument with
            {
                LastMaterializedPackageSha256 = overlay.CompositionPackageSha256,
                LastCompositionPackageSha256 = overlay.CompositionPackageSha256,
                LastActivatedProjectPackageSha256 = overlay.ActivatedProjectPackageSha256,
                LastQualifiedFinalStateHash = projectQualification.Session.CurrentStateHash,
                LastQualificationStatus = "GREEN"
            });
            var qualifiedDocument = authoring.Save();
            var historyPath = WriteHistory(
                state.ProjectFolder,
                materialized,
                overlay.CompositionPackageSha256,
                overlay.ActivatedProjectPackageSha256,
                projectQualification.Session.CurrentStateHash,
                qualifiedDocument.ParameterValues.Count,
                ledger,
                attempt.AttemptId);
            transaction.Commit();

            var capabilityPlan = projectQualification.StartRequest.CapabilityPlan
                                 ?? throw new InvalidOperationException("Capability-driven Runtime plan is missing.");
            var equipmentAction = capabilityPlan.OrderedActions.FirstOrDefault(action =>
                action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.EquipItem);
            var equipmentSummary = projectQualification.Session.LatestEquipmentSummary;
            var attributesSummary = projectQualification.Session.LatestAttributesSummary;
            var progressionSummary = projectQualification.Session.LatestProgressionSummary;
            var runtimeEvents = projectQualification.Session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();
            var damageEvent = runtimeEvents.LastOrDefault(runtimeEvent => runtimeEvent.EventType == "DamageApplied"
                                  && (runtimeEvent.Args.ContainsKey("equipmentDamageBonus") || runtimeEvent.Args.ContainsKey("statDamageBonus")))
                              ?? runtimeEvents.LastOrDefault(runtimeEvent => runtimeEvent.EventType == "DamageApplied");
            var weaponDamageBonus = 0;
            var combatDamageDelta = 0;
            if (equipmentAction is not null)
            {
                var rawDelta = damageEvent?.Args.GetValueOrDefault("equipmentDamageBonus");
                if (!string.IsNullOrWhiteSpace(rawDelta))
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
                statDamageObserved = !string.IsNullOrWhiteSpace(rawStatDamage);
                if (statDamageObserved)
                    statDamageBonus = decimal.Parse(rawStatDamage!, NumberStyles.Number, CultureInfo.InvariantCulture);
            }
            var progressionAction = capabilityPlan.OrderedActions.FirstOrDefault(action =>
                action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.InspectProgression);
            ProgressionState? inspectedProgression = null;
            if (progressionAction is not null)
                inspectedProgression = runtimeState.Progressions.Single(progression =>
                    progression.ProgressionId == progressionAction.ResolvedTargetId);
            var rawTotalAdditionalDamage = damageEvent?.Args.GetValueOrDefault("totalAdditionalDamage");
            var totalAdditionalDamage = string.IsNullOrWhiteSpace(rawTotalAdditionalDamage)
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

            return new GameProjectBuildResult
            {
                Status = "GREEN",
                Passed = true,
                HumanSummary = string.Join(Environment.NewLine, summaryLines),
                SelectedMechanicCount = state.Library.Manifest.RequiredCoreModuleCount + savedDocument.SelectedModuleIds.Count,
                ConfiguredParameterCount = qualifiedDocument.ParameterValues.Count,
                PackageSha256 = overlay.ActivatedProjectPackageSha256,
                CompositionPackageSha256 = overlay.CompositionPackageSha256,
                ActivatedProjectPackageSha256 = overlay.ActivatedProjectPackageSha256,
                FinalStateHash = projectQualification.Session.CurrentStateHash,
                CheckpointReloadPassed = projectQualification.CheckpointReplay.Passed,
                FullReplayEquivalent = projectQualification.FinalReplay.Passed
                                       && projectQualification.FinalReplay.ActualStateHash == projectQualification.Session.CurrentStateHash,
                ActionBindingPassed = projectQualification.ActionDescriptorExecutionBindingPassed,
                PackageActivated = true,
                PackageActivationTransactional = true,
                CertificationExecutedCount = ledger.ExecutedCount,
                CertificationReusedCount = ledger.ReusedCount,
                BuildHistoryPath = historyPath,
                RequiredSupportFileCount = supportFilePlan.RequiredFileCount,
                CopiedSupportFileCount = supportActivation.CopiedFileCount,
                ReusedSupportFileCount = supportActivation.ReusedFileCount,
                SupportFilesPrepared = true,
                SupportFileDiagnostics = supportFilePlan.Diagnostics,
                StagedProjectValidationPassed = true,
                RealProjectValidationPassed = true,
                RuntimePlaythroughPlanId = capabilityPlan.PlanId,
                CapabilityCount = capabilityPlan.CapabilityIds.Count,
                PlannedActionCount = capabilityPlan.OrderedActions.Count,
                CheckpointActionCount = projectQualification.CheckpointActionCount,
                FinalReplayActionCount = projectQualification.FinalReplay.ReplayedActionCount,
                PlaythroughSignature = capabilityPlan.ActionPlanSignature,
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
                AttemptedCapabilityCount = capabilityPlan.CapabilityIds.Count,
                AttemptedPlannedActionCount = capabilityPlan.OrderedActions.Count,
                AttemptedCheckpointActionCount = projectQualification.CheckpointActionCount,
                AttemptedFinalReplayActionCount = projectQualification.FinalReplay.ReplayedActionCount,
                AttemptedCompositionPackageSha256 = overlay.CompositionPackageSha256,
                AttemptedFinalStateHash = projectQualification.Session.CurrentStateHash,
                RuntimeFrames = projectQualification.Session.ActionJournal
                    .OrderBy(entry => entry.ActionIndex)
                    .ThenBy(entry => entry.ActionRequestId, StringComparer.Ordinal)
                    .Select(entry => new GameProjectRuntimeFrame
                    {
                        Index = entry.ActionIndex,
                        ActionId = entry.ActionId,
                        Title = string.IsNullOrWhiteSpace(entry.CanonicalStepId) ? entry.ActionId : entry.CanonicalStepId,
                        Category = entry.Category,
                        StateHash = entry.StateHashAfter
                    }).ToList()
            };
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
        FeatureModuleParameterizedCompositionResult result,
        string compositionPackageSha256,
        string activatedProjectPackageSha256,
        string finalStateHash,
        int configuredParameterCount,
        FeatureModuleCertificationLedger ledger,
        string attemptId)
    {
        var root = GameProjectFeatureModuleAuthoringService.ConfinedPath(
            projectFolder,
            UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot);
        Directory.CreateDirectory(root);
        var fileName = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffffffZ") + ".json";
        var path = Path.Combine(root, fileName);
        var entry = new GameProjectBuildHistoryEntry
        {
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Status = "GREEN",
            PackageSha256 = activatedProjectPackageSha256,
            CompositionPackageSha256 = compositionPackageSha256,
            ActivatedProjectPackageSha256 = activatedProjectPackageSha256,
            FinalStateHash = finalStateHash,
            SelectedMechanicCount = result.SelectedModuleCount,
            ConfiguredParameterCount = configuredParameterCount,
            CertificationExecutedCount = ledger.ExecutedCount,
            CertificationReusedCount = ledger.ReusedCount,
            CheckpointReloadPassed = result.CheckpointReloadPassed,
            FullReplayEquivalent = result.FullReplayEquivalent,
            ActionBindingPassed = result.ActionBindingPassed,
            AttemptId = attemptId,
            AttemptStatus = "GREEN",
            AttemptedSelectedModuleIds = result.SourceDocument.SelectedModuleIds,
            AttemptedCapabilityCount = result.Qualification.Artifacts.Session.CapabilityPlan?.CapabilityIds.Count ?? 0,
            AttemptedPlannedActionCount = result.Qualification.Artifacts.Session.CapabilityPlan?.OrderedActions.Count ?? 0,
            AttemptedCheckpointActionCount = result.Qualification.Artifacts.CheckpointReplay.ReplayedActionCount,
            AttemptedFinalReplayActionCount = result.Qualification.Artifacts.FinalReplay.ReplayedActionCount
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
        string packageSha256)
    {
        var selected = document.SelectedModuleIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
        var capabilityPlan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(
            effectiveCatalog.Modules.Where(module => module.Required
                                                     || selected.Contains(module.ModuleId, StringComparer.Ordinal)).ToList(),
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
