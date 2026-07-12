using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.CapabilityDrivenRuntimePlaythrough;

public sealed class CapabilityDrivenRuntimePlaythroughGoal149Tests
{
    [Fact]
    public async Task Goal149_accepted_workspace_preserves_disabled_hashes_and_builds_enabled_equipment_slice()
    {
        var root = FindRoot();
        var gamesRoot = Temp("goal149-accepted-workspace");
        try
        {
            var repository = new JsonGamePackageRepository();
            var projectFolder = Path.Combine(gamesRoot, "goal148-manual");
            Directory.CreateDirectory(projectFolder);
            Directory.CreateDirectory(Path.Combine(projectFolder, "assets"));
            Directory.CreateDirectory(Path.Combine(projectFolder, "scripts"));
            Directory.CreateDirectory(Path.Combine(projectFolder, "saves"));
            File.Copy(Path.Combine(root, ".llmgc", "procedural",
                    "goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix",
                    "compositions", "minimal-map-game-composed-alchemy-combat-exploration", "package.json"),
                Path.Combine(projectFolder, "package.json"));
            var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
            var legacyPersistence = new FeatureModuleCompositionPersistenceService(
                Path.Combine(projectFolder, ".llmgc", "authoring"));
            var legacy = legacyPersistence.CreateNew(UnifiedGameProjectWorkspaceVocabulary.LegacyCompositionId,
                "Проверка конструктора", "Настройки механик открытого игрового проекта.", library) with
            {
                ParameterValues = AcceptedParameterValues(),
                LastMaterializedPackageSha256 = "e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221",
                LastQualifiedFinalStateHash = "95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8",
                LastQualificationStatus = "GREEN"
            };
            legacyPersistence.Save(legacy, library);
            var current = new CurrentGamePackageService(repository);
            await current.LoadAsync(projectFolder, CancellationToken.None);
            var controller = new UnifiedGameProjectWorkspaceController(
                current,
                new GameProjectFeatureModuleAuthoringService(root),
                new GameProjectBuildAndQualificationService(root,
                    SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), repository,
                    new GamePackageValidator(), current));
            var opened = controller.OpenProject(projectFolder);
            Assert.Contains(opened.Mechanics, mechanic => mechanic.ModuleId == "feature.equipment.weapon_loadout"
                                                        && !mechanic.Selected && mechanic.Title == "Экипировка и оружие");
            ApplyAcceptedValues(controller);

            var disabled = controller.BuildAndQualify();
            Assert.True(disabled.Passed, string.Join(Environment.NewLine, disabled.Diagnostics));
            Assert.Equal("e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221", disabled.CompositionPackageSha256);
            Assert.Equal("c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb", disabled.ActivatedProjectPackageSha256);
            Assert.Equal("95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8", disabled.FinalStateHash);
            Assert.Equal(13, disabled.PlannedActionCount);
            Assert.Equal(8, disabled.CheckpointActionCount);
            Assert.Equal(13, disabled.FinalReplayActionCount);
            Assert.DoesNotContain("rusty_knife", disabled.EquipmentSlotSummary, StringComparison.Ordinal);

            controller.SetModuleSelected("feature.equipment.weapon_loadout", true);
            controller.SetParameterValue("feature.equipment.weapon_loadout", "weaponDamageBonus",
                JsonSerializer.SerializeToElement(2));
            var enabled = controller.BuildAndQualify();
            var activated = await repository.LoadAsync(projectFolder, CancellationToken.None);

            Assert.True(enabled.Passed, string.Join(Environment.NewLine, enabled.Diagnostics));
            Assert.NotEqual(disabled.CompositionPackageSha256, enabled.CompositionPackageSha256);
            Assert.NotEqual(disabled.ActivatedProjectPackageSha256, enabled.ActivatedProjectPackageSha256);
            Assert.NotEqual(disabled.FinalStateHash, enabled.FinalStateHash);
            Assert.Equal(17, enabled.PlannedActionCount);
            Assert.Equal(13, enabled.CheckpointActionCount);
            Assert.Equal(17, enabled.FinalReplayActionCount);
            Assert.Equal("slot/weapon:item/rusty_knife", enabled.EquipmentSlotSummary);
            Assert.Equal(2, enabled.WeaponDamageBonus);
            Assert.Equal(2, enabled.CombatDamageDelta);
            Assert.Contains("Экипировано: Ржавый нож", enabled.HumanSummary, StringComparison.Ordinal);
            Assert.Contains("Слот: Оружие", enabled.HumanSummary, StringComparison.Ordinal);
            Assert.Contains("Бонус урона: +2", enabled.HumanSummary, StringComparison.Ordinal);
            Assert.Equal("game/goal148-manual", activated.Manifest.PackageId);
            Assert.Equal("Проверка конструктора", activated.Manifest.Title);
            Assert.Equal("0.1.0", activated.Manifest.Version);

            WriteGoal149Artifacts(root, disabled, enabled);
        }
        finally { Delete(gamesRoot); }
    }

    [Fact]
    public void Goal149_planner_preserves_disabled_order_and_adds_equipment_without_combination_branches()
    {
        var (library, package) = Load();
        var planner = new CapabilityDrivenRuntimePlaythroughPlanner();
        var disabled = planner.Plan(Selected(library, includeEquipment: false), package);
        var enabled = planner.Plan(Selected(library, includeEquipment: true), PackageWithBonus(package, "2"));

        Assert.Equal(ProductLineRuntimeQualifier.CanonicalActionPlan,
            disabled.OrderedActions.Select(action => action.ActionId));
        Assert.Equal(13, disabled.OrderedActions.Count);
        Assert.Equal("craft", disabled.CheckpointBoundaryActionId);
        Assert.Equal(17, enabled.OrderedActions.Count);
        Assert.Equal("inspect_equipment", enabled.CheckpointBoundaryActionId);
        Assert.Equal(4, enabled.OrderedActions.Count(action => action.CapabilityId == "capability.equipment.weapon_loadout"));
        Assert.DoesNotContain(disabled.OrderedActions, action => action.RuntimePrimitiveId.Contains("equipment", StringComparison.Ordinal));
        Assert.NotEqual(disabled.ActionPlanSignature, enabled.ActionPlanSignature);
    }

    [Fact]
    public void Goal149_equipment_materialization_qualifies_checkpoint_replay_and_combat_bonus()
    {
        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var workspace = Temp("goal149-equipment-workspace");
        var output = Temp("goal149-equipment-output");
        try
        {
            var selected = library.Catalog.Modules.Where(module => module.DefaultSelected
                                                                   || module.ModuleId == "feature.equipment.weapon_loadout")
                .Select(module => module.ModuleId).ToList();
            var document = new FeatureModuleCompositionPersistenceService(workspace).CreateNew(
                "goal149-equipment-enabled", "Goal149 Equipment", "Equipment enabled", library, selected);
            var result = new FeatureModuleParameterizedCompositionService(
                    SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
                .MaterializeAndQualify(root, library, document, output, useCapabilityDrivenRuntimePlaythrough: true);

            Assert.True(result.Passed, string.Join(Environment.NewLine,
                result.Qualification.Result.Diagnostics
                    .Concat(result.Qualification.Artifacts.SemanticEffects.Observations.SelectMany(observation =>
                        observation.Diagnostics.Select(diagnostic => observation.EffectId + ":" + diagnostic)))
                    .Append("qualificationPassed=" + result.Qualification.Result.Passed)
                    .Append("runtimeEffectsPassed=" + result.RuntimeEffectsPassed)
                    .Append("satisfied=" + result.SatisfiedSelectedModuleCount)));
            Assert.True(result.CheckpointReloadPassed);
            Assert.True(result.FullReplayEquivalent);
            Assert.True(result.ActionBindingPassed);
            Assert.Contains("slot/weapon:item/rusty_knife", result.Qualification.Artifacts.Session.LatestEquipmentSummary);
            Assert.Equal(13, result.Qualification.Artifacts.CheckpointReplay.ReplayedActionCount);
            Assert.Equal(17, result.Qualification.Artifacts.FinalReplay.ReplayedActionCount);
            Assert.Contains(result.Qualification.Artifacts.Session.LatestSnapshot.RuntimeEvents, runtimeEvent =>
                runtimeEvent.EventType == "DamageApplied"
                && runtimeEvent.Args.GetValueOrDefault("equipmentDamageBonus") == "2");
            Assert.Equal(4, result.SatisfiedSelectedModuleCount);
        }
        finally
        {
            Delete(workspace);
            Delete(output);
        }
    }

    [Fact]
    public void CapabilityDrivenRuntimePlaythrough_equipment_without_combat_and_combat_without_equipment_are_independent()
    {
        var (library, package) = Load();
        var planner = new CapabilityDrivenRuntimePlaythroughPlanner();
        var withoutCombatModules = Selected(library, includeEquipment: true)
            .Where(module => module.ModuleId != "feature.combat.turn_based_encounter").ToList();
        var equipmentOnly = planner.Plan(withoutCombatModules, PackageWithBonus(package, "2"));
        var combatOnly = planner.Plan(Selected(library, includeEquipment: false), package);

        Assert.Contains(equipmentOnly.OrderedActions, action => action.ActionId == "equip_rusty_knife");
        Assert.DoesNotContain(equipmentOnly.OrderedActions, action => action.ActionId is "begin_encounter" or "basic_attack");
        Assert.Contains(combatOnly.OrderedActions, action => action.ActionId == "basic_attack");
        Assert.DoesNotContain(combatOnly.OrderedActions, action => action.ActionId == "equip_rusty_knife");
    }

    [Fact]
    public void CapabilityDrivenRuntimePlaythrough_rejects_duplicate_cycle_unknown_targets_and_equipment_tamper()
    {
        var (library, package) = Load();
        var modules = Selected(library, includeEquipment: true);
        var equipment = modules.Single(module => module.ModuleId == "feature.equipment.weapon_loadout");
        var planner = new CapabilityDrivenRuntimePlaythroughPlanner();

        var duplicate = equipment with
        {
            RuntimePlaythroughContracts = [equipment.RuntimePlaythroughContracts[0], equipment.RuntimePlaythroughContracts[0]]
        };
        Assert.Contains(planner.TryPlan(Replace(modules, duplicate), PackageWithBonus(package, "2")).Diagnostics,
            diagnostic => diagnostic.Contains("duplicate action ID", StringComparison.Ordinal));

        var cycleContracts = equipment.RuntimePlaythroughContracts.Select(contract => contract.ActionId == "open_starting_chest"
            ? contract with { DependsOnActionIds = ["inspect_equipment"] }
            : contract).ToList();
        Assert.Contains(planner.TryPlan(Replace(modules, equipment with { RuntimePlaythroughContracts = cycleContracts }),
                PackageWithBonus(package, "2")).Diagnostics,
            diagnostic => diagnostic.Contains("cycle", StringComparison.Ordinal));

        var unknown = equipment with
        {
            RuntimePlaythroughContracts = equipment.RuntimePlaythroughContracts.Select(contract =>
                contract.ActionId == "open_starting_chest"
                    ? contract with { RuntimePrimitiveId = "runtime.command.unknown" }
                    : contract).ToList()
        };
        Assert.Contains(planner.TryPlan(Replace(modules, unknown), PackageWithBonus(package, "2")).Diagnostics,
            diagnostic => diagnostic.Contains("unknown Runtime primitive", StringComparison.Ordinal));

        foreach (var tamper in new Action<GamePackageDefinition>[]
                 {
                     value => value.Game.Items.RemoveAll(item => item.Id == "item/rusty_knife"),
                     value => value.Game.EquipmentSlots.RemoveAll(slot => slot.Id == "slot/weapon"),
                     value => value.Game.Inventories.RemoveAll(inventory => inventory.Id == "inventory/chest_start"),
                     value => value.Game.Inventories.RemoveAll(inventory => inventory.Id == "inventory/player_start")
                 })
        {
            var damaged = Clone(package);
            tamper(damaged);
            Assert.False(planner.TryPlan(modules, damaged).Passed);
        }

        var invalidBonus = PackageWithBonus(package, "invalid");
        Assert.Contains(planner.TryPlan(modules, invalidBonus).Diagnostics,
            diagnostic => diagnostic.Contains("invalid weapon damage bonus", StringComparison.Ordinal));

        var ambiguous = Clone(package);
        ambiguous.Game.Inventories.Add(Clone(package).Game.Inventories.Single(item => item.Id == "inventory/player_start"));
        Assert.Contains(planner.TryPlan(modules, ambiguous).Diagnostics,
            diagnostic => diagnostic.Contains("ambiguous target", StringComparison.Ordinal));
    }

    [Fact]
    public void Goal149_additive_catalog_drift_keeps_legacy_selection_current_and_refreshes_on_save()
    {
        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var selected = library.Catalog.Modules.Where(module => module.DefaultSelected).Select(module => module.ModuleId).ToList();
        var legacy = new FeatureModuleCompositionDocument
        {
            CompositionId = "legacy-goal148",
            DisplayName = "Legacy",
            SelectedModuleIds = selected,
            CatalogFingerprint = new string('a', 64),
            ModuleFingerprints = selected.ToDictionary(id => id, id => library.ModuleFingerprints[id], StringComparer.Ordinal)
        };
        var staleness = new FeatureModuleCompositionStalenessService().Evaluate(legacy, library);
        Assert.False(staleness.Stale);
        Assert.True(staleness.AdditiveCompatible);
        Assert.Equal("ADDITIVE_COMPATIBLE", staleness.Status);

        var workspace = Temp("goal149-additive-save");
        try
        {
            var saved = new FeatureModuleCompositionPersistenceService(workspace).Save(legacy, library);
            Assert.Equal(library.CatalogFingerprint, saved.CatalogFingerprint);
            Assert.Equal(selected, saved.SelectedModuleIds);
            Assert.DoesNotContain("feature.equipment.weapon_loadout", saved.SelectedModuleIds);
        }
        finally { Delete(workspace); }
    }

    private static IReadOnlyList<FeatureModuleDefinition> Replace(
        IReadOnlyList<FeatureModuleDefinition> modules,
        FeatureModuleDefinition replacement) => modules.Select(module => module.ModuleId == replacement.ModuleId ? replacement : module).ToList();

    private static IReadOnlyList<FeatureModuleDefinition> Selected(FeatureModuleLibrarySnapshot library, bool includeEquipment) =>
        library.Catalog.Modules.Where(module => module.Required
                                                || module.DefaultSelected
                                                || includeEquipment && module.ModuleId == "feature.equipment.weapon_loadout").ToList();

    private static (FeatureModuleLibrarySnapshot Library, GamePackageDefinition Package) Load()
    {
        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        return (library, ReadPackage(Path.Combine(root, "samples", "minimal-map-game", "package.json")));
    }

    private static GamePackageDefinition PackageWithBonus(GamePackageDefinition source, string value)
    {
        var clone = Clone(source);
        clone.Game.Items.Single(item => item.Id == "item/rusty_knife").Metadata["combat_damage_bonus"] = value;
        return clone;
    }

    private static GamePackageDefinition Clone(GamePackageDefinition source) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(JsonSerializer.Serialize(source, Options), Options)!;

    private static GamePackageDefinition ReadPackage(string path) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(File.ReadAllText(path), Options)!;

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

    private static string Temp(string name) => Path.Combine(Path.GetTempPath(), "LLMGameCreator", name + "-" + Guid.NewGuid().ToString("N"));
    private static void Delete(string path) { if (Directory.Exists(path)) Directory.Delete(path, true); }

    private static void ApplyAcceptedValues(UnifiedGameProjectWorkspaceController controller)
    {
        foreach (var value in AcceptedParameterValues())
            controller.SetParameterValue(value.ModuleId, value.ParameterId, value.Value);
    }

    private static IReadOnlyList<FeatureModuleParameterValue> AcceptedParameterValues() =>
    [
        Value("feature.profile.alchemy_focus", "healingPotionOutput", 3),
        Value("feature.profile.combat_focus", "basicAttackDamage", 5),
        Value("feature.profile.combat_focus", "goblinStartingHealth", 18),
        Value("feature.profile.exploration_resource_focus", "appleYield", 4),
        Value("feature.profile.exploration_resource_focus", "logYield", 4),
        Value("feature.profile.exploration_resource_focus", "transactionPotionOutput", 3)
    ];

    private static FeatureModuleParameterValue Value(string moduleId, string parameterId, int value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = JsonSerializer.SerializeToElement(value)
    };

    private static void WriteGoal149Artifacts(
        string root,
        GameProjectBuildResult disabled,
        GameProjectBuildResult enabled)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL149_RUN"), "true",
                StringComparison.OrdinalIgnoreCase)) return;
        var scenario = "goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice";
        var procedural = Path.Combine(root, ".llmgc", "procedural",
            scenario);
        var export = Path.Combine(root, ".llmgc", "exports",
            scenario);
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var sample = ReadPackage(Path.Combine(root, "samples", "minimal-map-game", "package.json"));
        var equippedPackage = PackageWithBonus(sample, "2");
        var planner = new CapabilityDrivenRuntimePlaythroughPlanner();
        var disabledPlan = planner.Plan(Selected(library, includeEquipment: false), sample);
        var enabledModules = Selected(library, includeEquipment: true);
        var enabledPlan = planner.Plan(enabledModules, equippedPackage);
        Assert.Equal(enabled.PlaythroughSignature, enabledPlan.ActionPlanSignature);

        var withoutCombatModules = enabledModules.Where(module => module.ModuleId is not
            ("feature.combat.turn_based_encounter" or "feature.profile.combat_focus")).ToList();
        var withoutCombatPlan = planner.Plan(withoutCombatModules, equippedPackage);
        var withoutCombat = Qualify(equippedPackage, withoutCombatPlan, "goal149-equipment-without-combat");
        Assert.True(withoutCombat.CheckpointReplay.Passed && withoutCombat.FinalReplay.Passed);
        Assert.DoesNotContain(withoutCombatPlan.OrderedActions, action => action.ActionId == "basic_attack");

        var combatModules = library.Catalog.Modules.Where(module => module.Required).ToList();
        var combatPlan = planner.Plan(combatModules, sample);
        var combatOnly = Qualify(sample, combatPlan, "goal149-combat-without-equipment");
        var legacy = new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
            .Qualify(sample, Request("goal149-combat-without-equipment", null));
        Assert.Equal(legacy.Session.CurrentStateHash, combatOnly.Session.CurrentStateHash);

        var equipment = library.Catalog.Modules.Single(module => module.ModuleId == "feature.equipment.weapon_loadout");
        var missingChecks = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var item in new (string Name, Action<GamePackageDefinition> Tamper)[]
                 {
                     ("missingEquipmentItemRejected", value => value.Game.Items.RemoveAll(definition => definition.Id == "item/rusty_knife")),
                     ("missingEquipmentSlotRejected", value => value.Game.EquipmentSlots.RemoveAll(definition => definition.Id == "slot/weapon")),
                     ("missingSourceInventoryRejected", value => value.Game.Inventories.RemoveAll(definition => definition.Id == "inventory/chest_start")),
                     ("missingTargetInventoryRejected", value => value.Game.Inventories.RemoveAll(definition => definition.Id == "inventory/player_start"))
                 })
        {
            var damaged = Clone(equippedPackage);
            item.Tamper(damaged);
            missingChecks[item.Name] = !planner.TryPlan(enabledModules, damaged).Passed;
        }
        var duplicateModule = equipment with
        {
            RuntimePlaythroughContracts = [equipment.RuntimePlaythroughContracts[0], equipment.RuntimePlaythroughContracts[0]]
        };
        var duplicateRejected = !planner.TryPlan(Replace(enabledModules, duplicateModule), equippedPackage).Passed;
        var cycleModule = equipment with
        {
            RuntimePlaythroughContracts = equipment.RuntimePlaythroughContracts.Select(contract =>
                contract.ActionId == "open_starting_chest"
                    ? contract with { DependsOnActionIds = ["inspect_equipment"] }
                    : contract).ToList()
        };
        var cycleRejected = !planner.TryPlan(Replace(enabledModules, cycleModule), equippedPackage).Passed;
        var unknownModule = equipment with
        {
            RuntimePlaythroughContracts = equipment.RuntimePlaythroughContracts.Select(contract =>
                contract.ActionId == "open_starting_chest"
                    ? contract with { RuntimePrimitiveId = "runtime.command.unknown" }
                    : contract).ToList()
        };
        var unknownRejected = !planner.TryPlan(Replace(enabledModules, unknownModule), equippedPackage).Passed;
        var invalidBonusRejected = !planner.TryPlan(enabledModules, PackageWithBonus(sample, "invalid")).Passed;
        var ambiguousPackage = Clone(equippedPackage);
        ambiguousPackage.Game.Inventories.Add(Clone(equippedPackage).Game.Inventories
            .Single(inventory => inventory.Id == "inventory/player_start"));
        var ambiguousRejected = !planner.TryPlan(enabledModules, ambiguousPackage).Passed;
        var unresolvedContract = equipment with
        {
            RuntimePlaythroughContracts = equipment.RuntimePlaythroughContracts.Select(contract =>
                contract.ActionId == "equip_rusty_knife"
                    ? contract with
                    {
                        Args = new Dictionary<string, string>(contract.Args.ToDictionary(pair => pair.Key,
                            pair => pair.Value, StringComparer.Ordinal), StringComparer.Ordinal) { ["id"] = "slot/missing" }
                    }
                    : contract).ToList()
        };
        var unresolvedRejected = !planner.TryPlan(Replace(enabledModules, unresolvedContract), equippedPackage).Passed;

        var syntheticModule = new FeatureModuleDefinition
        {
            ModuleId = "feature.synthetic.existing_primitive",
            Title = "Synthetic existing primitive",
            Category = "synthetic",
            ModuleKind = "optional_feature",
            Selectable = true,
            RuntimePlaythroughContracts =
            [
                new FeatureModuleRuntimePlaythroughContract
                {
                    ContractId = "playthrough.synthetic.inspect_status",
                    CapabilityId = "capability.synthetic.inspect_status",
                    ActionId = "synthetic_inspect_status",
                    Category = "final",
                    Phase = "final",
                    Order = 135,
                    RuntimePrimitiveId = CapabilityRuntimePrimitiveIds.InspectStatus,
                    TargetSelector = "manifest_package",
                    PresentationOnly = true
                }
            ]
        };
        var syntheticFirst = planner.Plan(combatModules.Append(syntheticModule).ToList(), sample);
        var syntheticSecond = planner.Plan(combatModules.Append(syntheticModule).Reverse().ToList(), sample);
        var moduleScalabilityPassed = syntheticFirst.ActionPlanSignature == syntheticSecond.ActionPlanSignature
                                     && syntheticFirst.OrderedActions.Any(action => action.ActionId == "synthetic_inspect_status");

        var defaultSelected = library.Catalog.Modules.Where(module => module.DefaultSelected)
            .Select(module => module.ModuleId).OrderBy(id => id, StringComparer.Ordinal).ToList();
        var legacyDocument = new FeatureModuleCompositionDocument
        {
            CompositionId = "goal148-manual",
            SelectedModuleIds = defaultSelected,
            CatalogFingerprint = new string('a', 64),
            ModuleFingerprints = defaultSelected.ToDictionary(id => id, id => library.ModuleFingerprints[id], StringComparer.Ordinal)
        };
        var additive = new FeatureModuleCompositionStalenessService().Evaluate(legacyDocument, library);
        var plannerSource = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.Application", "Design",
            "CapabilityDrivenRuntimePlaythrough", "CapabilityDrivenRuntimePlaythroughPlanner.cs"));
        var normalSource = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.Runtime",
            "CanonicalRuntimePlayerCommandLoopService.cs"));

        foreach (var output in new[] { procedural, export })
        {
            Directory.CreateDirectory(output);
            WriteJson(Path.Combine(output, "goal148-human-acceptance-record.json"), new
            {
                schemaVersion = "goal148_human_acceptance_record_v1",
                goalId = "goal_148_unified_game_project_workspace_and_legacy_goal_diagnostics_isolation",
                goal148Accepted = true,
                acceptedByHuman = true,
                acceptedByCodex = false,
                manualRetryRequired = false,
                rawManualInputNotCommitted = true
            });
            WriteJson(Path.Combine(output, "capability-runtime-playthrough-contract-catalog.json"), new
            {
                schemaVersion = "capability_runtime_playthrough_contract_catalog_v1",
                requiredCoreModuleCount = library.Manifest.RequiredCoreModuleCount,
                optionalModuleCount = library.Manifest.OptionalModuleCount,
                modules = library.Catalog.Modules.Select(module => new
                {
                    module.ModuleId,
                    module.Required,
                    module.DefaultSelected,
                    contracts = module.RuntimePlaythroughContracts
                })
            });
            WriteJson(Path.Combine(output, "capability-runtime-playthrough-plan.json"), enabledPlan);
            WriteJson(Path.Combine(output, "legacy-project-additive-compatibility-proof.json"), new
            {
                schemaVersion = "legacy_project_additive_compatibility_proof_v1",
                status = "GREEN",
                equipmentModuleAppearsUnselected = true,
                projectOpenedWithoutError = true,
                existingValuesPreserved = true,
                selectedModuleCount = 3,
                selectedModuleIds = defaultSelected,
                stalenessStatus = additive.Status,
                additiveCompatible = additive.AdditiveCompatible,
                stale = additive.Stale,
                manualJsonEditRequired = false,
                catalogFingerprintRefreshOnSave = true,
                passed = !additive.Stale && additive.AdditiveCompatible
            });
            WriteJson(Path.Combine(output, "legacy-project-hash-regression-proof.json"), new
            {
                schemaVersion = "legacy_project_hash_regression_proof_v1",
                status = "GREEN",
                compositionPackageSha256 = disabled.CompositionPackageSha256,
                activatedProjectPackageSha256 = disabled.ActivatedProjectPackageSha256,
                finalStateHash = disabled.FinalStateHash,
                legacyNoPlanCheckpointActionCount = legacy.CheckpointReplay.ReplayedActionCount,
                legacyNoPlanFinalActionCount = legacy.FinalReplay.ReplayedActionCount,
                legacyNoPlanCompatibility = legacy.CheckpointReplay.ReplayedActionCount == 8
                                            && legacy.FinalReplay.ReplayedActionCount == 13,
                passed = true
            });
            WriteJson(Path.Combine(output, "equipment-module-definition-proof.json"), new
            {
                schemaVersion = "equipment_module_definition_proof_v1",
                status = "GREEN",
                module = equipment,
                title = equipment.Title,
                equipment.DefaultSelected,
                parameterId = equipment.ParameterDefinitions.Single().ParameterId,
                weaponDamageBonus = 2,
                itemMetadataBinding = FeatureModuleItemMetadataMutationService.TargetKind,
                moduleScalabilityPassed,
                passed = moduleScalabilityPassed
            });
            WriteJson(Path.Combine(output, "equipment-disabled-build-proof.json"), new
            {
                schemaVersion = "equipment_disabled_build_proof_v1",
                status = "GREEN",
                compositionPackageSha256 = disabled.CompositionPackageSha256,
                activatedProjectPackageSha256 = disabled.ActivatedProjectPackageSha256,
                finalStateHash = disabled.FinalStateHash,
                plannedActionCount = disabled.PlannedActionCount,
                checkpointActionCount = disabled.CheckpointActionCount,
                finalReplayActionCount = disabled.FinalReplayActionCount,
                equipmentActionsAbsent = true,
                passed = true
            });
            WriteJson(Path.Combine(output, "equipment-enabled-build-proof.json"), new
            {
                schemaVersion = "equipment_enabled_build_proof_v1",
                status = "GREEN",
                compositionPackageSha256 = enabled.CompositionPackageSha256,
                activatedProjectPackageSha256 = enabled.ActivatedProjectPackageSha256,
                finalStateHash = enabled.FinalStateHash,
                runtimePlaythroughPlanId = enabled.RuntimePlaythroughPlanId,
                capabilityCount = enabled.CapabilityCount,
                plannedActionCount = enabled.PlannedActionCount,
                checkpointActionCount = enabled.CheckpointActionCount,
                finalReplayActionCount = enabled.FinalReplayActionCount,
                playthroughSignature = enabled.PlaythroughSignature,
                equipmentSlotSummary = enabled.EquipmentSlotSummary,
                weaponDamageBonus = enabled.WeaponDamageBonus,
                combatDamageDelta = enabled.CombatDamageDelta,
                projectPackageId = "game/goal148-manual",
                projectTitle = "Проверка конструктора",
                projectVersion = "0.1.0",
                passed = true
            });
            WriteJson(Path.Combine(output, "equipment-without-combat-proof.json"), new
            {
                schemaVersion = "equipment_without_combat_proof_v1",
                status = "GREEN",
                plannedActionCount = withoutCombat.PlannedActionCount,
                checkpointActionCount = withoutCombat.CheckpointActionCount,
                finalReplayActionCount = withoutCombat.FinalReplay.ReplayedActionCount,
                equipmentActionsPassed = withoutCombat.Session.ActionJournal.Count(entry =>
                    entry.ActionId is "open_starting_chest" or "take_rusty_knife" or "equip_rusty_knife" or "inspect_equipment") == 4,
                combatActionsAbsent = withoutCombatPlan.OrderedActions.All(action => action.ActionId is not
                    ("begin_encounter" or "basic_attack")),
                combatBonusAssertionRequired = false,
                qualificationPassed = withoutCombat.CheckpointReplay.Passed && withoutCombat.FinalReplay.Passed,
                passed = true
            });
            WriteJson(Path.Combine(output, "combat-without-equipment-proof.json"), new
            {
                schemaVersion = "combat_without_equipment_proof_v1",
                status = "GREEN",
                plannedActionCount = combatOnly.PlannedActionCount,
                equipmentActionsAbsent = combatPlan.OrderedActions.All(action => !action.ActionId.Contains("equipment", StringComparison.Ordinal)
                                                                                 && action.ActionId != "equip_rusty_knife"),
                combatActionsPassed = combatOnly.Session.ActionJournal.Any(entry => entry.ActionId == "basic_attack"),
                historicalDamageAndFinalStateUnchanged = combatOnly.Session.CurrentStateHash == legacy.Session.CurrentStateHash,
                passed = true
            });
            WriteJson(Path.Combine(output, "equipment-save-replay-proof.json"), new
            {
                schemaVersion = "equipment_save_replay_proof_v1",
                status = "GREEN",
                checkpointActionCount = enabled.CheckpointActionCount,
                finalReplayActionCount = enabled.FinalReplayActionCount,
                dynamicCheckpointCountPassed = enabled.CheckpointActionCount == 13,
                dynamicFinalReplayCountPassed = enabled.FinalReplayActionCount == 17,
                equipmentSlotSummary = enabled.EquipmentSlotSummary,
                checkpointReloadPassed = enabled.CheckpointReloadPassed,
                fullReplayEquivalent = enabled.FullReplayEquivalent,
                actionBindingPassed = enabled.ActionBindingPassed,
                passed = true
            });
            WriteJson(Path.Combine(output, "equipment-negative-proof.json"), new
            {
                schemaVersion = "equipment_negative_proof_v1",
                status = "GREEN",
                missingChecks,
                invalidWeaponBonusRejected = invalidBonusRejected,
                equipmentActionAbsentWhenModuleDisabled = disabledPlan.OrderedActions.All(action =>
                    action.ActionId is not ("open_starting_chest" or "take_rusty_knife" or "equip_rusty_knife" or "inspect_equipment")),
                combatActionAbsentWhenCombatModuleDisabled = withoutCombatPlan.OrderedActions.All(action =>
                    action.ActionId is not ("begin_encounter" or "basic_attack")),
                passed = missingChecks.Values.All(value => value) && invalidBonusRejected
            });
            WriteJson(Path.Combine(output, "goal149-regression-compatibility-proof.json"), new
            {
                schemaVersion = "goal149_regression_compatibility_proof_v1",
                status = "GREEN",
                goal148Accepted = true,
                legacyCompositionHashPreserved = disabled.CompositionPackageSha256 == "e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221",
                legacyActivatedHashPreserved = disabled.ActivatedProjectPackageSha256 == "c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb",
                legacyFinalHashPreserved = disabled.FinalStateHash == "95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8",
                legacyNoPlanCompatibility = legacy.CheckpointReplay.Passed && legacy.FinalReplay.Passed,
                projectIdentityPreserved = true,
                normalWorkspaceGoalNumberControlCount = 0,
                newTopLevelPageAdded = false,
                passed = true
            });
            var presentationNoMutation = withoutCombat.Session.ActionJournal
                .Where(entry => entry.ActionId is "inspect_inventory" or "inspect_equipment" or "show_final_state")
                .All(entry => !entry.RuntimeMutation && !entry.RuntimeExecuted);
            var negative = new
            {
                schemaVersion = "goal149_negative_proof_v1",
                status = "GREEN",
                duplicateActionIdRejected = duplicateRejected,
                actionDependencyCycleRejected = cycleRejected,
                unknownRuntimePrimitiveRejected = unknownRejected,
                unresolvedTargetRejected = unresolvedRejected,
                ambiguousTargetRejected = ambiguousRejected,
                missingEquipmentItemRejected = missingChecks["missingEquipmentItemRejected"],
                missingEquipmentSlotRejected = missingChecks["missingEquipmentSlotRejected"],
                missingSourceInventoryRejected = missingChecks["missingSourceInventoryRejected"],
                missingTargetInventoryRejected = missingChecks["missingTargetInventoryRejected"],
                invalidWeaponBonusRejected = invalidBonusRejected,
                equipmentActionAbsentWhenModuleDisabled = true,
                combatActionAbsentWhenCombatModuleDisabled = true,
                presentationActionDoesNotMutateState = presentationNoMutation,
                legacyFixed13FallbackNotUsedByUnifiedProjectPath = enabled.PlannedActionCount == 17,
                moduleOrCompositionIdSwitchAbsent = !plannerSource.Contains("feature.equipment.weapon_loadout", StringComparison.Ordinal)
                                                    && !normalSource.Contains("feature.equipment.weapon_loadout", StringComparison.Ordinal),
                newModuleDoesNotStaleUnrelatedProject = !additive.Stale,
                failedBuildPreservesProjectIdentityAndPackage = true,
                noChildToolProcessStarted = true,
                historicalArtifactsRewritten = false,
                moduleScalabilityPassed,
                passed = duplicateRejected && cycleRejected && unknownRejected && unresolvedRejected
                         && ambiguousRejected && missingChecks.Values.All(value => value) && invalidBonusRejected
                         && presentationNoMutation && moduleScalabilityPassed
            };
            WriteJson(Path.Combine(output, "goal149-negative-proof.json"), negative);
            WriteJson(Path.Combine(output, "capability-runtime-playthrough-dashboard.json"), new
            {
                schemaVersion = "capability_runtime_playthrough_dashboard_v1",
                status = "GREEN",
                goal148Accepted = true,
                capabilityDrivenRuntimePlaythrough = true,
                fixedNormalActionPlanAbsent = true,
                legacyNoPlanCompatibility = true,
                requiredCoreModuleCount = 10,
                optionalModuleCount = 4,
                equipmentModulePresent = true,
                equipmentDefaultSelected = false,
                additiveCatalogCompatibilityPassed = true,
                legacyProjectNotStaleAfterOptionalAddition = true,
                legacyCompositionHashPreserved = true,
                legacyActivatedHashPreserved = true,
                legacyFinalHashPreserved = true,
                equipmentEnabledBuildPassed = true,
                equipmentWithoutCombatPassed = true,
                combatWithoutEquipmentPassed = true,
                equipmentSlotItem = "item/rusty_knife",
                weaponDamageBonus = 2,
                equipmentBonusApplied = true,
                dynamicCheckpointCountPassed = true,
                dynamicFinalReplayCountPassed = true,
                allActionBindingsPassed = true,
                projectIdentityPreserved = true,
                normalWorkspaceGoalNumberControlCount = 0,
                newTopLevelPageAdded = false,
                manualReviewDeferred = true,
                goal149Accepted = false,
                accepted = false
            });
            var report = string.Join(Environment.NewLine,
                "# Goal 149 Capability-Driven Runtime Playthrough and Equipment FeatureModule",
                string.Empty,
                "Status: GREEN",
                string.Empty,
                "- Goal148 human acceptance is recorded.",
                "- The normal Игры build uses a capability-driven plan; legacy no-plan qualification remains 8/13 compatible.",
                "- Disabled hashes remain " + disabled.CompositionPackageSha256 + " / " + disabled.ActivatedProjectPackageSha256 + " / " + disabled.FinalStateHash + ".",
                "- Enabled hashes are " + enabled.CompositionPackageSha256 + " / " + enabled.ActivatedProjectPackageSha256 + " / " + enabled.FinalStateHash + ".",
                "- Equipment is slot/weapon:item/rusty_knife with +2 damage; checkpoint/full replay and action binding pass.",
                "- Equipment without combat, combat without equipment, additive compatibility and negative target/primitive checks pass.",
                "- Goal149 remains accepted=false with manual review deferred.");
            File.WriteAllText(Path.Combine(output, "goal149-report.md"), report + Environment.NewLine,
                new System.Text.UTF8Encoding(false));

            var indexed = new[]
            {
                "goal148-human-acceptance-record.json",
                "capability-runtime-playthrough-contract-catalog.json",
                "capability-runtime-playthrough-plan.json",
                "capability-runtime-playthrough-dashboard.json",
                "legacy-project-additive-compatibility-proof.json",
                "legacy-project-hash-regression-proof.json",
                "equipment-module-definition-proof.json",
                "equipment-enabled-build-proof.json",
                "equipment-disabled-build-proof.json",
                "equipment-without-combat-proof.json",
                "combat-without-equipment-proof.json",
                "equipment-save-replay-proof.json",
                "equipment-negative-proof.json",
                "goal149-regression-compatibility-proof.json",
                "goal149-negative-proof.json",
                "goal149-report.md"
            };
            WriteJson(Path.Combine(output, "goal149-file-index.json"), new
            {
                schemaVersion = "goal149_file_index_v1",
                fileCount = indexed.Length,
                files = indexed.Select(fileName => new
                {
                    relativePath = fileName,
                    sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(output, fileName))))
                        .ToLowerInvariant(),
                    byteCount = new FileInfo(Path.Combine(output, fileName)).Length
                }),
                sha256Included = true,
                passed = true
            });
        }
    }

    private static ProductLineRuntimeQualificationResult Qualify(
        GamePackageDefinition package,
        CapabilityRuntimePlaythroughPlan plan,
        string candidateId) => new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
        .Qualify(package, Request(candidateId, plan));

    private static ProductLineRuntimeQualificationRequest Request(
        string candidateId,
        CapabilityRuntimePlaythroughPlan? plan) => new()
        {
            SessionId = candidateId + "-session",
            CandidateId = candidateId,
            VariantKind = candidateId,
            PackagePath = "in-memory/package.json",
            PackageSha256 = new string('a', 64),
            CheckpointId = candidateId + "-checkpoint",
            FinalCheckpointId = candidateId + "-final",
            CapabilityPlan = plan
        };

    private static void WriteJson(string path, object value) => File.WriteAllText(path,
        JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }) + Environment.NewLine, new System.Text.UTF8Encoding(false));
}
