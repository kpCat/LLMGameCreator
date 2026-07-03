using System.Text.Json;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class VisualWorldStreamPreviewWorkspaceProductSmokeTests
{
    private static readonly HashSet<string> BinaryOrRasterMediaExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes"
    };

    [Fact]
    public async Task Goal092VisualWorldStreamPreviewWorkspaceProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new VisualWorldStreamPreviewWorkspaceService();
        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);
        var write = await service.BuildAndWriteAsync(repoRoot, projectRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.ProofStatusJson, second.ProofStatusJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.ProofStatusJsonPath));
        Assert.True(File.Exists(write.WinFormsBindingInventoryJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var proof = JsonDocument.Parse(await File.ReadAllTextAsync(write.ProofStatusJsonPath));
        using var binding = JsonDocument.Parse(await File.ReadAllTextAsync(write.WinFormsBindingInventoryJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        var groups = catalog.RootElement.GetProperty("groups").EnumerateArray().ToArray();
        Assert.True(catalog.RootElement.GetProperty("groupCount").GetInt32() >= 5);
        Assert.True(catalog.RootElement.GetProperty("svgTextPreviewCount").GetInt32() >= 4);
        Assert.Contains(groups, item => item.GetProperty("groupId").GetString() == "microtiles");
        Assert.Contains(groups, item => item.GetProperty("groupId").GetString() == "map_patches");
        Assert.Contains(groups, item => item.GetProperty("groupId").GetString() == "region_composer");
        Assert.Contains(groups, item => item.GetProperty("groupId").GetString() == "world_profiles");
        var streamGroup = Assert.Single(
            groups,
            item => item.GetProperty("groupId").GetString() == "chunk_stream_windows");
        var streamEntries = streamGroup.GetProperty("entries").EnumerateArray().ToArray();
        Assert.Equal(
            4,
            streamEntries.Count(item =>
                item.GetProperty("artifactKind").GetString()
                == "text_svg_chunk_stream_window_overview"));

        var svgEntries = catalog.RootElement.GetProperty("svgEntries").EnumerateArray().ToArray();
        Assert.All(svgEntries, entry =>
        {
            var relativePath = entry.GetProperty("relativePath").GetString() ?? string.Empty;
            Assert.False(Path.IsPathFullyQualified(relativePath), relativePath);
            Assert.EndsWith(".svg", relativePath);
            Assert.True(entry.GetProperty("safeToDisplayAsText").GetBoolean());
        });

        var proofs = proof.RootElement.GetProperty("proofs").EnumerateArray().ToArray();
        Assert.True(proof.RootElement.GetProperty("passed").GetBoolean());
        AssertProofPassed(proofs, "goal091.seam");
        AssertProofPassed(proofs, "goal091.cache_reuse");
        AssertProofPassed(proofs, "goal091.layer_transition");
        AssertProofPassed(proofs, "goal091.negative");
        Assert.True(binding.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("goal091StreamWindowsVisible").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noAbsolutePaths").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noBinaryOrRasterMediaAdded").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRuntimeUnityProviderSchemaProjectDependencyChanges").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noPromptDumps").GetBoolean());

        var expectedPrefixes = quality.RootElement
            .GetProperty("expectedChangedPathPrefixes")
            .EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(expectedPrefixes, item => item.StartsWith("src/LLMGameCreator.Runtime", StringComparison.Ordinal));
        Assert.DoesNotContain(expectedPrefixes, item => item.StartsWith("unity/", StringComparison.Ordinal));
        Assert.DoesNotContain(expectedPrefixes, item => item.StartsWith("src/LLMGameCreator.GamePackage", StringComparison.Ordinal));
        Assert.DoesNotContain(expectedPrefixes, item => item.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));

        var mediaFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(path => BinaryOrRasterMediaExtensions.Contains(Path.GetExtension(path)))
            .ToList();
        Assert.Empty(mediaFiles);

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("visual_world_stream_preview_workspace_verification required", report);
        Assert.Contains("goal091.seam", report);
        Assert.DoesNotContain(repoRoot, report, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertProofPassed(JsonElement[] proofs, string proofId)
    {
        var proof = Assert.Single(proofs, item => item.GetProperty("proofId").GetString() == proofId);
        Assert.True(proof.GetProperty("passed").GetBoolean(), proofId);
    }

    private static string ResolveProjectFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
