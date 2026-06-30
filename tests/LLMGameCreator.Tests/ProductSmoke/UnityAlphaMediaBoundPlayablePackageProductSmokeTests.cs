using System.Text.Json;
using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class UnityAlphaMediaBoundPlayablePackageProductSmokeTests
{
    [Fact]
    public async Task UnityAlphaMediaBoundPlayablePackageProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var outputRoot = ResolveOutputFolder(repoRoot);
        var service = new UnityAlphaMediaBoundPlayablePackageEvidenceService();
        var write = await service.BuildAndWriteAsync(
            outputRoot,
            new UnityAlphaMediaBoundOptions
            {
                RepositoryRootPath = repoRoot,
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        AssertFile(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.SourceManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.StagingManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.FamilyPanelModelsJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.UnityLoadContractJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.UnityLoadProofJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.SmokeLogSummaryJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.PreviewExportPayloadsJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.HashInventoryJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.InvalidMatrixJsonFileName);
        AssertFile(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.ArtifactScopeReportJsonFileName);
        Assert.True(File.Exists(write.ReportMarkdownPath));
        AssertFile(write.StagingDirectoryPath, UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath);

        using var sourceManifest = Parse(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.SourceManifestJsonFileName);
        using var stagingManifest = Parse(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.StagingManifestJsonFileName);
        using var contract = Parse(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.UnityLoadContractJsonFileName);
        using var proof = Parse(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.UnityLoadProofJsonFileName);
        using var invalid = Parse(write.OutputDirectoryPath, UnityAlphaMediaBoundPlayablePackageEvidenceService.InvalidMatrixJsonFileName);
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.False(sourceManifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(sourceManifest.RootElement.GetProperty("goal055AcceptedByUserHandoff").GetBoolean());
        Assert.True(stagingManifest.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(15, stagingManifest.RootElement.GetProperty("physicalMediaFileCount").GetInt32());
        Assert.True(contract.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("accepted=false", report);
        Assert.Contains("manualGate=unity_alpha_media_bound_playable_package_verification", report);
        Assert.Contains("goal055AcceptedByUserHandoff=true", report);
        Assert.Contains("streamingAssetsPayloadStaged=true", report);
        Assert.Contains("physicalMediaFileCount=15", report);

        var status = ExtractReportValue(report, "implementationStatus");
        Assert.Contains(status, new[] { "GREEN", "BLOCKED" });
        var proofPassed = proof.RootElement.GetProperty("passed").GetBoolean();
        if (status == "GREEN")
        {
            Assert.True(proofPassed);
            Assert.True(proof.RootElement.GetProperty("unityEditorOrPlayerExecuted").GetBoolean());
            Assert.Contains("pngLoadProofPassed=true", report);
            Assert.Contains("wavLoadProofPassed=true", report);
            Assert.Contains("bundleProofPassed=true", report);
            Assert.Contains("unityMediaLoadContractPassed=true", report);
            Assert.Contains("familyMediaPanelProofPassed=true", report);
            Assert.Contains("media_bound_manifest_loaded=true", report);
            Assert.Contains("media_bound_family_panel_proof=map_panel_rpg", report);
            Assert.Contains("media_bound_family_panel_proof=survival_sandbox", report);
            Assert.Contains("media_bound_family_panel_proof=first_person_grid_dungeon", report);
        }
        else
        {
            Assert.False(proofPassed);
            Assert.NotEqual(string.Empty, proof.RootElement.GetProperty("blockerCode").GetString());
            Assert.Contains("unityMediaLoadContractPassed=false", report);
        }

        foreach (var binding in write.Result.StagingManifest.Bindings)
        {
            Assert.True(File.Exists(Path.Combine(write.StagingDirectoryPath, binding.RelativePath.Replace('/', Path.DirectorySeparatorChar))), "Missing staged media file: " + binding.RelativePath);
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
