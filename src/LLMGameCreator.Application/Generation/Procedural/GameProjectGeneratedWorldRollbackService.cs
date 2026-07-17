using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GameProjectGeneratedWorldRollbackService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _repositoryRoot;
    private readonly ICurrentGamePackageService _currentPackageService;
    private readonly IGamePackageRepository _packageRepository;
    private readonly GameProjectBuildAndQualificationService _builder;
    private readonly SeededGeneratedProjectSourceService _sourceService;
    private readonly GameProjectSeedRegenerationDiffService _diffService;
    private readonly GameProjectSeedRegenerationTransaction _transaction;
    private readonly IGameProjectOperationCoordinator _operationCoordinator;
    private readonly GameProjectSeedRegenerationCandidateSealService _sealService;
    private readonly IGameProjectSeedRegenerationTruthReader _truthReader;
    private readonly IGameProjectSeedRegenerationCommitValidator _commitValidator;
    private readonly GeneratedWorldHistoryService _historyService;
    private readonly GameProjectGeneratedWorldChangeRecordService _worldChangeRecordService;
    private readonly Dictionary<string, SealedGeneratedWorldRollbackCandidate> _previews = new(StringComparer.Ordinal);

    public GameProjectGeneratedWorldRollbackService(
        string repositoryRoot,
        ICurrentGamePackageService currentPackageService,
        IGamePackageRepository packageRepository,
        IGamePackageValidator packageValidator,
        GameProjectBuildAndQualificationService builder,
        SeededGeneratedProjectSourceService sourceService,
        GeneratedWorldHistoryService historyService,
        GameProjectGeneratedWorldChangeRecordService worldChangeRecordService,
        GameProjectSeedRegenerationDiffService? diffService = null,
        GameProjectSeedRegenerationTransaction? transaction = null,
        IGameProjectOperationCoordinator? operationCoordinator = null,
        GameProjectSeedRegenerationCandidateSealService? sealService = null,
        IGameProjectSeedRegenerationTruthReader? truthReader = null,
        IGameProjectSeedRegenerationCommitValidator? commitValidator = null,
        GameProjectSeedRegenerationRecordService? regenerationRecordService = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _currentPackageService = currentPackageService ?? throw new ArgumentNullException(nameof(currentPackageService));
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        ArgumentNullException.ThrowIfNull(packageValidator);
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _worldChangeRecordService = worldChangeRecordService
                                    ?? throw new ArgumentNullException(nameof(worldChangeRecordService));
        _diffService = diffService ?? new GameProjectSeedRegenerationDiffService();
        _transaction = transaction ?? new GameProjectSeedRegenerationTransaction();
        _operationCoordinator = operationCoordinator ?? builder.OperationCoordinator;
        _sealService = sealService ?? new GameProjectSeedRegenerationCandidateSealService();
        _truthReader = truthReader ?? new GameProjectSeedRegenerationTruthReader(_repositoryRoot, _sourceService);
        var record = regenerationRecordService
                     ?? new GameProjectSeedRegenerationRecordService(_repositoryRoot, _sourceService);
        _commitValidator = commitValidator ?? new GameProjectSeedRegenerationCommitValidator(
            _repositoryRoot, _sourceService, packageValidator, record);
    }

    public GameProjectGeneratedWorldRollbackRequest CreateRequest(string projectFolder, string targetWorldId)
    {
        using var operation = RequireOperation(projectFolder, GameProjectOperationKinds.Recovery);
        var current = Capture(projectFolder, operation);
        var history = _historyService.Read(projectFolder, targetWorldId);
        if (!history.Passed || history.Manifest is null)
            throw new InvalidOperationException(history.Diagnostics.FirstOrDefault() ?? "world_rollback.target_invalid");
        var currentWorldId = _historyService.WorldId(projectFolder, current.Source);
        if (string.Equals(currentWorldId, targetWorldId, StringComparison.Ordinal))
            throw new InvalidOperationException("world_rollback.no_semantic_change");
        return new GameProjectGeneratedWorldRollbackRequest
        {
            ProjectFolder = current.ProjectFolder,
            TargetWorldId = targetWorldId,
            ExpectedTruthTokens = current.Tokens,
            ExpectedAuthoritativeInventorySha256 = _truthReader.CaptureAuthoritativeInventorySha256(projectFolder),
            ExpectedWorldHistoryManifestSha256 = HashFile(Path.Combine(history.EntryPath, "manifest.json")),
            ExpectedWorldHistoryTreeSha256 = history.Manifest.GenerationTreeSha256
        };
    }

    public GameProjectGeneratedWorldRollbackPreview Preview(
        GameProjectGeneratedWorldRollbackRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var operation = _operationCoordinator.TryAcquire(
            request.ProjectFolder, GameProjectOperationKinds.WorldHistoryRollbackPreview);
        if (!operation.Acquired) return FailedPreview(request.TargetWorldId, operation.Diagnostic);
        var attemptId = Guid.NewGuid().ToString("N")[..12];
        string? candidate = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = Capture(request.ProjectFolder, operation);
            var race = CompareTokens(request.ExpectedTruthTokens, current.Tokens);
            if (race.Count > 0) return FailedPreview(request.TargetWorldId, "world_rollback.current_truth_changed", attemptId);
            if (!string.Equals(_truthReader.CaptureAuthoritativeInventorySha256(request.ProjectFolder),
                    request.ExpectedAuthoritativeInventorySha256, StringComparison.Ordinal))
                return FailedPreview(request.TargetWorldId, "world_rollback.current_truth_changed", attemptId);
            var target = _historyService.Read(request.ProjectFolder, request.TargetWorldId);
            if (!target.Passed || target.Manifest is null)
                return FailedPreview(request.TargetWorldId,
                    target.Diagnostics.FirstOrDefault() ?? "world_rollback.target_invalid", attemptId);
            if (!string.Equals(HashFile(Path.Combine(target.EntryPath, "manifest.json")),
                    request.ExpectedWorldHistoryManifestSha256, StringComparison.Ordinal)
                || !string.Equals(target.Manifest.GenerationTreeSha256,
                    request.ExpectedWorldHistoryTreeSha256, StringComparison.Ordinal))
                return FailedPreview(request.TargetWorldId, "world_rollback.target_invalid", attemptId);
            var currentWorldId = _historyService.WorldId(current.ProjectFolder, current.Source);
            if (string.Equals(currentWorldId, request.TargetWorldId, StringComparison.Ordinal))
                return FailedPreview(request.TargetWorldId, "world_rollback.no_semantic_change", attemptId);

            candidate = CandidateRoot(attemptId);
            GameProjectSeedRegenerationService.CloneProject(current.ProjectFolder, candidate);
            using var candidateOperation = _operationCoordinator.TryAcquireChild(
                operation, candidate, GameProjectOperationKinds.Build);
            if (!candidateOperation.Acquired)
                return CandidateFailure(request.TargetWorldId, attemptId, candidate, candidateOperation.Diagnostic);
            var generationRoot = Confined(candidate, SeededGeneratedProjectVocabulary.GenerationRelativeRoot);
            if (Directory.Exists(generationRoot)) Directory.Delete(generationRoot, recursive: true);
            CopyDirectory(Path.Combine(target.EntryPath, "generation"), generationRoot);
            PrepareCandidatePackage(candidate, current.Identity);
            var authoring = NewAuthoring();
            var package = LoadPackage(candidate);
            var originalState = authoring.OpenProject(candidate, package, candidateOperation);
            var selected = originalState.Document.SelectedModuleIds.ToList();
            var parameters = ParameterJson(originalState.Document);
            authoring.ApplyQualifiedDocument(originalState.Document with
            {
                LastMaterializedPackageSha256 = string.Empty,
                LastCompositionPackageSha256 = string.Empty,
                LastActivatedProjectPackageSha256 = string.Empty,
                LastQualifiedFinalStateHash = string.Empty,
                LastQualificationStatus = "NOT_RUN"
            });
            authoring.Save(candidateOperation);

            _currentPackageService.LoadAsync(candidate, cancellationToken).GetAwaiter().GetResult();
            GameProjectBuildResult first;
            GameProjectBuildResult second;
            UnifiedGameProjectWorkspaceSnapshot snapshot;
            GameProjectAuthoringState reopenedState;
            try
            {
                first = _builder.Build(authoring, candidateOperation, cancellationToken);
                if (!first.Passed) return CandidateFailure(request.TargetWorldId, attemptId, candidate,
                    first.Diagnostics.FirstOrDefault() ?? "world_rollback.candidate_build_failed", first);
                second = _builder.Build(authoring, candidateOperation, cancellationToken);
                if (!second.Passed) return CandidateFailure(request.TargetWorldId, attemptId, candidate,
                    second.Diagnostics.FirstOrDefault() ?? "world_rollback.candidate_repeat_failed", second);
                if (!BuildIdentityEquals(first, second)) return CandidateFailure(
                    request.TargetWorldId, attemptId, candidate, "world_rollback.candidate_repeat_mismatch", second);
                var reopenedAuthoring = NewAuthoring();
                snapshot = NewController(reopenedAuthoring).OpenProject(candidate, candidateOperation);
                reopenedState = reopenedAuthoring.State;
            }
            finally
            {
                _currentPackageService.LoadAsync(current.ProjectFolder, CancellationToken.None).GetAwaiter().GetResult();
            }
            var candidateSource = _sourceService.Validate(candidate);
            var authoringPreserved = reopenedState.Document.SelectedModuleIds.SequenceEqual(selected, StringComparer.Ordinal)
                                     && string.Equals(ParameterJson(reopenedState.Document), parameters,
                                         StringComparison.Ordinal)
                                     && string.Equals(
                                         new FeatureModuleAuthoringFingerprintService().Calculate(
                                             reopenedState.Document, reopenedState.Library).Sha256,
                                         current.Tokens.QualifiedAuthoringFingerprint, StringComparison.Ordinal);
            var identityPreserved = string.Equals(
                GameProjectSeedRegenerationService.IdentityFingerprint(reopenedState.Identity),
                current.Tokens.ProjectIdentityFingerprint, StringComparison.Ordinal);
            var diff = _diffService.Compare(current.Source, candidateSource, authoringPreserved, identityPreserved);
            if (!diff.GameplayChanged || !authoringPreserved || !identityPreserved
                || snapshot.GeneratedWorld?.Status != "CAMPAIGN_CURRENT"
                || snapshot.GeneratedEncounterCombat is not { Passed: true, Status: "CAMPAIGN_CURRENT" }
                || snapshot.GeneratedWorldActivation is not { Passed: true }
                || snapshot.GeneratedRegionTravel is not { Passed: true }
                || snapshot.AcceptedMechanicsCompatibility is not { Passed: true }
                || snapshot.ReleaseCandidateRecordConfigurationStatus == "CURRENT")
                return CandidateFailure(request.TargetWorldId, attemptId, candidate,
                    "world_rollback.candidate_qualification_incomplete", second);

            var publication = Capture(current.ProjectFolder, operation);
            if (CompareTokens(request.ExpectedTruthTokens, publication.Tokens).Count > 0
                || !string.Equals(_truthReader.CaptureAuthoritativeInventorySha256(current.ProjectFolder),
                    request.ExpectedAuthoritativeInventorySha256, StringComparison.Ordinal))
                return CandidateFailure(request.TargetWorldId, attemptId, candidate,
                    "world_rollback.current_truth_changed", second);
            var historyFile = Path.GetFileName(second.BuildHistoryPath);
            var seal = _sealService.Create(candidate, Guid.NewGuid().ToString("N"), attemptId,
                historyFile, second, snapshot, diff, reopenedState);
            var preview = new GameProjectGeneratedWorldRollbackPreview
            {
                AttemptId = attemptId,
                Status = "GREEN",
                Stage = "candidate_sealed",
                TargetWorldId = request.TargetWorldId,
                CandidateSealSha256 = seal.SealSha256,
                CandidateRoot = candidate,
                CandidateBuildHistoryFileName = historyFile,
                Diff = diff,
                CandidateBuild = second,
                CandidateSnapshot = snapshot,
                TargetManifest = target.Manifest
            };
            _previews[attemptId] = new SealedGeneratedWorldRollbackCandidate
            {
                CandidateRoot = candidate,
                Seal = seal,
                PublicPreview = preview,
                CandidateBuild = second,
                CandidateSnapshot = snapshot,
                Diff = diff,
                ExpectedTruthTokens = publication.Tokens,
                ExpectedAuthoritativeInventorySha256 = request.ExpectedAuthoritativeInventorySha256,
                TargetManifest = target.Manifest,
                ExpectedWorldHistoryManifestSha256 = request.ExpectedWorldHistoryManifestSha256,
                ExpectedWorldHistoryTreeSha256 = request.ExpectedWorldHistoryTreeSha256
            };
            return preview;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidOperationException or JsonException
                                           or OperationCanceledException)
        {
            if (candidate is not null && Directory.Exists(candidate)) Directory.Delete(candidate, recursive: true);
            return FailedPreview(request.TargetWorldId, exception.Message, attemptId);
        }
    }

    public GameProjectGeneratedWorldRollbackResult Apply(
        GameProjectGeneratedWorldRollbackRequest request,
        GameProjectGeneratedWorldRollbackPreview preview,
        GameProjectSeedRegenerationFailurePoint failurePoint = GameProjectSeedRegenerationFailurePoint.None)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(preview);
        using var operation = _operationCoordinator.TryAcquire(
            request.ProjectFolder, GameProjectOperationKinds.WorldHistoryRollbackApply);
        if (!operation.Acquired) return FailedResult(preview, operation.Diagnostic);
        if (!_previews.TryGetValue(preview.AttemptId, out var cached)
            || !CallerPreviewMatches(preview, cached.PublicPreview)
            || !string.Equals(preview.CandidateSealSha256, cached.Seal.SealSha256, StringComparison.Ordinal)
            || !Directory.Exists(cached.CandidateRoot))
            return FailedResult(preview, "regeneration.candidate_seal_mismatch");
        try
        {
            using var candidateOperation = _operationCoordinator.TryAcquireChild(
                operation, cached.CandidateRoot, GameProjectOperationKinds.Build);
            if (!candidateOperation.Acquired) return FailedResult(preview, candidateOperation.Diagnostic);
            var candidatePackage = LoadPackage(cached.CandidateRoot);
            var candidateAuthoring = NewAuthoring();
            candidateAuthoring.OpenProject(cached.CandidateRoot, candidatePackage, candidateOperation);
            _currentPackageService.LoadAsync(cached.CandidateRoot, CancellationToken.None).GetAwaiter().GetResult();
            UnifiedGameProjectWorkspaceSnapshot freshSnapshot;
            try
            {
                freshSnapshot = NewController(candidateAuthoring).OpenProject(cached.CandidateRoot, candidateOperation);
            }
            finally
            {
                _currentPackageService.LoadAsync(request.ProjectFolder, CancellationToken.None).GetAwaiter().GetResult();
            }
            var seal = _sealService.Verify(cached.CandidateRoot, cached.Seal, cached.CandidateBuild,
                freshSnapshot, cached.Diff, candidateAuthoring.State);
            if (!seal.Passed) return FailedResult(preview,
                seal.Diagnostics.FirstOrDefault() ?? "regeneration.candidate_tampered");
            if (freshSnapshot.GeneratedWorld?.Status != "CAMPAIGN_CURRENT"
                || freshSnapshot.GeneratedEncounterCombat is not { Passed: true, Status: "CAMPAIGN_CURRENT" }
                || freshSnapshot.GeneratedWorldActivation is not { Passed: true }
                || freshSnapshot.GeneratedRegionTravel is not { Passed: true }
                || freshSnapshot.AcceptedMechanicsCompatibility is not { Passed: true }
                || freshSnapshot.ReleaseCandidateRecordConfigurationStatus == "CURRENT")
                return FailedResult(preview, "regeneration.candidate_tampered");
            var target = _historyService.Read(request.ProjectFolder, cached.PublicPreview.TargetWorldId);
            if (!target.Passed || target.Manifest is null
                || !string.Equals(HashFile(Path.Combine(target.EntryPath, "manifest.json")),
                    cached.ExpectedWorldHistoryManifestSha256, StringComparison.Ordinal)
                || !string.Equals(target.Manifest.GenerationTreeSha256,
                    cached.ExpectedWorldHistoryTreeSha256, StringComparison.Ordinal))
                return FailedResult(preview, "world_rollback.target_invalid");

            var current = Capture(request.ProjectFolder, operation);
            if (CompareTokens(cached.ExpectedTruthTokens, current.Tokens).Count > 0
                || !string.Equals(_truthReader.CaptureAuthoritativeInventorySha256(current.ProjectFolder),
                    cached.ExpectedAuthoritativeInventorySha256, StringComparison.Ordinal))
                return FailedResult(preview, "world_rollback.current_truth_changed");
            var candidateSource = _sourceService.Validate(cached.CandidateRoot);
            if (candidateSource is not { Present: true, Passed: true, Source: not null })
                return FailedResult(preview, "regeneration.candidate_tampered");
            var fromWorldId = _historyService.WorldId(current.ProjectFolder, current.Source);
            var toWorldId = _historyService.WorldId(cached.CandidateRoot, candidateSource);
            if (!string.Equals(toWorldId, cached.PublicPreview.TargetWorldId, StringComparison.Ordinal))
                return FailedResult(preview, "world_rollback.target_invalid");
            var rcPath = new GameProjectReleaseCandidateRecordService().RecordPath(current.ProjectFolder);
            var previousRcHash = File.Exists(rcPath) ? HashFile(rcPath) : null;
            var worldChange = new GameProjectGeneratedWorldChangeRecord
            {
                OperationKind = "history_rollback",
                AttemptId = cached.PublicPreview.AttemptId,
                FromWorldId = fromWorldId,
                ToWorldId = toWorldId,
                OldSourceRecordSha256 = current.Tokens.SourceRecordSha256,
                NewSourceRecordSha256 = cached.Seal.SourceRecordSha256,
                OldPackageSha256 = current.Tokens.ActivatedPackageSha256,
                NewPackageSha256 = cached.CandidateBuild.PackageSha256,
                OldCompositionPackageSha256 = current.Tokens.CompositionPackageSha256,
                NewCompositionPackageSha256 = cached.CandidateBuild.CompositionPackageSha256,
                OldFinalStateHash = current.Tokens.FinalStateHash,
                NewFinalStateHash = cached.CandidateBuild.FinalStateHash,
                QualifiedAuthoringFingerprint = cached.CandidateBuild.QualifiedAuthoringFingerprint,
                Diff = cached.Diff,
                SelectedBuildHistoryFileName = cached.Seal.SelectedBuildHistoryFileName,
                PreviousReleaseCandidateRecordSha256 = previousRcHash,
                PreviousReleaseCandidateStatus = previousRcHash is null ? "ABSENT" : "LAST_SUCCESS",
                CandidateSealSha256 = cached.Seal.SealSha256
            };
            var worldChangeJson = _worldChangeRecordService.Serialize(worldChange);
            var applied = _transaction.Apply(new GameProjectSeedRegenerationTransactionRequest
            {
                AttemptId = cached.PublicPreview.AttemptId,
                ProjectFolder = current.ProjectFolder,
                CandidateFolder = cached.CandidateRoot,
                CandidateBuildHistoryFileName = cached.Seal.SelectedBuildHistoryFileName,
                FailurePoint = failurePoint,
                ExpectedTruthTokens = cached.ExpectedTruthTokens,
                ExpectedAuthoritativeInventorySha256 = cached.ExpectedAuthoritativeInventorySha256,
                CandidateSealSha256 = cached.Seal.SealSha256,
                OperationLease = operation,
                TruthReader = _truthReader,
                CommitValidator = _commitValidator,
                CommitValidationRequest = new GameProjectSeedRegenerationCommitValidationRequest
                {
                    ProjectFolder = current.ProjectFolder,
                    OperationKind = "history_rollback",
                    CandidateSeal = cached.Seal,
                    ExpectedProjectIdentityFingerprint = current.Tokens.ProjectIdentityFingerprint,
                    SelectedBuildHistoryFileName = cached.Seal.SelectedBuildHistoryFileName,
                    PreviousReleaseCandidateRecordSha256 = previousRcHash,
                    ExpectedWorldChangeRecordSha256 = HashText(worldChangeJson)
                },
                WorldHistoryService = _historyService,
                BeforeWorldHistoryOperationKind = GeneratedWorldHistoryOperationKinds.HistoryRollbackBefore,
                AfterWorldHistoryOperationKind = GeneratedWorldHistoryOperationKinds.HistoryRollbackAfter,
                WorldChangeRecordRelativePath = GameProjectGeneratedWorldChangeVocabulary.RelativePath,
                WorldChangeRecordJson = worldChangeJson
            });
            if (!applied.Passed) return new GameProjectGeneratedWorldRollbackResult
            {
                AttemptId = cached.PublicPreview.AttemptId,
                TargetWorldId = cached.PublicPreview.TargetWorldId,
                CandidateSealSha256 = cached.Seal.SealSha256,
                Stage = "atomic_apply",
                Diff = cached.Diff,
                CandidateBuild = cached.CandidateBuild,
                Diagnostics = applied.Diagnostics,
                RollbackApplied = applied.RollbackApplied,
                JournalStatus = applied.JournalStatus,
                TransactionState = applied.TransactionState
            };
            candidateOperation.Dispose();
            if (Directory.Exists(cached.CandidateRoot)) Directory.Delete(cached.CandidateRoot, recursive: true);
            _previews.Remove(cached.PublicPreview.AttemptId);
            operation.Dispose();
            try
            {
                _currentPackageService.LoadAsync(current.ProjectFolder, CancellationToken.None).GetAwaiter().GetResult();
                var authoring = NewAuthoring();
                var snapshot = NewController(authoring).OpenProject(current.ProjectFolder);
                return Success(cached, applied, snapshot, []);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                               or InvalidOperationException or JsonException)
            {
                return Success(cached, applied, null,
                    ["world_rollback.presentation_reopen_failed:" + exception.Message]) with
                {
                    CommittedWithPresentationDiagnostic = true
                };
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidOperationException or JsonException)
        {
            return FailedResult(preview, exception.Message);
        }
    }

    private CurrentCapture Capture(string projectFolder, GameProjectOperationLease operationLease)
    {
        var project = Path.GetFullPath(projectFolder);
        var source = _sourceService.Validate(project);
        if (source is not { Present: true, Passed: true, Source: not null })
            throw new InvalidOperationException(source.Present
                ? "world_rollback.current_source_invalid" : "world_rollback.not_generated_project");
        var package = LoadPackage(project);
        var authoring = NewAuthoring();
        var state = authoring.OpenProject(project, package, operationLease);
        var fingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(state.Document, state.Library);
        if (!fingerprint.Passed) throw new InvalidOperationException("world_rollback.current_authoring_invalid");
        return new CurrentCapture(project, source, state.Identity, new GameProjectSeedRegenerationTruthTokens
        {
            SourceRecordSha256 = HashFile(Confined(project, SeededGeneratedProjectVocabulary.SourceRelativePath)),
            QualifiedAuthoringFingerprint = fingerprint.Sha256,
            AuthoringRevision = state.Document.Revision,
            ActivatedPackageSha256 = HashFile(Confined(project, "package.json")),
            CompositionPackageSha256 = state.Document.LastCompositionPackageSha256,
            FinalStateHash = state.Document.LastQualifiedFinalStateHash,
            ProjectIdentityFingerprint = GameProjectSeedRegenerationService.IdentityFingerprint(state.Identity),
            ReleaseCandidateRecordSha256 = File.Exists(new GameProjectReleaseCandidateRecordService().RecordPath(project))
                ? HashFile(new GameProjectReleaseCandidateRecordService().RecordPath(project)) : null
        });
    }

    private void PrepareCandidatePackage(string candidate, GameProjectIdentityDocument identity)
    {
        var generatedBase = Confined(candidate, SeededGeneratedProjectVocabulary.GenerationRelativeRoot + "/"
                                              + SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName);
        var staged = Confined(candidate, ".llmgc/regeneration-candidate-package/package.json");
        Directory.CreateDirectory(Path.GetDirectoryName(staged)!);
        new GameProjectPackageIdentityOverlayService().Overlay(generatedBase, staged, identity);
        File.Move(staged, Confined(candidate, "package.json"), overwrite: true);
        Directory.Delete(Path.GetDirectoryName(staged)!, recursive: true);
    }

    private GameProjectFeatureModuleAuthoringService NewAuthoring() =>
        new(_repositoryRoot, operationCoordinator: _operationCoordinator);

    private UnifiedGameProjectWorkspaceController NewController(GameProjectFeatureModuleAuthoringService authoring) => new(
        _currentPackageService, authoring, _builder,
        generatedSourceService: _sourceService,
        generatedWorldSummaryService: new GameProjectGeneratedWorldSummaryService(),
        operationCoordinator: _operationCoordinator);

    private GamePackageDefinition LoadPackage(string projectFolder) =>
        _packageRepository.LoadAsync(projectFolder, CancellationToken.None).GetAwaiter().GetResult();

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: false);
        }
    }

    private static bool BuildIdentityEquals(GameProjectBuildResult left, GameProjectBuildResult right) =>
        string.Equals(left.PackageSha256, right.PackageSha256, StringComparison.Ordinal)
        && string.Equals(left.CompositionPackageSha256, right.CompositionPackageSha256, StringComparison.Ordinal)
        && string.Equals(left.FinalStateHash, right.FinalStateHash, StringComparison.Ordinal)
        && left.GeneratedWorld?.SourceRequestSha256 == right.GeneratedWorld?.SourceRequestSha256
        && left.GeneratedRegionTravel?.FinalStateHash == right.GeneratedRegionTravel?.FinalStateHash;

    private static IReadOnlyList<string> CompareTokens(
        GameProjectSeedRegenerationTruthTokens expected,
        GameProjectSeedRegenerationTruthTokens actual)
    {
        var diagnostics = new List<string>();
        if (!string.Equals(expected.SourceRecordSha256, actual.SourceRecordSha256, StringComparison.Ordinal))
            diagnostics.Add("source");
        if (!string.Equals(expected.QualifiedAuthoringFingerprint, actual.QualifiedAuthoringFingerprint,
                StringComparison.Ordinal) || expected.AuthoringRevision != actual.AuthoringRevision)
            diagnostics.Add("authoring");
        if (!string.Equals(expected.ActivatedPackageSha256, actual.ActivatedPackageSha256, StringComparison.Ordinal)
            || !string.Equals(expected.CompositionPackageSha256, actual.CompositionPackageSha256, StringComparison.Ordinal)
            || !string.Equals(expected.FinalStateHash, actual.FinalStateHash, StringComparison.Ordinal))
            diagnostics.Add("package");
        if (!string.Equals(expected.ProjectIdentityFingerprint, actual.ProjectIdentityFingerprint,
                StringComparison.Ordinal)) diagnostics.Add("identity");
        if (!string.Equals(expected.ReleaseCandidateRecordSha256, actual.ReleaseCandidateRecordSha256,
                StringComparison.Ordinal)) diagnostics.Add("rc");
        return diagnostics;
    }

    private static bool CallerPreviewMatches(
        GameProjectGeneratedWorldRollbackPreview caller,
        GameProjectGeneratedWorldRollbackPreview cached) =>
        string.Equals(caller.AttemptId, cached.AttemptId, StringComparison.Ordinal)
        && string.Equals(caller.TargetWorldId, cached.TargetWorldId, StringComparison.Ordinal)
        && string.Equals(caller.CandidateSealSha256, cached.CandidateSealSha256, StringComparison.Ordinal)
        && string.Equals(caller.CandidateRoot, cached.CandidateRoot, StringComparison.Ordinal)
        && string.Equals(caller.CandidateBuildHistoryFileName, cached.CandidateBuildHistoryFileName,
            StringComparison.Ordinal)
        && string.Equals(caller.CandidateBuild?.PackageSha256, cached.CandidateBuild?.PackageSha256,
            StringComparison.Ordinal)
        && string.Equals(JsonSerializer.Serialize(caller.Diff, JsonOptions),
            JsonSerializer.Serialize(cached.Diff, JsonOptions), StringComparison.Ordinal);

    private static string ParameterJson(FeatureModuleCompositionDocument document) => JsonSerializer.Serialize(
        document.ParameterValues.OrderBy(value => value.ModuleId, StringComparer.Ordinal)
            .ThenBy(value => value.ParameterId, StringComparer.Ordinal), JsonOptions);

    private static string CandidateRoot(string attemptId)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "RollbackCandidates", attemptId);
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        return root;
    }

    private static string Confined(string root, string relative) =>
        GameProjectSeedRegenerationService.Confined(root, relative);

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string HashText(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private GameProjectOperationLease RequireOperation(string projectFolder, string operationKind)
    {
        var operation = _operationCoordinator.TryAcquire(projectFolder, operationKind);
        if (!operation.Acquired) throw new InvalidOperationException(operation.Diagnostic);
        return operation;
    }

    private static GameProjectGeneratedWorldRollbackPreview FailedPreview(
        string targetWorldId,
        string diagnostic,
        string attemptId = "") => new()
    {
        AttemptId = attemptId,
        TargetWorldId = targetWorldId,
        Stage = "precondition",
        Diagnostics = [diagnostic]
    };

    private static GameProjectGeneratedWorldRollbackPreview CandidateFailure(
        string targetWorldId,
        string attemptId,
        string candidate,
        string diagnostic,
        GameProjectBuildResult? build = null)
    {
        try { if (Directory.Exists(candidate)) Directory.Delete(candidate, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return new GameProjectGeneratedWorldRollbackPreview
        {
            AttemptId = attemptId,
            TargetWorldId = targetWorldId,
            Stage = "candidate_qualification",
            CandidateBuild = build,
            Diagnostics = [diagnostic]
        };
    }

    private static GameProjectGeneratedWorldRollbackResult FailedResult(
        GameProjectGeneratedWorldRollbackPreview preview,
        string diagnostic) => new()
    {
        AttemptId = preview.AttemptId,
        TargetWorldId = preview.TargetWorldId,
        CandidateSealSha256 = preview.CandidateSealSha256,
        Stage = "apply_precondition",
        Diff = preview.Diff,
        CandidateBuild = preview.CandidateBuild,
        Diagnostics = [diagnostic]
    };

    private static GameProjectGeneratedWorldRollbackResult Success(
        SealedGeneratedWorldRollbackCandidate cached,
        GameProjectSeedRegenerationTransactionResult applied,
        UnifiedGameProjectWorkspaceSnapshot? snapshot,
        IReadOnlyList<string> diagnostics) => new()
    {
        AttemptId = cached.PublicPreview.AttemptId,
        Status = "GREEN",
        Stage = "committed",
        TargetWorldId = cached.PublicPreview.TargetWorldId,
        CandidateSealSha256 = cached.Seal.SealSha256,
        Diff = cached.Diff,
        CandidateBuild = cached.CandidateBuild,
        AuthoritativeSnapshot = snapshot,
        Diagnostics = diagnostics,
        Applied = true,
        JournalStatus = applied.JournalStatus,
        TransactionState = applied.TransactionState,
        BuildHistoryFileName = applied.BuildHistoryFileName
    };

    private sealed record CurrentCapture(
        string ProjectFolder,
        SeededGeneratedProjectSourceValidationResult Source,
        GameProjectIdentityDocument Identity,
        GameProjectSeedRegenerationTruthTokens Tokens);
}
