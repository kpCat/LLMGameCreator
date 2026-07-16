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
using LLMGameCreator.Tests.Application.Goal159;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal160;

[Collection(Goal160Collection.Name)]
public sealed class Goal160WorldRollbackCandidateTests
{
    [Fact]
    public void Behavioral_target_current_world_is_rejected_as_no_op()
    {
        var state = Goal160RollbackState.Value;
        var error = Assert.Throws<InvalidOperationException>(() =>
            state.Bundle.Controller.CreateGeneratedWorldRollbackRequest(state.FinalHistory.CurrentWorldId));
        Assert.Equal("world_rollback.no_semantic_change", error.Message);
    }

    [Fact]
    public void Behavioral_missing_target_is_rejected()
    {
        var state = Goal160RollbackState.Value;
        var error = Assert.Throws<InvalidOperationException>(() =>
            state.Bundle.Controller.CreateGeneratedWorldRollbackRequest(new string('0', 64)));
        Assert.Contains("world_history.target_missing", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_candidate_uses_historical_generation_with_current_authoring()
    {
        var state = Goal160RollbackState.Value;
        Assert.Equal(state.TargetWorldId, state.Preview.TargetWorldId);
        Assert.Equal(state.OriginalSelectedModuleIds, state.CandidateSelectedModuleIds);
        Assert.Equal(state.OriginalParameterJson, state.CandidateParameterJson);
    }

    [Fact]
    public void Behavioral_candidate_preserves_current_identity()
    {
        var state = Goal160RollbackState.Value;
        Assert.Equal(state.OriginalIdentityJson, state.CandidateIdentityJson);
    }

    [Fact]
    public void Behavioral_candidate_build_repeat_and_fresh_reopen_are_travel_current()
    {
        var state = Goal160RollbackState.Value;
        Assert.Equal("GREEN", state.Preview.Status);
        Assert.True(state.Preview.CandidateBuild?.Passed);
        Assert.Equal("TRAVEL_CURRENT", state.Preview.CandidateSnapshot?.GeneratedWorld?.Status);
        Assert.True(state.Preview.CandidateSnapshot?.GeneratedRegionTravel?.Passed);
        Assert.True(state.Preview.CandidateSnapshot?.AcceptedMechanicsCompatibility?.Passed);
    }

    [Fact]
    public void Behavioral_candidate_release_candidate_is_not_current()
    {
        Assert.NotEqual("CURRENT",
            Goal160RollbackState.Value.Preview.CandidateSnapshot?.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_current_to_target_diff_is_data_derived()
    {
        var diff = Goal160RollbackState.Value.Preview.Diff!;
        Assert.True(diff.GameplayChanged);
        Assert.NotEqual(diff.OldSeed, diff.NewSeed);
        Assert.NotEqual(diff.OldGeneratedBaseSha256, diff.NewGeneratedBaseSha256);
        Assert.True(diff.AuthoringPreserved);
        Assert.True(diff.ProjectIdentityPreserved);
    }

    [Fact]
    public void Behavioral_preview_publishes_sealed_candidate()
    {
        var preview = Goal160RollbackState.Value.Preview;
        Assert.Equal(64, preview.CandidateSealSha256.Length);
        Assert.Equal("candidate_sealed", preview.Stage);
    }
}

internal static class Goal160RollbackState
{
    private static readonly Lazy<Goal160RollbackFixture> Fixture = new(Goal160RollbackFixture.Create);
    public static Goal160RollbackFixture Value => Fixture.Value;
}

internal sealed record Goal160Bundle(
    UnifiedGameProjectWorkspaceController Controller,
    GameProjectGeneratedWorldRollbackService Rollback,
    GameProjectSeedRegenerationService Regeneration,
    SeededGeneratedProjectSourceService Source,
    GameProjectFeatureModuleAuthoringService Authoring,
    CurrentGamePackageService Current,
    GeneratedWorldHistoryService History,
    GameProjectGeneratedWorldChangeRecordService WorldChange,
    ProjectStandaloneBuildService Standalone);

internal sealed record Goal160RollbackFixture(
    GeneratedProject Project,
    Goal160Bundle Bundle,
    string TargetWorldId,
    GameProjectGeneratedWorldRollbackRequest Request,
    GameProjectGeneratedWorldRollbackPreview Preview,
    GameProjectGeneratedWorldRollbackResult Result,
    GeneratedWorldHistoryReadResult InitialHistory,
    GeneratedWorldHistoryReadResult FinalHistory,
    IReadOnlyList<string> OriginalSelectedModuleIds,
    IReadOnlyList<string> CandidateSelectedModuleIds,
    string OriginalParameterJson,
    string CandidateParameterJson,
    string OriginalIdentityJson,
    string CandidateIdentityJson,
    byte[] OldReleaseCandidateBytes,
    byte[] NewReleaseCandidateBytes,
    int InitialBuildHistoryCount,
    int FinalBuildHistoryCount,
    GameProjectGeneratedWorldChangeRecord WorldChangeRecord)
{
    public static Goal160RollbackFixture Create()
    {
        var project = Goal156TestKit.Copy(Goal159SuccessState.Value.Project, "goal160-rollback");
        var bundle = CreateBundle(project.Path);
        var initial = bundle.Controller.ReadGeneratedWorldHistory();
        Assert.True(initial.Passed, string.Join(Environment.NewLine, initial.Diagnostics));
        var target = initial.Entries.Single(entry => !entry.IsCurrent).WorldId;
        var state = bundle.Authoring.State;
        var selected = state.Document.SelectedModuleIds.ToList();
        var parameters = ParameterJson(state);
        var identity = JsonSerializer.Serialize(state.Identity);
        var rcPath = new GameProjectReleaseCandidateRecordService().RecordPath(project.Path);
        var rc = File.ReadAllBytes(rcPath);
        var historyRoot = Path.Combine(project.Path, UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot);
        var buildCount = Directory.EnumerateFiles(historyRoot, "*.json").Count();
        var request = bundle.Controller.CreateGeneratedWorldRollbackRequest(target);
        var preview = bundle.Controller.PreviewGeneratedWorldRollback(request);
        Assert.True(preview.Status == "GREEN", string.Join(Environment.NewLine, preview.Diagnostics));
        var candidatePackage = Goal156TestKit.Load(preview.CandidateRoot);
        var candidateAuthoring = new GameProjectFeatureModuleAuthoringService(Goal156TestKit.RepositoryRoot);
        var candidateState = candidateAuthoring.OpenProject(preview.CandidateRoot, candidatePackage);
        var candidateSelected = candidateState.Document.SelectedModuleIds.ToList();
        var candidateParameters = ParameterJson(candidateState);
        var candidateIdentity = JsonSerializer.Serialize(candidateState.Identity);
        var result = bundle.Controller.ApplyGeneratedWorldRollback(request, preview);
        Assert.True(result.Status == "GREEN", string.Join(Environment.NewLine, result.Diagnostics));
        Assert.True(result.Applied);
        var final = bundle.Controller.ReadGeneratedWorldHistory();
        var change = bundle.WorldChange.Read(project.Path);
        Assert.True(change.Passed, string.Join(Environment.NewLine, change.Diagnostics));
        return new Goal160RollbackFixture(
            project, bundle, target, request, preview, result, initial, final,
            selected, candidateSelected, parameters, candidateParameters, identity, candidateIdentity,
            rc, File.ReadAllBytes(rcPath), buildCount,
            Directory.EnumerateFiles(historyRoot, "*.json").Count(), change.Record!);
    }

    internal static Goal160Bundle CreateBundle(string project)
    {
        var repositoryRoot = Goal156TestKit.RepositoryRoot;
        var repository = new JsonGamePackageRepository();
        var validator = new GamePackageValidator();
        var current = new CurrentGamePackageService(repository);
        current.LoadAsync(project, CancellationToken.None).GetAwaiter().GetResult();
        var coordinator = new GameProjectOperationCoordinator();
        var baseline = new Goal142GeneratedProjectBaselineProvider(repositoryRoot);
        var overlay = new GeneratedProjectOverlayService(validator);
        var source = new SeededGeneratedProjectSourceService(
            validator, overlayService: overlay, baselineProvider: baseline);
        var summary = new GameProjectGeneratedWorldSummaryService(overlay);
        var runtime = new DefaultGameRuntime();
        var serializer = new RuntimeStateSerializer();
        var builder = new GameProjectBuildAndQualificationService(
            repositoryRoot,
            SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
            repository,
            validator,
            current,
            generatedSource: source,
            generatedSummary: summary,
            generatedActivation: new GameProjectGeneratedWorldActivationService(runtime, serializer, validator),
            generatedTravelOverlay: new GeneratedWorldTravelOverlayService(),
            generatedTravelActivation: new GameProjectGeneratedRegionTravelActivationService(runtime, serializer),
            operationCoordinator: coordinator);
        var transaction = new GameProjectSeedRegenerationTransaction();
        var record = new GameProjectSeedRegenerationRecordService(repositoryRoot, source);
        var history = new GeneratedWorldHistoryService(source);
        var worldChange = new GameProjectGeneratedWorldChangeRecordService(source, history);
        var truth = new GameProjectSeedRegenerationTruthReader(repositoryRoot, source);
        var commit = new GameProjectSeedRegenerationCommitValidator(repositoryRoot, source, validator, record);
        var seal = new GameProjectSeedRegenerationCandidateSealService();
        var regeneration = new GameProjectSeedRegenerationService(
            repositoryRoot, current, repository, validator, builder,
            new SeededGeneratedProjectArtifactFactory(baseline, validator, overlay: overlay), source,
            transaction: transaction, recordService: record, operationCoordinator: coordinator,
            sealService: seal, truthReader: truth, commitValidator: commit,
            worldHistoryService: history, worldChangeRecordService: worldChange);
        var rollback = new GameProjectGeneratedWorldRollbackService(
            repositoryRoot, current, repository, validator, builder, source, history, worldChange,
            transaction: transaction, operationCoordinator: coordinator, sealService: seal,
            truthReader: truth, commitValidator: commit, regenerationRecordService: record);
        var authoring = new GameProjectFeatureModuleAuthoringService(repositoryRoot,
            operationCoordinator: coordinator);
        var standalone = new ProjectStandaloneBuildService(repositoryRoot);
        var controller = new UnifiedGameProjectWorkspaceController(
            current, authoring, builder, standaloneBuild: standalone,
            generatedSourceService: source, generatedWorldSummaryService: summary,
            regenerationService: regeneration, operationCoordinator: coordinator,
            worldRollbackService: rollback);
        controller.OpenProject(project);
        return new Goal160Bundle(controller, rollback, regeneration, source, authoring, current,
            history, worldChange, standalone);
    }

    internal static string ParameterJson(GameProjectAuthoringState state) => JsonSerializer.Serialize(
        state.Document.ParameterValues.OrderBy(value => value.ModuleId, StringComparer.Ordinal)
            .ThenBy(value => value.ParameterId, StringComparer.Ordinal));
}
