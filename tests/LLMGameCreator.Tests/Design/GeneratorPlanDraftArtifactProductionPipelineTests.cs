using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;
using GeneratedArtifactRepository = LLMGameCreator.Application.Design.IGeneratedArtifactRepository;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanDraftArtifactProductionPipelineTests
{
    [Fact]
    public async Task ProducerCreatesGameProfileDraftJson()
    {
        var artifact = await new DeterministicGeneratorPlanDraftArtifactProducer()
            .ProduceAsync(QueueItem("game_profile_v1"), new GeneratorPlanDraftArtifactProductionRequest { BatchId = "batch/test" }, CancellationToken.None);

        using var json = JsonDocument.Parse(artifact.ContentJson);

        Assert.Equal(GeneratorPlanProducedDraftArtifactState.ReadyForApproval, artifact.State);
        Assert.Equal("artifact/game_profile_v1", json.RootElement.GetProperty("artifact_id").GetString());
        Assert.True(json.RootElement.GetProperty("draft").GetBoolean());
        Assert.Equal("top_down", json.RootElement.GetProperty("game").GetProperty("camera").GetString());
        Assert.True(json.RootElement.GetProperty("pillars").GetArrayLength() > 0);
    }

    [Fact]
    public async Task ProducerCreatesKnownContractSpecificPayloads()
    {
        var producer = new DeterministicGeneratorPlanDraftArtifactProducer();
        var expectations = new Dictionary<string, string>
        {
            ["semantic_pack_v1"] = "semantic_groups",
            ["mechanics_pack_v1"] = "mechanics",
            ["scene_pack_v1"] = "scenes",
            ["entity_pack_v1"] = "entities",
            ["quest_pack_v1"] = "quests"
        };

        foreach (var (kind, propertyName) in expectations)
        {
            var artifact = await producer.ProduceAsync(QueueItem(kind), new GeneratorPlanDraftArtifactProductionRequest { BatchId = "batch/test" }, CancellationToken.None);
            using var json = JsonDocument.Parse(artifact.ContentJson);

            Assert.True(json.RootElement.TryGetProperty(propertyName, out var property), kind);
            Assert.True(property.GetArrayLength() > 0, kind);
        }
    }

    [Fact]
    public async Task ProducerCreatesGenericPayloadForUnknownKind()
    {
        var artifact = await new DeterministicGeneratorPlanDraftArtifactProducer()
            .ProduceAsync(QueueItem("unknown_pack_v1"), new GeneratorPlanDraftArtifactProductionRequest { BatchId = "batch/test" }, CancellationToken.None);

        using var json = JsonDocument.Parse(artifact.ContentJson);

        Assert.True(json.RootElement.TryGetProperty("draft_sections", out var sections));
        Assert.Equal("unknown_pack_v1", json.RootElement.GetProperty("artifact_kind").GetString());
        Assert.True(sections.GetArrayLength() > 0);
    }

    [Fact]
    public async Task ProducerCreatesBlockedEnvelopeForBlockedItemWhenBlockedProductionDisabled()
    {
        var queueResult = QueueResultWithItems(QueueItem("quest_pack_v1") with { State = GeneratorPlanDraftArtifactQueueItemState.Blocked });

        var result = await new GeneratorPlanDraftArtifactProductionService()
            .ProduceAsync(queueResult, new GeneratorPlanDraftArtifactProductionRequest { ProduceBlockedItems = false }, CancellationToken.None);

        var artifact = Assert.Single(result.Batch.Artifacts);
        using var json = JsonDocument.Parse(artifact.ContentJson);

        Assert.Equal(GeneratorPlanProducedDraftArtifactState.Blocked, artifact.State);
        Assert.True(json.RootElement.GetProperty("blocked").GetBoolean());
        Assert.NotEmpty(artifact.RepairRequestId);
    }

    [Fact]
    public async Task ProducedJsonIsValidJson()
    {
        var producer = new DeterministicGeneratorPlanDraftArtifactProducer();

        foreach (var kind in new[] { "game_profile_v1", "semantic_pack_v1", "mechanics_pack_v1", "scene_pack_v1", "entity_pack_v1", "quest_pack_v1", "unknown_pack_v1" })
        {
            var artifact = await producer.ProduceAsync(QueueItem(kind), new GeneratorPlanDraftArtifactProductionRequest { BatchId = "batch/test" }, CancellationToken.None);
            using var json = JsonDocument.Parse(artifact.ContentJson);

            Assert.Equal("0.1", json.RootElement.GetProperty("schema_version").GetString());
        }
    }

    [Fact]
    public async Task ServiceProducesArtifactsFromQueueResult()
    {
        var result = await new GeneratorPlanDraftArtifactProductionService()
            .ProduceAsync(QueueResultWithItems(QueueItem("game_profile_v1")), new GeneratorPlanDraftArtifactProductionRequest(), CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Equal(GeneratorPlanDraftArtifactProductionStatus.ReadyForApproval, result.Status);
        Assert.Equal(GeneratorPlanProducedDraftArtifactState.ReadyForApproval, Assert.Single(result.Batch.Artifacts).State);
        Assert.NotEmpty(result.MarkdownReport);
    }

    [Fact]
    public async Task ServiceProducesArtifactsFromExamplePath()
    {
        using var temp = new TempDirectory();
        var path = WriteExample(temp.Path);

        var result = await new GeneratorPlanDraftArtifactProductionService()
            .ProduceFromExampleAsync(path, new GeneratorPlanDraftArtifactProductionRequest(), CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Equal("example/test/v1", result.Batch.SourcePreviewExampleId);
        Assert.Single(result.Batch.Artifacts);
    }

    [Fact]
    public async Task ServiceReturnsInvalidWhenQueueInvalid()
    {
        var result = await new GeneratorPlanDraftArtifactProductionService()
            .ProduceAsync(new GeneratorPlanDraftArtifactQueueResult
            {
                Ok = false,
                Status = GeneratorPlanDraftArtifactQueueStatus.Invalid,
                Queue = new GeneratorPlanDraftArtifactQueue { Id = "queue/invalid" }
            }, new GeneratorPlanDraftArtifactProductionRequest(), CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Equal(GeneratorPlanDraftArtifactProductionStatus.Invalid, result.Status);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactProductionDiagnosticCodes.QueueInvalid);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactProductionDiagnosticCodes.NoArtifacts);
    }

    [Fact]
    public async Task ServiceCanSkipMarkdownRendering()
    {
        var result = await new GeneratorPlanDraftArtifactProductionService()
            .ProduceAsync(QueueResultWithItems(QueueItem("game_profile_v1")), new GeneratorPlanDraftArtifactProductionRequest { RenderMarkdown = false }, CancellationToken.None);

        Assert.Empty(result.MarkdownReport);
    }

    [Fact]
    public async Task ServiceKeepsBlockedItemsBlocked()
    {
        var item = QueueItem("quest_pack_v1") with { State = GeneratorPlanDraftArtifactQueueItemState.Blocked };
        var result = await new GeneratorPlanDraftArtifactProductionService()
            .ProduceAsync(QueueResultWithItems(item), new GeneratorPlanDraftArtifactProductionRequest { ProduceBlockedItems = true }, CancellationToken.None);

        Assert.Equal(GeneratorPlanDraftArtifactProductionStatus.Blocked, result.Status);
        Assert.Equal(GeneratorPlanProducedDraftArtifactState.Blocked, Assert.Single(result.Batch.Artifacts).State);
    }

    [Fact]
    public void ValidatorAcceptsValidProductionBatch()
    {
        var batch = new GeneratorPlanDraftArtifactProductionValidator().Validate(Batch(Artifact("artifact/a")));

        Assert.Equal(GeneratorPlanDraftArtifactProductionValidationState.Valid, GeneratorPlanDraftArtifactProductionPolicy.ToValidationState(batch.Summary));
    }

    [Fact]
    public void ValidatorReportsNoArtifactsAsError()
    {
        var batch = new GeneratorPlanDraftArtifactProductionValidator().Validate(new GeneratorPlanDraftArtifactProductionBatch { Id = "batch/test" });

        Assert.Contains(batch.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactProductionDiagnosticCodes.NoArtifacts && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
    }

    [Fact]
    public void ValidatorReportsDuplicateArtifactIdsAsError()
    {
        var batch = new GeneratorPlanDraftArtifactProductionValidator().Validate(Batch(Artifact("artifact/a"), Artifact("artifact/a") with { Id = "produced/b" }));

        Assert.Contains(batch.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactProductionDiagnosticCodes.DuplicateArtifactId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
    }

    [Fact]
    public void ValidatorReportsInvalidJsonAsError()
    {
        var batch = new GeneratorPlanDraftArtifactProductionValidator().Validate(Batch(Artifact("artifact/a") with { ContentJson = "{ invalid" }));

        Assert.Contains(batch.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactInvalidJson && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
    }

    [Fact]
    public void ValidatorReportsMissingSchemaVersionAsWarning()
    {
        var batch = new GeneratorPlanDraftArtifactProductionValidator().Validate(Batch(Artifact("artifact/a") with { ContentJson = "{\"artifact_id\":\"artifact/a\"}" }));

        Assert.Contains(batch.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactMissingSchemaVersion && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
    }

    [Fact]
    public void ValidatorReportsArtifactIdMismatchAsWarning()
    {
        var batch = new GeneratorPlanDraftArtifactProductionValidator().Validate(Batch(Artifact("artifact/a") with { ContentJson = "{\"schema_version\":\"0.1\",\"artifact_id\":\"artifact/b\"}" }));

        Assert.Contains(batch.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactProductionDiagnosticCodes.ArtifactContentArtifactIdMismatch && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
    }

    [Fact]
    public void ValidatorReportsBlockedArtifactWithoutRepairRequestAsWarning()
    {
        var batch = new GeneratorPlanDraftArtifactProductionValidator().Validate(Batch(Artifact("artifact/a") with { State = GeneratorPlanProducedDraftArtifactState.Blocked, RepairRequestId = string.Empty }));

        Assert.Contains(batch.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactProductionDiagnosticCodes.BlockedArtifactMissingRepairRequest && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
    }

    [Fact]
    public void ValidationPolicyMapsOnlyWarningsAndErrorsToValidationResults()
    {
        var results = GeneratorPlanDraftArtifactProductionPolicy.ToValidationResults("artifact/result",
        [
            new GeneratorPlanDraftArtifactProductionDiagnostic { Severity = GeneratorPlanPreviewDiagnosticSeverity.Info, Code = "info", Message = "Info" },
            new GeneratorPlanDraftArtifactProductionDiagnostic { Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning, Code = "warning", Message = "Warning" }
        ]);

        var result = Assert.Single(results);
        Assert.Equal("warning", result.Code);
    }

    [Fact]
    public void MarkdownRendererRendersSummaryArtifactsDiagnosticsAndJsonPreview()
    {
        var batch = Batch(Artifact("artifact/a")) with
        {
            Diagnostics =
            [
                new GeneratorPlanDraftArtifactProductionDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "warning",
                    ArtifactId = "artifact/a",
                    QueueItemId = "item/a",
                    Target = "content_json",
                    Message = "Warning"
                }
            ],
            Summary = new GeneratorPlanDraftArtifactProductionSummary
            {
                ArtifactCount = 1,
                ReadyForApprovalCount = 1,
                WarningCount = 1
            }
        };

        var markdown = new GeneratorPlanDraftArtifactProductionMarkdownRenderer().Render(batch);

        Assert.Contains("# Draft Artifact Production", markdown);
        Assert.Contains("| State | Artifact ID | Kind | Contract | Queue Item | Gates | Approval |", markdown);
        Assert.Contains("| Severity | Code | Artifact | Queue Item | Target | Message |", markdown);
        Assert.Contains("## Artifact JSON Preview", markdown);
        Assert.Contains("```json", markdown);
    }

    [Fact]
    public void MarkdownRendererEscapesTableCells()
    {
        var batch = Batch(Artifact("artifact|a") with { QueueItemId = "item|a" }) with
        {
            Diagnostics =
            [
                new GeneratorPlanDraftArtifactProductionDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "code|pipe",
                    Message = "Line 1\nLine 2"
                }
            ]
        };

        var markdown = new GeneratorPlanDraftArtifactProductionMarkdownRenderer().Render(batch);

        Assert.Contains("artifact\\|a", markdown);
        Assert.Contains("code\\|pipe", markdown);
        Assert.Contains("Line 1<br>Line 2", markdown);
    }

    [Fact]
    public void MarkdownRendererTruncatesLongJsonPreview()
    {
        var markdown = new GeneratorPlanDraftArtifactProductionMarkdownRenderer()
            .Render(Batch(Artifact("artifact/a") with { ContentJson = "{\"artifact_id\":\"artifact/a\",\"value\":\"" + new string('x', 2000) + "\"}" }));

        Assert.Contains("\n...", markdown);
    }

    [Fact]
    public async Task ArtifactServiceSavesBatchMarkdownAndProducedArtifacts()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = CreateArtifactService(database);
        var result = await service.SaveAsync(await ProductionResultAsync(), new GeneratorPlanDraftArtifactProductionArtifactSaveRequest(), CancellationToken.None);

        Assert.NotNull(await database.GetGeneratedArtifactByIdAsync(GeneratorPlanDraftArtifactProductionArtifactIds.BatchArtifactId, CancellationToken.None));
        Assert.NotNull(result.MarkdownArtifact);
        Assert.Single(result.ProducedArtifacts);
        Assert.NotNull(await database.GetGeneratedArtifactByIdAsync(result.ProducedArtifacts[0].Id, CancellationToken.None));
    }

    [Fact]
    public async Task ArtifactServiceSavesValidationResultsForBatchAndProducedArtifacts()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = CreateArtifactService(database);
        var production = await ProductionResultAsync(Artifact("artifact/a") with { ContentJson = "{\"artifact_id\":\"artifact/b\"}" });

        var result = await service.SaveAsync(production, new GeneratorPlanDraftArtifactProductionArtifactSaveRequest(), CancellationToken.None);

        Assert.Contains(result.ValidationResults, validation => validation.ArtifactId == GeneratorPlanDraftArtifactProductionArtifactIds.BatchArtifactId);
        Assert.Contains(result.ValidationResults, validation => validation.ArtifactId == "artifact/a");
    }

    [Fact]
    public async Task ArtifactServiceIsIdempotentForSameArtifactIds()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = CreateArtifactService(database);
        var production = await ProductionResultAsync();

        var first = await service.SaveAsync(production, new GeneratorPlanDraftArtifactProductionArtifactSaveRequest(), CancellationToken.None);
        var second = await service.SaveAsync(production, new GeneratorPlanDraftArtifactProductionArtifactSaveRequest(), CancellationToken.None);
        var artifacts = await database.ListGeneratedArtifactsAsync(CancellationToken.None);

        Assert.Equal(first.BatchArtifact.Id, second.BatchArtifact.Id);
        Assert.Equal(3, artifacts.Count);
    }

    [Fact]
    public async Task ArtifactServiceSupportsCustomBatchAndMarkdownArtifactIds()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = CreateArtifactService(database);

        var result = await service.SaveAsync(await ProductionResultAsync(), new GeneratorPlanDraftArtifactProductionArtifactSaveRequest
        {
            BatchArtifactId = "artifact/custom/batch",
            MarkdownArtifactId = "artifact/custom/markdown",
            GeneratedBy = "test"
        }, CancellationToken.None);

        Assert.Equal("artifact/custom/batch", result.BatchArtifact.Id);
        Assert.Equal("artifact/custom/markdown", result.MarkdownArtifact?.Id);
    }

    [Fact]
    public async Task ArtifactServiceSkipsMarkdownWhenRenderingDisabled()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = CreateArtifactService(database);
        var production = await ProductionResultAsync(renderMarkdown: false);

        var result = await service.SaveAsync(production, new GeneratorPlanDraftArtifactProductionArtifactSaveRequest(), CancellationToken.None);

        Assert.Null(result.MarkdownArtifact);
        Assert.Null(await database.GetGeneratedArtifactByIdAsync(GeneratorPlanDraftArtifactProductionArtifactIds.MarkdownArtifactId, CancellationToken.None));
    }

    [Fact]
    public async Task ReaderReturnsEmptyWhenMissing()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);

        var result = await new GeneratorPlanDraftArtifactProductionArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.False(result.Exists);
    }

    [Fact]
    public async Task ReaderReturnsBatchMarkdownProducedArtifactsAndValidationResults()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var production = await ProductionResultAsync(Artifact("artifact/a") with { ContentJson = "{\"artifact_id\":\"artifact/b\"}" });
        await CreateArtifactService(database).SaveAsync(production, new GeneratorPlanDraftArtifactProductionArtifactSaveRequest(), CancellationToken.None);

        var result = await new GeneratorPlanDraftArtifactProductionArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.True(result.Exists);
        Assert.NotNull(result.BatchArtifact);
        Assert.NotNull(result.MarkdownArtifact);
        Assert.Single(result.ProducedArtifacts);
        Assert.NotEmpty(result.ValidationResults);
    }

    [Fact]
    public async Task ReaderBuildsWorklistForApprovalAndRepair()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var blocked = Artifact("artifact/blocked") with
        {
            State = GeneratorPlanProducedDraftArtifactState.Blocked,
            RepairRequestId = "repair/blocked",
            RequiresHumanApproval = true
        };
        await CreateArtifactService(database).SaveAsync(await ProductionResultAsync(blocked), new GeneratorPlanDraftArtifactProductionArtifactSaveRequest(), CancellationToken.None);

        var result = await new GeneratorPlanDraftArtifactProductionArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        var item = Assert.Single(result.Worklist);
        Assert.Equal("artifact/blocked", item.ArtifactId);
        Assert.Equal(GeneratorPlanProducedDraftArtifactState.Blocked, item.State);
        Assert.True(item.RequiresHumanApproval);
        Assert.Equal("repair/blocked", item.RepairRequestId);
    }

    [Fact]
    public async Task ReaderWorksWhenMarkdownMissing()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        await CreateArtifactService(database).SaveAsync(await ProductionResultAsync(renderMarkdown: false), new GeneratorPlanDraftArtifactProductionArtifactSaveRequest(), CancellationToken.None);

        var result = await new GeneratorPlanDraftArtifactProductionArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.True(result.Exists);
        Assert.Null(result.MarkdownArtifact);
        Assert.Single(result.ProducedArtifacts);
    }

    private static async Task<GeneratorPlanDraftArtifactProductionResult> ProductionResultAsync(
        GeneratorPlanProducedDraftArtifact? artifact = null,
        bool renderMarkdown = true)
    {
        var batch = new GeneratorPlanDraftArtifactProductionBatch
        {
            Id = "batch/test",
            SourceQueueId = "queue/test",
            SourceDraftExecutionPlanId = "draft/test",
            SourcePreviewExampleId = "example/test/v1",
            SourcePath = "plan.example.json",
            Artifacts = [artifact ?? Artifact("artifact/a")]
        };
        batch = new GeneratorPlanDraftArtifactProductionValidator().Validate(batch);

        return await Task.FromResult(new GeneratorPlanDraftArtifactProductionResult
        {
            Ok = batch.Status != GeneratorPlanDraftArtifactProductionStatus.Invalid,
            Status = batch.Status,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            QueueResult = QueueResultWithItems(QueueItem("game_profile_v1")),
            Batch = batch,
            MarkdownReport = renderMarkdown ? new GeneratorPlanDraftArtifactProductionMarkdownRenderer().Render(batch) : string.Empty,
            Diagnostics = batch.Diagnostics
        });
    }

    private static GeneratorPlanDraftArtifactProductionBatch Batch(params GeneratorPlanProducedDraftArtifact[] artifacts)
    {
        return new GeneratorPlanDraftArtifactProductionBatch
        {
            Id = "batch/test",
            SourceQueueId = "queue/test",
            SourceDraftExecutionPlanId = "draft/test",
            SourcePreviewExampleId = "example/test/v1",
            SourcePath = "plan.example.json",
            Artifacts = artifacts
        };
    }

    private static GeneratorPlanProducedDraftArtifact Artifact(string artifactId)
    {
        return new GeneratorPlanProducedDraftArtifact
        {
            Id = "produced/" + artifactId,
            QueueItemId = "queue/test/item/1",
            SourceExecutionStepId = "step/test",
            ArtifactId = artifactId,
            ArtifactKind = "game_profile_v1",
            ExpectedArtifactContract = "game_profile_v1",
            State = GeneratorPlanProducedDraftArtifactState.ReadyForApproval,
            ContentJson = $$"""{"schema_version":"0.1","artifact_id":"{{artifactId}}"}""",
            ValidationGates = ["validation.level_0_json_shape"],
            RequiresHumanApproval = true
        };
    }

    private static GeneratorPlanDraftArtifactQueueResult QueueResultWithItems(params GeneratorPlanDraftArtifactQueueItem[] items)
    {
        var repairRequests = items
            .Where(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Blocked)
            .Select(item => new GeneratorPlanDraftArtifactRepairRequest
            {
                Id = "repair/" + item.Id,
                ArtifactId = item.ArtifactId,
                SourceExecutionStepId = item.SourceExecutionStepId,
                ReasonCode = "blocked_item",
                Message = "Blocked."
            })
            .ToList();

        var queue = new GeneratorPlanDraftArtifactQueue
        {
            Id = "queue/test",
            SourceDraftExecutionPlanId = "draft/test",
            SourcePreviewExampleId = "example/test/v1",
            SourcePath = "plan.example.json",
            Status = items.Any(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Blocked)
                ? GeneratorPlanDraftArtifactQueueStatus.Blocked
                : GeneratorPlanDraftArtifactQueueStatus.Ready,
            Items = items,
            RepairRequests = repairRequests
        };
        queue = queue with
        {
            Summary = new GeneratorPlanDraftArtifactQueueSummary
            {
                ItemCount = items.Length,
                ReadyItemCount = items.Count(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Ready),
                BlockedItemCount = items.Count(item => item.State == GeneratorPlanDraftArtifactQueueItemState.Blocked),
                RepairRequestCount = repairRequests.Count
            }
        };

        return new GeneratorPlanDraftArtifactQueueResult
        {
            Ok = true,
            Status = queue.Status,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            Queue = queue
        };
    }

    private static GeneratorPlanDraftArtifactQueueItem QueueItem(string artifactKind)
    {
        return new GeneratorPlanDraftArtifactQueueItem
        {
            Id = "queue/test/item/1",
            Order = 1,
            SourceExecutionStepId = "draft/test/step/1",
            State = GeneratorPlanDraftArtifactQueueItemState.Ready,
            ArtifactId = "artifact/" + artifactKind,
            ArtifactKind = artifactKind,
            ExpectedArtifactContract = artifactKind,
            ProducerRole = "role/designer_llm/v1",
            ContextPackTemplate = "context_template/design_discussion/v1",
            Inputs = ["input/test"],
            ValidationGates =
            [
                new GeneratorPlanDraftValidationGateTicket
                {
                    Id = "gate/test",
                    GateId = "validation.level_0_json_shape",
                    State = GeneratorPlanDraftValidationGateState.Pending,
                    ArtifactId = "artifact/" + artifactKind,
                    SourceExecutionStepId = "draft/test/step/1"
                }
            ],
            RequiresHumanApproval = true
        };
    }

    private static string WriteExample(string root)
    {
        var path = Path.Combine(root, "plan.example.json");
        File.WriteAllText(path, """
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
          "steps": [
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
        }
        """);
        return path;
    }

    private static GeneratorPlanDraftArtifactProductionArtifactService CreateArtifactService(GeneratedArtifactRepository repository)
    {
        return new GeneratorPlanDraftArtifactProductionArtifactService(
            new GeneratorPlanDraftArtifactProductionService(),
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
