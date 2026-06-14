using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Generation;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanStrictLlmEvaluationServiceTests
{
    [Fact]
    public async Task EvaluateLatestAuditReturnsMissingWhenNoAudit()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        var service = CreateEvaluationService(database, new FakeLlmChatClient());

        var result = await service.EvaluateLatestAuditAsync(CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmEvaluationDiagnosticCodes.MissingAudit);
    }

    [Fact]
    public async Task EvaluateLatestAuditComputesPassRepairFailMetrics()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        await SaveAuditAsync(database, AuditResult());
        var service = CreateEvaluationService(database, new FakeLlmChatClient());

        var result = await service.EvaluateLatestAuditAsync(CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Equal(3, result.Summary.TotalGenerationRuns);
        Assert.Equal(1, result.Summary.InitialPassCount);
        Assert.Equal(1, result.Summary.RepairPassCount);
        Assert.Equal(1, result.Summary.FailedCount);
        Assert.Equal(2, result.Summary.ValidArtifactCount);
    }

    [Fact]
    public async Task EvaluateLatestAuditGroupsDiagnosticHotspots()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        await SaveAuditAsync(database, AuditResult());
        var service = CreateEvaluationService(database, new FakeLlmChatClient());

        var result = await service.EvaluateLatestAuditAsync(CancellationToken.None);

        Assert.Contains(result.DiagnosticSummaries, diagnostic =>
            diagnostic.Code == GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid
            && diagnostic.Count == 2);
    }

    [Fact]
    public async Task EvaluateLatestAuditAddsQualityWarningsForGenericOrShortContent()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        await SaveAuditAsync(database, AuditResult());
        var service = CreateEvaluationService(database, new FakeLlmChatClient());

        var result = await service.EvaluateLatestAuditAsync(CancellationToken.None);

        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmEvaluationDiagnosticCodes.GenericTextWarning);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanStrictLlmEvaluationDiagnosticCodes.ShortDescriptionWarning);
    }

    [Fact]
    public async Task RunBatchUsesFakeLlmAndCreatesEvaluationResult()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        var fake = new FakeLlmChatClient(ValidGameProfile());
        var service = CreateEvaluationService(database, fake);

        var result = await service.RunEvaluationBatchAsync(BatchRequest("game_profile_v1"), CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.Equal(1, fake.CallCount);
        Assert.Equal(1, result.Summary.ValidArtifactCount);
        Assert.Contains("Strict LLM Generation Evaluation", result.MarkdownReport);
    }

    [Fact]
    public async Task RunBatchDoesNotStageForReviewByDefault()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        var service = CreateEvaluationService(database, new FakeLlmChatClient(ValidGameProfile()));

        await service.RunEvaluationBatchAsync(BatchRequest("game_profile_v1"), CancellationToken.None);
        var review = await new GeneratorPlanDraftArtifactApprovalArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.False(review.Exists);
    }

    [Fact]
    public async Task RunBatchCanStageValidArtifactsForReviewWhenExplicitlyEnabled()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        var service = CreateEvaluationService(database, new FakeLlmChatClient(ValidGameProfile()));

        var result = await service.RunEvaluationBatchAsync(BatchRequest("game_profile_v1") with { StageValidArtifactsForReview = true }, CancellationToken.None);
        var review = await new GeneratorPlanDraftArtifactApprovalArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.True(result.Ok);
        Assert.True(review.Exists);
        Assert.Single(review.Worklist);
        Assert.Equal(1, result.Summary.StagedForReviewCount);
    }

    [Fact]
    public async Task RunBatchAggregatesMultipleContractsAndIterations()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        var service = CreateEvaluationService(database, new FakeLlmChatClient(
            ValidGameProfile(),
            ValidGameProfile("Second Profile"),
            ValidScenePack(),
            ValidScenePack("scene/second")));

        var result = await service.RunEvaluationBatchAsync(BatchRequest("game_profile_v1", "scene_pack_v1") with { IterationsPerContract = 2 }, CancellationToken.None);

        Assert.Equal(4, result.Summary.TotalGenerationRuns);
        Assert.Equal(4, result.Summary.ValidArtifactCount);
        Assert.Equal(2, result.ContractSummaries.Count);
        Assert.All(result.ContractSummaries, summary => Assert.Equal(2, summary.Runs));
    }

    [Fact]
    public async Task EvaluationArtifactSaveAndReadLatest()
    {
        using var temp = new TempDirectory();
        var database = await CreateDatabaseAsync(temp.Path);
        await SaveSelectionAsync(database);
        var service = CreateEvaluationService(database, new FakeLlmChatClient(ValidGameProfile()));

        var result = await service.RunEvaluationBatchAsync(BatchRequest("game_profile_v1"), CancellationToken.None);
        var latest = await new GeneratorPlanStrictLlmEvaluationArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.True(latest.Exists);
        Assert.Equal(result.EvaluationId, latest.Result.EvaluationId);
        Assert.Contains("Strict LLM Generation Evaluation", latest.MarkdownReport);
    }

    private static GeneratorPlanStrictLlmEvaluationService CreateEvaluationService(SqliteDesignDatabase database, ILlmChatClient llmChatClient)
    {
        var catalog = new GeneratorPlanStrictLlmArtifactContractCatalog();
        var generationService = new GeneratorPlanStrictLlmArtifactGenerationService(
            new InMemorySettingsRepository(Settings()),
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

        var renderer = new GeneratorPlanStrictLlmEvaluationMarkdownRenderer();
        return new GeneratorPlanStrictLlmEvaluationService(
            new GeneratorPlanStrictLlmArtifactGenerationArtifactReader(database),
            generationService,
            new GeneratorPlanCapabilitySelectionArtifactReader(database),
            catalog,
            renderer,
            new GeneratorPlanStrictLlmEvaluationArtifactService(database));
    }

    private static GeneratorPlanStrictLlmEvaluationRequest BatchRequest(params string[] contracts)
    {
        return new GeneratorPlanStrictLlmEvaluationRequest
        {
            LlmProfileId = "local",
            ContractIds = contracts,
            IterationsPerContract = 1,
            EnableRepairAttempt = true,
            MaxRepairAttempts = 1,
            StageValidArtifactsForReview = false
        };
    }

    private static GeneratorPlanStrictLlmArtifactGenerationResult AuditResult()
    {
        return new GeneratorPlanStrictLlmArtifactGenerationResult
        {
            Ok = false,
            Status = GeneratorPlanStrictLlmArtifactGenerationStatus.Invalid,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            SourceCapabilitySelectionId = "selection/test",
            RequestedContractIds = ["game_profile_v1", "scene_pack_v1", "quest_pack_v1"],
            Artifacts =
            [
                new GeneratorPlanStrictLlmGeneratedArtifact
                {
                    ArtifactId = "artifact/strict_llm/game_profile_v1",
                    ArtifactKind = "game_profile_v1",
                    ExpectedArtifactContract = "game_profile_v1",
                    ContentJson = ValidGameProfile("Test"),
                    Valid = true
                },
                new GeneratorPlanStrictLlmGeneratedArtifact
                {
                    ArtifactId = "artifact/strict_llm/scene_pack_v1",
                    ArtifactKind = "scene_pack_v1",
                    ExpectedArtifactContract = "scene_pack_v1",
                    ContentJson = ValidScenePack(),
                    Valid = true,
                    Repaired = true
                }
            ],
            Attempts =
            [
                Attempt("game_profile_v1", 0, false, true),
                Attempt("scene_pack_v1", 0, false, false, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid),
                Attempt("scene_pack_v1", 1, true, true),
                Attempt("quest_pack_v1", 0, false, false, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid)
            ],
            Diagnostics =
            [
                Diagnostic(GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid, "quest_pack_v1")
            ]
        };
    }

    private static GeneratorPlanStrictLlmArtifactGenerationAttempt Attempt(string contractId, int index, bool repair, bool ok, string code = "")
    {
        return new GeneratorPlanStrictLlmArtifactGenerationAttempt
        {
            ContractId = contractId,
            AttemptIndex = index,
            IsRepairAttempt = repair,
            ParsedOk = ok,
            ValidationOk = ok,
            Diagnostics = string.IsNullOrWhiteSpace(code)
                ? Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>()
                : [Diagnostic(code, contractId)]
        };
    }

    private static GeneratorPlanStrictLlmArtifactDiagnostic Diagnostic(string code, string contractId)
    {
        return new GeneratorPlanStrictLlmArtifactDiagnostic
        {
            Severity = GeneratorPlanPreviewDiagnosticSeverity.Error,
            Code = code,
            Message = "Invalid JSON.",
            Target = "response",
            ContractId = contractId
        };
    }

    private static async Task SaveAuditAsync(SqliteDesignDatabase database, GeneratorPlanStrictLlmArtifactGenerationResult result)
    {
        await new GeneratorPlanStrictLlmArtifactGenerationArtifactService(database).SaveAsync(result, CancellationToken.None);
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
                Title = "Test selection",
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

    private static string ValidGameProfile(string title = "Focused Profile")
    {
        return $$"""
        {
          "schema_version": "0.1",
          "artifact_kind": "game_profile_v1",
          "game": {
            "title": "{{title}}",
            "description": "Tiny",
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

    private static string ValidScenePack(string sceneId = "scene/start")
    {
        return $$"""
        {
          "schema_version": "0.1",
          "artifact_kind": "scene_pack_v1",
          "scenes": [{ "id": "{{sceneId}}", "title": "Start", "description": "Opening scene with enough concrete detail.", "purpose": "Intro." }],
          "source_context": { "capability_selection_id": "selection/test" }
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

        public int CallCount { get; private set; }

        public Task<LlmChatResponse> CompleteAsync(LlmEndpointSettings profile, LlmChatRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
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
