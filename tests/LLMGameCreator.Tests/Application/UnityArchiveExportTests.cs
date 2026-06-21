using System.Text.Json;
using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveExportTests
{
    [Fact]
    public async Task DryRunCreatesExpectedDeterministicFilesAndMarkdown()
    {
        using var temp = new TempDirectory();
        var request = CreateCurrentRequest(temp.Path);
        var service = CreateService();

        var first = await service.ExportAsync(request);
        var firstContents = await ReadOutputsAsync(first);
        var second = await service.ExportAsync(request);
        var secondContents = await ReadOutputsAsync(second);

        Assert.True(Directory.Exists(first.OutputDirectoryPath));
        Assert.All(
            new[] { first.PlanJsonPath, first.PlanMarkdownPath, first.ArchiveManifestJsonPath, first.ValidationReportJsonPath },
            path => Assert.True(File.Exists(path), path));
        Assert.Equal(UnityArchiveExportReadiness.ExportableNow, first.Plan.Readiness);
        Assert.Equal(firstContents, secondContents);
        Assert.Equal(
            first.Plan.PlannedFiles.OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase).Select(file => file.RelativePath),
            first.Plan.PlannedFiles.Select(file => file.RelativePath));
        Assert.Contains("# Unity Archive Export Dry Run", firstContents.Markdown);
        Assert.Contains("## Blocked/future modules", firstContents.Markdown);
        Assert.DoesNotContain("timestamp", firstContents.Markdown, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnsafePlannedPathIsRejectedAndCannotEscapeOutputDirectory()
    {
        using var temp = new TempDirectory();
        var request = CreateCurrentRequest(temp.Path) with
        {
            ArchiveManifest = CreateCurrentRequest(temp.Path).ArchiveManifest with
            {
                DataPackages = ["../../escaped-package.json", "data/package.json"]
            }
        };

        var result = await CreateService().ExportAsync(request);

        Assert.Equal(UnityArchiveExportReadiness.Invalid, result.Plan.Readiness);
        Assert.Contains(result.Plan.Diagnostics, diagnostic =>
            diagnostic.Code == UnityArchiveExportDiagnosticCodes.UnsafePlannedPath);
        Assert.DoesNotContain(result.Plan.PlannedFiles, file => file.RelativePath.Contains("..", StringComparison.Ordinal));
        Assert.False(File.Exists(Path.Combine(temp.Path, "escaped-package.json")));
        Assert.All(Directory.EnumerateFiles(temp.Path, "*", SearchOption.AllDirectories), path => Assert.True(IsUnder(temp.Path, path)));
    }

    [Fact]
    public async Task FutureTargetIsBlockedAndMissingRequiredModuleIsReported()
    {
        using var temp = new TempDirectory();
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerMixedViewFuture, out var futureProfile));
        var futureArchive = presets.CreateTopDownGeneratedRpgArchive() with
        {
            TargetProfileId = futureProfile.TargetProfileId,
            RuntimeModuleIds = futureProfile.RequiredRuntimeModuleIds
        };

        var blocked = await CreateService().ExportAsync(new UnityArchiveExportDryRunRequest
        {
            ProjectRootPath = temp.Path,
            DesignBrief = brief,
            TargetProfile = futureProfile,
            ArchiveManifest = futureArchive,
            RuntimeModules = presets.ListRuntimeModules()
        });
        var missing = await CreateService().ExportAsync(new UnityArchiveExportDryRunRequest
        {
            ProjectRootPath = temp.Path,
            DesignBrief = brief,
            TargetProfile = futureProfile,
            ArchiveManifest = futureArchive with
            {
                RuntimeModuleIds = futureArchive.RuntimeModuleIds.Where(id => id != "unity.world.imported_real_map_future").ToList()
            },
            RuntimeModules = presets.ListRuntimeModules()
        });

        Assert.Equal(UnityArchiveExportReadiness.BlockedByFutureModules, blocked.Plan.Readiness);
        Assert.Contains(blocked.Plan.Diagnostics, diagnostic =>
            diagnostic.Code == UnityArchiveExportDiagnosticCodes.FutureRuntimeModule &&
            diagnostic.RelatedId == "unity.world.imported_real_map_future");
        Assert.Equal(UnityArchiveExportReadiness.MissingRequirements, missing.Plan.Readiness);
        Assert.Contains(missing.Plan.Diagnostics, diagnostic =>
            diagnostic.Code == UnityArchiveExportDiagnosticCodes.MissingRequiredRuntimeModule);
    }

    private static UnityArchiveExportDryRunRequest CreateCurrentRequest(string projectRoot)
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        return new UnityArchiveExportDryRunRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules()
        };
    }

    private static UnityArchiveExportDryRunService CreateService()
    {
        return new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer());
    }

    private static async Task<OutputContents> ReadOutputsAsync(UnityArchiveExportDryRunResult result)
    {
        return new OutputContents(
            await File.ReadAllTextAsync(result.PlanJsonPath),
            await File.ReadAllTextAsync(result.PlanMarkdownPath),
            await File.ReadAllTextAsync(result.ArchiveManifestJsonPath),
            await File.ReadAllTextAsync(result.ValidationReportJsonPath));
    }

    private static bool IsUnder(string root, string path)
    {
        var prefix = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        return Path.GetFullPath(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record OutputContents(string Plan, string Markdown, string Manifest, string Validation);

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
