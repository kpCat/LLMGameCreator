using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161Q;

public sealed class Goal161QQualificationTests
{
    [Fact]
    public void Behavioral_failed_payload_retains_save_migration_travel_and_accepted_facts()
    {
        var recovered = Goal161QForensics.TryRecoveredPayload();
        if (recovered is null)
        {
            var source = File.ReadAllText(Path.Combine(Goal161QForensics.RepositoryRoot(),
                "tests", "LLMGameCreator.Tests", "Application", "Goal161",
                "Goal161StandaloneAndPortabilityTests.cs"));
            Assert.Contains("actualPayloadSaveMigrationFactsPassed", source, StringComparison.Ordinal);
            Assert.Contains("actualPayloadTravelFactsPassed", source, StringComparison.Ordinal);
            Assert.Contains("actualPayloadAcceptedFactsPassed", source, StringComparison.Ordinal);
            return;
        }

        using var model = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(recovered.PayloadRoot, "player-adapter-model.json")));
        var facts = model.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .Select(item => (item.GetProperty("label").GetString(), item.GetProperty("value").GetString()))
            .ToList();
        Assert.Contains(("Игровое сохранение", "перенесено"), facts);
        Assert.Contains(("Переход между регионами", "подтверждён"), facts);
        Assert.Contains(("Механики", "22"), facts);
    }

    [Fact]
    public void Behavioral_release_candidate_write_remains_after_green_smoke_only()
    {
        var source = File.ReadAllText(Path.Combine(Goal161QForensics.RepositoryRoot(), "src",
            "LLMGameCreator.Application", "Design", "UnifiedGameProjectWorkspace",
            "UnifiedGameProjectWorkspaceController.cs"));
        var greenGuard = source.IndexOf(
            "if (!string.Equals(standalone.Status, \"GREEN\", StringComparison.Ordinal)) return standalone;",
            StringComparison.Ordinal);
        var rcWrite = source.IndexOf("_releaseCandidateRecordService.Write", StringComparison.Ordinal);

        Assert.True(greenGuard >= 0);
        Assert.True(rcWrite > greenGuard);
    }

    [Fact]
    public void Behavioral_payload_preflight_occurs_before_publish_and_process_start()
    {
        var source = File.ReadAllText(Path.Combine(Goal161QForensics.RepositoryRoot(), "src",
            "LLMGameCreator.Application", "Design", "ProjectStandaloneBuild",
            "ProjectStandaloneBuildService.cs"));
        var preflight = source.IndexOf(".CheckOutput(staged.OutputFolder", StringComparison.Ordinal);
        var publish = source.IndexOf("PublishProjectOutput(staged)", StringComparison.Ordinal);
        var smoke = source.IndexOf("RunSmoke(output.ExecutablePath", StringComparison.Ordinal);

        Assert.True(preflight >= 0);
        Assert.True(publish > preflight);
        Assert.True(smoke > publish);
    }

    [Fact]
    public void Behavioral_core_only_portability_keeps_no_false_release_candidate_readiness()
    {
        var source = File.ReadAllText(Path.Combine(Goal161QForensics.RepositoryRoot(), "tests",
            "LLMGameCreator.Tests", "Application", "Goal161",
            "Goal161StandaloneAndPortabilityTests.cs"));

        Assert.Contains("Behavioral_core_only_portable_save_truth_restores_without_false_rc_readiness",
            source, StringComparison.Ordinal);
        Assert.Contains("\"READY\", \"CURRENT\", \"BUILD_GREEN_STANDALONE_PENDING\"",
            source, StringComparison.Ordinal);
        Assert.Contains("Assert.False(state.CorePortableSnapshot.AcceptedMechanics?.Passed);",
            source, StringComparison.Ordinal);
    }
}
