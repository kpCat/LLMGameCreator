using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal161;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal164;

[Collection(Goal160Collection.Name)]
public sealed class Goal164CombatContractResolutionTests
{
    [Fact]
    public void Behavioral_real_generated_build_is_campaign_current()
    {
        var fixture = Goal164TestKit.AllSelectable;

        Assert.True(fixture.Build.Passed, string.Join(",", fixture.Build.Diagnostics));
        Assert.Equal("CAMPAIGN_CURRENT", fixture.Build.GeneratedEncounterCombat?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", fixture.Snapshot.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_contract_is_resolved_only_from_lane_a()
    {
        var fixture = Goal164TestKit.AllSelectable;
        var contract = Assert.IsType<GeneratedEncounterCombatContract>(fixture.Contract.Contract);

        Assert.True(fixture.Contract.Passed, string.Join(",", fixture.Contract.Diagnostics));
        Assert.DoesNotContain(fixture.Source.Overlay!.GeneratedRecords
                .Where(item => item.CollectionPath == "game.encounters").Select(item => item.RecordId),
            id => id == contract.SourceEncounterId);
    }

    [Fact]
    public void Behavioral_contract_resolution_is_deterministic()
    {
        var fixture = Goal164TestKit.AllSelectable;
        var repeated = new GeneratedEncounterCombatContractService().Resolve(
            fixture.LaneAPackage, fixture.Source.Overlay!, fixture.Runtime);

        Assert.True(repeated.Passed, string.Join(",", repeated.Diagnostics));
        Assert.Equal(fixture.Contract.Contract?.ContractId, repeated.Contract?.ContractId);
    }

    [Fact]
    public void Behavioral_contract_player_route_uses_actual_runtime()
    {
        var summary = Assert.IsType<GeneratedEncounterCombatContract>(
            Goal164TestKit.AllSelectable.Contract.Contract).QualificationSummary;

        Assert.True(summary.StartEncounterPassed);
        Assert.True(summary.PlayerRoutePassed);
        Assert.True(summary.BasicAttackPassed || summary.PackageAbilityPassed);
    }

    [Fact]
    public void Behavioral_contract_opponent_route_uses_actual_ai()
    {
        var summary = Assert.IsType<GeneratedEncounterCombatContract>(
            Goal164TestKit.AllSelectable.Contract.Contract).QualificationSummary;

        Assert.True(summary.OpponentAiPassed);
        Assert.True(summary.OpponentEffectObserved);
        Assert.True(summary.ControlReturnedOrEncounterTerminated);
    }

    [Fact]
    public void Behavioral_contract_preserves_exact_lane_a_sha()
    {
        var fixture = Goal164TestKit.AllSelectable;
        var contract = Assert.IsType<GeneratedEncounterCombatContract>(fixture.Contract.Contract);

        Assert.True(contract.QualificationSummary.PackageShaUnchanged);
        Assert.Equal(Goal164TestKit.CanonicalPackageSha(fixture.LaneAPackage),
            contract.SourcePackageSha256);
    }

    [Fact]
    public void Behavioral_contract_has_exact_definition_fingerprints()
    {
        var contract = Assert.IsType<GeneratedEncounterCombatContract>(
            Goal164TestKit.AllSelectable.Contract.Contract);

        Assert.NotEmpty(contract.ExactDefinitionFingerprints);
        Assert.All(contract.ExactDefinitionFingerprints, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.DefinitionId));
            Assert.Equal(64, item.CanonicalSha256.Length);
        });
    }

    [Fact]
    public void Behavioral_contract_uses_package_derived_health_and_damage_values()
    {
        var contract = Assert.IsType<GeneratedEncounterCombatContract>(
            Goal164TestKit.AllSelectable.Contract.Contract);
        var source = Goal164TestKit.AllSelectable.LaneAPackage.Game.Encounters
            .Single(item => item.Id == contract.SourceEncounterId);

        Assert.Equal(source.Participants.Single(item => item.Id == contract.PlayerRole.SourceParticipantId).Resources,
            contract.PlayerRole.Resources, Goal164TestKit.CanonicalComparer<LLMGameCreator.Domain.Definitions.OutputDefinition>());
        Assert.Equal(source.Participants.Single(item => item.Id == contract.OpponentRole.SourceParticipantId).Abilities,
            contract.OpponentRole.Abilities);
    }
}

