using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154C1;

public sealed class Goal154C1RealProjectLifecycleTests
{
    [Fact]
    public async Task Behavioral_disposable_goal148_project_persists_social_last_success_and_reuses_cached_standalone()
    {
        var root = FindRoot();
        var source = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator", "Games", "goal148-manual");
        Assert.True(Directory.Exists(source), "goal148-manual source project is required.");
        var sourceManifest = Manifest(source);
        var proofRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator", "Goal154C1");
        var defaultProject = Path.Combine(proofRoot, "default");
        var customProject = Path.Combine(proofRoot, "custom");
        var lockedProject = Path.Combine(proofRoot, "locked");
        foreach (var project in new[] { defaultProject, customProject, lockedProject })
        {
            if (Directory.Exists(project)) Directory.Delete(project, true);
            Copy(source, project);
            ResetCopiedAuthoring(project);
        }

        try
        {
            var first = await Open(root, defaultProject);
            Configure(first, threshold: 10, reward: 7);
            first.SaveAuthoring();
            var reopened = await Open(root, defaultProject);
            AssertValues(reopened.Snapshot(), 0, 10, 5, 10, 7);
            var defaultBuild = reopened.BuildAndQualify();
            Assert.True(defaultBuild.Passed, string.Join(";", defaultBuild.Diagnostics));
            Assert.Equal(17, defaultBuild.Social?.GoldAfterClaim);
            Assert.Equal(10, defaultBuild.Social?.ReputationAfter);
            var repeat = reopened.BuildAndQualify();
            Assert.True(repeat.Passed, string.Join(";", repeat.Diagnostics));
            Assert.Equal(defaultBuild.PackageSha256, repeat.PackageSha256);
            Assert.Equal(defaultBuild.CompositionPackageSha256, repeat.CompositionPackageSha256);
            Assert.Equal(defaultBuild.FinalStateHash, repeat.FinalStateHash);

            var restored = await Open(root, defaultProject);
            var restoredSnapshot = restored.Snapshot();
            Assert.True(restoredSnapshot.Social is { Present: true, Passed: true });
            Assert.Equal(defaultBuild.Social!.HumanFacts, restoredSnapshot.Social!.HumanFacts);
            restored.SetParameterValue(DialogueModule, "trustedReputationThreshold", JsonSerializer.SerializeToElement(101));
            var invalid = restored.BuildAndQualify();
            Assert.False(invalid.Passed);
            Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Contains("trustedReputationThreshold", StringComparison.Ordinal));
            var afterInvalidReopen = await Open(root, defaultProject);
            Assert.Equal(defaultBuild.Social.HumanFacts, afterInvalidReopen.Snapshot().Social?.HumanFacts);

            var custom = await Open(root, customProject);
            Configure(custom, threshold: 10, reward: 9);
            custom.SaveAuthoring();
            var customBuild = custom.BuildAndQualify();
            Assert.True(customBuild.Passed, string.Join(";", customBuild.Diagnostics));
            Assert.Equal(19, customBuild.Social?.GoldAfterClaim);
            Assert.Equal(9, customBuild.Social?.TrustedRewardDelta);
            Assert.NotEqual(defaultBuild.PackageSha256, customBuild.PackageSha256);
            Assert.NotEqual(defaultBuild.FinalStateHash, customBuild.FinalStateHash);

            var locked = await Open(root, lockedProject);
            Configure(locked, threshold: 20, reward: 7);
            locked.SaveAuthoring();
            var lockedBuild = locked.BuildAndQualify();
            Assert.True(lockedBuild.Passed, string.Join(";", lockedBuild.Diagnostics));
            Assert.Equal(10, lockedBuild.Social?.GoldAfterClaim);
            Assert.False(lockedBuild.Social?.RewardClaimed ?? true);
            Assert.DoesNotContain(lockedBuild.Social!.HumanFacts, fact => fact.Label == "Повторная награда");

            if (string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL154C1_RUN_STANDALONE"), "true", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
                var standalone = afterInvalidReopen.BuildWindowsStandalone();
                Assert.Equal("GREEN", standalone.Status);
                Assert.True(standalone.HostReused);
                Assert.False(standalone.HostRebuilt);
                Assert.True(standalone.LaunchSmokePassed);
                Assert.Equal(standalone.SelfCheckTotalCount, standalone.SelfCheckPassedCount);
                Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
                Assert.Equal(defaultBuild.PackageSha256, standalone.PackageSha256);
            }
            Assert.Equal(sourceManifest, Manifest(source));
        }
        finally
        {
            Assert.Equal(sourceManifest, Manifest(source));
        }
    }

    private const string FactionModule = "feature.faction.reputation_standing";
    private const string QuestModule = "feature.quest.faction_reputation_consequences";
    private const string DialogueModule = "feature.dialogue.reputation_gated_reward";

    private static void Configure(UnifiedGameProjectWorkspaceController controller, decimal threshold, decimal reward)
    {
        foreach (var inheritedProfile in new[]
                 {
                     "feature.profile.alchemy_focus", "feature.profile.combat_focus", "feature.profile.exploration_resource_focus"
                 })
        {
            if (controller.Snapshot().Mechanics.Any(item => item.ModuleId == inheritedProfile && item.Selected))
                controller.SetModuleSelected(inheritedProfile, false);
        }
        Select(controller, FactionModule);
        Select(controller, QuestModule);
        Select(controller, DialogueModule);
        Set(controller, FactionModule, "startingReputation", 0);
        Set(controller, QuestModule, "questReputationReward", 10);
        Set(controller, QuestModule, "questFailurePenalty", 5);
        Set(controller, DialogueModule, "trustedReputationThreshold", threshold);
        Set(controller, DialogueModule, "trustedGoldReward", reward);
    }

    private static void Set(UnifiedGameProjectWorkspaceController controller, string moduleId, string parameterId, decimal value) =>
        controller.SetParameterValue(moduleId, parameterId, JsonSerializer.SerializeToElement(value));

    private static void Select(UnifiedGameProjectWorkspaceController controller, string moduleId)
    {
        if (!controller.Snapshot().Mechanics.Any(item => item.ModuleId == moduleId && item.Selected))
            controller.SetModuleSelected(moduleId, true);
    }

    private static void AssertValues(UnifiedGameProjectWorkspaceSnapshot snapshot, params decimal[] expected)
    {
        var values = new[]
        {
            Value(snapshot, FactionModule, "startingReputation"), Value(snapshot, QuestModule, "questReputationReward"),
            Value(snapshot, QuestModule, "questFailurePenalty"), Value(snapshot, DialogueModule, "trustedReputationThreshold"),
            Value(snapshot, DialogueModule, "trustedGoldReward")
        };
        Assert.Equal(expected, values);
    }

    private static decimal Value(UnifiedGameProjectWorkspaceSnapshot snapshot, string module, string parameter) =>
        snapshot.Parameters.Single(item => item.ModuleId == module && item.ParameterId == parameter).Value.GetDecimal();

    private static async Task<UnifiedGameProjectWorkspaceController> Open(string root, string project)
    {
        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(project, CancellationToken.None);
        var controller = new UnifiedGameProjectWorkspaceController(current,
            new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(root, SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), repository,
                new GamePackageValidator(), current),
            standaloneBuild: new ProjectStandaloneBuildService(root));
        controller.OpenProject(project);
        return controller;
    }

    private static IReadOnlyList<string> Manifest(string root) => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
        .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/') + "|" + new FileInfo(path).Length + "|" + Hash(path))
        .OrderBy(value => value, StringComparer.Ordinal).ToList();
    private static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)); }
    private static void Copy(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!); File.Copy(file, target, true);
        }
    }
    private static void ResetCopiedAuthoring(string project)
    {
        var authoring = Path.Combine(project, ".llmgc", "authoring");
        if (!Directory.Exists(authoring)) return;
        foreach (var document in Directory.EnumerateFiles(authoring, "*.featurecomposition.json", SearchOption.TopDirectoryOnly))
            File.Delete(document);
    }
    private static string FindRoot()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current is not null; current = current.Parent)
            if (File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln"))) return current.FullName;
        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
