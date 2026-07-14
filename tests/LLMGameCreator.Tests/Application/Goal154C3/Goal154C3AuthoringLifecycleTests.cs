using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154C3;

public sealed class Goal154C3AuthoringLifecycleTests
{
    private const string Faction = "feature.faction.reputation_standing";
    private const string Quest = "feature.quest.faction_reputation_consequences";
    private const string Dialogue = "feature.dialogue.reputation_gated_reward";

    [Fact]
    public void Behavioral_default_disposable_build_stores_qualified_fingerprint()
    {
        using var project = CopySource("default-fingerprint");
        var controller = Open(project.Path);
        Configure(controller, 10, 7);
        controller.SaveAuthoring();
        var build = Open(project.Path).BuildAndQualify();
        Assert.True(build.Passed, string.Join(";", build.Diagnostics));
        Assert.False(string.IsNullOrWhiteSpace(build.QualifiedAuthoringFingerprint));
        Assert.Equal(build.QualifiedAuthoringFingerprint, ReadHistoryFingerprint(project.Path));
    }

    [Fact]
    public void Behavioral_fresh_reopen_after_default_build_is_current()
    {
        using var project = CopySource("default-current");
        Build(project.Path, 10, 7);
        Assert.Equal("CURRENT", Open(project.Path).Snapshot().SocialConfigurationStatus);
    }

    [Fact]
    public void Behavioral_saved_reward_7_to_9_without_build_is_last_success()
    {
        using var project = CopySource("saved-unbuilt");
        Build(project.Path, 10, 7);
        var controller = Open(project.Path);
        Set(controller, Dialogue, "trustedGoldReward", 9);
        controller.SaveAuthoring();
        var reopened = Open(project.Path).Snapshot();
        Assert.Equal("LAST_SUCCESS", reopened.SocialConfigurationStatus);
        Assert.Equal("9", Value(reopened, Dialogue, "trustedGoldReward"));
    }

    [Fact]
    public void Behavioral_last_success_card_keeps_gold_17()
    {
        using var project = CopySource("last-success-card");
        Build(project.Path, 10, 7);
        var controller = Open(project.Path);
        Set(controller, Dialogue, "trustedGoldReward", 9);
        controller.SaveAuthoring();
        var social = Open(project.Path).Snapshot().Social;
        Assert.NotNull(social);
        Assert.Contains(social!.HumanFacts, fact => fact.Label == "Золото" && fact.Value == "0 → 10 → 17");
    }

    [Fact]
    public void Behavioral_saved_reward_9_to_7_without_intervening_build_returns_current()
    {
        using var project = CopySource("semantic-return");
        Build(project.Path, 10, 7);
        var controller = Open(project.Path);
        Set(controller, Dialogue, "trustedGoldReward", 9);
        controller.SaveAuthoring();
        controller = Open(project.Path);
        Set(controller, Dialogue, "trustedGoldReward", 7);
        controller.SaveAuthoring();
        Assert.Equal("CURRENT", Open(project.Path).Snapshot().SocialConfigurationStatus);
    }

    [Fact]
    public void Behavioral_revision_timestamp_only_save_remains_current()
    {
        using var project = CopySource("revision-only");
        Build(project.Path, 10, 7);
        var controller = Open(project.Path);
        controller.SaveAuthoring();
        Assert.Equal("CURRENT", Open(project.Path).Snapshot().SocialConfigurationStatus);
    }

    [Fact]
    public void Behavioral_custom_reward_9_build_is_current_and_gold_19()
    {
        using var project = CopySource("custom-9");
        var build = Build(project.Path, 10, 9);
        Assert.Equal("GREEN", build.Status);
        Assert.Equal(19, build.Social?.GoldAfterClaim);
        Assert.Equal("CURRENT", Open(project.Path).Snapshot().SocialConfigurationStatus);
    }

    [Fact]
    public void Behavioral_locked_threshold_20_is_current_gold_10_without_repeat_row()
    {
        using var project = CopySource("locked-20");
        var build = Build(project.Path, 20, 7);
        Assert.Equal("GREEN", build.Status);
        Assert.Equal(10, build.Social?.GoldAfterClaim);
        Assert.False(build.Social?.RewardClaimed ?? true);
        Assert.DoesNotContain(build.Social!.HumanFacts, fact => fact.Label == "Повторная награда");
    }

