using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;

public sealed class Goal155AcceptedMechanicsWorkspaceTests
{
    [Fact]
    public void Behavioral_green_history_persists_and_restores_typed_accepted_summary()
    {
        using var fixture = LLMGameCreator.Tests.Application.Goal155.Goal155RcFixture.Create("history-accepted");
        var document = fixture.Document with
        {
            LastActivatedProjectPackageSha256 = fixture.Build.PackageSha256,
            LastCompositionPackageSha256 = fixture.Build.CompositionPackageSha256,
            LastQualifiedFinalStateHash = fixture.Build.FinalStateHash,
            LastQualificationStatus = "GREEN"
        };
        WriteHistory(fixture.Project, Entry(fixture) with { CompletedAtUtc = DateTimeOffset.UtcNow });
        var read = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(
            fixture.Project, document, fixture.Library);
        Assert.NotNull(read.LastSuccessfulBuild);
        Assert.True(read.LastSuccessfulBuild?.AcceptedMechanics?.Passed);
        Assert.Equal(22, read.LastSuccessfulBuild?.AcceptedMechanics?.SelectedMechanicCount);
        Assert.Equal("CURRENT", read.SocialConfigurationStatus);
    }

    [Fact]
    public void Behavioral_failed_attempt_and_old_history_without_summary_do_not_replace_or_break_last_success()
    {
        using var fixture = LLMGameCreator.Tests.Application.Goal155.Goal155RcFixture.Create("history-compat");
        var document = fixture.Document with
        {
            LastActivatedProjectPackageSha256 = fixture.Build.PackageSha256,
            LastCompositionPackageSha256 = fixture.Build.CompositionPackageSha256,
            LastQualifiedFinalStateHash = fixture.Build.FinalStateHash,
            LastQualificationStatus = "GREEN"
        };
        WriteHistory(fixture.Project, Entry(fixture) with
        {
            CompletedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-2),
            AcceptedMechanics = null
        }, "old-green.json");
        var oldRead = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(
            fixture.Project, document, fixture.Library);
        Assert.NotNull(oldRead.LastSuccessfulBuild);
        Assert.Null(oldRead.LastSuccessfulBuild?.AcceptedMechanics);

        WriteHistory(fixture.Project, Entry(fixture) with { CompletedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1) }, "accepted-green.json");
        WriteHistory(fixture.Project, new GameProjectBuildHistoryEntry
        {
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Status = "FAILED",
            AttemptStatus = "FAILED",
            FailureStage = "authoring.validation",
            Diagnostics = ["invalid"]
        }, "failed.json");
        var read = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(
            fixture.Project, document, fixture.Library);
        Assert.True(read.LastSuccessfulBuild?.AcceptedMechanics?.Passed);
        Assert.Equal(fixture.Build.AcceptedMechanics?.QualifiedAuthoringFingerprint,
            read.LastSuccessfulBuild?.AcceptedMechanics?.QualifiedAuthoringFingerprint);
        Assert.Equal(fixture.Build.AcceptedMechanics?.HumanFacts,
            read.LastSuccessfulBuild?.AcceptedMechanics?.HumanFacts);
        Assert.Equal(fixture.Build.AcceptedMechanics?.MissingFactKinds,
            read.LastSuccessfulBuild?.AcceptedMechanics?.MissingFactKinds);
    }

    private static GameProjectBuildHistoryEntry Entry(
        LLMGameCreator.Tests.Application.Goal155.Goal155RcFixture fixture) => new()
    {
        Status = "GREEN",
        AttemptStatus = "GREEN",
        PackageSha256 = fixture.Build.PackageSha256,
        ActivatedProjectPackageSha256 = fixture.Build.PackageSha256,
        CompositionPackageSha256 = fixture.Build.CompositionPackageSha256,
        FinalStateHash = fixture.Build.FinalStateHash,
        SelectedMechanicCount = fixture.Build.SelectedMechanicCount,
        ConfiguredParameterCount = fixture.Build.ConfiguredParameterCount,
        CheckpointReloadPassed = true,
        FullReplayEquivalent = true,
        ActionBindingPassed = true,
        QualifiedAuthoringFingerprint = fixture.Build.QualifiedAuthoringFingerprint,
        Social = fixture.Build.Social,
        AcceptedMechanics = fixture.Build.AcceptedMechanics
    };

    private static void WriteHistory(
        string project,
        GameProjectBuildHistoryEntry entry,
        string name = "entry.json")
    {
        var root = Path.Combine(project, ".llmgc", "build-history");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, name), JsonSerializer.Serialize(entry));
    }
}
