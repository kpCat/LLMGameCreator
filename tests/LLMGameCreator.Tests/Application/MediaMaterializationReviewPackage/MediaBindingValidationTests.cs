using LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaMaterializationReviewPackage;

public sealed class MediaBindingValidationTests
{
    [Fact]
    public void BindingValidationAndPayloadsResolveOnlyPhysicalFixtureMedia()
    {
        var result = MediaMaterializationReviewPackageTestFactory.BuildFromRepo();
        var inventoryPaths = result.MaterializedMediaInventory.Files
            .Select(item => item.RelativePath)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(result.ProvenanceLicenseLedger.Passed);
        Assert.True(result.BindingValidation.Passed);
        Assert.True(result.BindingValidation.EveryFamilyHasImageAndAudioFixture);
        Assert.True(result.PreviewExportMediaPayloads.Passed);
        Assert.True(result.PreviewExportMediaPayloads.AllMediaRefsResolveToInventory);
        Assert.False(result.PreviewExportMediaPayloads.GamePackageSchemaChanged);
        Assert.False(result.PreviewExportMediaPayloads.RuntimeUiUnityChanged);
        Assert.DoesNotContain(result.ProvenanceLicenseLedger.MaterializedFiles, item => item.ProviderImportedOrManual);
        Assert.Contains(result.ProvenanceLicenseLedger.LicenseDecisions, item => item.SourceKind == "unknown/no-license" && !item.PromotedInGoal054);
        Assert.Contains(result.ProvenanceLicenseLedger.LicenseDecisions, item => item.SourceKind == "imported-cc-by" && item.RequiresAttributionPayload && !item.PromotedInGoal054);

        Assert.All(result.BindingValidation.Bindings, binding =>
        {
            Assert.True(binding.SourceSlotExists);
            Assert.True(binding.MaterializedFileExistsInInventory);
            Assert.True(binding.FileHashMatchesExpected);
            Assert.True(binding.MediaKindMatchesSlot);
            Assert.True(binding.SafeRelativePath);
            Assert.False(binding.CrossFamilyLeakDetected);
            Assert.False(binding.UnapprovedProviderImportBound);
        });

        Assert.All(result.PreviewExportMediaPayloads.Payloads, payload =>
        {
            Assert.Equal("passed", payload.ValidationStatus);
            Assert.True(payload.IncludedInReviewPackage);
            Assert.NotEmpty(payload.ReferencedMediaBindingIds);
            Assert.NotEmpty(payload.PhysicalMediaFileRefs);
            Assert.All(payload.PhysicalMediaFileRefs, path => Assert.Contains(path, inventoryPaths));
        });
    }
}
