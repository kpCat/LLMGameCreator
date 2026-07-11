using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using RuntimeSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.ProductLineRuntimeQualification;

public sealed class ProductLineRuntimeQualifier
{
    public static IReadOnlyList<string> CanonicalActionPlan { get; } =
    [
        "start_runtime", "move", "interact", "inspect_inventory", "open_dialogue",
        "start_or_update_quest", "show_inventory", "craft", "harvest", "transaction",
        "begin_encounter", "basic_attack", "show_final_state"
    ];

    private readonly ISelectedRuntimeVariantInteractiveSessionService _runtime;

    public ProductLineRuntimeQualifier(ISelectedRuntimeVariantInteractiveSessionService runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    public ProductLineRuntimeQualificationResult Qualify(
        GamePackageDefinition package,
        ProductLineRuntimeQualificationRequest request)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(request);
        var start = new SelectedRuntimeVariantInteractiveSessionStartRequest
        {
            SessionId = request.SessionId,
            CandidateId = request.CandidateId,
            VariantKind = request.VariantKind,
            PackagePath = request.PackagePath,
            PackageSha256 = request.PackageSha256
        };
        var session = _runtime.StartSession(package, start);
        var catalog = session.AvailableActions.Select(CloneDescriptor).ToList();
        var invalid = _runtime.ExecuteAction(package, session, new()
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

        foreach (var action in CanonicalActionPlan.Take(8)) Execute(package, session, action);
        var checkpoint = _runtime.SaveCheckpoint(
            session,
            request.CheckpointId,
            request.CreatedAtUtc);
        Execute(package, session, "harvest");
        Execute(package, session, "transaction");
        var checkpointReplay = _runtime.ReloadCheckpoint(package, start, checkpoint);
        var checkpointEvidence = Freeze("checkpoint_reload", checkpointReplay);
        if (!checkpointReplay.Passed)
        {
            throw new InvalidOperationException("Runtime qualification checkpoint replay failed: " + request.CandidateId);
        }

        session = checkpointReplay.Session;
        foreach (var action in CanonicalActionPlan.Skip(8)) Execute(package, session, action);
        var finalCheckpoint = _runtime.SaveCheckpoint(
            session,
            request.FinalCheckpointId,
            request.CreatedAtUtc);
        var finalReplay = _runtime.ReloadCheckpoint(package, start, finalCheckpoint);
        var finalEvidence = Freeze("full_final_journal", finalReplay);
        var bindingPassed = session.ActionJournal.Where(entry => entry.RuntimeExecuted).All(entry =>
            entry.ExecutionBindingValidated
            && entry.TargetId == entry.ExecutionTargetId
            && !string.IsNullOrWhiteSpace(entry.CanonicalStepId)
            && entry.CanonicalStepIndex >= 0
            && entry.RuntimeCommandStartIndex >= 0
            && entry.RuntimeCommandEndIndex >= entry.RuntimeCommandStartIndex);

        return new ProductLineRuntimeQualificationResult
        {
            StartRequest = start,
            Session = session,
            ActionCatalog = catalog,
            Checkpoint = checkpoint,
            CheckpointReplay = checkpointEvidence,
            FinalReplay = finalEvidence,
            InvalidActionStateUnchanged = invalidUnchanged,
            ActionDescriptorExecutionBindingPassed = bindingPassed,
            CanonicalActionPlanSignature = string.Join("|", catalog.Select(action =>
                action.ActionId + ":" + action.CanonicalStepId + ":"
                + action.RuntimeCommandStartIndex + "-" + action.RuntimeCommandEndIndex))
        };
    }

    private void Execute(GamePackageDefinition package, RuntimeSession session, string actionId)
    {
        var result = _runtime.ExecuteAction(package, session, new()
        {
            ActionRequestId = session.SessionId + "-action-" + session.CurrentActionIndex.ToString("000"),
            SessionId = session.SessionId,
            ActionIndex = session.CurrentActionIndex,
            ActionId = actionId
        });
        if (result.Status != "EXECUTED" || !result.CorrelationPassed)
        {
            throw new InvalidOperationException(
                "Runtime qualification action failed: " + actionId + ":" + string.Join(";", result.Diagnostics));
        }
    }

    private static ProductLineRuntimeQualificationReplayEvidence Freeze(
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
}
