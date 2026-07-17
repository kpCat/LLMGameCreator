using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedEncounterCombatBindingService
{
    public GeneratedEncounterCombatBindingResult Bind(
        SeededGeneratedProjectSourceValidationResult strictSource,
        GamePackageDefinition preCombatPackage,
        GeneratedEncounterCombatContract contract)
    {
        ArgumentNullException.ThrowIfNull(strictSource);
        ArgumentNullException.ThrowIfNull(preCombatPackage);
        ArgumentNullException.ThrowIfNull(contract);
        if (strictSource is not
            {
                Present: true,
                Passed: true,
                RegeneratedPlan: not null,
                Overlay: not null
            })
            return Failed("generated_combat.binding_source_invalid");

        var diagnostics = new List<string>();
        var plan = strictSource.RegeneratedPlan;
        var rows = preCombatPackage.GeneratedContent.Encounters;
        var generatedPackageIds = strictSource.Overlay.GeneratedRecords
            .Where(item => string.Equals(item.CollectionPath, "game.encounters", StringComparison.Ordinal))
            .Select(item => item.RecordId)
            .ToHashSet(StringComparer.Ordinal);
        var bindings = new List<GeneratedEncounterCombatBinding>();

        foreach (var seed in plan.EncounterSeeds.OrderBy(item => item.EncounterSeedId, StringComparer.Ordinal))
        {
            var exactSourceId = CanonicalGeneratedSourceId(seed.EncounterSeedId);
            var mappedRows = rows.Where(item => string.Equals(item.SourceId, exactSourceId, StringComparison.Ordinal)).ToList();
            if (mappedRows.Count != 1)
            {
                diagnostics.Add(mappedRows.Count == 0
                    ? "generated_combat.binding_generated_content_missing"
                    : "generated_combat.binding_generated_content_duplicate");
                continue;
            }

            var packageMatches = preCombatPackage.Game.Encounters.Where(encounter =>
                    generatedPackageIds.Contains(encounter.Id)
                    && encounter.Metadata.TryGetValue("sourceEncounterSeedId", out var sourceId)
                    && string.Equals(sourceId, mappedRows[0].SourceId, StringComparison.Ordinal))
                .ToList();
            if (packageMatches.Count != 1)
            {
                diagnostics.Add(packageMatches.Count == 0
                    ? "generated_combat.binding_package_encounter_missing"
                    : "generated_combat.binding_package_encounter_duplicate");
                continue;
            }
            var encounter = packageMatches[0];
            bindings.Add(new GeneratedEncounterCombatBinding
            {
                EncounterSeedId = seed.EncounterSeedId,
                GeneratedContentSourceId = mappedRows[0].SourceId,
                PackageEncounterId = encounter.Id,
                BeforeEncounterSha256 = GeneratedEncounterCombatCanonical.Hash(encounter)
            });
        }

        var expectedSourceIds = plan.EncounterSeeds.Select(item => CanonicalGeneratedSourceId(item.EncounterSeedId))
            .ToHashSet(StringComparer.Ordinal);
        if (rows.Any(row => !expectedSourceIds.Contains(row.SourceId)))
            diagnostics.Add("generated_combat.binding_unplanned_generated_content");
        if (generatedPackageIds.Any(id => bindings.All(binding => binding.PackageEncounterId != id)))
            diagnostics.Add("generated_combat.binding_unbound_package_encounter");
        if (bindings.Select(item => item.PackageEncounterId).Distinct(StringComparer.Ordinal).Count() != bindings.Count)
            diagnostics.Add("generated_combat.binding_package_encounter_duplicate");
        if (bindings.Count != plan.EncounterSeeds.Count || bindings.Count != rows.Count
            || bindings.Count != generatedPackageIds.Count)
            diagnostics.Add("generated_combat.binding_count_mismatch");

        diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        return new GeneratedEncounterCombatBindingResult
        {
            Passed = diagnostics.Count == 0,
            Bindings = bindings.OrderBy(item => item.EncounterSeedId, StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics
        };
    }

    private static string CanonicalGeneratedSourceId(string sourceId) =>
        sourceId.StartsWith("generated/", StringComparison.Ordinal)
        || sourceId.StartsWith("seeded_generated_project/", StringComparison.Ordinal)
            ? sourceId
            : "generated/" + sourceId;

    private static GeneratedEncounterCombatBindingResult Failed(string diagnostic) => new()
    {
        Diagnostics = [diagnostic]
    };
}
