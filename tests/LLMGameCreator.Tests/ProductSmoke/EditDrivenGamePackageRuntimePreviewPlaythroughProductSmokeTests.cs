using System.Text.Json;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class EditDrivenGamePackageRuntimePreviewPlaythroughProductSmokeTests
{
    [Fact]
    public async Task Goal081ReadsGoal080PackageBuildsReplayProofAndRejectsInvalidPlaythroughs()
    {
        var service = new EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.PackageReadProof.ProjectedPackagePayloadRead);
        Assert.True(result.PackageReadProof.GamePackageValidationPassed);
        Assert.True(result.Transcript.Passed);
        Assert.True(result.CoverageLedger.Passed);
        Assert.True(result.NegativeProof.Passed);

        var packagePath = Path.Combine(
            ProjectRoot(),
            ".llmgc",
            "procedural",
            "goal-080-edit-driven-gamepackage-runtime-preview-bridge",
            "projected-gamepackage",
            "package.json");
        var package = ReadPackage(packagePath);
        var validation = new GamePackageValidator().Validate(package);
        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Issues.Select(issue => issue.ToString())));

        var start = new DefaultGameRuntime().Start(package);
        Assert.True(start.Success);
        Assert.Equal(result.PackageReadProof.StartMapId, start.State.CurrentMapId);

        var preview = new GeneratedPackageRuntimePreviewService().Build(package, start.State);
        Assert.NotNull(preview.CurrentScene);
        Assert.Empty(preview.Warnings);
        Assert.Equal(18, preview.Items.Count);
        Assert.Equal(9, preview.Quests.Count);

        Assert.Equal("package-read-proof.json", EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService.PackageReadProofFileName);
        Assert.Equal("playthrough-command-script.json", EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService.CommandScriptFileName);
        Assert.Equal("playthrough-transcript.json", EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService.TranscriptFileName);
        Assert.Equal("playthrough-negative-proof.json", EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService.NegativeProofFileName);

        var readProof = ReadArtifact<EditDrivenGamePackageRuntimePreviewPlaythroughPackageReadProof>(
            write.OutputDirectoryPath,
            EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService.PackageReadProofFileName);
        var script = ReadArtifact<EditDrivenGamePackageRuntimePreviewPlaythroughCommandScript>(
            write.OutputDirectoryPath,
            EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService.CommandScriptFileName);
        var transcript = ReadArtifact<EditDrivenGamePackageRuntimePreviewPlaythroughTranscript>(
            write.OutputDirectoryPath,
            EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService.TranscriptFileName);
        var negative = ReadArtifact<EditDrivenGamePackageRuntimePreviewPlaythroughNegativeProof>(
            write.OutputDirectoryPath,
            EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService.NegativeProofFileName);

        Assert.True(readProof.Passed);
        Assert.True(script.Passed);
        Assert.True(transcript.Passed);
        Assert.True(negative.Passed);
        Assert.Equal(script.CommandCount, transcript.CommandCount);
        Assert.NotEqual(transcript.InitialStateHash, transcript.FinalStateHash);
        Assert.Equal(
            57,
            script.Commands.SelectMany(command => command.CoveredGoal078ActionIds)
                .Distinct(StringComparer.Ordinal)
                .Count());
    }

    private static GamePackageDefinition ReadPackage(string packagePath)
    {
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(File.ReadAllText(packagePath), JsonOptions());
        Assert.NotNull(package);
        return package!;
    }

    private static T ReadArtifact<T>(string outputRoot, string fileName)
    {
        var value = JsonSerializer.Deserialize<T>(File.ReadAllText(Path.Combine(outputRoot, fileName)), JsonOptions());
        Assert.NotNull(value);
        return value!;
    }

    private static JsonSerializerOptions JsonOptions() =>
        new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
