using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Generation;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanStrictLlmArtifactGenerationServiceTests
{
    [Fact]
    public async Task GenerateRefusesMissingLlmProfile()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        var service = CreateService(database, new InMemorySettingsRepository(new AppSettings()), new FakeLlmChatClient());

        var result = await service.GenerateAsync(Request("game_profile_v1"), CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingLlmProfile);
    }

    [Fact]
    public async Task GenerateRefusesMissingCapabilitySelection()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        var service = CreateService(database, new InMemorySettingsRepository(Settings()), new FakeLlmChatClient(ValidGameProfile()));

        var result = await service.GenerateAsync(Request("game_profile_v1"), CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingCapabilitySelection);
    }

    [Fact]
    public async Task GenerateValidGameProfileStagesPendingReviewArtifact()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        var service = CreateService(database, new InMemorySettingsRepository(Settings()), new FakeLlmChatClient(ValidGameProfile()));

        var result = await service.GenerateAsync(Request("game_profile_v1"), CancellationToken.None);
        var review = await new GeneratorPlanDraftArtifactApprovalArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.True(review.Exists);
        Assert.Single(review.Worklist);
        Assert.Equal(GeneratorPlanDraftArtifactApprovalItemState.Pending, review.Worklist[0].State);
        Assert.Contains("\"approved_artifacts\": []", review.ApprovedArtifactSetArtifact?.Json);
    }

    [Fact]
    public async Task GenerateInvalidThenRepairValidStagesRepairedArtifact()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        var service = CreateService(database, new InMemorySettingsRepository(Settings()), new FakeLlmChatClient(InvalidGameProfile(), ValidGameProfile()));

        var result = await service.GenerateAsync(Request("game_profile_v1"), CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.Contains(result.Artifacts, artifact => artifact.Valid && artifact.Repaired);
        Assert.Contains(result.Attempts, attempt => attempt.IsRepairAttempt && attempt.ValidationOk);
    }

    [Fact]
    public async Task GenerateInvalidAfterRepairDoesNotStageApprovedArtifact()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        var service = CreateService(database, new InMemorySettingsRepository(Settings()), new FakeLlmChatClient(InvalidGameProfile(), InvalidGameProfile()));

        var result = await service.GenerateAsync(Request("game_profile_v1"), CancellationToken.None);
        var review = await new GeneratorPlanDraftArtifactApprovalArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.False(result.Ok);
        Assert.False(review.Exists);
    }

    [Fact]
    public async Task GenerateMultipleContractsCreatesMultiplePendingReviewItems()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        var service = CreateService(database, new InMemorySettingsRepository(Settings()), new FakeLlmChatClient(ValidGameProfile(), ValidScenePack()));

        var result = await service.GenerateAsync(Request("scene_pack_v1", "game_profile_v1"), CancellationToken.None);
        var review = await new GeneratorPlanDraftArtifactApprovalArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.Equal(2, review.Worklist.Count);
        Assert.All(review.Worklist, item => Assert.Equal(GeneratorPlanDraftArtifactApprovalItemState.Pending, item.State));
    }

    [Fact]
    public async Task AuditArtifactSavesDiagnosticsAndAttempts()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        var service = CreateService(database, new InMemorySettingsRepository(Settings()), new FakeLlmChatClient(InvalidGameProfile()));

        await service.GenerateAsync(Request("game_profile_v1") with { EnableRepairAttempt = false }, CancellationToken.None);
        var audit = await new GeneratorPlanStrictLlmArtifactGenerationArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.True(audit.Exists);
        Assert.NotEmpty(audit.Result.Attempts);
        Assert.NotEmpty(audit.ValidationResults);
        Assert.Contains(audit.Result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.EmptyRequiredArray);
    }

    private static GeneratorPlanStrictLlmArtifactGenerationService CreateService(
        SqliteDesignDatabase database,
        IAppSettingsRepository settingsRepository,
        ILlmChatClient llmChatClient)
    {
        var catalog = new GeneratorPlanStrictLlmArtifactContractCatalog();
        return new GeneratorPlanStrictLlmArtifactGenerationService(
            settingsRepository,
            llmChatClient,
            new GeneratorPlanCapabilitySelectionArtifactReader(database),
            catalog,
            new GeneratorPlanStrictLlmArtifactPromptBuilder(),
            new GeneratorPlanStrictJsonResponseParser(),
            new GeneratorPlanStrictLlmArtifactValidator(),
            new GeneratorPlanStrictLlmArtifactRepairPromptBuilder(),
            new GeneratorPlanDraftArtifactApprovalService(),
            new GeneratorPlanDraftArtifactApprovalArtifactService(new GeneratorPlanDraftArtifactApprovalService(), database),
            new GeneratorPlanStrictLlmArtifactGenerationArtifactService(database));
    }

    private static GeneratorPlanStrictLlmArtifactGenerationRequest Request(params string[] contracts)
    {
        return new GeneratorPlanStrictLlmArtifactGenerationRequest
        {
            LlmProfileId = "local",
            ContractIds = contracts,
            StageForReview = true,
            EnableRepairAttempt = true
        };
    }

    private static AppSettings Settings()
    {
        return new AppSettings
        {
            DefaultLlmProfileId = "local",
            LlmProfiles =
            [
                new LlmEndpointSettings
                {
                    Id = "local",
                    Title = "Local",
                    Endpoint = "http://localhost:1234/v1",
                    Model = "fake"
                }
            ]
        };
    }

    private static async Task SaveSelectionAsync(SqliteDesignDatabase database)
    {
        await new GeneratorPlanCapabilitySelectionArtifactService(database).SaveAsync(new GeneratorPlanCapabilitySelectionResult
        {
            Ok = true,
            Status = GeneratorPlanCapabilitySelectionStatus.Ready,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            Selection = new GeneratorPlanCapabilitySelection
            {
                SelectionId = "selection/test",
                Title = "Test",
                Purpose = "Generate test artifacts.",
                SelectedVariantIds = new GeneratorPlanCapabilitySelectedVariantIds
                {
                    PresentationModeId = "presentation_mode/top_down_2d",
                    WorldTopologyId = "world_topology/single_map",
                    ActorModelId = "actor_model/single_player_character",
                    InventoryModelId = "inventory_model/list_inventory",
                    CombatModelId = "combat_model/turn_based",
                    ProgressionModelId = "progression_model/level_xp",
                    PathfindingProfileId = "pathfinding/grid_4way",
                    NpcBehaviorModelId = "npc_behavior/static"
                },
                SelectedFeatureBundleIds = ["feature_bundle/core/v1"],
                ResolvedArtifactContracts = ["game_profile_v1", "scene_pack_v1", "quest_pack_v1", "mechanics_pack_v1"],
                ResolvedValidators = ["strict_json"],
                ResolvedRuntimeTargets = ["headless"]
            }
        }, CancellationToken.None);
    }

    private static async Task<SqliteDesignDatabase> CreateDatabaseAsync(string root)
    {
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(root, ".llmgc", "design.db"), CancellationToken.None);
        return database;
    }

    private static string ValidGameProfile()
    {
        return """
        {
          "schema_version": "0.1",
          "artifact_kind": "game_profile_v1",
          "game": {
            "title": "Test",
            "description": "Test game.",
            "genre": "RPG",
            "tone": "Bright",
            "presentation_mode": "presentation_mode/top_down_2d",
            "world_topology": "world_topology/single_map",
            "actor_model": "actor_model/single_player_character",
            "combat_model": "combat_model/turn_based",
            "core_loop": ["explore"]
          },
          "pillars": ["clear goals"],
          "source_context": { "capability_selection_id": "selection/test", "selected_variant_ids": {} }
        }
        """;
    }

    private static string InvalidGameProfile()
    {
        return """
        {
          "schema_version": "0.1",
          "artifact_kind": "game_profile_v1",
          "game": {
            "title": "Test",
            "description": "Test game.",
            "genre": "RPG",
            "tone": "Bright",
            "presentation_mode": "presentation_mode/top_down_2d",
            "world_topology": "world_topology/single_map",
            "actor_model": "actor_model/single_player_character",
            "combat_model": "combat_model/turn_based",
            "core_loop": []
          },
          "pillars": ["clear goals"],
          "source_context": { "capability_selection_id": "selection/test", "selected_variant_ids": {} }
        }
        """;
    }

    private static string ValidScenePack()
    {
        return """
        {
          "schema_version": "0.1",
          "artifact_kind": "scene_pack_v1",
          "scenes": [{ "id": "scene/start", "title": "Start", "description": "Start scene.", "purpose": "Intro." }],
          "source_context": {}
        }
        """;
    }

    private sealed class FakeLlmChatClient : ILlmChatClient
    {
        private readonly Queue<string> _responses;

        public FakeLlmChatClient(params string[] responses)
        {
            _responses = new Queue<string>(responses);
        }

        public Task<LlmChatResponse> CompleteAsync(LlmEndpointSettings profile, LlmChatRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new LlmChatResponse
            {
                Content = _responses.Count == 0 ? "{}" : _responses.Dequeue(),
                Endpoint = profile.Endpoint,
                Model = profile.Model
            });
        }
    }

    private sealed class InMemorySettingsRepository : IAppSettingsRepository
    {
        private readonly AppSettings _settings;

        public InMemorySettingsRepository(AppSettings settings)
        {
            _settings = settings;
        }

        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(_settings);
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
