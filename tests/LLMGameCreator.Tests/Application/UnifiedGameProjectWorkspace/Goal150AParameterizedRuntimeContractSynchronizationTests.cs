using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;

public sealed class Goal150AParameterizedRuntimeContractSynchronizationTests
{
    private static readonly string[] SelectedModules =
    [
        "feature.equipment.weapon_loadout",
        "feature.character.attributes",
        "feature.character.level_progression"
    ];

    [Fact]
    public async Task Custom_3_8_2_12_workspace_build_persists_reopens_and_rebuilds_deterministically()
    {
        var root = FindRoot();
        var temp = Temp("goal150a-custom-workspace");
        try
        {
            var library = Load(root);
            var firstController = await CreateWorkspace(root, temp, library);
            ApplyCustomSelection(firstController);
            var first = firstController.BuildAndQualify();
            Assert.True(first.Passed, first.HumanSummary + Environment.NewLine + string.Join(Environment.NewLine, first.Diagnostics));
            AssertCustomBuild(first);
            Assert.Equal(12, first.CertificationExecutedCount);

            var package = await new JsonGamePackageRepository().LoadAsync(temp, CancellationToken.None);
            Assert.Equal("3", package.Game.Items.Single(item => item.Id == "item/rusty_knife").Metadata["combat_damage_bonus"]);
            Assert.Equal(8, package.Game.Stats.Single(stat => stat.Id == "stat/strength").DefaultValue);
            Assert.Equal(8, package.Game.Encounters.Single(item => item.Id == "encounter/goblin_duel")
                .Participants.Single(item => item.Id == "player").Stats.Single(stat => stat.Id == "stat/strength").Amount);
            Assert.Equal("2", package.Game.Abilities.Single(ability => ability.Id == "ability/basic_attack")
                .Metadata["source_stat_damage_per_point"]);
            Assert.Equal(12, package.Game.Progressions.Single(item => item.Id == "progression/character_level")
                .Stages.Single(stage => stage.Id == "level/2").RequiredAmount);

            var reopened = await OpenWorkspace(root, temp);
            var snapshot = reopened.Snapshot();
            Assert.All(SelectedModules, id => Assert.Contains(snapshot.Mechanics, mechanic => mechanic.ModuleId == id && mechanic.Selected));
            Assert.Equal("3", Parameter(snapshot, SelectedModules[0], "weaponDamageBonus"));
            Assert.Equal("8", Parameter(snapshot, SelectedModules[1], "startingStrength"));
            Assert.Equal("2", Parameter(snapshot, SelectedModules[1], "damagePerStrengthPoint"));
            Assert.Equal("12", Parameter(snapshot, SelectedModules[2], "level2RequiredExperience"));

            var second = reopened.BuildAndQualify();
            Assert.True(second.Passed, second.HumanSummary + Environment.NewLine + string.Join(Environment.NewLine, second.Diagnostics));
            AssertCustomBuild(second);
            Assert.Equal(0, second.CertificationExecutedCount);
            Assert.Equal(12, second.CertificationReusedCount);
            Assert.Equal(first.CompositionPackageSha256, second.CompositionPackageSha256);
            Assert.Equal(first.ActivatedProjectPackageSha256, second.ActivatedProjectPackageSha256);
            Assert.Equal(first.FinalStateHash, second.FinalStateHash);
            Assert.Equal(first.PlaythroughSignature, second.PlaythroughSignature);
        }
        finally { Delete(temp); }
    }

    [Fact]
    public void Parameter_matrix_drives_exact_package_action_and_Runtime_effect_values()
    {
        var root = FindRoot();
        var library = Load(root);
        var output = Temp("goal150a-parameter-matrix");
        try
        {
            VerifyMatrix(root, output, library, [SelectedModules[0], SelectedModules[1]],
                SelectedModules[0], "weaponDamageBonus", [0m, 3m, 10m]);
            VerifyMatrix(root, output, library, [SelectedModules[1]],
                SelectedModules[1], "startingStrength", [0m, 8m, 20m]);
            VerifyMatrix(root, output, library, [SelectedModules[1]],
                SelectedModules[1], "damagePerStrengthPoint", [0m, 0.5m, 2m, 5m]);
            VerifyMatrix(root, output, library, [SelectedModules[2]],
                SelectedModules[2], "level2RequiredExperience", [1m, 12m, 1000m]);
        }
        finally { Delete(output); }
    }

