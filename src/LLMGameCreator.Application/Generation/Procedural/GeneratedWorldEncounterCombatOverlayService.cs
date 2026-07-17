using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedWorldEncounterCombatOverlayService
{
    private static readonly IReadOnlyList<string> AllowedFieldPaths =
    [
        "game.encounters[*].participants[*].abilities",
        "game.encounters[*].participants[*].resources",
        "game.encounters[*].participants[*].stats",
        "game.encounters[*].participants[*].inventoryId",
        "game.encounters[*].participants[*].metadata"
    ];

    public GeneratedWorldEncounterCombatOverlayResult Build(
        GamePackageDefinition preCombatPackage,
        GeneratedEncounterCombatContract contract,
        GeneratedEncounterCombatBindingResult bindingResult)
    {
        ArgumentNullException.ThrowIfNull(preCombatPackage);
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(bindingResult);
        if (!bindingResult.Passed)
            return Failed(bindingResult.Diagnostics.Count > 0
                ? bindingResult.Diagnostics
                : ["generated_combat.binding_invalid"]);

        var boundIds = bindingResult.Bindings.Select(item => item.PackageEncounterId)
            .ToHashSet(StringComparer.Ordinal);
        var before = GeneratedEncounterCombatCanonical.Clone(preCombatPackage);
        var after = GeneratedEncounterCombatCanonical.Clone(preCombatPackage);
        CanonicalizeGeneratedOrdering(before, boundIds);
        CanonicalizeGeneratedOrdering(after, boundIds);
        var diagnostics = new List<string>();

        foreach (var binding in bindingResult.Bindings.OrderBy(item => item.PackageEncounterId, StringComparer.Ordinal))
        {
            var encounters = after.Game.Encounters.Where(item => item.Id == binding.PackageEncounterId).ToList();
            if (encounters.Count != 1)
            {
                diagnostics.Add("generated_combat.binding_package_encounter_missing");
                continue;
            }
            var encounter = encounters[0];
            foreach (var participant in encounter.Participants)
                ApplyRole(participant, IsPlayer(participant.Team) ? contract.PlayerRole : contract.OpponentRole);
        }

        var countsBefore = DefinitionCounts(before);
        var countsAfter = DefinitionCounts(after);
        if (!countsBefore.OrderBy(item => item.Key, StringComparer.Ordinal)
                .SequenceEqual(countsAfter.OrderBy(item => item.Key, StringComparer.Ordinal)))
            diagnostics.Add("generated_combat.delta_unexpected_collection_change");
        ValidateControlledDelta(before, after, boundIds, diagnostics);
        ValidateReferences(after, boundIds, diagnostics);

        diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        var json = GeneratedEncounterCombatCanonical.Serialize(after) + Environment.NewLine;
        var beforeFingerprints = Fingerprints(before, boundIds);
        var afterFingerprints = Fingerprints(after, boundIds);
        var document = new GeneratedWorldEncounterCombatOverlayDocument
        {
            SourcePackageSha256 = GeneratedEncounterCombatCanonical.HashText(
                GeneratedEncounterCombatCanonical.Serialize(before) + Environment.NewLine),
            OutputPackageSha256 = GeneratedEncounterCombatCanonical.HashText(json),
            ContractId = contract.ContractId,
            GeneratedEncounterCount = boundIds.Count,
            BoundEncounterCount = bindingResult.Bindings.Count,
            GeneratedParticipantCount = after.Game.Encounters.Where(item => boundIds.Contains(item.Id))
                .Sum(item => item.Participants.Count),
            EncounterFingerprintsBefore = beforeFingerprints,
            EncounterFingerprintsAfter = afterFingerprints,
            AllowedFieldPaths = AllowedFieldPaths,
            DefinitionCollectionCountsBefore = countsBefore,
            DefinitionCollectionCountsAfter = countsAfter,
            Diagnostics = diagnostics,
            Passed = diagnostics.Count == 0
        };
        return new GeneratedWorldEncounterCombatOverlayResult
        {
            Passed = document.Passed,
            CombatOverlayPackage = after,
            CombatOverlayPackageJson = json,
            Document = document,
            Diagnostics = diagnostics
        };
    }

    private static void ApplyRole(
        EncounterParticipantDefinition participant,
        GeneratedEncounterCombatRoleContract role)
    {
        participant.Abilities = role.Abilities.OrderBy(value => value, StringComparer.Ordinal).ToList();
        participant.Resources = role.Resources.OrderBy(item => item.Id, StringComparer.Ordinal)
            .Select(item => GeneratedEncounterCombatCanonical.Clone(item)).ToList();
        participant.Stats = role.Stats.OrderBy(item => item.Id, StringComparer.Ordinal)
            .Select(item => GeneratedEncounterCombatCanonical.Clone(item)).ToList();
        participant.InventoryId = role.InventoryId;
        participant.Metadata = role.CombatMetadata.OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
    }

    private static void CanonicalizeGeneratedOrdering(
        GamePackageDefinition package,
        IReadOnlySet<string> generatedIds)
    {
        foreach (var encounter in package.Game.Encounters.Where(item => generatedIds.Contains(item.Id)))
            encounter.Participants = encounter.Participants.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        var sortedGenerated = new Queue<EncounterDefinition>(package.Game.Encounters
            .Where(item => generatedIds.Contains(item.Id)).OrderBy(item => item.Id, StringComparer.Ordinal));
        package.Game.Encounters = package.Game.Encounters
            .Select(item => generatedIds.Contains(item.Id) ? sortedGenerated.Dequeue() : item).ToList();
    }

    private static void ValidateControlledDelta(
        GamePackageDefinition before,
        GamePackageDefinition after,
        IReadOnlySet<string> generatedIds,
        ICollection<string> diagnostics)
    {
        if (!CanonicalEqual(before.Manifest, after.Manifest))
            diagnostics.Add("generated_combat.delta_baseline_changed");
        if (!CanonicalEqual(before.AssetCatalog, after.AssetCatalog)
            || !CanonicalEqual(before.ScriptCatalog, after.ScriptCatalog)
            || !CanonicalEqual(before.GeneratedContent, after.GeneratedContent))
            diagnostics.Add("generated_combat.delta_baseline_changed");

        var gameBefore = GeneratedEncounterCombatCanonical.Clone(before.Game);
        var gameAfter = GeneratedEncounterCombatCanonical.Clone(after.Game);
        gameBefore.Encounters = [];
        gameAfter.Encounters = [];
        if (!CanonicalEqual(gameBefore, gameAfter))
            diagnostics.Add("generated_combat.delta_unexpected_collection_change");

        var beforeById = before.Game.Encounters.GroupBy(item => item.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var afterById = after.Game.Encounters.GroupBy(item => item.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        if (!beforeById.Keys.OrderBy(value => value, StringComparer.Ordinal)
                .SequenceEqual(afterById.Keys.OrderBy(value => value, StringComparer.Ordinal)))
            diagnostics.Add("generated_combat.delta_unexpected_collection_change");
        foreach (var id in beforeById.Keys.Intersect(afterById.Keys, StringComparer.Ordinal))
        {
            if (beforeById[id].Count != 1 || afterById[id].Count != 1)
            {
                diagnostics.Add("generated_combat.delta_unexpected_collection_change");
                continue;
            }
            if (!generatedIds.Contains(id))
            {
                if (!CanonicalEqual(beforeById[id][0], afterById[id][0]))
                    diagnostics.Add("generated_combat.delta_non_generated_encounter_changed");
                continue;
            }
            if (!OnlyCombatFieldsChanged(beforeById[id][0], afterById[id][0]))
                diagnostics.Add("generated_combat.delta_forbidden_encounter_field");
        }
    }

    private static bool OnlyCombatFieldsChanged(EncounterDefinition before, EncounterDefinition after)
    {
        var outerBefore = GeneratedEncounterCombatCanonical.Clone(before);
        var outerAfter = GeneratedEncounterCombatCanonical.Clone(after);
        outerBefore.Participants = [];
        outerAfter.Participants = [];
        if (!CanonicalEqual(outerBefore, outerAfter)) return false;
        var beforeById = before.Participants.GroupBy(item => item.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var afterById = after.Participants.GroupBy(item => item.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        if (!beforeById.Keys.OrderBy(value => value, StringComparer.Ordinal)
                .SequenceEqual(afterById.Keys.OrderBy(value => value, StringComparer.Ordinal))) return false;
        foreach (var id in beforeById.Keys)
        {
            if (beforeById[id].Count != 1 || afterById[id].Count != 1) return false;
            var left = WithoutCombat(beforeById[id][0]);
            var right = WithoutCombat(afterById[id][0]);
            if (!CanonicalEqual(left, right)) return false;
        }
        return true;
    }

    private static EncounterParticipantDefinition WithoutCombat(EncounterParticipantDefinition value)
    {
        var clone = GeneratedEncounterCombatCanonical.Clone(value);
        clone.Abilities = [];
        clone.Resources = [];
        clone.Stats = [];
        clone.InventoryId = null;
        clone.Metadata = [];
        return clone;
    }

    private static void ValidateReferences(
        GamePackageDefinition package,
        IReadOnlySet<string> generatedIds,
        ICollection<string> diagnostics)
    {
        foreach (var participant in package.Game.Encounters.Where(item => generatedIds.Contains(item.Id))
                     .SelectMany(item => item.Participants))
        {
            if (participant.Resources.Any(item => package.Game.Resources.Count(definition => definition.Id == item.Id) != 1)
                || participant.Stats.Any(item => package.Game.Stats.Count(definition => definition.Id == item.Id) != 1)
                || participant.Abilities.Any(id => package.Game.Abilities.Count(definition => definition.Id == id) != 1)
                || participant.InventoryId is { Length: > 0 } inventoryId
                && package.Game.Inventories.Count(definition => definition.Id == inventoryId) != 1)
                diagnostics.Add("generated_combat.reference_invalid");
        }
    }

    private static IReadOnlyDictionary<string, int> DefinitionCounts(GamePackageDefinition package)
    {
        using var document = JsonDocument.Parse(GeneratedEncounterCombatCanonical.Serialize(package.Game));
        return document.RootElement.EnumerateObject()
            .Where(property => property.Value.ValueKind == JsonValueKind.Array)
            .ToDictionary(property => "game." + property.Name,
                property => property.Value.GetArrayLength(), StringComparer.Ordinal);
    }

    private static IReadOnlyList<GeneratedWorldEncounterCombatFingerprint> Fingerprints(
        GamePackageDefinition package,
        IReadOnlySet<string> generatedIds) => package.Game.Encounters
        .Where(item => generatedIds.Contains(item.Id)).OrderBy(item => item.Id, StringComparer.Ordinal)
        .Select(item => new GeneratedWorldEncounterCombatFingerprint
        {
            EncounterId = item.Id,
            CanonicalSha256 = GeneratedEncounterCombatCanonical.Hash(item)
        }).ToList();

    private static bool CanonicalEqual<T>(T left, T right) => string.Equals(
        GeneratedEncounterCombatCanonical.Serialize(left),
        GeneratedEncounterCombatCanonical.Serialize(right),
        StringComparison.Ordinal);

    private static bool IsPlayer(string? team) =>
        string.Equals(team, "player", StringComparison.OrdinalIgnoreCase);

    private static GeneratedWorldEncounterCombatOverlayResult Failed(IReadOnlyList<string> diagnostics) => new()
    {
        Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList()
    };
}
