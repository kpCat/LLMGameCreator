using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class EncounterAiService : IEncounterAiService
{
    private readonly IEncounterRuntimeService _encounterRuntimeService;

    public EncounterAiService(IEncounterRuntimeService encounterRuntimeService)
    {
        _encounterRuntimeService = encounterRuntimeService;
    }

    public GameRuntimeResult RunCurrentTurnAi(GamePackageDefinition package, GameRuntimeState state)
    {
        var encounter = state.ActiveEncounter;
        if (encounter == null || !encounter.Active || encounter.Participants.Count == 0)
        {
            return Failure(state, "encounter.not_active", "No active encounter.", null);
        }

        var participant = encounter.Participants[Math.Max(0, Math.Min(encounter.TurnIndex, encounter.Participants.Count - 1))];
        if (RuntimeStateHelpers.KindEquals(participant.Team, "player"))
        {
            return Failure(state, "encounter.ai.player_turn", "Current turn belongs to a player participant.", participant.Id);
        }

        var definition = package.Game.Encounters.FirstOrDefault(item => RuntimeStateHelpers.IdEquals(item.Id, encounter.EncounterId));
        var participantDefinition = definition?.Participants.FirstOrDefault(item => RuntimeStateHelpers.IdEquals(item.Id, participant.Id));
        var target = encounter.Participants.FirstOrDefault(item => item.Alive && !RuntimeStateHelpers.KindEquals(item.Team, participant.Team));
        if (target == null)
        {
            return Failure(state, "encounter.ai.target_missing", "AI could not find a living enemy target.", participant.Id);
        }

        var abilityId = participantDefinition?.Abilities.FirstOrDefault(id => package.Game.Abilities.Any(ability => RuntimeStateHelpers.IdEquals(ability.Id, id)));
        var result = string.IsNullOrWhiteSpace(abilityId)
            ? _encounterRuntimeService.BasicAttack(package, state, participant.Id, target.Id)
            : _encounterRuntimeService.UseAbility(package, state, abilityId!, participant.Id, target.Id);

        if (result.Success)
        {
            result.Events.Insert(0, RuntimeStateHelpers.Event(
                GameRuntimeEventType.AiActionChosen,
                $"AI chose action: {(abilityId ?? "basic_attack")}",
                participant.Id,
                new Dictionary<string, string> { ["target"] = target.Id, ["abilityId"] = abilityId ?? string.Empty }));
        }

        return result;
    }

    private static GameRuntimeResult Failure(GameRuntimeState state, string code, string message, string? targetId)
    {
        return new GameRuntimeResult
        {
            Success = false,
            State = state,
            Message = message,
            Diagnostics = new List<RuntimeDiagnostic> { RuntimeStateHelpers.Diagnostic(code, message, targetId) },
            Events = new List<GameRuntimeEvent> { RuntimeStateHelpers.Event(GameRuntimeEventType.ValidationFailed, message, targetId) }
        };
    }
}
