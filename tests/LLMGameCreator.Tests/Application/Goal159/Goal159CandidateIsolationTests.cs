using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal159;

[Collection(Goal156Collection.Name)]
public sealed class Goal159CandidateIsolationTests
{
    [Fact]
    public async Task Behavioral_legacy_project_rejects_regeneration()
    {
        using var scope = Goal156TestKit.Scope("goal159-legacy-reject");
        var summary = await scope.Service.CreateAsync(Goal156TestKit.TemplateRequest(scope.Root, "legacy"),
            CancellationToken.None);
        var bundle = Goal159TestKit.CreateBundle(summary.FolderPath);

        var error = Assert.Throws<InvalidOperationException>(() =>
            bundle.Controller.CreateGeneratedWorldRegenerationRequest(new()));

        Assert.Equal("regeneration.not_generated_project", error.Message);
    }

    [Fact]
    public void Behavioral_invalid_generated_source_rejects_regeneration()
    {
        using var project = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "goal159-invalid-source");
        Goal159TestKit.MutateSource(project.Path, root => root["planSha256"] = new string('0', 64));
        var bundle = Goal159TestKit.CreateBundle(project.Path);

        var error = Assert.Throws<InvalidOperationException>(() =>
            bundle.Controller.CreateGeneratedWorldRegenerationRequest(new()));