internal static class Goal164TestKit
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };
    private static readonly Lazy<Goal164BuildFixture> AllSelectableFixture =
        new(() => Goal164BuildFixture.Create(coreOnly: false));
    private static readonly Lazy<Goal164BuildFixture> CoreOnlyFixture =
        new(() => Goal164BuildFixture.Create(coreOnly: true));

    public static Goal164BuildFixture AllSelectable => AllSelectableFixture.Value;
    public static Goal164BuildFixture CoreOnly => CoreOnlyFixture.Value;
    public static string RepositoryRoot => Goal156TestKit.RepositoryRoot;

    public static string CanonicalPackageSha(GamePackageDefinition package) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(package, JsonOptions)))).ToLowerInvariant();

    public static IEqualityComparer<T> CanonicalComparer<T>() => new CanonicalJsonComparer<T>();
    public static string Canonical<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    public static string FileSha(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    public static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(
        JsonSerializer.Serialize(value, JsonOptions), JsonOptions)!;

    private sealed class CanonicalJsonComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y) => JsonSerializer.Serialize(x, JsonOptions)
                                         == JsonSerializer.Serialize(y, JsonOptions);
        public int GetHashCode(T obj) => JsonSerializer.Serialize(obj, JsonOptions).GetHashCode();
    }
}

