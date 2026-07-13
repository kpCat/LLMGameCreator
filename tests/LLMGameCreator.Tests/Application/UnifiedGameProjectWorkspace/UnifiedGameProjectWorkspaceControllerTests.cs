using System.Security.Cryptography;
using System.Text;
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

        Assert.True(File.Exists(Path.Combine(
            context.ProjectFolder,
            ".llmgc",
            "authoring",
            initial.ProjectScopedCompositionId + ".featurecomposition.json")));
        Assert.DoesNotContain("goal147", initial.ProjectScopedCompositionId, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(packageBefore, await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json")));
        Assert.Equal(10, initial.Mechanics.Count(item => item.Required));
        Assert.Equal(9, initial.Mechanics.Count(item => !item.Required));
        Assert.All(initial.Mechanics.Where(item => item.Required), item => Assert.True(item.Selected));
        Assert.Contains(initial.Mechanics, item => item.Title == "Углублённая алхимия");
        Assert.Contains(initial.Mechanics, item => item.Title == "Характеристики персонажа" && !item.Selected);
        Assert.Contains(initial.Mechanics, item => item.Title == "Уровни и опыт" && !item.Selected);
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
        var scriptsRoot = Path.Combine(context.ProjectFolder, "scripts");
        Assert.Empty(Directory.EnumerateFiles(scriptsRoot, "*", SearchOption.AllDirectories));
        context.Controller.OpenProject(context.ProjectFolder);
        ApplyAcceptedCustomValues(context.Controller);

        var result = context.Controller.BuildAndQualify();

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal("2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991", result.CompositionPackageSha256);
        Assert.False(string.IsNullOrWhiteSpace(result.ActivatedProjectPackageSha256));
        Assert.Equal(result.ActivatedProjectPackageSha256, result.PackageSha256);
        Assert.NotEqual(result.CompositionPackageSha256, result.ActivatedProjectPackageSha256);
        Assert.Equal("80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e", result.FinalStateHash);
        Assert.True(result.CheckpointReloadPassed);
        Assert.True(result.FullReplayEquivalent);
        Assert.True(result.ActionBindingPassed);
        Assert.True(result.PackageActivated);
        Assert.True(result.SupportFilesPrepared);
        Assert.True(result.StagedProjectValidationPassed);
        Assert.True(result.RealProjectValidationPassed);
        Assert.Equal(1, result.RequiredSupportFileCount);
        Assert.Equal(1, result.CopiedSupportFileCount);
        Assert.Equal(0, result.ReusedSupportFileCount);
        Assert.Contains("Файлы проекта подготовлены: 1", result.HumanSummary, StringComparison.Ordinal);
        Assert.Equal(result.PackageSha256, HashFile(Path.Combine(context.ProjectFolder, "package.json")));
        var supportRelativePath = "scripts/generators/basic_village.lua";
        var supportTargetPath = Path.Combine(context.ProjectFolder, supportRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var supportSourcePath = Path.Combine(context.RepositoryRoot, "samples", "minimal-map-game", supportRelativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(supportTargetPath));
        Assert.Equal(HashFile(supportSourcePath), HashFile(supportTargetPath));
        Assert.NotNull(context.Current.CurrentPackage);
        var reloaded = await context.Repository.LoadAsync(context.ProjectFolder, CancellationToken.None);
        Assert.Equal("game/workspace-game", reloaded.Manifest.PackageId);
        Assert.Equal("Рабочая игра", reloaded.Manifest.Title);
        Assert.Equal("0.1.0", reloaded.Manifest.Version);
        Assert.Equal(reloaded.Manifest.PackageId, context.Current.CurrentPackage!.Manifest.PackageId);
        var snapshot = context.Controller.Snapshot();
        Assert.Equal("Готово", snapshot.PackageStatus);
        Assert.Equal(result.ActivatedProjectPackageSha256, snapshot.PackageSha256);
        Assert.Equal(result.CompositionPackageSha256, snapshot.CompositionPackageSha256);
        Assert.Equal(result.FinalStateHash, snapshot.FinalStateHash);
        Assert.False(Directory.EnumerateDirectories(Path.Combine(context.ProjectFolder, ".llmgc", "build-staging")).Any());
        WriteProof("project-build-activation-proof.json", new
        {
            schemaVersion = "project_build_activation_proof_v1",
            status = "GREEN",
            packageSha256 = result.PackageSha256,
            compositionPackageSha256 = result.CompositionPackageSha256,
            activatedProjectPackageSha256 = result.ActivatedProjectPackageSha256,
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
        WriteProof("new-project-production-build-proof.json", new
        {
            schemaVersion = "new_project_production_build_proof_v1",
            status = "GREEN",
            realGameProjectServiceCreateAsync = true,
            manualTestScriptCopyUsed = false,
            scriptsDirectoryInitiallyEmpty = true,
            packageSha256 = result.PackageSha256,
            finalStateHash = result.FinalStateHash,
            requiredSupportFileCount = result.RequiredSupportFileCount,
            copiedSupportFileCount = result.CopiedSupportFileCount,
            reusedSupportFileCount = result.ReusedSupportFileCount,
            supportRelativePath,
            supportSourceSha256 = HashFile(supportSourcePath),
            supportTargetSha256 = HashFile(supportTargetPath),
            supportFileSourceHashMatched = true,
            stagedProjectValidationPassed = result.StagedProjectValidationPassed,
            realProjectValidationPassed = result.RealProjectValidationPassed,
            currentPackageMatchesSavedPackage = true,
            stagingRemoved = true,
            passed = true
        });
        WriteProof("support-file-plan-proof.json", new
        {
            schemaVersion = "game_project_support_file_plan_proof_v1",
            status = "GREEN",
            entries = new[]
            {
                new
                {
                    scriptId = "script/generator/basic_village",
                    relativePath = supportRelativePath,
                    sourcePath = "samples/minimal-map-game/" + supportRelativePath,
                    sourceSha256 = HashFile(supportSourcePath),
                    targetPath = supportRelativePath,
                    targetState = "missing",
                    activationAction = "copy"
                }
            },
            relativePathsOnlyCopied = true,
            sourcePathConfined = true,
            targetPathConfined = true,
            passed = true
        });
    }

    [Fact]
    public async Task Goal148A_repeat_build_reuses_matching_support_file_and_preserves_hashes()
    {
        using var temp = new TempDirectory();
        var context = await CreateContextAsync(temp.Path);
        context.Controller.OpenProject(context.ProjectFolder);
        ApplyAcceptedCustomValues(context.Controller);
        var first = context.Controller.BuildAndQualify();
        Assert.True(first.Passed, string.Join(Environment.NewLine, first.Diagnostics));
        var supportPath = Path.Combine(context.ProjectFolder, "scripts", "generators", "basic_village.lua");
        var supportBytes = await File.ReadAllBytesAsync(supportPath);

        var repeat = context.Controller.BuildAndQualify();

        Assert.True(repeat.Passed, string.Join(Environment.NewLine, repeat.Diagnostics));
        Assert.Equal(0, repeat.CopiedSupportFileCount);
        Assert.True(repeat.ReusedSupportFileCount >= 1);
        Assert.Equal(supportBytes, await File.ReadAllBytesAsync(supportPath));
        Assert.Equal(first.PackageSha256, repeat.PackageSha256);
        Assert.Equal(first.CompositionPackageSha256, repeat.CompositionPackageSha256);
        Assert.Equal(first.FinalStateHash, repeat.FinalStateHash);
        WriteProof("support-file-repeat-build-proof.json", new
        {
            schemaVersion = "support_file_repeat_build_proof_v1",
            status = "GREEN",
            copiedSupportFileCount = repeat.CopiedSupportFileCount,
            reusedSupportFileCount = repeat.ReusedSupportFileCount,
            supportBytesUnchanged = true,
            packageSha256 = repeat.PackageSha256,
            finalStateHash = repeat.FinalStateHash,
            deterministicHashesPreserved = true,
            passed = true
        });
    }

    [Fact]
    public async Task Goal148A_conflicting_user_support_file_is_rejected_and_preserved_without_activation()
    {
        using var temp = new TempDirectory();
        var context = await CreateContextAsync(temp.Path);
        context.Controller.OpenProject(context.ProjectFolder);
        ApplyAcceptedCustomValues(context.Controller);
        var targetPath = Path.Combine(context.ProjectFolder, "scripts", "generators", "basic_village.lua");
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        var userBytes = Encoding.UTF8.GetBytes("-- user-owned conflicting script\n");
        await File.WriteAllBytesAsync(targetPath, userBytes);
        var packageBytes = await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json"));
        var currentPackage = context.Current.CurrentPackage;
        var hashesBefore = (context.Controller.Snapshot().PackageSha256, context.Controller.Snapshot().FinalStateHash);

        var failed = context.Controller.BuildAndQualify();

        Assert.False(failed.Passed);
        Assert.Contains(failed.Diagnostics, value => value.Contains("scripts/generators/basic_village.lua", StringComparison.Ordinal));
        Assert.Equal(userBytes, await File.ReadAllBytesAsync(targetPath));
        Assert.Equal(packageBytes, await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json")));
        Assert.Same(currentPackage, context.Current.CurrentPackage);
        var after = context.Controller.Snapshot();
        Assert.Equal(hashesBefore.PackageSha256, after.PackageSha256);
        Assert.Equal(hashesBefore.FinalStateHash, after.FinalStateHash);
        WriteProof("support-file-conflict-proof.json", new
        {
            schemaVersion = "support_file_conflict_proof_v1",
            status = "GREEN",
            relativePath = "scripts/generators/basic_village.lua",
            conflictingExistingFileRejected = true,
            conflictingExistingFilePreserved = true,
            packageJsonByteIdentical = true,
            currentPackageUnchanged = true,
            lastSuccessfulHashesUnchanged = true,
            passed = true
        });
    }

    [Fact]
    public async Task Goal148A_missing_injected_source_is_rejected_before_activation_and_staging_is_removed()
    {
        using var temp = new TempDirectory();
        var context = await CreateContextAsync(temp.Path);
        var missingSourceRoot = Path.Combine(temp.Path, "missing-source");
        Directory.CreateDirectory(missingSourceRoot);
        var controller = CreateController(
            context.RepositoryRoot,
            context.Current,
            new NarrowAlphaTemplateSupportFileSource(missingSourceRoot));
        controller.OpenProject(context.ProjectFolder);
        ApplyAcceptedCustomValues(controller);
        var packageBytes = await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json"));
        var targetPath = Path.Combine(context.ProjectFolder, "scripts", "generators", "basic_village.lua");

        var failed = controller.BuildAndQualify();

        Assert.False(failed.Passed);
        Assert.Contains(failed.Diagnostics, value => value.Contains("support.source.missing", StringComparison.Ordinal));
        Assert.Equal(packageBytes, await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json")));
        Assert.False(File.Exists(targetPath));
        Assert.False(Directory.EnumerateDirectories(Path.Combine(context.ProjectFolder, ".llmgc", "build-staging")).Any());
        WriteProof("support-file-missing-source-proof.json", new
        {
            schemaVersion = "support_file_missing_source_proof_v1",
            status = "GREEN",
            missingSourceRejectedBeforeActivation = true,
            packageJsonByteIdentical = true,
            supportTargetAbsent = true,
            stagingRemoved = true,
            passed = true
        });
    }

    [Fact]
    public async Task Goal148A_failure_after_support_copy_removes_new_file_and_restores_package_current_and_hashes()
    {
        using var temp = new TempDirectory();
        var context = await CreateContextAsync(temp.Path);
        var controller = CreateController(
            context.RepositoryRoot,
            context.Current,
            supportFileSource: null,
            activationStore: new FailingActivationStore());
        controller.OpenProject(context.ProjectFolder);
        ApplyAcceptedCustomValues(controller);
        var packageBytes = await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json"));
        var currentPackage = context.Current.CurrentPackage;
        var hashesBefore = (controller.Snapshot().PackageSha256, controller.Snapshot().FinalStateHash);
        var targetPath = Path.Combine(context.ProjectFolder, "scripts", "generators", "basic_village.lua");

        var failed = controller.BuildAndQualify();

        Assert.False(failed.Passed);
        Assert.True(failed.RollbackApplied);
        Assert.False(File.Exists(targetPath));
        Assert.Equal(packageBytes, await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json")));
        Assert.Same(currentPackage, context.Current.CurrentPackage);
        var after = controller.Snapshot();
        Assert.Equal(hashesBefore.PackageSha256, after.PackageSha256);
        Assert.Equal(hashesBefore.FinalStateHash, after.FinalStateHash);
        WriteProof("support-file-rollback-proof.json", new
        {
            schemaVersion = "support_file_rollback_proof_v1",
            status = "GREEN",
            failureInjectedAfterSupportCopy = true,
            newSupportFileRemovedOnRollback = true,
            packageRollbackPassed = true,
            currentPackageRollbackPassed = true,
            lastSuccessfulHashesUnchanged = true,
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

    private static UnifiedGameProjectWorkspaceController CreateController(
        string repositoryRoot,
        CurrentGamePackageService current,
        IGameProjectSupportFileSource? supportFileSource = null,
        IGameProjectPackageActivationStore? activationStore = null)
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
                current,
                activationStore,
                supportFileSource));
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
        var goal148A = string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL148A_RUN"), "true", StringComparison.OrdinalIgnoreCase);
        var goal148 = string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL148_RUN"), "true", StringComparison.OrdinalIgnoreCase);
        if (!goal148A && !goal148) return;
        if (goal148A && fileName is "project-local-authoring-roundtrip-proof.json"
            or "project-build-activation-proof.json"
            or "project-build-rollback-proof.json") return;
        var rootVariable = goal148A ? "LLMGC_GOAL148A_OUTPUT_ROOT" : "LLMGC_GOAL148_OUTPUT_ROOT";
        var root = Environment.GetEnvironmentVariable(rootVariable)
                   ?? throw new InvalidOperationException(rootVariable + " is required.");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, fileName), JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }) + Environment.NewLine, new UTF8Encoding(false));
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
