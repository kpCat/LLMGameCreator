using LLMGameCreator.Application.Design.Atlas;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class AtlasRegistryImportServiceTests
{
    [Fact]
    public async Task ImportAtlasRegistryLoadsDocumentsAndExamples()
    {
        using var temp = new TempDirectory();
        var atlasRoot = CreateAtlasRoot(temp.Path);

        await WriteAtlasFileAsync(atlasRoot, "capability_atlas.json", """
        {
          "schema_version": "0.1",
          "id": "capability_atlas/v1",
          "title": "Capability Atlas",
          "purpose": "Test capability atlas.",
          "runtime_targets": ["debug"]
        }
        """);

        await WriteAtlasFileAsync(atlasRoot, "reference_profiles.json", """
        {
          "schema_version": "0.1",
          "id": "reference_profiles/v1",
          "title": "Reference Profiles",
          "purpose": "Test reference profiles.",
          "profiles": [
            { "id": "profile/test/v1", "title": "Test Profile", "purpose": "Test profile." }
          ]
        }
        """);

        await WriteAtlasFileAsync(atlasRoot, "validation_pipeline.json", """
        {
          "schema_version": "0.1",
          "id": "validation_pipeline/v1",
          "title": "Validation Pipeline",
          "purpose": "Test validation pipeline.",
          "validation_levels": [
            { "id": "validation.level_0_json_shape", "title": "Shape" }
          ]
        }
        """);

        await WriteAtlasFileAsync(atlasRoot, "feature_bundle_map.json", """
        {
          "schema_version": "0.1",
          "id": "feature_bundle_map/v1",
          "title": "Feature Bundle Map",
          "purpose": "Test feature bundle map.",
          "feature_bundles": [
            { "id": "feature_bundle/test/v1", "title": "Test Bundle", "purpose": "Test bundle." }
          ]
        }
        """);

        await WriteExampleFileAsync(atlasRoot, "test_profile_plan.example.json", """
        {
          "schema_version": "0.1",
          "example_id": "example/test_profile_plan/v1",
          "title": "Test Profile Plan",
          "purpose": "Test generator plan example.",
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
              "title": "Normalize profile",
              "order": 0,
              "producer_role": "role/designer_llm/v1",
              "context_pack_template": "context_template/design_discussion/v1",
              "expected_artifact_contract": "game_profile_v1",
              "validation_gates": ["validation.level_0_json_shape"]
            }
          ]
        }
        """);

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(temp.Path, CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Contains(result.Documents, document => document.Id == "capability_atlas/v1");
        var example = Assert.Single(result.Examples);
        Assert.Equal("example/test_profile_plan/v1", example.ExampleId);
        Assert.Equal("profile/test/v1", example.SourceProfileId);
        Assert.Equal(1, example.StepCount);
        Assert.Contains("feature_bundle/test/v1", example.SelectedFeatureBundles);
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == AtlasDiagnosticCodes.InvalidJson);
    }

    [Fact]
    public async Task ImportAtlasRegistryReportsInvalidJsonWithoutThrowing()
    {
        using var temp = new TempDirectory();
        var atlasRoot = CreateAtlasRoot(temp.Path);
        await WriteAtlasFileAsync(atlasRoot, "capability_atlas.json", "{ invalid json");

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(temp.Path, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == AtlasDiagnosticCodes.InvalidJson &&
            diagnostic.Severity == AtlasDiagnosticSeverity.Error);
    }

    [Fact]
    public async Task ImportAtlasRegistryDetectsDuplicateIds()
    {
        using var temp = new TempDirectory();
        var atlasRoot = CreateAtlasRoot(temp.Path);

        await WriteAtlasFileAsync(atlasRoot, "capability_atlas.json", """
        {
          "schema_version": "0.1",
          "id": "duplicate/v1",
          "title": "First",
          "purpose": "First duplicate."
        }
        """);

        await WriteAtlasFileAsync(atlasRoot, "reference_profiles.json", """
        {
          "schema_version": "0.1",
          "id": "duplicate/v1",
          "title": "Second",
          "purpose": "Second duplicate."
        }
        """);

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(temp.Path, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == AtlasDiagnosticCodes.DuplicateId &&
            diagnostic.Severity == AtlasDiagnosticSeverity.Error &&
            diagnostic.Id == "duplicate/v1");
    }

    [Fact]
    public async Task ImportAtlasRegistryReportsMissingRoot()
    {
        using var temp = new TempDirectory();
        var missingRoot = Path.Combine(temp.Path, "missing");

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(missingRoot, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == AtlasDiagnosticCodes.MissingRoot &&
            diagnostic.Severity == AtlasDiagnosticSeverity.Error);
    }

    [Fact]
    public async Task ImportAtlasRegistryWarnsAboutUnknownReferences()
    {
        using var temp = new TempDirectory();
        var atlasRoot = CreateAtlasRoot(temp.Path);

        await WriteAtlasFileAsync(atlasRoot, "capability_atlas.json", """
        {
          "schema_version": "0.1",
          "id": "capability_atlas/v1",
          "title": "Capability Atlas",
          "purpose": "References a missing profile.",
          "required_feature_bundles": [
            "feature_bundle/missing/v1"
          ]
        }
        """);

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(temp.Path, CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == AtlasDiagnosticCodes.ReferenceUnknown &&
            diagnostic.Severity == AtlasDiagnosticSeverity.Warning &&
            diagnostic.Id == "feature_bundle/missing/v1");
    }

    private static string CreateAtlasRoot(string root)
    {
        var atlasRoot = Path.Combine(root, "generator-library", "atlas");
        Directory.CreateDirectory(atlasRoot);
        Directory.CreateDirectory(Path.Combine(atlasRoot, "examples"));
        File.WriteAllText(Path.Combine(atlasRoot, "ATLAS_INDEX.md"), "# Test Atlas Index");
        return atlasRoot;
    }

    private static Task WriteAtlasFileAsync(string atlasRoot, string fileName, string json)
    {
        return File.WriteAllTextAsync(Path.Combine(atlasRoot, fileName), json, CancellationToken.None);
    }

    private static Task WriteExampleFileAsync(string atlasRoot, string fileName, string json)
    {
        return File.WriteAllTextAsync(Path.Combine(atlasRoot, "examples", fileName), json, CancellationToken.None);
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
