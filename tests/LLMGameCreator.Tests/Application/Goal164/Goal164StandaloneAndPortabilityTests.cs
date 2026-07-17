using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Generation.Procedural;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal164;

[Collection(Goal160Collection.Name)]
public sealed class Goal164StandaloneAndPortabilityTests
{
    [Fact]
    public void Behavioral_standalone_payload_uses_combat_overlay_primary_hashes()
    {
        var state = Goal164PortableState.AllSelectable;

        Assert.Equal(state.Build.Build.PackageSha256, state.Service.Request?.PackageSha256);
        Assert.Equal(state.Build.Build.CompositionPackageSha256,
            state.Service.Request?.CompositionPackageSha256);
        Assert.Equal(state.Build.Build.FinalStateHash, state.Service.Request?.FinalStateHash);
    }

    [Fact]
    public void Behavioral_standalone_payload_contains_combat_human_facts()
    {
        var facts = Goal164PortableState.AllSelectable.Service.Request!.HumanReviewFacts;

        Assert.Contains(facts, item => item.Label == "Боевая готовность" && item.Value == "подтверждена");
        Assert.Contains(facts, item => item.Label == "Победа и награда" && item.Value == "получены");
    }

    [Fact]
    public void Behavioral_standalone_payload_contains_generated_combat_runtime_frames()
    {
        var state = Goal164PortableState.AllSelectable;

        Assert.NotEmpty(state.Service.Request!.RuntimeFrames);
        Assert.Equal(state.Build.Build.RuntimeFrames.Count, state.Service.Request.RuntimeFrames.Count);
        Assert.Contains(state.Service.Request.RuntimeFrames,
            item => item.ActionId == nameof(LLMGameCreator.Runtime.Abstractions.GameRuntimeCommandType.BasicAttack));
    }

