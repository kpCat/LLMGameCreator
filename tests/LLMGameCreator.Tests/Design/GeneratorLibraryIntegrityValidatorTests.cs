using LLMGameCreator.Application.Design;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorLibraryIntegrityValidatorTests
{
    [Fact]
    public async Task IntegrityValidatorPassesCurrentGeneratorLibrary()
    {
        var report = await new GeneratorLibraryIntegrityValidator().ValidateAsync(FindRepositoryRoot(), CancellationToken.None);

        Assert.Equal(0, report.Summary.ErrorCount);
        Assert.True(report.Summary.ManifestCount > 0);
        Assert.True(report.Summary.ModuleCount > 0);
    }

    [Fact]
    public async Task IntegrityValidatorReportsMissingManifestFileEntry()
    {
        using var temp = new TempDirectory();
        await CreateManifestAsync(temp.Path, """
        {
          "id": "batch/test/v1",
          "batch": "001",
          "title": "Test",
          "purpose": "Test",
          "files": ["lua/missing.lua", "BATCH_001_REPORT.md"],
          "modules": [{ "id": "core/test/v1", "path": "lua/test.lua", "category": "core", "capabilities": ["core.test"] }]
        }
        """);
        WriteLibraryFile(temp.Path, "lua/test.lua");
        WriteLibraryFile(temp.Path, "BATCH_001_REPORT.md");

        var report = await new GeneratorLibraryIntegrityValidator().ValidateAsync(temp.Path, CancellationToken.None);

        Assert.Contains(report.Issues, issue => issue.Code == "file.missing" && issue.Severity == GeneratorLibraryIntegritySeverity.Error);
    }

    [Fact]
    public async Task IntegrityValidatorReportsMissingModulePath()
    {
        using var temp = new TempDirectory();
        await CreateManifestAsync(temp.Path, """
        {
          "id": "batch/test/v1",
          "batch": "001",
          "title": "Test",
          "purpose": "Test",
          "files": ["BATCH_001_REPORT.md"],
          "modules": [{ "id": "core/test/v1", "path": "lua/missing.lua", "category": "core", "capabilities": ["core.test"] }]
        }
        """);
        WriteLibraryFile(temp.Path, "BATCH_001_REPORT.md");

        var report = await new GeneratorLibraryIntegrityValidator().ValidateAsync(temp.Path, CancellationToken.None);

        Assert.Contains(report.Issues, issue => issue.Code == "module.path_missing" && issue.Severity == GeneratorLibraryIntegritySeverity.Error);
    }

    [Fact]
    public async Task IntegrityValidatorReportsAliasFields()
    {
        using var temp = new TempDirectory();
        await CreateManifestAsync(temp.Path, """
        {
          "id": "batch/test/v1",
          "batch": "001",
          "title": "Test",
          "purpose": "Test",
          "files": ["lua/test.lua", "BATCH_001_REPORT.md"],
          "modules": [{
            "id": "core/test/v1",
            "module_id": "core/old_test/v1",
            "path": "lua/test.lua",
            "file": "lua/old_test.lua",
            "category": "core",
            "capabilities": ["core.test"],
            "depends_on_contracts": ["core/diagnostics/v1"]
          }]
        }
        """);
        WriteLibraryFile(temp.Path, "lua/test.lua");
        WriteLibraryFile(temp.Path, "BATCH_001_REPORT.md");

        var report = await new GeneratorLibraryIntegrityValidator().ValidateAsync(temp.Path, CancellationToken.None);

        Assert.Contains(report.Issues, issue => issue.Code == "manifest.alias.module_id" && issue.Severity == GeneratorLibraryIntegritySeverity.Warning);
        Assert.Contains(report.Issues, issue => issue.Code == "manifest.alias.file" && issue.Severity == GeneratorLibraryIntegritySeverity.Warning);
        Assert.Contains(report.Issues, issue => issue.Code == "manifest.alias.depends_on_contracts" && issue.Severity == GeneratorLibraryIntegritySeverity.Warning);
    }

    [Fact]
    public async Task IntegrityValidatorReportsRootLeakage()
    {
        using var temp = new TempDirectory();
        Directory.CreateDirectory(Path.Combine(temp.Path, "lua"));
        await CreateManifestAsync(temp.Path, """
        {
          "id": "batch/test/v1",
          "batch": "001",
          "title": "Test",
          "purpose": "Test",
          "files": ["lua/test.lua", "BATCH_001_REPORT.md"],
          "modules": [{ "id": "core/test/v1", "path": "lua/test.lua", "category": "core", "capabilities": ["core.test"] }]
        }
        """);
        WriteLibraryFile(temp.Path, "lua/test.lua");
        WriteLibraryFile(temp.Path, "BATCH_001_REPORT.md");

        var report = await new GeneratorLibraryIntegrityValidator().ValidateAsync(temp.Path, CancellationToken.None);

        Assert.Contains(report.Issues, issue => issue.Code == "root.leakage" && issue.Severity == GeneratorLibraryIntegritySeverity.Error);
    }

    [Fact]
    public async Task IntegrityValidatorReportsDuplicateModuleIds()
    {
        using var temp = new TempDirectory();
        await CreateManifestAsync(temp.Path, """
        {
          "id": "batch/test-a/v1",
          "batch": "001",
          "title": "Test A",
          "purpose": "Test",
          "files": ["lua/test_a.lua", "BATCH_001_REPORT.md"],
          "modules": [{ "id": "core/test/v1", "path": "lua/test_a.lua", "category": "core", "capabilities": ["core.test.a"] }]
        }
        """, "a.manifest.json");
        await CreateManifestAsync(temp.Path, """
        {
          "id": "batch/test-b/v1",
          "batch": "002",
          "title": "Test B",
          "purpose": "Test",
          "files": ["lua/test_b.lua", "BATCH_002_REPORT.md"],
          "modules": [{ "id": "core/test/v1", "path": "lua/test_b.lua", "category": "core", "capabilities": ["core.test.b"] }]
        }
        """, "b.manifest.json");
        WriteLibraryFile(temp.Path, "lua/test_a.lua");
        WriteLibraryFile(temp.Path, "lua/test_b.lua");
        WriteLibraryFile(temp.Path, "BATCH_001_REPORT.md");
        WriteLibraryFile(temp.Path, "BATCH_002_REPORT.md");

        var report = await new GeneratorLibraryIntegrityValidator().ValidateAsync(temp.Path, CancellationToken.None);

        Assert.Contains(report.Issues, issue => issue.Code == "module.id.duplicate" && issue.Severity == GeneratorLibraryIntegritySeverity.Error);
    }

    [Fact]
    public async Task IntegrityValidatorIgnoresSchemaExampleManifest()
    {
        using var temp = new TempDirectory();
        var manifests = Path.Combine(temp.Path, "generator-library", "manifests");
        Directory.CreateDirectory(manifests);
        await File.WriteAllTextAsync(Path.Combine(manifests, "MANIFEST_CONTRACT.schema.example.json"), "{ invalid json", CancellationToken.None);
        WriteLibraryFile(temp.Path, "docs/lua/MANIFEST_CONTRACT.md");

        var report = await new GeneratorLibraryIntegrityValidator().ValidateAsync(temp.Path, CancellationToken.None);

        Assert.DoesNotContain(report.Issues, issue => issue.Code == "manifest.invalid_json");
        Assert.Equal(0, report.Summary.ManifestCount);
    }

    private static async Task CreateManifestAsync(string repositoryRoot, string json, string fileName = "test.manifest.json")
    {
        WriteRequiredContractFiles(repositoryRoot);
        var manifests = Path.Combine(repositoryRoot, "generator-library", "manifests");
        Directory.CreateDirectory(manifests);
        await File.WriteAllTextAsync(Path.Combine(manifests, fileName), json, CancellationToken.None);
    }

    private static void WriteRequiredContractFiles(string repositoryRoot)
    {
        WriteLibraryFile(repositoryRoot, "docs/lua/MANIFEST_CONTRACT.md");
        WriteLibraryFile(repositoryRoot, "manifests/MANIFEST_CONTRACT.schema.example.json", "{}");
    }

    private static void WriteLibraryFile(string repositoryRoot, string relativePath, string content = "")
    {
        var path = Path.Combine(repositoryRoot, "generator-library", relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("Repository root was not found.");
        }

        return directory.FullName;
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
