using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.SelectedRuntimeVariantPlayerAdapter;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class SelectedRuntimeVariantPlayerAdapterScriptProof
{
    [Fact]
    public async Task WritesGoal143SelectedRuntimeVariantPlayerAdapterArtifacts()
    {
        var root = ProjectRoot();
        var request = new SelectedRuntimeVariantPlayerAdapterRequest
        {
            SelectedHandoffPath = EnvOrDefault(
                "LLMGC_GOAL143_SELECTED_HANDOFF_PATH",
                SelectedRuntimeVariantPlayerAdapterVocabulary.SourceSelectedHandoffPath),
            SelectedPackagePath = EnvOrDefault(
                "LLMGC_GOAL143_SELECTED_PACKAGE_PATH",
                SelectedRuntimeVariantPlayerAdapterVocabulary.SourcePackagePath),
            SelectedOutcomePath = EnvOrDefault(
                "LLMGC_GOAL143_SELECTED_OUTCOME_PATH",
                SelectedRuntimeVariantPlayerAdapterVocabulary.SourceOutcomePath),
            SelectedRoundtripResultPath = EnvOrDefault(
                "LLMGC_GOAL143_SELECTED_ROUNDTRIP_RESULT_PATH",
                SelectedRuntimeVariantPlayerAdapterVocabulary.SourceRoundtripResultPath),
            OutputRoot = EnvOrDefault(
                "LLMGC_GOAL143_OUTPUT_ROOT",
                SelectedRuntimeVariantPlayerAdapterVocabulary.ProceduralOutputDirectory),
            UnitySmokePath = EnvOrDefault(
                "LLMGC_GOAL143_UNITY_SMOKE_PATH",
                SelectedRuntimeVariantPlayerAdapterVocabulary.UnitySmokeRelativePath)
        };

        var write = await new SelectedRuntimeVariantPlayerAdapterService(
                RuntimeBackedPlayerCommandRoundtripService.CreateDefault())
            .BuildAndWriteAsync(root, request);

        Assert.True(write.Result.CorePassed);
        Assert.False(write.Result.Accepted);
        Assert.Equal(
            SelectedRuntimeVariantPlayerAdapterVocabulary.CandidateId,
            write.Model.CandidateId);
        Assert.Equal(SelectedRuntimeVariantPlayerAdapterVocabulary.RecipeId, write.Model.RecipeId);
        Assert.Equal(
            SelectedRuntimeVariantPlayerAdapterVocabulary.VariantKind,
            write.Model.VariantKind);
        Assert.Equal(100, write.Model.Score);
        Assert.True(write.Handoff.SelectedPackageSha256MatchesHandoff);
        Assert.True(write.Handoff.SelectedFinalStateHashMatches);
        Assert.Equal(15, write.Model.FrameCount);
        Assert.Equal(6, write.Model.RequestCount);
        Assert.Equal(15, write.Model.SnapshotCount);
        Assert.Equal(4, write.Model.RuntimeRoutedRequestCount);
        Assert.Equal(2, write.Model.PresentationOnlyRequestCount);
        Assert.Equal(0, write.Model.PresentationOnlyRuntimeExecutionCount);
        Assert.True(write.Model.RequestResponseCorrelationPassed);
        Assert.True(write.Model.SequentialCursorContinuityPassed);
        Assert.True(write.Model.StateHashContinuityPassed);
        Assert.True(write.Model.SelectedVariantEffectVisible);
        Assert.True(write.Model.NoBalancedBaselineFallback);
        Assert.True(write.NegativeProof.Passed);
        Assert.All(write.Frames.Frames, frame =>
        {
            Assert.True(frame.RuntimeAuthority);
            Assert.False(frame.ProjectionOnly);
            Assert.False(frame.UnityGameplayTruth);
        });

        foreach (var name in RequiredFiles())
        {
            Assert.Contains(write.WrittenFiles, path =>
                path == request.OutputRoot + "/" + name);
            Assert.Contains(write.WrittenFiles, path =>
                path == SelectedRuntimeVariantPlayerAdapterVocabulary.ExportPackageDirectory
                        + "/"
                        + name);
        }

        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        if (string.Equals(
                Environment.GetEnvironmentVariable("LLMGC_GOAL143_REQUIRE_UNITY_SMOKE"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Assert.Equal("GREEN", write.Dashboard.Status);
            Assert.True(write.UnitySmoke.Passed);
        }
    }

    private static IReadOnlyList<string> RequiredFiles() =>
    [
        SelectedRuntimeVariantPlayerAdapterVocabulary.AcceptanceFileName,
        SelectedRuntimeVariantPlayerAdapterVocabulary.HandoffFileName,
        SelectedRuntimeVariantPlayerAdapterVocabulary.ModelFileName,
        SelectedRuntimeVariantPlayerAdapterVocabulary.FramesFileName,
        SelectedRuntimeVariantPlayerAdapterVocabulary.ResultFileName,
        SelectedRuntimeVariantPlayerAdapterVocabulary.DashboardFileName,
        SelectedRuntimeVariantPlayerAdapterVocabulary.NegativeProofFileName,
        SelectedRuntimeVariantPlayerAdapterVocabulary.FileIndexFileName,
        SelectedRuntimeVariantPlayerAdapterVocabulary.UnitySmokeFileName,
        SelectedRuntimeVariantPlayerAdapterVocabulary.OneClickReportJsonFileName,
        SelectedRuntimeVariantPlayerAdapterVocabulary.OneClickReportMarkdownFileName
    ];

    private static string EnvOrDefault(string name, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

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
