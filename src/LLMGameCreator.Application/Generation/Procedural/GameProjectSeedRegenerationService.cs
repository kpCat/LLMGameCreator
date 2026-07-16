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
    private readonly IGamePackageValidator _packageValidator;
    private readonly GameProjectBuildAndQualificationService _builder;
    private readonly SeededGeneratedProjectArtifactFactory _artifactFactory;
    private readonly SeededGeneratedProjectSourceService _sourceService;
    private readonly GameProjectSeedRegenerationDiffService _diffService;
    private readonly GameProjectSeedRegenerationTransaction _transaction;
    private readonly GameProjectSeedRegenerationRecordService _recordService;
    private readonly Dictionary<string, GameProjectSeedRegenerationPreview> _previews = new(StringComparer.Ordinal);
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
        GameProjectSeedRegenerationRecordService? recordService = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _currentPackageService = currentPackageService ?? throw new ArgumentNullException(nameof(currentPackageService));
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _packageValidator = packageValidator ?? throw new ArgumentNullException(nameof(packageValidator));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _artifactFactory = artifactFactory ?? throw new ArgumentNullException(nameof(artifactFactory));
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _diffService = diffService ?? new GameProjectSeedRegenerationDiffService();
        _transaction = transaction ?? new GameProjectSeedRegenerationTransaction();
        _recordService = recordService ?? new GameProjectSeedRegenerationRecordService(_repositoryRoot, _sourceService);
    }

    public bool Running => Volatile.Read(ref _running) != 0;

    public GameProjectSeedRegenerationTransactionResult Recover(string projectFolder) =>
        _transaction.Recover(projectFolder);

    public GameProjectSeedRegenerationRequest CreateRequest(
        string projectFolder,
        SeededGeneratedProjectGenerationRequest generationRequest)
    {
        var tokens = Capture(projectFolder).Tokens;
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

    public GameProjectSeedRegenerationPreview Preview(
        GameProjectSeedRegenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0 || _builder.BuildRunning)
            return FailedPreview("regeneration.concurrent_operation");
        var attemptId = Guid.NewGuid().ToString("N")[..12];
        string? candidate = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var recovery = _transaction.Recover(request.ProjectFolder);
            if (!recovery.Passed) return FailedPreview(
                recovery.Diagnostics.FirstOrDefault() ?? "regeneration.recovery_required", attemptId);
            var current = Capture(request.ProjectFolder);
            var concurrency = CompareTokens(request, current.Tokens);
            if (concurrency.Count > 0) return FailedPreview(concurrency[0], attemptId);
            var normalizedRequest = SeededGeneratedProjectSourceService.NormalizeRequest(request.GenerationRequest);
            var requestedResolved = new LLMGameCreator.Application.RuntimePreview.GenerationPresetOptionsService()
                .Resolve(normalizedRequest);
            if (SemanticEquals(current.Source.ResolvedGenerationOptions!, requestedResolved))
                return FailedPreview("regeneration.no_semantic_change", attemptId);

            candidate = CandidateRoot(attemptId);
            CloneProject(current.ProjectFolder, candidate);
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
            var candidateAuthoring = new GameProjectFeatureModuleAuthoringService(_repositoryRoot);
            var candidatePackage = LoadPackage(candidate);
            var candidateState = candidateAuthoring.OpenProject(candidate, candidatePackage);
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
            candidateAuthoring.Save();

            _currentPackageService.LoadAsync(candidate, cancellationToken).GetAwaiter().GetResult();
            GameProjectBuildResult first;
            GameProjectBuildResult second;
            UnifiedGameProjectWorkspaceSnapshot candidateSnapshot;
            try
            {
                first = _builder.Build(candidateAuthoring, cancellationToken);
                if (!first.Passed) return CandidateFailure(attemptId, candidate,
                    first.Diagnostics.FirstOrDefault() ?? "regeneration.candidate_build_failed", first);
                second = _builder.Build(candidateAuthoring, cancellationToken);
                if (!second.Passed) return CandidateFailure(attemptId, candidate,
                    second.Diagnostics.FirstOrDefault() ?? "regeneration.candidate_repeat_failed", second);
                if (!BuildIdentityEquals(first, second)) return CandidateFailure(
                    attemptId, candidate, "regeneration.candidate_repeat_mismatch", second);
                _currentPackageService.LoadAsync(candidate, cancellationToken).GetAwaiter().GetResult();
                var reopenedAuthoring = new GameProjectFeatureModuleAuthoringService(_repositoryRoot);
                var reopenedController = NewController(reopenedAuthoring);
                candidateSnapshot = reopenedController.OpenProject(candidate);
            }
            finally
            {
                _currentPackageService.LoadAsync(current.ProjectFolder, CancellationToken.None).GetAwaiter().GetResult();
            }

            var candidateSource = _sourceService.Validate(candidate);
            var finalCandidateState = candidateAuthoring.State;
            var authoringPreserved = finalCandidateState.Document.SelectedModuleIds
                                         .SequenceEqual(preservedSelected, StringComparer.Ordinal)
                                     && string.Equals(ParameterJson(finalCandidateState.Document), preservedParameters,
                                         StringComparison.Ordinal)
                                     && string.Equals(
                                         new FeatureModuleAuthoringFingerprintService().Calculate(
                                             finalCandidateState.Document, finalCandidateState.Library).Sha256,
                                         current.Tokens.QualifiedAuthoringFingerprint, StringComparison.Ordinal);
            var identityPreserved = string.Equals(IdentityFingerprint(finalCandidateState.Identity),
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
                return CandidateFailure(attemptId, candidate, "regeneration.candidate_qualification_incomplete", second);
            var historyFileName = Path.GetFileName(second.BuildHistoryPath);
            var preview = new GameProjectSeedRegenerationPreview
            {
                AttemptId = attemptId,
                Status = "GREEN",
                Stage = "candidate_qualified",
                CurrentSourceSummary = current.Source.ResolvedGenerationOptions!.StableSummary,
                CandidateSourceSummary = candidateSource.ResolvedGenerationOptions!.StableSummary,
                Diff = diff,
                CandidateBuild = second,
                CandidateSnapshot = candidateSnapshot,
                ExpectedTruthTokens = current.Tokens,
                CandidateRoot = candidate,
                CandidateBuildHistoryFileName = historyFileName
            };
            _previews[attemptId] = preview;
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
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0 || _builder.BuildRunning)
            return FailedResult(preview, "regeneration.concurrent_operation");
        try
        {
            if (!_previews.TryGetValue(preview.AttemptId, out var cached)
                || cached.Status != "GREEN"
                || !string.Equals(cached.CandidateRoot, preview.CandidateRoot, StringComparison.Ordinal)
                || !Directory.Exists(cached.CandidateRoot))
                return FailedResult(preview, "regeneration.preview_unavailable");
            var current = Capture(request.ProjectFolder);
            var concurrency = CompareTokens(request, current.Tokens);
            if (concurrency.Count > 0) return FailedResult(preview, concurrency[0]);
            var candidateSource = _sourceService.Validate(preview.CandidateRoot);
            if (candidateSource is not { Present: true, Passed: true, Source: not null })
                return FailedResult(preview, "regeneration.candidate_source_invalid");
            var rcPath = new GameProjectReleaseCandidateRecordService().RecordPath(current.ProjectFolder);
            var previousRcHash = File.Exists(rcPath) ? GameProjectSeedRegenerationRecordService.HashFile(rcPath) : null;
            var record = new GameProjectSeedRegenerationRecord
            {
                AttemptId = preview.AttemptId,
                OldSourceRecordSha256 = current.Tokens.SourceRecordSha256,
                NewSourceRecordSha256 = GameProjectSeedRegenerationRecordService.HashFile(Confined(
                    preview.CandidateRoot, SeededGeneratedProjectVocabulary.SourceRelativePath)),
                OldRequestSha256 = preview.Diff!.OldSourceRequestSha256,
                NewRequestSha256 = preview.Diff.NewSourceRequestSha256,
                OldPlanSha256 = preview.Diff.OldPlanSha256,
                NewPlanSha256 = preview.Diff.NewPlanSha256,
                OldOverlaySha256 = preview.Diff.OldOverlaySha256,
                NewOverlaySha256 = preview.Diff.NewOverlaySha256,
                OldGeneratedBaseSha256 = preview.Diff.OldGeneratedBaseSha256,
                NewGeneratedBaseSha256 = preview.Diff.NewGeneratedBaseSha256,
                OldPackageSha256 = current.Tokens.ActivatedPackageSha256,
                NewPackageSha256 = preview.CandidateBuild!.PackageSha256,
                NewCompositionPackageSha256 = preview.CandidateBuild.CompositionPackageSha256,
                NewFinalStateHash = preview.CandidateBuild.FinalStateHash,
                QualifiedAuthoringFingerprint = preview.CandidateBuild.QualifiedAuthoringFingerprint,
                SelectedModuleCount = preview.CandidateBuild.AttemptedSelectedModuleIds.Count,
                ConfiguredParameterCount = preview.CandidateBuild.ConfiguredParameterCount,
                Diff = preview.Diff,
                CandidateBuildHistoryFileName = preview.CandidateBuildHistoryFileName,
                PreviousReleaseCandidateRecordSha256 = previousRcHash,
                PreviousReleaseCandidateStatus = previousRcHash is null ? "ABSENT" : "LAST_SUCCESS"
            };
            var applied = _transaction.Apply(new GameProjectSeedRegenerationTransactionRequest
            {
                AttemptId = preview.AttemptId,
                ProjectFolder = current.ProjectFolder,
                CandidateFolder = preview.CandidateRoot,
                CandidateBuildHistoryFileName = preview.CandidateBuildHistoryFileName,
                RegenerationRecordJson = _recordService.Serialize(record),
                FailurePoint = failurePoint
            });
            if (!applied.Passed) return new GameProjectSeedRegenerationResult
            {
                AttemptId = preview.AttemptId,
                Status = "FAILED",
                Stage = "atomic_apply",
                Diff = preview.Diff,
                CandidateBuild = preview.CandidateBuild,
                Diagnostics = applied.Diagnostics,
                RollbackApplied = applied.RollbackApplied,
                AuthoritativeFilesChanged = applied.ChangedRelativePaths,
                JournalStatus = applied.JournalStatus
            };
            _currentPackageService.LoadAsync(current.ProjectFolder, CancellationToken.None).GetAwaiter().GetResult();
            var authoring = new GameProjectFeatureModuleAuthoringService(_repositoryRoot);
            var controller = NewController(authoring);
            var authoritative = controller.OpenProject(current.ProjectFolder);
            var recordRead = _recordService.Read(current.ProjectFolder);
            if (!recordRead.Passed
                || authoritative.GeneratedWorld?.Status != "TRAVEL_CURRENT"
                || authoritative.ReleaseCandidateConfigurationStatus != "BUILD_GREEN_STANDALONE_PENDING")
                throw new InvalidOperationException("regeneration.authoritative_validation_failed:"
                                                    + string.Join(";", recordRead.Diagnostics));
            if (Directory.Exists(preview.CandidateRoot)) Directory.Delete(preview.CandidateRoot, recursive: true);
            _previews.Remove(preview.AttemptId);
            return new GameProjectSeedRegenerationResult
            {
                AttemptId = preview.AttemptId,
                Status = "GREEN",
                Stage = "committed",
                Diff = preview.Diff,
                CandidateBuild = preview.CandidateBuild,
                AuthoritativeSnapshot = authoritative,
                Applied = true,
                AuthoritativeFilesChanged = applied.ChangedRelativePaths,
                JournalStatus = applied.JournalStatus,
                BuildHistoryFileName = applied.BuildHistoryFileName
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidOperationException or JsonException)
        {
            return FailedResult(preview, exception.Message);
        }
        finally { Volatile.Write(ref _running, 0); }
    }

    public GameProjectSeedRegenerationTruthTokens CaptureTruthTokens(string projectFolder) =>
        Capture(projectFolder).Tokens;

    private CurrentCapture Capture(string projectFolder)
    {
        var project = Path.GetFullPath(projectFolder);
        var source = _sourceService.Validate(project);
        if (source is not { Present: true, Passed: true, Source: not null, ResolvedGenerationOptions: not null })
            throw new InvalidOperationException(source.Present
                ? "regeneration.generated_source_invalid"
                : "regeneration.not_generated_project");
        var package = LoadPackage(project);
        var authoring = new GameProjectFeatureModuleAuthoringService(_repositoryRoot);
        var state = authoring.OpenProject(project, package);
        var fingerprint = new FeatureModuleAuthoringFingerprintService().Calculate(state.Document, state.Library);
        if (!fingerprint.Passed) throw new InvalidOperationException("regeneration.authoring_invalid");
        var packageHash = GameProjectSeedRegenerationRecordService.HashFile(Confined(project, "package.json"));
        if (!string.IsNullOrWhiteSpace(state.Document.LastActivatedProjectPackageSha256)
            && !string.Equals(packageHash, state.Document.LastActivatedProjectPackageSha256, StringComparison.Ordinal))
            throw new InvalidOperationException("regeneration.package_changed");
        var identityFingerprint = IdentityFingerprint(state.Identity);
        var rcPath = new GameProjectReleaseCandidateRecordService().RecordPath(project);
        var rcHash = File.Exists(rcPath) ? GameProjectSeedRegenerationRecordService.HashFile(rcPath) : null;
        return new CurrentCapture(project, source, state.Identity, new GameProjectSeedRegenerationTruthTokens
        {
            SourceRecordSha256 = GameProjectSeedRegenerationRecordService.HashFile(Confined(
                project, SeededGeneratedProjectVocabulary.SourceRelativePath)),
            QualifiedAuthoringFingerprint = fingerprint.Sha256,
            AuthoringRevision = state.Document.Revision,
            ActivatedPackageSha256 = packageHash,
            CompositionPackageSha256 = state.Document.LastCompositionPackageSha256,
            FinalStateHash = state.Document.LastQualifiedFinalStateHash,
            ProjectIdentityFingerprint = identityFingerprint,
            ReleaseCandidateRecordSha256 = rcHash
        });
    }

    private static IReadOnlyList<string> CompareTokens(
        GameProjectSeedRegenerationRequest expected,
        GameProjectSeedRegenerationTruthTokens actual)
    {
        var diagnostics = new List<string>();
        if (!string.Equals(expected.ExpectedSourceRecordSha256, actual.SourceRecordSha256, StringComparison.Ordinal))
            diagnostics.Add("regeneration.source_changed");
        if (!string.Equals(expected.ExpectedQualifiedAuthoringFingerprint, actual.QualifiedAuthoringFingerprint,
                StringComparison.Ordinal) || expected.ExpectedAuthoringRevision != actual.AuthoringRevision)
            diagnostics.Add("regeneration.authoring_changed");
        if (!string.Equals(expected.ExpectedActivatedPackageSha256, actual.ActivatedPackageSha256,
                StringComparison.Ordinal)
            || !string.Equals(expected.ExpectedCompositionPackageSha256, actual.CompositionPackageSha256,
                StringComparison.Ordinal)
            || !string.Equals(expected.ExpectedFinalStateHash, actual.FinalStateHash, StringComparison.Ordinal))
            diagnostics.Add("regeneration.package_changed");
        if (!string.Equals(expected.ExpectedProjectIdentityFingerprint, actual.ProjectIdentityFingerprint,
                StringComparison.Ordinal)) diagnostics.Add("regeneration.identity_changed");
        if (!string.Equals(expected.ExpectedReleaseCandidateRecordSha256, actual.ReleaseCandidateRecordSha256,
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

    private UnifiedGameProjectWorkspaceController NewController(GameProjectFeatureModuleAuthoringService authoring) => new(
        _currentPackageService,
        authoring,
        _builder,
        generatedSourceService: _sourceService,
        generatedWorldSummaryService: new GameProjectGeneratedWorldSummaryService());

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

    private static void CloneProject(string source, string target)
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
            StringComparison.OrdinalIgnoreCase);

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string Confined(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.Equals(fullRoot, comparison) && !path.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("regeneration.path_escape");
        return path;
    }

    private static GameProjectSeedRegenerationPreview CandidateFailure(
        string attemptId,
        string candidate,
        string diagnostic,
        GameProjectBuildResult? build = null)
    {
        if (Directory.Exists(candidate)) Directory.Delete(candidate, recursive: true);
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
        Diagnostics = [diagnostic]
    };

    private sealed record CurrentCapture(
        string ProjectFolder,
        SeededGeneratedProjectSourceValidationResult Source,
        GameProjectIdentityDocument Identity,
        GameProjectSeedRegenerationTruthTokens Tokens);
}
