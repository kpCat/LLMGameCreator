using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169C;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169CRetainedInputTests
{
    [Fact]
    public void Behavioral_goal169b_audit_intake_is_exact()
    {
        using var dashboard = ReadEvidence(
            "goal169b-dashboard.json");
        var root = dashboard.RootElement;

        Assert.Equal("BLOCKED", root.GetProperty("status")
            .GetString());
        Assert.Equal(
            "standalone.payload.human_facts_parse_mismatch",
            root.GetProperty("publicationBlocker").GetString());
        Assert.True(root.GetProperty(
            "blockersClosedByGoal169BImplementation").GetBoolean());
        Assert.True(root.GetProperty(
            "postSmokeCompatibilityFixApplied").GetBoolean());
        Assert.Equal(1, root.GetProperty(
            "hiddenSmokeInvocationCount").GetInt32());
        Assert.Equal(0, root.GetProperty("retryCount").GetInt32());
    }

    [Fact]
    public void Behavioral_failed_goal169b_run_remains_unpublished()
    {
        using var proof = ReadEvidence(
            "payload-only-standalone-proof.json");
        var root = proof.RootElement;

        Assert.Equal("BLOCKED", root.GetProperty("status")
            .GetString());
        Assert.False(root.GetProperty(
            "immutablePointerPublished").GetBoolean());
        Assert.False(root.GetProperty(
            "immutableRunStatusPublished").GetBoolean());
        Assert.False(root.GetProperty(
            "standaloneLaunchStarted").GetBoolean());
        Assert.True(root.GetProperty(
            "correctiveSmokeProhibited").GetBoolean());
    }

    [Fact]
    public void Behavioral_failed_goal169b_forensics_are_readable()
    {
        using var proof = ReadEvidence(
            "payload-only-standalone-proof.json");
        var failedRun = proof.RootElement.GetProperty(
            "failedRunRoot").GetString();

        Assert.False(string.IsNullOrWhiteSpace(failedRun));
        Assert.True(Directory.Exists(failedRun));
        Assert.True(File.Exists(Path.Combine(failedRun!,
            "build-manifest.json")));
        Assert.True(File.Exists(Path.Combine(failedRun, "g_Data",
            "StreamingAssets", "LLMGameCreatorProject",
            "player-adapter-model.json")));
        Assert.False(File.Exists(Path.Combine(failedRun,
            "run-status.json")));
    }

    [Theory]
    [InlineData("Goal169")]
    [InlineData("Goal169A")]
    public void Behavioral_retained_published_output_inventory_is_readable(
        string label)
    {
        using var proof = ReadEvidence(
            "retained-runs-immutability-proof.json");
        var retained = proof.RootElement.GetProperty("retained")
            .EnumerateArray().Single(item =>
                item.GetProperty("label").GetString() == label);

        Assert.True(File.Exists(retained.GetProperty("pointerPath")
            .GetString()!));
        Assert.True(Directory.Exists(retained.GetProperty("runRoot")
            .GetString()!));
        Assert.True(Directory.Exists(retained.GetProperty(
            "payloadRoot").GetString()!));
        Assert.True(File.Exists(retained.GetProperty(
            "standaloneHistoryPath").GetString()!));
        Assert.Equal(64, retained.GetProperty("pointerSha256")
            .GetString()!.Length);
        Assert.Equal(64, retained.GetProperty("runTreeSha256")
            .GetString()!.Length);
    }

    [Fact]
    public void Behavioral_goal169b_post_fix_non_smoke_truth_is_green()
    {
        using var proof = ReadEvidence(
            "payload-only-standalone-proof.json");
        var root = proof.RootElement;

        Assert.Equal(13, root.GetProperty(
            "postFixStructuralSelfChecksPassed").GetInt32());
        Assert.Equal(13, root.GetProperty(
            "postFixStructuralSelfChecksTotal").GetInt32());
        Assert.True(root.GetProperty(
            "postFixLegacyParserCompatibilityPassed").GetBoolean());
        Assert.Equal(71, root.GetProperty(
            "postFixNonSmokePassCount").GetInt32());
    }

    private static JsonDocument ReadEvidence(string name)
    {
        var path = Path.Combine(Goal164TestKit.RepositoryRoot,
            ".llmgc", "procedural",
            "goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure",
            name);
        return JsonDocument.Parse(
            File.ReadAllText(path, Encoding.UTF8));
    }

}
