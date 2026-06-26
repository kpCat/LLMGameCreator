using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.Application.Design.UnityRuntimeExport;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.AlphaBuild;

public sealed class AlphaRunnableBuildAcceptanceTests
{
    [Fact]
    public async Task BuildsStableAlphaBlockerArtifactsFromAcceptedEvidence()
    {
        using var temp = new TempDirectory();
        var evidence = BuildPriorEvidence(temp.Path);
        var service = new AlphaRunnableBuildAcceptanceService();

        var first = service.BuildFromAcceptedEvidence(temp.Path, evidence.Content, evidence.Assets);
        var second = service.BuildFromAcceptedEvidence(temp.Path, evidence.Content, evidence.Assets);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.False(first.Report.Accepted);
        Assert.True(first.Report.BlockerReached, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.Equal("alpha_unity_build_environment_blocker", first.Report.FinalStatus);
        Assert.Equal("unity_runtime_export_vertical_slice_artifact_verification passed", first.Report.PreviousAcceptedGate);
        Assert.Equal(["S106", "S107", "S108", "S109", "S110", "S111", "S112", "S113"], first.Report.CompletedSlices);
        Assert.Equal("alpha-runnable-build", first.Report.ProductSmokeRoute);
        Assert.Equal(3, first.Report.StyleCandidates.Count);
        Assert.Equal(["frontier_survival", "gothic_mystery", "trade_caravan"], first.Report.StyleCandidates.Select(item => item.StyleId).ToArray());
        Assert.Equal("frontier_survival", first.Report.PrimaryBuildCandidate.StyleId);
        Assert.All(first.Report.StyleCandidates, candidate =>
        {
            Assert.True(candidate.Accepted, string.Join(Environment.NewLine, candidate.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
            Assert.NotEmpty(candidate.PackageId);
            Assert.NotEmpty(candidate.PackageHash);
            Assert.NotEmpty(candidate.AssetManifestHash);
            Assert.NotEmpty(candidate.ExportManifestHash);
            Assert.NotEmpty(candidate.LoopRefs.MapId);
            Assert.NotEmpty(candidate.LoopRefs.NpcId);
            Assert.NotEmpty(candidate.LoopRefs.QuestId);
            Assert.NotEmpty(candidate.LoopRefs.DialogueId);
            Assert.NotEmpty(candidate.LoopRefs.ItemId);
            Assert.NotEmpty(candidate.LoopRefs.EventId);
            Assert.NotEmpty(candidate.CommandHints);
            Assert.Equal(5, candidate.AssetRefs.Count);
        });
        Assert.True(first.Report.Staging.Passed);
        Assert.True(first.Report.Staging.FileCount >= 11);
        Assert.True(first.Report.Staging.TotalByteCount > 0);
        Assert.Contains(first.Report.Staging.Files, item => item.RelativePath == "game-data/game-package.json");
        Assert.Contains(first.Report.Staging.Files, item => item.RelativePath == "runtime/alpha-launch-metadata.json");
        Assert.Contains(first.Report.Staging.Files, item => item.Kind == "asset_payload");
        Assert.False(first.Report.WindowsExecutableProduced);
        Assert.False(first.Report.UnityEditorExecuted);
        Assert.False(first.Report.UnityBuildProduced);
        Assert.False(first.Report.LaunchVerified);
        Assert.False(first.Report.PlayLoopVerified);
        Assert.False(first.Report.RuntimePreviewDependency);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ProjectFilesChanged);
        Assert.False(first.Report.GeneratorLibraryChanged);
        Assert.False(first.Report.ExternalExecution.AnyExecuted());
        Assert.True(first.Report.InvalidMatrix.Passed);
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(File.Exists(Path.Combine(write.StagingDirectoryPath, "game-data", "game-package.json")));
        Assert.True(Directory.Exists(write.BuildDirectoryPath));
        Assert.False(Directory.EnumerateFiles(write.BuildDirectoryPath, "*.exe", SearchOption.AllDirectories).Any());

        var roundTrip = JsonSerializer.Deserialize<AlphaRunnableBuildReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(roundTrip);
        Assert.True(roundTrip!.BlockerReached);
    }

    [Fact]
    public void InvalidFakeLeakAndExpectationOnlyScenariosAreRejectedCausally()
    {
        using var temp = new TempDirectory();
        var evidence = BuildPriorEvidence(temp.Path);
        var report = new AlphaRunnableBuildAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, evidence.Content, evidence.Assets)
            .Report;
        var invalid = report.InvalidMatrix.Scenarios.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.True(report.InvalidMatrix.Passed);
        Assert.True(report.InvalidMatrix.ScenarioCount >= 14);
        Assert.Contains(invalid["missing_accepted_goal012_evidence"].Diagnostics, item => item.Code == "alpha_build.contract.missing_goal012_evidence");
        Assert.Contains(invalid["package_hash_mismatch"].Diagnostics, item => item.Code == "alpha_build.contract.package_hash_mismatch");
        Assert.Contains(invalid["asset_manifest_hash_mismatch"].Diagnostics, item => item.Code == "alpha_build.contract.asset_manifest_hash_mismatch");
        Assert.Contains(invalid["export_manifest_hash_mismatch"].Diagnostics, item => item.Code == "alpha_build.contract.export_manifest_hash_mismatch");
        Assert.Contains(invalid["missing_staged_game_data"].Diagnostics, item => item.Code == "alpha_build.staging.missing_game_data");
        Assert.Contains(invalid["missing_staged_asset_payload"].Diagnostics, item => item.Code == "alpha_build.staging.missing_asset_payload");
        Assert.Contains(invalid["missing_executable"].Diagnostics, item => item.Code == "alpha_build.output.missing_executable");
        Assert.Contains(invalid["mismatched_executable_build_file_hash"].Diagnostics, item => item.Code == "alpha_build.output.hash_mismatch");
        Assert.Contains(invalid["path_traversal_in_staging_manifest"].Diagnostics, item => item.Code == "alpha_build.staging.unsafe_path");
        Assert.Contains(invalid["absolute_output_path_injection"].Diagnostics, item => item.Code == "alpha_build.output.unsafe_path");
        Assert.Contains(invalid["copied_expectation_report_without_build_files"].Diagnostics, item => item.Code == "alpha_build.invalid.expectation_only_report");
        Assert.Contains(invalid["runtime_preview_dependency_claim"].Diagnostics, item => item.Code == "alpha_build.contract.runtime_preview_dependency");
        Assert.Contains(invalid["unity_build_claim_without_artifact"].Diagnostics, item => item.Code == "alpha_build.output.unity_build_claim_without_artifact");
        Assert.Contains(invalid["cross_style_package_export_asset_leakage"].Diagnostics, item => item.Code == "alpha_build.contract.cross_style_leakage");
        Assert.All(invalid.Values, scenario =>
        {
            Assert.False(scenario.ActualValid);
            Assert.Contains(scenario.Diagnostics, item => item.Severity == "error");
        });
    }

    [Fact]
    public async Task GeneratedAlphaArtifactsUseCompactWindowsFriendlyPaths()
    {
        using var temp = new TempDirectory();
        var evidence = BuildPriorEvidence(temp.Path);
        var service = new AlphaRunnableBuildAcceptanceService();

        var result = service.BuildFromAcceptedEvidence(temp.Path, evidence.Content, evidence.Assets);
        var write = await service.WriteAsync(temp.Path, result);
        var artifactRoot = write.OutputDirectoryPath;
        var generatedFiles = Directory.EnumerateFiles(artifactRoot, "*", SearchOption.AllDirectories)
            .Select(path => new
            {
                FullPath = path,
                RelativePath = Path.GetRelativePath(artifactRoot, path).Replace('\\', '/'),
                FileName = Path.GetFileName(path)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

        Assert.NotEmpty(generatedFiles);
        Assert.All(generatedFiles, file =>
        {
            Assert.True(file.RelativePath.Length <= 160, $"{file.RelativePath} length was {file.RelativePath.Length}");
            Assert.True(file.FileName.Length <= 96, $"{file.FileName} length was {file.FileName.Length}");
            Assert.DoesNotContain("game-content-generation-", file.FileName, StringComparison.Ordinal);
            Assert.DoesNotContain("frontier-survival-item-icon-ui-graphic", file.FileName, StringComparison.Ordinal);
        });
        Assert.All(result.Report.StyleCandidates.SelectMany(candidate => candidate.AssetRefs), asset =>
        {
            Assert.StartsWith("assets/", asset.ExportRelativePath, StringComparison.Ordinal);
            Assert.Contains("/asset-", asset.ExportRelativePath, StringComparison.Ordinal);
        });

        foreach (var candidate in result.Report.StyleCandidates)
        {
            var exportRoot = Path.Combine(temp.Path, candidate.ExportFolderRelativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(Directory.Exists(exportRoot));
            foreach (var asset in candidate.AssetRefs)
            {
                var path = Path.Combine(exportRoot, asset.ExportRelativePath.Replace('/', Path.DirectorySeparatorChar));
                Assert.True(File.Exists(path), asset.ExportRelativePath);
                var bytes = File.ReadAllBytes(path);
                Assert.Equal(asset.ByteCount, bytes.LongLength);
                Assert.Equal(asset.Hash, ComputeHash(bytes));
            }
        }

        var exportManifests = Directory.EnumerateFiles(Path.Combine(artifactRoot, "source-evidence"), "export-manifest.json", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
        Assert.NotEmpty(exportManifests);
        foreach (var manifestPath in exportManifests)
        {
            var exportRoot = Path.GetDirectoryName(manifestPath)!;
            var manifest = JsonSerializer.Deserialize<UnityRuntimeExportFileManifest>(
                File.ReadAllText(manifestPath),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            Assert.NotNull(manifest);
            Assert.All(manifest.Files, file =>
            {
                Assert.True(IsSafeRelativePath(file.RelativePath), file.RelativePath);
                var path = Path.Combine(exportRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                Assert.True(File.Exists(path), file.RelativePath);
                var bytes = File.ReadAllBytes(path);
                Assert.Equal(file.ByteCount, bytes.LongLength);
                Assert.Equal(file.Hash, ComputeHash(bytes));
                Assert.True(Path.GetFileName(file.RelativePath).Length <= 96, file.RelativePath);
            });
            Assert.All(manifest.Files.Where(file => file.Kind == "asset_payload"), file =>
            {
                Assert.Contains("/asset-", file.RelativePath, StringComparison.Ordinal);
                Assert.DoesNotContain("game-content-generation-", Path.GetFileName(file.RelativePath), StringComparison.Ordinal);
                Assert.DoesNotContain("frontier-survival-item-icon-ui-graphic", Path.GetFileName(file.RelativePath), StringComparison.Ordinal);
                Assert.False(string.IsNullOrWhiteSpace(file.LogicalId));
            });
        }

        foreach (var file in result.Report.Staging.Files)
        {
            Assert.True(IsSafeRelativePath(file.RelativePath), file.RelativePath);
            var path = Path.Combine(write.StagingDirectoryPath, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), file.RelativePath);
            var bytes = File.ReadAllBytes(path);
            Assert.Equal(file.ByteCount, bytes.LongLength);
            Assert.Equal(file.Hash, ComputeHash(bytes));
        }
        Assert.True(result.Report.BlockerReached);
        Assert.Equal("alpha_unity_build_environment_blocker", result.Report.FinalStatus);
        Assert.False(result.Report.WindowsExecutableProduced);
    }

    [Fact]
    public void RemovingExpectationOnlyMutationMakesExpectedInvalidMatrixFail()
    {
        using var temp = new TempDirectory();
        var evidence = BuildPriorEvidence(temp.Path);

        var report = new AlphaRunnableBuildAcceptanceService()
            .BuildFromAcceptedEvidence(
                temp.Path,
                evidence.Content,
                evidence.Assets,
                new AlphaRunnableBuildOptions { IncludeExpectationOnlyInvalidMutation = false })
            .Report;

        Assert.False(report.InvalidMatrix.Passed);
        var scenario = report.InvalidMatrix.Scenarios.Single(item => item.ScenarioId == "copied_expectation_report_without_build_files");
        Assert.True(scenario.ActualValid);
        Assert.DoesNotContain(scenario.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public void NonUnityCSharpFileWithBuildCommandTextDoesNotSatisfyRepositoryBuildPath()
    {
        using var temp = new TempDirectory();
        var sourcePath = Path.Combine(temp.Path, "src", "LLMGameCreator.Application", "Design", "AlphaBuild", "FakeBuildNotes.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
        File.WriteAllText(sourcePath, "public static class FakeBuildNotes { public const string Command = \"BuildPipeline.BuildPlayer(); -buildWindows64Player\"; }");
        var prosePath = Path.Combine(temp.Path, "unity", "LLMGameCreatorAlpha", "Assets", "Editor", "FakeBuildNotes.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(prosePath)!);
        File.WriteAllText(prosePath, "public static class FakeBuildNotes { public const string Note = \"BuildPipeline.BuildPlayer();\"; }");
        var evidence = BuildPriorEvidence(temp.Path);

        var report = new AlphaRunnableBuildAcceptanceService()
            .BuildFromAcceptedEvidence(temp.Path, evidence.Content, evidence.Assets)
            .Report;

        Assert.True(report.BlockerReached, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.False(report.BuildEnvironment.RepoBuildScriptFound);
        Assert.Contains(report.BuildEnvironment.Diagnostics, item => item.Code == "alpha_build.environment.no_repo_build_script");
    }

    [Fact]
    public void DetectsRepositoryUnityProjectAndRealAssetsBuildEntrypoint()
    {
        using var temp = new TempDirectory();
        CreateUnityTemplate(temp.Path);
        var evidence = BuildPriorEvidence(temp.Path);

        var report = new AlphaRunnableBuildAcceptanceService()
            .BuildFromAcceptedEvidence(
                temp.Path,
                evidence.Content,
                evidence.Assets,
                new AlphaRunnableBuildOptions { RepositoryRootPath = temp.Path })
            .Report;

        Assert.True(report.BuildEnvironment.RepoUnityProjectFound);
        Assert.Equal("unity/LLMGameCreatorAlpha", report.BuildEnvironment.RepoUnityProjectRelativePath);
        Assert.True(report.BuildEnvironment.RepoBuildScriptFound);
        Assert.Equal("unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs", report.BuildEnvironment.RepoBuildScriptRelativePath);
        Assert.DoesNotContain(report.BuildEnvironment.Diagnostics, item => item.Code == "alpha_build.environment.no_repo_unity_project");
        Assert.DoesNotContain(report.BuildEnvironment.Diagnostics, item => item.Code == "alpha_build.environment.no_repo_build_script");
        Assert.DoesNotContain(report.BuildOutput.Diagnostics, item => item.Code == "alpha_build.output.no_supported_repo_build_path");
        Assert.False(report.WindowsExecutableProduced);
    }

    [Fact]
    public async Task BuildOutputValidationAcceptsPhysicalExecutableAndStagedStreamingAssets()
    {
        using var temp = new TempDirectory();
        CreateUnityTemplate(temp.Path);
        var evidence = BuildPriorEvidence(temp.Path);
        var service = new AlphaRunnableBuildAcceptanceService();
        var first = service.BuildFromAcceptedEvidence(
            temp.Path,
            evidence.Content,
            evidence.Assets,
            new AlphaRunnableBuildOptions { RepositoryRootPath = temp.Path });
        var write = await service.WriteAsync(temp.Path, first);
        var executablePath = Path.Combine(write.BuildDirectoryPath, "LLMGameCreatorAlpha.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(executablePath)!);
        await File.WriteAllBytesAsync(executablePath, new byte[] { (byte)'M', (byte)'Z', 0, 1, 2, 3 });
        CopyDirectory(
            write.StagingDirectoryPath,
            Path.Combine(write.BuildDirectoryPath, "LLMGameCreatorAlpha_Data", "StreamingAssets", "LLMGameCreatorAlpha"));

        var second = service.BuildFromAcceptedEvidence(
            temp.Path,
            evidence.Content,
            evidence.Assets,
            new AlphaRunnableBuildOptions
            {
                RepositoryRootPath = temp.Path,
                PreserveExistingBuildOutputForValidation = true
            });

        Assert.False(second.Report.Accepted);
        Assert.False(second.Report.BlockerReached);
        Assert.Equal(AlphaRunnableBuildAcceptanceService.FinalGate, second.Report.FinalStatus);
        Assert.True(second.Report.BuildOutput.Passed, string.Join(Environment.NewLine, second.Report.BuildOutput.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.True(second.Report.WindowsExecutableProduced);
        Assert.True(second.Report.UnityBuildProduced);
        Assert.Equal("LLMGameCreatorAlpha.exe", second.Report.BuildOutput.ExecutableRelativePath);
        var executable = Assert.Single(second.Report.BuildOutput.Files, item => item.RelativePath == "LLMGameCreatorAlpha.exe");
        Assert.Equal(6, executable.ByteCount);
        Assert.Equal(ComputeHash(await File.ReadAllBytesAsync(executablePath)), executable.Hash);
        Assert.Contains(second.Report.BuildOutput.Files, item => item.RelativePath == "LLMGameCreatorAlpha_Data/StreamingAssets/LLMGameCreatorAlpha/game-data/game-package.json");
        Assert.False(second.Report.LaunchVerified);
        Assert.False(second.Report.PlayLoopVerified);
    }

    private static (ContentGenerationScaleAcceptanceResult Content, MinimumAssetPipelineAcceptanceResult Assets) BuildPriorEvidence(string projectRoot)
    {
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(ResolveContentPackDirectory(), projectRoot);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(projectRoot, ResolveAssetPackDirectory(), content);
        return (content, assets);
    }

    private static string ResolveContentPackDirectory() =>
        Path.Combine(FindRepoRoot(), "samples", "content-generation-packs");

    private static string ResolveAssetPackDirectory() =>
        Path.Combine(FindRepoRoot(), "samples", "minimum-asset-pipeline");

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

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path) &&
        !Path.IsPathRooted(path) &&
        !path.Contains('\\') &&
        !path.Contains(':', StringComparison.Ordinal) &&
        !path.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(part => part == "..");

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void CreateUnityTemplate(string repoRoot)
    {
        var projectRoot = Path.Combine(repoRoot, "unity", "LLMGameCreatorAlpha");
        Directory.CreateDirectory(Path.Combine(projectRoot, "ProjectSettings"));
        Directory.CreateDirectory(Path.Combine(projectRoot, "Packages"));
        Directory.CreateDirectory(Path.Combine(projectRoot, "Assets", "Editor"));
        File.WriteAllText(Path.Combine(projectRoot, "ProjectSettings", "ProjectVersion.txt"), "m_EditorVersion: 6000.1.10f1");
        File.WriteAllText(Path.Combine(projectRoot, "Packages", "manifest.json"), "{}");
        File.WriteAllText(
            Path.Combine(projectRoot, "Assets", "Editor", "AlphaBuildEntrypoint.cs"),
            "using UnityEditor; public static class AlphaBuildEntrypoint { public static void BuildWindows64() { BuildPipeline.BuildPlayer(new BuildPlayerOptions()); } }");
    }

    private static void CopyDirectory(string sourceRoot, string destinationRoot)
    {
        foreach (var sourcePath in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, sourcePath);
            var destinationPath = Path.Combine(destinationRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath, overwrite: true);
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
