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
        if (Interlocked.CompareExchange(ref _buildRunning, 1, 0) != 0)
            return Failure("Сборка уже выполняется. Дождитесь её завершения.", ["concurrent build rejected"]);

        string? stagingRoot = null;
        GameProjectBuildTransaction? transaction = null;
        FeatureModuleCompositionDocument? savedDocument = null;
        FeatureModuleCompositionDocument? preBuildDocument = null;
        var preBuildDirty = false;
        GameProjectSupportFilePlan? supportFilePlan = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var preBuildState = authoring.State;
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
            var validation = new FeatureModuleCompositionDocumentValidator().Validate(savedDocument, state.Library);
            if (!validation.Passed)
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Исправьте настройки механик перед сборкой.",
                    validation.Diagnostics);

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
                    ledger.Entries.SelectMany(entry => entry.Diagnostics).ToList());

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
            if (!materialized.Passed)
                return RollbackFailure(
                    authoring,
                    preBuildDocument,
                    preBuildDirty,
                    transaction,
                    "Игра не прошла проверку Runtime.",
                    materialized.Qualification.Result.Diagnostics);

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
                state.Library,
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
                ledger);
            transaction.Commit();

            var capabilityPlan = projectQualification.StartRequest.CapabilityPlan
                                 ?? throw new InvalidOperationException("Capability-driven Runtime plan is missing.");
            var equipmentAction = capabilityPlan.OrderedActions.FirstOrDefault(action =>
                action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.EquipItem);
            var equipmentSummary = projectQualification.Session.LatestEquipmentSummary;
            var attributesSummary = projectQualification.Session.LatestAttributesSummary;
            var progressionSummary = projectQualification.Session.LatestProgressionSummary;
            var weaponDamageBonus = 0;
            var combatDamageDelta = 0;
            if (equipmentAction is not null)
            {
                var itemId = equipmentAction.Args.GetValueOrDefault("itemId") ?? string.Empty;
                var item = qualifiedPackage.Game.Items.Single(definition => definition.Id == itemId);
                if (item.Metadata.TryGetValue("combat_damage_bonus", out var rawBonus))
                    weaponDamageBonus = int.Parse(rawBonus, NumberStyles.Integer, CultureInfo.InvariantCulture);
                var rawDelta = projectQualification.Session.LatestSnapshot.RuntimeEvents
                    .Where(runtimeEvent => runtimeEvent.EventType == "DamageApplied")
                    .Select(runtimeEvent => runtimeEvent.Args.GetValueOrDefault("equipmentDamageBonus"))
                    .LastOrDefault(value => !string.IsNullOrWhiteSpace(value));
                if (!string.IsNullOrWhiteSpace(rawDelta))
                    combatDamageDelta = (int)decimal.Parse(rawDelta, NumberStyles.Number, CultureInfo.InvariantCulture);
            }
            var runtimeState = projectQualification.Session.CanonicalSession.RuntimeSession.GameplayState;
            var attributesAction = capabilityPlan.OrderedActions.FirstOrDefault(action =>
                action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.InspectAttributes);
            var statDamageBonus = 0m;
            StatValueState? inspectedStat = null;
            if (attributesAction is not null)
            {
                inspectedStat = runtimeState.Stats.Single(stat => stat.StatId == attributesAction.ResolvedTargetId);
                var scalingAbility = qualifiedPackage.Game.Abilities.Single(ability =>
                    ability.Metadata.GetValueOrDefault("source_stat_damage_stat_id") == inspectedStat.StatId);
                var baseline = decimal.Parse(scalingAbility.Metadata["source_stat_damage_baseline"],
                    NumberStyles.Number, CultureInfo.InvariantCulture);
                var perPoint = decimal.Parse(scalingAbility.Metadata["source_stat_damage_per_point"],
                    NumberStyles.Number, CultureInfo.InvariantCulture);
                statDamageBonus = ((decimal)inspectedStat.Value - baseline) * perPoint;
            }
            var progressionAction = capabilityPlan.OrderedActions.FirstOrDefault(action =>
                action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.InspectProgression);
            ProgressionState? inspectedProgression = null;
            if (progressionAction is not null)
                inspectedProgression = runtimeState.Progressions.Single(progression =>
                    progression.ProgressionId == progressionAction.ResolvedTargetId);
            var totalAdditionalDamage = statDamageBonus + weaponDamageBonus;
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
                summaryLines.Add("Бонус урона: +" + weaponDamageBonus.ToString(CultureInfo.InvariantCulture));
            }
            if (attributesAction is not null && inspectedStat is not null)
            {
                summaryLines.Add(attributesAction.Args.GetValueOrDefault("title", inspectedStat.StatId)
                                 + ": " + FormatNumber(inspectedStat.Value));
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
                TotalAdditionalDamage = totalAdditionalDamage
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
            return Failure(
                "Сборка не завершена. Текущий пакет не изменён.",
                [exception.Message],
                rollback,
                supportFilePlan);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(stagingRoot) && Directory.Exists(stagingRoot))
                Directory.Delete(stagingRoot, recursive: true);
            Volatile.Write(ref _buildRunning, 0);
        }
    }

    private GameProjectBuildResult RollbackFailure(
        GameProjectFeatureModuleAuthoringService authoring,
        FeatureModuleCompositionDocument preBuildDocument,
        bool preBuildDirty,
        GameProjectBuildTransaction transaction,
        string summary,
        IReadOnlyList<string> diagnostics,
        GameProjectSupportFilePlan? supportFilePlan = null)
    {
        var rolledBack = transaction.Rollback();
        authoring.RestoreInMemoryDocument(preBuildDocument, preBuildDirty);
        return Failure(summary + " Текущий пакет не изменён.", diagnostics, rolledBack, supportFilePlan);
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
        FeatureModuleCertificationLedger ledger)
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
            ActionBindingPassed = result.ActionBindingPassed
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
        FeatureModuleLibrarySnapshot library,
        FeatureModuleCompositionDocument document,
        string packagePath,
        string packageSha256)
    {
        var selected = document.SelectedModuleIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
        var capabilityPlan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(
            library.Catalog.Modules.Where(module => module.Required
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

    private static GameProjectBuildResult Failure(
        string summary,
        IReadOnlyList<string> diagnostics,
        bool rollback = false,
        GameProjectSupportFilePlan? supportFilePlan = null) => new()
        {
            Status = "FAILED",
            HumanSummary = summary,
            Diagnostics = diagnostics,
            RollbackApplied = rollback,
            PackageActivationTransactional = true,
            RequiredSupportFileCount = supportFilePlan?.RequiredFileCount ?? 0,
            SupportFileDiagnostics = supportFilePlan?.Diagnostics ?? []
        };
}
