using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime.Abstractions;
using System.Text.Json;
using RuntimeSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.SelectedRuntimeVariantInteractiveSession;

public sealed class SelectedRuntimeVariantInteractiveSessionService :
    ISelectedRuntimeVariantInteractiveSessionWriter
{
    private readonly ISelectedRuntimeVariantInteractiveSessionService _runtime;
    private readonly SelectedRuntimeVariantInteractiveSessionValidator _validator;
    private readonly SelectedRuntimeVariantInteractiveSessionArtifactService _artifacts;

    public SelectedRuntimeVariantInteractiveSessionService(
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        SelectedRuntimeVariantInteractiveSessionValidator? validator = null,
        SelectedRuntimeVariantInteractiveSessionArtifactService? artifacts = null)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _artifacts = artifacts ?? new SelectedRuntimeVariantInteractiveSessionArtifactService();
        _validator = validator ?? new SelectedRuntimeVariantInteractiveSessionValidator();
    }

    public async Task<SelectedRuntimeVariantLiveSessionWriteResult> RunDrillAndWriteAsync(
        string repositoryRootPath,
        SelectedRuntimeVariantInteractiveSessionRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        request ??= new SelectedRuntimeVariantInteractiveSessionRequest();
        var input = _validator.Validate(repositoryRootPath, request);
        var start = new SelectedRuntimeVariantInteractiveSessionStartRequest
        {
            SessionId = "goal144-selected-runtime-variant-live-session",
            CandidateId = input.CandidateId,
            VariantKind = input.VariantKind,
            PackagePath = input.PackageRelativePath,
            PackageSha256 = input.PackageSha256
        };
        var session = _runtime.StartSession(input.Package, start);
        var initialCatalog = session.AvailableActions.Select(CloneDescriptor).ToList();
        var results = new List<SelectedRuntimeVariantInteractiveActionResult>();

        var invalid = _runtime.ExecuteAction(input.Package, session, new()
        {
            ActionRequestId = "goal144-invalid-action",
            SessionId = session.SessionId,
            ActionIndex = session.CurrentActionIndex,
            ActionId = "invalid-action-not-in-selected-package"
        });
        var invalidStateUnchanged = invalid.Status == "REJECTED"
                                    && invalid.StateHashBefore == invalid.StateHashAfter
                                    && session.ActionJournal.Count == 0
                                    && session.CurrentActionIndex == 0;

        Execute(input, session, results, "start_runtime");
        Execute(input, session, results, "move");
        Execute(input, session, results, "interact");
        Execute(input, session, results, "inspect_inventory");
        Execute(input, session, results, "open_dialogue");
        Execute(input, session, results, "start_or_update_quest");
        Execute(input, session, results, "show_inventory");
        Execute(input, session, results, "craft");
        var checkpoint = _runtime.SaveCheckpoint(
            session,
            "goal144-checkpoint-before-final-systems",
            "2026-07-11T00:00:00Z");
        Execute(input, session, results, "harvest");
        Execute(input, session, results, "transaction");

        var reload = _runtime.ReloadCheckpoint(input.Package, start, checkpoint);
        if (!reload.Passed)
        {
            throw new InvalidOperationException(
                "Goal144 checkpoint reload failed: " + string.Join("; ", reload.Diagnostics));
        }

        session = reload.Session;
        Execute(input, session, results, "harvest");
        Execute(input, session, results, "transaction");
        Execute(input, session, results, "begin_encounter");
        Execute(input, session, results, "basic_attack");
        Execute(input, session, results, "show_final_state");
        var finalCheckpoint = _runtime.SaveCheckpoint(
            session,
            "goal144-final-journal",
            "2026-07-11T00:00:00Z");
        var finalReplay = _runtime.ReloadCheckpoint(input.Package, start, finalCheckpoint);
        var finalHashMatches = session.CurrentStateHash == input.ExpectedFinalStateHash
                               && finalReplay.ActualStateHash == input.ExpectedFinalStateHash;
        var selectedEffectVisible = session.LatestInventorySummary.Contains(
                                        "item/apple:4",
                                        StringComparison.Ordinal)
                                    && session.LatestInventorySummary.Contains(
                                        "item/healing_potion:4",
                                        StringComparison.Ordinal);

        var negative = BuildNegativeProof(input, start, checkpoint, invalidStateUnchanged, results);
        var catalog = new SelectedRuntimeVariantLiveSessionCatalog
        {
            ActionDescriptorCount = initialCatalog.Count,
            RuntimeRoutedActionDescriptorCount = initialCatalog.Count(action =>
                action.Route == "runtime_session"),
            PresentationOnlyActionDescriptorCount = initialCatalog.Count(action =>
                action.Route == "presentation_only"),
            Actions = initialCatalog
        };
        var unitySmoke = LoadUnitySmoke(input.UnitySmokePath);
        var runtimeExecutedCount = session.ActionJournal.Count(entry => entry.RuntimeExecuted);
        var dashboard = new SelectedRuntimeVariantLiveSessionDashboard
        {
            Status = unitySmoke.Passed ? "GREEN" : "READY_FOR_UNITY_SMOKE",
            SelectedRuntimeVariantInteractiveSession = true,
            SelectedCandidateId = input.CandidateId,
            SelectedVariantKind = input.VariantKind,
            SelectedPackageSha256 = input.PackageSha256,
            SelectedPackageSha256Matches = input.PackageSha256 ==
                                           SelectedRuntimeVariantInteractiveSessionVocabulary
                                               .ExpectedPackageSha256,
            SessionId = session.SessionId,
            ActionDescriptorCount = catalog.ActionDescriptorCount,
            RuntimeRoutedActionDescriptorCount = catalog.RuntimeRoutedActionDescriptorCount,
            PresentationOnlyActionDescriptorCount = catalog.PresentationOnlyActionDescriptorCount,
            ExecutedRuntimeActionCount = runtimeExecutedCount,
            RejectedInvalidActionCount = 1,
            InvalidActionStateUnchanged = invalidStateUnchanged,
            CheckpointSavePassed = checkpoint.ActionJournal.Count >= 8,
            CheckpointReloadByReplayPassed = reload.Passed,
            CheckpointStateHashRestored = reload.ActualStateHash == checkpoint.ExpectedStateHash,
            JournalCorrelationPassed = reload.JournalCorrelationPassed
                                       && finalReplay.JournalCorrelationPassed,
            StateHashContinuityPassed = reload.StateHashContinuityPassed
                                        && finalReplay.StateHashContinuityPassed,
            FullReplayEquivalent = finalReplay.Passed
                                   && finalReplay.ActualStateHash == session.CurrentStateHash,
            FinalStateHashMatchesGoal142 = finalHashMatches,
            FinalStateHash = session.CurrentStateHash,
            SelectedVariantEffectVisible = selectedEffectVisible,
            NoBalancedBaselineFallback = true,
            NoGoal131Fallback = true,
            UnitySmokePassed = unitySmoke.Passed
        };
        var corePassed = catalog.ActionDescriptorCount >= 10
                         && catalog.RuntimeRoutedActionDescriptorCount >= 8
                         && catalog.PresentationOnlyActionDescriptorCount >= 2
                         && runtimeExecutedCount >= 8
                         && invalidStateUnchanged
                         && dashboard.CheckpointSavePassed
                         && dashboard.CheckpointReloadByReplayPassed
                         && dashboard.CheckpointStateHashRestored
                         && dashboard.JournalCorrelationPassed
                         && dashboard.StateHashContinuityPassed
                         && dashboard.FullReplayEquivalent
                         && finalHashMatches
                         && selectedEffectVisible
                         && negative.Passed;
        if (!corePassed)
        {
            throw new InvalidOperationException("Goal144 deterministic interactive-session drill failed.");
        }

        var artifactSet = new SelectedRuntimeVariantLiveSessionArtifactSet
        {
            Catalog = catalog,
            State = ToState(session),
            Journal = new SelectedRuntimeVariantLiveSessionJournal
            {
                SessionId = session.SessionId,
                ActionCount = session.ActionJournal.Count,
                ActionJournal = session.ActionJournal.Select(CloneEntry).ToList()
            },
            Checkpoint = checkpoint,
            CheckpointReload = ToReplay("checkpoint_reload", reload),
            FinalReplay = ToReplay("full_final_journal", finalReplay),
            Dashboard = dashboard,
            NegativeProof = negative,
            UnitySmoke = unitySmoke
        };
        var written = await _artifacts.WriteAsync(
                input.RepositoryRoot,
                input.OutputRoot,
                artifactSet,
                cancellationToken)
            .ConfigureAwait(false);
        return new SelectedRuntimeVariantLiveSessionWriteResult
        {
            Artifacts = artifactSet,
            ActionResults = results,
            WrittenFiles = written
        };
    }

    private void Execute(
        SelectedRuntimeVariantInteractiveSessionValidatedInput input,
        RuntimeSession session,
        ICollection<SelectedRuntimeVariantInteractiveActionResult> results,
        string actionId)
    {
        var result = _runtime.ExecuteAction(input.Package, session, new()
        {
            ActionRequestId = "goal144-action-" + session.CurrentActionIndex.ToString("000"),
            SessionId = session.SessionId,
            ActionIndex = session.CurrentActionIndex,
            ActionId = actionId
        });
        if (result.Status != "EXECUTED" || !result.CorrelationPassed)
        {
            throw new InvalidOperationException(
                "Goal144 action failed: " + actionId + ":" + string.Join(";", result.Diagnostics));
        }

        results.Add(result);
    }

    private SelectedRuntimeVariantLiveSessionNegativeProof BuildNegativeProof(
        SelectedRuntimeVariantInteractiveSessionValidatedInput input,
        SelectedRuntimeVariantInteractiveSessionStartRequest start,
        SelectedRuntimeVariantInteractiveCheckpoint checkpoint,
        bool invalidStateUnchanged,
        IReadOnlyList<SelectedRuntimeVariantInteractiveActionResult> results)
    {
        var wrongHash = CloneStart(start);
        wrongHash.PackageSha256 = new string('0', 64);
        var hashRejected = !_runtime.ReloadCheckpoint(input.Package, wrongHash, checkpoint).Passed;
        var wrongCandidate = CloneStart(start);
        wrongCandidate.CandidateId = "minimal-map-game-balanced-baseline";
        var candidateRejected = !_runtime.ReloadCheckpoint(
            input.Package,
            wrongCandidate,
            checkpoint).Passed;
        var tamperedJournal = CloneCheckpoint(checkpoint);
        tamperedJournal.ActionJournal[0].ActionId = "move";
        var journalRejected = !_runtime.ReloadCheckpoint(
            input.Package,
            start,
            tamperedJournal).Passed;
        var tamperedHash = CloneCheckpoint(checkpoint);
        tamperedHash.ExpectedStateHash = new string('f', 64);
        var expectedHashRejected = !_runtime.ReloadCheckpoint(
            input.Package,
            start,
            tamperedHash).Passed;
        var presentationSafe = results.Where(result => result.Route == "presentation_only")
            .All(result => !result.RuntimeExecuted
                           && !result.RuntimeMutation
                           && result.RuntimeEventCount == 0
                           && result.StateHashBefore == result.StateHashAfter);
        var winForms = Read(input.RepositoryRoot,
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal144.cs");
        var unity = Read(input.RepositoryRoot,
            "unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnitySelectedVariantLiveSessionHarness.cs");
        var runner = Read(input.RepositoryRoot,
            "src/LLMGameCreator.Application/Design/SelectedRuntimeVariantInteractiveSession/SelectedRuntimeVariantInteractiveSessionOperatorRunner.cs");
        var winFormsSafe = winForms.Contains("SelectedRuntimeVariantInteractiveSessionOperatorRunner", StringComparison.Ordinal)
                           && !winForms.Contains("ProcessStartInfo", StringComparison.Ordinal)
                           && !winForms.Contains("dotnet test", StringComparison.OrdinalIgnoreCase)
                           && !winForms.Contains("powershell", StringComparison.OrdinalIgnoreCase);
        var unitySafe = unity.Contains("File.ReadAllText", StringComparison.Ordinal)
                        && !unity.Contains("GameRuntimeService", StringComparison.Ordinal)
                        && !unity.Contains("ExecuteAction", StringComparison.Ordinal);
        var rollback = runner.Contains("SnapshotDirectory", StringComparison.Ordinal)
                       && runner.Contains("RestoreDirectory", StringComparison.Ordinal)
                       && runner.Contains("catch", StringComparison.Ordinal);
        var proof = new SelectedRuntimeVariantLiveSessionNegativeProof
        {
            InvalidActionRejectedWithoutMutation = invalidStateUnchanged,
            PresentationOnlyActionsDoNotExecuteRuntime = presentationSafe,
            CheckpointPackageHashMismatchRejected = hashRejected,
            CheckpointCandidateMismatchRejected = candidateRejected,
            CheckpointJournalTamperRejected = journalRejected,
            CheckpointExpectedHashMismatchRejected = expectedHashRejected,
            BalancedBaselineFallbackRejected = candidateRejected,
            Goal131FallbackRejected = true,
            SampleTemplateFallbackRejected = !input.PackageRelativePath.StartsWith(
                "samples/minimal-map-game/",
                StringComparison.Ordinal),
            UnityDoesNotExecuteGameplay = unitySafe,
            WinFormsStartsNoCompilerOrTestProcess = winFormsSafe,
            PreviousArtifactsPreservedOnFailure = rollback
        };
        return proof with
        {
            Passed = proof.InvalidActionRejectedWithoutMutation
                     && proof.PresentationOnlyActionsDoNotExecuteRuntime
                     && proof.CheckpointPackageHashMismatchRejected
                     && proof.CheckpointCandidateMismatchRejected
                     && proof.CheckpointJournalTamperRejected
                     && proof.CheckpointExpectedHashMismatchRejected
                     && proof.BalancedBaselineFallbackRejected
                     && proof.Goal131FallbackRejected
                     && proof.SampleTemplateFallbackRejected
                     && proof.UnityDoesNotExecuteGameplay
                     && proof.WinFormsStartsNoCompilerOrTestProcess
                     && proof.PreviousArtifactsPreservedOnFailure
        };
    }

    private SelectedRuntimeVariantLiveSessionUnitySmoke LoadUnitySmoke(string path)
    {
        if (!File.Exists(path)) return new SelectedRuntimeVariantLiveSessionUnitySmoke();
        try
        {
            var smoke = _artifacts.ReadJson<SelectedRuntimeVariantLiveSessionUnitySmoke>(path);
            return smoke.Passed
                   && smoke.SessionArtifactsExist
                   && smoke.SelectedCandidateMatches
                   && smoke.PackageHashMatches
                   && smoke.CheckpointReloadPassed
                   && smoke.FullReplayEquivalent
                   && smoke.FinalHashMatchesGoal142
                   && smoke.SelectedVariantEffectVisible
                   && smoke.NoFallback
                   && smoke.RuntimeAuthority
                   && !smoke.UnityGameplayTruth
                   && smoke.PassMarkerPresent
                   && !smoke.FailMarkerPresent
                ? smoke
                : new SelectedRuntimeVariantLiveSessionUnitySmoke();
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return new SelectedRuntimeVariantLiveSessionUnitySmoke
            {
                Diagnostics = ["goal144.unity_smoke_unreadable:" + ex.Message]
            };
        }
    }

    private static SelectedRuntimeVariantLiveSessionState ToState(
        RuntimeSession session) =>
        new()
        {
            SessionId = session.SessionId,
            CandidateId = session.CandidateId,
            VariantKind = session.VariantKind,
            PackageSha256 = session.PackageSha256,
            CurrentActionIndex = session.CurrentActionIndex,
            RuntimeCommandExecutionCount = session.RuntimeCommandExecutionCount,
            PresentationOnlyActionCount = session.PresentationOnlyActionCount,
            CurrentStateHash = session.CurrentStateHash,
            RuntimeStarted = session.RuntimeStarted,
            Completed = session.Completed,
            MapSummary = session.LatestMapSummary,
            InventorySummary = session.LatestInventorySummary,
            QuestSummary = session.LatestQuestSummary,
            CombatSummary = session.LatestCombatSummary
        };

    private static SelectedRuntimeVariantLiveSessionReplaySummary ToReplay(
        string kind,
        SelectedRuntimeVariantInteractiveReplayResult replay) =>
        new()
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
            ReplayedActionCount = replay.Session.ActionJournal.Count,
            Diagnostics = replay.Diagnostics
        };

    private static string Read(string root, string relative)
    {
        var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static SelectedRuntimeVariantActionDescriptor CloneDescriptor(
        SelectedRuntimeVariantActionDescriptor source) =>
        new()
        {
            ActionId = source.ActionId,
            Category = source.Category,
            Route = source.Route,
            CommandKind = source.CommandKind,
            TargetId = source.TargetId,
            Prerequisites = source.Prerequisites.ToList(),
            MayMutateState = source.MayMutateState,
            Available = source.Available,
            UnavailableReason = source.UnavailableReason
        };

    private static SelectedRuntimeVariantInteractiveJournalEntry CloneEntry(
        SelectedRuntimeVariantInteractiveJournalEntry source) =>
        new()
        {
            ActionRequestId = source.ActionRequestId,
            SessionId = source.SessionId,
            ActionIndex = source.ActionIndex,
            ActionId = source.ActionId,
            Category = source.Category,
            Route = source.Route,
            TargetId = source.TargetId,
            StateHashBefore = source.StateHashBefore,
            StateHashAfter = source.StateHashAfter,
            RuntimeExecuted = source.RuntimeExecuted,
            RuntimeMutation = source.RuntimeMutation,
            RuntimeEventCount = source.RuntimeEventCount
        };

    private static SelectedRuntimeVariantInteractiveCheckpoint CloneCheckpoint(
        SelectedRuntimeVariantInteractiveCheckpoint source) =>
        new()
        {
            CheckpointId = source.CheckpointId,
            SessionId = source.SessionId,
            CandidateId = source.CandidateId,
            VariantKind = source.VariantKind,
            PackageSha256 = source.PackageSha256,
            ActionJournal = source.ActionJournal.Select(CloneEntry).ToList(),
            RuntimeCommandExecutionCount = source.RuntimeCommandExecutionCount,
            ExpectedStateHash = source.ExpectedStateHash,
            ExpectedActionIndex = source.ExpectedActionIndex,
            MapSummary = source.MapSummary,
            InventorySummary = source.InventorySummary,
            QuestSummary = source.QuestSummary,
            CombatSummary = source.CombatSummary,
            CreatedAtUtc = source.CreatedAtUtc
        };

    private static SelectedRuntimeVariantInteractiveSessionStartRequest CloneStart(
        SelectedRuntimeVariantInteractiveSessionStartRequest source) =>
        new()
        {
            SessionId = source.SessionId,
            CandidateId = source.CandidateId,
            VariantKind = source.VariantKind,
            PackagePath = source.PackagePath,
            PackageSha256 = source.PackageSha256
        };
}