        Assert.Equal("regeneration.generated_source_invalid", error.Message);
    }

    [Fact]
    public void Behavioral_semantic_no_op_changes_nothing()
    {
        using var project = Goal156TestKit.Copy(Goal157PortableState.Value.Project, "goal159-no-op");
        var before = Goal159TestKit.TreeHashes(project.Path);
        var bundle = Goal159TestKit.CreateBundle(project.Path);
        var snapshot = bundle.Controller.Snapshot();
        var request = bundle.Controller.CreateGeneratedWorldRegenerationRequest(
            snapshot.GeneratedWorldGenerationRequest!);

        var preview = bundle.Controller.PreviewGeneratedWorldRegeneration(request);

        Assert.Equal("regeneration.no_semantic_change", Assert.Single(preview.Diagnostics));
        Assert.Equal(before, Goal159TestKit.TreeHashes(project.Path));
    }

    [Theory]
    [InlineData("source")]
    [InlineData("authoring")]
    [InlineData("package")]
    [InlineData("identity")]
    [InlineData("rc")]
    public void Behavioral_optimistic_token_mismatch_rejects_before_candidate(string token)
    {
        using var project = Goal156TestKit.Copy(Goal157PortableState.Value.Project,
            "goal159-token-" + token);
        var bundle = Goal159TestKit.CreateBundle(project.Path);
        var request = bundle.Controller.CreateGeneratedWorldRegenerationRequest(
            Goal159TestKit.ChangedRequest(bundle.Controller.Snapshot(), "goal159-token-change"));
        request = token switch
        {
            "source" => request with { ExpectedSourceRecordSha256 = new string('0', 64) },
            "authoring" => request with { ExpectedAuthoringRevision = request.ExpectedAuthoringRevision + 1 },
            "package" => request with { ExpectedActivatedPackageSha256 = new string('0', 64) },
            "identity" => request with { ExpectedProjectIdentityFingerprint = new string('0', 64) },
            _ => request with { ExpectedReleaseCandidateRecordSha256 = new string('0', 64) }
        };

        var preview = bundle.Controller.PreviewGeneratedWorldRegeneration(request);

        Assert.Equal("regeneration." + (token == "rc" ? "release_candidate" : token) + "_changed",
            Assert.Single(preview.Diagnostics));
        Assert.Empty(preview.CandidateRoot);
    }

    [Fact]
    public void Behavioral_candidate_root_is_outside_authoritative_project()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.False(fixture.CandidateRoot.StartsWith(
            fixture.Project.Path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase));
        Assert.Contains("RegenerationCandidates", fixture.CandidateRoot, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_transient_directories_are_excluded_from_candidate_clone()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.False(fixture.CandidateHadBuilds);
        Assert.False(fixture.CandidateHadBuildStaging);
        Assert.False(fixture.CandidateHadRegenerationRoot);
    }

    [Fact]
    public void Behavioral_candidate_source_v2_strict_validation_passes()
    {
        var source = Goal159SuccessState.Value.CandidateSource;

        Assert.True(source.Passed, string.Join(Environment.NewLine, source.Diagnostics));
        Assert.Equal(SeededGeneratedProjectVocabulary.SourceV2SchemaVersion, source.Source?.SchemaVersion);
    }

    [Fact]
    public void Behavioral_candidate_selected_modules_equal_original()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.Equal(fixture.OriginalSelectedModuleIds, fixture.CandidateSelectedModuleIds);
    }

    [Fact]
    public void Behavioral_candidate_parameter_json_equals_original()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.Equal(fixture.OriginalParameterJson, fixture.CandidateParameterJson);
    }

    [Fact]
    public void Behavioral_candidate_identity_equals_original()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.Equal(fixture.OriginalIdentityJson, fixture.CandidateIdentityJson);
    }

    [Fact]
    public void Behavioral_candidate_lane_a_accepted_compatibility_passes()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.True(fixture.Preview.CandidateSnapshot?.AcceptedMechanics?.Passed);
        Assert.True(fixture.Preview.CandidateSnapshot?.AcceptedMechanicsCompatibility?.Passed);
    }

    [Fact]
    public void Behavioral_candidate_travel_build_passes()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.True(fixture.Preview.CandidateBuild?.Passed);
        Assert.True(fixture.Preview.CandidateBuild?.GeneratedRegionTravel?.Passed);
    }

    [Fact]
    public void Behavioral_candidate_repeat_and_fresh_reopen_are_current()
    {
        var fixture = Goal159SuccessState.Value;

        Assert.Equal("GREEN", fixture.Preview.Status);
        Assert.Equal("TRAVEL_CURRENT", fixture.Preview.CandidateSnapshot?.GeneratedWorld?.Status);
        Assert.Equal(fixture.Preview.CandidateBuild?.FinalStateHash,
            fixture.Preview.CandidateSnapshot?.GeneratedRegionTravel?.FinalStateHash);
    }

    [Fact]
    public void Behavioral_candidate_old_rc_is_never_current()
    {
        var status = Goal159SuccessState.Value.CandidateReleaseCandidateStatus;

        Assert.Contains(status, new[] { "LAST_SUCCESS", "ABSENT" });
        Assert.NotEqual("CURRENT", status);
    }

    [Fact]
    public void Behavioral_stale_source_token_at_final_recheck_rejects_apply()
    {
        using var project = Goal156TestKit.Copy(Goal157PortableState.Value.Project, "goal159-final-recheck");
        var bundle = Goal159TestKit.CreateBundle(project.Path);
        var request = bundle.Controller.CreateGeneratedWorldRegenerationRequest(
            Goal159TestKit.ChangedRequest(bundle.Controller.Snapshot(), "goal159-final-recheck-seed"));
        var preview = bundle.Controller.PreviewGeneratedWorldRegeneration(request);
        Assert.Equal("GREEN", preview.Status);
        var packageBefore = Goal156TestKit.Hash(Path.Combine(project.Path, "package.json"));
        var historyBefore = Goal159TestKit.TreeHashes(Path.Combine(project.Path,
            UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot));
        File.AppendAllText(Goal159TestKit.SourcePath(project.Path), Environment.NewLine);

        var result = bundle.Controller.ApplyGeneratedWorldRegeneration(request, preview);

        Assert.False(result.Applied);
        Assert.Contains("regeneration.source_changed", result.Diagnostics);
        Assert.Equal(packageBefore, Goal156TestKit.Hash(Path.Combine(project.Path, "package.json")));
        Assert.Equal(historyBefore, Goal159TestKit.TreeHashes(Path.Combine(project.Path,
            UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot)));
        if (Directory.Exists(preview.CandidateRoot)) Directory.Delete(preview.CandidateRoot, recursive: true);
    }

    [Fact]
    public async Task Behavioral_concurrent_regeneration_request_is_rejected_atomically()
    {
        using var project = Goal156TestKit.Copy(Goal157PortableState.Value.Project, "goal159-concurrent");
        var bundle = Goal159TestKit.CreateBundle(project.Path);
        var request = bundle.Controller.CreateGeneratedWorldRegenerationRequest(
            Goal159TestKit.ChangedRequest(bundle.Controller.Snapshot(), "goal159-concurrent-seed"));
        var firstTask = Task.Run(() => bundle.Controller.PreviewGeneratedWorldRegeneration(request));
        Assert.True(SpinWait.SpinUntil(() => bundle.Regeneration.Running, TimeSpan.FromSeconds(5)));

        var rejected = bundle.Controller.PreviewGeneratedWorldRegeneration(request);
        var first = await firstTask;

        Assert.Contains("regeneration.concurrent_operation", rejected.Diagnostics);
        Assert.Equal("GREEN", first.Status);
        if (Directory.Exists(first.CandidateRoot)) Directory.Delete(first.CandidateRoot, recursive: true);
    }

    [Fact]
    public void Behavioral_invalid_candidate_changes_no_authoritative_file()
    {
        using var project = Goal156TestKit.Copy(Goal157PortableState.Value.Project, "goal159-invalid-candidate");
        var bundle = Goal159TestKit.CreateBundle(project.Path);
        var before = Goal159TestKit.TreeHashes(project.Path);
        var changed = Goal159TestKit.ChangedRequest(bundle.Controller.Snapshot(), "goal159-invalid-candidate-seed")
            with { Mode = "unsupported-mode" };
        var request = bundle.Controller.CreateGeneratedWorldRegenerationRequest(changed);

        var preview = bundle.Controller.PreviewGeneratedWorldRegeneration(request);

        Assert.Equal("FAILED", preview.Status);
        Assert.Equal(before, Goal159TestKit.TreeHashes(project.Path));
    }
}