    [Fact]
    public void Effective_binding_contract_rejects_invalid_targets_references_arithmetic_and_cycles()
    {
        var library = Load(FindRoot());
        var attributes = Module(library.Catalog, SelectedModules[1]);
        var progression = Module(library.Catalog, SelectedModules[2]);
        var binder = new FeatureModuleParameterBindingService();

        AssertRejected("unknown or unselected parameter reference", Bind(attributes with
        {
            EffectiveValueBindings = [Binding("bad", "runtime_effect_expected_value",
                "runtime_effect.player_strength_equals", "expectedValue", "${parameter:feature.character.level_progression.level2RequiredExperience}")]
        }, [SelectedModules[1]]));
        AssertRejected("unknown mutation operation target", Bind(attributes with
        {
            EffectiveValueBindings = [Binding("bad", "mutation_operation_field", "missing.operation", "newValue", "1")]
        }));
        AssertRejected("unknown Runtime effect target", Bind(attributes with
        {
            EffectiveValueBindings = [Binding("bad", "runtime_effect_expected_value", "missing.effect", "expectedValue", "1")]
        }));
        AssertRejected("unknown Runtime playthrough action target", Bind(progression with
        {
            EffectiveValueBindings = [Binding("bad", "runtime_playthrough_arg", "missing.action", "amount", "1")]
        }, [SelectedModules[2]]));
        AssertRejected("duplicate binding target", Bind(attributes with
        {
            EffectiveValueBindings =
            [
                Binding("a", "runtime_effect_expected_value", "runtime_effect.player_strength_equals", "expectedValue", "1"),
                Binding("b", "runtime_effect_expected_value", "runtime_effect.player_strength_equals", "expectedValue", "2")
            ]
        }));
        AssertRejected("incompatible target field", Bind(attributes with
        {
            EffectiveValueBindings = [Binding("bad", "runtime_effect_expected_value",
                "runtime_effect.player_strength_equals", "comparisonKind", "1")]
        }));
        AssertRejected("nonnumeric value", Bind(attributes with
        {
            EffectiveValueBindings = [Binding("bad", "runtime_effect_expected_value",
                "runtime_effect.player_strength_equals", "expectedValue", "not-a-number")]
        }));
        AssertRejected("division by zero", Bind(attributes with
        {
            EffectiveValueBindings = [Binding("bad", "runtime_effect_expected_value",
                "runtime_effect.player_strength_equals", "expectedValue", "1 / (2 - 2)")]
        }));
        AssertRejected("unsupported effective binding target kind", Bind(attributes with
        {
            EffectiveValueBindings = [Binding("bad", "runtime_playthrough_field",
                "inspect_character_attributes", "title", "1")]
        }));

        var syntheticOperations = attributes.MutationOperations.Concat(
        [
            attributes.MutationOperations[0] with { OperationId = "synthetic.a" },
            attributes.MutationOperations[0] with { OperationId = "synthetic.b" }
        ]).ToList();
        AssertRejected("cycle", Bind(attributes with
        {
            MutationOperations = syntheticOperations,
            EffectiveValueBindings =
            [
                Binding("cycle.a", "mutation_operation_field", "synthetic.a", "newValue", "${operation:synthetic.b.newValue}"),
                Binding("cycle.b", "mutation_operation_field", "synthetic.b", "newValue", "${operation:synthetic.a.newValue}")
            ]
        }));

        FeatureModuleParameterBindingResult Bind(FeatureModuleDefinition module, IReadOnlyList<string>? selected = null)
        {
            var catalog = library.Catalog with
            {
                Modules = library.Catalog.Modules.Select(item => item.ModuleId == module.ModuleId ? module : item).ToList()
            };
            return binder.Bind(catalog, selected ?? [module.ModuleId], []);
        }
    }

