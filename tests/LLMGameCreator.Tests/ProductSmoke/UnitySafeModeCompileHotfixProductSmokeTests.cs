using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnitySafeModeCompileHotfixProductSmokeTests
{
    private const string Scenario = "goal-114-unity-safe-mode-compile-hotfix";

    private static readonly string[] JsonUtilityTargets =
    {
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResult.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResultStore.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSaveLoadController.cs"
    };

    private static readonly (string RelativePath, string ExpectedCall)[] WrapperTargets =
    {
        ("unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs", "Refresh();"),
        ("unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelController.cs", "RefreshPayload();"),
        ("unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs", "RefreshPayload();"),
        ("unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs", "RefreshPayload();")
    };

    private static readonly string[] RequiredEvidenceFiles =
    {
        "unity-safe-mode-compile-hotfix-report.md",
        "unity-safe-mode-compile-hotfix-dashboard.json",
        "unity-safe-mode-compile-hotfix-source-scan.json",
        "unity-safe-mode-compile-hotfix-negative-proof.json",
        "unity-safe-mode-compile-hotfix-file-index.json"
    };

    [Fact]
    public void Goal114UnitySafeModeCompileHotfixProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        foreach (var relativePath in JsonUtilityTargets)
        {
            var text = Read(repoRoot, relativePath);
            Assert.DoesNotContain("JsonUtility.", text, StringComparison.Ordinal);
        }

        foreach (var target in WrapperTargets)
        {
            var text = Read(repoRoot, target.RelativePath);
            Assert.Contains("public void RefreshPayloadStatus()", text, StringComparison.Ordinal);
            Assert.Contains(target.ExpectedCall, text, StringComparison.Ordinal);
        }

        var proceduralRoot = Path.Combine(
            repoRoot,
            ".llmgc",
            "procedural",
            Scenario);
        var exportRoot = Path.Combine(
            repoRoot,
            ".llmgc",
            "exports",
            Scenario);
        AssertFilesExist(proceduralRoot, RequiredEvidenceFiles);
        AssertFilesExist(exportRoot, RequiredEvidenceFiles);

        using var sourceScan = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            proceduralRoot,
            "unity-safe-mode-compile-hotfix-source-scan.json")));
        using var fileIndex = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            proceduralRoot,
            "unity-safe-mode-compile-hotfix-file-index.json")));
        using var dashboard = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            proceduralRoot,
            "unity-safe-mode-compile-hotfix-dashboard.json")));

        Assert.Equal("GREEN", dashboard.RootElement.GetProperty("implementationStatus").GetString());
        Assert.Equal(
            "offline_geoworld_alpha_manual_acceptance_verification",
            dashboard.RootElement.GetProperty("manualGate").GetString());
        Assert.False(dashboard.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(dashboard.RootElement.GetProperty("manualGateRemainsOpen").GetBoolean());
        Assert.False(dashboard.RootElement.GetProperty("manualResultCreatedOrCommitted").GetBoolean());

        Assert.True(sourceScan.RootElement.GetProperty("manualGateRemainsOpen").GetBoolean());
        Assert.True(sourceScan.RootElement.GetProperty("noManualResultWrites").GetBoolean());
        Assert.True(sourceScan.RootElement.GetProperty("noAlphaRuntimeBootstrapChanges").GetBoolean());
        Assert.True(sourceScan.RootElement.GetProperty("noScenePrefabProjectPackageSettingsChanges").GetBoolean());
        Assert.True(sourceScan.RootElement.GetProperty("noStreamingAssetsChanges").GetBoolean());
        Assert.False(sourceScan.RootElement.GetProperty("manualRootExistsDuringScan").GetBoolean());
        foreach (var item in sourceScan.RootElement.GetProperty("jsonUtilityReferenceCounts").EnumerateArray())
        {
            Assert.Equal(0, item.GetProperty("unqualifiedJsonUtilityReferenceCount").GetInt32());
        }

        foreach (var item in sourceScan.RootElement.GetProperty("refreshPayloadStatusWrappers").EnumerateArray())
        {
            Assert.True(item.GetProperty("wrapperPresent").GetBoolean(), item.GetProperty("relativePath").GetString());
            Assert.True(item.GetProperty("callsExistingRefreshMethod").GetBoolean(), item.GetProperty("relativePath").GetString());
        }

        var expectedChangedPaths = fileIndex.RootElement
            .GetProperty("expectedChangedPaths")
            .EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(
            expectedChangedPaths,
            path => path.EndsWith("AlphaRuntimeBootstrap.cs", StringComparison.Ordinal));
        Assert.DoesNotContain(
            expectedChangedPaths,
            path => path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
    }

    private static string Read(string repoRoot, string relativePath)
    {
        return File.ReadAllText(Path.Combine(
            repoRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static void AssertFilesExist(string directory, IReadOnlyList<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            Assert.True(File.Exists(Path.Combine(directory, fileName)), fileName);
        }
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
