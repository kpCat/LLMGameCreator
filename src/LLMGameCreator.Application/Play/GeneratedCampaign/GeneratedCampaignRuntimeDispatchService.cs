using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed record GeneratedCampaignRuntimeDispatchResult
{
    public UnifiedRuntimeResult UnifiedRuntimeResult { get; init; } = new();
    public string CommandKind { get; init; } = string.Empty;
    public GameRuntimeCommandType? GameplayCommandType { get; init; }
    public PlayerCommandType? PlayerCommandType { get; init; }
    public string PackageSha256Before { get; init; } = string.Empty;
    public string PackageSha256After { get; init; } = string.Empty;
    public bool PackageReferencePreserved { get; init; }
    public IReadOnlyList<string> DefinitionIdsUsed { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool Passed => UnifiedRuntimeResult.Success
                          && PackageReferencePreserved
                          && string.Equals(PackageSha256Before, PackageSha256After,
                              StringComparison.Ordinal)
                          && !Diagnostics.Contains("campaign.package_mutated_during_dispatch",
                              StringComparer.Ordinal);
}

public sealed class GeneratedCampaignRuntimeDispatchService
{
    private readonly IUnifiedGameRuntimeService _runtime;

    public GeneratedCampaignRuntimeDispatchService(IUnifiedGameRuntimeService runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    public GeneratedCampaignRuntimeDispatchResult Dispatch(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        GeneratedCampaignPlannedAction planned)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(planned);

        if (planned.PlayerCommand is not null)
            return DispatchPlayer(package, session, planned.PlayerCommand);
        if (planned.RuntimeCommand is not null)
            return DispatchGameplay(package, session, planned.RuntimeCommand);
        return Rejected(package, session, "campaign.action_command_missing", string.Empty, null, null);
    }

    public GeneratedCampaignRuntimeDispatchResult DispatchGameplay(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        GameRuntimeCommand command)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(command);
        var validation = ValidateGameplayCommand(package, session, command);
        if (validation.Count > 0)
            return Rejected(package, session, validation[0], command.Type.ToString(), command.Type, null,
                DefinitionIds(command));

        var before = PackageSha256(package);
        var dispatchedPackage = package;
        var result = _runtime.ExecuteGameplayCommand(dispatchedPackage, session, command);
        var after = PackageSha256(package);
        var diagnostics = result.Diagnostics.Select(item => item.Code).ToList();
        if (!string.Equals(before, after, StringComparison.Ordinal))
            diagnostics.Add("campaign.package_mutated_during_dispatch");
        return new GeneratedCampaignRuntimeDispatchResult
        {
            UnifiedRuntimeResult = result,
            CommandKind = command.Type.ToString(),
            GameplayCommandType = command.Type,
            PackageSha256Before = before,
            PackageSha256After = after,
            PackageReferencePreserved = ReferenceEquals(package, dispatchedPackage),
            DefinitionIdsUsed = DefinitionIds(command),
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).ToList()
        };
    }

    public GeneratedCampaignRuntimeDispatchResult DispatchPlayer(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        PlayerCommand command)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(command);
        var before = PackageSha256(package);
        var dispatchedPackage = package;
        var result = _runtime.ExecutePlayerCommand(dispatchedPackage, session, command);
        var after = PackageSha256(package);
        var diagnostics = result.Diagnostics.Select(item => item.Code).ToList();
        if (!string.Equals(before, after, StringComparison.Ordinal))
            diagnostics.Add("campaign.package_mutated_during_dispatch");
        return new GeneratedCampaignRuntimeDispatchResult
        {
            UnifiedRuntimeResult = result,
            CommandKind = "PlayerCommand." + command.Type,
            PlayerCommandType = command.Type,
            PackageSha256Before = before,
            PackageSha256After = after,
            PackageReferencePreserved = ReferenceEquals(package, dispatchedPackage),
            DefinitionIdsUsed = string.IsNullOrWhiteSpace(command.TargetId) ? [] : [command.TargetId],
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).ToList()
        };
    }

    public static string PackageSha256(GamePackageDefinition package)
    {
        var json = JsonSerializer.Serialize(package);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
    }

    private static IReadOnlyList<string> ValidateGameplayCommand(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        GameRuntimeCommand command)
    {
        if (command.Type is not GameRuntimeCommandType.BasicAttack and not GameRuntimeCommandType.UseAbility)
            return [];
        var encounter = session.GameplayState.ActiveEncounter;
        if (encounter is not { Active: true }
            || !command.Args.TryGetValue("sourceParticipantId", out var sourceId)
            || string.IsNullOrWhiteSpace(sourceId))
            return ["campaign.ability_not_available"];
        var source = encounter.Participants.SingleOrDefault(item => IdEquals(item.Id, sourceId));
        var target = encounter.Participants.SingleOrDefault(item => IdEquals(item.Id, command.TargetId));
        if (source is not { Alive: true } || !KindEquals(source.Team, "player"))
            return ["campaign.ability_not_available"];
        if (target is not { Alive: true } || KindEquals(target.Team, source.Team))
            return ["campaign.ability_target_invalid"];
        var definition = package.Game.Encounters.SingleOrDefault(item =>
            IdEquals(item.Id, encounter.EncounterId));
        if (definition is null
            || definition.Participants.Count(item => IdEquals(item.Id, source.Id)) != 1
            || definition.Participants.Count(item => IdEquals(item.Id, target.Id)) != 1)
            return ["campaign.ability_target_invalid"];
        if (command.Type == GameRuntimeCommandType.BasicAttack) return [];
        if (string.IsNullOrWhiteSpace(command.Id)
            || package.Game.Abilities.Count(item => IdEquals(item.Id, command.Id)) != 1)
            return ["campaign.ability_not_available"];
        var sourceDefinition = definition.Participants.Single(item => IdEquals(item.Id, source.Id));
        return sourceDefinition.Abilities.Count(item => IdEquals(item, command.Id)) == 1
            ? []
            : ["campaign.ability_not_available"];
    }

    private static GeneratedCampaignRuntimeDispatchResult Rejected(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        string diagnostic,
        string commandKind,
        GameRuntimeCommandType? gameplayType,
        PlayerCommandType? playerType,
        IReadOnlyList<string>? definitionIds = null)
    {
        var hash = PackageSha256(package);
        return new GeneratedCampaignRuntimeDispatchResult
        {
            UnifiedRuntimeResult = new UnifiedRuntimeResult
            {
                Success = false,
                Session = session,
                Diagnostics =
                [
                    new RuntimeDiagnostic
                    {
                        Code = diagnostic,
                        Message = "Campaign action is not available for the exact package."
                    }
                ]
            },
            CommandKind = commandKind,
            GameplayCommandType = gameplayType,
            PlayerCommandType = playerType,
            PackageSha256Before = hash,
            PackageSha256After = hash,
            PackageReferencePreserved = true,
            DefinitionIdsUsed = definitionIds ?? [],
            Diagnostics = [diagnostic]
        };
    }

    private static IReadOnlyList<string> DefinitionIds(GameRuntimeCommand command)
    {
        var ids = new[]
            {
                command.Id,
                command.TargetId,
                command.InventoryId,
                command.Args.GetValueOrDefault("sourceParticipantId")
            }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        return ids;
    }

    private static bool IdEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static bool KindEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
