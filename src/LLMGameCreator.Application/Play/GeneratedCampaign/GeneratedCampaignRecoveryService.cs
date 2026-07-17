using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed record GeneratedCampaignRecoveryCheckpoint
{
    public GeneratedCampaignProjectTruth Truth { get; init; } = new();
    public string ProjectIdentityFingerprint { get; init; } = string.Empty;
    public string WorldId { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public string EncounterId { get; init; } = string.Empty;
    public string EncounterTitle { get; init; } = string.Empty;
    public string PreEncounterSessionJson { get; init; } = string.Empty;
    public UnifiedRuntimeSession PreEncounterSession { get; init; } = new();
    public string PreEncounterSessionSha256 { get; init; } = string.Empty;
    public string MapStateSha256 { get; init; } = string.Empty;
    public string GameplayStateSha256 { get; init; } = string.Empty;
    public string CreatedFromActionId { get; init; } = string.Empty;
    public bool Invalidated { get; init; }
}

public sealed record GeneratedCampaignRecoveryState
{
    public GeneratedCampaignRecoveryCheckpoint? Checkpoint { get; init; }
    public bool Stale { get; init; }
}

public sealed record GeneratedCampaignRecoveryValidation
{
    public bool Passed { get; init; }
    public bool Stale { get; init; }
    public string HumanReason { get; init; } = string.Empty;
    public UnifiedRuntimeSession? Session { get; init; }
}

public sealed class GeneratedCampaignRecoveryService
{
    public const string RetryActionId = "campaign-recovery-retry";
    public const string ContinueActionId = "campaign-recovery-continue";
    public const string NewGameActionId = "campaign-recovery-new-game";

    public GeneratedCampaignRecoveryCheckpoint? Checkpoint { get; private set; }

    public GeneratedCampaignRecoveryCheckpoint Prepare(
        GeneratedCampaignProjectTruth truth,
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        string encounterId,
        string encounterTitle,
        string createdFromActionId = "")
    {
        ArgumentNullException.ThrowIfNull(truth);
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(session);
        var json = Serialize(session);
        var copied = Deserialize(json);
        if (!string.Equals(SessionSha(session), SessionSha(copied), StringComparison.Ordinal))
            throw new InvalidOperationException("campaign.recovery_checkpoint_roundtrip_failed");
        return new GeneratedCampaignRecoveryCheckpoint
        {
            Truth = truth,
            ProjectIdentityFingerprint = truth.ProjectIdentityFingerprint,
            WorldId = truth.WorldId,
            PackageSha256 = PackageSha(package),
            CompositionPackageSha256 = truth.CompositionPackageSha256,
            QualifiedAuthoringFingerprint = truth.QualifiedAuthoringFingerprint,
            EncounterId = encounterId,
            EncounterTitle = encounterTitle,
            PreEncounterSessionJson = json,
            PreEncounterSession = copied,
            PreEncounterSessionSha256 = SessionSha(copied),
            MapStateSha256 = Hash(session.MapState),
            GameplayStateSha256 = Hash(session.GameplayState),
            CreatedFromActionId = createdFromActionId
        };
    }

    public void Commit(GeneratedCampaignRecoveryCheckpoint prepared) =>
        Checkpoint = prepared ?? throw new ArgumentNullException(nameof(prepared));

    public void Clear() => Checkpoint = null;

    public void Invalidate() =>
        Checkpoint = Checkpoint is null ? null : Checkpoint with { Invalidated = true };

    public GeneratedCampaignRecoveryValidation Restore(
        GeneratedCampaignProjectTruth? truth,
        GamePackageDefinition? package)
    {
        if (Checkpoint is null)
            return new GeneratedCampaignRecoveryValidation { HumanReason = "Нет сохранённой точки перед встречей." };
        if (Checkpoint.Invalidated || truth is null || package is null
            || !GeneratedCampaignSessionTruthService.Same(Checkpoint.Truth, truth)
            || !CheckpointTruthExact(Checkpoint, truth)
            || !string.Equals(Checkpoint.PackageSha256, PackageSha(package), StringComparison.Ordinal)
            || !CheckpointSessionExact(Checkpoint))
        {
            Invalidate();
            return new GeneratedCampaignRecoveryValidation
            {
                Stale = true,
                HumanReason = "Мир кампании изменился; повторить эту встречу нельзя."
            };
        }

        return new GeneratedCampaignRecoveryValidation
        {
            Passed = true,
            Session = Deserialize(Checkpoint.PreEncounterSessionJson)
        };
    }

    public GeneratedCampaignRecoveryProjection Project(bool canContinue, string continueReason)
    {
        if (Checkpoint is null) return new GeneratedCampaignRecoveryProjection();
        var retryEnabled = !Checkpoint.Invalidated;
        var disabled = !retryEnabled
            ? "Точка перед встречей устарела из-за изменения мира."
            : !canContinue ? continueReason : string.Empty;
        return new GeneratedCampaignRecoveryProjection
        {
            Available = true,
            EncounterTitle = Checkpoint.EncounterTitle,
            RetryEnabled = retryEnabled,
            ContinueEnabled = canContinue,
            NewGameEnabled = true,
            DisabledReason = disabled
        };
    }

    public IReadOnlyList<GeneratedCampaignAction> RecoveryActions(
        GeneratedCampaignRecoveryProjection recovery) =>
    [
        new GeneratedCampaignAction
        {
            ActionId = RetryActionId,
            Kind = GeneratedCampaignActionKind.RetryEncounter,
            Title = "Повторить встречу",
            Description = "Вернуться к точке непосредственно перед началом этой встречи.",
            Enabled = recovery.RetryEnabled,
            DisabledReason = recovery.RetryEnabled ? string.Empty : recovery.DisabledReason,
            Primary = true,
            TargetTitle = recovery.EncounterTitle
        },
        new GeneratedCampaignAction
        {
            ActionId = ContinueActionId,
            Kind = GeneratedCampaignActionKind.RecoveryLoad,
            Title = "Продолжить с сохранения",
            Description = recovery.ContinueEnabled
                ? "Восстановить текущее сохранение без запуска новой игры."
                : "Продолжение недоступно: нет совместимого сохранения.",
            Enabled = recovery.ContinueEnabled,
            DisabledReason = recovery.ContinueEnabled ? string.Empty : recovery.DisabledReason,
            TargetTitle = recovery.EncounterTitle
        },
        new GeneratedCampaignAction
        {
            ActionId = NewGameActionId,
            Kind = GeneratedCampaignActionKind.NewGame,
            Title = "Начать новую игру",
            Description = "Начать кампанию заново с её стартовой карты.",
            Enabled = recovery.NewGameEnabled,
            DisabledReason = recovery.NewGameEnabled ? string.Empty : recovery.DisabledReason,
            TargetTitle = recovery.EncounterTitle
        }
    ];

    public static bool IsDefeat(UnifiedRuntimeSession session)
    {
        var encounter = session.GameplayState.ActiveEncounter;
        return encounter is { Active: false }
               && encounter.Participants.Any(item => IsPlayer(item.Team))
               && encounter.Participants.Where(item => IsPlayer(item.Team)).All(item => !item.Alive);
    }

    public static bool IsVictory(UnifiedRuntimeSession session)
    {
        var encounter = session.GameplayState.ActiveEncounter;
        return encounter is { Active: false }
               && encounter.Participants.Any(item => IsPlayer(item.Team) && item.Alive)
               && encounter.Participants.Where(item => !IsPlayer(item.Team)).All(item => !item.Alive);
    }

    public static string SessionSha(UnifiedRuntimeSession session) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(Serialize(session)))).ToLowerInvariant();

    private static string Serialize(UnifiedRuntimeSession session) => JsonSerializer.Serialize(session);

    private static UnifiedRuntimeSession Deserialize(string json) =>
        JsonSerializer.Deserialize<UnifiedRuntimeSession>(json)
        ?? throw new InvalidOperationException("campaign.recovery_checkpoint_copy_failed");

    private static bool CheckpointTruthExact(
        GeneratedCampaignRecoveryCheckpoint checkpoint,
        GeneratedCampaignProjectTruth truth) =>
        checkpoint.ProjectIdentityFingerprint == truth.ProjectIdentityFingerprint
        && checkpoint.WorldId == truth.WorldId
        && checkpoint.CompositionPackageSha256 == truth.CompositionPackageSha256
        && checkpoint.QualifiedAuthoringFingerprint == truth.QualifiedAuthoringFingerprint;

    private static bool CheckpointSessionExact(GeneratedCampaignRecoveryCheckpoint checkpoint)
    {
        try
        {
            var session = Deserialize(checkpoint.PreEncounterSessionJson);
            return string.Equals(checkpoint.PreEncounterSessionSha256, SessionSha(session), StringComparison.Ordinal)
                   && string.Equals(checkpoint.MapStateSha256, Hash(session.MapState), StringComparison.Ordinal)
                   && string.Equals(checkpoint.GameplayStateSha256, Hash(session.GameplayState), StringComparison.Ordinal);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string Hash<T>(T value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)))).ToLowerInvariant();

    private static string PackageSha(GamePackageDefinition package) =>
        GeneratedCampaignRuntimeDispatchService.PackageSha256(package);

    private static bool IsPlayer(string? team) =>
        string.Equals(team, "player", StringComparison.OrdinalIgnoreCase);
}
