using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;
using GeneratedArtifactRepository = LLMGameCreator.Application.Design.IGeneratedArtifactRepository;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanDraftArtifactApprovalPipelineTests
{
    [Fact]
    public async Task ServiceCreatesPendingSnapshotFromProductionResult()
    {
        var result = new GeneratorPlanDraftArtifactApprovalService().CreateSnapshot(
            await ProductionResultAsync(),
            new GeneratorPlanDraftArtifactApprovalRequest());

        var item = Assert.Single(result.Snapshot.Items);
        Assert.True(result.Ok);
        Assert.Equal(GeneratorPlanDraftArtifactStagingStatus.NeedsReview, result.Status);
        Assert.Equal(GeneratorPlanDraftArtifactApprovalItemState.Pending, item.State);
        Assert.NotEmpty(result.MarkdownReport);
    }

    [Fact]
    public async Task ServiceAutoApprovesValidArtifactsWhenRequested()
    {
        var result = new GeneratorPlanDraftArtifactApprovalService().CreateSnapshot(
            await ProductionResultAsync(),
            new GeneratorPlanDraftArtifactApprovalRequest { AutoApproveValidArtifacts = true });

        Assert.Equal(GeneratorPlanDraftArtifactStagingStatus.ReadyForPackage, result.Status);
        Assert.Equal(GeneratorPlanDraftArtifactApprovalItemState.Approved, Assert.Single(result.Snapshot.Items).State);
    }

    [Fact]
    public async Task ServiceAppliesExplicitApproveRejectRepairDecisions()
    {
        var approved = Artifact("artifact/approved");
        var rejected = Artifact("artifact/rejected");
        var blocked = Artifact("artifact/blocked") with
        {
            State = GeneratorPlanProducedDraftArtifactState.Blocked,
            RepairRequestId = "repair/blocked"
        };

        var result = new GeneratorPlanDraftArtifactApprovalService().CreateSnapshot(
            await ProductionResultAsync(approved, rejected, blocked),
            new GeneratorPlanDraftArtifactApprovalRequest
            {
                Decisions =
                [
                    new GeneratorPlanDraftArtifactApprovalDecision { ArtifactId = "artifact/approved", Decision = GeneratorPlanDraftArtifactApprovalDecisionKind.Approved },
                    new GeneratorPlanDraftArtifactApprovalDecision { ArtifactId = "artifact/rejected", Decision = GeneratorPlanDraftArtifactApprovalDecisionKind.Rejected, ReasonCode = "out_of_scope" },
                    new GeneratorPlanDraftArtifactApprovalDecision { ArtifactId = "artifact/blocked", Decision = GeneratorPlanDraftArtifactApprovalDecisionKind.RepairRequested, ReasonCode = "blocked_json" }
                ]
            });

        Assert.Equal(GeneratorPlanDraftArtifactStagingStatus.NeedsRepair, result.Status);
        Assert.Contains(result.Snapshot.Items, item => item.ArtifactId == "artifact/approved" && item.State == GeneratorPlanDraftArtifactApprovalItemState.Approved);
        Assert.Contains(result.Snapshot.Items, item => item.ArtifactId == "artifact/rejected" && item.State == GeneratorPlanDraftArtifactApprovalItemState.Rejected);
        Assert.Contains(result.Snapshot.Items, item => item.ArtifactId == "artifact/blocked" && item.State == GeneratorPlanDraftArtifactApprovalItemState.RepairRequested);
    }

    [Fact]
    public async Task ServiceKeepsBlockedArtifactsBlockedWithoutDecision()
    {
        var result = new GeneratorPlanDraftArtifactApprovalService().CreateSnapshot(
            await ProductionResultAsync(Artifact("artifact/blocked") with
            {
                State = GeneratorPlanProducedDraftArtifactState.Blocked,
                RepairRequestId = "repair/blocked"
            }),
            new GeneratorPlanDraftArtifactApprovalRequest());

        Assert.Equal(GeneratorPlanDraftArtifactStagingStatus.NeedsRepair, result.Status);
        Assert.Equal(GeneratorPlanDraftArtifactApprovalItemState.Blocked, Assert.Single(result.Snapshot.Items).State);
    }

    [Fact]
    public async Task ServiceCreatesSnapshotFromExamplePathAndCanSkipMarkdownRendering()
    {
        using var temp = new TempDirectory();
        var path = WriteExample(temp.Path);

        var result = await new GeneratorPlanDraftArtifactApprovalService()
            .CreateSnapshotFromExampleAsync(path, new GeneratorPlanDraftArtifactApprovalRequest { RenderMarkdown = false, AutoApproveValidArtifacts = true }, CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Equal("example/test/v1", result.Snapshot.SourcePreviewExampleId);
        Assert.Empty(result.MarkdownReport);
        Assert.Single(result.Snapshot.Items);
    }

    [Fact]
    public void ValidatorAndPolicyReportErrorsWarningsAndValidationResults()
    {
        var valid = new GeneratorPlanDraftArtifactApprovalValidator().Validate(Snapshot(Item("artifact/a") with { State = GeneratorPlanDraftArtifactApprovalItemState.Approved }));
        var noItems = new GeneratorPlanDraftArtifactApprovalValidator().Validate(new GeneratorPlanDraftArtifactStagingSnapshot { Id = "snapshot/empty" });
        var invalid = new GeneratorPlanDraftArtifactApprovalValidator().Validate(Snapshot(
            Item("artifact/a") with { ContentJson = "{ invalid" },
            Item("artifact/a") with { State = GeneratorPlanDraftArtifactApprovalItemState.Approved },
            Item("artifact/rejected") with { State = GeneratorPlanDraftArtifactApprovalItemState.Rejected, DecisionReasonCode = string.Empty },
            Item("artifact/repair") with { State = GeneratorPlanDraftArtifactApprovalItemState.RepairRequested, DecisionReasonCode = string.Empty },
            Item("artifact/blocked") with { State = GeneratorPlanDraftArtifactApprovalItemState.Blocked, RepairRequestId = string.Empty }));

        var validationResults = GeneratorPlanDraftArtifactApprovalPolicy.ToValidationResults("artifact/result",
        [
            new GeneratorPlanDraftArtifactApprovalDiagnostic { Severity = GeneratorPlanPreviewDiagnosticSeverity.Info, Code = "info", Message = "Info" },
            new GeneratorPlanDraftArtifactApprovalDiagnostic { Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning, Code = "warning", Message = "Warning" }
        ]);

        Assert.Equal(GeneratorPlanDraftArtifactApprovalValidationState.Valid, GeneratorPlanDraftArtifactApprovalPolicy.ToValidationState(valid.Summary));
        Assert.Contains(noItems.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactApprovalDiagnosticCodes.NoItems && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactApprovalDiagnosticCodes.DuplicateArtifactId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ItemInvalidJson && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactApprovalDiagnosticCodes.RejectedItemMissingReason && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactApprovalDiagnosticCodes.RepairRequestedMissingReason && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactApprovalDiagnosticCodes.BlockedItemWithoutRepairRequest && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.DoesNotContain(validationResults, result => result.Severity == GeneratorPlanPreviewDiagnosticSeverity.Info);
        Assert.Single(validationResults);
    }

    [Fact]
    public void MarkdownRendererRendersWorklistApprovedSetRejectedRepairDiagnosticsAndEscapesCells()
    {
        var snapshot = Snapshot(
            Item("artifact|approved") with { State = GeneratorPlanDraftArtifactApprovalItemState.Approved },
            Item("artifact/rejected") with { State = GeneratorPlanDraftArtifactApprovalItemState.Rejected, DecisionReasonCode = "bad|fit", DecisionComment = "Line 1\nLine 2" }) with
        {
            Diagnostics =
            [
                new GeneratorPlanDraftArtifactApprovalDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "code|pipe",
                    ArtifactId = "artifact|approved",
                    Target = "content_json",
                    Message = "Line 1\nLine 2"
                }
            ]
        };
        snapshot = snapshot with
        {
            Summary = GeneratorPlanDraftArtifactApprovalPolicy.BuildSummary(snapshot, snapshot.Diagnostics)
        };

        var markdown = new GeneratorPlanDraftArtifactApprovalMarkdownRenderer().Render(snapshot);

        Assert.Contains("# Draft Artifact Approval/Staging", markdown);
        Assert.Contains("| State | Artifact ID | Kind | Contract | Requires approval | Repair request | Reason |", markdown);
        Assert.Contains("## Approved Artifact Set", markdown);
        Assert.Contains("## Rejected / Repair", markdown);
        Assert.Contains("## Approved JSON Preview", markdown);
        Assert.Contains("artifact\\|approved", markdown);
        Assert.Contains("Line 1<br>Line 2", markdown);
    }

    [Fact]
    public void MarkdownRendererTruncatesLongJsonPreview()
    {
        var snapshot = Snapshot(Item("artifact/a") with
        {
            State = GeneratorPlanDraftArtifactApprovalItemState.Approved,
            ContentJson = "{\"artifact_id\":\"artifact/a\",\"value\":\"" + new string('x', 2000) + "\"}"
        });
        snapshot = snapshot with { Summary = GeneratorPlanDraftArtifactApprovalPolicy.BuildSummary(snapshot, snapshot.Diagnostics) };

        var markdown = new GeneratorPlanDraftArtifactApprovalMarkdownRenderer().Render(snapshot);

        Assert.Contains("\n...", markdown);
    }

    [Fact]
    public async Task ArtifactServiceSavesStagingMarkdownApprovedSetValidationAndIsIdempotent()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = CreateArtifactService(database);
        var approval = new GeneratorPlanDraftArtifactApprovalService().CreateSnapshot(
            await ProductionResultAsync(),
            new GeneratorPlanDraftArtifactApprovalRequest { AutoApproveValidArtifacts = true });

        var first = await service.SaveAsync(approval, new GeneratorPlanDraftArtifactApprovalArtifactSaveRequest(), CancellationToken.None);
        var second = await service.SaveAsync(approval, new GeneratorPlanDraftArtifactApprovalArtifactSaveRequest(), CancellationToken.None);
        var artifacts = await database.ListGeneratedArtifactsAsync(CancellationToken.None);

        Assert.NotNull(first.MarkdownArtifact);
        Assert.Equal(first.StagingArtifact.Id, second.StagingArtifact.Id);
        Assert.Equal(3, artifacts.Count);
        Assert.NotNull(await database.GetGeneratedArtifactByIdAsync(GeneratorPlanDraftArtifactApprovalArtifactIds.ApprovedArtifactSetArtifactId, CancellationToken.None));
        Assert.Equal(first.ValidationResults.Count, (await database.ListValidationResultsByArtifactAsync(first.StagingArtifact.Id, CancellationToken.None)).Count);

        using var approvedSet = JsonDocument.Parse(first.ApprovedArtifactSetArtifact.Json);
        Assert.True(approvedSet.RootElement.GetProperty("approved_artifacts")[0].GetProperty("content_json").ValueKind == JsonValueKind.Object);
    }

    [Fact]
    public async Task ArtifactServiceSupportsCustomArtifactIdsAndSkipsMarkdownWhenDisabled()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = CreateArtifactService(database);
        var approval = new GeneratorPlanDraftArtifactApprovalService().CreateSnapshot(
            await ProductionResultAsync(),
            new GeneratorPlanDraftArtifactApprovalRequest { RenderMarkdown = false, AutoApproveValidArtifacts = true });

        var result = await service.SaveAsync(approval, new GeneratorPlanDraftArtifactApprovalArtifactSaveRequest
        {
            StagingArtifactId = "artifact/custom/staging",
            MarkdownArtifactId = "artifact/custom/markdown",
            ApprovedArtifactSetArtifactId = "artifact/custom/approved",
            GeneratedBy = "test"
        }, CancellationToken.None);

        Assert.Equal("artifact/custom/staging", result.StagingArtifact.Id);
        Assert.Equal("artifact/custom/approved", result.ApprovedArtifactSetArtifact.Id);
        Assert.Null(result.MarkdownArtifact);
    }

    [Fact]
    public async Task ReaderReturnsEmptySavedArtifactsValidationWorklistAndWorksWhenMarkdownMissing()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var reader = new GeneratorPlanDraftArtifactApprovalArtifactReader(database);
        var missing = await reader.ReadLatestAsync(CancellationToken.None);
        var approval = new GeneratorPlanDraftArtifactApprovalService().CreateSnapshot(
            await ProductionResultAsync(),
            new GeneratorPlanDraftArtifactApprovalRequest { RenderMarkdown = false, AutoApproveValidArtifacts = true });

        await CreateArtifactService(database).SaveAsync(approval, new GeneratorPlanDraftArtifactApprovalArtifactSaveRequest(), CancellationToken.None);
        var loaded = await reader.ReadLatestAsync(CancellationToken.None);

        Assert.False(missing.Exists);
        Assert.True(loaded.Exists);
        Assert.NotNull(loaded.StagingArtifact);
        Assert.Null(loaded.MarkdownArtifact);
        Assert.NotNull(loaded.ApprovedArtifactSetArtifact);
        Assert.Single(loaded.Worklist);
        Assert.Equal(GeneratorPlanDraftArtifactApprovalItemState.Approved, loaded.Worklist[0].State);
    }

    private static async Task<GeneratorPlanDraftArtifactProductionResult> ProductionResultAsync(params GeneratorPlanProducedDraftArtifact[] artifacts)
    {
        var batch = new GeneratorPlanDraftArtifactProductionBatch
        {
            Id = "batch/test",
            SourceQueueId = "queue/test",
            SourceDraftExecutionPlanId = "draft/test",
            SourcePreviewExampleId = "example/test/v1",
            SourcePath = "plan.example.json",
            Artifacts = artifacts.Length == 0 ? [Artifact("artifact/a")] : artifacts
        };
        batch = new GeneratorPlanDraftArtifactProductionValidator().Validate(batch);

        return await Task.FromResult(new GeneratorPlanDraftArtifactProductionResult
        {
            Ok = batch.Status != GeneratorPlanDraftArtifactProductionStatus.Invalid,
            Status = batch.Status,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            Batch = batch,
            Diagnostics = batch.Diagnostics
        });
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

    private static GeneratorPlanDraftArtifactStagingSnapshot Snapshot(params GeneratorPlanDraftArtifactApprovalItem[] items)
    {
        var snapshot = new GeneratorPlanDraftArtifactStagingSnapshot
        {
            Id = "snapshot/test",
            SourceProductionBatchId = "batch/test",
            SourcePreviewExampleId = "example/test/v1",
            SourcePath = "plan.example.json",
            Items = items
        };

        return new GeneratorPlanDraftArtifactApprovalValidator().Validate(snapshot);
    }

    private static GeneratorPlanDraftArtifactApprovalItem Item(string artifactId)
    {
        return new GeneratorPlanDraftArtifactApprovalItem
        {
            ArtifactId = artifactId,
            ArtifactKind = "game_profile_v1",
            State = GeneratorPlanDraftArtifactApprovalItemState.Pending,
            SourceProductionBatchId = "batch/test",
            QueueItemId = "queue/test/item/1",
            SourceExecutionStepId = "step/test",
            ExpectedArtifactContract = "game_profile_v1",
            ContentJson = $$"""{"schema_version":"0.1","artifact_id":"{{artifactId}}"}""",
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

    private static GeneratorPlanDraftArtifactApprovalArtifactService CreateArtifactService(GeneratedArtifactRepository repository)
    {
        return new GeneratorPlanDraftArtifactApprovalArtifactService(
            new GeneratorPlanDraftArtifactApprovalService(),
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
