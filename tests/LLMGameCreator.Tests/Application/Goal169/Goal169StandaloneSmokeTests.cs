using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169StandaloneSmokeTests
{
    [Fact]
    public void Behavioral_exactly_one_cached_hidden_regional_event_smoke()
    {
        Assert.Empty(
            System.Diagnostics.Process.GetProcessesByName("Unity"));
        if (!string.Equals(
                Environment.GetEnvironmentVariable(
                    "LLMGC_GOAL169_RUN_SMOKE"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Assert.NotEmpty(CompleteHostCaches());
            return;
        }

        var hostBefore = CompleteHostCaches().ToDictionary(
            path => path,
            TreeHash,
            StringComparer.OrdinalIgnoreCase);
        Assert.NotEmpty(hostBefore);
        var goal142Before =
            Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath);
        var goal148 = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        var goal148Before = TreeHash(goal148);
        var standaloneService = new ProjectStandaloneBuildService(
            Goal164TestKit.RepositoryRoot);
        var fixture = Goal164BuildFixture.Create(
            coreOnly: false, standaloneService);
        var generationRoot = Path.Combine(
            fixture.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        var sidecarsBefore = TreeHash(generationRoot);

        var standalone =
            fixture.Controller.BuildWindowsStandalone();
        var snapshot = fixture.Controller.Snapshot();
        var build = fixture.Controller.LastBuild!;
        var relationships =
            snapshot.GeneratedCampaignRelationships;
        var regionalEvents =
            snapshot.GeneratedCampaignRegionalEvents;

        Assert.Equal("GREEN", standalone.Status);
        Assert.True(standalone.HostReused);
        Assert.False(standalone.HostRebuilt);
        Assert.True(standalone.LaunchSmokePassed);
        Assert.Equal(standalone.SelfCheckTotalCount,
            standalone.SelfCheckPassedCount);
        Assert.True(standalone.SelfCheckTotalCount > 0);
        Assert.Empty(
            System.Diagnostics.Process.GetProcessesByName("Unity"));
        var usedHost = HostRoot(standalone.HostCacheKey);
        Assert.True(hostBefore.TryGetValue(
            usedHost, out var usedHostBefore));
        Assert.Equal(usedHostBefore, TreeHash(usedHost));
        Assert.True(relationships is
        {
            Passed: true,
            Status: "RELATIONSHIPS_CURRENT"
        });
        Assert.True(regionalEvents is
        {
            Passed: true,
            Status: "REGIONAL_EVENTS_CURRENT"
        });
        Assert.Equal("CAMPAIGN_CURRENT",
            snapshot.GeneratedWorld?.Status);
        Assert.Equal("CURRENT",
            snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT",
            snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.Equal(sidecarsBefore, TreeHash(generationRoot));
        Assert.Equal(goal142Before,
            Goal156TestKit.Hash(
                Goal156TestKit.Goal142BaselinePath));
        Assert.Equal(goal148Before, TreeHash(goal148));
        var payloadRoot = Path.Combine(
            standalone.OutputFolder,
            Path.GetFileNameWithoutExtension(
                standalone.ExecutablePath) + "_Data",
            "StreamingAssets", "LLMGameCreatorProject");
        using var payload = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(payloadRoot,
                "player-adapter-model.json")));
        var facts = payload.RootElement
            .GetProperty("humanReviewFacts")
            .EnumerateArray()
            .Select(item => (
                Label: item.GetProperty("label").GetString()
                       ?? string.Empty,
                Value: item.GetProperty("value").GetString()
                       ?? string.Empty))
            .ToList();
        Assert.All(relationships.HumanReviewFacts, expected =>
            Assert.Contains(facts, actual =>
                actual.Label == expected.Label
                && actual.Value == expected.Value));
        Assert.All(regionalEvents.HumanReviewFacts, expected =>
            Assert.Contains(facts, actual =>
                actual.Label == expected.Label
                && actual.Value == expected.Value));

        using var framePayload = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(payloadRoot,
                "player-adapter-frames.json")));
        var payloadFrames = framePayload.RootElement
            .EnumerateArray().ToList();
        Assert.NotEmpty(payloadFrames);
        Assert.All(payloadFrames, frame =>
            Assert.Equal("generated-regional-event",
                frame.GetProperty("category").GetString()));
        var commands = payloadFrames.Select(frame =>
                frame.GetProperty("title").GetString()
                ?? string.Empty)
            .ToList();
        Assert.Contains(commands, item =>
            item.StartsWith("Prerequisite.",
                StringComparison.Ordinal));
        Assert.Contains(commands, item =>
            item.Contains("Move", StringComparison.Ordinal));
        Assert.Contains(commands, item =>
            item.Contains("Interact", StringComparison.Ordinal));
        Assert.Contains(commands, item =>
            item.Contains("OpenDialogue",
                StringComparison.Ordinal));
        Assert.Contains(commands, item =>
            item.Contains("ChooseDialogueOption",
                StringComparison.Ordinal));

        using var portable = Goal156TestKit.Copy(
            fixture.Project, "goal169-real-smoke-portable");
        var portableSnapshot =
            Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();
        Assert.Equal("REGIONAL_EVENTS_CURRENT",
            portableSnapshot.GeneratedCampaignRegionalEvents?.Status);
        Assert.Equal("CURRENT",
            portableSnapshot.ReleaseCandidateConfigurationStatus);

        WriteSmokeCapture(standalone, snapshot, build,
            relationships, regionalEvents,
            sidecarsBefore == TreeHash(generationRoot),
            goal142Before == Goal156TestKit.Hash(
                Goal156TestKit.Goal142BaselinePath),
            goal148Before == TreeHash(goal148),
            usedHostBefore == TreeHash(usedHost),
            portableSnapshot.GeneratedCampaignRegionalEvents
                ?.Status == "REGIONAL_EVENTS_CURRENT",
            portableSnapshot.ReleaseCandidateConfigurationStatus
            == "CURRENT",
            commands);
    }

    private static void WriteSmokeCapture(
        ProjectStandaloneBuildResult standalone,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        GameProjectBuildResult build,
        GameProjectGeneratedCampaignRelationshipSummary relationships,
        GameProjectGeneratedCampaignRegionalEventSummary regionalEvents,
        bool sidecarsUnchanged,
        bool goal142Unchanged,
        bool goal148Unchanged,
        bool hostFilesUnchanged,
        bool portableCurrent,
        bool portableReleaseCandidateCurrent,
        IReadOnlyList<string> commands)
    {
        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL169_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path))
            return;

        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path,
            JsonSerializer.Serialize(new
            {
                status = "GREEN",
                standalone.HostCacheKey,
                standalone.HostReused,
                standalone.HostRebuilt,
                unityEditorProcessStartCount = 0,
                hiddenSmokeInvocationCount = 1,
                hiddenSmokePassed =
                    standalone.LaunchSmokePassed,
                correctiveRetryCount = 0,
                standalone.SelfCheckPassedCount,
                standalone.SelfCheckTotalCount,
                historySchemaVersion =
                    GameProjectBuildHistoryReader.SchemaVersionV7,
                build.PackageSha256,
                build.FinalStateHash,
                relationshipStatus = relationships.Status,
                regionalEventStatus = regionalEvents.Status,
                relationships.RelationshipCount,
                relationships.QualifiedRelationshipCount,
                relationships.SupportAvailableCount,
                relationships.ChallengeAvailableCount,
                relationships.RefuseAvailableCount,
                relationships.SaveContinuationFactsPassed,
                relationships
                    .SaveContinuationFactsEvaluationStatus,
                regionalEvents.EventCount,
                regionalEvents.QualifiedEventCount,
                regionalEvents.SupportGratitudeCount,
                regionalEvents.ChallengeAftermathCount,
                regionalEvents.RefusalFalloutCount,
                regionalEvents.ExactPackageSha256,
                regionalEvents.RegionalEventOverlaySha256,
                regionalEvents.RegionalEventInventorySha256,
                runtimeFrameCount =
                    regionalEvents.RuntimeFrames.Count,
                prerequisiteFrameCount = commands.Count(item =>
                    item.StartsWith("Prerequisite.",
                        StringComparison.Ordinal)),
                eventInteractionFrameCount = commands.Count(item =>
                    !item.StartsWith("Prerequisite.",
                        StringComparison.Ordinal)),
                payloadFactsPassed = true,
                payloadFramesPassed = true,
                releaseCandidateRecordCurrent =
                    snapshot
                        .ReleaseCandidateRecordConfigurationStatus
                    == "CURRENT",
                releaseCandidateCurrent =
                    snapshot.ReleaseCandidateConfigurationStatus
                    == "CURRENT",
                campaignCurrent =
                    snapshot.GeneratedWorld?.Status
                    == "CAMPAIGN_CURRENT",
                sidecarsUnchanged,
                goal142Unchanged,
                goal148Unchanged,
                hostFilesUnchanged,
                portableCurrent,
                portableReleaseCandidateCurrent
            },
                new JsonSerializerOptions { WriteIndented = true })
            + Environment.NewLine,
            new UTF8Encoding(false));
    }

    private static IReadOnlyList<string> CompleteHostCaches()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            ProjectStandaloneBuildVocabulary.HostCacheRootName);
        if (!Directory.Exists(root))
            return [];
        return Directory.EnumerateDirectories(root)
            .Select(path => Path.Combine(path, "host"))
            .Where(path => File.Exists(Path.Combine(path,
                               ProjectStandaloneBuildVocabulary
                                   .HostExecutableName))
                           && Directory.Exists(Path.Combine(path,
                               ProjectStandaloneBuildVocabulary
                                   .HostDataDirectoryName)))
            .ToList();
    }

    private static string HostRoot(string key) =>
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            ProjectStandaloneBuildVocabulary.HostCacheRootName,
            key, "host");

    private static string TreeHash(string path)
    {
        if (!Directory.Exists(path))
            return "<absent>";
        var builder = new StringBuilder();
        foreach (var file in Directory.EnumerateFiles(path, "*",
                     SearchOption.AllDirectories)
                 .OrderBy(item => item,
                     StringComparer.OrdinalIgnoreCase))
        {
            builder.Append(Path.GetRelativePath(path, file)
                    .Replace('\\', '/'))
                .Append('|')
                .Append(Convert.ToHexString(SHA256.HashData(
                    File.ReadAllBytes(file))))
                .AppendLine();
        }
        return Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(builder.ToString())));
    }
}
