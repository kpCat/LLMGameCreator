using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanPreviewPipelineTests
{
    [Fact]
    public async Task LoaderLoadsValidExampleAndOrdersStepsDeterministically()
    {
        using var temp = new AtlasTempDirectory();
        var path = WriteExample(temp.Path, stepsJson: """
        [
          {
            "id": "step/second",
            "order": 2,
            "title": "Second",
            "producer_role": "role/batch_generator_llm/v1",
            "context_pack_template": "context_template/strict_single_json_artifact/v1",
            "expected_artifact_contract": "semantic_pack_v1",
            "validation_gates": ["validation.level_0_json_shape"]
          },
          {
            "id": "step/first",
            "order": 1,
            "title": "First",
            "producer_role": "role/designer_llm/v1",
            "context_pack_template": "context_template/design_discussion/v1",
            "expected_artifact_contract": "game_profile_v1",
            "validation_gates": ["validation.level_1_ids_enums_refs"]
          }
        ]
        """);

        var preview = await new GeneratorPlanPreviewLoader().LoadAsync(path, CancellationToken.None);

        Assert.Equal("example/test/v1", preview.ExampleId);
        Assert.Equal(["step/first", "step/second"], preview.Steps.Select(step => step.Id).ToArray());
        Assert.Contains(preview.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanPreviewDiagnosticCodes.Loaded);
    }

    [Fact]
    public async Task LoaderReturnsInvalidJsonDiagnostic()
    {
        using var temp = new AtlasTempDirectory();
        var path = Path.Combine(temp.Path, "broken.example.json");
        await File.WriteAllTextAsync(path, "{ invalid", CancellationToken.None);

        var preview = await new GeneratorPlanPreviewLoader().LoadAsync(path, CancellationToken.None);

        Assert.Equal(GeneratorPlanPreviewDiagnosticCodes.InvalidJson, Assert.Single(preview.Diagnostics).Code);
        Assert.Equal(1, preview.Summary.ErrorCount);
    }

    [Fact]
    public async Task LoaderHandlesMissingOptionalFieldsWithoutThrowing()
    {
        using var temp = new AtlasTempDirectory();
        var path = Path.Combine(temp.Path, "minimal.example.json");
        await File.WriteAllTextAsync(path, """
        {
          "example_id": "example/minimal/v1",
          "title": "Minimal",
          "source_profile": { "id": "profile/minimal/v1" },
          "selected_feature_bundles": ["feature_bundle/minimal/v1"],
          "target_artifacts": ["game_profile_v1"],
          "steps": [{ "id": "step/minimal", "order": 1 }]
        }
        """, CancellationToken.None);

        var preview = await new GeneratorPlanPreviewLoader().LoadAsync(path, CancellationToken.None);

        Assert.Equal("example/minimal/v1", preview.ExampleId);
        Assert.Equal("step/minimal", Assert.Single(preview.Steps).Id);
        Assert.Empty(preview.Steps[0].ValidationGates);
    }

    [Fact]
    public void ValidatorReportsRequiredErrorsAndWarningsDeterministically()
    {
        var preview = new GeneratorPlanPreview
        {
            SourcePath = "plan.example.json",
            Steps =
            [
                new GeneratorPlanPreviewStep { Id = "step/a", Order = 1 },
                new GeneratorPlanPreviewStep { Id = "step/a", Order = 1 }
            ]
        };

        var result = new GeneratorPlanPreviewValidator().Validate(preview);

        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanPreviewDiagnosticCodes.MissingExampleId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanPreviewDiagnosticCodes.MissingTitle && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanPreviewDiagnosticCodes.StepDuplicateId && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanPreviewDiagnosticCodes.StepOrderDuplicate && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanPreviewDiagnosticCodes.StepMissingExpectedArtifactContract && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanPreviewDiagnosticCodes.StepMissingValidationGates && diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning);
    }

    [Fact]
    public void ValidatorAcceptsCompletePlanWithoutErrors()
    {
        var preview = CompletePreview();

        var result = new GeneratorPlanPreviewValidator().Validate(preview);

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
        Assert.Equal(1, result.Summary.StepCount);
        Assert.Equal(1, result.Summary.TargetArtifactCount);
        Assert.Equal(1, result.Summary.FeatureBundleCount);
    }

    [Fact]
    public void MarkdownRendererRendersSummaryStepsDiagnosticsAndEscapesCells()
    {
        var preview = CompletePreview() with
        {
            Title = "Title|Pipe\nLine",
            Diagnostics =
            [
                new GeneratorPlanPreviewDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = GeneratorPlanPreviewDiagnosticCodes.StepMissingValidationGates,
                    Path = "a|b.example.json",
                    StepId = "step/a",
                    Message = "Line 1\nLine 2"
                }
            ],
            Summary = new GeneratorPlanPreviewSummary
            {
                StepCount = 1,
                TargetArtifactCount = 1,
                FeatureBundleCount = 1,
                WarningCount = 1
            }
        };

        var markdown = new GeneratorPlanPreviewMarkdownRenderer().Render(preview);

        Assert.Contains("# Generator Plan Preview", markdown);
        Assert.Contains("Status: **WARNINGS**", markdown);
        Assert.Contains("Title\\|Pipe<br>Line", markdown);
        Assert.Contains("| Order | Step ID | Title | Producer | Expected artifact | Gates |", markdown);
        Assert.Contains("a\\|b.example.json", markdown);
        Assert.Contains("Line 1<br>Line 2", markdown);
    }

    [Fact]
    public void MarkdownRendererHandlesEmptySteps()
    {
        var markdown = new GeneratorPlanPreviewMarkdownRenderer().Render(new GeneratorPlanPreview());

        Assert.Contains("_No generator plan steps were reported._", markdown);
        Assert.Contains("_No diagnostics were reported._", markdown);
    }

    [Fact]
    public async Task PreviewServiceReturnsOkWarningsAndFailedWithoutPersistingArtifacts()
    {
        using var temp = new AtlasTempDirectory();
        var validPath = WriteExample(temp.Path);
        var warningPath = WriteExample(temp.Path, "warning.example.json", stepsJson: """
        [
          {
            "id": "step/warning",
            "order": 1,
            "title": "Warning"
          }
        ]
        """);
        var invalidPath = Path.Combine(temp.Path, "invalid.example.json");
        await File.WriteAllTextAsync(invalidPath, "{ invalid", CancellationToken.None);
        var service = new GeneratorPlanPreviewService();

        var valid = await service.PreviewAsync(new GeneratorPlanPreviewRequest { SourcePath = validPath }, CancellationToken.None);
        var warning = await service.PreviewAsync(new GeneratorPlanPreviewRequest { SourcePath = warningPath }, CancellationToken.None);
        var invalid = await service.PreviewAsync(new GeneratorPlanPreviewRequest { SourcePath = invalidPath }, CancellationToken.None);

        Assert.True(valid.Ok);
        Assert.Equal(GeneratorPlanPreviewValidationState.Valid, valid.Status);
        Assert.NotEmpty(valid.MarkdownReport);
        Assert.True(warning.Ok);
        Assert.Equal(GeneratorPlanPreviewValidationState.Warnings, warning.Status);
        Assert.False(invalid.Ok);
        Assert.Equal(GeneratorPlanPreviewValidationState.Invalid, invalid.Status);
    }

    [Fact]
    public async Task ArtifactServiceSavesArtifactsValidationResultsAndSupportsIdempotenceCustomIdsAndNoMarkdown()
    {
        using var temp = new AtlasTempDirectory();
        var warningPath = WriteExample(temp.Path, stepsJson: """
        [
          {
            "id": "step/warning",
            "order": 1,
            "title": "Warning"
          }
        ]
        """);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = new GeneratorPlanPreviewArtifactService(new GeneratorPlanPreviewService(), database);

        var request = new GeneratorPlanPreviewArtifactRequest
        {
            PreviewRequest = new GeneratorPlanPreviewRequest { SourcePath = warningPath }
        };

        var first = await service.CaptureAsync(request, CancellationToken.None);
        var second = await service.CaptureAsync(request, CancellationToken.None);
        var custom = await service.CaptureAsync(new GeneratorPlanPreviewArtifactRequest
        {
            PreviewRequest = new GeneratorPlanPreviewRequest { SourcePath = warningPath, RenderMarkdown = false },
            ResultArtifactId = "artifact/generator_plan_preview/custom",
            MarkdownArtifactId = "artifact/generator_plan_preview_markdown/custom",
            GeneratedBy = "test"
        }, CancellationToken.None);

        Assert.Equal(GeneratorPlanPreviewValidationState.Warnings, first.ResultArtifact.ValidationState);
        Assert.NotNull(first.MarkdownArtifact);
        Assert.Contains(first.ValidationResults, result => result.Code == GeneratorPlanPreviewDiagnosticCodes.StepMissingValidationGates);
        Assert.Equal(first.ResultArtifact.Id, second.ResultArtifact.Id);
        Assert.Null(custom.MarkdownArtifact);
        Assert.Equal("artifact/generator_plan_preview/custom", custom.ResultArtifact.Id);
        Assert.NotNull(await database.GetGeneratedArtifactByIdAsync(GeneratorPlanPreviewArtifactIds.ResultArtifactId, CancellationToken.None));
        Assert.Equal(first.ValidationResults.Count, (await database.ListValidationResultsByArtifactAsync(GeneratorPlanPreviewArtifactIds.ResultArtifactId, CancellationToken.None)).Count);
    }

    [Fact]
    public async Task ReaderReturnsEmptySavedArtifactsAndMissingMarkdown()
    {
        using var temp = new AtlasTempDirectory();
        var path = WriteExample(temp.Path);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var reader = new GeneratorPlanPreviewArtifactReader(database);

        var missing = await reader.ReadLatestAsync(CancellationToken.None);

        await new GeneratorPlanPreviewArtifactService(new GeneratorPlanPreviewService(), database)
            .CaptureAsync(new GeneratorPlanPreviewArtifactRequest
            {
                PreviewRequest = new GeneratorPlanPreviewRequest
                {
                    SourcePath = path,
                    RenderMarkdown = false
                }
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
            Steps =
            [
                new GeneratorPlanPreviewStep
                {
                    Id = "step/profile_summary",
                    Order = 1,
                    Title = "Profile summary",
                    ProducerRole = "role/designer_llm/v1",
                    ContextPackTemplate = "context_template/design_discussion/v1",
                    ExpectedArtifactContract = "game_profile_v1",
                    ValidationGates = ["validation.level_0_json_shape"]
                }
            ]
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

    private static async Task<SqliteDesignDatabase> CreateInitializedDatabaseAsync(string root)
    {
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(root, ".llmgc", "design.db"), CancellationToken.None);
        return database;
    }
}