internal sealed record Goal164BuildFixture(
    GeneratedProject Project,
    CurrentGamePackageService Current,
    Goal161ServiceBundle Saves,
    SeededGeneratedProjectSourceValidationResult Source,
    GamePackageDefinition LaneAPackage,
    GeneratedEncounterCombatContractResult Contract,
    GameProjectBuildResult Build,
    UnifiedGameProjectWorkspaceSnapshot Snapshot,
    GamePackageDefinition Package,
    IUnifiedGameRuntimeService Runtime,
    IReadOnlyDictionary<string, string> GenerationSidecarHashesBefore,
    UnifiedGameProjectWorkspaceController Controller,
    GameProjectSeedRegenerationService Regeneration,
    GameProjectGeneratedWorldRollbackService Rollback,
    GeneratedWorldHistoryService WorldHistory,
    SeededGeneratedProjectSourceService SourceService)
{
    public static Goal164BuildFixture Create(
        bool coreOnly,
        IProjectStandaloneBuildService? standalone = null)
    {
        var sourceProject = coreOnly ? Goal156TestKit.CoreOnly : Goal156TestKit.AllSelectable;
        var project = Goal156TestKit.Copy(sourceProject,
            coreOnly ? "goal164-core-only" : "goal164-all-selectable");
        var repository = new JsonGamePackageRepository();
        var validator = new GamePackageValidator();
        var current = new CurrentGamePackageService(repository);
        current.LoadAsync(project.Path, CancellationToken.None).GetAwaiter().GetResult();
        var coordinator = new GameProjectOperationCoordinator();
        var baseline = new Goal142GeneratedProjectBaselineProvider(Goal156TestKit.RepositoryRoot);
        var overlay = new GeneratedProjectOverlayService(validator);
        var source = new SeededGeneratedProjectSourceService(
            validator, overlayService: overlay, baselineProvider: baseline);
        var sourceResult = source.Validate(project.Path);
        var generationRoot = Path.Combine(project.Path, SeededGeneratedProjectVocabulary.GenerationRelativeRoot
            .Replace('/', Path.DirectorySeparatorChar));
        var sidecarHashes = SeededGeneratedProjectVocabulary.RequiredSidecarFileNames
            .Append(SeededGeneratedProjectVocabulary.SourceJsonFileName)
            .ToDictionary(name => name, name => Goal164TestKit.FileSha(Path.Combine(generationRoot, name)),
                StringComparer.Ordinal);
        var laneA = Goal164TestKit.Clone(current.CurrentPackage
                                        ?? throw new InvalidOperationException("goal164.package_missing"));
        var history = new GeneratedWorldHistoryService(source);
        var saves = Goal161ServiceBundle.Create(coordinator, source, history);
        var contract = new GeneratedEncounterCombatContractService().Resolve(
            laneA, sourceResult.Overlay ?? throw new InvalidOperationException("goal164.overlay_missing"),
            saves.Runtime);
        var summary = new GameProjectGeneratedWorldSummaryService(overlay);
        var mapRuntime = new DefaultGameRuntime();
        var serializer = new RuntimeStateSerializer();
        var builder = new GameProjectBuildAndQualificationService(
            Goal156TestKit.RepositoryRoot,
            SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
            repository,
            validator,
            current,
            generatedSource: source,
            generatedSummary: summary,
            generatedActivation: new GameProjectGeneratedWorldActivationService(
                mapRuntime, serializer, validator),
            generatedTravelOverlay: new GeneratedWorldTravelOverlayService(),
            generatedTravelActivation: new GameProjectGeneratedRegionTravelActivationService(
                mapRuntime, serializer),
            operationCoordinator: coordinator,
            generatedCombatRuntime: saves.Runtime);
        var transaction = new GameProjectSeedRegenerationTransaction();
        var record = new GameProjectSeedRegenerationRecordService(
            Goal156TestKit.RepositoryRoot, source);
        var worldChange = new GameProjectGeneratedWorldChangeRecordService(source, history);
        var truth = new GameProjectSeedRegenerationTruthReader(Goal156TestKit.RepositoryRoot, source);
        var commit = new GameProjectSeedRegenerationCommitValidator(
            Goal156TestKit.RepositoryRoot, source, validator, record);
        var seal = new GameProjectSeedRegenerationCandidateSealService();
        var regeneration = new GameProjectSeedRegenerationService(
            Goal156TestKit.RepositoryRoot, current, repository, validator, builder,
            new SeededGeneratedProjectArtifactFactory(baseline, validator, overlay: overlay), source,
            transaction: transaction, recordService: record, operationCoordinator: coordinator,
            sealService: seal, truthReader: truth, commitValidator: commit,
            worldHistoryService: history, worldChangeRecordService: worldChange);
        var rollback = new GameProjectGeneratedWorldRollbackService(
            Goal156TestKit.RepositoryRoot, current, repository, validator, builder, source, history, worldChange,
            transaction: transaction, operationCoordinator: coordinator, sealService: seal,
            truthReader: truth, commitValidator: commit, regenerationRecordService: record);
        var controller = new UnifiedGameProjectWorkspaceController(
            current,
            new GameProjectFeatureModuleAuthoringService(Goal156TestKit.RepositoryRoot,
                operationCoordinator: coordinator),
            builder,
            standaloneBuild: standalone,
            generatedSourceService: source,
            generatedWorldSummaryService: summary,
            regenerationService: regeneration,
            operationCoordinator: coordinator,
            worldRollbackService: rollback,
            generatedGameplaySaveService: saves.Save,
            generatedGameplaySaveMigrationService: saves.Migration,
            generatedGameplaySavesSummaryService: saves.Summary);
        controller.OpenProject(project.Path);
        var build = controller.BuildAndQualify();
        var snapshot = controller.Snapshot();
        var package = current.CurrentPackage
                      ?? throw new InvalidOperationException("goal164.current_package_missing");
        return new Goal164BuildFixture(project, current, saves, sourceResult, laneA, contract,
            build, snapshot, package, saves.Runtime, sidecarHashes, controller, regeneration,
            rollback, history, source);
    }
}
