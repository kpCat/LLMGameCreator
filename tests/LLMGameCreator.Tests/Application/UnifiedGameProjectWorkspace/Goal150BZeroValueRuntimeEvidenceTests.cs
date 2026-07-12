using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;

public sealed class Goal150BZeroValueRuntimeEvidenceTests
{
    private const string EquipmentModule = "feature.equipment.weapon_loadout";

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 3)]
    public async Task Equipment_only_build_reports_observed_equipment_and_total_without_stat_evidence(
        int configuredBonus, int expectedTotal)
    {
        var root = Goal150AParameterizedRuntimeContractSynchronizationTests.FindRoot();
        var temp = Goal150AParameterizedRuntimeContractSynchronizationTests.Temp("goal150b-equipment-only");
        try
        {
            var library = Goal150AParameterizedRuntimeContractSynchronizationTests.Load(root);
            var controller = await Goal150AParameterizedRuntimeContractSynchronizationTests.CreateWorkspace(root, temp, library);
            controller.SetModuleSelected(EquipmentModule, true);
            controller.SetParameterValue(EquipmentModule, "weaponDamageBonus",
                JsonSerializer.SerializeToElement(configuredBonus));

            var result = controller.BuildAndQualify();

            Assert.True(result.Passed, result.HumanSummary + Environment.NewLine + string.Join(Environment.NewLine, result.Diagnostics));
            Assert.Equal(configuredBonus, result.WeaponDamageBonus);
            Assert.Equal(configuredBonus, result.CombatDamageDelta);
            Assert.Equal(0, result.StatDamageBonus);
            Assert.Equal(expectedTotal, result.TotalAdditionalDamage);
            Assert.True(result.CheckpointReloadPassed);
            Assert.True(result.FullReplayEquivalent);
            Assert.True(result.ActionBindingPassed);
            Assert.DoesNotContain("Сила:", result.HumanSummary, StringComparison.Ordinal);
            var package = await new JsonGamePackageRepository().LoadAsync(temp, CancellationToken.None);
            Assert.Equal(configuredBonus.ToString(System.Globalization.CultureInfo.InvariantCulture),
                package.Game.Items.Single(item => item.Id == "item/rusty_knife").Metadata["combat_damage_bonus"]);
        }
        finally { Goal150AParameterizedRuntimeContractSynchronizationTests.Delete(temp); }
    }

    [Fact]
    public void Decimal_expression_overflow_is_a_deterministic_failed_binding()
    {
        var root = Goal150AParameterizedRuntimeContractSynchronizationTests.FindRoot();
        var library = Goal150AParameterizedRuntimeContractSynchronizationTests.Load(root);
        var equipment = library.Catalog.Modules.Single(module => module.ModuleId == EquipmentModule);
        var overflowing = equipment with
        {
            EffectiveValueBindings = equipment.EffectiveValueBindings.Select((binding, index) => index == 0
                ? binding with { ValueExpression = "79228162514264337593543950335 * 2" }
                : binding).ToList()
        };
        var catalog = library.Catalog with
        {
            Modules = library.Catalog.Modules.Select(module => module.ModuleId == EquipmentModule ? overflowing : module).ToList()
        };
        var result = new FeatureModuleParameterBindingService().Bind(catalog, [EquipmentModule],
        [
            new FeatureModuleParameterValue
            {
                ModuleId = EquipmentModule,
                ParameterId = "weaponDamageBonus",
                Value = JsonSerializer.SerializeToElement(3)
            }
        ]);

        Assert.False(result.Passed);
        Assert.Equal(["numeric expression overflow rejected"], result.Diagnostics);
    }
}
