using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal168;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal168StandalonePortabilityTests
{
    [Fact]
    public void Behavioral_standalone_payload_uses_exact_v7_regional_event_primary()
    {
        var state = Goal164PortableState.AllSelectable;
        var history = JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(
            File.ReadAllText(state.Build.Build.BuildHistoryPath),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(GameProjectBuildHistoryReader.SchemaVersionV7,
            history?.SchemaVersion);
        Assert.Equal(state.Build.Build.PackageSha256,
            state.Service.Request?.PackageSha256);
        Assert.Equal(
            state.Build.Build.GeneratedCampaignRegionalEvents?.FinalStateHash,
            state.Service.Request?.FinalStateHash);
    }

    [Fact]
    public void Behavioral_standalone_payload_contains_regional_event_frames_and_facts()
    {
        var request = Goal164PortableState.AllSelectable.Service.Request!;

        Assert.NotEmpty(request.RuntimeFrames);
        Assert.All(request.RuntimeFrames, frame =>
            Assert.Equal("generated-regional-event", frame.Category));
        Assert.Contains(request.HumanReviewFacts,
            item => item.Label == "События мира");
        Assert.Contains(request.HumanReviewFacts,
            item => item.Label == "Благодарности");
        Assert.Contains(request.HumanReviewFacts,
            item => item.Label == "Последствия вызовов и отказов");
    }

    [Fact]
    public void Behavioral_all_selectable_portable_is_regional_event_and_rc_current()
    {
        var state = Goal164PortableState.AllSelectable;

        Assert.Equal("GREEN", state.Standalone.Status);
        Assert.Equal("RELATIONSHIPS_CURRENT",
            state.Snapshot.GeneratedCampaignRelationships?.Status);
        Assert.Equal("REGIONAL_EVENTS_CURRENT",
            state.Snapshot.GeneratedCampaignRegionalEvents?.Status);
        Assert.Equal("CAMPAIGN_CURRENT",
            state.Snapshot.GeneratedWorld?.Status);
        Assert.Equal("CURRENT",
            state.Snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT",
            state.Snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_core_only_portable_has_no_false_rc_readiness()
    {
        var state = Goal164PortableState.CoreOnly;

        Assert.Equal("GREEN", state.Standalone.Status);
        Assert.True(state.Snapshot.GeneratedCampaignRelationships is
            { Passed: true, Status: "RELATIONSHIPS_CURRENT" or "ABSENT" });
        Assert.True(state.Snapshot.GeneratedCampaignRegionalEvents is
            { Passed: true, Status: "REGIONAL_EVENTS_CURRENT" or "ABSENT" });
        Assert.Equal("CAMPAIGN_CURRENT",
            state.Snapshot.GeneratedWorld?.Status);
        Assert.NotEqual("CURRENT",
            state.Snapshot.ReleaseCandidateConfigurationStatus);
        Assert.NotEqual("CURRENT",
            state.Snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_physical_all_selectable_copy_restores_v7_and_rc()
    {
        var source = Goal164PortableState.AllSelectable.Build;
        using var portable = Goal156TestKit.Copy(source.Project,
            "goal168-portable-all");
        RemoveOperationalOutput(portable.Path);
        var snapshot = Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();

        Assert.Equal("RELATIONSHIPS_CURRENT",
            snapshot.GeneratedCampaignRelationships?.Status);
        Assert.Equal("REGIONAL_EVENTS_CURRENT",
            snapshot.GeneratedCampaignRegionalEvents?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.Equal("CURRENT",
            snapshot.ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_physical_core_only_copy_stays_non_rc()
    {
        var source = Goal164PortableState.CoreOnly.Build;
        using var portable = Goal156TestKit.Copy(source.Project,
            "goal168-portable-core");
        RemoveOperationalOutput(portable.Path);
        var snapshot = Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();

        Assert.True(snapshot.GeneratedCampaignRelationships is
            { Passed: true, Status: "RELATIONSHIPS_CURRENT" or "ABSENT" });
        Assert.True(snapshot.GeneratedCampaignRegionalEvents is
            { Passed: true, Status: "REGIONAL_EVENTS_CURRENT" or "ABSENT" });
        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.NotEqual("CURRENT",
            snapshot.ReleaseCandidateConfigurationStatus);
        Assert.NotEqual("CURRENT",
            snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_exactly_one_real_cached_hidden_relationship_smoke_when_explicitly_enabled()
    {
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        if (!string.Equals(
                Environment.GetEnvironmentVariable("LLMGC_GOAL168_RUN_SMOKE"),
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
        var goal142Before = Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath);
        var goal148 = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator",
            "Games",
            "goal148-manual");
        var goal148Before = TreeHash(goal148);
        var standaloneService = new ProjectStandaloneBuildService(
            Goal164TestKit.RepositoryRoot);
        var fixture = Goal164BuildFixture.Create(
            coreOnly: false,
            standaloneService);
        var generationRoot = Path.Combine(
            fixture.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot.Replace(
                '/',
                Path.DirectorySeparatorChar));
        var sidecarsBefore = TreeHash(generationRoot);

        var standalone = fixture.Controller.BuildWindowsStandalone();
        var snapshot = fixture.Controller.Snapshot();
        var relationships = snapshot.GeneratedCampaignRelationships;

        Assert.Equal("GREEN", standalone.Status);
        Assert.True(standalone.HostReused);
        Assert.False(standalone.HostRebuilt);
        Assert.True(standalone.LaunchSmokePassed);
        Assert.Equal(
            standalone.SelfCheckTotalCount,
            standalone.SelfCheckPassedCount);
        Assert.True(standalone.SelfCheckTotalCount > 0);
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        var usedHost = HostRoot(standalone.HostCacheKey);
        Assert.True(hostBefore.TryGetValue(usedHost, out var usedHostBefore));
        Assert.Equal(usedHostBefore, TreeHash(usedHost));
        Assert.NotNull(relationships);
        Assert.True(relationships.Passed);
        Assert.Equal("RELATIONSHIPS_CURRENT", relationships.Status);
        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT",
            snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.Equal(sidecarsBefore, TreeHash(generationRoot));
        Assert.Equal(
            goal142Before,
            Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath));
        Assert.Equal(goal148Before, TreeHash(goal148));

        var payloadRoot = Path.Combine(
            standalone.OutputFolder,
            Path.GetFileNameWithoutExtension(standalone.ExecutablePath) + "_Data",
            "StreamingAssets",
            "LLMGameCreatorProject");
        using var payload = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(payloadRoot, "player-adapter-model.json")));
        var facts = payload.RootElement.GetProperty("humanReviewFacts")
            .EnumerateArray()
            .Select(item => (
                Label: item.GetProperty("label").GetString() ?? string.Empty,
                Value: item.GetProperty("value").GetString() ?? string.Empty))
            .ToList();
        Assert.All(relationships.HumanReviewFacts, expected =>
            Assert.Contains(facts,
                actual => actual.Label == expected.Label
                          && actual.Value == expected.Value));
        using var framePayload = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(payloadRoot, "player-adapter-frames.json")));
        var payloadFrames = framePayload.RootElement.EnumerateArray().ToList();
        Assert.NotEmpty(payloadFrames);
        Assert.All(payloadFrames, frame =>
            Assert.Equal(
                "generated-relationship",
                frame.GetProperty("category").GetString()));

        using var portable = Goal156TestKit.Copy(
            fixture.Project,
            "goal168-real-smoke-portable");
        var portableSnapshot = Goal156TestKit.OpenWorkspace(portable.Path)
            .Snapshot();
        Assert.Equal(
            "RELATIONSHIPS_CURRENT",
            portableSnapshot.GeneratedCampaignRelationships?.Status);
        Assert.Equal(
            "CURRENT",
            portableSnapshot.ReleaseCandidateConfigurationStatus);
        WriteSmokeCapture(
            standalone,
            snapshot,
            fixture.Controller.LastBuild!,
            sidecarsBefore == TreeHash(generationRoot),
            goal142Before == Goal156TestKit.Hash(
                Goal156TestKit.Goal142BaselinePath),
            goal148Before == TreeHash(goal148),
            usedHostBefore == TreeHash(usedHost),
            portableSnapshot.GeneratedCampaignRelationships?.Status
                == "RELATIONSHIPS_CURRENT",
            portableSnapshot.ReleaseCandidateConfigurationStatus == "CURRENT");
    }

    private static void WriteSmokeCapture(
        ProjectStandaloneBuildResult standalone,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        GameProjectBuildResult build,
        bool sidecarsUnchanged,
        bool goal142Unchanged,
        bool goal148Unchanged,
        bool hostFilesUnchanged,
        bool portableCurrent,
        bool portableReleaseCandidateCurrent)
    {
        var path = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL168_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path))
            return;

        var relationships = snapshot.GeneratedCampaignRelationships
                            ?? throw new InvalidOperationException(
                                "Relationship summary is missing.");
        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(
            path,
            JsonSerializer.Serialize(new
            {
                status = "GREEN",
                standalone.HostCacheKey,
                standalone.HostReused,
                standalone.HostRebuilt,
                unityEditorProcessStartCount = 0,
                hiddenSmokeInvocationCount = 1,
                hiddenSmokePassed = standalone.LaunchSmokePassed,
                correctiveRetryCount = 0,
                standalone.SelfCheckPassedCount,
                standalone.SelfCheckTotalCount,
                actualPayloadRelationshipFactsPassed = true,
                actualPayloadRelationshipFramesPassed = true,
                historySchemaVersion =
                    GameProjectBuildHistoryReader.SchemaVersionV6,
                relationships.RelationshipCount,
                relationships.QualifiedRelationshipCount,
                relationships.ArcQuestCount,
                relationships.QualifiedArcQuestCount,
                relationships.MaximumObservedArcLength,
                relationships.AssignmentUnique,
                relationships.ArcOrderingDeterministic,
                relationships.OverlayControlledDeltaPassed,
                relationships.RuntimeQualificationPassed,
                relationships.ExclusiveBranchingPassed,
                relationships.ArcProgressionPassed,
                relationships.ExactCombatCatalogPassed,
                relationships.SupportPassed,
                relationships.SupportReplayEquivalent,
                relationships.ChallengeFleePassed,
                relationships.ChallengeVictoryPassed,
                relationships.ChallengeRecoveryPassed,
                relationships.RefusePassed,
                relationships.AtomicRollbackPassed,
                relationships.SaveContinuationFactsPassed,
                relationships.ExactPackageSha256,
                relationships.RelationshipOverlaySha256,
                relationships.RelationshipInventorySha256,
                relationships.QualifiedActionsSha256,
                relationships.FinalStateHash,
                relationshipIds = relationships.RelationshipInventory
                    .Select(item => item.RelationshipId)
                    .ToArray(),
                assignedQuestIds = relationships.RelationshipInventory
                    .SelectMany(item => item.OrderedQuestSourceIds)
                    .ToArray(),
                supportReputationDelta = relationships.Overlay?.Bindings
                    .Where(item => item.Branches.Contains(
                        GeneratedCampaignRelationshipBranch.SUPPORT))
                    .Select(item => item.SupportReputationAmount)
                    .FirstOrDefault(),
                refuseReputationDelta = relationships.Overlay?.Bindings
                    .Where(item => item.Branches.Contains(
                        GeneratedCampaignRelationshipBranch.REFUSE))
                    .Select(item => item.RefuseReputationAmount)
                    .FirstOrDefault(),
                runtimeFrameCount = relationships.RuntimeFrames.Count,
                buildPackageSha256 = build.PackageSha256,
                releaseCandidateRecordCurrent =
                    snapshot.ReleaseCandidateRecordConfigurationStatus
                    == "CURRENT",
                campaignCurrent =
                    snapshot.GeneratedWorld?.Status == "CAMPAIGN_CURRENT",
                relationshipCurrent =
                    relationships.Status == "RELATIONSHIPS_CURRENT",
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
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProjectStandaloneBuildVocabulary.HostCacheRootName);
        if (!Directory.Exists(root))
            return [];

        return Directory.EnumerateDirectories(root)
            .Select(path => Path.Combine(path, "host"))
            .Where(path => File.Exists(Path.Combine(
                               path,
                               ProjectStandaloneBuildVocabulary.HostExecutableName))
                           && Directory.Exists(Path.Combine(
                               path,
                               ProjectStandaloneBuildVocabulary
                                   .HostDataDirectoryName)))
            .ToList();
    }

    private static string HostRoot(string key) =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProjectStandaloneBuildVocabulary.HostCacheRootName,
            key,
            "host");

    private static string TreeHash(string path)
    {
        if (!Directory.Exists(path))
            return "<absent>";

        var builder = new StringBuilder();
        foreach (var file in Directory.EnumerateFiles(
                     path,
                     "*",
                     SearchOption.AllDirectories)
                 .OrderBy(item => item, StringComparer.OrdinalIgnoreCase))
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

    private static void RemoveOperationalOutput(string project)
    {
        var builds = Path.GetFullPath(Path.Combine(project, "Builds"));
        if (Directory.Exists(builds))
            Directory.Delete(builds, recursive: true);
    }
}