    [Fact]
    public void Behavioral_all_selectable_release_candidate_becomes_current_after_portable_build()
    {
        var state = Goal164PortableState.AllSelectable;

        Assert.True(state.Finalization.Status == "GREEN",
            state.Finalization.Stage + ":" + string.Join(",", state.Finalization.Diagnostics));
        Assert.Equal("CURRENT", state.Snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", state.Snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_core_only_portable_build_is_green_and_campaign_current()
    {
        var state = Goal164PortableState.CoreOnly;

        Assert.Equal("GREEN", state.Standalone.Status);
        Assert.Equal("CAMPAIGN_CURRENT", state.Build.Build.GeneratedEncounterCombat?.Status);
        Assert.Equal(state.Build.Build.PackageSha256, state.Standalone.PackageSha256);
    }

    [Fact]
    public void Behavioral_core_only_does_not_claim_false_rc_readiness()
    {
        var state = Goal164PortableState.CoreOnly;

        Assert.NotEqual("CURRENT", state.Snapshot.ReleaseCandidateConfigurationStatus);
        Assert.NotEqual("CURRENT", state.Snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_projects_primary_action_is_collect_and_play()
    {
        Assert.Equal("Собрать и играть", UnifiedGameProjectWorkspaceVocabulary.PrimaryActionText);
    }

    [Fact]
    public void Behavioral_exactly_one_real_cached_hidden_combat_smoke_when_explicitly_enabled()
    {
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL164_RUN_SMOKE"), "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Assert.NotEmpty(CompleteHostCaches());
            return;
        }

        var hostBefore = CompleteHostCaches().ToDictionary(path => path, TreeHash,
            StringComparer.OrdinalIgnoreCase);
        Assert.NotEmpty(hostBefore);
        var goal142Before = Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath);
        var goal148 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        var goal148Before = TreeHash(goal148);
        var standaloneService = new ProjectStandaloneBuildService(Goal164TestKit.RepositoryRoot);
        var fixture = Goal164BuildFixture.Create(coreOnly: false, standaloneService);
        var generationRoot = Path.Combine(fixture.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot.Replace('/', Path.DirectorySeparatorChar));
        var sidecarsBefore = TreeHash(generationRoot);

        var standalone = fixture.Controller.BuildWindowsStandalone();
        var snapshot = fixture.Controller.Snapshot();

        Assert.Equal("GREEN", standalone.Status);
        Assert.True(standalone.HostReused);
        Assert.False(standalone.HostRebuilt);
        Assert.True(standalone.LaunchSmokePassed);
        Assert.Equal(standalone.SelfCheckTotalCount, standalone.SelfCheckPassedCount);
        Assert.True(standalone.SelfCheckTotalCount > 0);
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        var usedHost = HostRoot(standalone.HostCacheKey);
        Assert.True(hostBefore.TryGetValue(usedHost, out var usedHostBefore));
        Assert.Equal(usedHostBefore, TreeHash(usedHost));
        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedEncounterCombat?.Status);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.Equal(sidecarsBefore, TreeHash(generationRoot));
        Assert.Equal(goal142Before, Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath));
        Assert.Equal(goal148Before, TreeHash(goal148));

        var payloadRoot = Path.Combine(standalone.OutputFolder,
            Path.GetFileNameWithoutExtension(standalone.ExecutablePath) + "_Data", "StreamingAssets",
            "LLMGameCreatorProject");
        using var payload = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(payloadRoot, "player-adapter-model.json")));
        var facts = payload.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .Select(item => (Label: item.GetProperty("label").GetString() ?? string.Empty,
                Value: item.GetProperty("value").GetString() ?? string.Empty)).ToList();
        Assert.All(snapshot.GeneratedEncounterCombat!.HumanReviewFacts, expected =>
            Assert.Contains(facts, actual => actual.Label == expected.Label && actual.Value == expected.Value));

        using var portable = Goal156TestKit.Copy(fixture.Project, "goal164-real-smoke-portable");
        var portableSnapshot = Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();
        Assert.Equal("CAMPAIGN_CURRENT", portableSnapshot.GeneratedWorld?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", portableSnapshot.GeneratedEncounterCombat?.Status);
        Assert.Equal("CURRENT", portableSnapshot.ReleaseCandidateConfigurationStatus);
        var currentPackage = fixture.Current.CurrentPackage!;
        var generatedEncounterSourceIds = currentPackage.GeneratedContent.Encounters
            .Select(item => item.SourceId).ToHashSet(StringComparer.Ordinal);
        var generatedParticipantCount = currentPackage.Game.Encounters
            .Where(item => item.Metadata.TryGetValue("sourceEncounterSeedId", out var sourceId)
                           && generatedEncounterSourceIds.Contains(sourceId))
            .Sum(item => item.Participants.Count);
        WriteSmokeCapture(standalone, snapshot, fixture.Controller.LastBuild!, generatedParticipantCount,
            sidecarsBefore == TreeHash(generationRoot),
            goal142Before == Goal156TestKit.Hash(Goal156TestKit.Goal142BaselinePath),
            goal148Before == TreeHash(goal148), usedHostBefore == TreeHash(usedHost),
            portableSnapshot.GeneratedWorld?.Status == "CAMPAIGN_CURRENT");
    }

    private static void WriteSmokeCapture(
        ProjectStandaloneBuildResult standalone,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        GameProjectBuildResult build,
        int generatedParticipantCount,
        bool sidecarsUnchanged,
        bool goal142Unchanged,
        bool goal148Unchanged,
        bool hostFilesUnchanged,
        bool portableCurrent)
    {
        var path = Environment.GetEnvironmentVariable("LLMGC_GOAL164_CAPTURE_PATH");
        if (string.IsNullOrWhiteSpace(path)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(new
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
            actualPayloadCombatFactsPassed = true,
            combatContractId = snapshot.GeneratedEncounterCombat?.ContractId,
            contractSourcePackageSha256 = snapshot.GeneratedEncounterCombat?.ContractSourcePackageSha256,
            generatedEncounterCount = snapshot.GeneratedEncounterCombat?.GeneratedEncounterCount,
            qualifiedGeneratedEncounterCount = snapshot.GeneratedEncounterCombat?.QualifiedEncounterCount,
            generatedParticipantsReboundCount = generatedParticipantCount,
            laneBCombatPackageSha256 = build.PackageSha256,
            laneACompatibilityPassed = build.AcceptedMechanicsCompatibility?.Passed,
            historySchemaVersion = GameProjectBuildHistoryReader.SchemaVersionV4,
            completeQuestCommandCount = snapshot.GeneratedEncounterCombat?.CompleteQuestCommandCount,
            advanceObjectiveCommandCount = snapshot.GeneratedEncounterCombat?.AdvanceObjectiveCommandCount,
            representativeReplayEquivalent = snapshot.GeneratedEncounterCombat?.ReplayPassed,
            releaseCandidateRecordCurrent = snapshot.ReleaseCandidateRecordConfigurationStatus == "CURRENT",
            campaignCurrent = snapshot.GeneratedWorld?.Status == "CAMPAIGN_CURRENT",
            combatCurrent = snapshot.GeneratedEncounterCombat?.Status == "CAMPAIGN_CURRENT",
            sidecarsUnchanged,
            goal142Unchanged,
            goal148Unchanged,
            hostFilesUnchanged,
            portableCurrent
        }, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            new UTF8Encoding(false));
    }

    private static IReadOnlyList<string> CompleteHostCaches()
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProjectStandaloneBuildVocabulary.HostCacheRootName);
        if (!Directory.Exists(root)) return [];
        return Directory.EnumerateDirectories(root).Select(path => Path.Combine(path, "host"))
            .Where(path => File.Exists(Path.Combine(path, ProjectStandaloneBuildVocabulary.HostExecutableName))
                           && Directory.Exists(Path.Combine(path,
                               ProjectStandaloneBuildVocabulary.HostDataDirectoryName))
                           && File.Exists(Path.Combine(path, "UnityPlayer.dll"))
                           && Directory.Exists(Path.Combine(path, "MonoBleedingEdge"))
                           && File.Exists(Path.Combine(path, "host-cache-manifest.json")))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string HostRoot(string key) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ProjectStandaloneBuildVocabulary.HostCacheRootName, key, "host");

    private static string TreeHash(string root)
    {
        var stable = string.Join("\n", Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/') + "|" + Goal156TestKit.Hash(path)));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(stable))).ToLowerInvariant();
    }
}

