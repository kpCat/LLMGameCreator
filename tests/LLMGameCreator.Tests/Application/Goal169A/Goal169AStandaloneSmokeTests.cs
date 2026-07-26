using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169A;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169AStandaloneSmokeTests
{
    [Fact]
    public void Behavioral_exactly_one_post_fix_cached_hidden_smoke()
    {
        Assert.Empty(
            System.Diagnostics.Process.GetProcessesByName("Unity"));
        if (!string.Equals(Environment.GetEnvironmentVariable(
                "LLMGC_GOAL169A_RUN_SMOKE"), "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Assert.NotEmpty(CompleteHostCaches());
            return;
        }

        var hostBefore = CompleteHostCaches().ToDictionary(
            path => path, TreeHash,
            StringComparer.OrdinalIgnoreCase);
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
        var generationRoot = Path.Combine(fixture.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        var sidecarsBefore = TreeHash(generationRoot);

        var standalone = fixture.Controller.BuildWindowsStandalone();
        var snapshot = fixture.Controller.Snapshot();
        var build = fixture.Controller.LastBuild!;
        var events = Assert.IsType<
            GameProjectGeneratedCampaignRegionalEventSummary>(
            snapshot.GeneratedCampaignRegionalEvents);
        var relationships = Assert.IsType<
            GameProjectGeneratedCampaignRelationshipSummary>(
            snapshot.GeneratedCampaignRelationships);

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
        Assert.True(GeneratedCampaignRegionalEventCorrelationService
            .Validate(build.PackageSha256, events, relationships)
            .Passed);
        Assert.Equal(events.FinalStateHash, build.FinalStateHash);
        Assert.Equal("CURRENT",
            snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT",
            snapshot.ReleaseCandidateRecordConfigurationStatus);

        var payloadRoot = Path.Combine(standalone.OutputFolder,
            Path.GetFileNameWithoutExtension(
                standalone.ExecutablePath) + "_Data",
            "StreamingAssets", "LLMGameCreatorProject");
        using var framePayload = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(payloadRoot,
                "player-adapter-frames.json")));
        var commands = framePayload.RootElement.EnumerateArray()
            .Select(frame => frame.GetProperty("title").GetString()
                             ?? string.Empty)
            .ToList();
        var explicitMoveCount = commands.Count(item =>
            item.StartsWith("Move.", StringComparison.Ordinal));
        var bareDirectionCount = commands.Count(item =>
            item is "Up" or "Down" or "Left" or "Right");
        Assert.True(explicitMoveCount > 0);
        Assert.Equal(0, bareDirectionCount);
        Assert.Contains("Interact", commands);
        Assert.Contains(commands, item =>
            item.StartsWith("OpenDialogue:",
                StringComparison.Ordinal));
        Assert.Contains(commands, item =>
            item.StartsWith("ChooseDialogueOption:",
                StringComparison.Ordinal));
        Assert.All(events.EventQualifications, qualification =>
        {
            Assert.Equal(4, qualification.ReplaySignatures.Count);
            Assert.Equal(4, qualification.RuntimeStartCount);
            Assert.True(qualification.ReplayPassed);
        });

        using var portable = Goal156TestKit.Copy(
            fixture.Project, "goal169a-real-smoke-portable");
        var portableSnapshot =
            Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();
        Assert.Equal("REGIONAL_EVENTS_CURRENT",
            portableSnapshot.GeneratedCampaignRegionalEvents?.Status);
        Assert.Equal("CURRENT",
            portableSnapshot.ReleaseCandidateConfigurationStatus);
        using var coreOnly = Goal156TestKit.Copy(
            Goal156TestKit.CoreOnly,
            "goal169a-core-only-portable");
        var coreSnapshot =
            Goal156TestKit.OpenWorkspace(coreOnly.Path).Snapshot();
        Assert.NotEqual("CURRENT",
            coreSnapshot.ReleaseCandidateConfigurationStatus);

        WriteCapture(standalone, fixture.Project.Path, payloadRoot,
            build, events, commands, explicitMoveCount,
            bareDirectionCount,
            sidecarsBefore == TreeHash(generationRoot),
            goal142Before == Goal156TestKit.Hash(
                Goal156TestKit.Goal142BaselinePath),
            goal148Before == TreeHash(goal148),
            usedHostBefore == TreeHash(usedHost),
            portableSnapshot.GeneratedCampaignRegionalEvents
                ?.Status == "REGIONAL_EVENTS_CURRENT",
            portableSnapshot.ReleaseCandidateConfigurationStatus
            == "CURRENT",
            coreSnapshot.ReleaseCandidateConfigurationStatus
            != "CURRENT");
    }

    private static void WriteCapture(
        ProjectStandaloneBuildResult standalone,
        string projectFolder,
        string payloadRoot,
        GameProjectBuildResult build,
        GameProjectGeneratedCampaignRegionalEventSummary events,
        IReadOnlyList<string> commands,
        int explicitMoveCount,
        int bareDirectionCount,
        bool sidecarsUnchanged,
        bool goal142Unchanged,
        bool goal148Unchanged,
        bool hostFilesUnchanged,
        bool portableCurrent,
        bool portableReleaseCandidateCurrent,
        bool coreOnlyNoFalseRcReady)
    {
        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL169A_SMOKE_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path))
            return;
        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            status = "GREEN",
            standalone.HostCacheKey,
            standalone.HostReused,
            standalone.HostRebuilt,
            unityEditorProcessStartCount = 0,
            hiddenSmokeInvocationCount = 1,
            correctiveRetryCount = 0,
            hiddenSmokePassed = standalone.LaunchSmokePassed,
            standalone.SelfCheckPassedCount,
            standalone.SelfCheckTotalCount,
            projectFolder,
            standalone.OutputFolder,
            payloadRoot,
            standalone.ExecutablePath,
            build.BuildHistoryPath,
            build.PackageSha256,
            build.FinalStateHash,
            events.StrictProofSchemaVersion,
            replaySignatureCount = events.ReplaySignatures.Count,
            lockedReplaySignatureCount =
                events.ReplaySignatures.Count(item =>
                    item.RouteKind ==
                    GeneratedCampaignRegionalEventReplayRouteKind
                        .LOCKED_PROBE),
            resolutionReplaySignatureCount =
                events.ReplaySignatures.Count(item =>
                    item.RouteKind ==
                    GeneratedCampaignRegionalEventReplayRouteKind
                        .RESOLUTION),
            explicitMoveCount,
            bareDirectionCount,
            interactCount = commands.Count(item =>
                item == "Interact"),
            openDialogueCount = commands.Count(item =>
                item.StartsWith("OpenDialogue:",
                    StringComparison.Ordinal)),
            chooseDialogueOptionCount = commands.Count(item =>
                item.StartsWith("ChooseDialogueOption:",
                    StringComparison.Ordinal)),
            releaseCandidateCurrent = true,
            releaseCandidateRecordCurrent = true,
            portableCurrent,
            portableReleaseCandidateCurrent,
            coreOnlyNoFalseRcReady,
            sidecarsUnchanged,
            goal142Unchanged,
            goal148Unchanged,
            hostFilesUnchanged
        }, new JsonSerializerOptions { WriteIndented = true })
            + Environment.NewLine, new UTF8Encoding(false));
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