internal static partial class Goal159TestKit
{
    public static Goal159ControllerBundle CreateBundle(string project)
    {
        var repository = new JsonGamePackageRepository();
        var validator = new GamePackageValidator();
        var current = new CurrentGamePackageService(repository);
        current.LoadAsync(project, CancellationToken.None).GetAwaiter().GetResult();
        var baseline = new Goal142GeneratedProjectBaselineProvider(Goal156TestKit.RepositoryRoot);
        var overlay = new GeneratedProjectOverlayService(validator);
        var source = new SeededGeneratedProjectSourceService(
            validator, overlayService: overlay, baselineProvider: baseline);
        var summary = new GameProjectGeneratedWorldSummaryService(overlay);
        var runtime = new DefaultGameRuntime();
        var serializer = new RuntimeStateSerializer();
        var builder = new GameProjectBuildAndQualificationService(
            Goal156TestKit.RepositoryRoot,
            SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
            repository,
            validator,
            current,
            generatedSource: source,
            generatedSummary: summary,
            generatedActivation: new GameProjectGeneratedWorldActivationService(runtime, serializer, validator),
            generatedTravelOverlay: new GeneratedWorldTravelOverlayService(),
            generatedTravelActivation: new GameProjectGeneratedRegionTravelActivationService(runtime, serializer));
        var artifactFactory = new SeededGeneratedProjectArtifactFactory(baseline, validator, overlay: overlay);
        var record = new GameProjectSeedRegenerationRecordService(Goal156TestKit.RepositoryRoot, source);
        var regeneration = new GameProjectSeedRegenerationService(
            Goal156TestKit.RepositoryRoot,
            current,
            repository,
            validator,
            builder,
            artifactFactory,
            source,
            recordService: record);
        var authoring = new GameProjectFeatureModuleAuthoringService(Goal156TestKit.RepositoryRoot);
        var controller = new UnifiedGameProjectWorkspaceController(
            current,
            authoring,
            builder,
            standaloneBuild: new ProjectStandaloneBuildService(Goal156TestKit.RepositoryRoot),
            generatedSourceService: source,
            generatedWorldSummaryService: summary,
            regenerationService: regeneration);
        controller.OpenProject(project);
        return new Goal159ControllerBundle(controller, regeneration, source, authoring, current, record);
    }

    public static SeededGeneratedProjectGenerationRequest ChangedRequest(
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        string seed) => new()
    {
        Seed = seed,
        Mode = snapshot.GeneratedWorldResolvedOptions!.Mode,
        PresetId = snapshot.GeneratedWorldResolvedOptions.PresetId
    };

    public static SortedDictionary<string, string> TreeHashes(string root)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            result[Path.GetRelativePath(root, file).Replace('\\', '/')] =
                Goal156TestKit.Hash(file);
        return result;
    }

    public static string ParameterJson(GameProjectAuthoringState state) => JsonSerializer.Serialize(
        state.Document.ParameterValues.OrderBy(value => value.ModuleId, StringComparer.Ordinal)
            .ThenBy(value => value.ParameterId, StringComparer.Ordinal));
}

internal sealed record Goal159ControllerBundle(
    UnifiedGameProjectWorkspaceController Controller,
    GameProjectSeedRegenerationService Regeneration,
    SeededGeneratedProjectSourceService Source,
    GameProjectFeatureModuleAuthoringService Authoring,
    CurrentGamePackageService Current,
    GameProjectSeedRegenerationRecordService Record);

internal static class Goal159SuccessState
{
    private static readonly Lazy<Goal159SuccessFixture> Fixture = new(Goal159SuccessFixture.Create);
    public static Goal159SuccessFixture Value => Fixture.Value;
}

