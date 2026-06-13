using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;
using GeneratedArtifactRepository = LLMGameCreator.Application.Design.IGeneratedArtifactRepository;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanDraftExecutionPipelineTests
{
    [Fact]
    public void PlannerCreatesDeterministicDraftPlanStepIdsArtifactsAndApproval()
    {
        var preview = CompletePreview();

        var plan = new GeneratorPlanDraftExecutionPlanner().CreateDraftPlan(preview);

        Assert.Equal("draft_execution/example/test/v1", plan.Id);
        Assert.Equal(GeneratorPlanDraftExecutionStatus.Ready, plan.Status);
        Assert.Equal("draft_execution/example/test/v1/step/step_profile_summary", Assert.Single(plan.Steps).Id);
        Assert.Equal("artifact/draft_execution/draft_execution/example/test/v1/step/1/game_profile_v1", plan.Steps[0].PlannedArtifactId);
        Assert.Equal("game_profile_v1", plan.Steps[0].PlannedArtifactKind);
        Assert.Equal("repair/draft_execution/draft_execution/example/test/v1/step/1", plan.Steps[0].RepairRequestId);
        Assert.True(plan.Steps[0].RequiresHumanApproval);
        Assert.Equal(1, plan.Summary.PlannedArtifactCount);
    }

    [Fact]
    public void PlannerMarksStepsBlockedWhenExecutionContractsAreMissingAndCanDisableApproval()
    {
        var preview = CompletePreview() with
        {
            Steps =
            [
                CompleteStep() with { ExpectedArtifactContract = string.Empty },
                CompleteStep() with { Id = "step/no_gates", Order = 2, ValidationGates = Array.Empty<string>() }
            ]
        };

        var plan = new GeneratorPlanDraftExecutionPlanner().CreateDraftPlan(
            preview,
            new GeneratorPlanDraftExecutionPlannerOptions { RequireHumanApprovalByDefault = false });

        Assert.Equal(GeneratorPlanDraftExecutionStatus.Blocked, plan.Status);
        Assert.All(plan.Steps, step => Assert.Equal(GeneratorPlanDraftExecutionStepState.Blocked, step.State));
        Assert.All(plan.Steps, step => Assert.False(step.RequiresHumanApproval));
        Assert.Equal("unknown", plan.Steps[0].PlannedArtifactKind);
        Assert.Equal(2, plan.Summary.BlockedStepCount);
    }

    [Fact]
    public void ValidatorAcceptsValidPlanAndReportsErrorsWarningsDeterministically()
    {
        var valid = new GeneratorPlanDraftExecutionValidator()
            .Validate(new GeneratorPlanDraftExecutionPlanner().CreateDraftPlan(CompletePreview()));

        var invalid = new GeneratorPlanDraftExecutionValidator().Validate(new GeneratorPlanDraftExecutionPlan
        {
            Id = string.Empty,
            Steps =
            [
                new GeneratorPlanDraftExecutionStep
                {
                    Id = "step/a",
                    PlannedArtifactId = "artifact/a"
                },
                new GeneratorPlanDraftExecutionStep
                {
                    Id = "step/a",
                    SourcePreviewStepId = "step/source",
                    PlannedArtifactId = "artifact/a",
                    ProducerRole = "role/test",
                    ExpectedArtifactContract = "contract/test",
                    ValidationGates = ["gate/test"]
                }
            ]
        });

        Assert.Equal(GeneratorPlanDraftExecutionValidationState.Valid, GeneratorPlanDraftExecutionPolicy.ToValidationState(valid.Summary));
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftExecutionDiagnosticCodes.MissingPlanId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftExecutionDiagnosticCodes.DuplicateStepId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftExecutionDiagnosticCodes.DuplicatePlannedArtifactId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftExecutionDiagnosticCodes.StepMissingSourcePreviewStepId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftExecutionDiagnosticCodes.StepMissingProducerRole && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Equal(GeneratorPlanDraftExecutionValidationState.Invalid, GeneratorPlanDraftExecutionPolicy.ToValidationState(invalid.Summary));
    }

    [Fact]
    public void ValidatorReportsNoStepsAndPolicyMapsWarningsOnly()
    {
        var noSteps = new GeneratorPlanDraftExecutionValidator().Validate(new GeneratorPlanDraftExecutionPlan { Id = "plan/empty" });
        var warnings = new GeneratorPlanDraftExecutionValidator().Validate(new GeneratorPlanDraftExecutionPlan
        {
            Id = "plan/warnings",
            Steps =
            [
                new GeneratorPlanDraftExecutionStep
                {
                    Id = "step/warning",
                    SourcePreviewStepId = "step/warning",
                    PlannedArtifactId = "artifact/warning"
                }
            ],
            Diagnostics =
            [
                new GeneratorPlanDraftExecutionDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Info,
                    Code = "info",
                    Message = "Info"
                }
            ]
        });

        var validationResults = GeneratorPlanDraftExecutionPolicy.ToValidationResults("artifact/result", warnings.Diagnostics);

        Assert.Contains(noSteps.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftExecutionDiagnosticCodes.NoSteps && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Equal(GeneratorPlanDraftExecutionValidationState.Warnings, GeneratorPlanDraftExecutionPolicy.ToValidationState(warnings.Summary));
        Assert.DoesNotContain(validationResults, result => result.Severity == GeneratorPlanPreviewDiagnosticSeverity.Info);
        Assert.All(validationResults, result => Assert.Equal("artifact/result", result.ArtifactId));
    }

    [Fact]
    public void MarkdownRendererRendersSummaryStepsDiagnosticsAndEscapesCells()
    {
        var plan = new GeneratorPlanDraftExecutionPlanner().CreateDraftPlan(CompletePreview() with
        {
            Title = "Title|Pipe\nLine"
        }) with
        {
            Diagnostics =
            [
                new GeneratorPlanDraftExecutionDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "code|pipe",
                    StepId = "step/a",
                    Target = "target|pipe",
                    Message = "Line 1\nLine 2"
                }
            ]
        };
        plan = plan with
        {
            Summary = new GeneratorPlanDraftExecutionSummary
            {
                StepCount = plan.Steps.Count,
                PlannedArtifactCount = plan.Steps.Count,
                RepairRequestCount = plan.Steps.Count,
                WarningCount = 1
            }
        };

        var markdown = new GeneratorPlanDraftExecutionMarkdownRenderer().Render(plan);
        var empty = new GeneratorPlanDraftExecutionMarkdownRenderer().Render(new GeneratorPlanDraftExecutionPlan());

        Assert.Contains("# Generator Plan Draft Execution", markdown);
        Assert.Contains("| Order | State | Step ID | Preview Step | Producer | Expected Artifact | Planned Artifact | Gates | Approval |", markdown);
        Assert.Contains("code\\|pipe", markdown);
        Assert.Contains("target\\|pipe", markdown);
        Assert.Contains("Line 1<br>Line 2", markdown);
        Assert.Contains("_No draft execution steps were reported._", empty);
        Assert.Contains("_No diagnostics were reported._", empty);
    }

    [Fact]
    public async Task ServiceCreatesDraftFromPreviewAndExamplePathAndCanSkipMarkdown()
    {
        using var temp = new TempDirectory();
        var path = WriteExample(temp.Path);
        var preview = await new GeneratorPlanPreviewService().PreviewAsync(new GeneratorPlanPreviewRequest { SourcePath = path }, CancellationToken.None);
        var service = new GeneratorPlanDraftExecutionService();

        var fromPreview = await service.CreateDraftAsync(preview, new GeneratorPlanDraftExecutionRequest(), CancellationToken.None);
        var fromExample = await service.CreateDraftFromExampleAsync(path, new GeneratorPlanDraftExecutionRequest { PlanId = "draft_execution/custom" }, CancellationToken.None);
        var noMarkdown = await service.CreateDraftAsync(preview, new GeneratorPlanDraftExecutionRequest { RenderMarkdown = false }, CancellationToken.None);

        Assert.True(fromPreview.Ok);
        Assert.Equal(GeneratorPlanDraftExecutionStatus.Ready, fromPreview.Status);
        Assert.NotEmpty(fromPreview.MarkdownReport);
        Assert.Equal("draft_execution/custom", fromExample.Plan.Id);
        Assert.Empty(noMarkdown.MarkdownReport);
    }

    [Fact]
    public async Task ServiceReturnsInvalidWhenPreviewInvalid()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "broken.example.json");
        await File.WriteAllTextAsync(path, "{ invalid", CancellationToken.None);

        var result = await new GeneratorPlanDraftExecutionService()
            .CreateDraftFromExampleAsync(path, new GeneratorPlanDraftExecutionRequest(), CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Equal(GeneratorPlanDraftExecutionStatus.Invalid, result.Status);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanDraftExecutionDiagnosticCodes.PreviewDiagnostic);
    }

    [Fact]
    public async Task ArtifactServiceSavesResultMarkdownValidationAndSupportsIdempotenceCustomIdsAndNoMarkdown()
    {
        using var temp = new TempDirectory();
        var path = WriteExample(temp.Path, stepsJson: """
        [
          {
            "id": "step/warning",
            "order": 1,
            "title": "Warning"
          }
        ]
        """);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = CreateArtifactService(database);
        var request = new GeneratorPlanDraftExecutionArtifactRequest
        {
            PreviewRequest = new GeneratorPlanPreviewRequest { SourcePath = path }
        };

        var first = await service.CaptureAsync(request, CancellationToken.None);
        var second = await service.CaptureAsync(request, CancellationToken.None);
        var custom = await service.CaptureAsync(new GeneratorPlanDraftExecutionArtifactRequest
        {
            PreviewRequest = new GeneratorPlanPreviewRequest { SourcePath = path },
            DraftRequest = new GeneratorPlanDraftExecutionRequest { RenderMarkdown = false },
            ResultArtifactId = "artifact/generator_plan_draft_execution/custom",
            MarkdownArtifactId = "artifact/generator_plan_draft_execution_markdown/custom",
            GeneratedBy = "test"
        }, CancellationToken.None);

        Assert.Equal(GeneratorPlanDraftExecutionValidationState.Warnings, first.ResultArtifact.ValidationState);
        Assert.NotNull(first.MarkdownArtifact);
        Assert.Contains(first.ValidationResults, result => result.Code == GeneratorPlanDraftExecutionDiagnosticCodes.StepMissingValidationGates);
        Assert.Equal(first.ResultArtifact.Id, second.ResultArtifact.Id);
        Assert.Equal(first.ValidationResults.Count, (await database.ListValidationResultsByArtifactAsync(first.ResultArtifact.Id, CancellationToken.None)).Count);
        Assert.Null(custom.MarkdownArtifact);
        Assert.Equal("artifact/generator_plan_draft_execution/custom", custom.ResultArtifact.Id);
    }

    [Fact]
    public async Task ReaderReturnsEmptySavedArtifactsAndWorksWhenMarkdownMissing()
    {
        using var temp = new TempDirectory();
        var path = WriteExample(temp.Path);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var reader = new GeneratorPlanDraftExecutionArtifactReader(database);

        var missing = await reader.ReadLatestAsync(CancellationToken.None);
        await CreateArtifactService(database).CaptureAsync(new GeneratorPlanDraftExecutionArtifactRequest
        {
            PreviewRequest = new GeneratorPlanPreviewRequest { SourcePath = path },
            DraftRequest = new GeneratorPlanDraftExecutionRequest { RenderMarkdown = false }
        }, CancellationToken.None);
        var loaded = await reader.ReadLatestAsync(CancellationToken.None);

        Assert.False(missing.Exists);
        Assert.True(loaded.Exists);
        Assert.NotNull(loaded.ResultArtifact);
        Assert.Null(loaded.MarkdownArtifact);
        Assert.Empty(loaded.ValidationResults);
    }

    private static GeneratorPlanPreview CompletePreview()
    {
        return new GeneratorPlanPreview
        {
            SourcePath = "plan.example.json",
            ExampleId = "example/test/v1",
            Title = "Test",
            Purpose = "Preview test.",
            SourceProfileId = "profile/test/v1",
            SelectedFeatureBundles = ["feature_bundle/test/v1"],
            TargetArtifacts = ["game_profile_v1"],
            Steps = [CompleteStep()],
            Summary = new GeneratorPlanPreviewSummary
            {
                StepCount = 1,
                TargetArtifactCount = 1,
                FeatureBundleCount = 1
            }
        };
    }

    private static GeneratorPlanPreviewStep CompleteStep()
    {
        return new GeneratorPlanPreviewStep
        {
            Id = "step/profile_summary",
            Order = 1,
            Title = "Profile summary",
            ProducerRole = "role/designer_llm/v1",
            ContextPackTemplate = "context_template/design_discussion/v1",
            ExpectedArtifactContract = "game_profile_v1",
            Inputs = ["game_profile_v1"],
            ValidationGates = ["validation.level_0_json_shape"]
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

    private static GeneratorPlanDraftExecutionArtifactService CreateArtifactService(GeneratedArtifactRepository repository)
    {
        var previewService = new GeneratorPlanPreviewService();
        return new GeneratorPlanDraftExecutionArtifactService(
            previewService,
            new GeneratorPlanDraftExecutionService(),
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