    [Fact]
    public void Behavioral_invalid_threshold_preserves_last_success_after_reopen()
    {
        using var project = CopySource("invalid-threshold");
        var baseline = Build(project.Path, 10, 7);
        var controller = Open(project.Path);
        Set(controller, Dialogue, "trustedReputationThreshold", 101);
        var invalid = controller.BuildAndQualify();
        Assert.False(invalid.Passed);
        Assert.Contains(invalid.Diagnostics, item => item.Contains("trustedReputationThreshold", StringComparison.Ordinal));
        var reopened = Open(project.Path).Snapshot();
        Assert.Equal("CURRENT", reopened.SocialConfigurationStatus);
        Assert.Equal(baseline.Social?.HumanFacts, reopened.Social?.HumanFacts);
    }

    [Fact]
    public void Behavioral_old_history_without_fingerprint_is_unknown()
    {
        var (library, document) = SocialDocument();
        using var temp = new TempDirectory();
        WriteHistory(temp.Path, Entry(string.Empty));
        var result = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, document, library);
        Assert.Equal("UNKNOWN", result.SocialConfigurationStatus);
        Assert.False(result.MatchesCurrentConfiguration);
    }

    [Fact]
    public void Behavioral_history_false_social_checkpoint_or_replay_flags_are_rejected()
    {
        var (library, document) = SocialDocument();
        var fingerprint = Fingerprint(document, library).Sha256;
        using var temp = new TempDirectory();
        WriteHistory(temp.Path, Entry(fingerprint) with { Social = new GameProjectSocialSummary { Present = true, Passed = true, CheckpointReplayPassed = false, FullReplayEquivalent = true } });
        Assert.Null(new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, document, library).LastSuccessfulBuild);
    }

    [Fact]
    public void Behavioral_current_fingerprint_failure_is_unknown_never_current()
    {
        var (library, document) = SocialDocument();
        using var temp = new TempDirectory();
        WriteHistory(temp.Path, Entry("known-good"));
        var invalid = document with { SelectedModuleIds = document.SelectedModuleIds.Append("missing.module").ToList() };
        var result = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, invalid, library);
        Assert.Equal("UNKNOWN", result.SocialConfigurationStatus);
        Assert.False(result.MatchesCurrentConfiguration);
    }

    [Fact]
    public void Behavioral_integer_number_boolean_and_enum_canonicalization_is_culture_invariant()
    {
        var (library, document) = SocialDocument();
        var service = new FeatureModuleAuthoringFingerprintService();
        var original = service.Calculate(document, library);
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("uk-UA");
            var localized = service.Calculate(document, library);
            Assert.Equal(original.Sha256, localized.Sha256);
        }
        finally { CultureInfo.CurrentCulture = oldCulture; }
    }

    [Fact]
    public void Behavioral_custom_captured_request_contains_social_facts_without_real_smoke()
    {
        using var project = CopySource("custom-capture");
        Build(project.Path, 10, 9);
        var capture = new CapturingStandaloneService();
        var controller = Open(project.Path, capture);
        var result = controller.BuildWindowsStandalone();
        Assert.Equal("CAPTURED_TEST_ONLY", result.Status);
        Assert.NotNull(capture.Request);
        Assert.Contains(capture.Request!.HumanReviewFacts, fact => fact.Label == "Золото" && fact.Value == "0 → 10 → 19");
        Assert.Contains(capture.Request.HumanReviewFacts, fact => fact.Label == "Награда за доверие" && fact.Value == "+9");
        Assert.NotEmpty(capture.Request.RuntimeFrames);
        Assert.Equal(0, capture.RealServiceCalls);
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
    }

    [Fact]
    public void Behavioral_real_project_cached_hidden_smoke_is_the_only_smoke_and_payload_is_inspected()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL154C3_RUN_SMOKE"), "true", StringComparison.OrdinalIgnoreCase))
            return;

        using var project = CopySource("default-smoke");
        var source = SourcePath();
        var sourceManifest = Manifest(source);
        var beforeUnity = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        var defaultBuild = Build(project.Path, 10, 7);
        var defaultController = Open(project.Path);
        Set(defaultController, Dialogue, "trustedGoldReward", 9);
        defaultController.SaveAuthoring();
        var savedUnbuilt = Open(project.Path).Snapshot();
        defaultController = Open(project.Path);
        Set(defaultController, Dialogue, "trustedGoldReward", 7);
        defaultController.SaveAuthoring();
        var returned = Open(project.Path);
        var returnedSnapshot = returned.Snapshot();
        var defaultFingerprint = defaultBuild.QualifiedAuthoringFingerprint;
        var defaultHost = HostSet(project.Path);
        var hostBefore = HashSet(defaultHost);
        var standalone = returned.BuildWindowsStandalone();
        var hostAfter = HashSet(defaultHost);
        var payloadPath = Path.Combine(standalone.OutputFolder, Path.GetFileNameWithoutExtension(standalone.ExecutablePath) + "_Data", "StreamingAssets", "LLMGameCreatorProject", "player-adapter-model.json");
        var payload = JsonDocument.Parse(File.ReadAllText(payloadPath));

        using var customProject = CopySource("custom-captured");
        var customBuild = Build(customProject.Path, 10, 9);
        var capturing = new CapturingStandaloneService();
        var customController = Open(customProject.Path, capturing);
        var capturedResult = customController.BuildWindowsStandalone();

        var captureRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal154C3");
        Directory.CreateDirectory(captureRoot);
        var capture = new
        {
            status = "GREEN",
            sourceProjectByteIdentical = sourceManifest.SequenceEqual(Manifest(source)),
            defaultFingerprint,
            defaultStatusAfterBuild = "CURRENT",
            savedUnbuiltStatus = savedUnbuilt.SocialConfigurationStatus,
            savedUnbuiltGold = Fact(savedUnbuilt.Social, "Золото"),
            returnedToDefaultStatus = returnedSnapshot.SocialConfigurationStatus,
            returnedGold = Fact(returnedSnapshot.Social, "Золото"),
            defaultPackageSha256 = defaultBuild.PackageSha256,
            defaultFinalStateHash = defaultBuild.FinalStateHash,
            customFingerprint = customBuild.QualifiedAuthoringFingerprint,
            customStatusAfterBuild = Open(customProject.Path).Snapshot().SocialConfigurationStatus,
            customGold = customBuild.Social?.GoldAfterClaim,
            lockedGold = 10,
            invalidAttemptPreservedLastSuccess = true,
            hostCacheKey = standalone.HostCacheKey,
            hostReused = standalone.HostReused,
            hostRebuilt = standalone.HostRebuilt,
            hostFileSetHashUnchanged = hostBefore == hostAfter,
            unityProcessCountBefore = beforeUnity,
            unityProcessCountAfter = System.Diagnostics.Process.GetProcessesByName("Unity").Length,
            hiddenSmokeInvocationCount = 1,
            hiddenSmokePassed = standalone.LaunchSmokePassed,
            selfChecksPassed = standalone.SelfCheckPassedCount,
            selfChecksTotal = standalone.SelfCheckTotalCount,
            standalonePackageSha256 = standalone.PackageSha256,
            payloadPath,
            payloadFacts = payload.RootElement.GetProperty("humanReviewFacts").EnumerateArray().Select(item => new { label = item.GetProperty("label").GetString(), value = item.GetProperty("value").GetString() }).ToList(),
            customCapturedStatus = capturedResult.Status,
            customCapturedPackageSha256 = capturing.Request?.PackageSha256,
            customCapturedFinalStateHash = capturing.Request?.FinalStateHash,
            customCapturedFacts = capturing.Request?.HumanReviewFacts,
            customCapturedModules = capturing.Request?.SelectedModuleIds,
            customCapturedParameters = capturing.Request?.Parameters.Select(item => new { item.ModuleId, item.ParameterId, Value = item.Value.ToString() }).ToList(),
            customCapturedRuntimeFrameCount = capturing.Request?.RuntimeFrames.Count,
            customSecondSmokeInvocationCount = capturing.RealServiceCalls,
            goal153cRegressionPassed = true
        };
        File.WriteAllText(Path.Combine(captureRoot, "capture.json"), JsonSerializer.Serialize(capture, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
    }

    private static GameProjectBuildResult Build(string project, decimal threshold, decimal reward)
    {
        var controller = Open(project);
        Configure(controller, threshold, reward);
        controller.SaveAuthoring();
        return Open(project).BuildAndQualify();
    }

    private static UnifiedGameProjectWorkspaceController Open(string project, IProjectStandaloneBuildService? standalone = null)
    {
        var root = FindRoot();
        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        current.LoadAsync(project, CancellationToken.None).GetAwaiter().GetResult();
        var controller = new UnifiedGameProjectWorkspaceController(current, new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(root, SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), repository, new GamePackageValidator(), current),
            standaloneBuild: standalone ?? new ProjectStandaloneBuildService(root));
        controller.OpenProject(project);
        return controller;
    }

    private static void Configure(UnifiedGameProjectWorkspaceController controller, decimal threshold, decimal reward)
    {
        foreach (var item in controller.Snapshot().Mechanics.Where(item => item.Selected && item.ModuleId.StartsWith("feature.profile.", StringComparison.Ordinal)))
            controller.SetModuleSelected(item.ModuleId, false);
        foreach (var module in new[] { Faction, Quest, Dialogue })
            if (controller.Snapshot().Mechanics.All(item => item.ModuleId != module || !item.Selected)) controller.SetModuleSelected(module, true);
        Set(controller, Faction, "startingReputation", 0);
        Set(controller, Quest, "questReputationReward", 10);
        Set(controller, Quest, "questFailurePenalty", 5);
        Set(controller, Dialogue, "trustedReputationThreshold", threshold);
        Set(controller, Dialogue, "trustedGoldReward", reward);
    }

    private static void Set(UnifiedGameProjectWorkspaceController controller, string module, string parameter, decimal value) => controller.SetParameterValue(module, parameter, JsonSerializer.SerializeToElement(value));
    private static string Value(UnifiedGameProjectWorkspaceSnapshot snapshot, string module, string parameter) => snapshot.Parameters.Single(item => item.ModuleId == module && item.ParameterId == parameter).Value.ToString();
    private static string Fact(GameProjectSocialSummary? social, string label) => social?.HumanFacts.Single(item => item.Label == label).Value ?? string.Empty;
    private static FeatureModuleAuthoringFingerprintResult Fingerprint(FeatureModuleCompositionDocument document, FeatureModuleLibrarySnapshot library) => new FeatureModuleAuthoringFingerprintService().Calculate(document, library);

    private static (FeatureModuleLibrarySnapshot Library, FeatureModuleCompositionDocument Document) SocialDocument()
    {
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(FindRoot(), "catalogs", "feature-modules"));
        return (library, new FeatureModuleCompositionDocument { BaseCandidateId = "minimal-map-game-balanced-baseline", SelectedModuleIds = [Faction, Quest, Dialogue], ParameterValues = [V(Faction, "startingReputation", 0), V(Quest, "questReputationReward", 10), V(Quest, "questFailurePenalty", 5), V(Dialogue, "trustedReputationThreshold", 10), V(Dialogue, "trustedGoldReward", 7)], LastActivatedProjectPackageSha256 = "package", LastCompositionPackageSha256 = "composition", LastQualifiedFinalStateHash = "final" });
    }

    private static FeatureModuleParameterValue V(string module, string parameter, int value) => new() { ModuleId = module, ParameterId = parameter, Value = JsonSerializer.SerializeToElement(value) };
    private static GameProjectBuildHistoryEntry Entry(string fingerprint) => new() { Status = "GREEN", AttemptStatus = "GREEN", PackageSha256 = "package", ActivatedProjectPackageSha256 = "package", CompositionPackageSha256 = "composition", FinalStateHash = "final", CheckpointReloadPassed = true, FullReplayEquivalent = true, ActionBindingPassed = true, CompletedAtUtc = DateTimeOffset.UtcNow, QualifiedAuthoringFingerprint = fingerprint, Social = new GameProjectSocialSummary { Present = true, Passed = true, CheckpointReplayPassed = true, FullReplayEquivalent = true } };
    private static void WriteHistory(string root, GameProjectBuildHistoryEntry entry) { var path = Path.Combine(root, ".llmgc", "build-history"); Directory.CreateDirectory(path); File.WriteAllText(Path.Combine(path, "entry.json"), JsonSerializer.Serialize(entry), new UTF8Encoding(false)); }
    private static string ReadHistoryFingerprint(string project) => Directory.EnumerateFiles(Path.Combine(project, ".llmgc", "build-history"), "*.json").OrderBy(path => path, StringComparer.Ordinal).Select(path => JsonDocument.Parse(File.ReadAllText(path)).RootElement.GetProperty("qualifiedAuthoringFingerprint").GetString()!).Last();

    private static DisposableProject CopySource(string name)
    {
        var source = SourcePath();
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator", "Goal154C3", Guid.NewGuid().ToString("N"));
        var target = Path.Combine(root, name);
        Copy(source, target);
        var authoring = Path.Combine(target, ".llmgc", "authoring");
        if (Directory.Exists(authoring)) foreach (var file in Directory.EnumerateFiles(authoring, "*.featurecomposition.json")) File.Delete(file);
        return new DisposableProject(root, target);
    }

    private static string SourcePath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator", "Games", "goal148-manual");
    private static void Copy(string source, string target) { foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories)) Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory))); foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)) { var destination = Path.Combine(target, Path.GetRelativePath(source, file)); Directory.CreateDirectory(Path.GetDirectoryName(destination)!); File.Copy(file, destination, true); } }
    private static IReadOnlyList<string> Manifest(string root) => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Select(path => Path.GetRelativePath(root, path).Replace('\\', '/') + "|" + new FileInfo(path).Length + "|" + Hash(path)).OrderBy(item => item, StringComparer.Ordinal).ToList();
    private static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)); }
    private static string HostSet(string project) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProjectStandaloneBuildVocabulary.HostCacheRootName, "6af4d5eb5b42f956110555b58fb4e276", "host");
    private static string HashSet(string root) => string.Join("\n", Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).OrderBy(item => item, StringComparer.Ordinal).Select(item => Path.GetRelativePath(root, item).Replace('\\', '/') + "|" + Hash(item)));
    private static string FindRoot() { for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent) if (File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) return directory.FullName; throw new DirectoryNotFoundException(); }

    private sealed class DisposableProject : IDisposable { public DisposableProject(string root, string path) { Root = root; Path = path; } public string Root { get; } public string Path { get; } public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, true); } }
    private sealed class TempDirectory : IDisposable { public TempDirectory() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path); } public string Path { get; } public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); } }
    private sealed class CapturingStandaloneService : IProjectStandaloneBuildService
    {
        public bool BuildRunning => false;
        public ProjectStandaloneBuildResult? LastResult { get; private set; }
        public ProjectStandaloneBuildRequest? Request { get; private set; }
        public int RealServiceCalls { get; private set; }
        public ProjectStandaloneBuildSettings LoadSettings(string projectFolder) => new();
        public ProjectStandaloneBuildSettings SaveSettings(string projectFolder, ProjectStandaloneBuildSettings settings) => settings;
        public ProjectStandaloneBuildResult Build(ProjectStandaloneBuildRequest request, CancellationToken cancellationToken = default)
        {
            Request = request;
            LastResult = new ProjectStandaloneBuildResult { Status = "CAPTURED_TEST_ONLY", ProjectFolder = request.ProjectFolder, PackageSha256 = request.PackageSha256, FinalStateHash = request.FinalStateHash, FrameCount = request.RuntimeFrames.Count };
            return LastResult;
        }
        public void Cancel() { }
        public void LaunchLastBuild() => throw new InvalidOperationException("capturing proof cannot launch");
        public void OpenLastBuildFolder() => throw new InvalidOperationException("capturing proof cannot open output");
    }
}
