using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Runtime.Abstractions;

public sealed class SelectedRuntimeVariantActionDescriptor
{
    public string ActionId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string CommandKind { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string CanonicalStepId { get; set; } = string.Empty;
    public int CanonicalStepIndex { get; set; } = -1;
    public int RuntimeCommandStartIndex { get; set; } = -1;
    public int RuntimeCommandEndIndex { get; set; } = -1;
    public string ExecutionTargetId { get; set; } = string.Empty;
    public bool ExecutionBindingValidated { get; set; }
    public IReadOnlyList<string> Prerequisites { get; set; } = new List<string>();
    public bool MayMutateState { get; set; }
    public bool Available { get; set; }
    public string UnavailableReason { get; set; } = string.Empty;
}

public sealed class SelectedRuntimeVariantInteractiveSessionStartRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string CandidateId { get; set; } = string.Empty;
    public string VariantKind { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string PackageSha256 { get; set; } = string.Empty;
    public CapabilityRuntimePlaythroughPlan? CapabilityPlan { get; set; }
}

public sealed class SelectedRuntimeVariantInteractiveActionRequest
{
    public string ActionRequestId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public int ActionIndex { get; set; }
    public string ActionId { get; set; } = string.Empty;
}

public sealed class SelectedRuntimeVariantInteractiveActionResult
{
    public string ActionRequestId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public int ActionIndex { get; set; }
    public string ActionId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string CommandKind { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string CanonicalStepId { get; set; } = string.Empty;
    public int CanonicalStepIndex { get; set; } = -1;
    public int RuntimeCommandStartIndex { get; set; } = -1;
    public int RuntimeCommandEndIndex { get; set; } = -1;
    public string ExecutionTargetId { get; set; } = string.Empty;
    public bool ExecutionBindingValidated { get; set; }
    public string StateHashBefore { get; set; } = string.Empty;
    public string StateHashAfter { get; set; } = string.Empty;
    public bool RuntimeExecuted { get; set; }
    public bool RuntimeMutation { get; set; }
    public int RuntimeEventCount { get; set; }
    public bool CorrelationPassed { get; set; }
    public string Status { get; set; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; set; } = new List<string>();
}

public sealed class SelectedRuntimeVariantInteractiveJournalEntry
{
    public string ActionRequestId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public int ActionIndex { get; set; }
    public string ActionId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string CommandKind { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string CanonicalStepId { get; set; } = string.Empty;
    public int CanonicalStepIndex { get; set; } = -1;
    public int RuntimeCommandStartIndex { get; set; } = -1;
    public int RuntimeCommandEndIndex { get; set; } = -1;
    public string ExecutionTargetId { get; set; } = string.Empty;
    public bool ExecutionBindingValidated { get; set; }
    public string StateHashBefore { get; set; } = string.Empty;
    public string StateHashAfter { get; set; } = string.Empty;
    public bool RuntimeExecuted { get; set; }
    public bool RuntimeMutation { get; set; }
    public int RuntimeEventCount { get; set; }
}

public sealed class SelectedRuntimeVariantInteractiveSession
{
    public string SessionId { get; set; } = string.Empty;
    public string CandidateId { get; set; } = string.Empty;
    public string VariantKind { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string PackageSha256 { get; set; } = string.Empty;
    public int CurrentActionIndex { get; set; }
    public int RuntimeCommandExecutionCount { get; set; }
    public int PresentationOnlyActionCount { get; set; }
    public string CurrentStateHash { get; set; } = "not_loaded";
    public bool RuntimeStarted { get; set; }
    public bool Completed { get; set; }
    public CapabilityRuntimePlaythroughPlan? CapabilityPlan { get; set; }
    public IReadOnlyList<SelectedRuntimeVariantActionDescriptor> AvailableActions { get; set; } =
        new List<SelectedRuntimeVariantActionDescriptor>();
    public List<SelectedRuntimeVariantInteractiveJournalEntry> ActionJournal { get; set; } = new();
    public CanonicalRuntimePlayerCommandLoopSnapshot LatestSnapshot { get; set; } = new();
    public string LatestMapSummary { get; set; } = string.Empty;
    public string LatestInventorySummary { get; set; } = string.Empty;
    public string LatestQuestSummary { get; set; } = string.Empty;
    public string LatestCombatSummary { get; set; } = string.Empty;
    public string LatestEquipmentSummary { get; set; } = string.Empty;
    public string LatestAttributesSummary { get; set; } = string.Empty;
    public string LatestProgressionSummary { get; set; } = string.Empty;
    public CanonicalRuntimePlayerCommandLoopSession CanonicalSession { get; set; } = new();
}

public sealed class SelectedRuntimeVariantInteractiveCheckpoint
{
    public string CheckpointId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string CandidateId { get; set; } = string.Empty;
    public string VariantKind { get; set; } = string.Empty;
    public string PackageSha256 { get; set; } = string.Empty;
    public string CapabilityPlanId { get; set; } = string.Empty;
    public string CapabilityPlanSignature { get; set; } = string.Empty;
    public IReadOnlyList<SelectedRuntimeVariantInteractiveJournalEntry> ActionJournal { get; set; } =
        new List<SelectedRuntimeVariantInteractiveJournalEntry>();
    public int RuntimeCommandExecutionCount { get; set; }
    public string ExpectedStateHash { get; set; } = string.Empty;
    public int ExpectedActionIndex { get; set; }
    public string MapSummary { get; set; } = string.Empty;
    public string InventorySummary { get; set; } = string.Empty;
    public string QuestSummary { get; set; } = string.Empty;
    public string CombatSummary { get; set; } = string.Empty;
    public string EquipmentSummary { get; set; } = string.Empty;
    public string AttributesSummary { get; set; } = string.Empty;
    public string ProgressionSummary { get; set; } = string.Empty;
    public string CreatedAtUtc { get; set; } = string.Empty;
}

public sealed class SelectedRuntimeVariantInteractiveReplayResult
{
    public bool Passed { get; set; }
    public bool PackageHashValidated { get; set; }
    public bool CandidateValidated { get; set; }
    public bool JournalCorrelationPassed { get; set; }
    public bool StateHashContinuityPassed { get; set; }
    public bool ExpectedStateHashMatched { get; set; }
    public string ExpectedStateHash { get; set; } = string.Empty;
    public string ActualStateHash { get; set; } = string.Empty;
    public int ReplayedActionCount { get; set; }
    public SelectedRuntimeVariantInteractiveSession Session { get; set; } = new();
    public IReadOnlyList<string> Diagnostics { get; set; } = new List<string>();
}

public interface ISelectedRuntimeVariantInteractiveSessionService
{
    SelectedRuntimeVariantInteractiveSession StartSession(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSessionStartRequest request);

    SelectedRuntimeVariantInteractiveActionResult ExecuteAction(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session,
        SelectedRuntimeVariantInteractiveActionRequest request);

    SelectedRuntimeVariantInteractiveCheckpoint SaveCheckpoint(
        SelectedRuntimeVariantInteractiveSession session,
        string checkpointId,
        string createdAtUtc);

    SelectedRuntimeVariantInteractiveReplayResult ReloadCheckpoint(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSessionStartRequest request,
        SelectedRuntimeVariantInteractiveCheckpoint checkpoint);
}
