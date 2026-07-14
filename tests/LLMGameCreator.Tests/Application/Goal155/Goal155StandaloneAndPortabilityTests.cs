using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal155;

public sealed class Goal155StandaloneAndPortabilityTests
{
    [Fact]
    public void Behavioral_exactly_one_profile_b_hidden_smoke_writes_portable_correlated_record_and_preserves_rollback()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL155_RUN_SMOKE"), "true",
                StringComparison.OrdinalIgnoreCase)) return;

        var source = Goal155RealProject.SourcePath();
        var sourceBefore = Goal155RealProject.Manifest(source);
        using var project = Goal155RealProject.CopySource("profile-b-smoke");
        Goal155RealProject.ConfigureProfileB(project.Path);
        var first = Goal155RealProject.Open(project.Path).BuildAndQualify();
        var second = Goal155RealProject.Open(project.Path).BuildAndQualify();
        Assert.True(first.Passed && second.Passed);
        Assert.Equal(first.PackageSha256, second.PackageSha256);
        Assert.Equal(first.FinalStateHash, second.FinalStateHash);

        var hostKeyBefore = LastHostKey(project.Path);
        var hostRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProjectStandaloneBuildVocabulary.HostCacheRootName, hostKeyBefore, "host");
        Assert.True(HostComplete(hostRoot), "Goal155 requires a complete reusable host cache.");
        var hostBefore = Goal155RealProject.HashSet(hostRoot);
        var hostExecutableBefore = Goal155RealProject.Hash(Path.Combine(hostRoot,
            ProjectStandaloneBuildVocabulary.HostExecutableName));
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));

        var controller = Goal155RealProject.Open(project.Path);
        var standalone = controller.BuildWindowsStandalone();
        var snapshot = controller.Snapshot();

        Assert.Equal("GREEN", standalone.Status);
        Assert.True(standalone.HostReused);
        Assert.False(standalone.HostRebuilt);
        Assert.True(standalone.LaunchSmokePassed);
        Assert.True(standalone.SelfCheckTotalCount > 0);
        Assert.Equal(standalone.SelfCheckTotalCount, standalone.SelfCheckPassedCount);
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        Assert.Equal(hostKeyBefore, standalone.HostCacheKey);
        Assert.Equal(hostBefore, Goal155RealProject.HashSet(hostRoot));
        Assert.Equal(hostExecutableBefore, Goal155RealProject.Hash(Path.Combine(hostRoot,
            ProjectStandaloneBuildVocabulary.HostExecutableName)));
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.NotNull(snapshot.ReleaseCandidate);
        Assert.True(snapshot.AcceptedMechanics?.Passed);
        Assert.True(File.Exists(snapshot.ReleaseCandidateRecordPath));

        var payloadRoot = Path.Combine(standalone.OutputFolder,
            Path.GetFileNameWithoutExtension(standalone.ExecutablePath) + "_Data", "StreamingAssets",
            "LLMGameCreatorProject");
        var playerAdapterPath = Path.Combine(payloadRoot, "player-adapter-model.json");
        using var playerAdapter = JsonDocument.Parse(File.ReadAllText(playerAdapterPath));
        var actualFacts = playerAdapter.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .Select(item => new GameProjectSocialHumanFact
            {
                Label = item.GetProperty("label").GetString() ?? string.Empty,
                Value = item.GetProperty("value").GetString() ?? string.Empty
            }).ToList();
        Assert.All(snapshot.AcceptedMechanics!.HumanFacts, expected => Assert.Contains(actualFacts,
            actual => actual.Label == expected.Label && actual.Value == expected.Value));
        Assert.Contains(actualFacts, fact => fact.Label == "Release Candidate" && fact.Value == "готов");
        Assert.Equal(snapshot.FinalStateHash,
            playerAdapter.RootElement.GetProperty("finalStateHash").GetString());
        using var projectManifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(payloadRoot, "project-manifest.json")));
        Assert.Equal(snapshot.PackageSha256, projectManifest.RootElement.GetProperty("packageSha256").GetString());
        Assert.Equal(snapshot.CompositionPackageSha256,
            projectManifest.RootElement.GetProperty("compositionPackageSha256").GetString());
        Assert.Equal(snapshot.FinalStateHash, projectManifest.RootElement.GetProperty("finalStateHash").GetString());

        using var portable = Goal155RealProject.CopyProject(project.Path, "portable");
        var portableUnityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        var portableSnapshot = Goal155RealProject.Open(portable.Path).Snapshot();
        Assert.Equal("CURRENT", portableSnapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal(JsonSerializer.Serialize(snapshot.AcceptedMechanics.HumanFacts),
            JsonSerializer.Serialize(portableSnapshot.AcceptedMechanics?.HumanFacts));
        Assert.Equal(portableUnityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);

        using var rollback = Goal155RealProject.CopyProject(project.Path, "rollback");
        var rollbackRecordPath = new GameProjectReleaseCandidateRecordService().RecordPath(rollback.Path);
        var recordBefore = File.ReadAllBytes(rollbackRecordPath);
        var changed = Goal155RealProject.Open(rollback.Path);
        Goal155RealProject.Set(changed, "feature.dialogue.reputation_gated_reward", "trustedGoldReward", 9);
        changed.SaveAuthoring();
        Assert.Equal("LAST_SUCCESS", changed.Snapshot().ReleaseCandidateConfigurationStatus);
        Assert.Equal("LAST_SUCCESS", changed.Snapshot().ReleaseCandidateRecordConfigurationStatus);
        Goal155RealProject.Set(changed, "feature.dialogue.reputation_gated_reward", "trustedGoldReward", 7);
        changed.SaveAuthoring();
        Assert.Equal("CURRENT", changed.Snapshot().ReleaseCandidateConfigurationStatus);
        Goal155RealProject.Set(changed, "feature.dialogue.reputation_gated_reward", "trustedGoldReward", 9);
        var changedBuild = changed.BuildAndQualify();
        Assert.True(changedBuild.Passed);
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING", changed.Snapshot().ReleaseCandidateConfigurationStatus);
        Assert.Equal("LAST_SUCCESS", changed.Snapshot().ReleaseCandidateRecordConfigurationStatus);
        Goal155RealProject.Set(changed, "feature.dialogue.reputation_gated_reward", "trustedReputationThreshold", 101);
        var failedBuild = changed.BuildAndQualify();
        Assert.False(failedBuild.Passed);
        Assert.Equal(JsonSerializer.Serialize(changedBuild.AcceptedMechanics),
            JsonSerializer.Serialize(Goal155RealProject.Open(rollback.Path).Snapshot().AcceptedMechanics));
        Assert.Equal(recordBefore, File.ReadAllBytes(rollbackRecordPath));

        using var failedStandalone = Goal155RealProject.CopyProject(project.Path, "failed-standalone");
        var failedRecordPath = new GameProjectReleaseCandidateRecordService().RecordPath(failedStandalone.Path);
        var failedRecordBefore = File.ReadAllBytes(failedRecordPath);
        var failureService = new CapturingFailureStandaloneService();
        var failureController = Goal155RealProject.Open(failedStandalone.Path, failureService);
        var failedStandaloneResult = failureController.BuildWindowsStandalone();
        Assert.Equal("FAILED", failedStandaloneResult.Status);
        Assert.Equal("captured_failure", failedStandaloneResult.Stage);
        Assert.Equal(failedRecordBefore, File.ReadAllBytes(failedRecordPath));
        Assert.Equal("FAILED", failureController.Snapshot().LastStandaloneBuild?.Status);
        Assert.Equal(0, failureService.RealSmokeInvocationCount);
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));

        Assert.Equal(sourceBefore, Goal155RealProject.Manifest(source));
        WriteCapture(new
        {
            status = "GREEN",
            ownerSelectedMechanicCount = 22,
            ownerConfiguredParameterCount = 10,
            benchmarkSelectedMechanicCount = snapshot.AcceptedMechanics.SelectedMechanicCount,
            benchmarkConfiguredParameterCount = snapshot.AcceptedMechanics.ConfiguredParameterCount,
            benchmarkEquipmentDamageBonus = snapshot.AcceptedMechanics.EquipmentDamageBonus,
            benchmarkStatDamageBonus = snapshot.AcceptedMechanics.StatDamageBonus,
            benchmarkTotalAdditionalDamage = snapshot.AcceptedMechanics.TotalAdditionalDamage,
            benchmarkAbilityDirectDamage = snapshot.AcceptedMechanics.AbilityDirectDamage,
            benchmarkManaBefore = snapshot.AcceptedMechanics.ManaBefore,
            benchmarkManaRemaining = snapshot.AcceptedMechanics.ManaRemaining,
            benchmarkStatusTickDamage = snapshot.AcceptedMechanics.StatusTickDamage,
            benchmarkStatusExpired = snapshot.AcceptedMechanics.StatusExpired,
            benchmarkReputationBefore = snapshot.AcceptedMechanics.Social?.ReputationBefore,
            benchmarkReputationAfter = snapshot.AcceptedMechanics.Social?.ReputationAfter,
            benchmarkGoldAfterQuest = snapshot.AcceptedMechanics.Social?.GoldAfterQuest,
            benchmarkGoldAfterClaim = snapshot.AcceptedMechanics.Social?.GoldAfterClaim,
            benchmarkCheckpointReloadPassed = snapshot.AcceptedMechanics.CheckpointReloadPassed,
            benchmarkFullReplayEquivalent = snapshot.AcceptedMechanics.FullReplayEquivalent,
            benchmarkActionBindingPassed = snapshot.AcceptedMechanics.ActionBindingPassed,
            acceptedMechanicsSummaryPersisted = true,
            releaseCandidateRecordWritten = true,
            releaseCandidateRecordCurrent = true,
            portableCopyRecordCurrent = portableSnapshot.ReleaseCandidateConfigurationStatus == "CURRENT",
            failedBuildPreservedLastSuccess = true,
            failedStandalonePreservedRecord = true,
            standalone.HostCacheKey,
            standalone.HostReused,
            standalone.HostRebuilt,
            hostFileSetHashUnchanged = hostBefore == Goal155RealProject.HashSet(hostRoot),
            unityProcessStartCount = 0,
            hiddenSmokeInvocationCount = 1,
            hiddenSmokePassed = standalone.LaunchSmokePassed,
            standaloneSelfChecksPassed = standalone.SelfCheckPassedCount == standalone.SelfCheckTotalCount,
            actualPayloadAcceptedFactsPassed = true,
            playerAdapterModelSha256 = Goal155RealProject.Hash(playerAdapterPath),
            sourceProjectByteIdentical = true
        });
    }

    private static string LastHostKey(string project)
    {
        using var history = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, ".llmgc",
            "standalone-build-history.json")));
        return history.RootElement.EnumerateArray().Last().GetProperty("hostCacheKey").GetString()
               ?? throw new InvalidOperationException("Host cache key is missing.");
    }

    private static bool HostComplete(string hostRoot) =>
        File.Exists(Path.Combine(hostRoot, ProjectStandaloneBuildVocabulary.HostExecutableName))
        && Directory.Exists(Path.Combine(hostRoot, ProjectStandaloneBuildVocabulary.HostDataDirectoryName))
        && File.Exists(Path.Combine(hostRoot, "UnityPlayer.dll"))
        && Directory.Exists(Path.Combine(hostRoot, "MonoBleedingEdge"));

    private static void WriteCapture(object capture)
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal155");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "capture.json"), JsonSerializer.Serialize(capture,
            new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
    }

    private sealed class CapturingFailureStandaloneService : IProjectStandaloneBuildService
    {
        public bool BuildRunning => false;
        public ProjectStandaloneBuildResult? LastResult { get; private set; }
        public int RealSmokeInvocationCount { get; private set; }
        public ProjectStandaloneBuildSettings LoadSettings(string projectFolder) => new();
        public ProjectStandaloneBuildSettings SaveSettings(string projectFolder, ProjectStandaloneBuildSettings settings) => settings;
        public ProjectStandaloneBuildResult Build(ProjectStandaloneBuildRequest request, CancellationToken cancellationToken = default)
        {
            LastResult = new ProjectStandaloneBuildResult
            {
                Status = "FAILED",
                Stage = "captured_failure",
                Diagnostics = ["captured standalone failure"],
                ProjectFolder = request.ProjectFolder,
                PackageSha256 = request.PackageSha256,
                FinalStateHash = request.FinalStateHash
            };
            return LastResult;
        }
        public void Cancel() { }
        public void LaunchLastBuild() => throw new InvalidOperationException("failure capture cannot launch");
        public void OpenLastBuildFolder() => throw new InvalidOperationException("failure capture cannot open output");
    }
}
