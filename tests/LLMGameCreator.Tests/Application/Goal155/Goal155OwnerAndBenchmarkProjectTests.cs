using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal155;

public sealed class Goal155OwnerAndBenchmarkProjectTests
{
    [Fact]
    public void Behavioral_owner_source_is_exact_22_selected_10_configured_and_read_only()
    {
        var source = Goal155RealProject.SourcePath();
        var before = Goal155RealProject.Manifest(source);
        using var document = JsonDocument.Parse(File.ReadAllText(Goal155RealProject.ProjectDocument(source)));
        var optional = document.RootElement.GetProperty("selectedModuleIds").GetArrayLength();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(
            Goal155HumanAcceptanceLedgerTests.Root(), "catalogs", "feature-modules"));
        Assert.Equal(12, optional);
        Assert.Equal(10, document.RootElement.GetProperty("parameterValues").GetArrayLength());
        Assert.Equal(22, library.Manifest.RequiredCoreModuleCount + optional);
        Assert.Equal(before, Goal155RealProject.Manifest(source));
    }

    [Fact]
    public void Behavioral_profile_a_exact_owner_build_repeat_reopen_is_current_and_deterministic()
    {
        var source = Goal155RealProject.SourcePath();
        var sourceBefore = Goal155RealProject.Manifest(source);
        using var project = Goal155RealProject.CopySource("profile-a");
        var initial = Goal155RealProject.Open(project.Path).Snapshot();
        Assert.Equal(22, initial.SelectedMechanicCount);
        Assert.Equal(10, Goal155RealProject.ExplicitParameterCount(project.Path));

        var first = Goal155RealProject.Open(project.Path).BuildAndQualify();
        var second = Goal155RealProject.Open(project.Path).BuildAndQualify();
        var reopened = Goal155RealProject.Open(project.Path).Snapshot();

        Assert.True(first.Passed, string.Join("; ", first.Diagnostics));
        Assert.True(second.Passed, string.Join("; ", second.Diagnostics));
        Assert.True(first.AcceptedMechanics?.Passed);
        Assert.Equal(22, first.SelectedMechanicCount);
        Assert.Equal(10, first.ConfiguredParameterCount);
        Assert.Equal(first.PackageSha256, second.PackageSha256);
        Assert.Equal(first.CompositionPackageSha256, second.CompositionPackageSha256);
        Assert.Equal(first.FinalStateHash, second.FinalStateHash);
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING", reopened.ReleaseCandidateConfigurationStatus);
        Assert.True(reopened.AcceptedMechanics?.Passed);
        Assert.Equal(2, reopened.AcceptedMechanics?.AbilityDirectDamage);
        Assert.Equal(12, reopened.AcceptedMechanics?.ManaBefore);
        Assert.Equal(9, reopened.AcceptedMechanics?.ManaRemaining);
        Assert.Equal(1, reopened.AcceptedMechanics?.StatusTickDamage);
        Assert.True(reopened.AcceptedMechanics?.StatusExpired);
        Assert.Equal(17, reopened.AcceptedMechanics?.Social?.GoldAfterClaim);
        Assert.Equal(sourceBefore, Goal155RealProject.Manifest(source));
    }

    [Fact]
    public async Task Behavioral_profile_b_22_14_proves_damage_ability_mana_status_social_and_repeat()
    {
        var source = Goal155RealProject.SourcePath();
        var sourceBefore = Goal155RealProject.Manifest(source);
        using var project = Goal155RealProject.CopySource("profile-b");
        Goal155RealProject.ConfigureProfileB(project.Path);
        var saved = Goal155RealProject.Open(project.Path).Snapshot();
        Assert.Equal(22, saved.SelectedMechanicCount);
        Assert.Equal(14, Goal155RealProject.ExplicitParameterCount(project.Path));

        var first = Goal155RealProject.Open(project.Path).BuildAndQualify();
        var second = Goal155RealProject.Open(project.Path).BuildAndQualify();
        var reopened = Goal155RealProject.Open(project.Path).Snapshot();

        Assert.True(first.Passed, string.Join("; ", first.Diagnostics));
        Assert.True(second.Passed, string.Join("; ", second.Diagnostics));
        Assert.True(first.AcceptedMechanics?.Passed);
        Assert.Equal(22, first.SelectedMechanicCount);
        Assert.Equal(14, first.ConfiguredParameterCount);
        var package = await new JsonGamePackageRepository().LoadAsync(project.Path, CancellationToken.None);
        Assert.Equal("3", package.Game.Items.Single(item => item.Id == "item/rusty_knife")
            .Metadata["combat_damage_bonus"]);
        Assert.Equal("2", package.Game.Abilities.Single(ability => ability.Id == "ability/basic_attack")
            .Metadata["source_stat_damage_per_point"]);
        Assert.True(first.WeaponDamageBonus == 3,
            "weapon=" + first.WeaponDamageBonus + "; equipment=" + first.EquipmentSlotSummary
            + "; attributes=" + first.AttributesSummary + "; summary=" + first.HumanSummary);
        Assert.Equal(6, first.StatDamageBonus);
        Assert.Equal(9, first.TotalAdditionalDamage);
        Assert.Contains("12", first.ProgressionSummary, StringComparison.Ordinal);
        Assert.Equal(2, first.AbilityDirectDamage);
        Assert.Equal(12, first.ManaBefore);
        Assert.Equal(9, first.ManaRemaining);
        Assert.Equal(1, first.StatusTickDamage);
        Assert.Contains("5", first.StatusSummary, StringComparison.Ordinal);
        Assert.True(first.StatusExpired);
        Assert.Equal(0, first.Social?.ReputationBefore);
        Assert.Equal(10, first.Social?.ReputationAfter);
        Assert.Equal(10, first.Social?.GoldAfterQuest);
        Assert.Equal(17, first.Social?.GoldAfterClaim);
        Assert.True(first.CheckpointReloadPassed && first.FullReplayEquivalent && first.ActionBindingPassed);
        Assert.Equal(first.PackageSha256, second.PackageSha256);
        Assert.Equal(first.CompositionPackageSha256, second.CompositionPackageSha256);
        Assert.Equal(first.FinalStateHash, second.FinalStateHash);
        Assert.True(reopened.AcceptedMechanics?.Passed);
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING", reopened.ReleaseCandidateConfigurationStatus);
        var invalidController = Goal155RealProject.Open(project.Path);
        Goal155RealProject.Set(invalidController, "feature.dialogue.reputation_gated_reward",
            "trustedReputationThreshold", 101);
        var invalid = invalidController.BuildAndQualify();
        var afterFailure = Goal155RealProject.Open(project.Path).Snapshot();
        Assert.False(invalid.Passed);
        Assert.Equal(JsonSerializer.Serialize(reopened.AcceptedMechanics),
            JsonSerializer.Serialize(afterFailure.AcceptedMechanics));
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING", afterFailure.ReleaseCandidateConfigurationStatus);
        Assert.Equal(sourceBefore, Goal155RealProject.Manifest(source));
    }

    [Fact]
    public void Behavioral_core_only_composition_remains_buildable_and_cannot_claim_rc_ready()
    {
        using var project = Goal155RealProject.CopySource("core-only");
        Goal155RealProject.ConfigureCoreOnly(project.Path);
        var controller = Goal155RealProject.Open(project.Path);
        var build = controller.BuildAndQualify();
        var snapshot = controller.Snapshot();
        Assert.True(build.Passed, string.Join("; ", build.Diagnostics));
        Assert.False(build.AcceptedMechanics?.Passed);
        Assert.NotEmpty(build.AcceptedMechanics?.MissingFactKinds ?? []);
        Assert.NotEqual("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.False(File.Exists(snapshot.ReleaseCandidateRecordPath));
    }
}

