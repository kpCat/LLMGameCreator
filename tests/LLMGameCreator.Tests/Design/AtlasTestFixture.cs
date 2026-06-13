using LLMGameCreator.Application.Design.Atlas;

namespace LLMGameCreator.Tests.Design;

internal static class AtlasTestFixture
{
    public const string ProfileId = "profile/test/v1";
    public const string FeatureBundleId = "feature_bundle/test/v1";
    public const string ExampleId = "example/test/v1";
    public const string StepId = "step/profile_summary";

    public static string CreateCompleteAtlasRoot(string root)
    {
        var atlasRoot = CreateAtlasRoot(root);

        foreach (var fileName in AtlasRegistryImportService.KnownAtlasFileNames)
        {
            if (fileName.Equals("ATLAS_INDEX.md", StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith("examples/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            WriteMinimalAtlasDocument(
                atlasRoot,
                fileName,
                Path.GetFileNameWithoutExtension(fileName) + "/v1",
                Path.GetFileNameWithoutExtension(fileName).Replace('_', ' '));
        }

        WriteReferenceProfiles(atlasRoot);
        WriteFeatureBundleMap(atlasRoot);
        WriteExamplePlan(atlasRoot);
        return atlasRoot;
    }

    public static string CreateCompleteAtlasRootWithUnknownReference(string root)
    {
        var atlasRoot = CreateCompleteAtlasRoot(root);
        File.WriteAllText(Path.Combine(atlasRoot, "capability_atlas.json"), """
        {
          "schema_version": "0.1",
          "id": "capability_atlas/v1",
          "title": "Capability Atlas",
          "purpose": "References a missing bundle.",
          "required_feature_bundles": [
            "feature_bundle/missing/v1"
          ]
        }
        """);
        return atlasRoot;
    }

    public static string CreateAtlasRootWithInvalidJson(string root)
    {
        var atlasRoot = CreateCompleteAtlasRoot(root);
        File.WriteAllText(Path.Combine(atlasRoot, "capability_atlas.json"), "{ invalid json");
        return atlasRoot;
    }

    public static string CreateAtlasRootWithMissingKnownFiles(string root)
    {
        var atlasRoot = CreateAtlasRoot(root);
        WriteMinimalAtlasDocument(atlasRoot, "capability_atlas.json", "capability_atlas/v1", "Capability Atlas");
        return atlasRoot;
    }

    public static void WriteMinimalAtlasDocument(string atlasRoot, string fileName, string id, string title)
    {
        File.WriteAllText(Path.Combine(atlasRoot, fileName), $$"""
        {
          "schema_version": "0.1",
          "id": "{{id}}",
          "title": "{{title}}",
          "purpose": "Minimal test atlas document."
        }
        """);
    }

    public static void WriteReferenceProfiles(string atlasRoot)
    {
        File.WriteAllText(Path.Combine(atlasRoot, "reference_profiles.json"), $$"""
        {
          "schema_version": "0.1",
          "id": "reference_profiles/v1",
          "title": "Reference Profiles",
          "purpose": "Minimal test atlas document.",
          "profiles": [
            {
              "id": "{{ProfileId}}",
              "title": "Test Profile",
              "purpose": "Minimal test atlas document."
            }
          ]
        }
        """);
    }

    public static void WriteFeatureBundleMap(string atlasRoot)
    {
        File.WriteAllText(Path.Combine(atlasRoot, "feature_bundle_map.json"), $$"""
        {
          "schema_version": "0.1",
          "id": "feature_bundle_map/v1",
          "title": "Feature Bundle Map",
          "purpose": "Minimal test atlas document.",
          "feature_bundles": [
            {
              "id": "{{FeatureBundleId}}",
              "title": "Test Feature Bundle",
              "purpose": "Minimal test atlas document."
            }
          ]
        }
        """);
    }

    public static void WriteExamplePlan(string atlasRoot)
    {
        var examplesRoot = Path.Combine(atlasRoot, "examples");
        Directory.CreateDirectory(examplesRoot);
        File.WriteAllText(Path.Combine(examplesRoot, "test.example.json"), $$"""
        {
          "schema_version": "0.1",
          "example_id": "{{ExampleId}}",
          "title": "Test Example",
          "purpose": "Minimal test atlas document.",
          "source_profile": {
            "id": "{{ProfileId}}"
          },
          "selected_feature_bundles": [
            "{{FeatureBundleId}}"
          ],
          "target_artifacts": [
            "game_profile_v1"
          ],
          "steps": [
            {
              "id": "{{StepId}}",
              "title": "Profile summary"
            }
          ]
        }
        """);
    }

    private static string CreateAtlasRoot(string root)
    {
        var atlasRoot = Path.Combine(root, "generator-library", "atlas");
        Directory.CreateDirectory(atlasRoot);
        Directory.CreateDirectory(Path.Combine(atlasRoot, "examples"));
        File.WriteAllText(Path.Combine(atlasRoot, "ATLAS_INDEX.md"), "# Test Atlas Index");
        return atlasRoot;
    }
}

internal sealed class AtlasTempDirectory : IDisposable
{
    public AtlasTempDirectory()
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
