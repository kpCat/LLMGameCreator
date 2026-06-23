using System.Text.Json;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveMaterializationTests
{
    private static readonly string[] RequiredFiles =
    [
        "manifest/unity-game-archive.json",
        "composition/game-design-brief.json",
        "composition/unity-target-profile.json",
        "composition/runtime-modules-index.json",
        "ui/layouts-index.json",
        "assets/asset-requests.json",
        "assets/asset-request-index.json",
        "assets/asset-slots.json",
        "audio/audio-requests.json",
        "audio/audio-request-index.json",
        "audio/audio-slots.json",
        "lua/module-requests.json",
        "lua/modules-index.json",
        "lua/module-slots.json",
        "production/fulfillment-plan.json",
        "production/readiness-report.json",
        "production/fulfillment-state.json",
        "production/fulfilled-assets-index.json",
        "production/fulfilled-audio-index.json",
        "production/fulfilled-lua-index.json",
        "production/invalid-outputs.json",
        "providers/manual-import/jobs.json",
        "providers/comfyui/jobs.json",
        "providers/suno/jobs.json",
        "providers/local-audio/jobs.json",
        "providers/procedural/jobs.json",
        "localization/index.json",
        "export-report.md",
        "export-validation.json"
    ];

    [Fact]
    public async Task UnityArchiveMaterializationCreatesRequiredDeterministicFiles()
    {
        using var temp = new TempDirectory();
        var service = CreateService();
        var request = CreateCurrentRequest(temp.Path);

        var first = await service.MaterializeAsync(request);
        var firstContents = ReadArchiveContents(first.OutputDirectoryPath);
        var second = await service.MaterializeAsync(request);
        var secondContents = ReadArchiveContents(second.OutputDirectoryPath);

        Assert.Equal(UnityArchiveMaterializationReadiness.MaterializedWithWarnings, first.Readiness);
        Assert.Equal(RequiredFiles.OrderBy(path => path), first.MaterializedFiles.Select(file => file.RelativePath).OrderBy(path => path));
        Assert.Equal(firstContents, secondContents);
        Assert.All(RequiredFiles, relativePath => Assert.True(File.Exists(ArchivePath(first.OutputDirectoryPath, relativePath)), relativePath));
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(ArchivePath(first.OutputDirectoryPath, "manifest/unity-game-archive.json")));
        Assert.Equal("topdown-generated-rpg", manifest.RootElement.GetProperty("gameId").GetString());
        Assert.Null(first.ZipFilePath);
    }

    [Fact]
    public async Task UnityArchiveMaterializationUnsafeInputWritesOnlyContainedValidation()
    {
        using var temp = new TempDirectory();
        var current = CreateCurrentRequest(temp.Path);
        var request = current with
        {
            ArchiveManifest = current.ArchiveManifest with
            {
                DataPackages = ["../../escaped-package.json"]
            }
        };

        var result = await CreateService().MaterializeAsync(request);

        Assert.Equal(UnityArchiveMaterializationReadiness.Invalid, result.Readiness);
        Assert.Equal([UnityArchiveMaterializationService.ValidationFilePath], result.MaterializedFiles.Select(file => file.RelativePath));
        Assert.False(File.Exists(Path.Combine(temp.Path, "escaped-package.json")));
        Assert.All(Directory.EnumerateFiles(temp.Path, "*", SearchOption.AllDirectories), path => Assert.True(IsUnder(temp.Path, path)));
    }

    [Fact]
    public async Task UnityArchiveMaterializationFutureTargetIsMetadataOnly()
    {
        using var temp = new TempDirectory();
        var request = CreateFutureRequest(temp.Path);

        var result = await CreateService().MaterializeAsync(request);

        Assert.Equal(UnityArchiveMaterializationReadiness.MaterializedMetadataOnly, result.Readiness);
        Assert.Equal(UnityArchiveExportReadiness.BlockedByFutureModules, result.DryRunResult.Plan.Readiness);
        Assert.True(File.Exists(ArchivePath(result.OutputDirectoryPath, "manifest/unity-game-archive.json")));
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == UnityArchiveMaterializationDiagnosticCodes.FutureModulesMetadataOnly);
    }

    [Fact]
    public async Task UnityArchiveMaterializationMissingRequirementBlocksArchiveContractFiles()
    {
        using var temp = new TempDirectory();
        var future = CreateFutureRequest(temp.Path);
        var missingModuleId = future.TargetProfile.RequiredRuntimeModuleIds.First();
        var request = future with
        {
            ArchiveManifest = future.ArchiveManifest with
            {
                RuntimeModuleIds = future.ArchiveManifest.RuntimeModuleIds
                    .Where(moduleId => !string.Equals(moduleId, missingModuleId, StringComparison.OrdinalIgnoreCase))
                    .ToList()
            }
        };

        var result = await CreateService().MaterializeAsync(request);

        Assert.Equal(UnityArchiveMaterializationReadiness.Blocked, result.Readiness);
        Assert.Equal(UnityArchiveExportReadiness.MissingRequirements, result.DryRunResult.Plan.Readiness);
        Assert.Equal([UnityArchiveMaterializationService.ValidationFilePath], result.MaterializedFiles.Select(file => file.RelativePath));
        Assert.False(File.Exists(ArchivePath(result.OutputDirectoryPath, "manifest/unity-game-archive.json")));
    }

    [Fact]
    public async Task UnityArchiveMaterializationProviderJobPlanErrorsBlockPlayableSuccess()
    {
        using var temp = new TempDirectory();
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));

        var dryRunService = new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer());
        var pipelineService = new UnityArchiveAssetAudioLuaRequestService();
        var providerJobPlanService = new UnityArchiveProviderJobPlanService();
        var materializationService = new UnityArchiveMaterializationService(dryRunService, null, pipelineService, providerJobPlanService);

        var package = new GamePackageDefinition
        {
            Manifest = new LLMGameCreator.Domain.Definitions.GameManifest
            {
                PackageId = "game/provider-job-plan-error",
                Title = "Provider Job Plan Error Test"
            },
            GeneratedContent = new GeneratedContentDefinition
            {
                Scenes = [new GeneratedSceneDefinition { SourceId = "scene/test", PackageMapId = "map/test", Title = "Test" }],
                Npcs = [new GeneratedNpcDefinition { SourceId = "npc/test", Name = "Test", SceneId = "scene/test" }]
            }
        };

        var request = new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = temp.Path,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            GamePackage = package
        };

        var result = await materializationService.MaterializeAsync(request);

        Assert.NotEqual(UnityArchiveMaterializationReadiness.MaterializedPlayableContract, result.Readiness);
        Assert.Contains(result.Diagnostics, d => d.Severity == UnityArchiveExportDiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task UnityArchiveMaterializationDoesNotDoublePrefixRequestDiagnostics()
    {
        using var temp = new TempDirectory();
        var request = CreateCurrentRequest(temp.Path);

        var dryRunService = new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer());
        var materializationService = new UnityArchiveMaterializationService(dryRunService);

        var result = await materializationService.MaterializeAsync(request);

        Assert.DoesNotContain(result.Diagnostics, d => d.Code.Contains("request.request."));
    }

    private static UnityArchiveMaterializationRequest CreateCurrentRequest(string projectRoot)
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        return new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules()
        };
    }

    private static UnityArchiveMaterializationRequest CreateFutureRequest(string projectRoot)
    {
        var request = CreateCurrentRequest(projectRoot);
        var presets = new UnityTargetContractPresetProvider();
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerMixedViewFuture, out var profile));
        return request with
        {
            TargetProfile = profile,
            ArchiveManifest = request.ArchiveManifest with
            {
                TargetProfileId = profile.TargetProfileId,
                RuntimeModuleIds = profile.RequiredRuntimeModuleIds
            }
        };
    }

    private static UnityArchiveMaterializationService CreateService()
    {
        var dryRun = new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer());
        return new UnityArchiveMaterializationService(dryRun);
    }

    private static IReadOnlyList<string> ReadArchiveContents(string outputDirectory)
    {
        return Directory.EnumerateFiles(outputDirectory, "*", SearchOption.AllDirectories)
            .OrderBy(path => Path.GetRelativePath(outputDirectory, path), StringComparer.OrdinalIgnoreCase)
            .Select(File.ReadAllText)
            .ToList();
    }

    private static string ArchivePath(string outputDirectory, string relativePath)
    {
        return Path.Combine(outputDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static bool IsUnder(string root, string path)
    {
        var prefix = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        return Path.GetFullPath(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
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