    [Fact]
    public void Two_stat_scaled_abilities_keep_basic_attack_execution_and_event_bound_summary_unambiguous()
    {
        var root = FindRoot();
        var library = Load(root);
        var bound = Bind(library, []);
        Assert.True(bound.Passed, string.Join(Environment.NewLine, bound.Diagnostics));
        var packageJson = File.ReadAllText(Path.Combine(root, "samples", "minimal-map-game", "package.json"));
        var mutation = new FeatureModulePackageMutationService().Apply(packageJson, bound.EffectiveMutationOperations);
        Assert.True(mutation.Passed, string.Join(Environment.NewLine, mutation.Diagnostics));
        var package = Deserialize(mutation.PackageJson);
        var basic = package.Game.Abilities.Single(ability => ability.Id == "ability/basic_attack");
        var second = JsonSerializer.Deserialize<AbilityDefinition>(JsonSerializer.Serialize(basic, Options), Options)!;
        second.Id = "ability/second_strength_attack";
        second.Name = "Second strength attack";
        package.Game.Abilities.Add(second);

        var selected = bound.EffectiveCatalog.Modules
            .Where(module => module.Required || SelectedModules.Contains(module.ModuleId, StringComparer.Ordinal)).ToList();
        var plan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(selected, package);
        Assert.Single(plan.OrderedActions, action => action.ActionId == "basic_attack");
        var result = new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
            .Qualify(package, new ProductLineRuntimeQualificationRequest
            {
                SessionId = "goal150a-two-abilities-session",
                CandidateId = "goal150a-two-abilities",
                VariantKind = "synthetic_two_stat_scaled_abilities",
                PackagePath = "in-memory/package.json",
                PackageSha256 = new string('a', 64),
                CheckpointId = "goal150a-two-abilities-checkpoint",
                FinalCheckpointId = "goal150a-two-abilities-final",
                CapabilityPlan = plan
            });
        Assert.True(result.CheckpointReplay.Passed);
        Assert.True(result.FinalReplay.Passed);
        Assert.True(result.ActionDescriptorExecutionBindingPassed);
        var damage = result.Session.LatestSnapshot.RuntimeEvents.Last(item => item.EventType == "DamageApplied");
        Assert.Equal("2", damage.Args["statDamageBonus"]);
        Assert.Equal("2", damage.Args["equipmentDamageBonus"]);
        Assert.Equal("4", damage.Args["totalAdditionalDamage"]);
        var summarySource = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.Application", "Design",
            "UnifiedGameProjectWorkspace", "GameProjectBuildAndQualificationService.cs"));
        Assert.DoesNotContain("qualifiedPackage.Game.Abilities.Single", summarySource, StringComparison.Ordinal);
        Assert.Contains("DamageApplied", summarySource, StringComparison.Ordinal);
    }

    [Fact]
    public void Binding_contract_fingerprint_changes_invalidate_only_the_affected_module_closure()
    {
        var root = FindRoot();
        var library = Load(root);
        foreach (var moduleId in SelectedModules)
            VerifyBindingFingerprintInvalidation(root, library, moduleId);
    }

    private static void VerifyMatrix(
        string root,
        string output,
        FeatureModuleLibrarySnapshot library,
        IReadOnlyList<string> selectedModules,
        string moduleId,
        string parameterId,
        IReadOnlyList<decimal> values)
    {
        foreach (var value in values)
        {
            var supplied = Value(moduleId, parameterId, value);
            var binding = Bind(library, [supplied], selectedModules);
            Assert.True(binding.Passed, string.Join(Environment.NewLine, binding.Diagnostics));
            var id = parameterId + "-" + value.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace('.', '_');
            var qualification = new FeatureModuleCompositionService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
                .ComposeAndQualify(root, binding.EffectiveCatalog, selectedModules, Path.Combine(output, id),
                    "goal150a-" + id, useCapabilityDrivenRuntimePlaythrough: true);
            Assert.True(qualification.Result.Passed,
                string.Join(Environment.NewLine, qualification.Result.Diagnostics)
                + Environment.NewLine + string.Join(Environment.NewLine,
                    qualification.Artifacts.SemanticEffects.Observations.Select(observation =>
                        observation.EffectId + ":expected=" + observation.ExpectedValue + ":actual="
                        + observation.ActualValue + ":passed=" + observation.Passed + ":"
                        + string.Join("|", observation.Diagnostics))));
            var package = Deserialize(qualification.Artifacts.PackageJson);
            var observations = qualification.Artifacts.SemanticEffects.Observations;
            if (parameterId == "weaponDamageBonus")
            {
                Assert.Equal(Format(value), package.Game.Items.Single(item => item.Id == "item/rusty_knife").Metadata["combat_damage_bonus"]);
                AssertExact(observations, "runtime_effect.equipment_combat_damage_delta", value);
            }
            else if (parameterId == "startingStrength")
            {
                Assert.Equal((double)value, package.Game.Stats.Single(stat => stat.Id == "stat/strength").DefaultValue);
                AssertExact(observations, "runtime_effect.player_strength_equals", value);
                AssertExact(observations, "runtime_effect.combat_stat_damage_delta", (value - 5m));
            }
            else if (parameterId == "damagePerStrengthPoint")
            {
                Assert.Equal(Format(value), package.Game.Abilities.Single(ability => ability.Id == "ability/basic_attack")
                    .Metadata["source_stat_damage_per_point"]);
                AssertExact(observations, "runtime_effect.combat_stat_damage_delta", 2m * value);
            }
            else
            {
                Assert.Equal((double)value, package.Game.Progressions.Single(item => item.Id == "progression/character_level")
                    .Stages.Single(stage => stage.Id == "level/2").RequiredAmount);
                var action = qualification.Artifacts.Session.CapabilityPlan!.OrderedActions
                    .Single(item => item.ActionId == "gain_character_experience");
                Assert.Equal(Format(value), action.Args["amount"]);
                AssertExact(observations, "runtime_effect.character_progression_amount", value);
                Assert.Equal("level/2", observations.Single(item => item.EffectId == "runtime_effect.character_progression_stage").ActualValue);
            }
        }
    }

    private static void VerifyBindingFingerprintInvalidation(
        string root,
        FeatureModuleLibrarySnapshot library,
        string moduleId)
    {
        var cache = Temp("goal150a-binding-cert-cache");
        var output = Temp("goal150a-binding-cert-output");
        try
        {
            var service = new FeatureModuleCertificationService(
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                new FeatureModuleCertificationCache(cache));
            var first = service.Certify(root, library, new string('a', 64), output);
            var second = service.Certify(root, library, new string('a', 64), output);
            Assert.Equal(12, first.ExecutedCount);
            Assert.Equal(12, second.ReusedCount);

            var changedModule = Module(library.Catalog, moduleId) with
            {
                EffectiveValueBindings = Module(library.Catalog, moduleId).EffectiveValueBindings.Select((binding, index) =>
                    index == 0 ? binding with { ValueExpression = "(" + binding.ValueExpression + ") + 0" } : binding).ToList()
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
            var changed = service.Certify(root, changedLibrary, new string('a', 64), output);
            Assert.Equal(1, changed.ExecutedCount);
            Assert.Equal(11, changed.ReusedCount);
            Assert.Equal(1, changed.InvalidatedCount);
            Assert.NotEqual(second.Entries.Single(entry => entry.ModuleId == moduleId).ModuleFingerprint,
                changed.Entries.Single(entry => entry.ModuleId == moduleId).ModuleFingerprint);
            Assert.All(changed.Entries.Where(entry => entry.ModuleId.StartsWith("feature.profile.", StringComparison.Ordinal)),
                entry => Assert.Equal(second.Entries.Single(previous => previous.ModuleId == entry.ModuleId).ModuleFingerprint,
                    entry.ModuleFingerprint));
        }
        finally { Delete(cache); Delete(output); }
    }

    private static void AssertExact(IReadOnlyList<FeatureModuleRuntimeEffectObservation> observations, string effectId, decimal value)
    {
        var observation = observations.Single(item => item.EffectId == effectId);
        Assert.True(observation.Passed, string.Join(Environment.NewLine, observation.Diagnostics));
        Assert.Equal(Format(value), observation.ExpectedValue);
        Assert.Equal(value, decimal.Parse(observation.ActualValue, System.Globalization.CultureInfo.InvariantCulture));
    }

    private static FeatureModuleParameterBindingResult Bind(
        FeatureModuleLibrarySnapshot library,
        IReadOnlyList<FeatureModuleParameterValue> values,
        IReadOnlyList<string>? selectedModules = null) =>
        new FeatureModuleParameterBindingService().Bind(library.Catalog, selectedModules ?? SelectedModules, values);

    private static FeatureModuleEffectiveValueBinding Binding(
        string id, string kind, string targetId, string field, string expression) => new()
    {
        BindingId = id,
        TargetKind = kind,
        TargetId = targetId,
        TargetField = field,
        ValueExpression = expression
    };

    private static void AssertRejected(string marker, FeatureModuleParameterBindingResult result)
    {
        Assert.False(result.Passed);
        Assert.Contains(marker, string.Join(";", result.Diagnostics), StringComparison.OrdinalIgnoreCase);
    }

    internal static void AssertCustomBuild(GameProjectBuildResult result)
    {
        Assert.Equal(3, result.WeaponDamageBonus);
        Assert.Equal(3, result.CombatDamageDelta);
        Assert.Equal(6, result.StatDamageBonus);
        Assert.Equal(9, result.TotalAdditionalDamage);
        Assert.Contains("Экипировано: Ржавый нож", result.HumanSummary, StringComparison.Ordinal);
        Assert.Contains("Бонус урона: +3", result.HumanSummary, StringComparison.Ordinal);
        Assert.Contains("Сила: 8", result.HumanSummary, StringComparison.Ordinal);
        Assert.Contains("Бонус урона от силы: +6", result.HumanSummary, StringComparison.Ordinal);
        Assert.Contains("Уровень: 2", result.HumanSummary, StringComparison.Ordinal);
        Assert.Contains("Опыт: 12", result.HumanSummary, StringComparison.Ordinal);
        Assert.Contains("stat/strength=8", result.AttributesSummary, StringComparison.Ordinal);
        Assert.Contains("progression/character_level=12:level/2", result.ProgressionSummary, StringComparison.Ordinal);
        Assert.True(result.CheckpointReloadPassed);
        Assert.True(result.FullReplayEquivalent);
        Assert.True(result.ActionBindingPassed);
        Assert.True(result.PackageActivated);
        Assert.True(result.PackageActivationTransactional);
        Assert.True(result.StagedProjectValidationPassed);
        Assert.True(result.RealProjectValidationPassed);
    }

    internal static async Task<UnifiedGameProjectWorkspaceController> CreateWorkspace(
        string root, string projectFolder, FeatureModuleLibrarySnapshot library)
    {
        Directory.CreateDirectory(projectFolder);
        foreach (var name in new[] { "assets", "scripts", "saves" }) Directory.CreateDirectory(Path.Combine(projectFolder, name));
        File.Copy(Path.Combine(root, ".llmgc", "procedural",
                "goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix",
                "compositions", "minimal-map-game-composed-alchemy-combat-exploration", "package.json"),
            Path.Combine(projectFolder, "package.json"));
        var persistence = new FeatureModuleCompositionPersistenceService(Path.Combine(projectFolder, ".llmgc", "authoring"));
        var document = persistence.CreateNew(UnifiedGameProjectWorkspaceVocabulary.LegacyCompositionId,
            "Проверка конструктора", "Настройки механик открытого игрового проекта.", library) with
        {
            ParameterValues = AcceptedProfileValues(),
            LastQualificationStatus = "GREEN"
        };
        persistence.Save(document, library);
        return await OpenWorkspace(root, projectFolder);
    }

    internal static async Task<UnifiedGameProjectWorkspaceController> OpenWorkspace(string root, string projectFolder)
    {
        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(projectFolder, CancellationToken.None);
        var controller = new UnifiedGameProjectWorkspaceController(current,
            new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(root,
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), repository,
                new GamePackageValidator(), current));
        controller.OpenProject(projectFolder);
        return controller;
    }

    internal static void ApplyCustomSelection(UnifiedGameProjectWorkspaceController controller)
    {
        foreach (var module in SelectedModules) controller.SetModuleSelected(module, true);
        controller.SetParameterValue(SelectedModules[0], "weaponDamageBonus", JsonSerializer.SerializeToElement(3));
        controller.SetParameterValue(SelectedModules[1], "startingStrength", JsonSerializer.SerializeToElement(8));
        controller.SetParameterValue(SelectedModules[1], "damagePerStrengthPoint", JsonSerializer.SerializeToElement(2));
        controller.SetParameterValue(SelectedModules[2], "level2RequiredExperience", JsonSerializer.SerializeToElement(12));
    }

    private static IReadOnlyList<FeatureModuleParameterValue> AcceptedProfileValues() =>
    [
        Value("feature.profile.alchemy_focus", "healingPotionOutput", 3m),
        Value("feature.profile.combat_focus", "basicAttackDamage", 5m),
        Value("feature.profile.combat_focus", "goblinStartingHealth", 18m),
        Value("feature.profile.exploration_resource_focus", "appleYield", 4m),
        Value("feature.profile.exploration_resource_focus", "logYield", 4m),
        Value("feature.profile.exploration_resource_focus", "transactionPotionOutput", 3m)
    ];

    private static FeatureModuleParameterValue Value(string moduleId, string parameterId, decimal value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = decimal.Truncate(value) == value
            ? JsonSerializer.SerializeToElement((int)value)
            : JsonSerializer.SerializeToElement(value)
    };

    private static string Parameter(UnifiedGameProjectWorkspaceSnapshot snapshot, string moduleId, string parameterId) =>
        snapshot.Parameters.Single(item => item.ModuleId == moduleId && item.ParameterId == parameterId).Value.GetRawText();

    private static FeatureModuleDefinition Module(FeatureModuleCatalogDocument catalog, string id) =>
        catalog.Modules.Single(module => module.ModuleId == id);

    internal static FeatureModuleLibrarySnapshot Load(string root) =>
        new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));

    private static GamePackageDefinition Deserialize(string json) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(json, Options)!;

    private static string Format(decimal value) =>
        value.ToString("0.############################", System.Globalization.CultureInfo.InvariantCulture);

    private static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    internal static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }

    internal static string Temp(string name) =>
        Path.Combine(Path.GetTempPath(), "LLMGameCreator", name + "-" + Guid.NewGuid().ToString("N"));

    internal static void Delete(string path)
    {
        if (Directory.Exists(path)) Directory.Delete(path, true);
    }
}
