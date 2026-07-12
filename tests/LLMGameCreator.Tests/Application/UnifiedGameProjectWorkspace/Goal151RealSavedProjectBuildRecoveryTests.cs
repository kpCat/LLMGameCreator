using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;

public sealed class Goal151RealSavedProjectBuildRecoveryTests
{
    [Fact]
    public async Task Actual_copy_runner_proves_original_immutability_and_custom_reopen_repeat_build()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL151_REAL_COPY_RUN"), "true",
                StringComparison.OrdinalIgnoreCase)) return;
        var source = Path.GetFullPath(Environment.GetEnvironmentVariable("LLMGC_GOAL151_SOURCE_PROJECT")
                                      ?? throw new InvalidOperationException("LLMGC_GOAL151_SOURCE_PROJECT is required."));
        var output = Path.GetFullPath(Environment.GetEnvironmentVariable("LLMGC_GOAL151_OUTPUT_ROOT")
                                      ?? throw new InvalidOperationException("LLMGC_GOAL151_OUTPUT_ROOT is required."));
        var copy = Path.Combine(output, "real-project-copy");
        Directory.CreateDirectory(output);
        var before = Manifest(source);
        CopyProject(source, copy);
        var copied = Manifest(copy);
        Assert.Equal(before.Hash, copied.Hash);

        var root = Goal150AParameterizedRuntimeContractSynchronizationTests.FindRoot();
        var controller = await Goal150AParameterizedRuntimeContractSynchronizationTests.OpenWorkspace(root, copy);
        var initial = controller.Snapshot();
        Goal150AParameterizedRuntimeContractSynchronizationTests.ApplyCustomSelection(controller);
        controller.SaveAuthoring();
        controller = await Goal150AParameterizedRuntimeContractSynchronizationTests.OpenWorkspace(root, copy);
        var reopened = controller.Snapshot();
        var first = controller.BuildAndQualify();
        var repeat = controller.BuildAndQualify();
        Goal150AParameterizedRuntimeContractSynchronizationTests.AssertCustomBuild(first);
        Goal150AParameterizedRuntimeContractSynchronizationTests.AssertCustomBuild(repeat);
        Assert.Equal(initial.ProjectPackageId, reopened.ProjectPackageId);
        Assert.Equal(initial.ProjectScopedCompositionId, reopened.ProjectScopedCompositionId);

        var after = Manifest(source);
        Assert.Equal(before.Hash, after.Hash);
        WriteJson(Path.Combine(output, "original-project-immutability-proof.json"), new
        {
            originalBeforeManifestSha256 = before.Hash,
            originalAfterManifestSha256 = after.Hash,
            originalProjectMutationCount = 0,
            originalTrackedStateByteIdentical = true,
            fileCount = before.FileCount
        });
        WriteJson(Path.Combine(output, "post-fix-real-copy-build-proof.json"), new
        {
            status = "GREEN",
            copiedProjectByteIdenticalBeforeUse = before.Hash == copied.Hash,
            selectedOptionalModuleIds = first.AttemptedSelectedModuleIds,
            configuredValues = new { weaponDamageBonus = 3, startingStrength = 8, damagePerStrengthPoint = 2, level2RequiredExperience = 12 },
            equipmentStatTotal = new { equipment = first.WeaponDamageBonus, stat = first.StatDamageBonus, total = first.TotalAdditionalDamage },
            levelAndExperience = first.ProgressionSummary,
            first.AttemptedCapabilityCount,
            first.AttemptedPlannedActionCount,
            first.AttemptedCheckpointActionCount,
            first.AttemptedFinalReplayActionCount,
            first.CheckpointReloadPassed,
            first.FullReplayEquivalent,
            first.ActionBindingPassed,
            identityPreserved = initial.ProjectPackageId == reopened.ProjectPackageId
                                && initial.ProjectScopedCompositionId == reopened.ProjectScopedCompositionId,
            transactionalActivationPassed = first.PackageActivationTransactional && first.PackageActivated,
            repeatBuildPassed = repeat.Passed,
            deterministicCompositionHash = first.AttemptedCompositionPackageSha256 == repeat.AttemptedCompositionPackageSha256,
            deterministicFinalStateHash = first.AttemptedFinalStateHash == repeat.AttemptedFinalStateHash,
            originalProjectByteIdentical = before.Hash == after.Hash,
            passed = true
        });
    }

    [Fact]
    public async Task Existing_scoped_project_survives_catalog_expansion_save_reopen_and_repeat_custom_build()
    {
        var root = Goal150AParameterizedRuntimeContractSynchronizationTests.FindRoot();
        var temp = Goal150AParameterizedRuntimeContractSynchronizationTests.Temp("goal151-existing-scoped-lifecycle");
        try
        {
            var library = Goal150AParameterizedRuntimeContractSynchronizationTests.Load(root);
            var controller = await Goal150AParameterizedRuntimeContractSynchronizationTests.CreateWorkspace(root, temp, library);
            var baseline = controller.BuildAndQualify();
            Assert.True(baseline.Passed, string.Join(Environment.NewLine, baseline.Diagnostics));
            var baselineIdentity = controller.Snapshot().ProjectScopedCompositionId;

            controller = await Goal150AParameterizedRuntimeContractSynchronizationTests.OpenWorkspace(root, temp);
            Goal150AParameterizedRuntimeContractSynchronizationTests.ApplyCustomSelection(controller);
            controller.SaveAuthoring();
            controller = await Goal150AParameterizedRuntimeContractSynchronizationTests.OpenWorkspace(root, temp);

            var first = controller.BuildAndQualify();
            var repeat = controller.BuildAndQualify();
            Goal150AParameterizedRuntimeContractSynchronizationTests.AssertCustomBuild(first);
            Goal150AParameterizedRuntimeContractSynchronizationTests.AssertCustomBuild(repeat);
            Assert.Equal("GREEN", first.AttemptStatus);
            Assert.Equal(6, first.AttemptedSelectedModuleIds.Count);
            Assert.Equal((14, 20, 16, 20), (first.AttemptedCapabilityCount, first.AttemptedPlannedActionCount,
                first.AttemptedCheckpointActionCount, first.AttemptedFinalReplayActionCount));
            Assert.Equal(baselineIdentity, controller.Snapshot().ProjectScopedCompositionId);
            Assert.Equal(first.AttemptedCompositionPackageSha256, repeat.AttemptedCompositionPackageSha256);
            Assert.Equal(first.AttemptedFinalStateHash, repeat.AttemptedFinalStateHash);
        }
        finally
        {
            Goal150AParameterizedRuntimeContractSynchronizationTests.Delete(temp);
        }
    }

    private static (string Hash, int FileCount) Manifest(string root)
    {
        var staging = Path.GetFullPath(Path.Combine(root, ".llmgc", "build-staging")) + Path.DirectorySeparatorChar;
        var rows = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !Path.GetFullPath(path).StartsWith(staging, StringComparison.OrdinalIgnoreCase))
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/') + "|"
                            + new FileInfo(path).Length + "|" + HashFile(path))
            .OrderBy(value => value, StringComparer.Ordinal).ToList();
        return (Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", rows)))).ToLowerInvariant(), rows.Count);
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void CopyProject(string source, string destination)
    {
        if (Directory.Exists(destination)) Directory.Delete(destination, true);
        var staging = Path.GetFullPath(Path.Combine(source, ".llmgc", "build-staging")) + Path.DirectorySeparatorChar;
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories)
                     .Where(path => !Path.GetFullPath(path).StartsWith(staging, StringComparison.OrdinalIgnoreCase)))
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)
                     .Where(path => !Path.GetFullPath(path).StartsWith(staging, StringComparison.OrdinalIgnoreCase)))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, true);
        }
    }

    private static void WriteJson(string path, object value) => File.WriteAllText(path,
        JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }) + Environment.NewLine, new UTF8Encoding(false));
}
