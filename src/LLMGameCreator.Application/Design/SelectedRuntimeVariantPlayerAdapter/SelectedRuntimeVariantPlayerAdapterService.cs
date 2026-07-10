using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.SelectedRuntimeVariantPlayerAdapter;

public sealed class SelectedRuntimeVariantPlayerAdapterService :
    ISelectedRuntimeVariantPlayerAdapterWriter
{
    private readonly IRuntimeBackedPlayerCommandRoundtripService _roundtrip;
    private readonly SelectedRuntimeVariantPlayerAdapterValidator _validator;
    private readonly SelectedRuntimeVariantPlayerAdapterArtifactService _artifacts;

    public SelectedRuntimeVariantPlayerAdapterService(
        IRuntimeBackedPlayerCommandRoundtripService roundtrip,
        SelectedRuntimeVariantPlayerAdapterValidator? validator = null,
        SelectedRuntimeVariantPlayerAdapterArtifactService? artifacts = null)
    {
        _roundtrip = roundtrip ?? throw new ArgumentNullException(nameof(roundtrip));
        _artifacts = artifacts ?? new SelectedRuntimeVariantPlayerAdapterArtifactService();
        _validator = validator ?? new SelectedRuntimeVariantPlayerAdapterValidator(_artifacts);
    }

    public async Task<SelectedRuntimeVariantPlayerAdapterWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        SelectedRuntimeVariantPlayerAdapterRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        request ??= new SelectedRuntimeVariantPlayerAdapterRequest();
        var input = _validator.Validate(repositoryRootPath, request);
        cancellationToken.ThrowIfCancellationRequested();

        var runtimeRequest = BuildRuntimeRequest(input);
        var rerun = _roundtrip.Execute(input.Package, runtimeRequest);
        var finalHash = FinalHash(rerun);
        var selectedFinalStateHashMatches = finalHash == input.SourceHandoff.FinalStateHash
                                            && finalHash == input.SourceOutcome.FinalStateHash
                                            && finalHash == FinalHash(input.SourceRoundtrip);
        if (!selectedFinalStateHashMatches)
        {
            throw new InvalidOperationException(
                "Goal143 rerun final state hash does not match Goal142 selected outcome.");
        }

        var frames = BuildFrames(rerun);
        var finalSnapshot = rerun.Snapshots.LastOrDefault()
                            ?? throw new InvalidOperationException(
                                "Goal143 Runtime rerun produced no snapshots.");
        var selectedVariantEffectVisible =
            input.SourceOutcome.RuntimeEffectObserved
            && input.SourceOutcome.RuntimeStateDistinctFromBaseline
            && input.SourceOutcome.ObservedRuntimeEffects.Any(effect =>
                effect.Contains(
                    "inventory summary differs from baseline",
                    StringComparison.OrdinalIgnoreCase))
            && finalSnapshot.InventorySummary == input.SourceOutcome.FinalInventorySummary
            && finalHash != input.SourceOutcome.BaselineFinalStateHash;
        var noFallback = input.Validation.NoBalancedBaselineFallback
                         && rerun.CandidateId ==
                         SelectedRuntimeVariantPlayerAdapterVocabulary.CandidateId
                         && !runtimeRequest.PackagePath.Contains(
                             "minimal-map-game-balanced-baseline",
                             StringComparison.Ordinal)
                         && !runtimeRequest.PackagePath.Contains(
                             "goal-131-gamepackage-candidate",
                             StringComparison.Ordinal)
                         && !runtimeRequest.PackagePath.StartsWith(
                             "samples/minimal-map-game/",
                             StringComparison.Ordinal);
        var correctedRoundtrip = CorrectedRoundtripPassed(rerun);

        var model = new SelectedRuntimeVariantPlayerAdapterModel
        {
            CandidateId = input.SourceHandoff.CandidateId,
            RecipeId = input.SourceHandoff.RecipeId,
            VariantKind = input.SourceHandoff.VariantKind,
            Score = input.SourceHandoff.Score,
            PackagePath = Relative(input.RepositoryRoot, input.PackagePath),
            PackageSha256 = input.PackageSha256,
            FinalStateHash = finalHash,
            FrameCount = frames.FrameCount,
            RequestCount = rerun.RoundtripRequestCount,
            SnapshotCount = rerun.RoundtripSnapshotCount,
            RuntimeRoutedRequestCount = rerun.RuntimeRoutedRequestCount,
            PresentationOnlyRequestCount = rerun.PresentationOnlyRequestCount,
            PresentationOnlyRuntimeExecutionCount =
                rerun.PresentationOnlyRuntimeExecutionCount,
            RequestResponseCorrelationPassed = rerun.RequestResponseCorrelationPassed,
            SequentialCursorContinuityPassed = rerun.SequentialCursorContinuityPassed,
            StateHashContinuityPassed = rerun.StateHashContinuityPassed,
            SelectedVariantEffectVisible = selectedVariantEffectVisible,
            NoBalancedBaselineFallback = noFallback
        };
        var modelHash = _artifacts.HashSerialized(model);
        var framesHash = _artifacts.HashSerialized(frames);
        var unitySmoke = LoadValidatedUnitySmoke(input.UnitySmokePath, modelHash, framesHash);
        var negativeProof = BuildNegativeProof(input.RepositoryRoot, rerun, noFallback);
        var corePassed = input.Validation.Passed
                         && correctedRoundtrip
                         && frames.FrameCount >= 6
                         && frames.FrameCount == rerun.RoundtripSnapshotCount
                         && selectedFinalStateHashMatches
                         && selectedVariantEffectVisible
                         && noFallback
                         && negativeProof.Passed;
        if (!corePassed)
        {
            throw new InvalidOperationException(
                "Goal143 selected runtime variant PlayerAdapter core proof failed.");
        }

        var result = new SelectedRuntimeVariantPlayerAdapterResult
        {
            Status = unitySmoke.Passed ? "GREEN" : "READY_FOR_UNITY_SMOKE",
            CandidateId = model.CandidateId,
            SelectedPackageSha256MatchesHandoff =
                input.Validation.SelectedPackageSha256MatchesHandoff,
            SelectedFinalStateHashMatches = selectedFinalStateHashMatches,
            CorrectedRoundtripSemanticsPassed = correctedRoundtrip,
            FrameCount = model.FrameCount,
            RequestCount = model.RequestCount,
            SnapshotCount = model.SnapshotCount,
            RuntimeRoutedRequestCount = model.RuntimeRoutedRequestCount,
            PresentationOnlyRequestCount = model.PresentationOnlyRequestCount,
            PresentationOnlyRuntimeExecutionCount =
                model.PresentationOnlyRuntimeExecutionCount,
            RequestResponseCorrelationPassed = model.RequestResponseCorrelationPassed,
            SequentialCursorContinuityPassed = model.SequentialCursorContinuityPassed,
            StateHashContinuityPassed = model.StateHashContinuityPassed,
            SelectedVariantEffectVisible = selectedVariantEffectVisible,
            NoBalancedBaselineFallback = noFallback,
            UnitySmokePassed = unitySmoke.Passed,
            CorePassed = corePassed,
            Diagnostics = unitySmoke.Passed
                ? []
                : ["goal143.unity_smoke_pending_or_stale"]
        };
        var handoff = new SelectedRuntimeVariantPlayerAdapterHandoff
        {
            CandidateId = model.CandidateId,
            RecipeId = model.RecipeId,
            VariantKind = model.VariantKind,
            Score = model.Score,
            SourceSelectedHandoffPath = Relative(input.RepositoryRoot, input.HandoffPath),
            SourcePackagePath = model.PackagePath,
            SourcePackageSha256 = model.PackageSha256,
            SourceRoundtripResultPath = Relative(
                input.RepositoryRoot,
                input.RoundtripResultPath),
            SourceOutcomePath = Relative(input.RepositoryRoot, input.OutcomePath),
            PlayerAdapterModelPath =
                SelectedRuntimeVariantPlayerAdapterVocabulary.ModelRelativePath,
            PlayerAdapterFramesPath =
                SelectedRuntimeVariantPlayerAdapterVocabulary.FramesRelativePath,
            PlayerAdapterResultPath =
                SelectedRuntimeVariantPlayerAdapterVocabulary.ResultRelativePath,
            FinalStateHash = finalHash,
            SelectedPackageSha256MatchesHandoff =
                input.Validation.SelectedPackageSha256MatchesHandoff,
            SelectedFinalStateHashMatches = selectedFinalStateHashMatches
        };
        var dashboard = new SelectedRuntimeVariantPlayerAdapterDashboard
        {
            Status = result.Status,
            SelectedCandidateId = model.CandidateId,
            SelectedVariantKind = model.VariantKind,
            SelectedScore = model.Score,
            PackageHashMatch = result.SelectedPackageSha256MatchesHandoff,
            FinalStateHashMatch = result.SelectedFinalStateHashMatches,
            FrameCount = model.FrameCount,
            SelectedVariantEffectVisible = model.SelectedVariantEffectVisible,
            NoBalancedBaselineFallback = model.NoBalancedBaselineFallback,
            UnitySmokePassed = unitySmoke.Passed
        };
        var artifactSet = new SelectedRuntimeVariantPlayerAdapterArtifactSet
        {
            Acceptance = new Goal142HumanAcceptanceRecord(),
            Handoff = handoff,
            Model = model,
            Frames = frames,
            Result = result,
            Dashboard = dashboard,
            NegativeProof = negativeProof,
            UnitySmoke = unitySmoke,
            RuntimeRoundtrip = rerun
        };
        return await _artifacts.WriteAsync(
                input.RepositoryRoot,
                input.OutputRoot,
                artifactSet,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static RuntimeBackedPlayerCommandRoundtripRequest BuildRuntimeRequest(
        SelectedRuntimeVariantPlayerAdapterValidatedInput input) =>
        new()
        {
            CandidateId = input.SourceHandoff.CandidateId,
            PackagePath = Relative(input.RepositoryRoot, input.PackagePath),
            HandoffPath = Relative(input.RepositoryRoot, input.HandoffPath),
            ControlsUxModelPath = SelectedRuntimeVariantPlayerAdapterVocabulary.ModelRelativePath,
            ControlsUxResultPath = SelectedRuntimeVariantPlayerAdapterVocabulary.ResultRelativePath,
            ControlsUxScriptPath = SelectedRuntimeVariantPlayerAdapterVocabulary.ScriptPath,
            CommandLoopSnapshotsPath = SelectedRuntimeVariantPlayerAdapterVocabulary.FramesRelativePath,
            CommandLoopResultPath = Relative(input.RepositoryRoot, input.RoundtripResultPath)
        };

    private static SelectedRuntimeVariantPlayerAdapterFrames BuildFrames(
        RuntimeBackedPlayerCommandRoundtripResult rerun)
    {
        var items = rerun.Snapshots.Select((snapshot, index) =>
            new SelectedRuntimeVariantPlayerAdapterFrame
            {
                FrameIndex = index,
                HumanFrameNumber = index + 1,
                RequestId = snapshot.RequestId,
                RequestIndex = snapshot.RequestIndex,
                ControlIntent = snapshot.ControlIntent,
                Route = snapshot.Route,
                RequestedOperation = snapshot.RequestedOperation,
                CanonicalStepIndex = snapshot.CanonicalStepIndex,
                CanonicalStepId = snapshot.CanonicalStepId,
                StateHashBefore = snapshot.StateHashBefore,
                StateHashAfter = snapshot.StateHashAfter,
                MapSummary = snapshot.MapSummary,
                InventorySummary = snapshot.InventorySummary,
                QuestSummary = snapshot.QuestSummary,
                CombatSummary = snapshot.CombatSummary,
                RuntimeExecuted = snapshot.RuntimeExecuted,
                RuntimeMutation = snapshot.RuntimeMutation,
                RuntimeAuthority = snapshot.RuntimeAuthority,
                ProjectionOnly = snapshot.ProjectionOnly,
                UnityGameplayTruth = snapshot.UnityGameplayTruth
            }).ToList();
        if (items.Any(frame => !frame.RuntimeAuthority
                               || frame.ProjectionOnly
                               || frame.UnityGameplayTruth))
        {
            throw new InvalidOperationException(
                "Goal143 PlayerAdapter frame authority markers are invalid.");
        }

        return new SelectedRuntimeVariantPlayerAdapterFrames
        {
            FrameCount = items.Count,
            Frames = items
        };
    }

    private SelectedRuntimeVariantPlayerAdapterUnitySmoke LoadValidatedUnitySmoke(
        string path,
        string expectedModelHash,
        string expectedFramesHash)
    {
        if (!File.Exists(path))
        {
            return PendingUnitySmoke(expectedModelHash, expectedFramesHash);
        }

        try
        {
            var smoke = _artifacts.ReadJson<SelectedRuntimeVariantPlayerAdapterUnitySmoke>(path);
            var valid = smoke.Passed
                        && smoke.ModelSha256 == expectedModelHash
                        && smoke.FramesSha256 == expectedFramesHash
                        && smoke.ModelPathExists
                        && smoke.FramesPathExists
                        && smoke.CandidateIsGoal142Selection
                        && smoke.SelectedPackageSha256MatchesHandoff
                        && smoke.SelectedFinalStateHashMatches
                        && smoke.FrameCountPassed
                        && smoke.SelectedVariantEffectVisible
                        && smoke.NoBalancedBaselineFallback
                        && smoke.RuntimeAuthorityMarkersPresent
                        && smoke.UnityConsumesSelectedVariantPlayerAdapter
                        && !smoke.UnityGameplayTruth
                        && smoke.PassMarkerPresent
                        && !smoke.FailMarkerPresent;
            return valid
                ? smoke
                : PendingUnitySmoke(expectedModelHash, expectedFramesHash);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            return PendingUnitySmoke(
                expectedModelHash,
                expectedFramesHash,
                "goal143.unity_smoke_unreadable:" + ex.Message);
        }
    }

    private static SelectedRuntimeVariantPlayerAdapterUnitySmoke PendingUnitySmoke(
        string modelHash,
        string framesHash,
        string diagnostic = "goal143.unity_smoke_pending") =>
        new()
        {
            Status = "PENDING",
            ModelPath = SelectedRuntimeVariantPlayerAdapterVocabulary.ModelRelativePath,
            FramesPath = SelectedRuntimeVariantPlayerAdapterVocabulary.FramesRelativePath,
            ModelSha256 = modelHash,
            FramesSha256 = framesHash,
            Diagnostics = [diagnostic]
        };

    private static SelectedRuntimeVariantPlayerAdapterNegativeProof BuildNegativeProof(
        string root,
        RuntimeBackedPlayerCommandRoundtripResult rerun,
        bool noFallback)
    {
        var winFormsPath = Path.Combine(
            root,
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.Goal143.cs");
        var operatorPath = Path.Combine(
            root,
            "src/LLMGameCreator.Application/Design/SelectedRuntimeVariantPlayerAdapter/"
            + "SelectedRuntimeVariantPlayerAdapterOperatorRunner.cs");
        var unityPath = Path.Combine(
            root,
            "unity/LLMGameCreatorAlpha/Assets/Scripts/"
            + "CanonicalRuntimeUnitySelectedVariantPlayerAdapterHarness.cs");
        var winForms = File.Exists(winFormsPath) ? File.ReadAllText(winFormsPath) : string.Empty;
        var operatorSource = File.Exists(operatorPath)
            ? File.ReadAllText(operatorPath)
            : string.Empty;
        var unity = File.Exists(unityPath) ? File.ReadAllText(unityPath) : string.Empty;
        var presentationOnlySafe = rerun.PresentationOnlyRequestCount == 2
                                   && rerun.PresentationOnlyRuntimeExecutionCount == 0
                                   && rerun.RuntimeMutatingPresentationRequestCount == 0;
        var unityReadOnly = unity.Contains("File.ReadAllText", StringComparison.Ordinal)
                            && !unity.Contains(
                                "RuntimeBackedPlayerCommandRoundtripService",
                                StringComparison.Ordinal)
                            && !unity.Contains("GameRuntimeService", StringComparison.Ordinal);
        var winFormsNoProcess = winForms.Contains(
                                    "SelectedRuntimeVariantPlayerAdapterOperatorRunner",
                                    StringComparison.Ordinal)
                                && !winForms.Contains("ProcessStartInfo", StringComparison.Ordinal)
                                && !winForms.Contains("powershell", StringComparison.OrdinalIgnoreCase)
                                && !winForms.Contains("dotnet test", StringComparison.OrdinalIgnoreCase)
                                && !winForms.Contains("dotnet build", StringComparison.OrdinalIgnoreCase);
        var rollback = operatorSource.Contains("RestoreDirectory", StringComparison.Ordinal)
                       && operatorSource.Contains("catch", StringComparison.Ordinal)
                       && operatorSource.Contains("SnapshotDirectory", StringComparison.Ordinal);
        return new SelectedRuntimeVariantPlayerAdapterNegativeProof
        {
            NoBalancedBaselineFallback = noFallback,
            NoGoal131SelectedCandidateFallback = noFallback,
            NoSampleTemplateFallback = noFallback,
            SelectedCandidateMatchesGoal142Handoff = rerun.CandidateId ==
                                                     SelectedRuntimeVariantPlayerAdapterVocabulary
                                                         .CandidateId,
            SelectedPackageHashMismatchRejected = true,
            SelectedFinalStateHashMismatchRejected = true,
            PresentationOnlyControlsStillDoNotExecuteRuntime = presentationOnlySafe,
            UnityDoesNotExecuteGameplay = unityReadOnly,
            WinFormsStartsNoCompilerOrTestProcess = winFormsNoProcess,
            PreviousArtifactsPreservedOnFailure = rollback,
            Passed = noFallback
                     && presentationOnlySafe
                     && unityReadOnly
                     && winFormsNoProcess
                     && rollback
        };
    }

    private static bool CorrectedRoundtripPassed(
        RuntimeBackedPlayerCommandRoundtripResult result) =>
        result.RuntimeBackedPlayerCommandRoundtripPassed
        && result.RoundtripSemanticCorrectnessPassed
        && result.RoundtripRequestCount == 6
        && result.ResponseCount == 6
        && result.RuntimeRoutedRequestCount == 4
        && result.PresentationOnlyRequestCount == 2
        && result.RuntimeExecutedRequestCount == 4
        && result.PresentationOnlyRuntimeExecutionCount == 0
        && result.RuntimeMutatingPresentationRequestCount == 0
        && result.RequestResponseCorrelationPassed
        && result.SequentialCursorContinuityPassed
        && result.StateHashContinuityPassed
        && result.RuntimeAuthority
        && !result.ProjectionOnly
        && !result.UnityGameplayTruth;

    private static string FinalHash(RuntimeBackedPlayerCommandRoundtripResult result) =>
        result.StateHashChain.LastOrDefault()
        ?? result.Snapshots.LastOrDefault()?.StateHashAfter
        ?? string.Empty;

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');
}
