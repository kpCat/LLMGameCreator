using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanDraftArtifactReviewServiceTests
{
    [Fact]
    public async Task LoadLatestReturnsMissingWhenNoStagingArtifact()
    {
        using var temp = new TempDirectory();
        var service = await CreateReviewServiceAsync(temp.Path);

        var result = await service.LoadLatestAsync(CancellationToken.None);

        Assert.False(result.Exists);
        Assert.Contains("No draft artifact staging snapshot", result.Message);
    }

    [Fact]
    public async Task CaptureReviewFromExampleCreatesPendingItems()
    {
        using var temp = new TempDirectory();
        var service = await CreateReviewServiceAsync(temp.Path);
        var example = WriteExample(temp.Path);

        var captured = await service.CaptureReviewFromExampleAsync(example, true, CancellationToken.None);

        Assert.True(captured.ApprovalResult.Ok);
        Assert.Equal(GeneratorPlanDraftArtifactStagingStatus.NeedsReview, captured.ApprovalResult.Status);
        Assert.All(captured.ApprovalResult.Snapshot.Items, item => Assert.Equal(GeneratorPlanDraftArtifactApprovalItemState.Pending, item.State));
        Assert.NotNull(captured.MarkdownArtifact);
    }

    [Fact]
    public async Task ApplyDecisionsApprovesSelectedAndUpdatesApprovedArtifactSet()
    {
        using var temp = new TempDirectory();
        var service = await CreateReviewServiceAsync(temp.Path);
        await service.CaptureReviewFromExampleAsync(WriteExample(temp.Path), true, CancellationToken.None);
        var latest = await service.LoadLatestAsync(CancellationToken.None);
        var artifactId = latest.Snapshot.Items[0].ArtifactId;

        var result = await service.ApplyDecisionsToLatestAsync(Request(artifactId, GeneratorPlanDraftArtifactApprovalDecisionKind.Approved), CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Contains(result.Snapshot.Items, item => item.ArtifactId == artifactId && item.State == GeneratorPlanDraftArtifactApprovalItemState.Approved);
        Assert.True(ApprovedSetContains(result.ApprovedArtifactSetArtifact.Json, artifactId));
    }

    [Fact]
    public async Task ApplyDecisionsRejectsSelectedAndExcludesFromApprovedSet()
    {
        using var temp = new TempDirectory();
        var service = await CreateReviewServiceAsync(temp.Path);
        await service.CaptureReviewFromExampleAsync(WriteExample(temp.Path), true, CancellationToken.None);
        var latest = await service.LoadLatestAsync(CancellationToken.None);
        var artifactId = latest.Snapshot.Items[0].ArtifactId;

        var result = await service.ApplyDecisionsToLatestAsync(Request(artifactId, GeneratorPlanDraftArtifactApprovalDecisionKind.Rejected), CancellationToken.None);

        Assert.Contains(result.Snapshot.Items, item => item.ArtifactId == artifactId && item.State == GeneratorPlanDraftArtifactApprovalItemState.Rejected);
        Assert.False(ApprovedSetContains(result.ApprovedArtifactSetArtifact.Json, artifactId));
    }

    [Fact]
    public async Task ApplyDecisionsRequestsRepairAndExcludesFromApprovedSet()
    {
        using var temp = new TempDirectory();
        var service = await CreateReviewServiceAsync(temp.Path);
        await service.CaptureReviewFromExampleAsync(WriteExample(temp.Path), true, CancellationToken.None);
        var latest = await service.LoadLatestAsync(CancellationToken.None);
        var artifactId = latest.Snapshot.Items[0].ArtifactId;

        var result = await service.ApplyDecisionsToLatestAsync(Request(artifactId, GeneratorPlanDraftArtifactApprovalDecisionKind.RepairRequested), CancellationToken.None);

        Assert.Contains(result.Snapshot.Items, item => item.ArtifactId == artifactId && item.State == GeneratorPlanDraftArtifactApprovalItemState.RepairRequested);
        Assert.False(ApprovedSetContains(result.ApprovedArtifactSetArtifact.Json, artifactId));
    }

    [Fact]
    public async Task ApproveAllValidPendingDoesNotApproveBlockedOrInvalidItems()
    {
        using var temp = new TempDirectory();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        await SaveSnapshotAsync(database, Snapshot(
            Item("artifact/valid"),
            Item("artifact/blocked") with { State = GeneratorPlanDraftArtifactApprovalItemState.Blocked, RepairRequestId = "repair/blocked" },
            Item("artifact/invalid") with { ContentJson = "{ invalid" }));
        var service = CreateReviewService(database);

        var result = await service.ApplyDecisionsToLatestAsync(new GeneratorPlanDraftArtifactReviewDecisionRequest
        {
            Decisions =
            [
                Decision("artifact/valid", GeneratorPlanDraftArtifactApprovalDecisionKind.Approved),
                Decision("artifact/blocked", GeneratorPlanDraftArtifactApprovalDecisionKind.Approved),
                Decision("artifact/invalid", GeneratorPlanDraftArtifactApprovalDecisionKind.Approved)
            ]
        }, CancellationToken.None);

        Assert.Contains(result.Snapshot.Items, item => item.ArtifactId == "artifact/valid" && item.State == GeneratorPlanDraftArtifactApprovalItemState.Approved);
        Assert.Contains(result.Snapshot.Items, item => item.ArtifactId == "artifact/blocked" && item.State == GeneratorPlanDraftArtifactApprovalItemState.Blocked);
        Assert.Contains(result.Snapshot.Items, item => item.ArtifactId == "artifact/invalid" && item.State == GeneratorPlanDraftArtifactApprovalItemState.Pending);
        Assert.True(ApprovedSetContains(result.ApprovedArtifactSetArtifact.Json, "artifact/valid"));
        Assert.False(ApprovedSetContains(result.ApprovedArtifactSetArtifact.Json, "artifact/blocked"));
        Assert.False(ApprovedSetContains(result.ApprovedArtifactSetArtifact.Json, "artifact/invalid"));
    }

    [Fact]
    public async Task ApplyDecisionsPersistsDecisionReasonAndComment()
    {
        using var temp = new TempDirectory();
        var service = await CreateReviewServiceAsync(temp.Path);
        await service.CaptureReviewFromExampleAsync(WriteExample(temp.Path), true, CancellationToken.None);
        var latest = await service.LoadLatestAsync(CancellationToken.None);
        var artifactId = latest.Snapshot.Items[0].ArtifactId;

        await service.ApplyDecisionsToLatestAsync(Request(artifactId, GeneratorPlanDraftArtifactApprovalDecisionKind.Rejected), CancellationToken.None);
        var reloaded = await service.LoadLatestAsync(CancellationToken.None);
        var item = reloaded.Snapshot.Items.Single(item => item.ArtifactId == artifactId);

        Assert.Equal("review_reason", item.DecisionReasonCode);
        Assert.Equal("Review comment.", item.DecisionComment);
        Assert.NotEqual(default, item.DecidedAtUtc);
    }

    private static GeneratorPlanDraftArtifactReviewDecisionRequest Request(string artifactId, string decision)
    {
        return new GeneratorPlanDraftArtifactReviewDecisionRequest
        {
            Decisions = [Decision(artifactId, decision)]
        };
    }

    private static GeneratorPlanDraftArtifactApprovalDecision Decision(string artifactId, string decision)
    {
        return new GeneratorPlanDraftArtifactApprovalDecision
        {
            ArtifactId = artifactId,
            Decision = decision,
            ReasonCode = "review_reason",
            Comment = "Review comment."
        };
    }

    private static async Task SaveSnapshotAsync(SqliteDesignDatabase database, GeneratorPlanDraftArtifactStagingSnapshot snapshot)
    {
        var validated = new GeneratorPlanDraftArtifactApprovalValidator().Validate(snapshot);
        await new GeneratorPlanDraftArtifactApprovalArtifactService(new GeneratorPlanDraftArtifactApprovalService(), database)
            .SaveAsync(new GeneratorPlanDraftArtifactApprovalResult
            {
                Ok = validated.Status != GeneratorPlanDraftArtifactStagingStatus.Invalid,
                Status = validated.Status,
                GeneratedAtUtc = DateTimeOffset.UtcNow,
                Snapshot = validated,
                Diagnostics = validated.Diagnostics
            }, new GeneratorPlanDraftArtifactApprovalArtifactSaveRequest(), CancellationToken.None);
    }

    private static GeneratorPlanDraftArtifactStagingSnapshot Snapshot(params GeneratorPlanDraftArtifactApprovalItem[] items)
    {
        return new GeneratorPlanDraftArtifactStagingSnapshot
        {
            Id = "snapshot/test",
            SourceProductionBatchId = "batch/test",
            SourcePreviewExampleId = "example/test/v1",
            SourcePath = "plan.example.json",
            Items = items
        };
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

    private static bool ApprovedSetContains(string json, string artifactId)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement
            .GetProperty("approved_artifacts")
            .EnumerateArray()
            .Any(element => element.GetProperty("artifact_id").GetString() == artifactId);
    }

    private static string WriteExample(string root)
    {
        var path = Path.Combine(root, "plan.example.json");
        File.WriteAllText(path, """
        {
          "schema_version": "0.1",
          "example_id": "example/review/v1",
          "title": "Review Generator Plan",
          "purpose": "Test artifact review.",
          "source_profile": {
            "id": "profile/review/v1"
          },
          "selected_feature_bundles": [
            "feature_bundle/review/v1"
          ],
          "target_artifacts": [
            "game_profile_v1",
            "scene_pack_v1"
          ],
          "steps": [
            {
              "id": "step/profile",
              "order": 1,
              "title": "Profile",
              "producer_role": "role/designer_llm/v1",
              "context_pack_template": "context_template/design_discussion/v1",
              "expected_artifact_contract": "game_profile_v1",
              "inputs": ["game_profile_v1"],
              "validation_gates": ["validation.level_0_json_shape"],
              "on_success": "stage_profile",
              "on_failure": "repair_profile"
            },
            {
              "id": "step/scene",
              "order": 2,
              "title": "Scene",
              "producer_role": "role/designer_llm/v1",
              "context_pack_template": "context_template/design_discussion/v1",
              "expected_artifact_contract": "scene_pack_v1",
              "inputs": ["scene_pack_v1"],
              "validation_gates": ["validation.level_0_json_shape"],
              "on_success": "stage_scene",
              "on_failure": "repair_scene"
            }
          ]
        }
        """);
        return path;
    }

    private static async Task<GeneratorPlanDraftArtifactReviewService> CreateReviewServiceAsync(string root)
    {
        return CreateReviewService(await CreateInitializedDatabaseAsync(root));
    }

    private static GeneratorPlanDraftArtifactReviewService CreateReviewService(SqliteDesignDatabase database)
    {
        return new GeneratorPlanDraftArtifactReviewService(
            new GeneratorPlanDraftArtifactApprovalArtifactReader(database),
            new GeneratorPlanDraftArtifactApprovalArtifactService(new GeneratorPlanDraftArtifactApprovalService(), database),
            new GeneratorPlanDraftArtifactApprovalValidator(),
            new GeneratorPlanDraftArtifactApprovalMarkdownRenderer());
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
