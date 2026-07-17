using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161;

[Collection(Goal160Collection.Name)]
public sealed class Goal161GeneratedSaveStoreTests
{
    [Fact]
    public void Behavioral_generated_save_creates_immutable_revision_and_slot_manifest()
    {
        using var fixture = Goal161SaveFixture.Create("immutable-store");
        var result = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(64, result.RevisionSha256.Length);
        Assert.Equal(GeneratedGameplaySaveStatus.CURRENT, result.Status);
        var slot = fixture.Services.Store.ReadSlot(fixture.Project.Path, "slot1");
        Assert.True(slot.Passed, string.Join(Environment.NewLine, slot.Diagnostics));
        Assert.Equal(result.RevisionSha256, slot.Manifest?.CurrentRevisionSha256);
        Assert.Equal(GeneratedGameplaySaveVocabulary.RevisionSchemaVersion,
            slot.CurrentRevision?.SchemaVersion);
        Assert.Equal(GeneratedGameplaySaveVocabulary.SlotSchemaVersion,
            slot.Manifest?.SchemaVersion);
    }

    [Fact]
    public void Behavioral_repeated_identical_save_deduplicates()
    {
        using var fixture = Goal161SaveFixture.Create("deduplicate");
        var first = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        var second = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        Assert.True(first.Passed && second.Passed);
        Assert.Equal(first.RevisionSha256, second.RevisionSha256);
        Assert.True(second.Deduplicated);
        Assert.Single(fixture.Services.Store.ReadSlot(fixture.Project.Path, "slot1").Revisions);
    }

    [Fact]
    public void Behavioral_changed_session_creates_child_revision()
    {
        using var fixture = Goal161SaveFixture.Create("parent");
        var first = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        fixture.Session.GameplayState.Tick++;
        var second = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        Assert.NotEqual(first.RevisionSha256, second.RevisionSha256);
        Assert.Equal(first.RevisionSha256, second.Revision?.ParentRevisionSha256);
        Assert.Equal(2, fixture.Services.Store.ReadSlot(fixture.Project.Path, "slot1").Revisions.Count);
    }