internal static class Goal155RealProject
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string SourcePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LLMGameCreator", "Games", "goal148-manual");

    public static Goal155DisposableProject CopySource(string name)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "G155", Guid.NewGuid().ToString("N")[..10]);
        var target = Path.Combine(root, name);
        Copy(SourcePath(), target);
        return new Goal155DisposableProject(root, target);
    }

    public static Goal155DisposableProject CopyProject(string source, string name)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "G155", Guid.NewGuid().ToString("N")[..10]);
        var target = Path.Combine(root, name);
        Copy(source, target);
        return new Goal155DisposableProject(root, target);
    }

    public static UnifiedGameProjectWorkspaceController Open(
        string project,
        IProjectStandaloneBuildService? standalone = null)
    {
        var root = Goal155HumanAcceptanceLedgerTests.Root();
        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        current.LoadAsync(project, CancellationToken.None).GetAwaiter().GetResult();
        var controller = new UnifiedGameProjectWorkspaceController(
            current,
            new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(
                root,
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                repository,
                new GamePackageValidator(),
                current),
            standaloneBuild: standalone ?? new ProjectStandaloneBuildService(root));
        controller.OpenProject(project);
        return controller;
    }

    public static void ConfigureProfileB(string project)
    {
        ReplaceAuthoring(project,
        [
            Parameter("feature.equipment.weapon_loadout", "weaponDamageBonus", 3),
            Parameter("feature.character.attributes", "startingStrength", 8),
            Parameter("feature.character.attributes", "damagePerStrengthPoint", 2),
            Parameter("feature.character.level_progression", "level2RequiredExperience", 12),
            Parameter("feature.combat.active_ability_loadout", "abilityBaseDamage", 2),
            Parameter("feature.magic.mana_spellcasting", "startingMana", 12),
            Parameter("feature.magic.mana_spellcasting", "abilityManaCost", 3),
            Parameter("feature.status.turn_effects", "statusDurationTurns", 5),
            Parameter("feature.status.turn_effects", "statusTickDamage", 1),
            Parameter("feature.faction.reputation_standing", "startingReputation", 0),
            Parameter("feature.quest.faction_reputation_consequences", "questReputationReward", 10),
            Parameter("feature.quest.faction_reputation_consequences", "questFailurePenalty", 5),
            Parameter("feature.dialogue.reputation_gated_reward", "trustedReputationThreshold", 10),
            Parameter("feature.dialogue.reputation_gated_reward", "trustedGoldReward", 7)
        ]);
    }

    public static void ConfigureCoreOnly(string project)
    {
        var path = ProjectDocument(project);
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        root["selectedModuleIds"] = new JsonArray();
        root["parameterValues"] = new JsonArray();
        File.WriteAllText(path, root.ToJsonString(JsonOptions) + Environment.NewLine, new UTF8Encoding(false));
    }

    public static void Set(
        UnifiedGameProjectWorkspaceController controller,
        string moduleId,
        string parameterId,
        decimal value) => controller.SetParameterValue(moduleId, parameterId, JsonSerializer.SerializeToElement(value));

    public static int ExplicitParameterCount(string project)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(ProjectDocument(project)));
        return document.RootElement.GetProperty("parameterValues").GetArrayLength();
    }

    public static string ProjectDocument(string project) => Directory.EnumerateFiles(
        Path.Combine(project, ".llmgc", "authoring"), "project-*.featurecomposition.json").Single();

    public static IReadOnlyList<string> Manifest(string root) => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
        .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/') + "|" + new FileInfo(path).Length + "|" + Hash(path))
        .OrderBy(value => value, StringComparer.Ordinal).ToList();

    public static string HashSet(string root) => string.Join("\n", Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
        .OrderBy(path => path, StringComparer.Ordinal)
        .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/') + "|" + Hash(path)));

    public static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static JsonObject Parameter(string moduleId, string parameterId, decimal value) => new()
    {
        ["moduleId"] = moduleId,
        ["parameterId"] = parameterId,
        ["value"] = JsonValue.Create(value)
    };

    private static void ReplaceAuthoring(string project, IReadOnlyList<JsonObject> parameters)
    {
        var path = ProjectDocument(project);
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var values = new JsonArray();
        foreach (var parameter in parameters) values.Add(parameter);
        root["parameterValues"] = values;
        File.WriteAllText(path, root.ToJsonString(JsonOptions) + Environment.NewLine, new UTF8Encoding(false));
    }

    private static void Copy(string source, string target)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, true);
        }
    }
}

internal sealed record Goal155DisposableProject(string Root, string Path) : IDisposable
{
    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, true);
    }
}
