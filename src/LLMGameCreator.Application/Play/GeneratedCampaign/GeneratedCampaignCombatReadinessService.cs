using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed record GeneratedCampaignCombatReadiness
{
    public string EncounterId { get; init; } = string.Empty;
    public bool Playable { get; init; }
    public bool BasicAttackAvailable { get; init; }
    public IReadOnlyList<string> AbilityIds { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class GeneratedCampaignCombatReadinessService
{
    public GeneratedCampaignCombatReadiness Evaluate(
        GamePackageDefinition package,
        EncounterDefinition? encounter)
    {
        ArgumentNullException.ThrowIfNull(package);
        if (encounter is null
            || package.Game.Encounters.Count(item => IdEquals(item.Id, encounter.Id)) != 1)
            return NotReady(encounter?.Id, "campaign.encounter_definition_missing");

        var players = encounter.Participants.Where(item => KindEquals(item.Team, "player")).ToList();
        var opponents = encounter.Participants.Where(item => !KindEquals(item.Team, "player")).ToList();
        if (players.Count == 0 || opponents.Count == 0)
            return NotReady(encounter.Id, "campaign.encounter_participants_invalid");

        var invalidResource = encounter.Participants.SelectMany(item => item.Resources)
            .FirstOrDefault(resource => package.Game.Resources.Count(definition =>
                IdEquals(definition.Id, resource.Id)) != 1);
        if (invalidResource is not null)
            return NotReady(encounter.Id, "campaign.encounter_resource_definition_missing");

        var basicAttack = players.Any(player => opponents.Any(opponent =>
            BasicAttackCanDamage(package, encounter, player, opponent)));
        var abilities = players.SelectMany(player => player.Abilities.Select(id => (Player: player, Id: id)))
            .Where(candidate => package.Game.Abilities.Count(ability =>
                IdEquals(ability.Id, candidate.Id)) == 1)
            .Where(candidate => opponents.Any(opponent => AbilityCanExecute(
                package,
                package.Game.Abilities.Single(ability => IdEquals(ability.Id, candidate.Id)),
                candidate.Player,
                opponent)))
            .Select(candidate => candidate.Id)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        if (!basicAttack && abilities.Count == 0)
            return NotReady(encounter.Id, "campaign.encounter_no_executable_player_action");
        return new GeneratedCampaignCombatReadiness
        {
            EncounterId = encounter.Id,
            Playable = true,
            BasicAttackAvailable = basicAttack,
            AbilityIds = abilities
        };
    }

    private static bool BasicAttackCanDamage(
        GamePackageDefinition package,
        EncounterDefinition encounter,
        EncounterParticipantDefinition source,
        EncounterParticipantDefinition target)
    {
        var ability = ResolveBasicAttackAbility(package, encounter, source);
        IReadOnlyList<string> resourceIds = ability is null
            ? ["resource/health"]
            : DamageResourceIds(ability).ToList();
        return resourceIds.Count > 0 && resourceIds.Any(resourceId =>
            target.Resources.Any(resource => IdEquals(resource.Id, resourceId)));
    }

    private static AbilityDefinition? ResolveBasicAttackAbility(
        GamePackageDefinition package,
        EncounterDefinition encounter,
        EncounterParticipantDefinition source)
    {
        if (encounter.Metadata.TryGetValue("default_attack_ability_id", out var defaultId))
            return package.Game.Abilities.FirstOrDefault(item => IdEquals(item.Id, defaultId));
        foreach (var abilityId in source.Abilities)
        {
            var ability = package.Game.Abilities.FirstOrDefault(item => IdEquals(item.Id, abilityId)
                && item.Tags.Any(tag => KindEquals(tag, "basic_attack")));
            if (ability is not null) return ability;
        }
        foreach (var abilityId in source.Abilities)
        {
            var ability = package.Game.Abilities.FirstOrDefault(item => IdEquals(item.Id, abilityId)
                && KindEquals(item.Kind, "attack"));
            if (ability is not null) return ability;
        }
        return null;
    }

    private static IEnumerable<string> DamageResourceIds(AbilityDefinition ability)
    {
        if (ability.Effects.Count == 0)
        {
            yield return ability.ResourceId ?? "resource/health";
            yield break;
        }
        foreach (var effect in ability.Effects.Where(effect =>
                     KindEquals(effect.Type, "damage_resource")
                     || KindEquals(effect.Type, "damage")
                     || KindEquals(effect.Type, "change_resource")
                     && effect.Args.TryGetValue("amount", out var raw)
                     && double.TryParse(raw, System.Globalization.NumberStyles.Float,
                         System.Globalization.CultureInfo.InvariantCulture, out var amount)
                     && amount < 0))
        {
            if (effect.Args.TryGetValue("resourceId", out var resourceId)
                || effect.Args.TryGetValue("id", out resourceId))
                yield return resourceId;
        }
    }

    private static bool AbilityCanExecute(
        GamePackageDefinition package,
        AbilityDefinition ability,
        EncounterParticipantDefinition source,
        EncounterParticipantDefinition target)
    {
        if (ability.Costs.Any(cost => source.Resources.All(resource => !IdEquals(resource.Id, cost.Id)
                || resource.Amount < cost.Amount)))
            return false;
        if (ability.Effects.Count == 0)
        {
            if (!KindEquals(ability.Kind, "attack")) return false;
            var resourceId = ability.ResourceId ?? "resource/health";
            return target.Resources.Any(resource => IdEquals(resource.Id, resourceId));
        }
        foreach (var effect in ability.Effects)
        {
            if (KindEquals(effect.Type, "damage_resource")
                || KindEquals(effect.Type, "damage")
                || KindEquals(effect.Type, "heal_resource")
                || KindEquals(effect.Type, "heal")
                || KindEquals(effect.Type, "change_resource"))
            {
                var resourceId = effect.Args.GetValueOrDefault("resourceId")
                                 ?? effect.Args.GetValueOrDefault("id");
                if (!string.IsNullOrWhiteSpace(resourceId)
                    && target.Resources.Any(resource => IdEquals(resource.Id, resourceId)))
                    return true;
            }
            if (KindEquals(effect.Type, "add_status") || KindEquals(effect.Type, "status"))
            {
                var statusId = effect.Args.GetValueOrDefault("statusId")
                               ?? effect.Args.GetValueOrDefault("id");
                if (!string.IsNullOrWhiteSpace(statusId)
                    && package.Game.Statuses.Count(status => IdEquals(status.Id, statusId)) == 1)
                    return true;
            }
            if (KindEquals(effect.Type, "change_stat"))
            {
                var statId = effect.Args.GetValueOrDefault("id");
                if (!string.IsNullOrWhiteSpace(statId)
                    && target.Stats.Any(stat => IdEquals(stat.Id, statId))
                    && package.Game.Stats.Count(stat => IdEquals(stat.Id, statId)) == 1)
                    return true;
            }
        }
        return false;
    }

    private static GeneratedCampaignCombatReadiness NotReady(string? encounterId, string diagnostic) =>
        new()
        {
            EncounterId = encounterId ?? string.Empty,
            Diagnostics = [diagnostic]
        };

    private static bool IdEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static bool KindEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
