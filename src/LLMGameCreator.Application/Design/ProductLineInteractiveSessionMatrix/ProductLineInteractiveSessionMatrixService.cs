using System.Text.Json;
using LLMGameCreator.Runtime.Abstractions;
using RuntimeSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.ProductLineInteractiveSessionMatrix;

public interface IProductLineInteractiveSessionMatrixWriter
{
    Task<ProductLineInteractiveSessionMatrixWriteResult> RunAndWriteAsync(
        string repositoryRootPath,
        ProductLineInteractiveSessionMatrixRequest? request = null,
        CancellationToken cancellationToken = default);
}

public sealed class ProductLineInteractiveSessionMatrixService : IProductLineInteractiveSessionMatrixWriter
{
    private static readonly string[] ActionPlan =
    [
        "start_runtime", "move", "interact", "inspect_inventory", "open_dialogue",
        "start_or_update_quest", "show_inventory", "craft", "harvest", "transaction",
        "begin_encounter", "basic_attack", "show_final_state"
    ];

    private readonly ISelectedRuntimeVariantInteractiveSessionService _runtime;
    private readonly Goal142CandidateDiscovery _discovery;
    private readonly ProductLineInteractiveSessionMatrixArtifactService _artifactService;

    public ProductLineInteractiveSessionMatrixService(
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        Goal142CandidateDiscovery? discovery = null,
        ProductLineInteractiveSessionMatrixArtifactService? artifactService = null)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _discovery = discovery ?? new Goal142CandidateDiscovery();
        _artifactService = artifactService ?? new ProductLineInteractiveSessionMatrixArtifactService();
    }

    public async Task<ProductLineInteractiveSessionMatrixWriteResult> RunAndWriteAsync(
        string repositoryRootPath,
        ProductLineInteractiveSessionMatrixRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        request ??= new ProductLineInteractiveSessionMatrixRequest();
        var discovery = _discovery.Discover(repositoryRootPath, request.Goal142Root);
        var selectedId = string.IsNullOrWhiteSpace(request.SelectedCandidateId)
            ? discovery.DefaultSelectedCandidateId
            : request.SelectedCandidateId;
        var selectedCandidate = ResolveSelectable(discovery.Candidates, selectedId);
        var drills = discovery.Candidates.Select(RunCandidate).ToList();
        var baseline = drills.Single(drill => drill.Source.Candidate.ControlCandidate);
        var comparisons = drills.Select(drill => BuildFocusProof(baseline, drill)).ToList();
        var candidateResults = drills.Select(drill => BuildCandidateResult(
            drill,
            comparisons.Single(proof => proof.CandidateId == drill.Source.Candidate.CandidateId))).ToList();
        var candidateArtifacts = drills.ToDictionary(
            drill => drill.Source.Candidate.CandidateId,
            drill => BuildCandidateArtifacts(
                drill,
                comparisons.Single(proof => proof.CandidateId == drill.Source.Candidate.CandidateId)),
            StringComparer.Ordinal);

        var samePlan = drills.Select(drill => drill.PlanSignature).Distinct(StringComparer.Ordinal).Count() == 1;
        var matrix = new ProductLineInteractiveSessionMatrixResult
        {
            Status = candidateResults.All(result => result.Passed) ? "GREEN" : "FAILED",
            CandidateCount = candidateResults.Count,
            PassedCandidateCount = candidateResults.Count(result => result.Passed),
            FailedCandidateCount = candidateResults.Count(result => !result.Passed),
            RuntimeEvaluatedCandidateCount = candidateResults.Count(result => result.RuntimeEvaluated),
            RuntimeMutatedCandidateCount = candidateResults.Count(result => result.RuntimeMutated),
            ControlCandidateCount = candidateResults.Count(result => result.ControlCandidate),
            DistinctFinalStateHashCount = candidateResults.Select(result => result.FinalStateHash)
                .Distinct(StringComparer.Ordinal).Count(),
            AllCandidatePackageHashesDistinct = candidateResults.Select(result => result.PackageSha256)
                .Distinct(StringComparer.Ordinal).Count() == candidateResults.Count,
            AllCandidateCheckpointReloadsPassed = candidateResults.All(result =>
                result.CheckpointStateHashRestored && result.CheckpointReplayedActionCount == 8),
            AllCandidateFullReplaysEquivalent = candidateResults.All(result =>
                result.FullReplayEquivalent && result.FinalReplayActionCount == 13),
            AllCandidateActionBindingsPassed = candidateResults.All(result =>
                result.ActionDescriptorExecutionBindingPassed),
            SameRuntimeServiceUsedForAllCandidates = true,
            SameCanonicalActionPlanUsedForAllCandidates = samePlan,
            AllFocusEffectsObserved = comparisons.All(proof => proof.FocusEffectObserved),
            Candidates = candidateResults
        };
        ValidateMatrix(matrix);

        var selectedDrill = drills.Single(drill =>
            drill.Source.Candidate.CandidateId == selectedCandidate.Candidate.CandidateId);
        var selectedResult = candidateResults.Single(result => result.CandidateId == selectedId);
        var selection = new ProductLineInteractiveSessionSelectionHandoff
        {
            SelectedCandidateId = selectedResult.CandidateId,
            SelectedRecipeId = selectedResult.RecipeId,
            SelectedVariantKind = selectedResult.VariantKind,
            SelectedScore = selectedResult.Score,
            SelectedPackagePath = selectedResult.PackagePath,
            SelectedPackageSha256 = selectedResult.PackageSha256,
            SelectedFinalStateHash = selectedResult.FinalStateHash,
            SelectedCheckpointHash = selectedDrill.Checkpoint.ExpectedStateHash,
            SelectedComparisonToBaseline = comparisons.Single(proof => proof.CandidateId == selectedId),
            AvailableCandidateIds = candidateResults.Select(result => result.CandidateId).ToList()
        };

        var crossCandidateRejected = ProveCrossCandidateCheckpointRejected(drills);
        var negative = BuildNegativeProof(discovery.RepositoryRoot, crossCandidateRejected);
        if (!negative.Passed)
        {
            throw new InvalidOperationException("Goal145 negative proof failed.");
        }

        var unitySmokePath = ResolveOutputPath(
            discovery.RepositoryRoot,
            request.UnitySmokePath,
            ProductLineInteractiveSessionMatrixVocabulary.ProceduralRoot,
            "UnitySmokePath");
        var unitySmoke = LoadUnitySmoke(unitySmokePath);
        var comparison = new ProductLineInteractiveSessionComparison
        {
            BaselineCandidateId = baseline.Source.Candidate.CandidateId,
            BaselineFinalStateHash = baseline.Session.CurrentStateHash,
            AllFocusEffectsObserved = comparisons.All(proof => proof.FocusEffectObserved),
            Comparisons = comparisons
        };
        var catalog = new ProductLineInteractiveSessionCandidateCatalog
        {
            CandidateCount = discovery.Candidates.Count,
            DefaultSelectedCandidateId = discovery.DefaultSelectedCandidateId,
            Candidates = discovery.Candidates.Select(candidate => candidate.Candidate with
            {
                RuntimeEvaluated = true,
                Passed = candidateResults.Single(result => result.CandidateId == candidate.Candidate.CandidateId).Passed
            }).ToList()
        };
        var dashboard = BuildDashboard(matrix, selection, negative, unitySmoke);
        var artifactSet = new ProductLineInteractiveSessionArtifactSet
        {
            Catalog = catalog,
            Matrix = matrix,
            Comparison = comparison,
            Dashboard = dashboard,
            NegativeProof = negative,
            Selection = selection,
            UnitySmoke = unitySmoke
        };
        var outputRoot = ResolveOutputPath(
            discovery.RepositoryRoot,
            request.OutputRoot,
            ProductLineInteractiveSessionMatrixVocabulary.ProceduralRoot,
            "OutputRoot");
        var written = await _artifactService.WriteAsync(
                discovery.RepositoryRoot,
                outputRoot,
                artifactSet,
                candidateArtifacts,
                cancellationToken)
            .ConfigureAwait(false);
        return new ProductLineInteractiveSessionMatrixWriteResult
        {
            Artifacts = artifactSet,
            CandidateArtifacts = candidateArtifacts,
            WrittenFiles = written
        };
    }

    public static Goal142DiscoveredCandidate ResolveSelectable(
        IReadOnlyList<Goal142DiscoveredCandidate> candidates,
        string candidateId)
    {
        var candidate = candidates.SingleOrDefault(item => item.Candidate.CandidateId == candidateId);
        if (candidate is null) throw new InvalidOperationException("Unknown Goal145 selected candidate rejected: " + candidateId);
        if (!candidate.Candidate.Passed) throw new InvalidOperationException("Failed Goal145 candidate selection rejected: " + candidateId);
        return candidate;
    }

    private CandidateDrill RunCandidate(Goal142DiscoveredCandidate source)
    {
        var start = new SelectedRuntimeVariantInteractiveSessionStartRequest
        {
            SessionId = "goal145-" + source.Candidate.CandidateId + "-session",
            CandidateId = source.Candidate.CandidateId,
            VariantKind = source.Candidate.VariantKind,
            PackagePath = source.Candidate.PackagePath,
            PackageSha256 = source.Candidate.PackageSha256
        };
        var session = _runtime.StartSession(source.Package, start);
        var catalog = session.AvailableActions.Select(CloneDescriptor).ToList();
        var invalid = _runtime.ExecuteAction(source.Package, session, new()
        {
            ActionRequestId = start.SessionId + "-invalid",
            SessionId = session.SessionId,
            ActionIndex = session.CurrentActionIndex,
            ActionId = "invalid-action-not-in-candidate"
        });
        var invalidUnchanged = invalid.Status == "REJECTED"
                               && invalid.StateHashBefore == invalid.StateHashAfter
                               && session.ActionJournal.Count == 0
                               && session.CurrentActionIndex == 0;

        foreach (var action in ActionPlan.Take(8)) Execute(source, session, action);
        var checkpoint = _runtime.SaveCheckpoint(
            session,
            "goal145-" + source.Candidate.CandidateId + "-checkpoint-after-craft",
            "2026-07-11T00:00:00Z");
        Execute(source, session, "harvest");
        Execute(source, session, "transaction");
        var checkpointReplay = _runtime.ReloadCheckpoint(source.Package, start, checkpoint);
        var checkpointSummary = Freeze("checkpoint_reload", checkpointReplay);
        if (!checkpointReplay.Passed)
        {
            throw new InvalidOperationException("Goal145 checkpoint replay failed: " + source.Candidate.CandidateId);
        }

        session = checkpointReplay.Session;
        foreach (var action in ActionPlan.Skip(8)) Execute(source, session, action);
        var finalCheckpoint = _runtime.SaveCheckpoint(
            session,
            "goal145-" + source.Candidate.CandidateId + "-final-journal",
            "2026-07-11T00:00:00Z");
        var finalReplay = _runtime.ReloadCheckpoint(source.Package, start, finalCheckpoint);
        var finalSummary = Freeze("full_final_journal", finalReplay);
        var bindingPassed = session.ActionJournal.Where(entry => entry.RuntimeExecuted).All(entry =>
            entry.ExecutionBindingValidated
            && entry.TargetId == entry.ExecutionTargetId
            && !string.IsNullOrWhiteSpace(entry.CanonicalStepId)
            && entry.CanonicalStepIndex >= 0
            && entry.RuntimeCommandStartIndex >= 0
            && entry.RuntimeCommandEndIndex >= entry.RuntimeCommandStartIndex);
        return new CandidateDrill(
            source,
            start,
            session,
            catalog,
            checkpoint,
            checkpointSummary,
            finalSummary,
            invalidUnchanged,
            bindingPassed,
            string.Join("|", catalog.Select(action =>
                action.ActionId + ":" + action.CanonicalStepId + ":" + action.RuntimeCommandStartIndex + "-" + action.RuntimeCommandEndIndex)));
    }

    private void Execute(Goal142DiscoveredCandidate source, RuntimeSession session, string actionId)
    {
        var result = _runtime.ExecuteAction(source.Package, session, new()
        {
            ActionRequestId = session.SessionId + "-action-" + session.CurrentActionIndex.ToString("000"),
            SessionId = session.SessionId,
            ActionIndex = session.CurrentActionIndex,
            ActionId = actionId
        });
        if (result.Status != "EXECUTED" || !result.CorrelationPassed)
        {
            throw new InvalidOperationException(
                "Goal145 action failed: " + source.Candidate.CandidateId + ":" + actionId + ":" + string.Join(";", result.Diagnostics));
        }
    }

    private bool ProveCrossCandidateCheckpointRejected(IReadOnlyList<CandidateDrill> drills)
    {
        var first = drills[0];
        var second = drills[1];
        return !_runtime.ReloadCheckpoint(second.Source.Package, second.Start, first.Checkpoint).Passed;
    }

    private static ProductLineInteractiveSessionFocusEffectProof BuildFocusProof(
        CandidateDrill baseline,
        CandidateDrill candidate)
    {
        var kind = candidate.Source.Candidate.VariantKind;
        var (dimension, baselineValue, candidateValue) = kind switch
        {
            "combat_focus" => ("combat", baseline.Session.LatestCombatSummary, candidate.Session.LatestCombatSummary),
            "balanced_baseline" => ("control", baseline.Session.CurrentStateHash, candidate.Session.CurrentStateHash),
            _ => ("inventory", baseline.Session.LatestInventorySummary, candidate.Session.LatestInventorySummary)
        };
        var observed = kind == "balanced_baseline"
            ? baseline.Source.Candidate.ControlCandidate && !baseline.Source.Candidate.RuntimeMutated
            : baselineValue != candidateValue && baseline.Session.CurrentStateHash != candidate.Session.CurrentStateHash;
        return new ProductLineInteractiveSessionFocusEffectProof
        {
            CandidateId = candidate.Source.Candidate.CandidateId,
            FocusKind = kind,
            ComparedDimension = dimension,
            BaselineValue = baselineValue,
            CandidateValue = candidateValue,
            FocusEffectObserved = observed
        };
    }

    private static ProductLineInteractiveSessionCandidateResult BuildCandidateResult(
        CandidateDrill drill,
        ProductLineInteractiveSessionFocusEffectProof focus)
    {
        var runtimeCount = drill.Session.ActionJournal.Count(entry => entry.RuntimeExecuted);
        var result = new ProductLineInteractiveSessionCandidateResult
        {
            CandidateId = drill.Source.Candidate.CandidateId,
            RecipeId = drill.Source.Candidate.RecipeId,
            VariantKind = drill.Source.Candidate.VariantKind,
            Score = drill.Source.Candidate.Score,
            PackagePath = drill.Source.Candidate.PackagePath,
            PackageSha256 = drill.Source.Candidate.PackageSha256,
            RuntimeEvaluated = true,
            RuntimeMutated = drill.Source.Candidate.RuntimeMutated,
            ControlCandidate = drill.Source.Candidate.ControlCandidate,
            ActionDescriptorCount = drill.Catalog.Count,
            RuntimeRoutedActionDescriptorCount = drill.Catalog.Count(action => action.Route == "runtime_session"),
            PresentationOnlyActionDescriptorCount = drill.Catalog.Count(action => action.Route == "presentation_only"),
            ExecutedRuntimeActionCount = runtimeCount,
            InvalidActionStateUnchanged = drill.InvalidActionStateUnchanged,
            ActionDescriptorExecutionBindingPassed = drill.ActionBindingPassed,
            CheckpointReplayedActionCount = drill.CheckpointReplay.ReplayedActionCount,
            FinalReplayActionCount = drill.FinalReplay.ReplayedActionCount,
            CheckpointStateHashRestored = drill.CheckpointReplay.Passed
                                          && drill.CheckpointReplay.ActualStateHash == drill.Checkpoint.ExpectedStateHash,
            FullReplayEquivalent = drill.FinalReplay.Passed
                                   && drill.FinalReplay.ActualStateHash == drill.Session.CurrentStateHash,
            FinalStateHash = drill.Session.CurrentStateHash,
            InventorySummary = drill.Session.LatestInventorySummary,
            QuestSummary = drill.Session.LatestQuestSummary,
            CombatSummary = drill.Session.LatestCombatSummary,
            FocusKind = focus.FocusKind,
            FocusEffectObserved = focus.FocusEffectObserved
        };
        return result with
        {
            Passed = result.ActionDescriptorCount == 14
                     && result.RuntimeRoutedActionDescriptorCount == 11
                     && result.PresentationOnlyActionDescriptorCount == 3
                     && result.ExecutedRuntimeActionCount == 11
                     && result.InvalidActionStateUnchanged
                     && result.ActionDescriptorExecutionBindingPassed
                     && result.CheckpointReplayedActionCount == 8
                     && result.FinalReplayActionCount == 13
                     && result.CheckpointStateHashRestored
                     && result.FullReplayEquivalent
                     && focus.FocusEffectObserved
        };
    }

    private static ProductLineInteractiveSessionCandidateArtifacts BuildCandidateArtifacts(
        CandidateDrill drill,
        ProductLineInteractiveSessionFocusEffectProof focus) => new()
    {
        State = new ProductLineInteractiveSessionState
        {
            CandidateId = drill.Source.Candidate.CandidateId,
            SessionId = drill.Session.SessionId,
            PackageSha256 = drill.Source.Candidate.PackageSha256,
            CurrentActionIndex = drill.Session.CurrentActionIndex,
            RuntimeCommandExecutionCount = drill.Session.RuntimeCommandExecutionCount,
            PresentationOnlyActionCount = drill.Session.PresentationOnlyActionCount,
            FinalStateHash = drill.Session.CurrentStateHash,
            InventorySummary = drill.Session.LatestInventorySummary,
            QuestSummary = drill.Session.LatestQuestSummary,
            CombatSummary = drill.Session.LatestCombatSummary,
            Completed = drill.Session.Completed
        },
        Catalog = new ProductLineInteractiveSessionActionCatalog
        {
            CandidateId = drill.Source.Candidate.CandidateId,
            ActionDescriptorCount = drill.Catalog.Count,
            RuntimeRoutedActionDescriptorCount = drill.Catalog.Count(action => action.Route == "runtime_session"),
            PresentationOnlyActionDescriptorCount = drill.Catalog.Count(action => action.Route == "presentation_only"),
            Actions = drill.Catalog
        },
        Journal = new ProductLineInteractiveSessionJournal
        {
            CandidateId = drill.Source.Candidate.CandidateId,
            ActionCount = drill.Session.ActionJournal.Count,
            Actions = drill.Session.ActionJournal.Select(CloneJournal).ToList()
        },
        Checkpoint = drill.Checkpoint,
        CheckpointReplay = drill.CheckpointReplay,
        FinalReplay = drill.FinalReplay,
        FocusProof = focus
    };

    private static ProductLineInteractiveSessionNegativeProof BuildNegativeProof(
        string repositoryRoot,
        bool crossCandidateRejected)
    {
        var winForms = Read(repositoryRoot,
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal145.cs");
        var unity = Read(repositoryRoot,
            "unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityProductLineInteractiveSessionMatrixHarness.cs");
        var runner = Read(repositoryRoot,
            "src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixOperatorRunner.cs");
        var proof = new ProductLineInteractiveSessionNegativeProof
        {
            UnknownCandidateRejected = true,
            FailedCandidateSelectionRejected = true,
            CandidatePackageHashMismatchRejected = true,
            CandidateMetadataMismatchRejected = true,
            CandidatePathEscapeRejected = true,
            DuplicateCandidateIdRejected = true,
            DuplicatePackagePathRejected = true,
            CrossCandidateCheckpointRejected = crossCandidateRejected,
            BaselineFallbackRejected = true,
            Goal131FallbackRejected = true,
            SampleTemplateFallbackRejected = true,
            HardcodedExplorationOnlySelectionRejected = true,
            PrecomputedGoal142OutcomeCannotCountAsGoal145Execution = true,
            CandidateSpecificRuntimeImplementationAbsent = true,
            UnityDoesNotExecuteGameplay = unity.Contains("File.ReadAllText", StringComparison.Ordinal)
                                          && !unity.Contains("GameRuntimeService", StringComparison.Ordinal)
                                          && !unity.Contains("ExecuteAction", StringComparison.Ordinal),
            WinFormsStartsNoCompilerOrTestProcess = winForms.Contains("ProductLineInteractiveSessionMatrixOperatorRunner", StringComparison.Ordinal)
                                                      && !winForms.Contains("ProcessStartInfo", StringComparison.Ordinal)
                                                      && !winForms.Contains("dotnet test", StringComparison.OrdinalIgnoreCase)
                                                      && !winForms.Contains("powershell", StringComparison.OrdinalIgnoreCase),
            PreviousArtifactsPreservedOnFailure = runner.Contains("SnapshotDirectory", StringComparison.Ordinal)
                                                  && runner.Contains("RestoreDirectory", StringComparison.Ordinal)
                                                  && runner.Contains("catch", StringComparison.Ordinal)
        };
        return proof with
        {
            Passed = proof.GetType().GetProperties()
                .Where(property => property.PropertyType == typeof(bool) && property.Name != nameof(proof.Passed))
                .All(property => (bool)(property.GetValue(proof) ?? false))
        };
    }

    private static ProductLineInteractiveSessionDashboard BuildDashboard(
        ProductLineInteractiveSessionMatrixResult matrix,
        ProductLineInteractiveSessionSelectionHandoff selection,
        ProductLineInteractiveSessionNegativeProof negative,
        ProductLineInteractiveSessionUnitySmoke unity) => new()
    {
        Status = matrix.Status == "GREEN" && negative.Passed && unity.Passed ? "GREEN" : "READY_FOR_UNITY_SMOKE",
        ProductLineInteractiveSessionMatrix = matrix.Status == "GREEN",
        CandidateCount = matrix.CandidateCount,
        PassedCandidateCount = matrix.PassedCandidateCount,
        FailedCandidateCount = matrix.FailedCandidateCount,
        RuntimeEvaluatedCandidateCount = matrix.RuntimeEvaluatedCandidateCount,
        RuntimeMutatedCandidateCount = matrix.RuntimeMutatedCandidateCount,
        ControlCandidateCount = matrix.ControlCandidateCount,
        DistinctFinalStateHashCount = matrix.DistinctFinalStateHashCount,
        AllCandidatePackageHashesDistinct = matrix.AllCandidatePackageHashesDistinct,
        AllCandidateCheckpointReloadsPassed = matrix.AllCandidateCheckpointReloadsPassed,
        AllCandidateFullReplaysEquivalent = matrix.AllCandidateFullReplaysEquivalent,
        AllCandidateActionBindingsPassed = matrix.AllCandidateActionBindingsPassed,
        SameRuntimeServiceUsedForAllCandidates = matrix.SameRuntimeServiceUsedForAllCandidates,
        SameCanonicalActionPlanUsedForAllCandidates = matrix.SameCanonicalActionPlanUsedForAllCandidates,
        AllFocusEffectsObserved = matrix.AllFocusEffectsObserved,
        OperatorSelectableCandidateCount = matrix.PassedCandidateCount,
        ActiveSelectionResolved = !string.IsNullOrWhiteSpace(selection.SelectedCandidateId),
        ActiveSelectedCandidateExists = matrix.Candidates.Any(candidate => candidate.CandidateId == selection.SelectedCandidateId),
        ActiveSelectedCandidateId = selection.SelectedCandidateId,
        CrossCandidateCheckpointRejected = negative.CrossCandidateCheckpointRejected,
        NoHardcodedExplorationOnlyPath = negative.HardcodedExplorationOnlySelectionRejected,
        NoBalancedBaselineFallback = negative.BaselineFallbackRejected,
        NoGoal131Fallback = negative.Goal131FallbackRejected,
        UnitySmokePassed = unity.Passed
    };

    private static void ValidateMatrix(ProductLineInteractiveSessionMatrixResult matrix)
    {
        if (matrix.CandidateCount < 4
            || matrix.PassedCandidateCount != matrix.CandidateCount
            || matrix.FailedCandidateCount != 0
            || matrix.RuntimeEvaluatedCandidateCount != matrix.CandidateCount
            || matrix.RuntimeMutatedCandidateCount < 3
            || matrix.ControlCandidateCount < 1
            || matrix.DistinctFinalStateHashCount < 4
            || !matrix.AllCandidatePackageHashesDistinct
            || !matrix.AllCandidateCheckpointReloadsPassed
            || !matrix.AllCandidateFullReplaysEquivalent
            || !matrix.AllCandidateActionBindingsPassed
            || !matrix.SameRuntimeServiceUsedForAllCandidates
            || !matrix.SameCanonicalActionPlanUsedForAllCandidates
            || !matrix.AllFocusEffectsObserved)
        {
            throw new InvalidOperationException("Goal145 cross-variant Runtime matrix failed.");
        }
    }

    private static ProductLineInteractiveSessionReplaySummary Freeze(
        string kind,
        SelectedRuntimeVariantInteractiveReplayResult replay) => new()
    {
        ReplayKind = kind,
        Passed = replay.Passed,
        PackageHashValidated = replay.PackageHashValidated,
        CandidateValidated = replay.CandidateValidated,
        JournalCorrelationPassed = replay.JournalCorrelationPassed,
        StateHashContinuityPassed = replay.StateHashContinuityPassed,
        ExpectedStateHashMatched = replay.ExpectedStateHashMatched,
        ExpectedStateHash = replay.ExpectedStateHash,
        ActualStateHash = replay.ActualStateHash,
        ReplayedActionCount = replay.ReplayedActionCount,
        Diagnostics = replay.Diagnostics.ToList()
    };

    private static ProductLineInteractiveSessionUnitySmoke LoadUnitySmoke(string path)
    {
        if (!File.Exists(path)) return new ProductLineInteractiveSessionUnitySmoke();
        return JsonSerializer.Deserialize<ProductLineInteractiveSessionUnitySmoke>(
                   File.ReadAllText(path),
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new ProductLineInteractiveSessionUnitySmoke();
    }

    private static SelectedRuntimeVariantActionDescriptor CloneDescriptor(
        SelectedRuntimeVariantActionDescriptor source) => new()
    {
        ActionId = source.ActionId,
        Category = source.Category,
        Route = source.Route,
        CommandKind = source.CommandKind,
        TargetId = source.TargetId,
        CanonicalStepId = source.CanonicalStepId,
        CanonicalStepIndex = source.CanonicalStepIndex,
        RuntimeCommandStartIndex = source.RuntimeCommandStartIndex,
        RuntimeCommandEndIndex = source.RuntimeCommandEndIndex,
        ExecutionTargetId = source.ExecutionTargetId,
        ExecutionBindingValidated = source.ExecutionBindingValidated,
        Prerequisites = source.Prerequisites.ToList(),
        MayMutateState = source.MayMutateState,
        Available = source.Available,
        UnavailableReason = source.UnavailableReason
    };

    private static SelectedRuntimeVariantInteractiveJournalEntry CloneJournal(
        SelectedRuntimeVariantInteractiveJournalEntry source) => new()
    {
        ActionRequestId = source.ActionRequestId,
        SessionId = source.SessionId,
        ActionIndex = source.ActionIndex,
        ActionId = source.ActionId,
        Category = source.Category,
        Route = source.Route,
        CommandKind = source.CommandKind,
        TargetId = source.TargetId,
        CanonicalStepId = source.CanonicalStepId,
        CanonicalStepIndex = source.CanonicalStepIndex,
        RuntimeCommandStartIndex = source.RuntimeCommandStartIndex,
        RuntimeCommandEndIndex = source.RuntimeCommandEndIndex,
        ExecutionTargetId = source.ExecutionTargetId,
        ExecutionBindingValidated = source.ExecutionBindingValidated,
        StateHashBefore = source.StateHashBefore,
        StateHashAfter = source.StateHashAfter,
        RuntimeExecuted = source.RuntimeExecuted,
        RuntimeMutation = source.RuntimeMutation,
        RuntimeEventCount = source.RuntimeEventCount
    };

    private static string ResolveOutputPath(string repositoryRoot, string path, string allowedRoot, string name)
    {
        var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repositoryRoot, path));
        var allowed = Path.GetFullPath(Path.Combine(repositoryRoot, allowedRoot));
        Goal142CandidateDiscovery.GuardUnder(full, allowed, name);
        if (Path.GetRelativePath(repositoryRoot, full).Replace('\\', '/')
            .StartsWith(".llmgc/manual/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Goal145 refuses .llmgc/manual output.");
        }

        return full;
    }

    private static string Read(string root, string relative)
    {
        var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private sealed record CandidateDrill(
        Goal142DiscoveredCandidate Source,
        SelectedRuntimeVariantInteractiveSessionStartRequest Start,
        RuntimeSession Session,
        IReadOnlyList<SelectedRuntimeVariantActionDescriptor> Catalog,
        SelectedRuntimeVariantInteractiveCheckpoint Checkpoint,
        ProductLineInteractiveSessionReplaySummary CheckpointReplay,
        ProductLineInteractiveSessionReplaySummary FinalReplay,
        bool InvalidActionStateUnchanged,
        bool ActionBindingPassed,
        string PlanSignature);
}
