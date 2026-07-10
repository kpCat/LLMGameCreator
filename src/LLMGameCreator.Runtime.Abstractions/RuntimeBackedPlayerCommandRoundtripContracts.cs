using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Runtime.Abstractions;

public sealed class RuntimeBackedPlayerCommandRoundtripRequest
{
    public string CandidateId { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string HandoffPath { get; set; } = string.Empty;
    public string ControlsUxModelPath { get; set; } = string.Empty;
    public string ControlsUxResultPath { get; set; } = string.Empty;
    public string ControlsUxScriptPath { get; set; } = string.Empty;
    public string CommandLoopSnapshotsPath { get; set; } = string.Empty;
    public string CommandLoopResultPath { get; set; } = string.Empty;
}

public sealed class RuntimeBackedPlayerCommandRoundtripInput
{
    public string CandidateId { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string HandoffPath { get; set; } = string.Empty;
    public string ControlsUxModelPath { get; set; } = string.Empty;
    public string ControlsUxResultPath { get; set; } = string.Empty;
    public string ControlsUxScriptPath { get; set; } = string.Empty;
    public string CommandLoopSnapshotsPath { get; set; } = string.Empty;
    public string CommandLoopResultPath { get; set; } = string.Empty;
    public bool PackagePathExists { get; set; }
    public bool HandoffPathExists { get; set; }
    public bool ControlsUxModelPathExists { get; set; }
    public bool ControlsUxResultPathExists { get; set; }
    public bool ControlsUxScriptPathExists { get; set; }
    public bool CommandLoopSnapshotsPathExists { get; set; }
    public bool CommandLoopResultPathExists { get; set; }
}

public sealed class RuntimeBackedPlayerCommandRoundtripControlRequest
{
    public int RequestIndex { get; set; }
    public string ControlIntent { get; set; } = string.Empty;
    public string SourceControlId { get; set; } = string.Empty;
    public string RuntimeCommandCoverage { get; set; } = string.Empty;
    public string RuntimeCommandKind { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public int CanonicalStepIndex { get; set; }
    public string CanonicalStepId { get; set; } = string.Empty;
    public bool RuntimeAuthority { get; set; } = true;
    public bool ProjectionOnly { get; set; }
    public bool UnityGameplayTruth { get; set; }
}

public sealed class RuntimeBackedPlayerCommandRoundtripSnapshot
{
    public int RequestIndex { get; set; }
    public string ControlIntent { get; set; } = string.Empty;
    public string RuntimeCommandCoverage { get; set; } = string.Empty;
    public int CanonicalStepIndex { get; set; }
    public string CanonicalStepId { get; set; } = string.Empty;
    public string StateHashBefore { get; set; } = string.Empty;
    public string StateHashAfter { get; set; } = string.Empty;
    public string MapSummary { get; set; } = string.Empty;
    public string VisibleInteractionSummary { get; set; } = string.Empty;
    public string DialogueSummary { get; set; } = string.Empty;
    public string QuestSummary { get; set; } = string.Empty;
    public string InventorySummary { get; set; } = string.Empty;
    public string CombatSummary { get; set; } = string.Empty;
    public int RuntimeEventCount { get; set; }
    public bool RuntimeAuthority { get; set; } = true;
    public bool ProjectionOnly { get; set; }
    public bool UnityGameplayTruth { get; set; }
}

public sealed class RuntimeBackedPlayerCommandRoundtripResponse
{
    public int RequestIndex { get; set; }
    public string ControlIntent { get; set; } = string.Empty;
    public string RuntimeCommandCoverage { get; set; } = string.Empty;
    public bool RuntimeExecuted { get; set; }
    public bool CanonicalStepRuntimeExecuted { get; set; }
    public RuntimeBackedPlayerCommandRoundtripSnapshot Snapshot { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public sealed class RuntimeBackedPlayerCommandRoundtripSession
{
    public string GoalId { get; set; } =
        "goal_141_runtime_backed_unity_player_command_roundtrip_bridge";
    public string CandidateId { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public int ExecutedRequestCount { get; set; }
    public int SnapshotCount { get; set; }
    public bool StateHashChainPresent { get; set; }
    public bool RuntimeAuthority { get; set; } = true;
    public bool ProjectionOnly { get; set; }
    public bool UnityGameplayTruth { get; set; }
    public bool ControlRequestBridgePresent { get; set; }
    public bool UnityConsumesRoundtripResult { get; set; } = true;
    public IReadOnlyList<string> StateHashChain { get; set; } = new List<string>();
    public IReadOnlyList<RuntimeBackedPlayerCommandRoundtripControlRequest> Requests { get; set; } =
        new List<RuntimeBackedPlayerCommandRoundtripControlRequest>();
    public IReadOnlyList<RuntimeBackedPlayerCommandRoundtripResponse> Responses { get; set; } =
        new List<RuntimeBackedPlayerCommandRoundtripResponse>();
}

public sealed class RuntimeBackedPlayerCommandRoundtripResult
{
    public string GoalId { get; set; } =
        "goal_141_runtime_backed_unity_player_command_roundtrip_bridge";
    public string CandidateId { get; set; } = string.Empty;
    public RuntimeBackedPlayerCommandRoundtripInput Inputs { get; set; } = new();
    public int RoundtripRequestCount { get; set; }
    public int RuntimeExecutedRequestCount { get; set; }
    public int RoundtripSnapshotCount { get; set; }
    public bool StateHashChainPresent { get; set; }
    public bool RuntimeAuthority { get; set; } = true;
    public bool ProjectionOnly { get; set; }
    public bool UnityGameplayTruth { get; set; }
    public bool ControlRequestBridgePresent { get; set; }
    public bool UnityConsumesRoundtripResult { get; set; } = true;
    public bool NoUnclassifiedErrorDiagnostics { get; set; }
    public bool RuntimeBackedPlayerCommandRoundtripPassed { get; set; }
    public IReadOnlyList<string> RequiredControlIntents { get; set; } = new List<string>();
    public IReadOnlyList<string> MissingControlIntents { get; set; } = new List<string>();
    public IReadOnlyList<string> RequiredRuntimeCommandCoverage { get; set; } = new List<string>();
    public IReadOnlyList<string> MissingRuntimeCommandCoverage { get; set; } = new List<string>();
    public IReadOnlyList<string> StateHashChain { get; set; } = new List<string>();
    public IReadOnlyList<RuntimeBackedPlayerCommandRoundtripControlRequest> Requests { get; set; } =
        new List<RuntimeBackedPlayerCommandRoundtripControlRequest>();
    public IReadOnlyList<RuntimeBackedPlayerCommandRoundtripResponse> Responses { get; set; } =
        new List<RuntimeBackedPlayerCommandRoundtripResponse>();
    public IReadOnlyList<RuntimeBackedPlayerCommandRoundtripSnapshot> Snapshots { get; set; } =
        new List<RuntimeBackedPlayerCommandRoundtripSnapshot>();
    public RuntimeBackedPlayerCommandRoundtripSession Session { get; set; } = new();
    public IReadOnlyList<string> Diagnostics { get; set; } = new List<string>();
}

public interface IRuntimeBackedPlayerCommandRoundtripService
{
    RuntimeBackedPlayerCommandRoundtripResult Execute(
        GamePackageDefinition package,
        RuntimeBackedPlayerCommandRoundtripRequest request);
}
