using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.CapabilityDrivenRuntimePlaythrough;

public sealed class CharacterAttributesLevelProgressionGoal150Tests
{
    [Fact]
    public void Goal150_all_optional_modules_certify_with_default_parameters()
    {
        var (library, _) = Load();
        var cache = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal150-cert-cache-" + Guid.NewGuid().ToString("N"));
        var output = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal150-cert-output-" + Guid.NewGuid().ToString("N"));
        try
        {
            var ledger = new FeatureModuleCertificationService(
                    SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                    new FeatureModuleCertificationCache(cache))
                .Certify(FindRoot(), library, new string('a', 64), output);
            var attributeOutput = Path.Combine(output, "attribute-detail");
            var attributeQualification = new FeatureModuleCompositionService(
                    SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
                .ComposeAndQualify(FindRoot(), library.Catalog, ["feature.character.attributes"], attributeOutput,
                    "goal150-attribute-detail", useCapabilityDrivenRuntimePlaythrough: true);
            var detail = string.Join(Environment.NewLine, ledger.Entries.Select(entry =>
                entry.ModuleId + ":" + entry.Status
                + ":materialization=" + entry.MaterializationPassed
                + ":runtime=" + entry.RuntimeQualificationPassed
                + ":effects=" + entry.TargetRuntimeEffectsPassed
                + ":checkpoint=" + entry.CheckpointReloadPassed
                + ":replay=" + entry.FullReplayEquivalent
                + ":binding=" + entry.ActionBindingPassed
                + ":" + string.Join("|", entry.Diagnostics)));
            detail += Environment.NewLine + string.Join(Environment.NewLine,
                attributeQualification.Result.SemanticEffects.Observations.Select(observation =>
                    observation.EffectId + ":passed=" + observation.Passed + ":actual=" + observation.ActualValue
                    + ":baseline=" + observation.BaselineValue + ":" + string.Join("|", observation.Diagnostics)));
            Assert.True(ledger.Status == "GREEN", detail);
        }
        finally
        {
            if (Directory.Exists(cache)) Directory.Delete(cache, true);
            if (Directory.Exists(output)) Directory.Delete(output, true);
        }
    }

    [Fact]
    public void Goal150_incremental_certification_invalidates_only_changed_new_module_and_dependents()
    {
        var (library, _) = Load();
        VerifyInvalidation(library, "feature.character.attributes", "startingStrength", 8,
            "goal150-attributes-invalidation");
        VerifyInvalidation(library, "feature.character.level_progression", "level2RequiredExperience", 11,
            "goal150-progression-invalidation");
    }

    [Fact]
    public void Goal150_catalog_and_extended_mutation_registry_are_deterministic_and_atomic()
    {
        var (library, packageJson) = Load();
        Assert.Equal(10, library.Manifest.RequiredCoreModuleCount);
        Assert.Equal(9, library.Manifest.OptionalModuleCount);
        Assert.Equal(19, library.Manifest.ModuleFileCount);
        var attributes = Module(library, "feature.character.attributes");
        var progression = Module(library, "feature.character.level_progression");
        var equipment = Module(library, "feature.equipment.weapon_loadout");
        Assert.False(attributes.DefaultSelected);
        Assert.False(progression.DefaultSelected);
        Assert.Equal(["damagePerStrengthPoint", "startingStrength"],
            attributes.ParameterDefinitions.Select(item => item.ParameterId).OrderBy(id => id, StringComparer.Ordinal));
        Assert.Equal("level2RequiredExperience", progression.ParameterDefinitions.Single().ParameterId);

        var operations = attributes.MutationOperations.Concat(progression.MutationOperations)
            .Concat(equipment.MutationOperations).ToList();
        var service = new FeatureModulePackageMutationService();
        var forward = service.Apply(packageJson, operations);
        var reverse = service.Apply(packageJson, operations.AsEnumerable().Reverse().ToList());
        Assert.True(forward.Passed, string.Join(Environment.NewLine, forward.Diagnostics));
        Assert.Equal(forward.PackageJson, reverse.PackageJson);
        Assert.Equal(7, forward.Operations.Count);
        Assert.Equal(6, forward.Operations.Select(item => item.TargetKind).Distinct(StringComparer.Ordinal).Count());

        var package = Deserialize(forward.PackageJson);
        Assert.Equal(7, package.Game.Stats.Single(item => item.Id == "stat/strength").DefaultValue);
        Assert.Equal(7, package.Game.Encounters.Single(item => item.Id == "encounter/goblin_duel")
            .Participants.Single(item => item.Id == "player").Stats.Single(item => item.Id == "stat/strength").Amount);
        var metadata = package.Game.Abilities.Single(item => item.Id == "ability/basic_attack").Metadata;
        Assert.Equal("stat/strength", metadata["source_stat_damage_stat_id"]);
        Assert.Equal("5", metadata["source_stat_damage_baseline"]);
        Assert.Equal("1", metadata["source_stat_damage_per_point"]);
        Assert.Equal(10, package.Game.Progressions.Single(item => item.Id == "progression/character_level")
            .Stages.Single(item => item.Id == "level/2").RequiredAmount);

        var invalid = operations.Select(operation => operation.OperationId == "attributes.player_strength_amount"
            ? operation with { ExpectedValue = "999" }
            : operation).ToList();
        var rejected = service.Apply(packageJson, invalid);
        Assert.False(rejected.Passed);
        Assert.Equal(packageJson, rejected.PackageJson);
    }

    [Fact]
    public void Goal150_attributes_progression_and_equipment_qualify_independently_and_additively()
    {
        var (library, packageJson) = Load();
        var attributes = Module(library, "feature.character.attributes");
        var progression = Module(library, "feature.character.level_progression");
        var equipment = Module(library, "feature.equipment.weapon_loadout");
        var baseModules = library.Catalog.Modules.Where(module => module.Required).ToList();
        var noCombat = baseModules.Where(module => module.ModuleId != "feature.combat.turn_based_encounter").ToList();

        var attributesPackage = Mutate(packageJson, attributes);
        var attributesWithoutCombatPlan = Plan(noCombat.Append(attributes), attributesPackage);
        var attributesWithoutCombat = Qualify(attributesPackage, attributesWithoutCombatPlan, "goal150-attributes-no-combat");
        AssertGreen(attributesWithoutCombat);
        Assert.Contains("stat/strength=7", attributesWithoutCombat.Session.LatestAttributesSummary, StringComparison.Ordinal);
        Assert.DoesNotContain(attributesWithoutCombatPlan.OrderedActions, action => action.ActionId == "basic_attack");

        var progressionPackage = Mutate(packageJson, progression);
        var progressionWithoutCombatPlan = Plan(noCombat.Append(progression), progressionPackage);
        var progressionWithoutCombat = Qualify(progressionPackage, progressionWithoutCombatPlan, "goal150-progression-no-combat");
        AssertGreen(progressionWithoutCombat);
        Assert.Contains("progression/character_level=10:level/2", progressionWithoutCombat.Session.LatestProgressionSummary,
            StringComparison.Ordinal);
        Assert.DoesNotContain(progressionWithoutCombatPlan.OrderedActions, action =>
            action.ActionId is "basic_attack" or "inspect_character_attributes");

        var attributesCombatPlan = Plan(baseModules.Append(attributes), attributesPackage);
        var attributesCombat = Qualify(attributesPackage, attributesCombatPlan, "goal150-attributes-combat");
        AssertGreen(attributesCombat);
        AssertDamage(attributesCombat, stat: "2", equipment: "0", total: "2");

        var equipmentPackage = Mutate(packageJson, equipment);
        var equipmentCombatPlan = Plan(baseModules.Append(equipment), equipmentPackage);
        var equipmentCombat = Qualify(equipmentPackage, equipmentCombatPlan, "goal150-equipment-combat");
        AssertGreen(equipmentCombat);
        var equipmentDamage = DamageEvent(equipmentCombat);
        Assert.Equal("2", equipmentDamage.Args["equipmentDamageBonus"]);
        Assert.False(equipmentDamage.Args.ContainsKey("statDamageBonus"));

        var combinedPackage = Mutate(packageJson, attributes, equipment);
        var combinedPlan = Plan(baseModules.Concat([attributes, equipment]), combinedPackage);
        var combined = Qualify(combinedPackage, combinedPlan, "goal150-attributes-equipment-combat");
        AssertGreen(combined);
        AssertDamage(combined, stat: "2", equipment: "2", total: "4");

        var attributesProgressionPackage = Mutate(packageJson, attributes, progression);
        var attributesProgressionPlan = Plan(noCombat.Concat([attributes, progression]), attributesProgressionPackage);
        var attributesProgression = Qualify(attributesProgressionPackage, attributesProgressionPlan,
            "goal150-attributes-progression");
        AssertGreen(attributesProgression);
        Assert.Contains("stat/strength=7", attributesProgression.Session.LatestAttributesSummary, StringComparison.Ordinal);
        Assert.Contains("progression/character_level=10:level/2", attributesProgression.Session.LatestProgressionSummary,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Goal150_planner_rejects_stat_progression_and_capability_tamper_before_mutation()
    {
        var (library, packageJson) = Load();
        var attributes = Module(library, "feature.character.attributes");
        var progression = Module(library, "feature.character.level_progression");
        var modules = library.Catalog.Modules.Where(module => module.Required).Concat([attributes, progression]).ToList();
        var package = Mutate(packageJson, attributes, progression);
        var planner = new CapabilityDrivenRuntimePlaythroughPlanner();

        var missingStat = Clone(package);
        missingStat.Game.Stats.Clear();
        Assert.Contains(planner.TryPlan(modules, missingStat).Diagnostics,
            diagnostic => diagnostic.Contains("source stat", StringComparison.Ordinal));

        var malformed = Clone(package);
        malformed.Game.Abilities.Single(item => item.Id == "ability/basic_attack")
            .Metadata["source_stat_damage_per_point"] = "invalid";
        Assert.Contains(planner.TryPlan(modules, malformed).Diagnostics,
            diagnostic => diagnostic.Contains("stat multiplier", StringComparison.Ordinal));

        var missingProgression = Clone(package);
        missingProgression.Game.Progressions.Clear();
        Assert.Contains(planner.TryPlan(modules, missingProgression).Diagnostics,
            diagnostic => diagnostic.Contains("unresolved target", StringComparison.Ordinal));

        var duplicate = progression with
        {
            RuntimePlaythroughContracts = [progression.RuntimePlaythroughContracts[0], progression.RuntimePlaythroughContracts[0]]
        };
        Assert.Contains(planner.TryPlan(modules.Select(module => module.ModuleId == duplicate.ModuleId ? duplicate : module).ToList(),
                package).Diagnostics,
            diagnostic => diagnostic.Contains("duplicate action ID", StringComparison.Ordinal));
    }

    private static CapabilityRuntimePlaythroughPlan Plan(
        IEnumerable<FeatureModuleDefinition> modules,
        GamePackageDefinition package) => new CapabilityDrivenRuntimePlaythroughPlanner().Plan(modules.ToList(), package);

    private static ProductLineRuntimeQualificationResult Qualify(
        GamePackageDefinition package,
        CapabilityRuntimePlaythroughPlan plan,
        string id) => new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
        .Qualify(package, new ProductLineRuntimeQualificationRequest
        {
            SessionId = id + "-session",
            CandidateId = id,
            VariantKind = id,
            PackagePath = "in-memory/package.json",
            PackageSha256 = new string('a', 64),
            CheckpointId = id + "-checkpoint",
            FinalCheckpointId = id + "-final",
            CapabilityPlan = plan
        });

    private static void AssertGreen(ProductLineRuntimeQualificationResult result)
    {
        Assert.True(result.CheckpointReplay.Passed, string.Join(Environment.NewLine, result.CheckpointReplay.Diagnostics));
        Assert.True(result.FinalReplay.Passed, string.Join(Environment.NewLine, result.FinalReplay.Diagnostics));
        Assert.True(result.ActionDescriptorExecutionBindingPassed);
    }

    private static void AssertDamage(
        ProductLineRuntimeQualificationResult result,
        string stat,
        string equipment,
        string total)
    {
        var damage = DamageEvent(result);
        Assert.Equal(stat, damage.Args["statDamageBonus"]);
        Assert.Equal(equipment, damage.Args["equipmentDamageBonus"]);
        Assert.Equal(total, damage.Args["totalAdditionalDamage"]);
        Assert.Equal("stat/strength", damage.Args["statId"]);
        Assert.Equal("7", damage.Args["statValue"]);
    }

    private static CanonicalRuntimePlayerCommandLoopRuntimeEvent DamageEvent(ProductLineRuntimeQualificationResult result) =>
        result.Session.LatestSnapshot.RuntimeEvents.Last(item => item.EventType == "DamageApplied");

    private static GamePackageDefinition Mutate(string packageJson, params FeatureModuleDefinition[] modules)
    {
        var result = new FeatureModulePackageMutationService().Apply(packageJson,
            modules.SelectMany(module => module.MutationOperations).ToList());
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        return Deserialize(result.PackageJson);
    }

    private static FeatureModuleDefinition Module(FeatureModuleLibrarySnapshot library, string id) =>
        library.Catalog.Modules.Single(module => module.ModuleId == id);

    private static void VerifyInvalidation(
        FeatureModuleLibrarySnapshot library,
        string moduleId,
        string parameterId,
        int newDefault,
        string name)
    {
        var cache = Path.Combine(Path.GetTempPath(), "LLMGameCreator", name + "-cache-" + Guid.NewGuid().ToString("N"));
        var output = Path.Combine(Path.GetTempPath(), "LLMGameCreator", name + "-output-" + Guid.NewGuid().ToString("N"));
        try
        {
            var service = new FeatureModuleCertificationService(
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                new FeatureModuleCertificationCache(cache));
            var first = service.Certify(FindRoot(), library, new string('a', 64), output);
            var second = service.Certify(FindRoot(), library, new string('a', 64), output);
            Assert.Equal(9, first.ExecutedCount);
            Assert.Equal(9, second.ReusedCount);

            var changedModule = Module(library, moduleId) with
            {
                ParameterDefinitions = Module(library, moduleId).ParameterDefinitions.Select(parameter =>
                    parameter.ParameterId == parameterId
                        ? parameter with { DefaultValue = JsonSerializer.SerializeToElement(newDefault) }
                        : parameter).ToList()
            };
            var modules = library.Catalog.Modules.Select(module => module.ModuleId == moduleId ? changedModule : module).ToList();
            var fingerprints = new FeatureModuleLibraryFingerprintService();
            var moduleFingerprints = modules.ToDictionary(module => module.ModuleId,
                fingerprints.ModuleFingerprint, StringComparer.Ordinal);
            var changedLibrary = library with
            {
                Catalog = library.Catalog with { Modules = modules },
                ModuleFingerprints = moduleFingerprints,
                CatalogFingerprint = fingerprints.CatalogFingerprint(moduleFingerprints)
            };
            var changed = service.Certify(FindRoot(), changedLibrary, new string('a', 64), output);
            Assert.Equal(1, changed.ExecutedCount);
            Assert.Equal(8, changed.ReusedCount);
            Assert.Equal(1, changed.InvalidatedCount);
            Assert.Equal("GREEN", changed.Entries.Single(entry => entry.ModuleId == "feature.equipment.weapon_loadout").Status);
        }
        finally
        {
            if (Directory.Exists(cache)) Directory.Delete(cache, true);
            if (Directory.Exists(output)) Directory.Delete(output, true);
        }
    }

    private static (FeatureModuleLibrarySnapshot Library, string PackageJson) Load()
    {
        var root = FindRoot();
        return (new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules")),
            File.ReadAllText(Path.Combine(root, "samples", "minimal-map-game", "package.json")));
    }

    private static GamePackageDefinition Deserialize(string json) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(json, Options)!;

    private static GamePackageDefinition Clone(GamePackageDefinition package) =>
        Deserialize(JsonSerializer.Serialize(package, Options));

    private static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
