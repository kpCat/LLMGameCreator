using System.Security.Cryptography;
using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaAssetCampaignOrchestration;

public sealed class MediaAssetFixtureBindingTests
{
    [Fact]
    public async Task FixtureFilesAreWrittenHashedAndBoundWithoutFinalMediaClaims()
    {
        using var temp = new TempMediaAssetCampaignProject();
        var service = MediaAssetCampaignTestFactory.CreateService();
        var result = service.Build(MediaAssetCampaignTestFactory.FindRepoRoot());
        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(result.FixtureInventory.Passed);
        Assert.True(result.BindingManifest.Passed);
        Assert.True(result.PreviewExportPayloads.Passed);
        Assert.True(result.FixtureInventory.FixtureFileCount > 0);
        Assert.Equal(result.FixtureInventory.FixtureFileCount, result.BindingManifest.BindingCount);
        Assert.All(result.FixtureInventory.Files, file =>
        {
            var path = Path.Combine(write.OutputDirectoryPath, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), "Missing fixture file: " + file.RelativePath);
            var bytes = File.ReadAllBytes(path);
            Assert.Equal(file.ByteLength, bytes.LongLength);
            Assert.Equal(file.Sha256, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
            Assert.DoesNotContain("finalMedia=true", File.ReadAllText(path), StringComparison.OrdinalIgnoreCase);
        });

        foreach (var familyId in MediaAssetCampaignVocabulary.FamilyIds)
        {
            Assert.Contains(result.BindingManifest.Bindings, item => item.FamilyId == familyId && item.MediaKind == "image");
            Assert.Contains(result.BindingManifest.Bindings, item => item.FamilyId == familyId && item.MediaKind == "audio");
            Assert.Contains(result.PreviewExportPayloads.Families, item => item.FamilyId == familyId && item.ExplicitFallbackForUnfilledSlots);
        }
    }
}
