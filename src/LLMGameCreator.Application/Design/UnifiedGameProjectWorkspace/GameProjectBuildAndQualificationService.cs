using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
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
    private int _buildRunning;

    public GameProjectBuildAndQualificationService(
        string repositoryRoot,
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        IGamePackageRepository packageRepository,
        IGamePackageValidator packageValidator,
        ICurrentGamePackageService currentPackageService,
        IGameProjectPackageActivationStore? activationStore = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _packageValidator = packageValidator ?? throw new ArgumentNullException(nameof(packageValidator));
        _currentPackageService = currentPackageService ?? throw new ArgumentNullException(nameof(currentPackageService));
        _activationStore = activationStore ?? new AtomicGameProjectPackageActivationStore();
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
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            savedDocument = authoring.Save();
            var state = authoring.State;
            var validation = new FeatureModuleCompositionDocumentValidator().Validate(savedDocument, state.Library);
            if (!validation.Passed)
                return Failure("Исправьте настройки механик перед сборкой.", validation.Diagnostics);

            stagingRoot = GameProjectFeatureModuleAuthoringService.ConfinedPath(
                state.ProjectFolder,
                UnifiedGameProjectWorkspaceVocabulary.BuildStagingRelativeRoot + "/" + Guid.NewGuid().ToString("N"));
            var materializationRoot = Path.Combine(stagingRoot, "materialized");
            var certificationExecutionRoot = Path.Combine(stagingRoot, "certification");
            Directory.CreateDirectory(stagingRoot);

            transaction = new GameProjectBuildTransaction(
                state.ProjectFolder,
                authoring.DocumentPath,
                _currentPackageService,
                _activationStore);

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
                    savedDocument,
                    transaction,
                    "Не удалось подтвердить совместимость выбранных механик.",
                    ledger.Entries.SelectMany(entry => entry.Diagnostics).ToList());

            var materializer = new FeatureModuleParameterizedCompositionService(_runtime);
            var materialized = materializer.MaterializeAndQualify(
                _repositoryRoot,
                state.Library,
                savedDocument,
                materializationRoot);
            if (!materialized.Passed)
                return RollbackFailure(
                    authoring,
                    savedDocument,
                    transaction,
                    "Игра не прошла проверку Runtime.",
                    materialized.Qualification.Result.Diagnostics);

            var qualifiedPackagePath = Path.Combine(
                materializationRoot,
                "compositions",
                savedDocument.CompositionId,
                "package.json");
            if (!File.Exists(qualifiedPackagePath))
                throw new FileNotFoundException("Qualified package was not materialized.", qualifiedPackagePath);
            var qualifiedHash = HashFile(qualifiedPackagePath);
            if (!string.Equals(qualifiedHash, materialized.PackageSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("Qualified package hash mismatch rejected.");

            var qualifiedPackage = _packageRepository.LoadAsync(Path.GetDirectoryName(qualifiedPackagePath)!, cancellationToken)
                .GetAwaiter().GetResult();
            var packageValidation = _packageValidator.Validate(qualifiedPackage, state.ProjectFolder);
            if (!packageValidation.IsValid)
                return RollbackFailure(
                    authoring,
                    savedDocument,
                    transaction,
                    "Собранный пакет содержит ошибки.",
                    packageValidation.Issues.Select(issue => issue.ToString()).ToList());

            transaction.ActivateAsync(qualifiedPackagePath, qualifiedPackage, cancellationToken)
                .GetAwaiter().GetResult();

            authoring.ApplyQualifiedDocument(materialized.QualifiedDocument);
            var qualifiedDocument = authoring.Save();
            var historyPath = WriteHistory(
                state.ProjectFolder,
                materialized,
                qualifiedDocument.ParameterValues.Count,
                ledger);
            transaction.Commit();

            return new GameProjectBuildResult
            {
                Status = "GREEN",
                Passed = true,
                HumanSummary = string.Join(Environment.NewLine,
                    "Игра успешно собрана и проверена.",
                    "Механик включено: " + (state.Library.Manifest.RequiredCoreModuleCount + savedDocument.SelectedModuleIds.Count),
                    "Параметров настроено: " + qualifiedDocument.ParameterValues.Count,
                    "Сохранение/загрузка: пройдено",
                    "Повтор действий: пройден",
                    "Пакет проекта обновлён"),
                SelectedMechanicCount = state.Library.Manifest.RequiredCoreModuleCount + savedDocument.SelectedModuleIds.Count,
                ConfiguredParameterCount = qualifiedDocument.ParameterValues.Count,
                PackageSha256 = materialized.PackageSha256,
                FinalStateHash = materialized.FinalStateHash,
                CheckpointReloadPassed = materialized.CheckpointReloadPassed,
                FullReplayEquivalent = materialized.FullReplayEquivalent,
                ActionBindingPassed = materialized.ActionBindingPassed,
                PackageActivated = true,
                PackageActivationTransactional = true,
                CertificationExecutedCount = ledger.ExecutedCount,
                CertificationReusedCount = ledger.ReusedCount,
                BuildHistoryPath = historyPath
            };
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or JsonException
            or OperationCanceledException)
        {
            var rollback = transaction?.Rollback() ?? false;
            if (savedDocument is not null) authoring.RestoreInMemoryDocument(savedDocument, dirty: false);
            return Failure(
                "Сборка не завершена. Текущий пакет не изменён.",
                [exception.Message],
                rollback);
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
        FeatureModuleCompositionDocument savedDocument,
        GameProjectBuildTransaction transaction,
        string summary,
        IReadOnlyList<string> diagnostics)
    {
        var rolledBack = transaction.Rollback();
        authoring.RestoreInMemoryDocument(savedDocument, dirty: false);
        return Failure(summary + " Текущий пакет не изменён.", diagnostics, rolledBack);
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
            PackageSha256 = result.PackageSha256,
            FinalStateHash = result.FinalStateHash,
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

    private static GameProjectBuildResult Failure(
        string summary,
        IReadOnlyList<string> diagnostics,
        bool rollback = false) => new()
        {
            Status = "FAILED",
            HumanSummary = summary,
            Diagnostics = diagnostics,
            RollbackApplied = rollback,
            PackageActivationTransactional = true
        };
}
