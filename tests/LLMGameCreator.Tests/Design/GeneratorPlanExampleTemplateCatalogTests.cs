using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanExampleTemplateCatalogTests
{
    [Fact]
    public void TemplateCatalogListsAtLeastFiveTemplates()
    {
        var templates = new GeneratorPlanExampleTemplateCatalog().ListTemplates();

        Assert.True(templates.Count >= 5);
        Assert.Contains(templates, template => template.Title == "Sky Lantern Outpost");
        Assert.All(templates, template => Assert.Contains("semantic_pack_v1", template.TargetArtifacts));
    }

    [Fact]
    public void TemplateCatalogTemplatesHaveUniqueIdsAndFileNames()
    {
        var catalog = new GeneratorPlanExampleTemplateCatalog();
        var templates = catalog.ListTemplates()
            .Select(template => catalog.GetTemplate(template.Id)!)
            .ToList();

        Assert.Equal(templates.Count, templates.Select(template => template.Summary.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(templates.Count, templates.Select(template => template.FileName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(templates, template => Assert.EndsWith(".example.json", template.FileName));
    }

    [Fact]
    public void TemplateCatalogTemplatesAreValidJson()
    {
        var catalog = new GeneratorPlanExampleTemplateCatalog();

        foreach (var summary in catalog.ListTemplates())
        {
            var template = catalog.GetTemplate(summary.Id);

            Assert.NotNull(template);
            using var document = JsonDocument.Parse(template.Json);
            Assert.Equal("0.1", document.RootElement.GetProperty("schema_version").GetString());
            Assert.Equal(summary.Title, document.RootElement.GetProperty("title").GetString());
        }
    }

    [Fact]
    public async Task TemplateCatalogTemplatesCanPreview()
    {
        using var temp = new TempDirectory();
        var service = new GeneratorPlanExampleTemplateService(new GeneratorPlanExampleTemplateCatalog());
        var previewService = new GeneratorPlanPreviewService();

        foreach (var template in service.ListTemplates())
        {
            var materialized = await service.MaterializeAsync(new GeneratorPlanExampleTemplateMaterializeRequest
            {
                TemplateId = template.Id,
                TargetDirectory = temp.Path,
                Overwrite = true
            }, CancellationToken.None);

            var preview = await previewService.PreviewAsync(new GeneratorPlanPreviewRequest
            {
                SourcePath = materialized.FilePath
            }, CancellationToken.None);

            Assert.True(materialized.Ok, materialized.Message);
            Assert.True(preview.Ok, string.Join(Environment.NewLine, preview.Diagnostics.Select(diagnostic => diagnostic.Message)));
            Assert.Equal(6, preview.Preview.Steps.Count);
            Assert.Equal(template.Title, preview.Preview.Title);
        }
    }

    [Fact]
    public async Task TemplateServiceMaterializesSelectedTemplateAndDoesNotOverwriteWhenOverwriteFalse()
    {
        using var temp = new TempDirectory();
        var service = new GeneratorPlanExampleTemplateService(new GeneratorPlanExampleTemplateCatalog());

        var first = await service.MaterializeAsync(new GeneratorPlanExampleTemplateMaterializeRequest
        {
            TemplateId = "sky-lantern-outpost",
            TargetDirectory = temp.Path
        }, CancellationToken.None);
        var second = await service.MaterializeAsync(new GeneratorPlanExampleTemplateMaterializeRequest
        {
            TemplateId = "sky-lantern-outpost",
            TargetDirectory = temp.Path,
            Overwrite = false
        }, CancellationToken.None);

        Assert.True(first.Ok, first.Message);
        Assert.True(File.Exists(first.FilePath));
        Assert.False(second.Ok);
        Assert.Equal(first.FilePath, second.FilePath);
    }

    [Fact]
    public async Task TemplateServiceRejectsUnknownTemplate()
    {
        using var temp = new TempDirectory();
        var service = new GeneratorPlanExampleTemplateService(new GeneratorPlanExampleTemplateCatalog());

        var result = await service.MaterializeAsync(new GeneratorPlanExampleTemplateMaterializeRequest
        {
            TemplateId = "missing-template",
            TargetDirectory = temp.Path
        }, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OneClickExportCanExportEachBuiltInTemplate()
    {
        var catalog = new GeneratorPlanExampleTemplateCatalog();
        var templateService = new GeneratorPlanExampleTemplateService(catalog);

        foreach (var template in catalog.ListTemplates())
        {
            using var temp = new TempDirectory();
            var runService = await CreateRunServiceAsync(temp.Path);
            var materialized = await templateService.MaterializeAsync(new GeneratorPlanExampleTemplateMaterializeRequest
            {
                TemplateId = template.Id,
                TargetDirectory = Path.Combine(temp.Path, "examples")
            }, CancellationToken.None);
            var exportFolder = Path.Combine(temp.Path, "export");

            var result = await runService.RunAsync(new GeneratorPlanPackageExportRunRequest
            {
                SourceExamplePath = materialized.FilePath,
                ExportFolderPath = exportFolder
            }, CancellationToken.None);

            Assert.True(materialized.Ok, materialized.Message);
            Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
            Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
            Assert.True(File.Exists(Path.Combine(exportFolder, "package.json")));
            Assert.True(
                result.Status is GeneratorPlanPackageExportRunStatus.Succeeded or GeneratorPlanPackageExportRunStatus.SucceededWithWarnings,
                result.Status);
        }
    }

    [Fact]
    public async Task OneClickExportTemplateProducesMeaningfulTitleAndQuest()
    {
        using var temp = new TempDirectory();
        var templateService = new GeneratorPlanExampleTemplateService(new GeneratorPlanExampleTemplateCatalog());
        var materialized = await templateService.MaterializeAsync(new GeneratorPlanExampleTemplateMaterializeRequest
        {
            TemplateId = "clockwork-orchard",
            TargetDirectory = Path.Combine(temp.Path, "examples")
        }, CancellationToken.None);
        var exportFolder = Path.Combine(temp.Path, "export");
        var runService = await CreateRunServiceAsync(temp.Path);

        var result = await runService.RunAsync(new GeneratorPlanPackageExportRunRequest
        {
            SourceExamplePath = materialized.FilePath,
            ExportFolderPath = exportFolder
        }, CancellationToken.None);
        using var packageJson = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(exportFolder, "package.json")));
        var manifest = packageJson.RootElement.GetProperty("manifest");
        var game = packageJson.RootElement.GetProperty("game");

        Assert.True(result.Ok, string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        Assert.Equal("Clockwork Orchard", manifest.GetProperty("title").GetString());
        Assert.Equal("game/clockwork/orchard", manifest.GetProperty("packageId").GetString());
        Assert.Contains("mechanical trees", manifest.GetProperty("description").GetString());
        Assert.Contains(game.GetProperty("quests").EnumerateArray(), quest => !string.IsNullOrWhiteSpace(quest.GetProperty("title").GetString()));
        Assert.Contains(game.GetProperty("abilities").EnumerateArray(), ability => !string.IsNullOrWhiteSpace(ability.GetProperty("name").GetString()));
        Assert.Contains(game.GetProperty("entityPrototypes").EnumerateArray(), entity => entity.GetProperty("id").GetString() != "entity/player");
    }

    private static async Task<GeneratorPlanPackageExportRunService> CreateRunServiceAsync(string root)
    {
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(root, ".llmgc", "design.db"), CancellationToken.None);
        var approvalReader = new GeneratorPlanDraftArtifactApprovalArtifactReader(database);
        return new GeneratorPlanPackageExportRunService(
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
