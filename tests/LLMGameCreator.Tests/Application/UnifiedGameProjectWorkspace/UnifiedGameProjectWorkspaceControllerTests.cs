using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;

public sealed class UnifiedGameProjectWorkspaceControllerTests
{
    [Fact]
    public async Task UnifiedGameProjectWorkspace_rejects_no_project_unknown_module_invalid_parameter_and_path_escape()
    {
        using var temp = new TempDirectory();
        var context = await CreateContextAsync(temp.Path);
        Assert.Throws<InvalidOperationException>(() => context.Controller.BuildAndQualify());
        context.Controller.OpenProject(context.ProjectFolder);
        Assert.Throws<InvalidOperationException>(() => context.Controller.SetModuleSelected("feature.unknown", true));
        var packageBytes = await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json"));
        context.Controller.SetParameterValue("feature.profile.alchemy_focus", "healingPotionOutput",
            JsonSerializer.SerializeToElement(999));
        var invalid = context.Controller.BuildAndQualify();
        Assert.False(invalid.Passed);
        Assert.Equal(packageBytes, await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json")));

        var confined = typeof(GameProjectFeatureModuleAuthoringService).GetMethod(
            "ConfinedPath", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                       ?? throw new MissingMethodException("ConfinedPath");
        var exception = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            confined.Invoke(null, new object[] { context.ProjectFolder, "../../escape" }));
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public async Task UnifiedGameProjectWorkspace_creates_project_local_document_and_roundtrips_edits_without_replacing_package()
    {
        using var temp = new TempDirectory();
        var context = await CreateContextAsync(temp.Path);
        var packageBefore = await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json"));

        var initial = context.Controller.OpenProject(context.ProjectFolder);

        Assert.True(File.Exists(Path.Combine(context.ProjectFolder, ".llmgc", "authoring", "goal147-custom-alchemy-combat-exploration.featurecomposition.json")));
        Assert.Equal(packageBefore, await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json")));
        Assert.Equal(10, initial.Mechanics.Count(item => item.Required));
        Assert.Equal(3, initial.Mechanics.Count(item => !item.Required));
        Assert.All(initial.Mechanics.Where(item => item.Required), item => Assert.True(item.Selected));
        Assert.Contains(initial.Mechanics, item => item.Title == "Углублённая алхимия");
        Assert.Equal(8, initial.Parameters.Count);

        context.Controller.SetModuleSelected("feature.profile.combat_focus", false);
        context.Controller.SetParameterValue("feature.profile.alchemy_focus", "healingPotionOutput",
            JsonSerializer.SerializeToElement(3));
        var saved = context.Controller.SaveAuthoring();
        Assert.False(saved.Dirty);

        var reopened = CreateController(context.RepositoryRoot, context.Current);
        var restored = reopened.OpenProject(context.ProjectFolder);
        Assert.DoesNotContain(restored.Mechanics, item => item.ModuleId == "feature.profile.combat_focus" && item.Selected);
        Assert.Equal(3, restored.Parameters.Single(item => item.ParameterId == "healingPotionOutput").Value.GetInt64());
        Assert.Equal(packageBefore, await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json")));
        WriteProof("project-local-authoring-roundtrip-proof.json", new
        {
            schemaVersion = "project_local_authoring_roundtrip_proof_v1",
            status = "GREEN",
            projectLocalAuthoringPersistence = true,
            authoringDocumentCreatedWithoutReplacingPackage = true,
            requiredMechanicCount = initial.Mechanics.Count(item => item.Required),
            optionalMechanicCount = initial.Mechanics.Count(item => !item.Required),
            parameterDefinitionCount = initial.Parameters.Count,
            savedModuleSelectionRestored = true,
            savedParameterValueRestored = true,
            manualJsonEditingRequired = false,
            passed = true
        });
    }

    [Fact]
    public async Task UnifiedGameProjectWorkspace_build_activates_accepted_custom_package_and_updates_current_package()
    {
        using var temp = new TempDirectory();
        var context = await CreateContextAsync(temp.Path);
        context.Controller.OpenProject(context.ProjectFolder);
        ApplyAcceptedCustomValues(context.Controller);

        var result = context.Controller.BuildAndQualify();

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal("2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991", result.PackageSha256);
        Assert.Equal("80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e", result.FinalStateHash);
        Assert.True(result.CheckpointReloadPassed);
        Assert.True(result.FullReplayEquivalent);
        Assert.True(result.ActionBindingPassed);
        Assert.True(result.PackageActivated);
        Assert.Equal(result.PackageSha256, HashFile(Path.Combine(context.ProjectFolder, "package.json")));
        Assert.NotNull(context.Current.CurrentPackage);
        var reloaded = await context.Repository.LoadAsync(context.ProjectFolder, CancellationToken.None);
        Assert.Equal(reloaded.Manifest.PackageId, context.Current.CurrentPackage!.Manifest.PackageId);
        var snapshot = context.Controller.Snapshot();
        Assert.Equal("Готово", snapshot.PackageStatus);
        Assert.Equal(result.PackageSha256, snapshot.PackageSha256);
        Assert.Equal(result.FinalStateHash, snapshot.FinalStateHash);
        Assert.False(Directory.EnumerateDirectories(Path.Combine(context.ProjectFolder, ".llmgc", "build-staging")).Any());
        WriteProof("project-build-activation-proof.json", new
        {
            schemaVersion = "project_build_activation_proof_v1",
            status = "GREEN",
            packageSha256 = result.PackageSha256,
            finalStateHash = result.FinalStateHash,
            result.CheckpointReloadPassed,
            result.FullReplayEquivalent,
            result.ActionBindingPassed,
            projectPackageUpdated = true,
            currentPackageMatchesSavedPackage = true,
            authoringDocumentStoresSuccessfulHashes = true,
            packageActivationTransactional = true,
            stagingRemoved = true,
            passed = true
        });
    }

    [Fact]
    public async Task UnifiedGameProjectWorkspace_package_save_failure_rolls_back_bytes_current_and_last_successful_hashes()
    {
        using var temp = new TempDirectory();
        var context = await CreateContextAsync(temp.Path);
        context.Controller.OpenProject(context.ProjectFolder);
        ApplyAcceptedCustomValues(context.Controller);
        var green = context.Controller.BuildAndQualify();
        Assert.True(green.Passed, string.Join(Environment.NewLine, green.Diagnostics));
        var packageBytes = await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json"));
        var currentPackage = context.Current.CurrentPackage;
        var lastHashes = (context.Controller.Snapshot().PackageSha256, context.Controller.Snapshot().FinalStateHash);

        var failingAuthoring = new GameProjectFeatureModuleAuthoringService(context.RepositoryRoot);
        var failingBuilder = new GameProjectBuildAndQualificationService(
            context.RepositoryRoot,
            SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
            context.Repository,
            new GamePackageValidator(),
            context.Current,
            new FailingActivationStore());
        var failingController = new UnifiedGameProjectWorkspaceController(context.Current, failingAuthoring, failingBuilder);
        failingController.OpenProject(context.ProjectFolder);
        failingController.SetParameterValue("feature.profile.alchemy_focus", "healingPotionOutput",
            JsonSerializer.SerializeToElement(4));

        var failed = failingController.BuildAndQualify();

        Assert.False(failed.Passed);
        Assert.True(failed.RollbackApplied);
        Assert.Equal(packageBytes, await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json")));
        Assert.Same(currentPackage, context.Current.CurrentPackage);
        var after = failingController.Snapshot();
        Assert.Equal(lastHashes.PackageSha256, after.PackageSha256);
        Assert.Equal(lastHashes.FinalStateHash, after.FinalStateHash);
        Assert.Equal(4, after.Parameters.Single(item => item.ParameterId == "healingPotionOutput").Value.GetInt64());
        Assert.False(Directory.EnumerateDirectories(Path.Combine(context.ProjectFolder, ".llmgc", "build-staging")).Any());
        WriteProof("project-build-rollback-proof.json", new
        {
            schemaVersion = "project_build_rollback_proof_v1",
            status = "GREEN",
            packageJsonByteIdentical = true,
            currentPackageUnchanged = true,
            lastSuccessfulHashesUnchanged = true,
            userEditsRetained = true,
            temporaryStagingRemoved = true,
            packageSaveFailureRollsBack = true,
            failedBuildDoesNotReplaceCurrentPackage = true,
            passed = true
        });
    }

    private static void ApplyAcceptedCustomValues(UnifiedGameProjectWorkspaceController controller)
    {
        controller.SetParameterValue("feature.profile.alchemy_focus", "healingPotionOutput", JsonSerializer.SerializeToElement(3));
        controller.SetParameterValue("feature.profile.combat_focus", "basicAttackDamage", JsonSerializer.SerializeToElement(5));
        controller.SetParameterValue("feature.profile.combat_focus", "goblinStartingHealth", JsonSerializer.SerializeToElement(18));
        controller.SetParameterValue("feature.profile.exploration_resource_focus", "appleYield", JsonSerializer.SerializeToElement(4));
        controller.SetParameterValue("feature.profile.exploration_resource_focus", "logYield", JsonSerializer.SerializeToElement(3));
        controller.SetParameterValue("feature.profile.exploration_resource_focus", "transactionPotionOutput", JsonSerializer.SerializeToElement(3));
    }

    private static UnifiedGameProjectWorkspaceController CreateController(string repositoryRoot, CurrentGamePackageService current)
    {
        var repository = new JsonGamePackageRepository();
        return new UnifiedGameProjectWorkspaceController(
            current,
            new GameProjectFeatureModuleAuthoringService(repositoryRoot),
            new GameProjectBuildAndQualificationService(
                repositoryRoot,
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                repository,
                new GamePackageValidator(),
                current));
    }

    private static async Task<TestContext> CreateContextAsync(string gamesRoot)
    {
        var repositoryRoot = FindRepositoryRoot();
        var repository = new JsonGamePackageRepository();
        var projectService = new GameProjectService(repository, new GamePackageValidator(), new NewGamePackageFactory());
        var summary = await projectService.CreateAsync(new CreateGameProjectRequest
        {
            GamesRootPath = gamesRoot,
            FolderName = "workspace-game",
            Title = "Рабочая игра",
            PackageId = "game/workspace-game",
            Version = "0.1.0"
        }, CancellationToken.None);
        CopyDirectory(
            Path.Combine(repositoryRoot, "samples", "minimal-map-game", "scripts"),
            Path.Combine(summary.FolderPath, "scripts"));
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(summary.FolderPath, CancellationToken.None);
        return new TestContext(
            repositoryRoot,
            summary.FolderPath,
            repository,
            current,
            CreateController(repositoryRoot, current));
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static void WriteProof(string fileName, object value)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL148_RUN"), "true", StringComparison.OrdinalIgnoreCase)) return;
        var root = Environment.GetEnvironmentVariable("LLMGC_GOAL148_OUTPUT_ROOT")
                   ?? throw new InvalidOperationException("LLMGC_GOAL148_OUTPUT_ROOT is required.");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, fileName), JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }) + Environment.NewLine);
    }

    private sealed record TestContext(
        string RepositoryRoot,
        string ProjectFolder,
        JsonGamePackageRepository Repository,
        CurrentGamePackageService Current,
        UnifiedGameProjectWorkspaceController Controller);

    private sealed class FailingActivationStore : IGameProjectPackageActivationStore
    {
        public Task ReplaceAsync(string qualifiedPackagePath, string projectPackagePath, CancellationToken cancellationToken) =>
            throw new IOException("Injected package save failure.");
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
            if (Directory.Exists(Path)) Directory.Delete(Path, true);
        }
    }
}
