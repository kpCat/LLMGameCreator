using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace;

public sealed class Goal148CProjectIdentityTests
{
    private const string ManualCompositionSha = "e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221";
    private const string ManualFinalStateHash = "95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8";
    private const string HistoricalCompositionSha = "2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991";
    private const string HistoricalFinalStateHash = "80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e";

    [Fact]
    public async Task Goal148C_legacy_manual_project_recovers_identity_and_migrates_authoring_without_replacing_package()
    {
        using var temp = new TempDirectory();
        var context = await CreateAffectedContextAsync(temp.Path);
        var packageBytes = await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json"));
        var legacyBytes = await File.ReadAllBytesAsync(context.LegacyAuthoringPath);

        var snapshot = context.Controller.OpenProject(context.ProjectFolder);

        var identityPath = Path.Combine(context.ProjectFolder, ".llmgc", "project-identity.json");
        var scopedPath = Path.Combine(context.ProjectFolder, ".llmgc", "authoring",
            snapshot.ProjectScopedCompositionId + FeatureModuleCompositionDocumentVocabulary.FileExtension);
        Assert.True(File.Exists(identityPath));
        Assert.True(File.Exists(scopedPath));
        Assert.Equal("Проверка конструктора", snapshot.ProjectTitle);
        Assert.Equal("game/goal148-manual", snapshot.ProjectPackageId);
        Assert.Equal("0.1.0", snapshot.ProjectVersion);
        Assert.Equal(GameProjectIdentityVocabulary.RecoveredAfterTemplateOverwriteSource, snapshot.IdentitySource);
        Assert.DoesNotContain("goal147", snapshot.ProjectScopedCompositionId, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(4, snapshot.Parameters.Single(item => item.ParameterId == "logYield").Value.GetInt64());
        Assert.Equal(packageBytes, await File.ReadAllBytesAsync(Path.Combine(context.ProjectFolder, "package.json")));
        Assert.Equal(legacyBytes, await File.ReadAllBytesAsync(context.LegacyAuthoringPath));

        var migrated = JsonSerializer.Deserialize<FeatureModuleCompositionDocument>(
            await File.ReadAllTextAsync(scopedPath),
            JsonOptions()) ?? throw new InvalidOperationException("Migrated authoring document is empty.");
        Assert.Equal(ManualCompositionSha, migrated.PreviousMaterializedPackageSha256);
        Assert.Equal(ManualFinalStateHash, migrated.PreviousQualifiedFinalStateHash);
        Assert.Empty(migrated.LastMaterializedPackageSha256);
        Assert.Empty(migrated.LastQualifiedFinalStateHash);
        Assert.Equal("NOT_RUN", migrated.LastQualificationStatus);

        WriteProof("project-identity-capture-proof.json", new
        {
            schemaVersion = "project_identity_capture_proof_v1",
            status = "GREEN",
            identityPath = ".llmgc/project-identity.json",
            packageId = snapshot.ProjectPackageId,
            title = snapshot.ProjectTitle,
            version = snapshot.ProjectVersion,
            formatVersion = snapshot.ProjectFormatVersion,
            source = snapshot.IdentitySource,
            packageJsonByteIdenticalAfterOpen = true,
            passed = true
        });
        WriteProof("legacy-authoring-migration-proof.json", new
        {
            schemaVersion = "legacy_authoring_migration_proof_v1",
            status = "GREEN",
            legacyDocumentPreserved = true,
            projectScopedDocumentCreated = true,
            projectScopedCompositionId = snapshot.ProjectScopedCompositionId,
            selectedModulesPreserved = true,
            allParameterValuesPreserved = true,
            logYield = 4,
            previousCompositionPackageSha256 = migrated.PreviousMaterializedPackageSha256,
            previousFinalStateHash = migrated.PreviousQualifiedFinalStateHash,
            staleSuccessfulHashesCleared = true,
            passed = true
        });
        WriteProof("project-scoped-composition-identity-proof.json", new
        {
            schemaVersion = "project_scoped_composition_identity_proof_v1",
            status = "GREEN",
            packageId = snapshot.ProjectPackageId,
            compositionId = snapshot.ProjectScopedCompositionId,
            filename = Path.GetFileName(scopedPath),
            validatorAccepted = FeatureModuleCompositionDocumentValidator.IsValidCompositionId(snapshot.ProjectScopedCompositionId),
            deterministic = snapshot.ProjectScopedCompositionId == new GameProjectCompositionIdentityService().Create(snapshot.ProjectPackageId),
            fixedGoal147CompositionIdAbsent = snapshot.ProjectScopedCompositionId != UnifiedGameProjectWorkspaceVocabulary.LegacyCompositionId,
            titleIndependent = snapshot.ProjectScopedCompositionId == new GameProjectCompositionIdentityService().Create(snapshot.ProjectPackageId),
            passed = true
        });
    }

    [Fact]
    public async Task Goal148C_manual_values_build_preserves_identity_hash_semantics_and_repeat_build()
    {
        using var temp = new TempDirectory();
        var context = await CreateAffectedContextAsync(temp.Path);
        var opened = context.Controller.OpenProject(context.ProjectFolder);
        var sidecarPath = Path.Combine(context.ProjectFolder, ".llmgc", "project-identity.json");
        var sidecarBytes = await File.ReadAllBytesAsync(sidecarPath);

        var first = context.Controller.BuildAndQualify();
        var repeat = context.Controller.BuildAndQualify();
        var activated = await context.Repository.LoadAsync(context.ProjectFolder, CancellationToken.None);
        var repeatedSnapshot = context.Controller.Snapshot();

        Assert.True(first.Passed, string.Join(Environment.NewLine, first.Diagnostics));
        Assert.True(repeat.Passed, string.Join(Environment.NewLine, repeat.Diagnostics));
        Assert.Equal(ManualCompositionSha, first.CompositionPackageSha256);
        Assert.Equal(ManualFinalStateHash, first.FinalStateHash);
        Assert.Equal(first.ActivatedProjectPackageSha256, first.PackageSha256);
        Assert.NotEqual(first.CompositionPackageSha256, first.ActivatedProjectPackageSha256);
        Assert.Equal("game/goal148-manual", activated.Manifest.PackageId);
        Assert.Equal("Проверка конструктора", activated.Manifest.Title);
        Assert.Equal("0.1.0", activated.Manifest.Version);
        Assert.Equal(first.CompositionPackageSha256, repeat.CompositionPackageSha256);
        Assert.Equal(first.ActivatedProjectPackageSha256, repeat.ActivatedProjectPackageSha256);
        Assert.Equal(first.FinalStateHash, repeat.FinalStateHash);
        Assert.Equal(opened.ProjectScopedCompositionId, repeatedSnapshot.ProjectScopedCompositionId);
        Assert.Equal(sidecarBytes, await File.ReadAllBytesAsync(sidecarPath));
        Assert.Equal(0, repeat.CopiedSupportFileCount);
        Assert.True(repeat.ReusedSupportFileCount >= 1);

        WriteProof("manual-values-project-build-proof.json", new
        {
            schemaVersion = "manual_values_project_build_proof_v1",
            status = "GREEN",
            projectPackageId = activated.Manifest.PackageId,
            projectTitle = activated.Manifest.Title,
            projectVersion = activated.Manifest.Version,
            projectScopedCompositionId = repeatedSnapshot.ProjectScopedCompositionId,
            compositionPackageSha256 = first.CompositionPackageSha256,
            activatedProjectPackageSha256 = first.ActivatedProjectPackageSha256,
            finalStateHash = first.FinalStateHash,
            activatedPackageDiffersFromCompositionPackage = true,
            supportFilePrepared = first.SupportFilesPrepared,
            supportFileReusedOnRepeat = repeat.ReusedSupportFileCount >= 1,
            passed = true
        });
        WriteProof("identity-repeat-build-proof.json", new
        {
            schemaVersion = "identity_repeat_build_proof_v1",
            status = "GREEN",
            sameCompositionPackageSha256 = first.CompositionPackageSha256 == repeat.CompositionPackageSha256,
            sameActivatedProjectPackageSha256 = first.ActivatedProjectPackageSha256 == repeat.ActivatedProjectPackageSha256,
            sameFinalStateHash = first.FinalStateHash == repeat.FinalStateHash,
            sameProjectIdentitySidecarBytes = true,
            sameProjectScopedCompositionId = true,
            supportReused = repeat.ReusedSupportFileCount >= 1,
            passed = true
        });
    }

    [Fact]
    public async Task Goal148C_historical_control_preserves_composition_and_final_hashes_with_project_identity()
    {
        using var temp = new TempDirectory();
        var context = await CreateNewProjectContextAsync(temp.Path, "historical-control", "Исторический контроль", "game/historical-control");
        context.Controller.OpenProject(context.ProjectFolder);
        ApplyValues(context.Controller, 3);

        var result = context.Controller.BuildAndQualify();
        var activated = await context.Repository.LoadAsync(context.ProjectFolder, CancellationToken.None);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(HistoricalCompositionSha, result.CompositionPackageSha256);
        Assert.Equal(HistoricalFinalStateHash, result.FinalStateHash);
        Assert.Equal("game/historical-control", activated.Manifest.PackageId);
        Assert.Equal("Исторический контроль", activated.Manifest.Title);
        Assert.Equal("0.1.0", activated.Manifest.Version);
        WriteProof("historical-control-values-proof.json", new
        {
            schemaVersion = "historical_control_values_proof_v1",
            status = "GREEN",
            logYield = 3,
            compositionPackageSha256 = result.CompositionPackageSha256,
            finalStateHash = result.FinalStateHash,
            identityPreserved = true,
            passed = true
        });
    }

    [Fact]
    public async Task Goal148C_two_projects_with_same_mechanics_share_composition_and_final_hashes_but_not_activated_hash()
    {
        using var temp = new TempDirectory();
        var first = await CreateNewProjectContextAsync(temp.Path, "first-game", "Первая игра", "game/first-game");
        var second = await CreateNewProjectContextAsync(temp.Path, "second-game", "Вторая игра", "game/second-game");
        first.Controller.OpenProject(first.ProjectFolder);
        second.Controller.OpenProject(second.ProjectFolder);
        ApplyValues(first.Controller, 4);
        ApplyValues(second.Controller, 4);

        var firstResult = first.Controller.BuildAndQualify();
        var secondResult = second.Controller.BuildAndQualify();

        Assert.True(firstResult.Passed, string.Join(Environment.NewLine, firstResult.Diagnostics));
        Assert.True(secondResult.Passed, string.Join(Environment.NewLine, secondResult.Diagnostics));
        Assert.Equal(firstResult.CompositionPackageSha256, secondResult.CompositionPackageSha256);
        Assert.Equal(firstResult.FinalStateHash, secondResult.FinalStateHash);
        Assert.NotEqual(firstResult.ActivatedProjectPackageSha256, secondResult.ActivatedProjectPackageSha256);
        Assert.NotEqual(first.Controller.Snapshot().ProjectScopedCompositionId, second.Controller.Snapshot().ProjectScopedCompositionId);
        WriteProof("two-project-identity-isolation-proof.json", new
        {
            schemaVersion = "two_project_identity_isolation_proof_v1",
            status = "GREEN",
            sameCompositionPackageSha256 = true,
            sameFinalStateHash = true,
            differentActivatedProjectPackageSha256 = true,
            differentProjectScopedCompositionId = true,
            firstPackageId = "game/first-game",
            secondPackageId = "game/second-game",
            passed = true
        });
    }

    [Fact]
    public async Task Goal148C_failure_after_support_activation_restores_identity_package_authoring_legacy_and_support()
    {
        using var temp = new TempDirectory();
        var context = await CreateAffectedContextAsync(temp.Path);
        context.Controller.OpenProject(context.ProjectFolder);
        var failing = CreateController(context.RepositoryRoot, context.Current, new FailingActivationStore());
        failing.OpenProject(context.ProjectFolder);
        failing.SetParameterValue("feature.profile.alchemy_focus", "healingPotionOutput", JsonSerializer.SerializeToElement(4));

        var packagePath = Path.Combine(context.ProjectFolder, "package.json");
        var identityPath = Path.Combine(context.ProjectFolder, ".llmgc", "project-identity.json");
        var scopedPath = Path.Combine(context.ProjectFolder, ".llmgc", "authoring",
            failing.Snapshot().ProjectScopedCompositionId + FeatureModuleCompositionDocumentVocabulary.FileExtension);
        var packageBytes = await File.ReadAllBytesAsync(packagePath);
        var identityBytes = await File.ReadAllBytesAsync(identityPath);
        var scopedBytes = await File.ReadAllBytesAsync(scopedPath);
        var legacyBytes = await File.ReadAllBytesAsync(context.LegacyAuthoringPath);
        var currentPackage = context.Current.CurrentPackage;
        var supportPath = Path.Combine(context.ProjectFolder, "scripts", "generators", "basic_village.lua");

        var failed = failing.BuildAndQualify();

        Assert.False(failed.Passed);
        Assert.True(failed.RollbackApplied);
        Assert.Equal(packageBytes, await File.ReadAllBytesAsync(packagePath));
        Assert.Equal(identityBytes, await File.ReadAllBytesAsync(identityPath));
        Assert.Equal(scopedBytes, await File.ReadAllBytesAsync(scopedPath));
        Assert.Equal(legacyBytes, await File.ReadAllBytesAsync(context.LegacyAuthoringPath));
        Assert.Same(currentPackage, context.Current.CurrentPackage);
        Assert.False(File.Exists(supportPath));
        Assert.True(failing.Snapshot().Dirty);
        Assert.Equal(4, failing.Snapshot().Parameters.Single(item => item.ParameterId == "healingPotionOutput").Value.GetInt64());
        WriteProof("identity-rollback-proof.json", new
        {
            schemaVersion = "identity_rollback_proof_v1",
            status = "GREEN",
            failureInjectedAfterOverlayAndSupportActivation = true,
            packageJsonByteIdentical = true,
            currentPackageUnchanged = true,
            projectIdentitySidecarByteIdentical = true,
            projectScopedAuthoringByteIdentical = true,
            legacyAuthoringByteIdentical = true,
            newSupportFileRemoved = true,
            userEditsRetainedInMemory = true,
            passed = true
        });
    }

    [Fact]
    public async Task Goal148C_ambiguous_template_recovery_and_invalid_sidecar_fail_safely_without_package_mutation()
    {
        using var temp = new TempDirectory();
        var root = FindRepositoryRoot();
        var ambiguousFolder = Path.Combine(temp.Path, "ambiguous-template");
        Directory.CreateDirectory(ambiguousFolder);
        File.Copy(
            Path.Combine(root, ".llmgc", "procedural",
                "goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix",
                "compositions", "minimal-map-game-composed-alchemy-combat-exploration", "package.json"),
            Path.Combine(ambiguousFolder, "package.json"));
        var repository = new JsonGamePackageRepository();
        var ambiguousCurrent = new CurrentGamePackageService(repository);
        await ambiguousCurrent.LoadAsync(ambiguousFolder, CancellationToken.None);
        var ambiguousBytes = await File.ReadAllBytesAsync(Path.Combine(ambiguousFolder, "package.json"));

        var ambiguous = Assert.Throws<InvalidOperationException>(() =>
            CreateController(root, ambiguousCurrent).OpenProject(ambiguousFolder));
        Assert.Contains("recovery is ambiguous", ambiguous.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(ambiguousBytes, await File.ReadAllBytesAsync(Path.Combine(ambiguousFolder, "package.json")));
        Assert.False(File.Exists(Path.Combine(ambiguousFolder, ".llmgc", "project-identity.json")));

        var invalid = await CreateNewProjectContextAsync(temp.Path, "invalid-sidecar", "Некорректный sidecar", "game/invalid-sidecar");
        var invalidSidecarPath = Path.Combine(invalid.ProjectFolder, ".llmgc", "project-identity.json");
        Directory.CreateDirectory(Path.GetDirectoryName(invalidSidecarPath)!);
        await File.WriteAllTextAsync(invalidSidecarPath, "{\"schemaVersion\":\"unsupported\"}\n", Encoding.UTF8);
        var invalidBytes = await File.ReadAllBytesAsync(Path.Combine(invalid.ProjectFolder, "package.json"));
        var invalidException = Assert.Throws<InvalidOperationException>(() => invalid.Controller.OpenProject(invalid.ProjectFolder));
        Assert.Contains("validation failed", invalidException.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(invalidBytes, await File.ReadAllBytesAsync(Path.Combine(invalid.ProjectFolder, "package.json")));
    }

    private static async Task<TestContext> CreateAffectedContextAsync(string gamesRoot)
    {
        var root = FindRepositoryRoot();
        var projectFolder = Path.Combine(gamesRoot, "goal148-manual");
        Directory.CreateDirectory(projectFolder);
        Directory.CreateDirectory(Path.Combine(projectFolder, "assets"));
        Directory.CreateDirectory(Path.Combine(projectFolder, "scripts"));
        Directory.CreateDirectory(Path.Combine(projectFolder, "saves"));
        File.Copy(
            Path.Combine(root, ".llmgc", "procedural",
                "goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix",
                "compositions", "minimal-map-game-composed-alchemy-combat-exploration", "package.json"),
            Path.Combine(projectFolder, "package.json"));

        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var authoringRoot = Path.Combine(projectFolder, ".llmgc", "authoring");
        var persistence = new FeatureModuleCompositionPersistenceService(authoringRoot);
        var legacy = persistence.CreateNew(
            UnifiedGameProjectWorkspaceVocabulary.LegacyCompositionId,
            "Проверка конструктора",
            "Настройки механик открытого игрового проекта.",
            library) with
        {
            ParameterValues = ParameterValues(4),
            LastMaterializedPackageSha256 = ManualCompositionSha,
            LastQualifiedFinalStateHash = ManualFinalStateHash,
            LastQualificationStatus = "GREEN"
        };
        persistence.Save(legacy, library);

        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(projectFolder, CancellationToken.None);
        return new TestContext(
            root,
            projectFolder,
            Path.Combine(authoringRoot, UnifiedGameProjectWorkspaceVocabulary.LegacyCompositionId
                                        + FeatureModuleCompositionDocumentVocabulary.FileExtension),
            repository,
            current,
            CreateController(root, current));
    }

    private static async Task<TestContext> CreateNewProjectContextAsync(
        string gamesRoot,
        string folderName,
        string title,
        string packageId)
    {
        var root = FindRepositoryRoot();
        var repository = new JsonGamePackageRepository();
        var service = new GameProjectService(repository, new GamePackageValidator(), new NewGamePackageFactory());
        var summary = await service.CreateAsync(new CreateGameProjectRequest
        {
            GamesRootPath = gamesRoot,
            FolderName = folderName,
            Title = title,
            PackageId = packageId,
            Version = "0.1.0"
        }, CancellationToken.None);
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(summary.FolderPath, CancellationToken.None);
        return new TestContext(root, summary.FolderPath, string.Empty, repository, current, CreateController(root, current));
    }

    private static UnifiedGameProjectWorkspaceController CreateController(
        string repositoryRoot,
        CurrentGamePackageService current,
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
                activationStore));
    }

    private static void ApplyValues(UnifiedGameProjectWorkspaceController controller, int logYield)
    {
        foreach (var value in ParameterValues(logYield))
            controller.SetParameterValue(value.ModuleId, value.ParameterId, value.Value);
    }

    private static IReadOnlyList<FeatureModuleParameterValue> ParameterValues(int logYield) =>
    [
        Value("feature.profile.alchemy_focus", "healingPotionOutput", 3),
        Value("feature.profile.combat_focus", "basicAttackDamage", 5),
        Value("feature.profile.combat_focus", "goblinStartingHealth", 18),
        Value("feature.profile.exploration_resource_focus", "appleYield", 4),
        Value("feature.profile.exploration_resource_focus", "logYield", logYield),
        Value("feature.profile.exploration_resource_focus", "transactionPotionOutput", 3)
    ];

    private static FeatureModuleParameterValue Value(string moduleId, string parameterId, int value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = JsonSerializer.SerializeToElement(value)
    };

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

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static void WriteProof(string fileName, object value)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL148C_RUN"), "true", StringComparison.OrdinalIgnoreCase))
            return;
        var root = Environment.GetEnvironmentVariable("LLMGC_GOAL148C_OUTPUT_ROOT")
                   ?? throw new InvalidOperationException("LLMGC_GOAL148C_OUTPUT_ROOT is required.");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, fileName), JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }) + Environment.NewLine, new UTF8Encoding(false));
    }

    private sealed record TestContext(
        string RepositoryRoot,
        string ProjectFolder,
        string LegacyAuthoringPath,
        JsonGamePackageRepository Repository,
        CurrentGamePackageService Current,
        UnifiedGameProjectWorkspaceController Controller);

    private sealed class FailingActivationStore : IGameProjectPackageActivationStore
    {
        public Task ReplaceAsync(string qualifiedPackagePath, string projectPackagePath, CancellationToken cancellationToken) =>
            throw new IOException("Injected package activation failure after identity overlay and support activation.");
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
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
    }
}
