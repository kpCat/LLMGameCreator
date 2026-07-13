using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;

public sealed class Goal153AbilityManaStatusWorkspaceTests
{
    [Fact]
    public async Task Goal153_real_project_copy_saves_reopens_builds_and_changes_only_with_typed_parameter()
    {
        var root = FindRoot();
        var source = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator", "Games", "goal148-manual");
        Assert.True(Directory.Exists(source), "Accepted goal148-manual source project is required.");
        var sourceBefore = TreeHash(source);
        var disposableRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal153-real-project-" + Guid.NewGuid().ToString("N"));
        var project = Path.Combine(disposableRoot, "goal148-manual-copy");
        CopyDirectory(source, project);
        try
        {
            var first = await Controller(root, project);
            var initial = first.OpenProject(project);
            Assert.Contains(initial.Mechanics, item => item.ModuleId == "feature.combat.active_ability_loadout" && !item.Selected);
            Assert.Contains(initial.Mechanics, item => item.ModuleId == "feature.magic.mana_spellcasting" && !item.Selected);
            Assert.Contains(initial.Mechanics, item => item.ModuleId == "feature.status.turn_effects" && !item.Selected);
            first.SetModuleSelected("feature.combat.active_ability_loadout", true);
            first.SetModuleSelected("feature.magic.mana_spellcasting", true);
            first.SetModuleSelected("feature.status.turn_effects", true);
            Set(first, "feature.combat.active_ability_loadout", "abilityBaseDamage", 2);
            Set(first, "feature.magic.mana_spellcasting", "startingMana", 12);
            Set(first, "feature.magic.mana_spellcasting", "abilityManaCost", 3);
            Set(first, "feature.status.turn_effects", "statusDurationTurns", 2);
            Set(first, "feature.status.turn_effects", "statusTickDamage", 1);
            first.SaveAuthoring();

            var reopened = await Controller(root, project);
            var restored = reopened.OpenProject(project);
            Assert.All(new[] { "feature.combat.active_ability_loadout", "feature.magic.mana_spellcasting", "feature.status.turn_effects" },
                id => Assert.Contains(restored.Mechanics, item => item.ModuleId == id && item.Selected));
            Assert.Equal(5, restored.Parameters.Count(item => item.ModuleId.StartsWith("feature.combat.active_ability", StringComparison.Ordinal)
                || item.ModuleId.StartsWith("feature.magic.mana", StringComparison.Ordinal)
                || item.ModuleId.StartsWith("feature.status.turn", StringComparison.Ordinal)));

            var firstBuild = reopened.BuildAndQualify();
            Assert.True(firstBuild.Passed, string.Join("; ", firstBuild.Diagnostics));
            Assert.Equal("Магический импульс", firstBuild.AbilitySummary);
            Assert.Equal(2, firstBuild.AbilityDirectDamage);
            Assert.Equal(12, firstBuild.ManaBefore);
            Assert.Equal(3, firstBuild.ManaSpent);
            Assert.Equal(9, firstBuild.ManaRemaining);
            Assert.Equal(1, firstBuild.StatusTickDamage);
            Assert.True(firstBuild.StatusExpired);
            Assert.True(firstBuild.CheckpointReloadPassed);
            Assert.True(firstBuild.FullReplayEquivalent);
            Assert.Contains("Способность: Магический импульс", firstBuild.HumanSummary);
            var firstPackageHash = firstBuild.CompositionPackageSha256;
            var firstFinalHash = firstBuild.FinalStateHash;

            Set(reopened, "feature.combat.active_ability_loadout", "abilityBaseDamage", 3);
            reopened.SaveAuthoring();
            var secondBuild = reopened.BuildAndQualify();
            Assert.True(secondBuild.Passed, string.Join("; ", secondBuild.Diagnostics));
            Assert.Equal(3, secondBuild.AbilityDirectDamage);
            Assert.NotEqual(firstPackageHash, secondBuild.CompositionPackageSha256);
            Assert.NotEqual(firstFinalHash, secondBuild.FinalStateHash);
            Assert.All(new[] { "feature.combat.active_ability_loadout", "feature.magic.mana_spellcasting", "feature.status.turn_effects" },
                id => Assert.Contains(reopened.Snapshot().Mechanics, item => item.ModuleId == id && item.Selected));
            var evidencePath = Environment.GetEnvironmentVariable("LLMGC_GOAL153_EVIDENCE_PATH");
            if (!string.IsNullOrWhiteSpace(evidencePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(evidencePath)!);
                File.WriteAllText(evidencePath, JsonSerializer.Serialize(new
                {
                    schemaVersion = "goal153_real_project_lifecycle_run_v1",
                    status = "GREEN",
                    configuredValues = new { abilityBaseDamage = 2, startingMana = 12, abilityManaCost = 3, statusDurationTurns = 2, statusTickDamage = 1 },
                    firstBuild = new { firstBuild.CompositionPackageSha256, firstBuild.FinalStateHash, firstBuild.AbilityDirectDamage, firstBuild.ManaBefore, firstBuild.ManaSpent, firstBuild.ManaRemaining, firstBuild.StatusTickDamage, firstBuild.StatusExpired, firstBuild.CheckpointReloadPassed, firstBuild.FullReplayEquivalent, firstBuild.PlannedActionCount },
                    secondBuild = new { secondBuild.CompositionPackageSha256, secondBuild.FinalStateHash, secondBuild.AbilityDirectDamage },
                    packageHashChanged = firstPackageHash != secondBuild.CompositionPackageSha256,
                    finalHashChanged = firstFinalHash != secondBuild.FinalStateHash,
                    sourceProjectByteIdentical = sourceBefore == TreeHash(source)
                }, new JsonSerializerOptions { WriteIndented = true }));
            }
            if (string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL153_STANDALONE"), "true", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
                var standalone = reopened.BuildWindowsStandalone();
                Assert.True(standalone.Status == "GREEN", JsonSerializer.Serialize(standalone));
                Assert.True(standalone.HostReused);
                Assert.False(standalone.HostRebuilt);
                Assert.True(standalone.LaunchSmokePassed);
                Assert.Equal(standalone.SelfCheckTotalCount, standalone.SelfCheckPassedCount);
                Assert.True(standalone.FrameCount >= secondBuild.RuntimeFrames.Count);
                Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
                var goal153aRoot = Environment.GetEnvironmentVariable("LLMGC_GOAL153A_EVIDENCE_ROOT");
                if (!string.IsNullOrWhiteSpace(goal153aRoot))
                {
                    Directory.CreateDirectory(goal153aRoot);
                    File.WriteAllText(Path.Combine(goal153aRoot, "cached-standalone-proof.json"),
                        JsonSerializer.Serialize(new
                        {
                            schemaVersion = "goal153a_cached_standalone_proof_v1",
                            status = "GREEN",
                            standalone.HostCacheKey,
                            standalone.HostReused,
                            standalone.HostRebuilt,
                            standalone.LaunchSmokePassed,
                            standalone.SelfCheckTotalCount,
                            standalone.SelfCheckPassedCount,
                            standalone.FrameCount,
                            unityProcessStartCount = 0,
                            humanFacts = new
                            {
                                abilityDamage = secondBuild.AbilityDirectDamage,
                                manaStart = secondBuild.ManaBefore,
                                manaCost = secondBuild.ManaSpent,
                                manaRemaining = secondBuild.ManaRemaining,
                                tickDamage = secondBuild.StatusTickDamage,
                                configuredDuration = 2,
                                statusExpired = secondBuild.StatusExpired
                            }
                        }, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
                }
            }
            Assert.Equal(sourceBefore, TreeHash(source));
        }
        finally
        {
            if (Directory.Exists(disposableRoot)) Directory.Delete(disposableRoot, true);
        }
    }

    private static void Set(UnifiedGameProjectWorkspaceController controller, string module, string parameter, int value) =>
        controller.SetParameterValue(module, parameter, JsonSerializer.SerializeToElement(value));

    private static async Task<UnifiedGameProjectWorkspaceController> Controller(string root, string project)
    {
        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(project, CancellationToken.None);
        return new UnifiedGameProjectWorkspaceController(current, new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(root, SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                repository, new GamePackageValidator(), current), standaloneBuild: new ProjectStandaloneBuildService(root));
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);
            var destination = Path.Combine(target, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, true);
        }
    }

    private static string TreeHash(string path)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).OrderBy(file => file, StringComparer.Ordinal))
        {
            hash.AppendData(System.Text.Encoding.UTF8.GetBytes(Path.GetRelativePath(path, file).Replace('\\', '/') + "\n"));
            hash.AppendData(File.ReadAllBytes(file));
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static string FindRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln"))) return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
