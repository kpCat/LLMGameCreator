using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class RuntimeBackedMicrogameStateAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/runtime-backed-microgame-state";
    public const string SnapshotJsonFileName = "runtime-backed-microgame-state-snapshot.json";
    public const string ReportMarkdownFileName = "runtime-backed-microgame-state-report.md";
    public const string ManualVerificationMarkdownFileName = "manual-runtime-backed-microgame-verification.md";
    public const string SnapshotSlotName = "runtime-backed-microgame-state";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IRuntimeStateSerializer? _runtimeStateSerializer;
    private readonly IRuntimeSnapshotStore? _runtimeSnapshotStore;

    public RuntimeBackedMicrogameStateAcceptanceService(
        IRuntimeStateSerializer? runtimeStateSerializer = null,
        IRuntimeSnapshotStore? runtimeSnapshotStore = null)
    {
        _runtimeStateSerializer = runtimeStateSerializer;
        _runtimeSnapshotStore = runtimeSnapshotStore;
    }

    public RuntimeBackedMicrogameStateAcceptanceResult Build(
        VisibleGeneratedPlayablePreviewResult visibleResult,
        string? projectRootPath = null)
    {
        ArgumentNullException.ThrowIfNull(visibleResult);

        var session = BuildSession(visibleResult);
        var persistence = BuildPersistenceEvidence(projectRootPath, session, visibleResult);
        var diagnostics = BuildDiagnostics(visibleResult, persistence);
        var snapshotWithoutHash = new RuntimeBackedMicrogameStateAcceptanceSnapshot
        {
            PackageId = visibleResult.Snapshot.PackageId,
            PackageTitle = visibleResult.Snapshot.PackageTitle,
            ActiveGoalId = visibleResult.Snapshot.MicrogameGoal.ActiveQuestId,
            ActiveGoalTitle = visibleResult.Snapshot.MicrogameGoal.ActiveQuestTitle,
            ObjectiveId = visibleResult.Snapshot.MicrogameGoal.RuntimeObjectiveId,
            ObjectiveText = visibleResult.Snapshot.MicrogameGoal.CurrentObjectiveText,
            ChallengeId = visibleResult.Snapshot.MicrogameChallenge.EncounterId,
            ChallengeTitle = visibleResult.Snapshot.MicrogameChallenge.EncounterTitle,
            RewardItemId = visibleResult.Snapshot.MicrogameChallenge.RewardItemId,
            RewardTitle = visibleResult.Snapshot.MicrogameChallenge.RewardTitle,
            RuntimeStartSucceeded = visibleResult.Report.RuntimeStartSucceeded,
            RuntimeMoveSucceeded = visibleResult.Snapshot.RuntimeAttempt.CommandAttempts.Any(item => string.Equals(item.CommandType, "move/right", StringComparison.OrdinalIgnoreCase) && item.Succeeded),
            RuntimeInteractSucceeded = visibleResult.Snapshot.RuntimeAttempt.CommandAttempts.Any(item => string.Equals(item.CommandType, "interact", StringComparison.OrdinalIgnoreCase) && item.Succeeded),
            ActiveGoalVisible = visibleResult.Report.ActiveGoalSelected,
            ProgressAdvanced = visibleResult.Report.GoalProgressAdvanced,
            GoalProgressStateSource = visibleResult.Snapshot.MicrogameGoal.ProgressStateSource,
            RuntimeGoalQuestId = visibleResult.Snapshot.MicrogameGoal.RuntimeQuestId,
            RuntimeGoalObjectiveId = visibleResult.Snapshot.MicrogameGoal.RuntimeObjectiveId,
            RuntimeGoalObjectiveCurrentAmount = visibleResult.Snapshot.MicrogameGoal.RuntimeObjectiveCurrentAmount,
            RuntimeGoalObjectiveRequiredAmount = visibleResult.Snapshot.MicrogameGoal.RuntimeObjectiveRequiredAmount,
            RuntimeGoalObjectiveCompleted = visibleResult.Snapshot.MicrogameGoal.RuntimeObjectiveCompleted,
            GoalProgressFallbackPreviewJournalUsed = visibleResult.Snapshot.MicrogameGoal.FallbackPreviewJournalUsed,
            ChallengeResolved = visibleResult.Report.ChallengeResolved,
            ChallengeStateSource = visibleResult.Snapshot.MicrogameChallenge.StateSource,
            RuntimeChallengeEncounterId = visibleResult.Snapshot.MicrogameChallenge.RuntimeEncounterId,
            RuntimeChallengeFlagId = visibleResult.Snapshot.MicrogameChallenge.RuntimeChallengeFlagId,
            RuntimeRewardItemId = visibleResult.Snapshot.MicrogameChallenge.RuntimeRewardItemId,
            RuntimeRewardAmount = visibleResult.Snapshot.MicrogameChallenge.RuntimeRewardAmount,
            RuntimeRewardGranted = visibleResult.Snapshot.MicrogameChallenge.RuntimeRewardGranted,
            RuntimeCompletionBacked = visibleResult.Snapshot.MicrogameChallenge.RuntimeCompletionBacked,
            ChallengeFallbackPreviewProjectionUsed = visibleResult.Snapshot.MicrogameChallenge.FallbackPreviewProjectionUsed,
            RewardVisible = visibleResult.Report.RewardVisible,
            CompletionVisible = visibleResult.Report.CompletionVisible,
            CompletionStatus = visibleResult.Snapshot.MicrogameChallenge.CompletionStatus,
            Persistence = persistence,
            Diagnostics = diagnostics
        };
        var hash = ComputeHash(JsonSerializer.Serialize(snapshotWithoutHash, JsonOptions));
        var snapshot = snapshotWithoutHash with { DeterministicHash = hash };
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);

        return new RuntimeBackedMicrogameStateAcceptanceResult
        {
            Snapshot = snapshot,
            SnapshotJson = json,
            ReportMarkdown = RenderReport(snapshot),
            ManualVerificationMarkdown = RenderManualVerification(snapshot)
        };
    }

    public async Task<RuntimeBackedMicrogameStateAcceptanceWriteResult> WriteAsync(
        string projectRootPath,
        RuntimeBackedMicrogameStateAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "runtime-backed-microgame-state"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var snapshotJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, SnapshotJsonFileName));
        var reportMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportMarkdownFileName));
        var manualVerificationMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ManualVerificationMarkdownFileName));
        EnsureContained(outputDirectory, snapshotJsonPath);
        EnsureContained(outputDirectory, reportMarkdownPath);
        EnsureContained(outputDirectory, manualVerificationMarkdownPath);

        await File.WriteAllTextAsync(snapshotJsonPath, result.SnapshotJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(manualVerificationMarkdownPath, result.ManualVerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new RuntimeBackedMicrogameStateAcceptanceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            SnapshotJsonPath = snapshotJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            ManualVerificationMarkdownPath = manualVerificationMarkdownPath
        };
    }

    private RuntimeBackedMicrogamePersistenceEvidence BuildPersistenceEvidence(
        string? projectRootPath,
        UnifiedRuntimeSession session,
        VisibleGeneratedPlayablePreviewResult visibleResult)
    {
        var evidence = new RuntimeBackedMicrogamePersistenceEvidence
        {
            SerializerAvailable = _runtimeStateSerializer != null,
            SnapshotStoreAvailable = _runtimeSnapshotStore != null,
            SnapshotSlotName = SnapshotSlotName
        };

        if (_runtimeStateSerializer != null)
        {
            var restored = _runtimeStateSerializer.DeserializeUnifiedSession(_runtimeStateSerializer.Serialize(session));
            evidence = evidence with
            {
                SerializationRoundtripSucceeded = MatchesRuntimeEvidence(restored, visibleResult)
            };
        }

        if (_runtimeSnapshotStore != null && !string.IsNullOrWhiteSpace(projectRootPath))
        {
            var save = _runtimeSnapshotStore.SaveSnapshot(projectRootPath, SnapshotSlotName, session);
            var load = save.Success
                ? _runtimeSnapshotStore.LoadSnapshot(projectRootPath, SnapshotSlotName)
                : new RuntimeSnapshotResult { Success = false, Message = save.Message, SlotName = SnapshotSlotName };
            evidence = evidence with
            {
                SnapshotSaveSucceeded = save.Success,
                SnapshotLoadSucceeded = load.Success && load.Session != null && MatchesRuntimeEvidence(load.Session, visibleResult),
                SnapshotSlotName = save.SlotName ?? load.SlotName ?? SnapshotSlotName
            };
        }

        return evidence;
    }

    private static UnifiedRuntimeSession BuildSession(VisibleGeneratedPlayablePreviewResult visibleResult)
    {
        var runtimeAttempt = visibleResult.Snapshot.RuntimeAttempt;
        var runtimeState = visibleResult.Snapshot.MicrogameChallenge.RuntimeState;
        if (string.IsNullOrWhiteSpace(runtimeState.PackageId))
        {
            runtimeState = visibleResult.Snapshot.MicrogameGoal.RuntimeState;
        }

        return new UnifiedRuntimeSession
        {
            MapState = new GameState
            {
                CurrentMapId = runtimeAttempt.CurrentMapId,
                PlayerPosition = new LLMGameCreator.Domain.Definitions.Position2D
                {
                    X = runtimeAttempt.PlayerCurrentPosition.X,
                    Y = runtimeAttempt.PlayerCurrentPosition.Y
                },
                Mode = "map",
                Flags = new Dictionary<string, string>
                {
                    ["packageId"] = visibleResult.Snapshot.PackageId,
                    ["goalProgressSource"] = visibleResult.Snapshot.MicrogameGoal.ProgressStateSource,
                    ["challengeStateSource"] = visibleResult.Snapshot.MicrogameChallenge.StateSource
                }
            },
            GameplayState = runtimeState,
            Metadata = new Dictionary<string, string>
            {
                ["acceptance"] = "runtime-backed-microgame-state",
                ["packageId"] = visibleResult.Snapshot.PackageId
            }
        };
    }

    private static bool MatchesRuntimeEvidence(
        UnifiedRuntimeSession session,
        VisibleGeneratedPlayablePreviewResult visibleResult)
    {
        var goal = visibleResult.Snapshot.MicrogameGoal;
        var challenge = visibleResult.Snapshot.MicrogameChallenge;
        var objective = session.GameplayState.Quests
            .FirstOrDefault(item => string.Equals(item.QuestId, goal.RuntimeQuestId, StringComparison.Ordinal))?
            .Objectives
            .FirstOrDefault(item => string.Equals(item.ObjectiveId, goal.RuntimeObjectiveId, StringComparison.Ordinal));
        var rewardStack = session.GameplayState.Inventories
            .SelectMany(item => item.Stacks)
            .FirstOrDefault(item => string.Equals(item.ItemId, challenge.RuntimeRewardItemId, StringComparison.Ordinal));
        var challengeFlag = session.GameplayState.Flags
            .FirstOrDefault(item => string.Equals(item.Id, challenge.RuntimeChallengeFlagId, StringComparison.Ordinal));

        return objective != null
            && objective.CurrentAmount.Equals(goal.RuntimeObjectiveCurrentAmount)
            && objective.RequiredAmount.Equals(goal.RuntimeObjectiveRequiredAmount)
            && objective.Completed == goal.RuntimeObjectiveCompleted
            && challengeFlag != null
            && string.Equals(challengeFlag.Value, "true", StringComparison.OrdinalIgnoreCase)
            && rewardStack != null
            && rewardStack.Amount.Equals(challenge.RuntimeRewardAmount)
            && session.GameplayState.ActiveEncounter != null
            && string.Equals(session.GameplayState.ActiveEncounter.EncounterId, challenge.RuntimeEncounterId, StringComparison.Ordinal)
            && !session.GameplayState.ActiveEncounter.Active;
    }

    private static IReadOnlyList<RuntimeBackedMicrogameStateAcceptanceDiagnostic> BuildDiagnostics(
        VisibleGeneratedPlayablePreviewResult visibleResult,
        RuntimeBackedMicrogamePersistenceEvidence persistence)
    {
        var diagnostics = visibleResult.Report.Diagnostics
            .Select(item => new RuntimeBackedMicrogameStateAcceptanceDiagnostic
            {
                Severity = item.Severity,
                Code = item.Code,
                Target = item.Target,
                Message = item.Message
            })
            .Concat(new[]
            {
                Diagnostic("info", "runtime_backed_microgame_state.no_external_execution", visibleResult.Snapshot.PackageId, "No LLM, provider, Lua, Unity or media execution was invoked."),
                Diagnostic("info", "runtime_backed_microgame_state.manual_verification_required", "manual_runtime_backed_microgame_verification", "Codex acceptance is headless; the next step is manual runtime-backed microgame verification."),
                Diagnostic(
                    persistence.SerializationRoundtripSucceeded ? "info" : "warning",
                    persistence.SerializationRoundtripSucceeded ? "runtime_backed_microgame_state.serialization_roundtrip_passed" : "runtime_backed_microgame_state.serialization_roundtrip_unavailable",
                    "runtime_state_serializer",
                    persistence.SerializationRoundtripSucceeded ? "Runtime-backed state evidence survived serializer roundtrip." : "Runtime serializer roundtrip was unavailable or did not preserve the selected evidence."),
                Diagnostic(
                    persistence.SnapshotSaveSucceeded && persistence.SnapshotLoadSucceeded ? "info" : "warning",
                    persistence.SnapshotSaveSucceeded && persistence.SnapshotLoadSucceeded ? "runtime_backed_microgame_state.snapshot_store_roundtrip_passed" : "runtime_backed_microgame_state.snapshot_store_roundtrip_unavailable",
                    persistence.SnapshotSlotName,
                    persistence.SnapshotSaveSucceeded && persistence.SnapshotLoadSucceeded ? "Runtime-backed state evidence survived snapshot save/load." : "Runtime snapshot save/load was unavailable or did not preserve the selected evidence.")
            })
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

        return diagnostics;
    }

    private static string RenderReport(RuntimeBackedMicrogameStateAcceptanceSnapshot snapshot)
    {
        var lines = new List<string>
        {
            "# Runtime-Backed Microgame State Acceptance",
            string.Empty,
            "- Deterministic: true",
            "- External execution: none",
            $"- Snapshot hash: `{snapshot.DeterministicHash}`",
            string.Empty,
            "## Runtime Evidence",
            string.Empty,
            $"- Runtime start: `{snapshot.RuntimeStartSucceeded.ToString().ToLowerInvariant()}`",
            $"- Movement: `{snapshot.RuntimeMoveSucceeded.ToString().ToLowerInvariant()}`",
            $"- Interaction: `{snapshot.RuntimeInteractSucceeded.ToString().ToLowerInvariant()}`",
            $"- Active goal visible: `{snapshot.ActiveGoalVisible.ToString().ToLowerInvariant()}`",
            $"- Progress source: `{FirstNonEmpty(snapshot.GoalProgressStateSource, "none")}`",
            $"- Runtime quest/objective: `{FirstNonEmpty(snapshot.RuntimeGoalQuestId, "none")}` / `{FirstNonEmpty(snapshot.RuntimeGoalObjectiveId, "none")}`",
            $"- Runtime progress: `{snapshot.RuntimeGoalObjectiveCurrentAmount:0.##}/{snapshot.RuntimeGoalObjectiveRequiredAmount:0.##}`",
            $"- Goal fallback used: `{snapshot.GoalProgressFallbackPreviewJournalUsed.ToString().ToLowerInvariant()}`",
            $"- Challenge source: `{FirstNonEmpty(snapshot.ChallengeStateSource, "none")}`",
            $"- Runtime challenge flag: `{FirstNonEmpty(snapshot.RuntimeChallengeFlagId, "none")}`",
            $"- Runtime reward: `{FirstNonEmpty(snapshot.RuntimeRewardItemId, "none")}` x`{snapshot.RuntimeRewardAmount:0.##}`",
            $"- Runtime reward granted: `{snapshot.RuntimeRewardGranted.ToString().ToLowerInvariant()}`",
            $"- Runtime completion backed: `{snapshot.RuntimeCompletionBacked.ToString().ToLowerInvariant()}`",
            $"- Challenge fallback used: `{snapshot.ChallengeFallbackPreviewProjectionUsed.ToString().ToLowerInvariant()}`",
            string.Empty,
            "## Persistence Evidence",
            string.Empty,
            $"- Serializer available: `{snapshot.Persistence.SerializerAvailable.ToString().ToLowerInvariant()}`",
            $"- Serializer roundtrip: `{snapshot.Persistence.SerializationRoundtripSucceeded.ToString().ToLowerInvariant()}`",
            $"- Snapshot store available: `{snapshot.Persistence.SnapshotStoreAvailable.ToString().ToLowerInvariant()}`",
            $"- Snapshot slot: `{snapshot.Persistence.SnapshotSlotName}`",
            $"- Snapshot save: `{snapshot.Persistence.SnapshotSaveSucceeded.ToString().ToLowerInvariant()}`",
            $"- Snapshot load: `{snapshot.Persistence.SnapshotLoadSucceeded.ToString().ToLowerInvariant()}`",
            string.Empty,
            "## Player-Facing Labels",
            string.Empty,
            $"- Package: `{snapshot.PackageTitle}` / `{snapshot.PackageId}`",
            $"- Active goal: `{FirstNonEmpty(snapshot.ActiveGoalTitle, snapshot.ActiveGoalId, "none")}`",
            $"- Objective: `{FirstNonEmpty(snapshot.ObjectiveText, snapshot.ObjectiveId, "none")}`",
            $"- Challenge: `{FirstNonEmpty(snapshot.ChallengeTitle, snapshot.ChallengeId, "none")}`",
            $"- Reward: `{FirstNonEmpty(snapshot.RewardTitle, snapshot.RewardItemId, "none")}`",
            $"- Completion: `{FirstNonEmpty(snapshot.CompletionStatus, "none")}`",
            string.Empty,
            "## Diagnostics",
            string.Empty
        };

        lines.AddRange(snapshot.Diagnostics.Count == 0
            ? ["- None"]
            : snapshot.Diagnostics.Select(item => $"- `{item.Severity}` `{item.Code}` target=`{item.Target}`: {item.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    private static string RenderManualVerification(RuntimeBackedMicrogameStateAcceptanceSnapshot snapshot)
    {
        var lines = new List<string>
        {
            "# Manual Runtime-Backed Microgame Verification",
            string.Empty,
            "Use this after Product Slice 040. Codex does not perform this manual UI check.",
            string.Empty,
            "1. Start `LLMGameCreator.WinForms`.",
            "2. Open Runtime Preview.",
            "3. Click `Generate Preview`.",
            "4. Click `Start`.",
            "5. Confirm the active goal is readable and backed by runtime quest/objective state.",
            "6. Move to the generated NPC/object/item marker and use the existing interaction command.",
            "7. Confirm interaction advances runtime-owned goal progress.",
            "8. Confirm challenge resolution, reward and completion show runtime-backed state evidence.",
            "9. If snapshot controls are available, save and reload the generated runtime state.",
            string.Empty,
            "Expected runtime-backed evidence:",
            string.Empty,
            $"- Package: `{snapshot.PackageTitle}` / `{snapshot.PackageId}`",
            $"- Active goal: `{FirstNonEmpty(snapshot.ActiveGoalTitle, snapshot.ActiveGoalId, "none")}`",
            $"- Runtime goal source: `{FirstNonEmpty(snapshot.GoalProgressStateSource, "none")}`",
            $"- Runtime quest/objective: `{FirstNonEmpty(snapshot.RuntimeGoalQuestId, "none")}` / `{FirstNonEmpty(snapshot.RuntimeGoalObjectiveId, "none")}`",
            $"- Challenge source: `{FirstNonEmpty(snapshot.ChallengeStateSource, "none")}`",
            $"- Reward: `{FirstNonEmpty(snapshot.RuntimeRewardItemId, snapshot.RewardItemId, "none")}` x`{snapshot.RuntimeRewardAmount:0.##}`",
            $"- Snapshot serializer roundtrip: `{snapshot.Persistence.SerializationRoundtripSucceeded.ToString().ToLowerInvariant()}`",
            $"- Snapshot store roundtrip: `{snapshot.Persistence.SnapshotSaveSucceeded.ToString().ToLowerInvariant()}` / `{snapshot.Persistence.SnapshotLoadSucceeded.ToString().ToLowerInvariant()}`",
            string.Empty,
            "Headless evidence already proved deterministic runtime state backing and existing snapshot persistence roundtrip when available."
        };

        return string.Join("\n", lines) + "\n";
    }

    private static RuntimeBackedMicrogameStateAcceptanceDiagnostic Diagnostic(
        string severity,
        string code,
        string target,
        string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Runtime-backed microgame state output path must stay under the project root.");
        }
    }
}

public sealed record RuntimeBackedMicrogameStateAcceptanceResult
{
    public RuntimeBackedMicrogameStateAcceptanceSnapshot Snapshot { get; init; } = new();
    public string SnapshotJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string ManualVerificationMarkdown { get; init; } = string.Empty;
}

public sealed record RuntimeBackedMicrogameStateAcceptanceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string SnapshotJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string ManualVerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record RuntimeBackedMicrogameStateAcceptanceSnapshot
{
    public string SchemaVersion { get; init; } = "1";
    public string DeterministicHash { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string ActiveGoalId { get; init; } = string.Empty;
    public string ActiveGoalTitle { get; init; } = string.Empty;
    public string ObjectiveId { get; init; } = string.Empty;
    public string ObjectiveText { get; init; } = string.Empty;
    public string ChallengeId { get; init; } = string.Empty;
    public string ChallengeTitle { get; init; } = string.Empty;
    public string RewardItemId { get; init; } = string.Empty;
    public string RewardTitle { get; init; } = string.Empty;
    public bool RuntimeStartSucceeded { get; init; }
    public bool RuntimeMoveSucceeded { get; init; }
    public bool RuntimeInteractSucceeded { get; init; }
    public bool ActiveGoalVisible { get; init; }
    public bool ProgressAdvanced { get; init; }
    public string GoalProgressStateSource { get; init; } = string.Empty;
    public string RuntimeGoalQuestId { get; init; } = string.Empty;
    public string RuntimeGoalObjectiveId { get; init; } = string.Empty;
    public double RuntimeGoalObjectiveCurrentAmount { get; init; }
    public double RuntimeGoalObjectiveRequiredAmount { get; init; }
    public bool RuntimeGoalObjectiveCompleted { get; init; }
    public bool GoalProgressFallbackPreviewJournalUsed { get; init; }
    public bool ChallengeResolved { get; init; }
    public string ChallengeStateSource { get; init; } = string.Empty;
    public string RuntimeChallengeEncounterId { get; init; } = string.Empty;
    public string RuntimeChallengeFlagId { get; init; } = string.Empty;
    public string RuntimeRewardItemId { get; init; } = string.Empty;
    public double RuntimeRewardAmount { get; init; }
    public bool RuntimeRewardGranted { get; init; }
    public bool RuntimeCompletionBacked { get; init; }
    public bool ChallengeFallbackPreviewProjectionUsed { get; init; }
    public bool RewardVisible { get; init; }
    public bool CompletionVisible { get; init; }
    public string CompletionStatus { get; init; } = string.Empty;
    public RuntimeBackedMicrogamePersistenceEvidence Persistence { get; init; } = new();
    public IReadOnlyList<RuntimeBackedMicrogameStateAcceptanceDiagnostic> Diagnostics { get; init; } = Array.Empty<RuntimeBackedMicrogameStateAcceptanceDiagnostic>();
}

public sealed record RuntimeBackedMicrogamePersistenceEvidence
{
    public bool SerializerAvailable { get; init; }
    public bool SerializationRoundtripSucceeded { get; init; }
    public bool SnapshotStoreAvailable { get; init; }
    public bool SnapshotSaveSucceeded { get; init; }
    public bool SnapshotLoadSucceeded { get; init; }
    public string SnapshotSlotName { get; init; } = string.Empty;
}

public sealed record RuntimeBackedMicrogameStateAcceptanceDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
