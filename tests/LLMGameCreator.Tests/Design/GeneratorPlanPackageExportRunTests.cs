using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanPackageExportRunTests
{
    [Fact]
    public async Task RunAsyncExportsPackageJsonFromExamplePath()
    {
        using var temp = new TempDirectory();
        var service = await CreateServiceAsync(temp.Path);
        var exportFolder = Path.Combine(temp.Path, "exported");

        var result = await service.RunAsync(new GeneratorPlanPackageExportRunRequest
        {
            SourceExamplePath = WriteExample(temp.Path),
            ExportFolderPath = exportFolder
        }, CancellationToken.None);

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.True(File.Exists(Path.Combine(exportFolder, "package.json")));
        Assert.Contains("\"title\": \"game profile v1\"", await File.ReadAllTextAsync(Path.Combine(exportFolder, "package.json")));
        Assert.Equal(Path.Combine(exportFolder, "package.json"), result.PackageJsonPath);
    }

    [Fact]
    public async Task RunAsyncSavesApprovalAssemblyAndRunArtifacts()
    {
        using var temp = new TempDirectory();
        var service = await CreateServiceAsync(temp.Path);
        var result = await service.RunAsync(new GeneratorPlanPackageExportRunRequest
        {
            SourceExamplePath = WriteExample(temp.Path),
            ExportFolderPath = Path.Combine(temp.Path, "package")
        }, CancellationToken.None);
        var artifacts = await service.Database.ListGeneratedArtifactsAsync(CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Contains(artifacts, artifact => artifact.Id == GeneratorPlanDraftArtifactApprovalArtifactIds.StagingArtifactId);
        Assert.Contains(artifacts, artifact => artifact.Id == GeneratorPlanDraftArtifactApprovalArtifactIds.ApprovedArtifactSetArtifactId);
        Assert.Contains(artifacts, artifact => artifact.Id == GeneratorPlanGamePackageAssemblyArtifactIds.AssemblyArtifactId);
        Assert.Contains(artifacts, artifact => artifact.Id == GeneratorPlanGamePackageAssemblyArtifactIds.PackageDraftArtifactId);
        Assert.Contains(artifacts, artifact => artifact.Id == GeneratorPlanPackageExportRunArtifactIds.RunArtifactId);
        Assert.NotNull(result.AssemblyArtifacts);
    }

    [Fact]
    public async Task RunAsyncReturnsFailedForMissingExamplePath()
    {
        using var temp = new TempDirectory();
        var service = await CreateServiceAsync(temp.Path);

        var result = await service.RunAsync(new GeneratorPlanPackageExportRunRequest
        {
            ExportFolderPath = Path.Combine(temp.Path, "package")
        }, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Equal(GeneratorPlanPackageExportRunStatus.Failed, result.Status);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanPackageExportRunDiagnosticCodes.MissingSourceExamplePath);
    }

    [Fact]
    public async Task RunAsyncReturnsFailedWhenExampleFileDoesNotExist()
    {
        using var temp = new TempDirectory();
        var service = await CreateServiceAsync(temp.Path);
        var missing = Path.Combine(temp.Path, "missing.example.json");

        var result = await service.RunAsync(new GeneratorPlanPackageExportRunRequest
        {
            SourceExamplePath = missing,
            ExportFolderPath = Path.Combine(temp.Path, "package")
        }, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanPackageExportRunDiagnosticCodes.SourceExampleNotFound && diagnostic.Target == missing);
    }

    [Fact]
    public async Task RunAsyncReturnsFailedWhenExportFolderMissing()
    {
        using var temp = new TempDirectory();
        var service = await CreateServiceAsync(temp.Path);

        var result = await service.RunAsync(new GeneratorPlanPackageExportRunRequest
        {
            SourceExamplePath = WriteExample(temp.Path)
        }, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorPlanPackageExportRunDiagnosticCodes.MissingExportFolderPath);
    }

    [Fact]
    public async Task RunAsyncCanSkipFinalMarkdownRendering()
    {
        using var temp = new TempDirectory();
        var service = await CreateServiceAsync(temp.Path);

        var result = await service.RunAsync(new GeneratorPlanPackageExportRunRequest
        {
            SourceExamplePath = WriteExample(temp.Path),
            ExportFolderPath = Path.Combine(temp.Path, "package"),
            RenderMarkdown = false
        }, CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Empty(result.MarkdownReport);
        Assert.Null(result.AssemblyArtifacts?.MarkdownArtifact);
    }

    [Fact]
    public async Task RunAsyncCanSkipFinalRunArtifactPersistence()
    {
        using var temp = new TempDirectory();
        var service = await CreateServiceAsync(temp.Path);

        var result = await service.RunAsync(new GeneratorPlanPackageExportRunRequest
        {
            SourceExamplePath = WriteExample(temp.Path),
            ExportFolderPath = Path.Combine(temp.Path, "package"),
            SaveArtifacts = false
        }, CancellationToken.None);
        var runArtifact = await service.Database.GetGeneratedArtifactByIdAsync(GeneratorPlanPackageExportRunArtifactIds.RunArtifactId, CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Null(runArtifact);
        Assert.NotNull(await service.Database.GetGeneratedArtifactByIdAsync(GeneratorPlanGamePackageAssemblyArtifactIds.AssemblyArtifactId, CancellationToken.None));
    }

    [Fact]
    public async Task RunArtifactServiceIsIdempotentForSameIds()
    {
        using var temp = new TempDirectory();
        var service = await CreateServiceAsync(temp.Path);
        var result = await service.RunAsync(new GeneratorPlanPackageExportRunRequest
        {
            SourceExamplePath = WriteExample(temp.Path),
            ExportFolderPath = Path.Combine(temp.Path, "package"),
            SaveArtifacts = false
        }, CancellationToken.None);
        var artifactService = new GeneratorPlanPackageExportRunArtifactService(service.Database);

        var first = await artifactService.SaveAsync(result, CancellationToken.None);
        var second = await artifactService.SaveAsync(result, CancellationToken.None);
        var artifacts = await service.Database.ListGeneratedArtifactsAsync(CancellationToken.None);

        Assert.Equal(first.RunArtifact.Id, second.RunArtifact.Id);
        Assert.Single(artifacts, artifact => artifact.Id == GeneratorPlanPackageExportRunArtifactIds.RunArtifactId);
    }

    [Fact]
    public async Task RunArtifactReaderReturnsEmptyWhenMissing()
    {
        using var temp = new TempDirectory();
        var service = await CreateServiceAsync(temp.Path);

        var result = await new GeneratorPlanPackageExportRunArtifactReader(service.Database).ReadLatestAsync(CancellationToken.None);

        Assert.False(result.Exists);
        Assert.Null(result.RunArtifact);
    }

    [Fact]
    public async Task RunArtifactReaderReturnsSavedRunMarkdownAndValidationResults()
    {
        using var temp = new TempDirectory();
        var service = await CreateServiceAsync(temp.Path);
        var result = await service.RunAsync(new GeneratorPlanPackageExportRunRequest
        {
            SourceExamplePath = WriteExample(temp.Path, "semantic_pack_v1"),
            ExportFolderPath = Path.Combine(temp.Path, "package")
        }, CancellationToken.None);

        var loaded = await new GeneratorPlanPackageExportRunArtifactReader(service.Database).ReadLatestAsync(CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Equal(GeneratorPlanPackageExportRunStatus.SucceededWithWarnings, result.Status);
        Assert.True(loaded.Exists);
        Assert.NotNull(loaded.RunArtifact);
        Assert.NotNull(loaded.MarkdownArtifact);
        Assert.NotEmpty(loaded.ValidationResults);
    }

    [Fact]
    public void MarkdownRendererRendersStatusFilesDiagnosticsAndEscapesCells()
    {
        var result = new GeneratorPlanPackageExportRunResult
        {
            Status = GeneratorPlanPackageExportRunStatus.SucceededWithWarnings,
            SourceExamplePath = "example|path\nline",
            ExportFolderPath = "export|folder",
            PackageJsonPath = "package|json",
            AssemblyResult = new GeneratorPlanGamePackageAssemblyResult
            {
                Status = GeneratorPlanGamePackageAssemblyStatus.ValidPackage,
                Package = new LLMGameCreator.GamePackage.GamePackageDefinition
                {
                    Manifest =
                    {
                        PackageId = "game|id",
                        Title = "Title|Pipe"
                    }
                }
            },
            Diagnostics =
            [
                new GeneratorPlanPackageExportRunDiagnostic
                {
                    Severity = GeneratorPlanPreviewDiagnosticSeverity.Warning,
                    Code = "code|pipe",
                    Target = "target|pipe",
                    Message = "Line 1\nLine 2"
                }
            ]
        };

        var markdown = new GeneratorPlanPackageExportRunMarkdownRenderer().Render(result);

        Assert.Contains("# One-click Package Export Run", markdown);
        Assert.Contains("## Files", markdown);
        Assert.Contains("## Diagnostics", markdown);
        Assert.Contains("example\\|path<br>line", markdown);
        Assert.Contains("Line 1<br>Line 2", markdown);
    }

    private static async Task<ServiceContext> CreateServiceAsync(string root)
    {
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(root, ".llmgc", "design.db"), CancellationToken.None);
        var approvalReader = new GeneratorPlanDraftArtifactApprovalArtifactReader(database);
        var service = new GeneratorPlanPackageExportRunService(
            new GeneratorPlanDraftArtifactApprovalArtifactService(new GeneratorPlanDraftArtifactApprovalService(), database),
            approvalReader,
            new GeneratorPlanGamePackageAssemblyService(
                new GeneratorPlanGamePackageAssembler(),
                new GamePackageValidator(),
                new GeneratorPlanGamePackageAssemblyValidator(),
                new GeneratorPlanGamePackageAssemblyMarkdownRenderer(),
                new JsonGamePackageRepository(),
                approvalReader),
            new GeneratorPlanGamePackageAssemblyArtifactService(database),
            new GeneratorPlanPackageExportRunMarkdownRenderer(),
            database);

        return new ServiceContext(service, database);
    }

    private static string WriteExample(string root, params string[] targetArtifacts)
    {
        var artifacts = targetArtifacts.Length == 0
            ? ["game_profile_v1", "scene_pack_v1", "entity_pack_v1", "quest_pack_v1"]
            : targetArtifacts;
        var path = Path.Combine(root, Guid.NewGuid().ToString("N") + ".example.json");
        File.WriteAllText(path, $$"""
        {
          "schema_version": "0.1",
          "example_id": "example/package-export/v1",
          "title": "Package Export Example",
          "purpose": "Test one-click package export.",
          "source_profile": {
            "id": "profile/package-export/v1"
          },
          "selected_feature_bundles": [
            "feature_bundle/package-export/v1"
          ],
          "target_artifacts": {{JsonSerializer.Serialize(artifacts)}},
          "steps": [
            {{string.Join(",", artifacts.Select((artifact, index) => $$"""
            {
              "id": "step/{{index}}",
              "order": {{index + 1}},
              "title": "{{artifact}}",
              "producer_role": "role/designer_llm/v1",
              "context_pack_template": "context_template/design_discussion/v1",
              "expected_artifact_contract": "{{artifact}}",
              "inputs": ["{{artifact}}"],
              "validation_gates": ["validation.level_0_json_shape"],
              "on_success": "stage_{{index}}",
              "on_failure": "repair_{{index}}"
            }
            """))}}
          ]
        }
        """);
        return path;
    }

    private sealed record ServiceContext(
        GeneratorPlanPackageExportRunService Runner,
        SqliteDesignDatabase Database)
    {
        public Task<GeneratorPlanPackageExportRunResult> RunAsync(GeneratorPlanPackageExportRunRequest request, CancellationToken cancellationToken)
        {
            return Runner.RunAsync(request, cancellationToken);
        }
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
