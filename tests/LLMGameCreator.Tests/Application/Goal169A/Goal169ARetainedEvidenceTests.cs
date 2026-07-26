using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169A;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169ARetainedEvidenceTests
{
    private const string Task =
        "goal-169-profile-neutral-relationships-and-reactive-regional-events";

    [Fact]
    public void Behavioral_retained_goal169_evidence_tree_is_byte_identical()
    {
        var procedural = Root(".llmgc", "procedural", Task);
        var export = Root(".llmgc", "exports", Task);
        var files = Directory.EnumerateFiles(procedural)
            .OrderBy(Path.GetFileName, StringComparer.Ordinal).ToList();

        Assert.Equal(15, files.Count);
        Assert.All(files, file =>
        {
            var copy = Path.Combine(export,
                Path.GetFileName(file));
            Assert.True(File.Exists(copy));
            Assert.Equal(File.ReadAllBytes(file),
                File.ReadAllBytes(copy));
        });
    }

    [Fact]
    public void Behavioral_retained_goal169_tree_matches_published_hash()
    {
        Assert.Equal(
            "2a128bfbc33f5c0c2b7fe5724b7bfb93a4cff1b8011a66ecfbb4ef720611e556",
            TreeHash(Root(".llmgc", "procedural", Task)));
    }

    [Fact]
    public void Behavioral_retained_smoke_keeps_original_direction_counts()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(Root(".llmgc", "procedural", Task),
                "standalone-rc-portability-proof.json")));
        var root = document.RootElement;

        Assert.Equal("BLOCKED",
            root.GetProperty("status").GetString());
        Assert.Equal(0,
            root.GetProperty("explicitMoveFrameCount").GetInt32());
        Assert.Equal(84,
            root.GetProperty("directionOnlyFrameCount").GetInt32());
        Assert.Equal(1,
            root.GetProperty("hiddenSmokeInvocationCount").GetInt32());
        Assert.Equal(0,
            root.GetProperty("correctiveRetryCount").GetInt32());
    }

    [Fact]
    public void Behavioral_retained_smoke_keeps_green_standalone_and_blocked_proof()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(Root(".llmgc", "procedural", Task),
                "standalone-rc-portability-proof.json")));
        var root = document.RootElement;

        Assert.True(root.GetProperty(
            "standaloneLaunchSmokePassed").GetBoolean());
        Assert.True(root.GetProperty("hostReused").GetBoolean());
        Assert.False(root.GetProperty("hostRebuilt").GetBoolean());
        Assert.Equal(0, root.GetProperty(
            "unityEditorProcessStartCount").GetInt32());
        Assert.Equal(
            "goal169.payload_move_command_not_explicit",
            root.GetProperty("failureCode").GetString());
    }

    private static string Root(params string[] parts) =>
        parts.Aggregate(Goal164TestKit.RepositoryRoot,
            Path.Combine);

    private static string TreeHash(string path)
    {
        var builder = new StringBuilder();
        foreach (var file in Directory.EnumerateFiles(path)
                     .OrderBy(Path.GetFileName,
                         StringComparer.Ordinal))
        {
            builder.Append(Path.GetFileName(file)).Append('|')
                .Append(Convert.ToHexString(SHA256.HashData(
                    File.ReadAllBytes(file))).ToLowerInvariant())
                .Append('\n');
        }
        return Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(builder.ToString())))
            .ToLowerInvariant();
    }
}
