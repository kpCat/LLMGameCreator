using Xunit;

namespace LLMGameCreator.Tests.Application.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class MultiFamilyRegressionTests
{
    [Fact]
    public async Task SharedLifecycleAndPreviewExportConsumptionPassForAllFamilies()
    {
        using var temp = await MultiFamilyGeneratedTemplateTestFactory.CreateProjectWithGoal037To040SourceAsync();

        var result = MultiFamilyGeneratedTemplateTestFactory.CreateService().Build(temp.Path);

        Assert.True(result.SharedLifecycleContract.Passed);
        Assert.Equal(3, result.SharedLifecycleContract.FamilyCount);
        Assert.All(result.SharedLifecycleContract.Families, item =>
        {
            Assert.True(item.OnlyFamilyExtensionDiffers);
            Assert.False(item.ArchitectureForked);
        });

        Assert.True(result.PreviewExportConsumptionMatrix.Passed);
        Assert.Equal(3, result.PreviewExportConsumptionMatrix.FamilyCount);
        Assert.True(result.PreviewExportConsumptionMatrix.SourceGoal040PreviewExportConsumed);
        Assert.All(result.PreviewExportConsumptionMatrix.Rows, item =>
        {
            Assert.True(item.FamilyLensFound);
            Assert.True(item.TransformedIntoLifecyclePlan);
            Assert.False(item.PayloadCopiedWithoutTransformation);
        });

        Assert.True(result.RegressionMatrix.Passed);
        Assert.True(result.RegressionMatrix.SharedLifecycleContractPassed);
        Assert.True(result.RegressionMatrix.NoArchitectureForks);
        Assert.True(result.RegressionMatrix.FamilySpecificMinimumsPassed);
        Assert.True(result.RegressionMatrix.PreviewExportConsumptionPassed);
    }
}
