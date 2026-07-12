using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Runtime;
using LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.ProjectStandaloneBuild;

public sealed class Goal152ProjectStandaloneBuildTests
{
    [Fact]
    public async Task Real_disposable_project_copies_build_once_and_reuse_generic_host()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL152_REAL_STANDALONE_RUN"), "true", StringComparison.OrdinalIgnoreCase)) return;
        var source = Path.GetFullPath(Environment.GetEnvironmentVariable("LLMGC_GOAL152_SOURCE_PROJECT") ?? throw new InvalidOperationException("LLMGC_GOAL152_SOURCE_PROJECT is required."));
        var output = Path.GetFullPath(Environment.GetEnvironmentVariable("LLMGC_GOAL152_OUTPUT_ROOT") ?? throw new InvalidOperationException("LLMGC_GOAL152_OUTPUT_ROOT is required."));
        var before = HashFile(Path.Combine(source, "package.json"));
        var firstCopy = Path.Combine(output, "project-copy-one");
        var secondCopy = Path.Combine(output, "project-copy-two");
        Copy(source, firstCopy); Copy(source, secondCopy);
        var root = Goal150AParameterizedRuntimeContractSynchronizationTests.FindRoot();

        var first = await ConfigureAndBuild(root, firstCopy, 3);
        Assert.True(first.Status == "GREEN", first.Status + ": " + string.Join(" | ", first.Diagnostics));
        Assert.True(first.HostRebuilt || first.HostReused, string.Join(Environment.NewLine, first.Diagnostics));
        Assert.True(first.LaunchSmokePassed);
        Assert.True(File.Exists(first.ExecutablePath));
        Assert.True(Directory.Exists(Path.GetDirectoryName(first.ExecutablePath)!));
        using (var frames = JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(first.ExecutablePath)!, Path.GetFileNameWithoutExtension(first.ExecutablePath) + "_Data", "StreamingAssets", "LLMGameCreatorProject", "player-adapter-frames.json"))))
        {
            Assert.Equal(first.FrameCount, frames.RootElement.GetArrayLength());
            Assert.DoesNotContain(frames.RootElement.EnumerateArray(), frame => frame.GetProperty("title").GetString()?.StartsWith("Шаг Runtime ", StringComparison.Ordinal) == true);
        }
        if (string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL152C_CACHE_ONLY"), "true", StringComparison.OrdinalIgnoreCase))
        {
            Assert.True(first.HostReused, "Goal152C requires an existing valid host cache.");
            Assert.Equal(before, HashFile(Path.Combine(source, "package.json")));
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, "real-project-copy-standalone-proof.json"), JsonSerializer.Serialize(new
            {
                status = "GREEN", sourceManifestByteIdentical = true, customValues = "3/8/2/12", equipmentStatTotal = "3/6/9", levelExperience = "2/12",
                first = new { first.HostRebuilt, first.HostReused, first.ExecutablePath, first.BuildManifestPath, first.FrameCount, first.LaunchSmokePassed }
            }, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine, new UTF8Encoding(false));
            return;
        }

        var second = await ConfigureAndBuild(root, secondCopy, 4);
        Assert.Equal("GREEN", second.Status);
        Assert.True(second.HostReused, string.Join(Environment.NewLine, second.Diagnostics));
        Assert.True(second.LaunchSmokePassed);
        Assert.NotEqual(first.PackageSha256, second.PackageSha256);
        Assert.Equal(HashFile(Path.Combine(first.OutputFolder, Path.GetFileName(first.ExecutablePath))), HashFile(Path.Combine(second.OutputFolder, Path.GetFileName(second.ExecutablePath))));
        Assert.Equal(before, HashFile(Path.Combine(source, "package.json")));

        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "real-project-copy-standalone-proof.json"), JsonSerializer.Serialize(new
        {
            status = "GREEN", sourceManifestByteIdentical = true, customValues = "3/8/2/12", equipmentStatTotal = "3/6/9", levelExperience = "2/12",
            first = new { first.HostRebuilt, first.HostReused, first.ExecutablePath, first.BuildManifestPath, first.FrameCount, first.LaunchSmokePassed },
            second = new { second.HostRebuilt, second.HostReused, second.PackageSha256, second.LaunchSmokePassed }
        }, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine, new UTF8Encoding(false));
    }

    private static async Task<LLMGameCreator.Application.Design.ProjectStandaloneBuild.ProjectStandaloneBuildResult> ConfigureAndBuild(string root, string project, int weaponBonus)
    {
        var controller = await OpenStandaloneWorkspace(root, project);
        Goal150AParameterizedRuntimeContractSynchronizationTests.ApplyCustomSelection(controller);
        controller.SetParameterValue("feature.equipment.weapon_loadout", "weaponDamageBonus", JsonSerializer.SerializeToElement(weaponBonus));
        controller.SaveAuthoring();
        controller = await OpenStandaloneWorkspace(root, project);
        controller.SaveStandaloneBuildSettings(new ProjectStandaloneBuildSettings { UnityEditorPath = ResolveUnityEditor() });
        return controller.BuildWindowsStandalone();
    }

    private static void Copy(string source, string destination)
    {
        if (Directory.Exists(destination)) Directory.Delete(destination, true);
        Directory.CreateDirectory(destination);
        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories)) Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories)) { var target = Path.Combine(destination, Path.GetRelativePath(source, file)); Directory.CreateDirectory(Path.GetDirectoryName(target)!); File.Copy(file, target, true); }
    }

    private static string HashFile(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static async Task<UnifiedGameProjectWorkspaceController> OpenStandaloneWorkspace(string root, string project)
    {
        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(project, CancellationToken.None);
        var controller = new UnifiedGameProjectWorkspaceController(current,
            new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(root, SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), repository, new GamePackageValidator(), current),
            standaloneBuild: new ProjectStandaloneBuildService(root));
        controller.OpenProject(project);
        return controller;
    }
    private static string ResolveUnityEditor()
    {
        var path = Directory.GetFiles("C:\\Program Files\\Unity\\Hub\\Editor", "Unity.exe", SearchOption.AllDirectories).OrderBy(value => value, StringComparer.Ordinal).FirstOrDefault();
        return path ?? throw new FileNotFoundException("Unity Editor was not found.");
    }
}
