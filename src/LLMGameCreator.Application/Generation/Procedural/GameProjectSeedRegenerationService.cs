using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GameProjectSeedRegenerationService
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
    private readonly SeededGeneratedProjectArtifactFactory _artifactFactory;
    private readonly SeededGeneratedProjectSourceService _sourceService;
    private readonly GameProjectSeedRegenerationDiffService _diffService;
    private readonly GameProjectSeedRegenerationTransaction _transaction;
    private readonly GameProjectSeedRegenerationRecordService _recordService;
    private readonly IGameProjectOperationCoordinator _operationCoordinator;
    private readonly GameProjectSeedRegenerationCandidateSealService _sealService;
    private readonly IGameProjectSeedRegenerationTruthReader _truthReader;
    private readonly IGameProjectSeedRegenerationCommitValidator _commitValidator;
    private readonly GeneratedWorldHistoryService _worldHistoryService;
    private readonly GameProjectGeneratedWorldChangeRecordService _worldChangeRecordService;
    private readonly Dictionary<string, SealedRegenerationCandidate> _previews = new(StringComparer.Ordinal);
    private int _running;

    public GameProjectSeedRegenerationService(
        string repositoryRoot,
        ICurrentGamePackageService currentPackageService,
        IGamePackageRepository packageRepository,
        IGamePackageValidator packageValidator,
        GameProjectBuildAndQualificationService builder,
        SeededGeneratedProjectArtifactFactory artifactFactory,
        SeededGeneratedProjectSourceService sourceService,
        GameProjectSeedRegenerationDiffService? diffService = null,
        GameProjectSeedRegenerationTransaction? transaction = null,
        GameProjectSeedRegenerationRecordService? recordService = null,
        IGameProjectOperationCoordinator? operationCoordinator = null,
        GameProjectSeedRegenerationCandidateSealService? sealService = null,
        IGameProjectSeedRegenerationTruthReader? truthReader = null,
        IGameProjectSeedRegenerationCommitValidator? commitValidator = null,
        GeneratedWorldHistoryService? worldHistoryService = null,
        GameProjectGeneratedWorldChangeRecordService? worldChangeRecordService = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _currentPackageService = currentPackageService ?? throw new ArgumentNullException(nameof(currentPackageService));
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        ArgumentNullException.ThrowIfNull(packageValidator);
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _artifactFactory = artifactFactory ?? throw new ArgumentNullException(nameof(artifactFactory));
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _diffService = diffService ?? new GameProjectSeedRegenerationDiffService();
        _transaction = transaction ?? new GameProjectSeedRegenerationTransaction();
        _recordService = recordService ?? new GameProjectSeedRegenerationRecordService(_repositoryRoot, _sourceService);
        _operationCoordinator = operationCoordinator ?? builder.OperationCoordinator;
        _sealService = sealService ?? new GameProjectSeedRegenerationCandidateSealService();
        _truthReader = truthReader ?? new GameProjectSeedRegenerationTruthReader(_repositoryRoot, _sourceService);
        _worldHistoryService = worldHistoryService ?? new GeneratedWorldHistoryService(_sourceService);
        _worldChangeRecordService = worldChangeRecordService
                                    ?? new GameProjectGeneratedWorldChangeRecordService(
                                        _sourceService, _worldHistoryService);
        _commitValidator = commitValidator ?? new GameProjectSeedRegenerationCommitValidator(
            _repositoryRoot, _sourceService, packageValidator, _recordService);
    }

    public bool Running => Volatile.Read(ref _running) != 0;

    public GameProjectSeedRegenerationTransactionResult Recover(string projectFolder)
    {
        using var operation = _operationCoordinator.TryAcquire(projectFolder, GameProjectOperationKinds.Recovery);
        return !operation.Acquired ? new GameProjectSeedRegenerationTransactionResult
        {
            Diagnostics = [operation.Diagnostic]
        } : _transaction.Recover(projectFolder, operation);
    }

    public GameProjectSeedRegenerationTransactionResult Recover(
        string projectFolder,
        GameProjectOperationLease operationLease) =>
        _transaction.Recover(projectFolder, operationLease);

    public GameProjectSeedRegenerationRequest CreateRequest(
        string projectFolder,
        SeededGeneratedProjectGenerationRequest generationRequest)
    {
        using var operation = RequireOperation(projectFolder, GameProjectOperationKinds.Recovery);
        var tokens = Capture(projectFolder, operation).Tokens;
        return new GameProjectSeedRegenerationRequest
        {
            ProjectFolder = Path.GetFullPath(projectFolder),
            GenerationRequest = SeededGeneratedProjectSourceService.NormalizeRequest(generationRequest),
            ExpectedSourceRecordSha256 = tokens.SourceRecordSha256,
            ExpectedQualifiedAuthoringFingerprint = tokens.QualifiedAuthoringFingerprint,
            ExpectedAuthoringRevision = tokens.AuthoringRevision,
            ExpectedActivatedPackageSha256 = tokens.ActivatedPackageSha256,
            ExpectedCompositionPackageSha256 = tokens.CompositionPackageSha256,
            ExpectedFinalStateHash = tokens.FinalStateHash,
            ExpectedProjectIdentityFingerprint = tokens.ProjectIdentityFingerprint,
            ExpectedReleaseCandidateRecordSha256 = tokens.ReleaseCandidateRecordSha256
        };
    }

    public GameProjectSeedRegenerationRecordReadResult ReadLastSuccessful(string projectFolder) =>
        _recordService.Read(projectFolder);

    public GeneratedWorldHistoryReadResult ReadWorldHistory(string projectFolder) =>
        _worldHistoryService.ReadAll(projectFolder);

    public GameProjectGeneratedWorldChangeReadResult ReadLastWorldChange(string projectFolder) =>
        _worldChangeRecordService.Read(projectFolder);

    public GameProjectSeedRegenerationPreview Preview(
        GameProjectSeedRegenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var operation = _operationCoordinator.TryAcquire(
            request.ProjectFolder, GameProjectOperationKinds.RegenerationPreview);
        if (!operation.Acquired) return new GameProjectSeedRegenerationPreview
        {
            Stage = "precondition",
            Diagnostics = [operation.Diagnostic, "regeneration.concurrent_operation"]
        };
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            return FailedPreview("project_operation.busy:" + GameProjectOperationKinds.RegenerationPreview);
        var attemptId = Guid.NewGuid().ToString("N")[..12];
        string? candidate = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var recovery = _transaction.Recover(request.ProjectFolder, operation);
            if (!recovery.Passed) return FailedPreview(
                recovery.Diagnostics.FirstOrDefault() ?? "regeneration.recovery_required", attemptId);
            var current = Capture(request.ProjectFolder, operation);
            var concurrency = CompareTokens(ExpectedTokens(request), current.Tokens);
            if (concurrency.Count > 0) return FailedPreview(concurrency[0], attemptId);
            var normalizedRequest = SeededGeneratedProjectSourceService.NormalizeRequest(request.GenerationRequest);
            var requestedResolved = new LLMGameCreator.Application.RuntimePreview.GenerationPresetOptionsService()
                .Resolve(normalizedRequest);
            if (SemanticEquals(current.Source.ResolvedGenerationOptions!, requestedResolved))
                return FailedPreview("regeneration.no_semantic_change", attemptId);

            candidate = CandidateRoot(attemptId);
            CloneProject(current.ProjectFolder, candidate);
            using var candidateOperation = _operationCoordinator.TryAcquireChild(
                operation, candidate, GameProjectOperationKinds.Build);
            if (!candidateOperation.Acquired)
                return CandidateFailure(attemptId, candidate, candidateOperation.Diagnostic);
            var generationRoot = Confined(candidate, SeededGeneratedProjectVocabulary.GenerationRelativeRoot);
            if (Directory.Exists(generationRoot)) Directory.Delete(generationRoot, recursive: true);
            var artifacts = _artifactFactory.Create(new SeededGeneratedProjectArtifactFactoryRequest
            {
                GenerationRequest = normalizedRequest,
                MechanicsProfileId = current.Source.Source!.MechanicsProfileId,
                OutputDirectory = generationRoot
            });
            if (!artifacts.Passed) return CandidateFailure(attemptId, candidate,
                artifacts.Diagnostics.FirstOrDefault() ?? "regeneration.candidate_generation_failed");
            cancellationToken.ThrowIfCancellationRequested();
            PrepareCandidatePackage(candidate, current.Identity);
            var candidateAuthoring = NewAuthoring();
            var candidatePackage = LoadPackage(candidate);
            var candidateState = candidateAuthoring.OpenProject(candidate, candidatePackage, candidateOperation);
            var preservedSelected = candidateState.Document.SelectedModuleIds.ToList();
            var preservedParameters = ParameterJson(candidateState.Document);
            candidateAuthoring.ApplyQualifiedDocument(candidateState.Document with
            {
                LastMaterializedPackageSha256 = string.Empty,
                LastCompositionPackageSha256 = string.Empty,
                LastActivatedProjectPackageSha256 = string.Empty,
                LastQualifiedFinalStateHash = string.Empty,
                LastQualificationStatus = "NOT_RUN"
            });
            candidateAuthoring.Save(candidateOperation);

            _currentPackageService.LoadAsync(candidate, cancellationToken).GetAwaiter().GetResult();
            GameProjectBuildResult first;
            GameProjectBuildResult second;
            UnifiedGameProjectWorkspaceSnapshot candidateSnapshot;
            GameProjectAuthoringState reopenedState;
            try
            {
                first = _builder.Build(candidateAuthoring, candidateOperation, cancellationToken);
                if (!first.Passed) return CandidateFailure(attemptId, candidate,
                    first.Diagnostics.FirstOrDefault() ?? "regeneration.candidate_build_failed", first);
                second = _builder.Build(candidateAuthoring, candidateOperation, cancellationToken);
                if (!second.Passed) return CandidateFailure(attemptId, candidate,
                    second.Diagnostics.FirstOrDefault() ?? "regeneration.candidate_repeat_failed", second);
                if (!BuildIdentityEquals(first, second)) return CandidateFailure(
                    attemptId, candidate, "regeneration.candidate_repeat_mismatch", second);
                _currentPackageService.LoadAsync(candidate, cancellationToken).GetAwaiter().GetResult();
                var reopenedAuthoring = NewAuthoring();
                var reopenedController = NewController(reopenedAuthoring);
                candidateSnapshot = reopenedController.OpenProject(candidate, candidateOperation);
                reopenedState = reopenedAuthoring.State;
            }
            finally
            {
                _currentPackageService.LoadAsync(current.ProjectFolder, CancellationToken.None).GetAwaiter().GetResult();
            }

            var candidateSource = _sourceService.Validate(candidate);
            var authoringPreserved = reopenedState.Document.SelectedModuleIds
                                         .SequenceEqual(preservedSelected, StringComparer.Ordinal)
                                     && string.Equals(ParameterJson(reopenedState.Document), preservedParameters,
                                         StringComparison.Ordinal)
                                     && string.Equals(
                                         new FeatureModuleAuthoringFingerprintService().Calculate(
                                             reopenedState.Document, reopenedState.Library).Sha256,
                                         current.Tokens.QualifiedAuthoringFingerprint, StringComparison.Ordinal);
            var identityPreserved = string.Equals(IdentityFingerprint(reopenedState.Identity),
                current.Tokens.ProjectIdentityFingerprint, StringComparison.Ordinal);
            var diff = _diffService.Compare(current.Source, candidateSource, authoringPreserved, identityPreserved);
            if (!diff.GameplayChanged || !authoringPreserved || !identityPreserved)
                return CandidateFailure(attemptId, candidate,
                    diff.Diagnostics.FirstOrDefault() ?? "regeneration.candidate_diff_failed", second);
            if (candidateSnapshot.GeneratedWorld?.Status != "TRAVEL_CURRENT"
                || candidateSnapshot.GeneratedWorldActivation is not { Passed: true }
                || candidateSnapshot.GeneratedRegionTravel is not { Passed: true }
                || candidateSnapshot.AcceptedMechanicsCompatibility is not { Passed: true }
                || candidateSnapshot.ReleaseCandidateRecordConfigurationStatus == "CURRENT")
                return CandidateFailure(attemptId, candidate,
                    "regeneration.candidate_qualification_incomplete", second);

            var publicationCapture = Capture(current.ProjectFolder, operation);
            var publicationRace = CompareTokens(ExpectedTokens(request), publicationCapture.Tokens);
            if (publicationRace.Count > 0)
                return CandidateFailure(attemptId, candidate, publicationRace[0], second);
            var inventory = _truthReader.CaptureAuthoritativeInventorySha256(current.ProjectFolder);
            var historyFileName = Path.GetFileName(second.BuildHistoryPath);
            var rootIdentity = Guid.NewGuid().ToString("N");
            var seal = _sealService.Create(candidate, rootIdentity, attemptId, historyFileName,
                second, candidateSnapshot, diff, reopenedState);
            var preview = new GameProjectSeedRegenerationPreview
            {
                AttemptId = attemptId,
                Status = "GREEN",
                Stage = "candidate_sealed",
                CurrentSourceSummary = current.Source.ResolvedGenerationOptions!.StableSummary,
                CandidateSourceSummary = candidateSource.ResolvedGenerationOptions!.StableSummary,
                Diff = diff,
                CandidateBuild = second,
                CandidateSnapshot = candidateSnapshot,
                ExpectedTruthTokens = publicationCapture.Tokens,
                CandidateRoot = candidate,
                CandidateBuildHistoryFileName = historyFileName,
                CandidateSealSha256 = seal.SealSha256,
                TransactionState = "not_started"
            };
            _previews[attemptId] = new SealedRegenerationCandidate
            {
                CandidateRoot = candidate,
                Seal = seal,
                PublicPreview = preview,
                CandidateBuild = second,
                CandidateSnapshot = candidateSnapshot,
                Diff = diff,
                ExpectedTruthTokens = publicationCapture.Tokens,
                ExpectedAuthoritativeInventorySha256 = inventory
            };
            return preview;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidOperationException or JsonException
                                           or OperationCanceledException)
        {
            if (candidate is not null && Directory.Exists(candidate)) Directory.Delete(candidate, recursive: true);
            return FailedPreview(exception.Message, attemptId);
        }
        finally { Volatile.Write(ref _running, 0); }
    }

    public GameProjectSeedRegenerationResult Apply(
        GameProjectSeedRegenerationRequest request,
        GameProjectSeedRegenerationPreview preview,
        GameProjectSeedRegenerationFailurePoint failurePoint = GameProjectSeedRegenerationFailurePoint.None)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(preview);
        using var operation = _operationCoordinator.TryAcquire(
            request.ProjectFolder, GameProjectOperationKinds.RegenerationApply);
        if (!operation.Acquired) return FailedResult(preview, operation.Diagnostic);
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            return FailedResult(preview, "project_operation.busy:" + GameProjectOperationKinds.RegenerationApply);
        SealedRegenerationCandidate? cached = null;
        try
        {
            if (!_previews.TryGetValue(preview.AttemptId, out cached)
                || cached.PublicPreview.Status != "GREEN"
                || !string.Equals(preview.CandidateSealSha256, cached.Seal.SealSha256, StringComparison.Ordinal)
                || !CallerPreviewMatches(preview, cached.PublicPreview)
                || !Directory.Exists(cached.CandidateRoot))
                return FailedResult(cached?.PublicPreview ?? preview,
                    "regeneration.candidate_seal_mismatch");

            using var candidateOperation = _operationCoordinator.TryAcquireChild(
                operation, cached.CandidateRoot, GameProjectOperationKinds.Build);
            if (!candidateOperation.Acquired)
                return FailedResult(cached.PublicPreview, candidateOperation.Diagnostic);
            var candidatePackage = LoadPackage(cached.CandidateRoot);
            var candidateAuthoring = NewAuthoring();
            candidateAuthoring.OpenProject(cached.CandidateRoot, candidatePackage, candidateOperation);
            UnifiedGameProjectWorkspaceSnapshot freshCandidateSnapshot;
            _currentPackageService.LoadAsync(cached.CandidateRoot, CancellationToken.None).GetAwaiter().GetResult();
            try
            {
                freshCandidateSnapshot = NewController(candidateAuthoring)
                    .OpenProject(cached.CandidateRoot, candidateOperation);
            }
            finally
            {
                _currentPackageService.LoadAsync(request.ProjectFolder, CancellationToken.None).GetAwaiter().GetResult();
            }
            var sealValidation = _sealService.Verify(cached.CandidateRoot, cached.Seal,
                cached.CandidateBuild, freshCandidateSnapshot, cached.Diff, candidateAuthoring.State);
            if (!sealValidation.Passed)
                return FailedResult(cached.PublicPreview,
                    sealValidation.Diagnostics.FirstOrDefault() ?? "regeneration.candidate_tampered");
            var candidateSource = _sourceService.Validate(cached.CandidateRoot);
            if (candidateSource is not { Present: true, Passed: true, Source: not null })
                return FailedResult(cached.PublicPreview, "regeneration.candidate_tampered");
            if (freshCandidateSnapshot.GeneratedWorld?.Status != "TRAVEL_CURRENT"
                || freshCandidateSnapshot.GeneratedWorldActivation is not { Passed: true }
                || freshCandidateSnapshot.GeneratedRegionTravel is not { Passed: true }
                || freshCandidateSnapshot.AcceptedMechanicsCompatibility is not { Passed: true }
                || freshCandidateSnapshot.ReleaseCandidateRecordConfigurationStatus == "CURRENT")
                return FailedResult(cached.PublicPreview, "regeneration.candidate_tampered");

            var current = Capture(request.ProjectFolder, operation);
            var concurrency = CompareTokens(cached.ExpectedTruthTokens, current.Tokens);
            if (concurrency.Count > 0) return FailedResult(cached.PublicPreview, concurrency[0]);
            if (!string.Equals(_truthReader.CaptureAuthoritativeInventorySha256(current.ProjectFolder),
                    cached.ExpectedAuthoritativeInventorySha256, StringComparison.Ordinal))
                return FailedResult(cached.PublicPreview, "regeneration.authoritative_inventory_changed");
            var rcPath = new GameProjectReleaseCandidateRecordService().RecordPath(current.ProjectFolder);
            var previousRcHash = File.Exists(rcPath) ? HashFile(rcPath) : null;
            var record = BuildRegenerationRecord(cached, current, previousRcHash);
            var fromWorldId = _worldHistoryService.WorldId(current.ProjectFolder, current.Source);
            var toWorldId = _worldHistoryService.WorldId(cached.CandidateRoot, candidateSource);
            var worldChange = new GameProjectGeneratedWorldChangeRecord
            {
                OperationKind = "regeneration",
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
            var applied = _transaction.Apply(new GameProjectSeedRegenerationTransactionRequest
            {
                AttemptId = cached.PublicPreview.AttemptId,
                ProjectFolder = current.ProjectFolder,
                CandidateFolder = cached.CandidateRoot,
                CandidateBuildHistoryFileName = cached.Seal.SelectedBuildHistoryFileName,
                RegenerationRecordJson = _recordService.Serialize(record),
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
                    OperationKind = "regeneration",
                    CandidateSeal = cached.Seal,
                    ExpectedProjectIdentityFingerprint = current.Tokens.ProjectIdentityFingerprint,
                    SelectedBuildHistoryFileName = cached.Seal.SelectedBuildHistoryFileName,
                    PreviousReleaseCandidateRecordSha256 = previousRcHash
                },
                WorldHistoryService = _worldHistoryService,
                BeforeWorldHistoryOperationKind = GeneratedWorldHistoryOperationKinds.RegenerationBefore,
                AfterWorldHistoryOperationKind = GeneratedWorldHistoryOperationKinds.RegenerationAfter,
                WorldChangeRecordRelativePath = GameProjectGeneratedWorldChangeVocabulary.RelativePath,
                WorldChangeRecordJson = _worldChangeRecordService.Serialize(worldChange)
            });
            if (!applied.Passed) return new GameProjectSeedRegenerationResult
            {
                AttemptId = cached.PublicPreview.AttemptId,
                Status = "FAILED",
                Stage = "atomic_apply",
                Diff = cached.Diff,
                CandidateBuild = cached.CandidateBuild,
                Diagnostics = applied.Diagnostics,
                RollbackApplied = applied.RollbackApplied,
                AuthoritativeFilesChanged = applied.ChangedRelativePaths,
                JournalStatus = applied.JournalStatus,
                TransactionState = applied.TransactionState,
                CandidateSealSha256 = cached.Seal.SealSha256
            };

            candidateOperation.Dispose();
            if (Directory.Exists(cached.CandidateRoot)) Directory.Delete(cached.CandidateRoot, recursive: true);
            _previews.Remove(cached.PublicPreview.AttemptId);
            operation.Dispose();
            try
            {
                _currentPackageService.LoadAsync(current.ProjectFolder, CancellationToken.None).GetAwaiter().GetResult();
                var authoring = NewAuthoring();
                var authoritative = NewController(authoring).OpenProject(current.ProjectFolder);
                return Success(cached, applied, authoritative, []);
            }
            catch (Exception presentationException) when (presentationException is IOException
                                                           or UnauthorizedAccessException
                                                           or InvalidOperationException
                                                           or JsonException)
            {
                return Success(cached, applied, null,
                    ["regeneration.presentation_reopen_failed:" + presentationException.Message]) with
                {
                    CommittedWithPresentationDiagnostic = true
                };
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidOperationException or JsonException)
        {
            return FailedResult(cached?.PublicPreview ?? preview, exception.Message);
        }
        finally { Volatile.Write(ref _running, 0); }
    }

    public GameProjectSeedRegenerationTruthTokens CaptureTruthTokens(string projectFolder)
    {
        using var operation = RequireOperation(projectFolder, GameProjectOperationKinds.Recovery);
        return Capture(projectFolder, operation).Tokens;
    }

    private CurrentCapture Capture(string projectFolder, GameProjectOperationLease operationLease)
    {
        var project = Path.GetFullPath(projectFolder);
        var source = _sourceService.Validate(project);
        if (source is not { Present: true, Passed: true, Source: not null, ResolvedGenerationOptions: not null })
            throw new InvalidOperationException(source.Present
                ? "regeneration.generated_source_invalid"
                : "regeneration.not_generated_project");
        var package = LoadPackage(project);
        var authoring = NewAuthoring();
        var state = authoring.OpenProject(project, package, operationLease);
        var fingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(state.Document, state.Library);
        if (!fingerprint.Passed) throw new InvalidOperationException("regeneration.authoring_invalid");
        var packageHash = HashFile(Confined(project, "package.json"));
        if (!string.IsNullOrWhiteSpace(state.Document.LastActivatedProjectPackageSha256)
            && !string.Equals(packageHash, state.Document.LastActivatedProjectPackageSha256, StringComparison.Ordinal))
            throw new InvalidOperationException("regeneration.package_changed");
        var identityFingerprint = IdentityFingerprint(state.Identity);
        var rcPath = new GameProjectReleaseCandidateRecordService().RecordPath(project);
        var rcHash = File.Exists(rcPath) ? HashFile(rcPath) : null;
        return new CurrentCapture(project, source, state.Identity, new GameProjectSeedRegenerationTruthTokens
        {
            SourceRecordSha256 = HashFile(Confined(project, SeededGeneratedProjectVocabulary.SourceRelativePath)),
            QualifiedAuthoringFingerprint = fingerprint.Sha256,
            AuthoringRevision = state.Document.Revision,
            ActivatedPackageSha256 = packageHash,
            CompositionPackageSha256 = state.Document.LastCompositionPackageSha256,
            FinalStateHash = state.Document.LastQualifiedFinalStateHash,
            ProjectIdentityFingerprint = identityFingerprint,
            ReleaseCandidateRecordSha256 = rcHash
        });
    }

    private static GameProjectSeedRegenerationTruthTokens ExpectedTokens(GameProjectSeedRegenerationRequest request) => new()
    {
        SourceRecordSha256 = request.ExpectedSourceRecordSha256,
        QualifiedAuthoringFingerprint = request.ExpectedQualifiedAuthoringFingerprint,
        AuthoringRevision = request.ExpectedAuthoringRevision,
        ActivatedPackageSha256 = request.ExpectedActivatedPackageSha256,
        CompositionPackageSha256 = request.ExpectedCompositionPackageSha256,
        FinalStateHash = request.ExpectedFinalStateHash,
        ProjectIdentityFingerprint = request.ExpectedProjectIdentityFingerprint,
        ReleaseCandidateRecordSha256 = request.ExpectedReleaseCandidateRecordSha256
    };

    private static IReadOnlyList<string> CompareTokens(
        GameProjectSeedRegenerationTruthTokens expected,
        GameProjectSeedRegenerationTruthTokens actual)
    {
        var diagnostics = new List<string>();
        if (!string.Equals(expected.SourceRecordSha256, actual.SourceRecordSha256, StringComparison.Ordinal))
            diagnostics.Add("regeneration.source_changed");
        if (!string.Equals(expected.QualifiedAuthoringFingerprint, actual.QualifiedAuthoringFingerprint,
                StringComparison.Ordinal) || expected.AuthoringRevision != actual.AuthoringRevision)
            diagnostics.Add("regeneration.authoring_changed");
        if (!string.Equals(expected.ActivatedPackageSha256, actual.ActivatedPackageSha256, StringComparison.Ordinal)
            || !string.Equals(expected.CompositionPackageSha256, actual.CompositionPackageSha256, StringComparison.Ordinal)
            || !string.Equals(expected.FinalStateHash, actual.FinalStateHash, StringComparison.Ordinal))
            diagnostics.Add("regeneration.package_changed");
        if (!string.Equals(expected.ProjectIdentityFingerprint, actual.ProjectIdentityFingerprint,
                StringComparison.Ordinal)) diagnostics.Add("regeneration.identity_changed");
        if (!string.Equals(expected.ReleaseCandidateRecordSha256, actual.ReleaseCandidateRecordSha256,
                StringComparison.Ordinal)) diagnostics.Add("regeneration.release_candidate_changed");
        return diagnostics;
    }

    private static bool SemanticEquals(
        SeededGeneratedProjectResolvedOptions left,
        SeededGeneratedProjectResolvedOptions right) =>
        string.Equals(left.Seed, right.Seed, StringComparison.Ordinal)
        && string.Equals(left.Mode, right.Mode, StringComparison.Ordinal)
        && string.Equals(left.PresetId, right.PresetId, StringComparison.Ordinal)
        && left.CompactStyleHintIds.SequenceEqual(right.CompactStyleHintIds, StringComparer.Ordinal)
        && left.SelectedVariantIds.SequenceEqual(right.SelectedVariantIds, StringComparer.Ordinal);

    private void PrepareCandidatePackage(string candidate, GameProjectIdentityDocument identity)
    {
        var generatedBase = Confined(candidate, SeededGeneratedProjectVocabulary.GenerationRelativeRoot + "/"
                                              + SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName);
        var staged = Confined(candidate, ".llmgc/regeneration-candidate-package/package.json");
        Directory.CreateDirectory(Path.GetDirectoryName(staged)!);
        new GameProjectPackageIdentityOverlayService().Overlay(generatedBase, staged, identity);
        File.Move(staged, Confined(candidate, "package.json"), overwrite: true);
        var stagingRoot = Path.GetDirectoryName(staged)!;
        if (Directory.Exists(stagingRoot)) Directory.Delete(stagingRoot, recursive: true);
    }

    private GameProjectFeatureModuleAuthoringService NewAuthoring() =>
        new(_repositoryRoot, operationCoordinator: _operationCoordinator);

    private UnifiedGameProjectWorkspaceController NewController(GameProjectFeatureModuleAuthoringService authoring) => new(
        _currentPackageService,
        authoring,
        _builder,
        generatedSourceService: _sourceService,
        generatedWorldSummaryService: new GameProjectGeneratedWorldSummaryService(),
        operationCoordinator: _operationCoordinator);

    private GamePackageDefinition LoadPackage(string projectFolder) =>
        _packageRepository.LoadAsync(projectFolder, CancellationToken.None).GetAwaiter().GetResult();

    private static bool BuildIdentityEquals(GameProjectBuildResult left, GameProjectBuildResult right) =>
        string.Equals(left.PackageSha256, right.PackageSha256, StringComparison.Ordinal)
        && string.Equals(left.CompositionPackageSha256, right.CompositionPackageSha256, StringComparison.Ordinal)
        && string.Equals(left.FinalStateHash, right.FinalStateHash, StringComparison.Ordinal)
        && left.GeneratedWorld?.SourceRequestSha256 == right.GeneratedWorld?.SourceRequestSha256
        && left.GeneratedRegionTravel?.FinalStateHash == right.GeneratedRegionTravel?.FinalStateHash;

    internal static string IdentityFingerprint(GameProjectIdentityDocument identity)
    {
        var stable = string.Join("\n", new[]
        {
            identity.PackageId, identity.Title, identity.Version, identity.FormatVersion,
            identity.Description, identity.Source
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(stable))).ToLowerInvariant();
    }

    private static string ParameterJson(FeatureModuleCompositionDocument document) => JsonSerializer.Serialize(
        document.ParameterValues.OrderBy(value => value.ModuleId, StringComparer.Ordinal)
            .ThenBy(value => value.ParameterId, StringComparer.Ordinal), JsonOptions);

    private static string CandidateRoot(string attemptId)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "RegenerationCandidates", attemptId);
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        return root;
    }

    internal static void CloneProject(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            var relative = Relative(source, directory);
            if (Excluded(relative)) continue;
            Directory.CreateDirectory(Confined(target, relative));
        }
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Relative(source, file);
            if (Excluded(relative) || relative.EndsWith(".lock", StringComparison.OrdinalIgnoreCase)
                                   || relative.Contains(".tmp-", StringComparison.OrdinalIgnoreCase)) continue;
            var destination = Confined(target, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: false);
        }
    }

    private static bool Excluded(string relative) => relative.Equals("Builds", StringComparison.OrdinalIgnoreCase)
        || relative.StartsWith("Builds/", StringComparison.OrdinalIgnoreCase)
        || relative.Equals(UnifiedGameProjectWorkspaceVocabulary.BuildStagingRelativeRoot,
            StringComparison.OrdinalIgnoreCase)
        || relative.StartsWith(UnifiedGameProjectWorkspaceVocabulary.BuildStagingRelativeRoot + "/",
            StringComparison.OrdinalIgnoreCase)
        || relative.Equals(GameProjectSeedRegenerationVocabulary.RegenerationRelativeRoot,
            StringComparison.OrdinalIgnoreCase)
        || relative.StartsWith(GameProjectSeedRegenerationVocabulary.RegenerationRelativeRoot + "/",
            StringComparison.OrdinalIgnoreCase)
        || relative.Equals(".llmgc/operations", StringComparison.OrdinalIgnoreCase)
        || relative.StartsWith(".llmgc/operations/", StringComparison.OrdinalIgnoreCase);

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    internal static string Confined(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.Equals(fullRoot, comparison) && !path.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("regeneration.path_escape");
        return path;
    }

    private static bool CallerPreviewMatches(
        GameProjectSeedRegenerationPreview caller,
        GameProjectSeedRegenerationPreview cached) =>
        string.Equals(caller.AttemptId, cached.AttemptId, StringComparison.Ordinal)
        && string.Equals(caller.CandidateSealSha256, cached.CandidateSealSha256, StringComparison.Ordinal)
        && string.Equals(caller.CandidateRoot, cached.CandidateRoot, StringComparison.Ordinal)
        && string.Equals(caller.CandidateBuildHistoryFileName, cached.CandidateBuildHistoryFileName,
            StringComparison.Ordinal)
        && string.Equals(caller.CandidateBuild?.PackageSha256, cached.CandidateBuild?.PackageSha256,
            StringComparison.Ordinal)
        && string.Equals(caller.CandidateBuild?.FinalStateHash, cached.CandidateBuild?.FinalStateHash,
            StringComparison.Ordinal)
        && string.Equals(JsonSerializer.Serialize(caller.Diff, JsonOptions),
            JsonSerializer.Serialize(cached.Diff, JsonOptions), StringComparison.Ordinal);

    private GameProjectSeedRegenerationRecord BuildRegenerationRecord(
        SealedRegenerationCandidate cached,
        CurrentCapture current,
        string? previousRcHash) => new()
    {
        AttemptId = cached.PublicPreview.AttemptId,
        OldSourceRecordSha256 = current.Tokens.SourceRecordSha256,
        NewSourceRecordSha256 = cached.Seal.SourceRecordSha256,
        OldRequestSha256 = cached.Diff.OldSourceRequestSha256,
        NewRequestSha256 = cached.Diff.NewSourceRequestSha256,
        OldPlanSha256 = cached.Diff.OldPlanSha256,
        NewPlanSha256 = cached.Diff.NewPlanSha256,
        OldOverlaySha256 = cached.Diff.OldOverlaySha256,
        NewOverlaySha256 = cached.Diff.NewOverlaySha256,
        OldGeneratedBaseSha256 = cached.Diff.OldGeneratedBaseSha256,
        NewGeneratedBaseSha256 = cached.Diff.NewGeneratedBaseSha256,
        OldPackageSha256 = current.Tokens.ActivatedPackageSha256,
        NewPackageSha256 = cached.CandidateBuild.PackageSha256,
        NewCompositionPackageSha256 = cached.CandidateBuild.CompositionPackageSha256,
        NewFinalStateHash = cached.CandidateBuild.FinalStateHash,
        QualifiedAuthoringFingerprint = cached.CandidateBuild.QualifiedAuthoringFingerprint,
        SelectedModuleCount = cached.CandidateBuild.AttemptedSelectedModuleIds.Count,
        ConfiguredParameterCount = cached.CandidateBuild.ConfiguredParameterCount,
        Diff = cached.Diff,
        CandidateBuildHistoryFileName = cached.Seal.SelectedBuildHistoryFileName,
        PreviousReleaseCandidateRecordSha256 = previousRcHash,
        PreviousReleaseCandidateStatus = previousRcHash is null ? "ABSENT" : "LAST_SUCCESS"
    };

    private static GameProjectSeedRegenerationResult Success(
        SealedRegenerationCandidate cached,
        GameProjectSeedRegenerationTransactionResult applied,
        UnifiedGameProjectWorkspaceSnapshot? snapshot,
        IReadOnlyList<string> diagnostics) => new()
    {
        AttemptId = cached.PublicPreview.AttemptId,
        Status = "GREEN",
        Stage = "committed",
        Diff = cached.Diff,
        CandidateBuild = cached.CandidateBuild,
        AuthoritativeSnapshot = snapshot,
        Diagnostics = diagnostics,
        Applied = true,
        AuthoritativeFilesChanged = applied.ChangedRelativePaths,
        JournalStatus = applied.JournalStatus,
        TransactionState = applied.TransactionState,
        BuildHistoryFileName = applied.BuildHistoryFileName,
        CandidateSealSha256 = cached.Seal.SealSha256
    };

    private static GameProjectSeedRegenerationPreview CandidateFailure(
        string attemptId,
        string candidate,
        string diagnostic,
        GameProjectBuildResult? build = null)
    {
        try { if (Directory.Exists(candidate)) Directory.Delete(candidate, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return new GameProjectSeedRegenerationPreview
        {
            AttemptId = attemptId,
            Stage = "candidate_qualification",
            Diagnostics = [diagnostic],
            CandidateBuild = build
        };
    }

    private static GameProjectSeedRegenerationPreview FailedPreview(string diagnostic, string attemptId = "") => new()
    {
        AttemptId = attemptId,
        Stage = "precondition",
        Diagnostics = [diagnostic]
    };

    private static GameProjectSeedRegenerationResult FailedResult(
        GameProjectSeedRegenerationPreview preview,
        string diagnostic) => new()
    {
        AttemptId = preview.AttemptId,
        Stage = "apply_precondition",
        Diff = preview.Diff,
        CandidateBuild = preview.CandidateBuild,
        CandidateSealSha256 = preview.CandidateSealSha256,
        Diagnostics = [diagnostic]
    };

    private GameProjectOperationLease RequireOperation(string projectFolder, string operationKind)
    {
        var operation = _operationCoordinator.TryAcquire(projectFolder, operationKind);
        if (!operation.Acquired) throw new InvalidOperationException(operation.Diagnostic);
        return operation;
    }

    private static string HashFile(string path) => GameProjectSeedRegenerationRecordService.HashFile(path);

    private sealed record CurrentCapture(
        string ProjectFolder,
        SeededGeneratedProjectSourceValidationResult Source,
        GameProjectIdentityDocument Identity,
        GameProjectSeedRegenerationTruthTokens Tokens);
}