internal static class Goal164PortableState
{
    private static readonly Lazy<Goal164PortableFixture> AllSelectableFixture = new(() => Create(false, finalize: true));
    private static readonly Lazy<Goal164PortableFixture> CoreOnlyFixture = new(() => Create(true, finalize: false));

    public static Goal164PortableFixture AllSelectable => AllSelectableFixture.Value;
    public static Goal164PortableFixture CoreOnly => CoreOnlyFixture.Value;

    private static Goal164PortableFixture Create(bool coreOnly, bool finalize)
    {
        var standalone = new Goal164CapturingStandaloneService();
        var build = Goal164BuildFixture.Create(coreOnly, standalone);
        var result = build.Controller.BuildWindowsStandalone();
        Assert.True(result.Status == "GREEN",
            result.Stage + ":" + string.Join(",", result.Diagnostics));
        var finalization = finalize
            ? build.Controller.FinalizeCurrentReleaseCandidate()
            : new GameProjectReleaseCandidateFinalizationResult { Status = "NOT_REQUESTED" };
        var snapshot = build.Controller.Snapshot();
        return new Goal164PortableFixture(build, standalone, result, finalization, snapshot);
    }
}

internal sealed record Goal164PortableFixture(
    Goal164BuildFixture Build,
    Goal164CapturingStandaloneService Service,
    ProjectStandaloneBuildResult Standalone,
    GameProjectReleaseCandidateFinalizationResult Finalization,
    UnifiedGameProjectWorkspaceSnapshot Snapshot);

internal sealed class Goal164CapturingStandaloneService : IProjectStandaloneBuildService
{
    private readonly CapturingPayloadStandaloneService _inner = new();

    public bool BuildRunning => _inner.BuildRunning;
    public ProjectStandaloneBuildResult? LastResult => _inner.LastResult;
    public ProjectStandaloneBuildRequest? Request => _inner.Request;
    public ProjectStandaloneBuildSettings LoadSettings(string projectFolder) =>
        _inner.LoadSettings(projectFolder);
    public ProjectStandaloneBuildSettings SaveSettings(
        string projectFolder,
        ProjectStandaloneBuildSettings settings) => _inner.SaveSettings(projectFolder, settings);
    public ProjectStandaloneBuildResult Build(
        ProjectStandaloneBuildRequest request,
        CancellationToken cancellationToken = default) => _inner.Build(request, cancellationToken);
    public ProjectStandaloneCurrentQualifiedResultReadResult LoadCurrentQualifiedResult(
        string projectFolder,
        string packageId) => LastResult is { Status: "GREEN" } result
        ? new ProjectStandaloneCurrentQualifiedResultReadResult { Passed = true, Result = result }
        : new ProjectStandaloneCurrentQualifiedResultReadResult
        {
            Diagnostics = "standalone.current_history_missing"
        };
    public void Cancel() => _inner.Cancel();
    public void LaunchLastBuild() => _inner.LaunchLastBuild();
    public void OpenLastBuildFolder() => _inner.OpenLastBuildFolder();
}
