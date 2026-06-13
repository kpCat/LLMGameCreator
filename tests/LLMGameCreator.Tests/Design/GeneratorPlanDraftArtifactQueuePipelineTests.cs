using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;
using GeneratedArtifactRepository = LLMGameCreator.Application.Design.IGeneratedArtifactRepository;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanDraftArtifactQueuePipelineTests
{
    [Fact]
    public void BuilderCreatesDeterministicQueueItemsGatesRepairRequestsAndMapsDiagnostics()
    {
        var plan = CompleteDraftExecutionPlan() with
        {
            Steps =
            [
                CompleteStep(),
                CompleteStep() with
                {
                    Id = "draft_execution/example/test/v1/step/step_blocked",
                    Order = 2,
                    State = GeneratorPlanDraftExecutionStepState.Blocked,
                    PlannedArtifactId = "artifact/draft_execution/draft_execution/example/test/v1/step/2/quest_pack_v1",
                    PlannedArtifactKind = "quest_pack_v1",
                    ExpectedArtifactContract = "quest_pack_v1"
                }
            ],
            Diagnostics =
            [
                new GeneratorPlanDraftExecutionDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = GeneratorPlanDraftExecutionDiagnosticCodes.StepMissingProducerRole,
                    StepId = "draft_execution/example/test/v1/step/step_profile_summary",
                    Message = "Producer role should be set."
                }
            ]
        };

        var queue = new GeneratorPlanDraftArtifactQueueBuilder().BuildQueue(plan);

        Assert.Equal("draft_artifact_queue/draft_execution/example/test/v1", queue.Id);
        Assert.Equal(GeneratorPlanDraftArtifactQueueStatus.Blocked, queue.Status);
        Assert.Equal(2, queue.Items.Count);
        Assert.Equal("draft_artifact_queue/draft_execution/example/test/v1/item/1", queue.Items[0].Id);
        Assert.Equal(GeneratorPlanDraftArtifactQueueItemState.Ready, queue.Items[0].State);
        Assert.Equal("draft_artifact_queue/draft_execution/example/test/v1/item/1/gate/validation.level_0_json_shape", Assert.Single(queue.Items[0].ValidationGates).Id);
        Assert.Equal(GeneratorPlanDraftValidationGateState.Pending, queue.Items[0].ValidationGates[0].State);
        Assert.Equal(GeneratorPlanDraftArtifactQueueItemState.Blocked, queue.Items[1].State);
        Assert.Single(queue.RepairRequests);
        Assert.Equal("blocked_item", queue.RepairRequests[0].ReasonCode);
        Assert.Contains(queue.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.ExecutionDiagnostic);

        var noRepairQueue = new GeneratorPlanDraftArtifactQueueBuilder().BuildQueue(
            plan,
            new GeneratorPlanDraftArtifactQueueBuilderOptions { CreateRepairRequestsForBlockedItems = false });

        Assert.Empty(noRepairQueue.RepairRequests);
    }

    [Fact]
    public void ValidatorAndPolicyReportQueueErrorsWarningsAndValidationResults()
    {
        var valid = new GeneratorPlanDraftArtifactQueueValidator().Validate(
            new GeneratorPlanDraftArtifactQueueBuilder().BuildQueue(CompleteDraftExecutionPlan()));
        var invalid = new GeneratorPlanDraftArtifactQueueValidator().Validate(new GeneratorPlanDraftArtifactQueue
        {
            Id = string.Empty,
            Items =
            [
                new GeneratorPlanDraftArtifactQueueItem
                {
                    Id = "item/a",
                    SourceExecutionStepId = string.Empty,
                    ArtifactId = "artifact/a",
                    ValidationGates =
                    [
                        new GeneratorPlanDraftValidationGateTicket
                        {
                            Id = "gate/empty",
                            GateId = string.Empty,
                            ArtifactId = "artifact/a"
                        }
                    ]
                },
                new GeneratorPlanDraftArtifactQueueItem
                {
                    Id = "item/a",
                    SourceExecutionStepId = "step/b",
                    ArtifactId = "artifact/a",
                    ArtifactKind = "game_profile_v1",
                    ExpectedArtifactContract = "game_profile_v1",
                    ValidationGates = [new GeneratorPlanDraftValidationGateTicket { Id = "gate/b", GateId = "validation.level_0_json_shape" }]
                }
            ],
            RepairRequests =
            [
                new GeneratorPlanDraftArtifactRepairRequest
                {
                    Id = "repair/a",
                    SourceExecutionStepId = "step/a",
                    ArtifactId = "artifact/a"
                }
            ],
            Diagnostics =
            [
                new GeneratorPlanDraftArtifactQueueDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Info,
                    Code = "info",
                    Message = "Info"
                }
            ]
        });
        var noItems = new GeneratorPlanDraftArtifactQueueValidator().Validate(new GeneratorPlanDraftArtifactQueue { Id = "queue/empty" });

        var validationResults = GeneratorPlanDraftArtifactQueuePolicy.ToValidationResults("artifact/result", invalid.Diagnostics);

        Assert.Equal(GeneratorPlanDraftArtifactQueueValidationState.Valid, GeneratorPlanDraftArtifactQueuePolicy.ToValidationState(valid.Summary));
        Assert.Contains(noItems.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.NoItems && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.MissingQueueId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.DuplicateItemId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.DuplicateArtifactId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.ItemMissingSourceExecutionStepId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.ItemMissingArtifactKind && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.ItemMissingExpectedArtifactContract && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.GateMissingId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.RepairRequestMissingReason && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Equal(GeneratorPlanDraftArtifactQueueValidationState.Invalid, GeneratorPlanDraftArtifactQueuePolicy.ToValidationState(invalid.Summary));
        Assert.DoesNotContain(validationResults, result => result.Severity == GeneratorPlanPreviewDiagnosticSeverity.Info);
        Assert.All(validationResults, result => Assert.Equal("artifact/result", result.ArtifactId));
    }

    [Fact]
    public void MarkdownRendererRendersSectionsEscapesCellsAndHandlesEmptySections()
    {
        var plan = CompleteDraftExecutionPlan() with
        {
            Steps =
            [
                CompleteStep(),
                CompleteStep() with
                {
                    Id = "draft_execution/example/test/v1/step/step_blocked",
                    Order = 2,
                    State = GeneratorPlanDraftExecutionStepState.Blocked,
                    PlannedArtifactId = "artifact/draft_execution/draft_execution/example/test/v1/step/2/quest_pack_v1",
                    PlannedArtifactKind = "quest_pack_v1",
                    ExpectedArtifactContract = "quest_pack_v1"
                }
            ]
        };
        var queue = new GeneratorPlanDraftArtifactQueueBuilder().BuildQueue(plan) with
        {
            Diagnostics =
            [
                new GeneratorPlanDraftArtifactQueueDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "code|pipe",
                    ItemId = "item|pipe",
                    ArtifactId = "artifact|pipe",
                    GateId = "gate|pipe",
                    Message = "Line 1\nLine 2"
                }
            ]
        };
        queue = queue with
        {
            Summary = new GeneratorPlanDraftArtifactQueueSummary
            {
                ItemCount = queue.Items.Count,
                ReadyItemCount = queue.Items.Count(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Ready),
                BlockedItemCount = queue.Items.Count(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Blocked),
                ValidationGateCount = queue.Items.Sum(item => item.ValidationGates.Count),
                RepairRequestCount = queue.RepairRequests.Count,
                WarningCount = 1
            }
        };

        var markdown = new GeneratorPlanDraftArtifactQueueMarkdownRenderer().Render(queue);
        var empty = new GeneratorPlanDraftArtifactQueueMarkdownRenderer().Render(new GeneratorPlanDraftArtifactQueue());

        Assert.Contains("# Draft Artifact Production Queue", markdown);
        Assert.Contains("| Order | State | Item ID | Step | Artifact | Kind | Contract | Gates | Approval |", markdown);
        Assert.Contains("| Gate ID | State | Artifact | Step |", markdown);
        Assert.Contains("| Request ID | State | Step | Artifact | Reason | Message |", markdown);
        Assert.Contains("code\\|pipe", markdown);
        Assert.Contains("Line 1<br>Line 2", markdown);
        Assert.Contains("_No draft artifact queue items were reported._", empty);
        Assert.Contains("_No validation gate tickets were reported._", empty);
        Assert.Contains("_No repair request drafts were reported._", empty);
        Assert.Contains("_No diagnostics were reported._", empty);
    }

    [Fact]
    public async Task ServiceCreatesQueueFromDraftResultAndExamplePathAndHandlesInvalidOrNoMarkdown()
    {
        using var temp = new TempDirectory();
        var path = WriteExample(temp.Path);
        var preview = await new GeneratorPlanPreviewService().PreviewAsync(new GeneratorPlanPreviewRequest { SourcePath = path }, CancellationToken.None);
        var draft = await new GeneratorPlanDraftExecutionService().CreateDraftAsync(preview, new GeneratorPlanDraftExecutionRequest(), CancellationToken.None);
        var service = new GeneratorPlanDraftArtifactQueueService();

        var fromDraft = await service.CreateQueueAsync(draft, new GeneratorPlanDraftArtifactQueueRequest(), CancellationToken.None);
        var fromExample = await service.CreateQueueFromExampleAsync(path, new GeneratorPlanDraftArtifactQueueRequest { QueueId = "queue/custom" }, CancellationToken.None);
        var noMarkdown = await service.CreateQueueAsync(draft, new GeneratorPlanDraftArtifactQueueRequest { RenderMarkdown = false }, CancellationToken.None);
        var invalidPath = Path.Combine(temp.Path, "broken.example.json");
        await File.WriteAllTextAsync(invalidPath, "{ invalid", CancellationToken.None);
        var invalid = await service.CreateQueueFromExampleAsync(invalidPath, new GeneratorPlanDraftArtifactQueueRequest(), CancellationToken.None);

        Assert.True(fromDraft.Ok);
        Assert.Equal(GeneratorPlanDraftArtifactQueueStatus.Ready, fromDraft.Status);
        Assert.NotEmpty(fromDraft.MarkdownReport);
        Assert.Equal("queue/custom", fromExample.Queue.Id);
        Assert.Empty(noMarkdown.MarkdownReport);
        Assert.False(invalid.Ok);
        Assert.Equal(GeneratorPlanDraftArtifactQueueStatus.Invalid, invalid.Status);
    }

    [Fact]
    public async Task ArtifactServiceSavesResultMarkdownValidationSupportsCustomIdsAndIdempotence()
    {
        using var temp = new TempDirectory();
        var warningPath = WriteExample(temp.Path, stepsJson: """
        [
          {
            "id": "step/warning",
            "order": 1,
            "title": "Warning",
            "producer_role": "role/designer_llm/v1",
            "context_pack_template": "context_template/design_discussion/v1",
            "expected_artifact_contract": "game_profile_v1"
          }
        ]
        """);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = CreateArtifactService(database);
        var request = new GeneratorPlanDraftArtifactQueueArtifactRequest
        {
            PreviewRequest = new GeneratorPlanPreviewRequest { SourcePath = warningPath }
        };

        var first = await service.CaptureAsync(request, CancellationToken.None);
        var second = await service.CaptureAsync(request, CancellationToken.None);
        var custom = await service.CaptureAsync(new GeneratorPlanDraftArtifactQueueArtifactRequest
        {
            PreviewRequest = new GeneratorPlanPreviewRequest { SourcePath = warningPath },
            QueueRequest = new GeneratorPlanDraftArtifactQueueRequest { RenderMarkdown = false },
            ResultArtifactId = "artifact/generator_plan_draft_artifact_queue/custom",
            MarkdownArtifactId = "artifact/generator_plan_draft_artifact_queue_markdown/custom",
            GeneratedBy = "test"
        }, CancellationToken.None);

        Assert.Equal(GeneratorPlanDraftArtifactQueueValidationState.Warnings, first.ResultArtifact.ValidationState);
        Assert.NotNull(first.MarkdownArtifact);
        Assert.Contains(first.ValidationResults, result => result.Code == GeneratorPlanDraftArtifactQueueDiagnosticCodes.ExecutionDiagnostic);
        Assert.Equal(first.ResultArtifact.Id, second.ResultArtifact.Id);
        Assert.Equal(first.ValidationResults.Count, (await database.ListValidationResultsByArtifactAsync(first.ResultArtifact.Id, CancellationToken.None)).Count);
        Assert.Null(custom.MarkdownArtifact);
        Assert.Equal("artifact/generator_plan_draft_artifact_queue/custom", custom.ResultArtifact.Id);
    }

    [Fact]
    public async Task ReaderReturnsEmptySavedArtifactsValidationResultsAndWorksWhenMarkdownMissing()
    {
        using var temp = new TempDirectory();
        var path = WriteExample(temp.Path);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var reader = new GeneratorPlanDraftArtifactQueueArtifactReader(database);

        var missing = await reader.ReadLatestAsync(CancellationToken.None);
        await CreateArtifactService(database).CaptureAsync(new GeneratorPlanDraftArtifactQueueArtifactRequest
        {
            PreviewRequest = new GeneratorPlanPreviewRequest { SourcePath = path },
            QueueRequest = new GeneratorPlanDraftArtifactQueueRequest { RenderMarkdown = false }
        }, CancellationToken.None);
        var loaded = await reader.ReadLatestAsync(CancellationToken.None);

        Assert.False(missing.Exists);
        Assert.True(loaded.Exists);
        Assert.NotNull(loaded.ResultArtifact);
        Assert.Null(loaded.MarkdownArtifact);
        Assert.Empty(loaded.ValidationResults);
    }

    private static GeneratorPlanDraftExecutionPlan CompleteDraftExecutionPlan()
    {
        var plan = new GeneratorPlanDraftExecutionPlan
        {
            Id = "draft_execution/example/test/v1",
            SourcePreviewExampleId = "example/test/v1",
            SourcePath = "plan.example.json",
            Title = "Test",
            Status = GeneratorPlanDraftExecutionStatus.Ready,
            Steps = [CompleteStep()]
        };

        return plan with
        {
            Summary = new GeneratorPlanDraftExecutionSummary
            {
                StepCount = 1,
                PlannedArtifactCount = 1
            }
        };
    }

    private static GeneratorPlanDraftExecutionStep CompleteStep()
    {
        return new GeneratorPlanDraftExecutionStep
        {
            Id = "draft_execution/example/test/v1/step/step_profile_summary",
            Order = 1,
            Title = "Profile summary",
            SourcePreviewStepId = "step/profile_summary",
            State = GeneratorPlanDraftExecutionStepState.Ready,
            ProducerRole = "role/designer_llm/v1",
            ContextPackTemplate = "context_template/design_discussion/v1",
            ExpectedArtifactContract = "game_profile_v1",
            Inputs = ["game_profile_v1"],
            ValidationGates = ["validation.level_0_json_shape"],
            PlannedArtifactId = "artifact/draft_execution/draft_execution/example/test/v1/step/1/game_profile_v1",
            PlannedArtifactKind = "game_profile_v1",
            RepairRequestId = "repair/draft_execution/draft_execution/example/test/v1/step/1",
            RequiresHumanApproval = true
        };
    }

    private static string WriteExample(string root, string fileName = "plan.example.json", string? stepsJson = null)
    {
        var path = Path.Combine(root, fileName);
        File.WriteAllText(path, $$"""
        {
          "schema_version": "0.1",
          "example_id": "example/test/v1",
          "title": "Test Generator Plan",
          "purpose": "Test plan preview.",
          "source_profile": {
            "id": "profile/test/v1"
          },
          "selected_feature_bundles": [
            "feature_bundle/test/v1"
          ],
          "target_artifacts": [
            "game_profile_v1"
          ],
          "steps": {{stepsJson ?? """
          [
            {
              "id": "step/profile_summary",
              "order": 1,
              "title": "Profile summary",
              "producer_role": "role/designer_llm/v1",
              "context_pack_template": "context_template/design_discussion/v1",
              "expected_artifact_contract": "game_profile_v1",
              "inputs": ["game_profile_v1"],
              "validation_gates": ["validation.level_0_json_shape"],
              "on_success": "stage_profile",
              "on_failure": "request_profile_clarification"
            }
          ]
          """}}
        }
        """);
        return path;
    }

    private static GeneratorPlanDraftArtifactQueueArtifactService CreateArtifactService(GeneratedArtifactRepository repository)
    {
        return new GeneratorPlanDraftArtifactQueueArtifactService(
            new GeneratorPlanDraftArtifactQueueService(),
            repository);
    }

    private static async Task<SqliteDesignDatabase> CreateInitializedDatabaseAsync(string root)
    {
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(root, ".llmgc", "design.db"), CancellationToken.None);
        return database;
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
