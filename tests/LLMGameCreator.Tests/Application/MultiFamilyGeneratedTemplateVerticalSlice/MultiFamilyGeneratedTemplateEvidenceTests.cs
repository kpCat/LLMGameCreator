using System.Text.Json;
using LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;
using Xunit;

namespace LLMGameCreator.Tests.Application.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class MultiFamilyGeneratedTemplateEvidenceTests
{
    [Fact]
    public async Task EvidenceWriterCreatesRequiredFilesAndDeterministicReport()
    {
        using var temp = await MultiFamilyGeneratedTemplateTestFactory.CreateProjectWithGoal037To040SourceAsync();

        var service = MultiFamilyGeneratedTemplateTestFactory.CreateService();
        var first = service.Build(temp.Path);
        var second = service.Build(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);
        var names = write.WrittenFiles.Select(path => Path.GetFileName(path) ?? string.Empty).OrderBy(item => item, StringComparer.Ordinal).ToArray();

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(
            [
                "family-loop-plan-first-person-grid-dungeon.json",
                "family-loop-plan-map-panel-rpg.json",
                "family-loop-plan-survival-sandbox.json",
                "family-simulatable-loop-proof-first-person-grid-dungeon.json",
                "family-simulatable-loop-proof-map-panel-rpg.json",
                "family-simulatable-loop-proof-survival-sandbox.json",
                "family-template-catalog.json",
                "invalid-family-diagnostics-matrix.json",
                "multi-family-generated-template-vertical-slice-report.md",
                "multi-family-regression-matrix.json",
                "preview-export-consumption-matrix.json",
                "shared-lifecycle-contract.json"
            ],
            names);

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.CatalogJsonFileName)));
        using var shared = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.SharedLifecycleContractJsonFileName)));
        using var mapProof = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.MapPanelProofJsonFileName)));
        using var survivalProof = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.SurvivalProofJsonFileName)));
        using var dungeonProof = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.GridDungeonProofJsonFileName)));
        using var regression = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.RegressionMatrixJsonFileName)));
        using var preview = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.PreviewExportConsumptionMatrixJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, MultiFamilyGeneratedTemplateEvidenceService.InvalidMatrixJsonFileName)));
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.Equal(3, catalog.RootElement.GetProperty("familyCount").GetInt32());
        Assert.True(catalog.RootElement.GetProperty("sourceGoal040PreviewExportConsumed").GetBoolean());
        Assert.True(shared.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(mapProof.RootElement.GetProperty("stateChanged").GetBoolean());
        Assert.True(survivalProof.RootElement.GetProperty("stateChanged").GetBoolean());
        Assert.True(dungeonProof.RootElement.GetProperty("stateChanged").GetBoolean());
        Assert.True(regression.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(preview.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("implementationStatus=GREEN", report);
        Assert.Contains("accepted=false", report);
        Assert.Contains("manualGate=multi_family_generated_template_vertical_slice_verification", report);
        Assert.Contains("familyCount=3", report);
        Assert.Contains("simulatableLoopProofCount=3", report);
        Assert.Contains("sourceGoal040PreviewExportConsumed=true", report);
        Assert.Contains("sharedLifecycleContractPassed=true", report);
        Assert.Contains("invalidMatrixPassed=true", report);
        Assert.Contains("multi_family_generated_template_vertical_slice_verification required", report);
    }
}