internal sealed record Goal159SuccessFixture(
    GeneratedProject Project,
    Goal159ControllerBundle Bundle,
    string OldSourceSchema,
    SeededGeneratedProjectSourceValidationResult Source,
    GameProjectSeedRegenerationRequest Request,
    GameProjectSeedRegenerationPreview Preview,
    GameProjectSeedRegenerationResult Result,
    GameProjectSeedRegenerationRecord Record,
    string CandidateRoot,
    SeededGeneratedProjectSourceValidationResult CandidateSource,
    IReadOnlyList<string> OriginalSelectedModuleIds,
    IReadOnlyList<string> CandidateSelectedModuleIds,
    string OriginalParameterJson,
    string CandidateParameterJson,
    string OriginalIdentityJson,
    string CandidateIdentityJson,
    string CandidateReleaseCandidateStatus,
    bool CandidateHadBuilds,
    bool CandidateHadBuildStaging,
    bool CandidateHadRegenerationRoot,
    byte[] OldReleaseCandidateBytes,
    byte[] NewReleaseCandidateBytes,
    SortedDictionary<string, string> OldHistoryHashes,
    SortedDictionary<string, string> NewHistoryHashes)
{
    public static Goal159SuccessFixture Create()
    {
        var project = Goal159TestKit.CreateV1Project(Goal157PortableState.Value.Project, "success");
        var bundle = Goal159TestKit.CreateBundle(project.Path);
        var oldSource = bundle.Source.Validate(project.Path);
        Assert.True(oldSource.Passed, string.Join(Environment.NewLine, oldSource.Diagnostics));
        var original = bundle.Authoring.State;
        var originalSelected = original.Document.SelectedModuleIds.ToList();
        var originalParameters = Goal159TestKit.ParameterJson(original);
        var originalIdentity = JsonSerializer.Serialize(original.Identity);
        var rcPath = new GameProjectReleaseCandidateRecordService().RecordPath(project.Path);
        var oldRc = File.ReadAllBytes(rcPath);
        var historyRoot = Path.Combine(project.Path, UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot);
        var oldHistory = Goal159TestKit.TreeHashes(historyRoot);
        WriteMarker(project.Path, "Builds/stale-clone-marker.txt");
        WriteMarker(project.Path, UnifiedGameProjectWorkspaceVocabulary.BuildStagingRelativeRoot
                                  + "/stale-clone-marker.txt");
        WriteMarker(project.Path, GameProjectSeedRegenerationVocabulary.RegenerationRelativeRoot
                                  + "/stale-clone-marker.txt");
        var request = bundle.Controller.CreateGeneratedWorldRegenerationRequest(
            Goal159TestKit.ChangedRequest(bundle.Controller.Snapshot(), "goal159-regenerated-world"));
        var preview = bundle.Controller.PreviewGeneratedWorldRegeneration(request);
        Assert.True(preview.Status == "GREEN", string.Join(Environment.NewLine, preview.Diagnostics));
        var candidateRoot = preview.CandidateRoot;
        var candidateSource = bundle.Source.Validate(candidateRoot);
        var candidatePackage = Goal156TestKit.Load(candidateRoot);
        var candidateAuthoring = new GameProjectFeatureModuleAuthoringService(Goal156TestKit.RepositoryRoot);
        var candidateState = candidateAuthoring.OpenProject(candidateRoot, candidatePackage);
        var candidateIdentity = JsonSerializer.Serialize(candidateState.Identity);
        var candidateRcStatus = preview.CandidateSnapshot!.ReleaseCandidateRecordConfigurationStatus;
        var candidateHadBuilds = File.Exists(Path.Combine(candidateRoot, "Builds", "stale-clone-marker.txt"));
        var candidateHadStaging = File.Exists(Path.Combine(candidateRoot,
            UnifiedGameProjectWorkspaceVocabulary.BuildStagingRelativeRoot, "stale-clone-marker.txt"));
        var candidateHadRegeneration = File.Exists(Path.Combine(candidateRoot,
            GameProjectSeedRegenerationVocabulary.RegenerationRelativeRoot, "stale-clone-marker.txt"));

        var result = bundle.Controller.ApplyGeneratedWorldRegeneration(request, preview);
        Assert.True(result.Status == "GREEN", string.Join(Environment.NewLine, result.Diagnostics));
        Assert.True(result.Applied);
        var source = bundle.Source.Validate(project.Path);
        var record = bundle.Record.Read(project.Path);
        Assert.True(record.Passed, string.Join(Environment.NewLine, record.Diagnostics));
        var newHistory = Goal159TestKit.TreeHashes(historyRoot);
        return new Goal159SuccessFixture(
            project, bundle, oldSource.Source!.SchemaVersion, source, request, preview, result, record.Record!,
            candidateRoot, candidateSource,
            originalSelected, candidateState.Document.SelectedModuleIds.ToList(),
            originalParameters, Goal159TestKit.ParameterJson(candidateState),
            originalIdentity, candidateIdentity, candidateRcStatus,
            candidateHadBuilds, candidateHadStaging, candidateHadRegeneration,
            oldRc, File.ReadAllBytes(rcPath), oldHistory, newHistory);
    }

    private static void WriteMarker(string project, string relative)
    {
        var path = Path.Combine(project, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "must-not-be-cloned");
    }
}
