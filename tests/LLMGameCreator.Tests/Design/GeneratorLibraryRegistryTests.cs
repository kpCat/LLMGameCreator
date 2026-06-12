using System.Text.Json;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Infrastructure.Storage;
using Microsoft.Data.Sqlite;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorLibraryRegistryTests
{
    [Fact]
    public async Task SqliteDesignDatabaseCreatesSchema()
    {
        using var temp = new TempDirectory();
        var databasePath = Path.Combine(temp.Path, "design.db");
        var database = new SqliteDesignDatabase();

        await database.InitializeAsync(databasePath, CancellationToken.None);

        Assert.Equal(1, await ReadUserVersionAsync(databasePath));
        Assert.Contains("design_metadata", await ListTablesAsync(databasePath));
        Assert.Contains("generator_modules", await ListTablesAsync(databasePath));
        Assert.Contains("capability_modules", await ListTablesAsync(databasePath));
        Assert.Contains("import_issues", await ListTablesAsync(databasePath));
    }

    [Fact]
    public async Task GeneratorLibraryImporterImportsBatch001To004Manifests()
    {
        using var temp = CreateLibraryWithRepositoryManifests();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var importer = new GeneratorLibraryImportService(database);

        var report = await importer.ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);
        var modules = await database.ListModulesAsync(CancellationToken.None);

        Assert.Equal(4, report.ImportedManifestCount);
        Assert.Contains(modules, module => module.Id == "core/diagnostics/v1");
        Assert.Contains(modules, module => module.Id == "core/rng/v1");
        Assert.Contains(modules, module => module.Id == "core/schema/v1");
        Assert.Contains(modules, module => module.Id == "core/id/v1");
        Assert.Contains(modules, module => module.Id == "core/grid/v1");
        Assert.Contains(modules, module => module.Id == "core/coordinates/v1");
        Assert.Contains(modules, module => module.Id == "core/time_model/v1");
        Assert.Contains(modules, module => module.Id == "core/turn_system/v1");
        Assert.Contains(modules, module => module.Id == "core/mode_transition/v1");
        Assert.Contains(modules, module => module.Id == "generation/capability_manifest/v1");
        Assert.Contains(modules, module => module.Id == "generation/module_manifest/v1");
        Assert.Contains(modules, module => module.Id == "generation/generator_plan/v1");
    }

    [Fact]
    public async Task GeneratorLibraryImportIsIdempotent()
    {
        using var temp = CreateLibraryWithRepositoryManifests();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var importer = new GeneratorLibraryImportService(database);

        await importer.ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);
        var moduleCount = await CountRowsAsync(Path.Combine(temp.Path, ".llmgc", "design.db"), "generator_modules");
        var fileCount = await CountRowsAsync(Path.Combine(temp.Path, ".llmgc", "design.db"), "generator_module_files");
        await importer.ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);

        Assert.Equal(moduleCount, await CountRowsAsync(Path.Combine(temp.Path, ".llmgc", "design.db"), "generator_modules"));
        Assert.Equal(fileCount, await CountRowsAsync(Path.Combine(temp.Path, ".llmgc", "design.db"), "generator_module_files"));
    }

    [Fact]
    public async Task GeneratorLibraryImporterNormalizesManifestVariants()
    {
        using var temp = new TempDirectory();
        var manifests = CreateManifestsFolder(temp.Path);
        await WriteManifestAsync(manifests, "runtime.manifest.json", """
        {
          "id": "batch/runtime/v1",
          "batch": "a",
          "runtime_targets": ["debug"],
          "architecture_notes": { "turn_modes": ["mixed"], "combat_modes": ["none"] },
          "modules": [{ "id": "core/runtime/v1", "path": "lua/core/runtime.lua", "capabilities": ["core.runtime"] }]
        }
        """);
        await WriteManifestAsync(manifests, "supported.manifest.json", """
        {
          "id": "batch/supported/v1",
          "batch": "b",
          "supported_runtime_targets": ["editor"],
          "supported_time_modes": ["turn_based"],
          "supported_combat_modes": ["tactical"],
          "modules": [{ "id": "core/supported/v1", "path": "lua/core/supported.lua", "capabilities": ["core.supported"] }]
        }
        """);
        var database = await CreateInitializedDatabaseAsync(temp.Path);

        await new GeneratorLibraryImportService(database).ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);

        var runtime = Assert.Single(await database.ListModulesByCapabilityAsync("core.runtime", CancellationToken.None));
        var supported = Assert.Single(await database.ListModulesByCapabilityAsync("core.supported", CancellationToken.None));
        Assert.Contains("debug", runtime.RuntimeTargetsJson);
        Assert.Contains("mixed", runtime.TurnModesJson);
        Assert.Contains("none", runtime.CombatModesJson);
        Assert.Contains("editor", supported.RuntimeTargetsJson);
        Assert.Contains("turn_based", supported.TurnModesJson);
        Assert.Contains("tactical", supported.CombatModesJson);
    }

    [Fact]
    public async Task GeneratorLibraryImporterReportsInvalidManifest()
    {
        using var temp = new TempDirectory();
        var manifests = CreateManifestsFolder(temp.Path);
        await WriteManifestAsync(manifests, "valid.manifest.json", """
        { "id": "batch/valid/v1", "modules": [{ "id": "core/valid/v1", "path": "lua/core/valid.lua", "capabilities": ["core.valid"] }] }
        """);
        await WriteManifestAsync(manifests, "invalid.manifest.json", """
        { "modules": [{ "id": "core/invalid/v1", "path": "lua/core/invalid.lua" }] }
        """);
        var database = await CreateInitializedDatabaseAsync(temp.Path);

        var report = await new GeneratorLibraryImportService(database).ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);
        var modules = await database.ListModulesAsync(CancellationToken.None);

        Assert.Contains(report.Issues, issue => issue.Code == "manifest.id.empty" && issue.Severity == "error");
        Assert.Contains(modules, module => module.Id == "core/valid/v1");
        Assert.DoesNotContain(modules, module => module.Id == "core/invalid/v1");
    }

    [Fact]
    public async Task GeneratorLibraryImporterIgnoresSchemaExampleManifest()
    {
        using var temp = new TempDirectory();
        var manifests = CreateManifestsFolder(temp.Path);
        await WriteManifestAsync(manifests, "MANIFEST_CONTRACT.schema.example.json", "{ }");
        await WriteManifestAsync(manifests, "valid.manifest.json", """
        { "id": "batch/valid/v1", "modules": [{ "id": "core/valid/v1", "path": "lua/core/valid.lua", "capabilities": ["core.valid"] }] }
        """);
        var database = await CreateInitializedDatabaseAsync(temp.Path);

        var report = await new GeneratorLibraryImportService(database).ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);

        Assert.Equal(1, report.ManifestCount);
        Assert.DoesNotContain(report.Issues, issue => issue.Code == "manifest.id.empty");
        Assert.Contains(await database.ListModulesAsync(CancellationToken.None), module => module.Id == "core/valid/v1");
    }

    [Fact]
    public async Task CleanImportClearsOldImportIssues()
    {
        using var temp = new TempDirectory();
        var manifests = CreateManifestsFolder(temp.Path);
        await WriteManifestAsync(manifests, "invalid.manifest.json", "{ \"modules\": [] }");
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var importer = new GeneratorLibraryImportService(database);

        await importer.ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);
        Assert.NotEmpty(await database.ListImportIssuesAsync(CancellationToken.None));

        File.Delete(Path.Combine(manifests, "invalid.manifest.json"));
        await WriteManifestAsync(manifests, "valid.manifest.json", """
        { "id": "batch/valid/v1", "modules": [{ "id": "core/valid/v1", "path": "lua/core/valid.lua", "capabilities": ["core.valid"] }] }
        """);
        await importer.ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);

        Assert.Empty(await database.ListImportIssuesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task SecondImportRemovesStaleRegistrySnapshotRows()
    {
        using var temp = new TempDirectory();
        var manifests = CreateManifestsFolder(temp.Path);
        await WriteManifestAsync(manifests, "first.manifest.json", """
        {
          "id": "batch/first/v1",
          "files": ["lua/core/first.lua"],
          "modules": [{ "id": "core/first/v1", "path": "lua/core/first.lua", "capabilities": ["core.first"] }]
        }
        """);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var importer = new GeneratorLibraryImportService(database);

        await importer.ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);
        Assert.Contains(await database.ListModulesAsync(CancellationToken.None), module => module.Id == "core/first/v1");

        File.Delete(Path.Combine(manifests, "first.manifest.json"));
        await WriteManifestAsync(manifests, "second.manifest.json", """
        {
          "id": "batch/second/v1",
          "files": ["lua/core/second.lua"],
          "modules": [{ "id": "core/second/v1", "path": "lua/core/second.lua", "capabilities": ["core.second"] }]
        }
        """);
        await importer.ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);

        var modules = await database.ListModulesAsync(CancellationToken.None);
        var capabilities = await database.ListCapabilitiesAsync(CancellationToken.None);
        Assert.DoesNotContain(modules, module => module.Id == "core/first/v1");
        Assert.DoesNotContain(capabilities, capability => capability.Id == "core.first");
        Assert.Equal(1, await CountRowsAsync(Path.Combine(temp.Path, ".llmgc", "design.db"), "generator_module_files"));
    }

    [Fact]
    public async Task RegistryCanQueryModulesByCapability()
    {
        using var temp = CreateLibraryWithRepositoryManifests();
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        await new GeneratorLibraryImportService(database).ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);

        var modules = await database.ListModulesByCapabilityAsync("core.rng.next", CancellationToken.None);

        var module = Assert.Single(modules);
        Assert.Equal("core/rng/v1", module.Id);
    }

    [Fact]
    public async Task RegistryStoresUnknownFieldsAsMetadata()
    {
        using var temp = new TempDirectory();
        var manifests = CreateManifestsFolder(temp.Path);
        await WriteManifestAsync(manifests, "custom.manifest.json", """
        {
          "id": "batch/custom/v1",
          "x_custom_field": "kept",
          "modules": [{ "id": "core/custom/v1", "path": "lua/core/custom.lua", "capabilities": ["core.custom"], "x_module_note": "kept too" }]
        }
        """);
        var database = await CreateInitializedDatabaseAsync(temp.Path);

        await new GeneratorLibraryImportService(database).ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);

        var capability = Assert.Single(await database.ListCapabilitiesAsync(CancellationToken.None));
        var module = Assert.Single(await database.ListModulesAsync(CancellationToken.None));
        Assert.Contains("x_custom_field", capability.MetadataJson);
        Assert.Contains("kept", capability.MetadataJson);
        Assert.Contains("x_module_note", module.MetadataJson);
    }

    [Fact]
    public async Task ImporterDoesNotExecuteLua()
    {
        using var temp = new TempDirectory();
        var manifests = CreateManifestsFolder(temp.Path);
        var markerPath = Path.Combine(temp.Path, "executed.txt");
        var luaPath = Path.Combine(temp.Path, "generator-library", "lua", "danger.lua");
        Directory.CreateDirectory(Path.GetDirectoryName(luaPath)!);
        await File.WriteAllTextAsync(luaPath, $"os.execute('echo executed > {markerPath}')", CancellationToken.None);
        await WriteManifestAsync(manifests, "danger.manifest.json", """
        { "id": "batch/danger/v1", "files": ["lua/danger.lua"], "modules": [{ "id": "core/danger/v1", "path": "lua/danger.lua", "capabilities": ["core.danger"] }] }
        """);
        var database = await CreateInitializedDatabaseAsync(temp.Path);

        await new GeneratorLibraryImportService(database).ImportGeneratorLibraryAsync(temp.Path, CancellationToken.None);

        Assert.False(File.Exists(markerPath));
        Assert.NotNull(await database.GetModuleByIdAsync("core/danger/v1", CancellationToken.None));
    }

    private static async Task<SqliteDesignDatabase> CreateInitializedDatabaseAsync(string root)
    {
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(root, ".llmgc", "design.db"), CancellationToken.None);
        return database;
    }

    private static TempDirectory CreateLibraryWithRepositoryManifests()
    {
        var temp = new TempDirectory();
        var manifests = CreateManifestsFolder(temp.Path);
        var source = Path.Combine(FindRepositoryRoot(), "generator-library", "manifests");
        foreach (var fileName in new[] { "core_foundation.manifest.json", "core_grid.manifest.json", "time_turn.manifest.json", "generation_manifest.manifest.json" })
        {
            File.Copy(Path.Combine(source, fileName), Path.Combine(manifests, fileName));
        }

        return temp;
    }

    private static string CreateManifestsFolder(string root)
    {
        var manifests = Path.Combine(root, "generator-library", "manifests");
        Directory.CreateDirectory(manifests);
        return manifests;
    }

    private static Task WriteManifestAsync(string manifestsFolder, string fileName, string json)
    {
        return File.WriteAllTextAsync(Path.Combine(manifestsFolder, fileName), json, CancellationToken.None);
    }

    private static async Task<int> ReadUserVersionAsync(string databasePath)
    {
        await using var connection = new SqliteConnection($"Data Source={databasePath};Pooling=False");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version;";
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task<List<string>> ListTablesAsync(string databasePath)
    {
        await using var connection = new SqliteConnection($"Data Source={databasePath};Pooling=False");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;";
        var tables = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    private static async Task<int> CountRowsAsync(string databasePath, string tableName)
    {
        await using var connection = new SqliteConnection($"Data Source={databasePath};Pooling=False");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName};";
        return Convert.ToInt32(await command.ExecuteScalarAsync());
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
