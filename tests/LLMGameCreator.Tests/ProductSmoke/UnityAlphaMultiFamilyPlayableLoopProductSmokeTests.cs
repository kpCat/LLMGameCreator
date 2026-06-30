using System.Text.Json;
using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;
using LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class UnityAlphaMultiFamilyPlayableLoopProductSmokeTests
{
    [Fact]
    public async Task UnityAlphaMultiFamilyPlayableLoopProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var outputRoot = ResolveOutputFolder(repoRoot);
        var service = new UnityAlphaMultiFamilyPlayableLoopEvidenceService();
        var write = await service.BuildAndWriteAsync(
            outputRoot,
            new UnityAlphaMultiFamilyOptions
            {
                RepositoryRootPath = repoRoot,
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.SourceManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyModeManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.UnityStagingManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyCommandPlanJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyLoopProofFileName("map_panel_rpg"));
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyLoopProofFileName("survival_sandbox"));
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyLoopProofFileName("first_person_grid_dungeon"));
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.PlayerLogSummaryJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.MediaBindingValidationJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.PreviewExportPayloadJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.ReviewPackageManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.InvalidMatrixJsonFileName);
        Assert.True(File.Exists(write.ReportMarkdownPath));
        AssertFile(write.StagingDirectoryPath, UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath);
        AssertFile(write.StagingDirectoryPath, UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyCommandPlanStagingRelativePath);

        using var sourceManifest = Parse(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.SourceManifestJsonFileName);
        using var familyModeManifest = Parse(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyModeManifestJsonFileName);
        using var stagingManifest = Parse(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.UnityStagingManifestJsonFileName);
        using var commandPlan = Parse(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.FamilyCommandPlanJsonFileName);
        using var playerSummary = Parse(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.PlayerLogSummaryJsonFileName);
        using var invalid = Parse(write.OutputDirectoryPath, UnityAlphaMultiFamilyPlayableLoopEvidenceService.InvalidMatrixJsonFileName);
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.False(sourceManifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(sourceManifest.RootElement.GetProperty("goal056AcceptedByUserHandoff").GetBoolean());
        Assert.True(sourceManifest.RootElement.GetProperty("goal056UnityProofPassed").GetBoolean());
        Assert.True(familyModeManifest.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(3, familyModeManifest.RootElement.GetProperty("familyCount").GetInt32());
        Assert.True(stagingManifest.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(commandPlan.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("accepted=false", report);
        Assert.Contains("manualGate=unity_alpha_multifamily_playable_loop_verification", report);
        Assert.Contains("goal056AcceptedByUserHandoff=true", report);
        Assert.Contains("mediaBindingValidationPassed=true", report);
        Assert.Contains("invalidMatrixPassed=true", report);

        var status = ExtractReportValue(report, "implementationStatus");
        Assert.Contains(status, new[] { "GREEN", "BLOCKED" });
        if (status == "GREEN")
        {
            Assert.True(write.Result.Report.AllFamilyLoopsVerified);
            Assert.True(playerSummary.RootElement.GetProperty("passed").GetBoolean());
            Assert.True(playerSummary.RootElement.GetProperty("playerExecuted").GetBoolean());
            Assert.Equal(0, playerSummary.RootElement.GetProperty("unityExitCode").GetInt32());
            Assert.Equal(0, playerSummary.RootElement.GetProperty("playerExitCode").GetInt32());
            Assert.Contains("family_loop_completed=map_panel_rpg", report);
            Assert.Contains("family_loop_completed=survival_sandbox", report);
            Assert.Contains("family_loop_completed=first_person_grid_dungeon", report);
            Assert.Contains("media_bound_hash_validation=true", report);
            Assert.Contains("review_package_proof=goal057", report);
            foreach (var proof in write.Result.FamilyLoopProofsByFamilyId.Values)
            {
                Assert.True(proof.Passed);
                Assert.True(proof.LoopStepCount >= 3);
            }
        }
        else
        {
            Assert.False(write.Result.Report.AllFamilyLoopsVerified);
            Assert.False(playerSummary.RootElement.GetProperty("passed").GetBoolean());
            Assert.NotEqual(string.Empty, write.Result.Report.Diagnostics.First(item => item.Code.StartsWith("goal057.unity.", StringComparison.Ordinal)).Code);
        }
    }

    private static void AssertFile(string directoryPath, string relativePath) =>
        Assert.True(File.Exists(Path.Combine(directoryPath, relativePath.Replace('/', Path.DirectorySeparatorChar))), "Missing evidence file: " + relativePath);

    private static JsonDocument Parse(string directoryPath, string fileName) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(directoryPath, fileName)));

    private static string ExtractReportValue(string report, string key)
    {
        foreach (var line in report.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            var prefix = key + "=";
            if (line.StartsWith(prefix, StringComparison.Ordinal))
            {
                return line[prefix.Length..].Trim();
            }
        }

        return string.Empty;
    }

    private static string ResolveOutputFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var outputFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(outputFolder);
        return outputFolder;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