    [Fact]
    public void Behavioral_prior_revision_bytes_remain_unchanged()
    {
        using var fixture = Goal161SaveFixture.Create("immutable-bytes");
        var first = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        var path = RevisionPath(fixture, "slot1", first.RevisionSha256);
        var before = File.ReadAllBytes(path);
        fixture.Session.GameplayState.Tick++;
        Assert.True(fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session).Passed);
        Assert.Equal(before, File.ReadAllBytes(path));
    }

    [Theory]
    [InlineData("")]
    [InlineData("../escape")]
    [InlineData("nested/slot")]
    [InlineData("bad slot")]
    public void Behavioral_invalid_slot_names_are_rejected(string slot)
    {
        using var fixture = Goal161SaveFixture.Create("invalid-slot");
        var result = fixture.Services.Save.Save(fixture.Project.Path, slot, fixture.Session);
        Assert.False(result.Passed);
        Assert.Contains(result.Diagnostics, item => item.Contains("generated_save.slot_invalid", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_revision_filename_hash_mismatch_is_rejected()
    {
        using var fixture = Goal161SaveFixture.Create("filename-hash");
        var saved = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        var fake = new string('0', 64);
        var root = fixture.Services.Store.RootPath(fixture.Project.Path);
        File.Copy(RevisionPath(fixture, "slot1", saved.RevisionSha256),
            Path.Combine(root, "slot1", "revisions", fake + ".json"));
        var read = fixture.Services.Store.ReadRevision(fixture.Project.Path, "slot1", fake);
        Assert.False(read.Passed);
        Assert.Contains("generated_save.revision_hash_mismatch", read.Diagnostics);
    }

    [Fact]
    public void Behavioral_slot_current_pointer_mismatch_is_rejected()
    {
        using var fixture = Goal161SaveFixture.Create("pointer");
        var saved = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        var path = Path.Combine(fixture.Services.Store.RootPath(fixture.Project.Path), "slot1", "slot.json");
        File.WriteAllText(path, File.ReadAllText(path).Replace(saved.RevisionSha256, new string('0', 64),
            StringComparison.Ordinal));
        var read = fixture.Services.Store.ReadSlot(fixture.Project.Path, "slot1");
        Assert.False(read.Passed);
        Assert.Contains("generated_save.revision_missing", read.Diagnostics);
    }

    [Fact]
    public void Behavioral_session_hash_tamper_is_rejected()
    {
        using var fixture = Goal161SaveFixture.Create("session-tamper");
        var saved = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        var path = RevisionPath(fixture, "slot1", saved.RevisionSha256);
        File.WriteAllText(path, File.ReadAllText(path).Replace("unified-v1", "unified-v2",
            StringComparison.Ordinal));
        Assert.False(fixture.Services.Save.Load(fixture.Project.Path, "slot1").Passed);
    }

    [Fact]
    public void Behavioral_definition_fingerprint_tamper_is_rejected()
    {
        using var fixture = Goal161SaveFixture.Create("fingerprint-tamper");
        var saved = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        var mutated = saved.Revision! with
        {
            RevisionSha256 = string.Empty,
            DefinitionFingerprints = saved.Revision.DefinitionFingerprints.Select((item, index) =>
                index == 0 ? item with { CanonicalSha256 = new string('0', 64) } : item).ToList()
        };
        Assert.True(fixture.Services.Store.WriteRevision(fixture.Project.Path, "tampered", mutated).Passed);
        var load = fixture.Services.Save.Load(fixture.Project.Path, "tampered");
        Assert.False(load.Passed);
        Assert.Equal(GeneratedGameplaySaveStatus.INVALID, load.Status);
        Assert.Contains("generated_save.definition_fingerprint_mismatch", load.Diagnostics);
    }

    [Fact]
    public void Behavioral_foreign_project_identity_is_rejected()
    {
        using var fixture = Goal161SaveFixture.Create("foreign");
        var saved = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        var foreign = saved.Revision! with
        {
            RevisionSha256 = string.Empty,
            ProjectIdentityFingerprint = new string('f', 64)
        };
        Assert.True(fixture.Services.Store.WriteRevision(fixture.Project.Path, "foreign", foreign).Passed);
        var load = fixture.Services.Save.Load(fixture.Project.Path, "foreign");
        Assert.False(load.Passed);
        Assert.Contains("generated_save.foreign_project_identity", load.Diagnostics);
    }

    [Fact]
    public void Behavioral_out_of_bounds_current_position_is_rejected()
    {
        using var fixture = Goal161SaveFixture.Create("position");
        fixture.Session.MapState.PlayerPosition.X = int.MaxValue;
        var saved = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        Assert.False(saved.Passed);
        Assert.Contains("generated_save.map_position_invalid", saved.Diagnostics);
    }

    [Fact]
    public void Behavioral_unresolved_package_definition_reference_is_rejected()
    {
        using var fixture = Goal161SaveFixture.Create("unresolved");
        fixture.Session.GameplayState.Inventories[0].Stacks.Add(new ItemStackState
        {
            ItemId = "item/missing", Amount = 1
        });
        var saved = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        Assert.False(saved.Passed);
        Assert.Contains(saved.Diagnostics, item => item.Contains(
            "generated_save.reference_unresolved:item:item/missing", StringComparison.Ordinal));
    }

    private static string RevisionPath(Goal161SaveFixture fixture, string slot, string sha) =>
        Path.Combine(fixture.Services.Store.RootPath(fixture.Project.Path), slot, "revisions", sha + ".json");
}

internal sealed class Goal161SaveFixture : IDisposable
{
    private Goal161SaveFixture(
        GeneratedProject project,
        Goal161ServiceBundle services,
        GamePackageDefinition package,
        UnifiedRuntimeSession session)
    {
        Project = project;
        Services = services;
        Package = package;
        Session = session;
    }

    public GeneratedProject Project { get; }
    public Goal161ServiceBundle Services { get; }
    public GamePackageDefinition Package { get; }
    public UnifiedRuntimeSession Session { get; }

    public static Goal161SaveFixture Create(string suffix, GeneratedProject? source = null)
    {
        var project = Goal156TestKit.Copy(source ?? Goal157BuildState.Value.Project,
            "goal161-" + suffix);
        var services = Goal161ServiceBundle.Create();
        var package = Goal156TestKit.Load(project.Path);
        var session = services.Runtime.Start(package).Session;
        return new Goal161SaveFixture(project, services, package, session);
    }

    public void Dispose() => Project.Dispose();
}

internal sealed record Goal161ServiceBundle(
    GameProjectOperationCoordinator Coordinator,
    GeneratedGameplayDefinitionFingerprintService Fingerprints,
    GeneratedGameplaySaveStore Store,
    GeneratedGameplaySaveValidator Validator,
    GeneratedGameplaySaveService Save,
    GeneratedGameplaySaveMigrationService Migration,
    GeneratedGameplaySavesSummaryService Summary,
    SeededGeneratedProjectSourceService Source,
    RuntimeStateSerializer Serializer,
    RuntimeSnapshotStore Legacy,
    IGameRuntimeService GameplayRuntime,
    IUnifiedGameRuntimeService Runtime)
{
    public static Goal161ServiceBundle Create(
        GameProjectOperationCoordinator? coordinator = null,
        SeededGeneratedProjectSourceService? source = null,
        GeneratedWorldHistoryService? history = null)
    {
        coordinator ??= new GameProjectOperationCoordinator();
        var fingerprints = new GeneratedGameplayDefinitionFingerprintService();
        var store = new GeneratedGameplaySaveStore();
        var serializer = new RuntimeStateSerializer();
        source ??= Goal156TestKit.SourceService;
        history ??= new GeneratedWorldHistoryService(source);
        var validator = new GeneratedGameplaySaveValidator(
            Goal156TestKit.RepositoryRoot,
            Goal156TestKit.Repository,
            Goal156TestKit.Validator,
            source,
            history,
            fingerprints,
            serializer,
            coordinator);
        var legacy = new RuntimeSnapshotStore(serializer);
        var gameplayRuntime = CreateGameplayRuntime();
        var save = new GeneratedGameplaySaveService(
            coordinator, validator, store, serializer, legacy, source);
        var migration = new GeneratedGameplaySaveMigrationService(
            coordinator, validator, store, fingerprints, serializer);
        var summary = new GeneratedGameplaySavesSummaryService(save);
        return new Goal161ServiceBundle(coordinator, fingerprints, store, validator, save, migration, summary, source,
            serializer, legacy, gameplayRuntime,
            new UnifiedGameRuntimeService(new DefaultGameRuntime(), gameplayRuntime));
    }

    private static IGameRuntimeService CreateGameplayRuntime()
    {
        var requirements = new RequirementEvaluator();
        var costs = new CostConsumer();
        var outputs = new OutputApplier();
        var recipes = new RecipeRuntimeService(requirements, costs, outputs);
        var transactions = new TransactionRuntimeService(requirements, costs, outputs);
        return new GameRuntimeService(
            new GameRuntimeStateFactory(),
            recipes,
            new LootRuntimeService(requirements, outputs),
            transactions,
            new ResourceNetworkRuntimeService(requirements, costs, outputs),
            new UseItemRuntimeService(requirements, outputs),
            new InteractionRuntimeService(requirements, outputs, recipes, transactions));
    }
}

internal sealed record Goal161WorldBundle(
    UnifiedGameProjectWorkspaceController Controller,
    Goal161ServiceBundle Saves,
    GameProjectSeedRegenerationService Regeneration,
    GameProjectGeneratedWorldRollbackService Rollback,
    SeededGeneratedProjectSourceService Source,
    GeneratedWorldHistoryService History,
    GameProjectOperationCoordinator Coordinator,
    CurrentGamePackageService Current)
{
    public static Goal161WorldBundle Create(string project)
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
        var history = new GeneratedWorldHistoryService(source);
        var saves = Goal161ServiceBundle.Create(coordinator, source, history);
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
            operationCoordinator: coordinator,
            generatedCombatRuntime: saves.Runtime);
        var transaction = new GameProjectSeedRegenerationTransaction();
        var record = new GameProjectSeedRegenerationRecordService(repositoryRoot, source);
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
        var controller = new UnifiedGameProjectWorkspaceController(
            current, authoring, builder,
            standaloneBuild: new ProjectStandaloneBuildService(repositoryRoot),
            generatedSourceService: source, generatedWorldSummaryService: summary,
            regenerationService: regeneration, operationCoordinator: coordinator,
            worldRollbackService: rollback, generatedGameplaySaveService: saves.Save,
            generatedGameplaySaveMigrationService: saves.Migration,
            generatedGameplaySavesSummaryService: saves.Summary);
        controller.OpenProject(project);
        return new Goal161WorldBundle(controller, saves, regeneration, rollback, source, history, coordinator, current);
    }
}
